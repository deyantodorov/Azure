$rg = 'dt-az-play2'
$app = 'docker-app-' + $(Get-Random)
$plan = $app + '-asp'
$loc = 'westeurope'
$container = 'microsoft/dotnet-samples:aspnetapp'

az group create `
 -n $rg `
 -l $loc

az appservice plan create `
 -n $plan `
 -g $rg `
 --sku B1 `
 --is-linux

az webapp create `
 -n $app `
 -g $rg `
 --plan $plan `
 --deployment-container-image-name $container

az webapp config appsettings set `
 -g $rg `
 -n $app `
 --settings WEBSITES_PORT=80

az webapp show `
 -n $app `
 -g $rg

az webapp show `
 -n $app `
 -g $rg `
 --query "defaultHostName" `
 -o tsv

az group delete -n $rg --yes
