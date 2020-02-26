$rand = Get-Random
$rg = "dt-az-play"
$planName = "amazon" + $rand
$appName = $planName + "-app"
$loc = "westeurope"
$stgAcc = "stgacc1328538807"

az appservice plan create `
 -n $planName `
 -g $rg `
 -l $loc `
 --sku B1

az webapp create `
 -n $appName `
 -g $rg `
 --plan $planName

az webapp log config `
 -n $appName `
 -g $rg `
 --level information `
 --application-logging true

 # build web app package
 $publishDir = "publish"
 $package = "publish.zip"

 if (Test-Path $publishDir) { Remove-Item -Recurse -Force $publishDir }
 if (Test-Path $package) { Remove-Item $package }

 dotnet publish -c release -o $publishDir WebBlobLogging.csproj
 Compress-Archive -Path $publishDir/* -DestinationPath $package -Force

 # deploy publish.zip using the kudo zip api
 az webapp deployment source config-zip `
  -n $appName `
  -g $rg `
  --src $package

# launch the site in a browser
$site = az webapp show -n $appName -g $rg --query "defaultHostName"
$site

# cool tail the logs :)
az webapp log tail --n $appName -g $rg

# configure storage account
az storage account create `
 -g $rg `
 -n $stgAcc `
 -l $loc `
 --sku Standard_LRS

$key = $(az storage account keys list `
 --account-name $stgAcc `
 -g $rg `
 --query "[0].value" `
 --output tsv)

$key

az storage container create `
 --name logs `
 --account-name $stgAcc `
 --account-key $key

az storage container list `
 --account-name $stgAcc `
 --account-key $key

az storage container policy list `
 -c logs `
 --account-name $stgAcc `
 --account-key $key

$today = Get-Date
$tomorrow = $today.AddDays(1)
$today.ToString("yyyy-MM-dd")
$tomorrow.ToString("yyyy-MM-dd")

az storage container policy create `
 -c logs `
 --name "logpolicytwo" `
 --start $today.ToString("yyyy-MM-dd") `
 --expiry $tomorrow.ToString("yyyy-MM-dd") `
 --permissions lwrd `
 --account-name $stgAcc `
 --account-key $key

$sas = az storage container generate-sas `
 --name logs `
 --policy-name logpolicy `
 --account-name $stgAcc `
 --account-key $key `
 -o tsv

$sas

$containerSasUrl = "https://$stgAcc.blob.core.windows.net/logs?$sas"
$containerSasUrl

$spname = "spname" + $rand

$sp = az ad sp create-for-rbac --name $spname | ConvertFrom-Json

$subid = az account show --query "id" -o tsv
$tenantid = az account show --query "tenantId" -o tsv

$clientId = $sp.appId
$clientSecret = $sp.password

"var clientId = `"$clientId`";" 
"var clientSecret = `"$clientSecret`";"
"var subscriptionId = `"$subid`";"
"var tenantId = `"$tenantid`";"
"var sasUrl = `"$containerSasUrl`";"

az webapp log show -n $appName -g $rg

az ad sp list --display-name $spname
az ad sp delete --id $sp.appId