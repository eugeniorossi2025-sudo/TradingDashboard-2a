# Code-path verify without SQL: builder must map MissionMarginEuro from session TotalMargin field.
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$builder = Join-Path $repoRoot 'backend\WebApi\Services\Implementations\MissionReportBuilder.cs'
$html = Join-Path $repoRoot 'backend\WebApi\Services\MissionReportHtmlBuilder.cs'
$controller = Join-Path $repoRoot 'backend\WebApi\Controllers\MissionController.cs'

$checks = @(
    @{ name = 'MissionMarginEuro from DB TotalMargin'; pass = (Select-String -Path $builder -Pattern 'MissionMarginEuro = s\.TotalMargin' -Quiet) }
    @{ name = 'PeriodNetPnlEuro separate from mission margin'; pass = (Select-String -Path $builder -Pattern 'session\.PeriodNetPnlEuro = summary' -Quiet) }
    @{ name = 'TotalMarginEuro alias to MissionMarginEuro'; pass = (Select-String -Path $builder -Pattern 'session\.TotalMarginEuro = session\.MissionMarginEuro' -Quiet) }
    @{ name = 'HTML uses MissionMarginEuro'; pass = (Select-String -Path $html -Pattern 'session\.MissionMarginEuro' -Quiet) }
    @{ name = 'HTML debug shows periodNetPnlEuro'; pass = (Select-String -Path $html -Pattern 'periodNetPnlEuro' -Quiet) }
    @{ name = 'Index TotalMarginEuro from session.TotalMargin'; pass = (Select-String -Path $controller -Pattern 'TotalMarginEuro = session\.TotalMargin' -Quiet) }
)

Write-Host '=== Mission margin code verify ==='
$fail = 0
foreach ($c in $checks) {
    $status = if ($c.pass) { 'PASS' } else { 'FAIL'; $fail++ }
    Write-Host "$status $($c.name)"
}
if ($fail -gt 0) { exit 2 }
Write-Host 'ALL PASS'
exit 0
