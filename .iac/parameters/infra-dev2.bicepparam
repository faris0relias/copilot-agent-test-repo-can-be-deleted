using '../infra.bicep'

param resourceGroupName = 'dev2-content-library-service-rg'
param envPrefix = 'dev2'
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
  'Date Created': '20221013'
}
param apiHostname = 'dev2-contentlibraryserviceapi.reliaslearning.com'
param consumerHostname = 'dev2-cls-consumer.reliaslearning.com'
param apiTrafficManagerMonitorPath = '/api/healthcheck'
param consumerTrafficManagerMonitorPath = '/healthcheck'
param privateDnsZoneResourceGroup = 'dev-eus2-lms-conn-rg'
param managementSubscriptionId = '5d8f65cc-1c9a-4fac-a100-f56c5e8a0070'
param connectivitySubscriptionId = '7fae9531-527e-40f9-b7b6-61929e161a69'
param existingManagedIdentitySubscriptionId = '3741355f-a2fe-4aa7-b024-1cecffdca327'
param sqlServerName = 'dev2-lms-sqlmi-001-fog.11fd7194a436.database.windows.net'
param identityUri = 'https://dev2-login.reliaslearning.com'
param rusticiClientUrl = 'https://dev2-engine.reliaslearning.com/engine'
param serviceBusResourceGroupName = 'dev2-sb-rg'
