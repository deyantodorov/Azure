$loc = "westeurope"
$rg = "dt-az-play"
$kvname = "keyvault" + $(Get-Random)
$spname = "serviceplan" + $(Get-Random)

az keyvault create `
 -n $kvname `
 -g $rg `
 --sku standard

az keyvault secret set `
 --vault-name $kvname `
 --name "connectionString" `
 --value "some connection string"

az keyvault secret show `
 --vault-name $kvname `
 --name connectionString

$sp = az ad sp create-for-rbac --name $spname | ConvertFrom-Json
$sp

$tenantid = az account show --query tenantId -o tsv

az login --service-principal `
 --username $sp.appId `
 --password $sp.password `
 --tenant $tenantid

az keyvault secret show `
 --vault-name $kvname `
 --name connectionString

az keyvault set-policy `
 --name $kvname `
 --spn $sp.Name `
 --secret-permissions get

az login --service-principal `
 --username $sp.appId `
 --password $sp.password `
 --tenant $tenantid

az keyvault secret show `
 --vault-name $kvname `
 --name connectionString

az ad sp delete --id $sp.appId
az keyvault delete --name $kvname