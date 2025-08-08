using '../infra.bicep'

param resourceGroupName = 'dev1-content-library-service-rg'
param envPrefix = 'dev1'
param primaryLocation = 'eastus2'
param pairedLocation = 'centralus'
param context = 'content-library-service'
//Required if character count for context param is longer than 7 characters
//kvContext should contain no more than 7 characters
param kvContext = 'cl-svc'
//Required if character count for context param is longer than 10 characters
//saContext should contain no more than 10 characters
param saContext = 'clsvc'
param envTags = {
  'Date Created': '20241001'
}
param apiHostname = 'dev1-contentlibraryserviceapi.reliaslearning.com'
param consumerHostname = 'dev1-cls-consumer.reliaslearning.com'
param apiTrafficManagerMonitorPath = '/api/healthcheck'
param consumerTrafficManagerMonitorPath = '/healthcheck'
param privateDnsZoneResourceGroup = 'dev-eus2-lms-conn-rg'
param managementSubscriptionId = '5d8f65cc-1c9a-4fac-a100-f56c5e8a0070'
param connectivitySubscriptionId = '7fae9531-527e-40f9-b7b6-61929e161a69'
param existingManagedIdentitySubscriptionId = '3741355f-a2fe-4aa7-b024-1cecffdca327'
param sqlServerName = 'dev1-lms-sqlmi-001-fog.9f0e1cdca4c9.database.windows.net'
param identityUri = 'https://dev1-login.reliaslearning.com'
param rusticiClientUrl = 'https://dev1-engine.reliaslearning.com/engine'
param serviceBusResourceGroupName = 'dev1-sb-rg'
