using '../infra.bicep'

param resourceGroupName = 'prd-de-content-library-service-rg'
param envPrefix = 'prd-de'
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
param apiHostname = 'content-library-serviceapi.relias.de'
param apiTrafficManagerMonitorPath = '/api/healthcheck'
param privateDnsZoneResourceGroup = 'prd-dewc-lms-conn-rg'
param managementSubscriptionId = 'ef305fde-0c06-4a71-8d00-239a290bd615'
param connectivitySubscriptionId = '20696ebd-7685-4c92-924c-4369b8543e22'
param consumerTrafficManagerMonitorPath = '/healthcheck'
param consumerHostname = 'cls-consumer.relias.de'
param existingManagedIdentitySubscriptionId = '6674136c-603f-4418-8397-06ece80c7273'
param sqlServerName = 'prd-de-lms-sqlmi-001-fog.04ad2f8d43ff.database.windows.net'
param identityUri = 'https://login.relias.de'
param rusticiClientUrl = 'http://engine.relias.de/engine'
param serviceBusResourceGroupName = 'prd-de-sb-rg'
