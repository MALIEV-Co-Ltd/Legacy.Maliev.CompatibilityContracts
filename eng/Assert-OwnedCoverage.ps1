param(
    [Parameter(Mandatory = $true)]
    [string] $CoveragePath,
    [decimal] $MinimumPercent = 80
)

$ErrorActionPreference = 'Stop'
[xml] $settings = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../coverage.runsettings') -Raw
$configuration = $settings.RunSettings.DataCollectionRunSettings.DataCollectors.DataCollector.Configuration
foreach ($name in @('Exclude', 'ExcludeByAttribute', 'ExcludeByFile')) {
    if (-not [string]::IsNullOrWhiteSpace([string] $configuration.$name)) {
        throw "Raw owned assembly coverage cannot use $name."
    }
}
if ([string] $configuration.SkipAutoProps -ne 'false') {
    throw 'Raw owned assembly coverage must include automatic property accessors.'
}
[xml] $coverage = Get-Content -LiteralPath $CoveragePath -Raw
$packages = @($coverage.coverage.packages.package | Where-Object name -eq 'Legacy.Maliev.CompatibilityContracts')
if ($packages.Count -ne 1) {
    throw 'Expected exactly one owned production assembly in the coverage report.'
}
$lines = @{}
foreach ($class in $packages[0].classes.class) {
    foreach ($line in $class.lines.line) {
        $key = "$($class.filename):$($line.number)"
        $lines[$key] = ([long] $line.hits -gt 0) -or ($lines[$key] -eq $true)
    }
}
if ($lines.Count -eq 0) {
    throw 'Owned assembly coverage is unavailable; there are no measured production lines.'
}
$covered = @($lines.Values | Where-Object { $_ -eq $true }).Count
$percent = [decimal] 100 * $covered / $lines.Count
Write-Output ('Owned raw line coverage: {0}/{1} ({2:N2}%)' -f $covered, $lines.Count, $percent)
if ($percent -lt $MinimumPercent) {
    throw "Owned raw assembly coverage is below $MinimumPercent%."
}
