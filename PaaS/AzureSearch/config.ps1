$rg = 'dt-az-play'
$name = 'azsearch' + $(Get-Random)
$sku = 'free'

az search service create `
 --name $name `
 -g $rg `
 --sku $sku

az search admin-key show `
 --service-name $name `
 -g $rg `
 --query "primaryKey"

az search query-key list `
 --service-name $name `
 -g $rg `
 --query "[0].key"

az search delete -n $name -g $rg -y