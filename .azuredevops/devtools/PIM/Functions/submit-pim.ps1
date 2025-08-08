function Submit-PIM {

    param (
        $env,
        $permissions
    )

    Connect-AzAccount -Tenant $env.tenantId -Subscription $env.subscriptionId
    $principal = Get-AzADUser -SignedIn
    Write-Host "Activating roles for $($principal.DisplayName) ($($principal.Mail))"

    foreach($permission in $permissions){
        $guid = New-Guid
        $startTime = Get-Date -Format o
        $scope = "/subscriptions/$($env.subscriptionId)/resourceGroups/$($permission.resourceGroup)/providers/$($permission.resourceType)/$($permission.resourceName)"
        $eligibleRoles = Get-AzRoleEligibilitySchedule -Scope $scope -Filter "asTarget()"
        $role = ($eligibleRoles | Where-Object { $_.RoleDefinitionDisplayName -eq $permission.roleName })[0] 

        if(!$role) {
            Write-Host "Role not found: $($permission.roleName)"
        }
        else{
            Write-Host "Activating $($role.RoleDefinitionDisplayName)..."
            # TODO - find maximum duration automatically
            New-AzRoleAssignmentScheduleRequest -Name $guid -Scope $role.Scope -ExpirationDuration PT4H -ExpirationType AfterDuration -PrincipalId $principal.Id -RequestType SelfActivate -RoleDefinitionId $role.RoledefinitionID -ScheduleInfoStartDateTime $startTime -Justification "access resource"
            Write-Host "Role Activated"
        }
    }
}