# $rg='your resource group'
# $loc='your location'
# $stgname='your account name'

az storage account create -n $stgname -g $rg -l $loc --sku Standard_LRS