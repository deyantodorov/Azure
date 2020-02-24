$rg = "dt-az-play"
$loc = "westeurope"
$serverName = "sqlserver" + $(Get-Random)
$adminLogin = "ServerAdmin"
$adminPassword = "P@ssword" + $(Get-Random)
$dbName = "masking"

az sql server create `
 -g $rg `
 -l $loc `
 -n $serverName `
 --admin-user $adminLogin `
 --admin-password $adminPassword

az sql db create `
 -g $rg `
 -s $serverName `
 -n $dbName `
 --service-objective S0

<#
CREATE TABLE dbo.Users (
  UserId int NOT NULL,
  AccountCode varchar(50) NOT NULL,
  Pin varchar(10) NOT NULL,
  Name varchar(50) NOT NULL
)
  
INSERT INTO Users VALUES(1, '123-4567-89', 'ABCD', 'Mike')
SELECT * FROM Users;
#>

New-AzureRmSqlDatabaseDataMaskingRule `
  -ResourceGroupName $rgName `
  -ServerName $serverName `
  -DatabaseName $dbName `
  -SchemaName "dbo" `
  -TableName "Users" `
  -ColumnName "AccountCode" `
  -MaskingFunction Text `
  -SuffixSize 2 `
  -ReplacementString "xxxxx"

<#
  USE masking;
  GO
  CREATE USER user1 WITHOUT LOGIN;
  GRANT SELECT ON OBJECT::dbo.Users TO user1;  
  GO
  
  USE masking;
  EXECUTE AS USER = 'user1';
  SELECT * FROM Users;
  REVERT;
#>  

Remove-AzureRmSqlDatabaseDataMaskingRule `
  -ResourceGroupName $rgName `
  -ServerName $serverName `
  -DatabaseName $dbName `
  -SchemaName "dbo" `
  -TableName "Users" `
  -ColumnName "AccountCode" 

New-AzureRmSqlDatabaseDataMaskingRule `
  -ResourceGroupName $rgName `
  -ServerName $serverName `
  -DatabaseName $dbName `
  -SchemaName "dbo" `
  -TableName "Users" `
  -ColumnName "AccountCode" `
  -MaskingFunction Text `
  -PrefixSize 2 `
  -ReplacementString "xxxxxxxx"
