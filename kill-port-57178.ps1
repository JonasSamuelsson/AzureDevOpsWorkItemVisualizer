[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [int]$Port = 57178
)

$listenerConnections = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
if (-not $listenerConnections) {
    Write-Output "No process is listening on port $Port."
    exit 0
}

$processIds = $listenerConnections | Select-Object -ExpandProperty OwningProcess -Unique

foreach ($processId in $processIds) {
    $process = Get-Process -Id $processId -ErrorAction Stop

    if ($PSCmdlet.ShouldProcess("PID $processId ($($process.ProcessName))", "Stop-Process")) {
        Stop-Process -Id $processId -Force -ErrorAction Stop
        Write-Output "Stopped PID $processId ($($process.ProcessName)) listening on port $Port."
    }
}
