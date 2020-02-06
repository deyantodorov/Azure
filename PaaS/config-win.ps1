$rg = 'dt-az-play'
$loc = 'westeurope'
$appname = 'webapp' + $(Get-Random)
$planname = $appname + '-asp'
$repourl = "https://github.com/azure-samples/php-docs-hello-world"

az appservice plan create `
 -n $planname `
 -g $rg `
 --sku B1 `
 -l $loc 

az webapp create `
 -n $appname `
 -g $rg `
 --plan $planname

az webapp deployment source config `
 -n $appname `
 -g $rg `
 --repo-url $repourl `
 --branch master `
 --manual-integration

 az webapp deployment source show `
  -n $appname `
  -g $rg

az webapp show `
 -n $appname `
 -g $rg

az webapp show `
 -n $appname `
 -g $rg `
 --query "defaultHostName" -o tsv

 az webapp show `
  -n $appname `
  -g $rg `
  --query "outboundIpAddresses" -o tsv

az webapp deployment source sync -n $appname -g $rg

az webapp delete -n $appname -g $rg
az appservice plan delete -n $planname -g $rg --yes
