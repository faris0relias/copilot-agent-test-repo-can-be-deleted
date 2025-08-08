using '../infra.bicep'

param resourceGroupName = 'stg-de-content-library-service-rg'
param envPrefix = 'stg-de'
param primaryLocation = 'germanywestcentral'
param pairedLocation = 'germanynorth'
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
param apiHostname = 'stg-content-library-serviceapi.reliaslearning.de'
param apiTrafficManagerMonitorPath = '/api/healthcheck'
param privateDnsZoneResourceGroup = 'prd-dewc-lms-conn-rg'
param managementSubscriptionId = 'ef305fde-0c06-4a71-8d00-239a290bd615'
param connectivitySubscriptionId = '20696ebd-7685-4c92-924c-4369b8543e22'
param consumerTrafficManagerMonitorPath = '/healthcheck'
param consumerHostname = 'stg-cls-consumer.reliaslearning.de'
param existingManagedIdentitySubscriptionId = 'e6036236-a7f0-41c5-850a-54de53a62b15'
param sqlServerName = 'stg-de-lms-sqlmi-001-fog.fd16442d94ac.database.windows.net'
param identityUri = 'https://slogin.reliaslearning.de'
param rusticiClientUrl = 'http://sengine.reliasleanring.de/engine'
param serviceBusResourceGroupName = 'stg-de-sb-rg'
