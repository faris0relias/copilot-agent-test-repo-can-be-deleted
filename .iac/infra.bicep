targetScope = 'subscription'

@description('Function to help automatically concatenate a deployment name with a character limit of 65.')
func deploymentName(baseName string) string => take('${baseName}-${uniqueString(string(deployment()))}', 65)

@description('Context Name')
param context string

@description('Key Vault name abbreviation, since the character count is limited to 24.')
param kvContext string

//#########################################
//#### Uncomment this block as needed. ####
//#########################################
@description('Storage Account name abbreviation, since the character count is limited to 24.')
param saContext string

@description('Resource group name')
param resourceGroupName string

@description('Environment abbreviation')
@allowed(['dev1', 'dev2', 'stg-us', 'stg-de', 'prd-us', 'prd-ca', 'prd-de', 'sbox-us'])
param envPrefix string

@description('Resource/Resource Group location')
@allowed([
  'eastus2'
  'centralus'
  'northcentralus'
  'southcentralus'
  'canadacentral'
  'canadaeast'
  'germanywestcentral'
  'germanynorth'
])
param primaryLocation string

@description('Resource/Resource Group location')
@allowed([
  'eastus2'
  'centralus'
  'northcentralus'
  'southcentralus'
  'canadacentral'
  'canadaeast'
  'germanywestcentral'
  'germanynorth'
])
param pairedLocation string

@description('The subscription ID of the network connectivity hub for Private Link.')
param connectivitySubscriptionId string

@description('Log analytics workspace id')
param managementSubscriptionId string

@description('The tags attached to the resource')
param envTags object = {}

@description('DNS value where the api will be accessible. This should match the host configured in the kuberentes ingress object.')
param apiHostname string = ''

@description('DNS value where the api will be accessible. This should match the host configured in the kuberentes ingress object.')
param consumerHostname string = ''

@description('Path to healthcheck for the webapi pods. Utilized by the traffic manager.')
param apiTrafficManagerMonitorPath string

@description('Path to healthcheck for the consumer. Utilized by the traffic manager.')
param consumerTrafficManagerMonitorPath string

@description('The resource group name that the private DNS zone is in for Private Link.')
param privateDnsZoneResourceGroup string

@description('Managed Identity subscription id.')
param existingManagedIdentitySubscriptionId string

@description('Param for connecting to sql server .')
param sqlServerName string

@description('URI for the Identity Service')
param identityUri string

@description('Time of the deployment')
param baseTime string = utcNow('yyyyMMddTHHmmssZ')

@description('Resource Incrementor, used for Blue/Green or Canary Deployments.')
param increment string = '001'

@description('URL for the Rustici Client')
param rusticiClientUrl string

@description('The resource group name for the Service Bus.')
param serviceBusResourceGroupName string

// cls-alerts-non-prod-aaaap7letzerozkakm5byvojie@relias-engineering.slack.com => #cm-audit-log-alerts-non-prod
// cls-alerts-prod-aaaaqanyksr4gzqnzadcmhzxie@relias-engineering.slack.com => #cm-audit-log-alerts-prod
var slackEmailChannelEmail  = {
  dev1: 'cls-alerts-non-prod-aaaap7letzerozkakm5byvojie@relias-engineering.slack.com'
  dev2: 'cls-alerts-non-prod-aaaap7letzerozkakm5byvojie@relias-engineering.slack.com'
  stg: 'cls-alerts-non-prod-aaaap7letzerozkakm5byvojie@relias-engineering.slack.com'
  prd: 'cls-alerts-prod-aaaaqanyksr4gzqnzadcmhzxie@relias-engineering.slack.com'
  sbox: 'cls-alerts-non-prod-aaaap7letzerozkakm5byvojie@relias-engineering.slack.com'
}

var serviceBusName = '${locationMap.shortEnvironment}-${locationMap[primaryLocation]}-sb-${increment}'
// Reference Azure Service Bus - Environmental Service Bus
resource extServiceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusName
  scope: resourceGroup(serviceBusResourceGroupName)
}

// Splits the envPrefix to grab the first segment - used for naming resource - eg. stg-ncus, prd-cac, etc.
var env = split(envPrefix, '-')[0]

// Joins the standard tags for your context with any custom tags passed in via the parameter file.
var tags = union(envTags, {
  ApplicationName: 'content-library-service'
  BusinessUnit: 'Platform'
  Owner: 'Relias'
  Environment: envPrefix
})

// Used in naming resources, will derive the shorthand from the full location code.
var locationMap = {
  eastus2: 'eus2'
  centralus: 'cus'
  southcentralus: 'scus'
  northcentralus: 'ncus'
  canadaeast: 'cae'
  canadacentral: 'cac'
  germanynorth: 'den'
  germanywestcentral: 'dewc'
  westeurope: 'weu'
  shortEnvironment: split(envPrefix, '-')[0]
}

//#########################################
//#### Uncomment this block as needed. ####
//#########################################
// // Settings specific to the frontend app component
// var app = {
//   primary: {
//     trafficManagerName: '${envPrefix}-${context}-app-tm-001'
//   }
//   paired: {}
// }

// Settings specific to the backendapi component
var api = {
  primary: {
    trafficManagerName: '${envPrefix}-${context}-api-tm-001'
  }
  paired: {}
}

// consumer tm set up 
var consumer = {
  primary: {
    trafficManagerName: '${envPrefix}-${context}-consumer-tm-001'
  }
  paired: {}
}

// Settings shared at the context level between all components
var common = {
  primary: {
    appInsightsName: '${env}-${locationMap[primaryLocation]}-${context}-insights-001'
    appConfigName: '${env}-${locationMap[primaryLocation]}-${context}-appconfig-001'
    keyVaultName: !empty(kvContext)
      ? '${env}-${locationMap[primaryLocation]}-${kvContext}-kv-001'
      : '${env}-${locationMap[primaryLocation]}-${context}-kv-001'
    vnetName: '${env}-${locationMap[primaryLocation]}-core-vnet'
    subnetName: '${env}-${locationMap[primaryLocation]}-prvendpt-snet'
    vnetResourceGroup: '${env}-${locationMap[primaryLocation]}-lms-core-rg'
    //#########################################
    //#### Uncomment this block as needed. ####
    //#########################################
    storageAccountName: !empty(saContext) ? '${env}${locationMap[primaryLocation]}${saContext}sa001' : '${env}${locationMap[primaryLocation]}${context}sa001'
    cosmosAccountName: '${env}-${locationMap[primaryLocation]}-${context}-cosmos-01'
  }
  paired: {
    appConfigName: 'Secondary'
    vnetName: '${env}-${locationMap[pairedLocation]}-core-vnet'
    subnetName: '${env}-${locationMap[pairedLocation]}-prvendpt-snet'
    vnetResourceGroup: '${env}-${locationMap[pairedLocation]}-lms-core-rg'
  }
}

// The DNS address of the primary endpoint - this is the cluster AGIC in the primary region.
var trafficManagerEndpoint1 = '${env}-${locationMap[primaryLocation]}-lms.${primaryLocation}.cloudapp.azure.com'

// The DNS address of the paired endpoint - this is the cluster AGIC in the paired region.
var trafficManagerEndpoint2 = '${env}-${locationMap[pairedLocation]}-lms.${pairedLocation}.cloudapp.azure.com'

// Name of the shared Log Analytics Workspace used for application insights.
var logAnalyticsWorkspaceName = '${env}-${locationMap[primaryLocation]}-appinsights-law-001'

// Resource Group of the shared Log Analytics Workspace used for application insights
var logAnalyticsWorkspaceRg = '${env}-${locationMap[primaryLocation]}-appinsights-rg'

//#####################################################################
//#### Uncomment this block and update with your values as needed. ####
//#####################################################################
// // Detials for the actionGroup, which is used to send alerts to the team.
var actionGroupConfig = {
  name: 'content-library-service-ag-001'
  shortName: take('cl-svc-ag1', 12) // action group shortName has a 12 character limit
  emailReceivers: [
    {
      name: 'womm'
      emailAddress: 'cyeung@relias.com' // temp
      useCommonAlertSchema: true
    }
    {
      name: 'cls-consumer-alert'
      emailAddress: 'cls-consumer-alert-aaaaorgbjz4aexebwienf3cb6e@relias-engineering.slack.com'
      useCommonAlertSchema: true
    }
  ]
}

// Creates the Resource Group for resources
module rg 'br/ReliasRegistry:microsoft.resources.resource-group:0.2' = {
  name: deploymentName('rg-${resourceGroupName}')
  scope: subscription()
  params: {
    name: resourceGroupName
    location: primaryLocation
    tags: tags
  }
}

// Retrieves the existing shared Log Analytics Workspace to be leveraged during the AppInsights deployment.
resource law 'Microsoft.OperationalInsights/workspaces@2020-08-01' existing = {
  name: logAnalyticsWorkspaceName
  scope: resourceGroup(managementSubscriptionId, logAnalyticsWorkspaceRg)
}

// Deploys Application Insights for Application Monitoring, utilizes the shared Log Analytics Workspace for storage to support cross-service queries.
module appInsights 'br/ReliasRegistry:microsoft.insights.component:0.3' = {
  name: deploymentName('${common.primary.appInsightsName}')
  scope: resourceGroup(resourceGroupName)
  dependsOn: [
    law
    rg
  ]
  params: {
    name: common.primary.appInsightsName
    location: primaryLocation
    workspaceResourceId: law.id
    tags: tags
  }
}

// Refernce the Application Insights resource to retrieve the connectionString without passing the value through the module output.
resource existingAppInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: common.primary.appInsightsName
  scope: resourceGroup(resourceGroupName)
}

var existingManagedIdentityName = '${envPrefix}-content-library-service-aksworkload-mi-001'
var existingManagedIdentityResourceGroup = '${envPrefix}-aksworkload-mi-rg'
resource existingManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: existingManagedIdentityName
  scope: resourceGroup(existingManagedIdentitySubscriptionId, existingManagedIdentityResourceGroup)
}

// Reference the Exsting network resources for private endpoints
resource privateLinkvirtualNetworkResource 'Microsoft.Network/virtualNetworks@2021-05-01' existing = {
  name: common.primary.vnetName
  scope: resourceGroup(common.primary.vnetResourceGroup)
}

resource privateLinkSubnetResource 'Microsoft.Network/virtualNetworks/subnets@2021-05-01' existing = {
  name: common.primary.subnetName
  parent: privateLinkvirtualNetworkResource
}

// Deploys the Azure Key Vault and Private Link for the context. 
// These secret values must be modified in the module, and the outputs are passed to Azure Application Configuration to be loaded by the service at runtime.
module keyVault 'br/ReliasRegistry:relias.key-vault.vault:0.4' = {
  name: deploymentName('${common.primary.keyVaultName}')
  scope: resourceGroup(resourceGroupName)
  dependsOn: [
    appInsights
    rg
  ]
  params: {
    keyVaultName: common.primary.keyVaultName
    location: primaryLocation
    sku: 'standard'
    tags: tags
    vnetName: common.primary.vnetName
    vnetResourceGroup: common.primary.vnetResourceGroup
    subnetName: common.primary.subnetName
    connectivitySubscriptionId: connectivitySubscriptionId
    privateDnsZoneResourceGroup: privateDnsZoneResourceGroup
    secrets: {
      secret1: {
        name: 'ApplicationInsightsConnectionString'
        value: existingAppInsights.properties.ConnectionString
        appConfigName: 'ApplicationInsights:ConnectionString'
      }

      //######################################################################################################################################
      //#### Sample code for adding new non-secure secrets to the key vault, Uncomment these blocks and update with your values as needed ####
      //######################################################################################################################################
      // secret2:{
      //   name: 'SampleSecret2'
      //   value: 'SampleValue2'
      //   appConfigName: 'SampleSecretAppConfigName2'
      // }
      // secret3:{
      //   name: 'SampleSecret3'
      //   value: 'SampleValue3'
      //   appConfigName: 'SampleSecretAppConfigName3'
      // } 
    }
  }
}

// set manually 
resource sbKeyVault 'Microsoft.KeyVault/vaults/secrets@2023-02-01' existing = {
  name: '${common.primary.keyVaultName}/ServiceBusConnectionString'
  scope: resourceGroup(resourceGroupName)
}

resource cosmosConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' existing = {
  name: '${common.primary.keyVaultName}/CosmosConnectionString'
  scope: resourceGroup(resourceGroupName)
}

resource rusticiApiId 'Microsoft.KeyVault/vaults/secrets@2023-02-01' existing = {
  name: '${common.primary.keyVaultName}/RusticiApiId'
  scope: resourceGroup(resourceGroupName)
}

resource rusticiApiSecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' existing = {
  name: '${common.primary.keyVaultName}/RusticiApiSecret'
  scope: resourceGroup(resourceGroupName)
}
// Deploys Azure Application Configuration to store AppSettings for the service(s) to be loaded at runtime.
module appConfig 'br/ReliasRegistry:relias.app-configuration.configuration-store:0.1' = {
  name: deploymentName('${common.primary.appConfigName}')
  scope: resourceGroup(resourceGroupName)
  dependsOn: [rg]
  params: {
    configStoreName: common.primary.appConfigName
    replicaName: common.paired.appConfigName
    location: primaryLocation
    replicaLocation: pairedLocation
    sku: 'standard'
    tags: tags
    configValues: [
      //##############################################################################################################################################
      //#### Sample code for adding new appconfig entries to the appconfig resource, Uncomment these blocks and update with your values as needed ####
      //##############################################################################################################################################
      {
        name: 'BaseSettings:SqlServerConnectionString'
        contentType: 'string'
        value: 'Server=${sqlServerName};Database=content-library-service;Authentication=Active Directory Managed Identity;User Id=${existingManagedIdentity.properties.clientId};Encrypt=true'
      }
      {
        name: 'BaseSettings:PolicyPublishedEventTopicName'
        contentType: 'string'
        value: 'compliance.policy.published'
      }
      {
        name: 'BaseSettings:PolicyPublishedEventSubscription'
        contentType: 'string'
        value: 'compliance.policy.published.cls'
      }
      {
        name: 'BaseSettings:PolicyUpdatedEventTopicName'
        contentType: 'string'
        value: 'compliance.policy.updated'
      }
      {
        name: 'BaseSettings:PolicyUpdatedEventSubscription'
        contentType: 'string'
        value: 'compliance.policy.updated.cls'
      }
      {
        name: 'BaseSettings:PolicyArchivedEventTopicName'
        contentType: 'string'
        value: 'compliance.policy.archived'
      }
      {
        name: 'BaseSettings:PolicyArchivedEventSubscription'
        contentType: 'string'
        value: 'compliance.policy.archived.cls'
      }
      {
        name: 'BaseSettings:ContentChangedEventTopicName'
        contentType: 'string'
        value: 'contentlibrary.content.changed'
      }
      {
        name: 'BaseSettings:PolicyContentAssignedEventTopicName'
        contentType: 'string'
        value: 'compliance.policy.content.assigned'
      }
      {
        name: 'BaseSettings:AssignContentEventTopicName'
        contentType: 'string'
        value: 'contentscheduler.assign.content'
      }
      {
        name: 'BaseSettings:AssignContentEventSubscription'
        contentType: 'string'
        value: 'contentscheduler.assign.content.cls'
      }
      {
        name: 'BaseSettings:PolicyAttestedEventTopicName'
        contentType: 'string'
        value: 'compliance.policy.attested'
      }
      {
        name: 'BaseSettings:PolicyAttestedEventSubscription'
        contentType: 'string'
        value: 'compliance.policy.attested.cls'
      }
      {
        name: 'BaseSettings:ContentCompletedEventTopicName'
        contentType: 'string'
        value: 'contentlibrary.content.completed'
      }
      {
        name: 'AppSettings:Identity'
        contentType: 'string'
        value: identityUri
      }
      {
        name: 'BaseSettings:PolicySyncEventTopicName'
        contentType: 'string'
        value: 'compliance.policy.published.sync'
      }
      {
        name: 'BaseSettings:PolicySyncEventSubscription'
        contentType: 'string'
        value: 'compliance.policy.published.sync.cls'
      }
      {
        name: 'BaseSettings:ContentArchivedEventTopicName'
        contentType: 'string'
        value: 'contentlibrary.content.archived'
      }
      {
        name: 'AppSettings:SqlServerConnectionString'
        contentType: 'string'
        value: 'Server=${sqlServerName};Database=content-library-service;Authentication=Active Directory Managed Identity;User Id=${existingManagedIdentity.properties.clientId};Encrypt=true'
      }
      {
        name: 'AppSettings:AccountEndpoint'
        value: 'https://${common.primary.cosmosAccountName}.documents.azure.com:443/'
      }
      {
        name: 'AppSettings:DatabaseId'
        value: 'ContentLibraryServiceCosmosDb'
      }  
      {
        name: 'AppSettings:RusticiApiUrl'
        value: rusticiClientUrl
      }
      {
        name: 'AppSettings:StorageAccount'
        contentType: 'string'
        value: storageAccountResource.properties.primaryEndpoints.blob
      }
      {
        name: 'AppSettings:RetryNumber'
        contentType: 'string'
        value: '60'
      }
    ]
    secretAppConfigReferences: union(keyVault.outputs.secretAppConfigReferences, [
      {
        name: 'BaseSettings:ServiceBusConnectionString'
        secretUri: sbKeyVault.properties.secretUri
      }
      {
        name: 'AppSettings:CosmosRepositoryOptions:CosmosConnectionString'
        secretUri: cosmosConnectionStringSecret.properties.secretUri
      }
      {
        name: 'AppSettings:RusticiApiId'
        secretUri: rusticiApiId.properties.secretUri
      }
      {
        name: 'AppSettings:RusticiApiSecret'
        secretUri: rusticiApiSecret.properties.secretUri
      }
    ])
  }
}

// Deploys the traffic manager for the backend api pods to route traffic between both regions.
module apiTrafficManager 'br/ReliasRegistry:relias.network.trafficmanagerprofile:0.2' = {
  name: deploymentName('${api.primary.trafficManagerName}')
  scope: resourceGroup(resourceGroupName)
  dependsOn: [rg]
  params: {
    trafficManagerName: api.primary.trafficManagerName
    tags: tags
    targetEndpoint1: trafficManagerEndpoint1
    targetEndpoint2: trafficManagerEndpoint2
    targetLocation1: primaryLocation
    targetLocation2: pairedLocation
    customHeaderHost: apiHostname
    monitorPath: apiTrafficManagerMonitorPath
  }
}

module consumerTrafficManager 'br/ReliasRegistry:relias.network.trafficmanagerprofile:0.2' = {
  name: deploymentName('${consumer.primary.trafficManagerName}')
  scope: resourceGroup(resourceGroupName)
  dependsOn: [rg]
  params: {
    trafficManagerName: consumer.primary.trafficManagerName
    tags: tags
    targetEndpoint1: trafficManagerEndpoint1
    targetEndpoint2: trafficManagerEndpoint2
    targetLocation1: primaryLocation
    targetLocation2: pairedLocation
    customHeaderHost: consumerHostname
    monitorPath: consumerTrafficManagerMonitorPath
  }
}

module deadLetterQueueAlert './modules/deadLetterQueueAlert.bicep' = {
  scope: resourceGroup(resourceGroupName)
  name: take('deadLetterQueueAlert_${baseTime}', 64)
  params: {
    environment: envPrefix
    location: primaryLocation
    serviceBusId:extServiceBus.id
    emailReceivers: [
      {
        name: 'Email #cls Slack ${slackEmailChannelEmail[locationMap.shortEnvironment]}'
        emailAddress: '${slackEmailChannelEmail[locationMap.shortEnvironment]}'
        useCommonAlertSchema: true
      }
    ]
  }
}

//###########################################################################################################################
//#### Sample code for adding new storage account resource, Uncomment these blocks and update with your values as needed ####
//###########################################################################################################################
module storageAccount 'br/ReliasRegistry:microsoft.storage.storage-account:0.8' = {
  name: deploymentName(common.primary.storageAccountName)
  scope: resourceGroup(resourceGroupName)
  dependsOn: [ rg ]
  params: {
    name: common.primary.storageAccountName
    location: primaryLocation
    publicNetworkAccess: 'Disabled'
    managedIdentities: {
      systemAssigned: true
    }
//     // //######################################################################################################################
//     // //#### Sample code for adding new Blob Services Object, Uncomment these blocks and update with your values as needed ####
//     // //######################################################################################################################
    blobServices: {
      containers: [
        {
          name: 'main'
        }
      ]
    }
    // Add private endpoints configuration
    privateEndpoints: [
      {
        name: 'pe-blob-${common.primary.storageAccountName}'
        privateLinkServiceConnectionName: 'pe-blob-${common.primary.storageAccountName}'
        subnetResourceId: privateLinkSubnetResource.id
        privateDnsZoneGroupName: storageAccountBlobPrivateDnsZone.name
        privateDnsZoneResourceIds: [
          storageAccountBlobPrivateDnsZone.id
        ]  
        resourceGroupName: resourceGroupName
        service: 'blob'
      }
    ]
//     // //#######################################################################################################################
//     // //#### Sample code for adding new File Services Object, Uncomment these blocks and update with your values as needed ####
//     // //#######################################################################################################################
//     // fileServices: {
//     //   shares: [
//     //     {
//     //       name: 'sampleshare1'
//     //     }
//     //     {
//     //       name: 'sampleshare2'
//     //     }
//     //   ]
//     // }
//     // //#########################################################################################################################
//     // //#### Sample code for adding new Queues Services Object, Uncomment these blocks and update with your values as needed ####
//     // //#########################################################################################################################
//     // queueServices: {
//     //   queues: [
//     //     {
//     //       name: 'samplequeue1'
//     //     }
//     //     {
//     //       name: 'samplequeue2'
//     //     }
//     //   ]
//     // }
//     // //########################################################################################################################
//     // //#### Sample code for adding new Table Services Object, Uncomment these blocks and update with your values as needed ####
//     // //########################################################################################################################
//     // tableServices: {
//     //   tables: [
//     //     {
//     //       name: 'sampletable1'
//     //     }
//     //     {
//     //       name: 'sampletable2'
//     //     }
//     //   ]
//     // }
    tags: tags
  }
}

resource storageAccountResource 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: common.primary.storageAccountName
  scope: resourceGroup(resourceGroupName)
}


module defenderForStorageSettings 'br/ReliasRegistry:relias.security.defenderforstoragsettings:0.2' = {
  name: take('defenderForStorageSettings_${baseTime}', 64)
  scope: resourceGroup(resourceGroupName)
  dependsOn: [
    storageAccount
  ]
  params: {
    storageAccountName: common.primary.storageAccountName
    defenderForStorageSettingsName: 'current'
    defenderForStorageSettingsIsEnabled: true
    malwareScanningIsEnabled: true
    sensitiveDataDiscoveryIsEnabled: true
    overrideSubscriptionLevelSettings: true
    capGBPerMonth: 10000
  }
}


//##################################################################################################################
//#### Sample code for adding new cosmos resource, Uncomment these blocks and update with your values as needed ####
//##################################################################################################################
// Reference Private DNS Zones for Cosmos
var cosmosPrivateDnsZoneName = 'privatelink.documents.azure.com'
resource cosmosPrivateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' existing = {
  name: cosmosPrivateDnsZoneName
  scope: resourceGroup(connectivitySubscriptionId, privateDnsZoneResourceGroup)
}

// Private DNS Zone for Blob Service
var storageEnvURL = environment().suffixes.storage
var storageAccountBlobPrivateDnsZoneName = 'privatelink.blob.${storageEnvURL}'
resource storageAccountBlobPrivateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' existing = {
  name: storageAccountBlobPrivateDnsZoneName
  scope: resourceGroup(connectivitySubscriptionId, privateDnsZoneResourceGroup)
}

module cosmosAccount 'br/ReliasRegistry:microsoft.document-db.database-account:0.6' = {
  name: deploymentName(common.primary.cosmosAccountName)
  scope: resourceGroup(resourceGroupName)
  params: {
    name: common.primary.cosmosAccountName
    location: primaryLocation
    disableLocalAuth: false
    backupPolicyType: 'Periodic'
    locations: [
      {
        locationName: primaryLocation
        failoverPriority: 0
        isZoneRedundant: false
      }
      {
        locationName: pairedLocation
        failoverPriority: 1
        isZoneRedundant: false
      }
    ]
    defaultConsistencyLevel: 'Strong'
    sqlDatabases: [
      {
        name: 'ContentLibraryServiceCosmosDb'
        containers: [
          {
            name: 'FinalExam'
            autoscaleSettingsMaxThroughput: 1000
            paths: [
              '/courseId'
            ]
            indexingPolicy: {
              indexingMode: 'consistent'
              includedPaths: [
                {
                  path: '/*'
                }
              ]
              excludedPaths: [
                {
                  path: '/_etag/?'
                }
              ]
            }
          }
          {
            name: 'LearningContent'
            autoscaleSettingsMaxThroughput: 1000
            paths: [
              '/courseId'
            ]
            indexingPolicy: {
              indexingMode: 'consistent'
              includedPaths: [
                {
                  path: '/*'
                }
              ]
              excludedPaths: [
                {
                  path: '/_etag/?'
                }
              ]
            }
          }
        ]
      }
    ]
    privateEndpoints: [
      {
        name: 'pe-${common.primary.cosmosAccountName}'
        privateLinkServiceConnectionName: 'pe-${common.primary.cosmosAccountName}'
        subnetResourceId: privateLinkSubnetResource.id
        privateDnsZoneGroup: {
          name: cosmosPrivateDnsZone.name
          privateDnsZoneGroupConfigs: [
            {
              privateDnsZoneResourceId: cosmosPrivateDnsZone.id
            }
          ]
        }
        resourceGroupName: common.primary.vnetResourceGroup
        service: 'sql'
      }
    ]
  }
  dependsOn: [
    rg
  ]
}

//#########################################
//#### Uncomment this block as needed. ####
//#########################################
// // Deploys the action group to send alerts to the emails specified. Can be modified to add SMS or other functionality as desired per the module.
module actionGroup 'br/ReliasRegistry:microsoft.insights.action-group:0.2' = {
  name: deploymentName(actionGroupConfig.name)
  scope: resourceGroup(resourceGroupName)
  dependsOn: [rg]
  params: {
    location: 'global'
    name: actionGroupConfig.name
    groupShortName: actionGroupConfig.shortName
    enabled: false // setting to false so it doesn't send emails.
    emailReceivers: actionGroupConfig.emailReceivers // commented out since email addresses are invalid sample addresses.
  }
}

//#########################################
//#### Uncomment this block as needed. ####
//#########################################
// Deploys out pod monitors for each grouping of pods in your context as defined in the podsToMonitor object.
// module podMonitor 'br/ReliasRegistry:relias.monitoring.k8s-pod-monitor:0.1' = {
//   name: deploymentName('podMonitor')
//   scope: resourceGroup(resourceGroupName)
//   dependsOn: [ actionGroup ]
//   params: {
//     actionGroupId: actionGroup.outputs.resourceId
//     env: env
//     primaryLocation: primaryLocation
//     pairedLocation: pairedLocation
//     enabled: false // setting to false so it doesn't incur cost when monitoring the pods.
//     podsToMonitor: [
//       {
//         name: 'webapp'
//         namespace: '${context}-ns'
//         monitorWindow: 15
//       }
//       {
//         name: 'webapi'
//         namespace: '${context}-ns'
//         monitorWindow: 15
//       }
//     ]
//   }
// }

//#########################################
//#### Uncomment this block as needed. ####
//#########################################
// // Healthcheck monitor for the webapp service
// module appHealthcheckMonitor 'br/ReliasRegistry:relias.monitoring.healthcheck-monitor:0.2' = {
//   name: deploymentName('${envPrefix}-${context}-app')
//   scope: resourceGroup(resourceGroupName)
//   params: {
//     name: '${envPrefix}-${context}-app'
//     tags: tags
//     location: primaryLocation
//     frequency: 1
//     timeout: 5
//     kind: 'standard'
//     enabled: false // setting to false so it doesn't incur cost
//     appInsightsId: appInsights.outputs.resourceId
//     actionGroupId: actionGroup.outputs.resourceId
//     contentMatchString: 'OK'
//     requestUrl: 'https://${appHostname}/health'
//   }
// }

//#########################################
//#### Uncomment this block as needed. ####
//#########################################
// Healthcheck monitor for the webapi service
// module apiHealthcheckMonitor 'br/ReliasRegistry:relias.monitoring.healthcheck-monitor:0.2' = {
//   name: deploymentName('${envPrefix}-${context}-api')
//   scope: resourceGroup(resourceGroupName)
//   params: {
//     name: '${envPrefix}-${context}-api'
//     tags: tags
//     location: primaryLocation
//     frequency: 1
//     timeout: 5
//     kind: 'standard'
//     enabled: false // setting to false so it doesn't incur cost
//     appInsightsId: appInsights.outputs.resourceId
//     actionGroupId: actionGroup.outputs.resourceId
//     contentMatchString: 'OK'
//     requestUrl: 'https://${apiHostname}/api/healthcheck'
//   }
// }
