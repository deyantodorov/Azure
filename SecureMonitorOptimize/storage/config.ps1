$loc = "westeurope"
$rg = "dt-az-play"
$stgAccName = "storage" + $(Get-Random)
$spName = "serviceprincipal" + $(Get-Random)

az storage account create `
 -g $rg `
 -n $stgAccName `
 -l $loc `
 --sku Standard_LRS

$sp = az ad sp create-for-rbac `
 -n $spName | ConvertFrom-Json

az role assignment list --assignee $sp.appId

az role assignment delete `
 --assignee $sp.appId --role Contributor

az role assignment list --assignee $sp.appId

$tenantid = az account show --query tenantId -o tsv

az login --service-principal `
 --username $sp.appId `
 --password $sp.password `
 --tenant $tenantid

az role assignment create `
 --assignee $sp.appId --role Reader

az storage container list `
 --account-name $stgAccName

$stgAccId = az storage account show `
 -n $stgAccName `
 --query id `
 -o tsv

$spobjid = az ad sp show `
 --id $sp.appId --query objectId -o tsv

$stgAccId
$spobjid

az login

az role assignment create `
 --role "Storage Account Contributor" `
 --assignee-object-id $spobjid `
 --scope $stgAccId

az login --service-principal `
 --username $sp.appId `
 --password $sp.password `
 --tenant $tenantid

az storage container create `
 --name contribcli `
 --account-name $stgAccName

az storage container list `
 --account-name $stgAccName

az login

az ad sp delete --id $sp.appId
az storage delete -n $stgAccName -g $rg