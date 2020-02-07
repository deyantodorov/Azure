$rg = 'dt-az-play'
$loc = 'westeurope'
$storage = 'storage' + (Get-Random)
$queue = 'incoming-orders'

az storage account create `
 -n $storage `
 -g $rg `
 -l $loc `
 --sku Standard_LRS `
 --kind StorageV2 `
 --access-tier Hot

$keys = $(az storage account keys list `
 --account-name $storage `
 -g $rg `
 --query "[0].value" `
 -o tsv)

 az storage queue create `
  -n $queue `
  --account-name $storage `
  --account-key $keys

az storage account show-connection-string `
 -n $storage `
 --query "connectionString"

$order1json = Get-Content -Path order1.json

 az storage message put `
 --account-name $storage `
 --account-key $keys `
 --queue-name $queue `
 --content $order1json
