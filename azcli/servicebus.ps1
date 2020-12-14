# $rg='your resource group'
# $loc='your location'
# $svcbusname='your account name'

az servicebus namespace create -n $svcbusname -g $rg -l $loc --sku Standard
az servicebus topic create -g $rg --namespace-name $svcbusname -n 'myevents'
az servicebus topic subscription create -g $rg --namespace-name $svcbusname --topic-name 'myevents' -n 'mysubscription'