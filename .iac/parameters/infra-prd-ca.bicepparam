using '../infra.bicep'

param resourceGroupName = 'prd-ca-content-library-service-rg'
param envPrefix = 'prd-ca'
param primaryLocation = 'canadacentral'
param pairedLocation = 'canadaeast'
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
param apiHostname = 'content-library-serviceapi.relias.ca'
param apiTrafficManagerMonitorPath = '/api/healthcheck'
param privateDnsZoneResourceGroup = 'prd-cac-lms-conn-rg'
param managementSubscriptionId = '4f20d1e5-7e76-44e1-8303-94fabcedc798'
param connectivitySubscriptionId = '0fa713d4-8de8-4982-99fb-e05d84f5e6b1'
param consumerTrafficManagerMonitorPath = '/healthcheck'
param consumerHostname = 'cls-consumer.relias.ca'
param existingManagedIdentitySubscriptionId = '760cd487-2f3b-42d2-82ea-124cb83096f4'
param sqlServerName = 'prd-ca-lms-sqlmi-001-fog.9a960ea45cf9.database.windows.net'
param identityUri = 'https://login.relias.ca'
param rusticiClientUrl = 'http://engine.relias.ca/engine'
param serviceBusResourceGroupName = 'prd-ca-sb-rg'
