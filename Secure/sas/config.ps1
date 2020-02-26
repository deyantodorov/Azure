$stgAccName = "storage" + $(Get-Random)
$container = "images"
$rg = "dt-az-play"
$loc = "westeurope"

az storage account create `
 -n $stgAccName `
 -g $rg `
 -l $loc `
 --sku Standard_LRS

$stgAccKey = $(az storage account keys list `
 -g $rg `
 --account-name $stgAccName `
 --query "[0].value" `
 --output tsv)

$stgAccKey

az storage container create `
 --account-name $stgAccName `
 --account-key $stgAccKey `
 --name $container

az storage blob upload `
 --account-name $stgAccName `
 --account-key $stgAccKey `
 --file rakia.jpg `
 --container-name $container `
 --name rakia.jpg

az storage blob url `
 --account-name $stgAccName `
 --account-key $stgAccKey `
 --container-name $container `
 --name rakia.jpg

$now = [DateTime]::UtcNow
$now

$start = $now.ToString('yyyy-MM-ddTHH:mmZ')
$end = $now.AddMinutes(5).ToString('yyyy-MM-ddTHH:mmZ')

$start
$end

$sas = az storage blob generate-sas `
 --account-name $stgAccName `
 --account-key $stgAccKey `
 --container-name $container `
 --name rakia.jpg `
 --permissions r `
 --start $start `
 --expiry $end

$sas

az storage blob url `
 --account-name $stgAccName `
 --account-key $stgAccKey `
 --container-name $container `
 --name rakia.jpg `
 --sas $sas `
 -o tsv