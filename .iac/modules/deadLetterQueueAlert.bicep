@description('Environment for the template deployment.')
@allowed([ 'dev1', 'dev2', 'stg', 'stg-us', 'stg-de', 'prd', 'prd-ca', 'prd-de', 'prd-us', 'sbox', 'sbox-us', 'playground' ])
param environment string

@description('Resource/Resource Group location')
@allowed([ 'eastus2', 'centralus', 'northcentralus', 'southcentralus', 'canadacentral', 'canadaeast', 'germanywestcentral', 'germanynorth', ])
param location string

@description('Tags for the Resource')
param tags object = {}

@description('Service Bus to monitor for dead letter alerts')
param serviceBusId string

type EmailReceiver = {
  name: string
  emailAddress: string
  useCommonAlertSchema: bool
}
@description('Email receivers for the health check failure notifications')
param emailReceivers EmailReceiver[]



// Used in naming resources, will derive the shorthand from the full location code.
var _locationMap = {
  eastus2: 'eus2'
  centralus: 'cus'
  southcentralus: 'scus'
  northcentralus: 'ncus'
  canadaeast: 'cae'
  canadacentral: 'cac'
  germanynorth: 'den'
  germanywestcentral: 'dewc'
  westeurope: 'weu'
}

var metricAlertName = '${environment}-${_locationMap[location]}-clsqueuedlq-alert'
var actionGroupName = '${environment}-${_locationMap[location]}-clsqueuedlq-ag'

resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: actionGroupName
  location: 'global'
  tags: tags
  properties: {
    groupShortName: take(actionGroupName, 12)
    enabled: true
    emailReceivers: emailReceivers
  }
}

resource metricAlert 'microsoft.insights/metricAlerts@2018-03-01' = {
  name: metricAlertName
  location: 'global'
  tags: {
    ApplicationName: 'Content Library Service'
    BusinessUnit: 'Platform'
    Environment: environment
    Owner: 'Relias'
  }
  properties: {
    actions: [
      {
        actionGroupId: actionGroup.id
        webHookProperties: {}
      }
    ]
    autoMitigate: true
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          criterionType: 'StaticThresholdCriterion'
          dimensions: [
            {
              name: 'EntityName'
              operator: 'Include'
              values: [
                'compliance.policy.published'
                'compliance.policy.updated'
                'compliance.policy.archived'
                'contentscheduler.assign.content'
                'compliance.policy.attested'
                'compliance.policy.published.sync'
              ]
            }
          ]
          metricName: 'DeadletteredMessages'
          metricNamespace: 'Microsoft.ServiceBus/namespaces'
          name: 'Metric1'
          skipMetricValidation: true
          threshold: 0
          timeAggregation: 'Maximum'
          operator: 'GreaterThan'
          autoMitigate: true
        }
      ]
    }
    description: 'CLS has Dead Letters'
    enabled: true
    scopes: [
        serviceBusId
    ]
    evaluationFrequency: 'PT1M'
    severity: 2
    targetResourceRegion: location
    targetResourceType: 'Microsoft.ServiceBus/namespaces'
    windowSize: 'PT5M'

  }
}

output actionGroupId string = actionGroup.id
