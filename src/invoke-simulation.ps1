PARAM(
    [Parameter(Mandatory = $true)]
    $StagingStoreRootPath
)

$failureRates = @(0.005, 0.01, 0.05, 0.1) 
$compensationFailureRates = @(0.005, 0.01, 0.05, 0.1) 
pushd
cd "$PSScriptRoot/ConsoleRunner"
& dotnet publish -c Release | out-null
popd
$failureRates | foreach { 
    $f = $_
    $jobs = @()
    Write-Host "Starting new batch, F: $f, CF: $compensationFailureRates"
    $compensationFailureRates | foreach {
        $cf = $_
        $jobs += (
            Start-Job -Name "F $f, CF $CF, CaPM Error Handling Simulation" -ScriptBlock {
                PARAM(
                    $consoleRunnerPath,
                    $storagePath, 
                    $failureRate, 
                    $compensationFailureRate
                )
                cd $consoleRunnerPath
                $ingestCount = 100000
                $numberOfComponentsPerIngest = 5
                Write-Host "Running:"
                Write-Host "& ./consolerunner.exe --mode simulation --failure-rate $failureRate --compensation-failure-rate $compensationFailureRate --ingest-count $ingestCount --number-of-components-per-ingest $numberOfComponentsPerIngest --storage-path '$storagePath\f-$failureRate-cf-$compensationFailureRate.txt'"
                & ./consolerunner.exe --mode simulation --failure-rate $failureRate --compensation-failure-rate $compensationFailureRate --ingest-count $ingestCount --number-of-components-per-ingest $numberOfComponentsPerIngest --storage-path "$storagePath\f-$failureRate-cf-$compensationFailureRate.txt"
            } -ArgumentList "$PSScriptRoot/ConsoleRunner/bin/Release/netcoreapp3.1/publish", $StagingStoreRootPath, $f, $cf
        )
    }
    while($jobs | get-job | where {$_.state -ne 'Completed'})
    {
        Start-Sleep -seconds 1
        $currentStatus = $jobs | get-job 
        $currentStatus | foreach {
            $_.ChildJobs[0].Information | select -last 1 | Write-Host
            $_.ChildJobs[0].Information.Clear()
        }
    }
    $jobs | remove-job
}