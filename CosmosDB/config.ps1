$rg = "dt-az-play"
$location = "westeurope"
$acc = "mycosmos$(Get-Random)"
$db = "myDatabase"

az group create `
 -n $rg `
 -l $location

az cosmosdb create `
 -g $rg `
 --name $acc `
 --kind GlobalDocumentDB `
 --locations "West Europe=0" "North Europe=1" `
 --default-consistency-level Strong `
 --enable-multiple-write-locations false `
 --enable-automatic-failover true

az cosmosdb database create `
 -g $rg `
 --name $acc `
 --db-name $db

az cosmosdb keys list `
 --name $acc `
 -g $rg

az cosmosdb list-connection-strings `
 --name $acc `
 -g $rg

az cosmosdb show `
 --name $acc `
 -g $rg `
 --query "documentEndpoint"
