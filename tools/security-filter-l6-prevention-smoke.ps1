$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$tempRoot = Join-Path ([IO.Path]::GetTempPath()) ("dash2a-security-filter-smoke-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tempRoot | Out-Null

try {
    $projectPath = Join-Path $tempRoot 'Smoke.csproj'
    $engineProject = Join-Path $repoRoot 'decision-engine\Decisore\Decisore.csproj'

    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="$engineProject" />
  </ItemGroup>
</Project>
"@ | Set-Content -LiteralPath $projectPath -Encoding UTF8

    @'
using Decisore.Engine;

static void Assert(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

var engine = new ProactiveEngine
{
    INITIAL_L6_AUTH = 1,
    SECURITY_FILTER_ENABLED = true,
    SECURITY_FILTER_MIN_SCORE = 3,
    SECURITY_FILTER_MIN_STREAK = 1,
    SECURITY_FILTER_MAX_SHOE_HAND = 20,
    SECURITY_FILTER_MAX_AVG_SECONDS = 999,
    SECURITY_FILTER_VERY_FAST_SECONDS = 999,
    SECURITY_FILTER_DELTA_WINDOW = 8
};

// Seed one real P/B hand so the next L5 loss has a measurable hand delta.
engine.FeedAndDecide(
    computer: "SMOKE",
    tableId: 1,
    handIndexMazzo: 16,
    margine: 0,
    esito: 'P',
    coloreGiocato: 'P',
    valoreGiocato: 1,
    martingalaCounter: 1,
    stato: "Sculping",
    elapsedMinutes: 1);

Thread.Sleep(20);

var advice = engine.FeedAndDecide(
    computer: "SMOKE",
    tableId: 1,
    handIndexMazzo: 17,
    margine: -35,
    esito: 'P',
    coloreGiocato: 'B',
    valoreGiocato: 35,
    martingalaCounter: 5,
    stato: "Sculping",
    elapsedMinutes: 1);

var telemetry = engine.getTelemetry();
var bot = telemetry.SecurityFilterByBot["SMOKE"];

Assert(advice.SecurityFilterActive, "Security filter should be active for the smoke scenario.");
Assert(advice.ActionCode == 3, $"Expected ActionCode 3, got {advice.ActionCode}.");
Assert(advice.GlobalAuthL6Counter == 1, $"L6 credit was consumed; expected 1, got {advice.GlobalAuthL6Counter}.");
Assert(telemetry.TotalAuthL6Authorized == 0, $"L6 auth was registered; expected 0, got {telemetry.TotalAuthL6Authorized}.");
Assert(telemetry.TotalSecurityFilterPreventedL6 == 1, $"PreventedL6 total mismatch; expected 1, got {telemetry.TotalSecurityFilterPreventedL6}.");
Assert(bot.PreventedL6 == 1, $"Bot PreventedL6 mismatch; expected 1, got {bot.PreventedL6}.");
Assert(bot.LastL6AuthorizationPBHandsPlayed == 0, "L6 authorization marker should not be written when Security Filter blocks.");

Console.WriteLine("SECURITY_FILTER_L6_PREVENTION_SMOKE=OK");
Console.WriteLine($"ActionCode={advice.ActionCode}");
Console.WriteLine($"SecurityRiskScore={advice.SecurityRiskScore}");
Console.WriteLine($"GlobalAuthL6Counter={advice.GlobalAuthL6Counter}");
Console.WriteLine($"TotalAuthL6Authorized={telemetry.TotalAuthL6Authorized}");
Console.WriteLine($"TotalSecurityFilterPreventedL6={telemetry.TotalSecurityFilterPreventedL6}");
'@ | Set-Content -LiteralPath (Join-Path $tempRoot 'Program.cs') -Encoding UTF8

    dotnet run --project $projectPath
}
finally {
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force
    }
}
