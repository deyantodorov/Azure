$rg = 'dt-az-play'
$loc = 'westeurope'
$stgAccName = 'stgname' + $(Get-Random)
$queueName = "toarchive"
$containerName = "images"

az storage account create `
 -g $rg `
 -n $stgAccName `
 -l $loc `
 --sku Standard_LRS

$key = $(az storage account keys list `
--account-name $stgAccName `
-g $rg `
--query "[0].value" `
--output tsv)

$key

az storage queue create `
 --name $queueName `
 --account-name $stgAccName `
 --account-key $key

az storage container create `
 --name $containerName `
 --account-name $stgAccName `
 --account-key $key

az storage container create `
 --name $containerName `
 --account-name $stgAccName `
 --account-name $key

az storage blob upload `
 --container-name $containerName `
 --name rakia.jpg `
 --file rakia.jpg `
 --account-name $stgAccName `
 --account-key $key

az storage account delete `
 -n $stgAccName `
 -g $rg `
 -y