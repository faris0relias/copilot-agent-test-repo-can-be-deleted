using '../infra.bicep'

param resourceGroupName = 'prd-us-content-library-service-rg'
param envPrefix = 'prd-us'
param primaryLocation = 'northcentralus'
param pairedLocation = 'southcentralus'
param context = 'content-library-service'
//Required if character count for context param is longer than 7 characters
//kvContext should contain no more than 7 characters
param kvContext = 'cl-svc'
//Required if character count for context param is longer than 10 characters
//saContext should contain no more than 10 characters
param saContext = 'clsvc'
param envTags = {
  'Date Created': '20221013'
}
param apiHostname = 'content-library-serviceapi.reliaslearning.com'
param apiTrafficManagerMonitorPath = '/api/healthcheck'
param privateDnsZoneResourceGroup = 'prd-ncus-lms-conn-rg'
param managementSubscriptionId = '148f5a47-b62d-43e6-a5e7-34cd39df4c5f'
param connectivitySubscriptionId = '462f3475-7dac-4ae8-bf9d-e0d165cfbef5'
param consumerTrafficManagerMonitorPath = '/healthcheck'
param consumerHostname = 'cls-consumer.reliaslearning.com'
param existingManagedIdentitySubscriptionId = '48d98a9a-2f7b-46ac-9a18-3aabd9fbd39a'
param sqlServerName = 'prd-us-lms-sqlmi-001-fog.df0914845b95.database.windows.net'
param identityUri = 'https://login.reliaslearning.com'
param rusticiClientUrl = 'http://engine.reliaslearning.com/engine'
param serviceBusResourceGroupName = 'prd-us-sb-rg'
