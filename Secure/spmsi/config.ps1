$spname = "myapp" + (Get-Random)

$sp = az ad sp create-for-rbac --name $spname | ConvertFrom-Json
$sp

az ad sp show --id $sp.appId
az ad sp list --display-name $spname

az role assignment list --assignee $sp.appId
az role assignment list --assignee $sp.appId --query '[0].{"roleDefinitionName"}'

az role definition list --output json `
 --query '[].{"roleName":roleName, "description":description}'

az role definition list --custom-role-only false --output json `
 --query '[].{"roleName":roleName, "description":description, "roleType":roleType}'

az role definition list `
 -n "Contributor"

az role definition list `
 --name "Contributor" `
 -o json `
 --query '[].{"actions":permissions[0].actions, "notActions":permissions[0].notActions}'

$webappname = "mywebapp" + $(Get-Random)
$rg = "dt-az-play"
$webappplan = $webappname
$loc = "westeurope"

az appservice plan create `
 -n $webappplan `
 -g $rg `
 --sku FREE

az webapp create `
 -g $rg `
 --plan $webappplan `
 -n $webappname

$sampleweb = az webapp show `
  --name $webappname `
  -g $rg | ConvertFrom-Json

$sampleweb.id

az role assignment create `
 --role "Website Contributor" `
 --assignee $sp.appId `
 --scope $sampleweb.id

az role assignment delete --assignee $sp.appId --role "Contributor"

az role definition list `
--output json

$sysid = az webapp identity assign `
 -g $rg `
 -n $webappname
$sysid

az webapp identity show `
 -n $webappname `
 -g $rg

