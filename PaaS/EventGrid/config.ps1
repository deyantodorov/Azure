$rg = "dt-az-play"
$loc = "westeurope"
$stgName = "storage" + $(Get-Random)

az storage account create `
 -n $stgName `
 -g $rg `
 -l $loc `
 --sku Standard_LRS `
 --kind StorageV2

$stgAccKey = $(az storage account keys list `
 -g $rg `
 --account-name $stgName `
 --query "[0].value" `
 --output tsv)

$stgAccKey

ngrok http -host-header=localhost 5000

$funcappdns = "7e345ca4.ngrok.io"
$viewrendpoint = "https://$funcappdns/api/updates"

$storageid = $(az storage account show `
 -n $stgName `
 -g $rg `
 --output tsv)

$storageid

az eventgrid event-subscription create `
 --source-resource-id $storageid `
 --name storagesubscription `
 --endpoint-type WebHook `
 --endpoint $viewrendpoint `
 --included-event-types "Microsoft.Storage.BlobCreated" `
 --subject-begins-with "/blobServices/default/containers/testcontainer"

