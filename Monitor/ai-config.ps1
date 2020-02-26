$rand = Get-Random
$rg = "dt-az-play"
$ainame = "appInsightName" + $rand

$propsFile = "props.json" 
'{"Application_Type":"web"}' | Out-File $propsFile

az resource create `
 -g $rg `
 -n $ainame `
 --resource-type "Microsoft.Insights/components" `
 --properties "@$propsFile"

 Remove-Item $propsFile

 az resource show `
  -g $rg `
  -n $ainame `
  --resource-type "Microsoft.Insights/components" `
  --query "properties.InstrumentationKey" `
  -o tsv