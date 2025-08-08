using '../infra.bicep'

param resourceGroupName = 'stg-us-content-library-service-rg'
param envPrefix = 'stg-us'
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
param apiHostname = 'stg-content-library-serviceapi.reliaslearning.com'
param apiTrafficManagerMonitorPath = '/api/healthcheck'
param privateDnsZoneResourceGroup = 'prd-ncus-lms-conn-rg'
param managementSubscriptionId = '148f5a47-b62d-43e6-a5e7-34cd39df4c5f'
param connectivitySubscriptionId = '462f3475-7dac-4ae8-bf9d-e0d165cfbef5'
param consumerTrafficManagerMonitorPath = '/healthcheck'
param consumerHostname = 'stg-cls-consumer.reliaslearning.com'
param existingManagedIdentitySubscriptionId = 'bda20fca-5f2e-4080-baf8-3154a48b2fee'
param sqlServerName = 'stg-us-lms-sqlmi-001-fog.6631384732a4.database.windows.net'
param identityUri = 'https://slogin.reliaslearning.com'
param rusticiClientUrl = 'http://sengine.reliaslearning.com/engine'
param serviceBusResourceGroupName = 'stg-us-sb-rg'
