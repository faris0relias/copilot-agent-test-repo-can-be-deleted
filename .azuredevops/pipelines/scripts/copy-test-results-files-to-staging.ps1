param (
  [Parameter(Mandatory = $true)]
  [string]
  $TestProject
)

function Test-IsGuid
{
    [OutputType([bool])]
    param
    (
        [Parameter(Mandatory = $true)]
        [string]$StringGuid
    )
 
   $ObjectGuid = [System.Guid]::empty
   return [System.Guid]::TryParse($StringGuid,[System.Management.Automation.PSReference]$ObjectGuid) # Returns True if successfully parsed
}

$resultsStagingPath = "$($Env:SYSTEM_ARTIFACTSDIRECTORY)/test-result-files"
Write-Output "`n resultsStagingPath: $resultsStagingPath"

if (!(Test-Path $resultsStagingPath)) {
    Write-Output "`nCreating test result files staging directory..."
    mkdir $resultsStagingPath > $null
}

# Copy the .trx files to the artifact staging folder
Write-Output "Searching for .trx files..."
$trxFiles = gci "$($Env:AGENT_TEMPDIRECTORY)" -recurse "*.trx"

Write-Output "`nCopying discovered test report files:"
foreach ($fileToStage in $trxFiles) {
    Write-Output "  $($fileToStage.FullName)"
    $destinationName = '{0}_{1}' -f "$TestProject", "$($fileToStage.Name)"
    Copy-Item $fileToStage.FullName $resultsStagingPath/$destinationName
}

# Copy the coverage report (opencover) files to the artifact staging folder
Write-Output "Searching for opencover.xml files..."
$coverageFiles = (gci $Env:AGENT_TEMPDIRECTORY -recurse "*opencover.xml")

Write-Output "`n Copying discovered coverage report (opencover) files:"
foreach ($fileToStage in $coverageFiles) {
    $guidDirectoryName = $fileToStage.Directory.Name
    if (Test-IsGuid $guidDirectoryName) {
        Write-Output "  $($fileToStage.FullName)"
        $destinationName = '{0}_{1}_{2}' -f "$TestProject", "$guidDirectoryName", "$($fileToStage.Name)"
        Copy-Item $fileToStage.FullName $resultsStagingPath/$destinationName
    }
}

# Copy the coverage report (cobertura) files to the artifact staging folder
Write-Output "Searching for *.cobertura.xml files..."
$coverageFiles = (gci $Env:AGENT_TEMPDIRECTORY -recurse "*cobertura.xml")

Write-Output "`n Copying discovered coverage report (cobertura) files:"
foreach ($fileToStage in $coverageFiles) {
    $guidDirectoryName = $fileToStage.Directory.Name
    if (Test-IsGuid $guidDirectoryName) {
        Write-Output "  $($fileToStage.FullName)"
        $destinationName = '{0}_{1}_{2}' -f "$TestProject", "$guidDirectoryName", "$($fileToStage.Name)"
        Copy-Item $fileToStage.FullName $resultsStagingPath/$destinationName
    }
}

Write-Output "`nStaged Test Result Files:"
$stagedFiles = gci $resultsStagingPath
foreach ($stagedFile in $stagedFiles) {
    Write-Output "  $($stagedFile.FullName)"
}