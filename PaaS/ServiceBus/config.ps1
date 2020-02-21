$rg = 'dt-az-play'
$name = 'name' + $(Get-Random)

az servicebus namespace create `
 -n $name `
 -g $rg

az servicebus namespace authorization-rule keys list `
 -g $rg `
 --namespace-name $name `
 --name RootManageSharedAccessKey `
 --query primaryConnectionString

az servicebus queue create `
 --namespace-name $name `
 -g $rg `
 -n testqueue

New-AzureRmServiceBusQueue `
 -ResourceGroupName $name `
 -NamespaceName $rg `
 -name testqueue `
 -EnablePartitioning $false