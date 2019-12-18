$random = Get-Random
$rgName = 'my-azure-batch-rg-' + $random
$location = 'westeurope'
$storageAccountName = 'mystgacc' + $random
$storageAccountSku = 'Standard_LRS'
$batchAccountName = 'mybatchacc' + $random
$poolName = 'myPool'
$jobName = 'myJob'

Write-Output('Resource group name is: ' + $rgName)
Write-Output('Storage account name is: ' + $storageAccountName)
Write-Output('Batch account name is: ' + $batchAccountName)
Write-Output('Pool name is: ' + $poolName)

Write-Output('1. Create Azure Resource Group')

az group create `
 -l $location `
 -n $rgName

Write-Output('2. Create Storage Account')

az storage account create `
 -g $rgName `
 -n $storageAccountName `
 -l $location `
 --sku $storageAccountSku

Write-Output('3. Create Batch Account')

az batch account create `
 -n $batchAccountName `
 --storage-account $storageAccountName `
 -g $rgName `
 -l $location 

Write-Output('4. Set Batch Account Login')

az batch account login `
 -n $batchAccountName `
 -g $rgName `
 --shared-key-auth

Write-Output('5. Create Batch Pool')

az batch pool create `
 --id $poolName `
 --vm-size Standard_A1_V2 `
 --target-dedicated-nodes 2 `
 --image canonical:ubuntuserver:16.04-LTS `
 --node-agent-sku-id 'batch.node.ubuntu 16.04'

az batch pool show `
 --pool-id $poolName `
 --query 'allocationState'

az batch job create `
 --id $jobName `
 --pool-id $poolName

for ($i = 0; $i -lt 4; $i++) {
    az batch task create `
     --task-id mytask$i `
     --job-id $jobName `
     --command-line "/bin/bash -c 'printenv | grep AZ_BATCH; sleep 90s'"
}

Write-Output('6. Show results')

az batch task show `
 --job-id $jobName `
 --task-id mytask1

az batch task file list `
 --job-id $jobName `
 --task-id mytask1 `
 --output table

az batch task file download `
 --job-id $jobName `
 --task-id mytask1 `
 --file-path stdout.txt `
 --destination ./stdout.txt

 Write-Output('7. Delete resources')

 az batch pool delete -n $poolName

 az group delete -n $rgName