. './Functions/submit-pim.ps1'

$env = (Get-Content -Path './Config/dev1-env.json' | ConvertFrom-Json)
$permissions = (Get-Content -Path './Config/dev1-reader.json' | ConvertFrom-Json).permissions

Submit-PIM -env $env -permissions $permissions
