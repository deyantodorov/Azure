$rg = "dt-az-play"
$location = "westeurope"
$acc = "storage$(Get-Random)"

az storage account create `
 -g $rg `
 -n $acc `
 -l $location `
 --sku Standard_LRS

az storage account show-connection-string `
 -n $acc `
 --query "connectionString"

az storage account delete `
 -n $acc `
 -g $rg
