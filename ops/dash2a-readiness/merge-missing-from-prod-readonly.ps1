# READ from remote prod DB, WRITE missing rows to local DB only. No prod writes.
param(
    [string]$LocalServer = '(localdb)\MSSQLLocalDB',
    [string]$LocalDb = 'Dash2A_LocalProdLike',
    [string]$AppsettingsPath = 'backend/WebApi/appsettings.json'
)

$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Set-Location $Root

Add-Type -AssemblyName 'System.Data'

$configKeys = @('DECISION_METHOD', 'STOP_WIN', 'STOP_TIME', 'STOP_LOSS', 'RUNTIME_MODE', 'BASE_UNIT')
$tables = @('Configurations', 'UserNotificationSettings', 'Pc_CurrentStatus', 'MissionSessions', 'MissionMarginSamples')

function Get-RemoteConnectionString {
    $json = Get-Content (Join-Path $Root $AppsettingsPath) -Raw | ConvertFrom-Json
    return $json.ConnectionStrings.DefaultConnection
}

function Get-LocalConnectionString {
    "Server=$LocalServer;Database=$LocalDb;Trusted_Connection=True;TrustServerCertificate=True;"
}

function Open-Conn([string]$Cs, [switch]$ReadOnly) {
    $c = New-Object System.Data.SqlClient.SqlConnection $Cs
    if ($ReadOnly) { $c.ConnectionString = $Cs + 'ApplicationIntent=ReadOnly;' }
    $c.Open()
    return $c
}

function Get-Scalar([System.Data.SqlClient.SqlConnection]$Conn, [string]$Sql) {
    $cmd = $Conn.CreateCommand()
    $cmd.CommandText = $Sql
    return [int]$cmd.ExecuteScalar()
}

function Get-ConfigRow([System.Data.SqlClient.SqlConnection]$Conn, [string]$Key) {
    $cmd = $Conn.CreateCommand()
    $cmd.CommandText = 'SELECT [K], [Description], [Pos], [Value] FROM dbo.Configurations WHERE [K] = @k'
    [void]$cmd.Parameters.AddWithValue('@k', $Key)
    $r = $cmd.ExecuteReader()
    if (-not $r.Read()) { $r.Close(); return $null }
    $row = @{
        Key = [string]$r['K']
        Description = if ($r['Description'] -is [DBNull]) { $null } else { [string]$r['Description'] }
        Pos = if ($r['Pos'] -is [DBNull]) { $null } else { [int]$r['Pos'] }
        Value = if ($r['Value'] -is [DBNull]) { $null } else { [string]$r['Value'] }
    }
    $r.Close()
    return $row
}

function Insert-Config([System.Data.SqlClient.SqlConnection]$Conn, $Row) {
    $cmd = $Conn.CreateCommand()
    $cmd.CommandText = @'
INSERT INTO dbo.Configurations ([K], [Description], [Pos], [Value])
VALUES (@k, @d, @p, @v);
'@
    [void]$cmd.Parameters.AddWithValue('@k', $Row.Key)
    if ($null -eq $Row.Description) { [void]$cmd.Parameters.AddWithValue('@d', [DBNull]::Value) } else { [void]$cmd.Parameters.AddWithValue('@d', $Row.Description) }
    if ($null -eq $Row.Pos) { [void]$cmd.Parameters.AddWithValue('@p', [DBNull]::Value) } else { [void]$cmd.Parameters.AddWithValue('@p', $Row.Pos) }
    if ($null -eq $Row.Value) { [void]$cmd.Parameters.AddWithValue('@v', [DBNull]::Value) } else { [void]$cmd.Parameters.AddWithValue('@v', $Row.Value) }
    [void]$cmd.ExecuteNonQuery()
}

function Get-LocalUsers([System.Data.SqlClient.SqlConnection]$Conn) {
    $cmd = $Conn.CreateCommand()
    $cmd.CommandText = 'SELECT Id, ISNULL(UserName,''''), ISNULL(Email,'''') FROM dbo.Users_v2 ORDER BY Id'
    $r = $cmd.ExecuteReader()
    $rows = @()
    while ($r.Read()) {
        $rows += @{
            Id = [int]$r[0]
            UserName = [string]$r[1]
            Email = [string]$r[2]
        }
    }
    $r.Close()
    return $rows
}

function Has-NotificationSetting([System.Data.SqlClient.SqlConnection]$Conn, [int]$UserId) {
    $cmd = $Conn.CreateCommand()
    $cmd.CommandText = 'SELECT COUNT(*) FROM dbo.UserNotificationSettings WHERE UserId = @u'
    [void]$cmd.Parameters.AddWithValue('@u', $UserId)
    return ([int]$cmd.ExecuteScalar()) -gt 0
}

function Insert-NotificationSetting([System.Data.SqlClient.SqlConnection]$Conn, [int]$UserId, [string]$Email) {
    $cmd = $Conn.CreateCommand()
    $cmd.CommandText = @'
INSERT INTO dbo.UserNotificationSettings (UserId, NotificationEmail, Enabled, Mission, [System], Errors, CreatedAtUtc, UpdatedAtUtc)
VALUES (@u, @e, 1, 1, 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
'@
    [void]$cmd.Parameters.AddWithValue('@u', $UserId)
    [void]$cmd.Parameters.AddWithValue('@e', $Email)
    [void]$cmd.ExecuteNonQuery()
}

$remoteCs = Get-RemoteConnectionString
$localCs = Get-LocalConnectionString
$b = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $remoteCs
Write-Host "Remote source: $($b.DataSource) / $($b.InitialCatalog) (read-only)"
Write-Host "Local target: $LocalServer / $LocalDb"

$backupDir = Join-Path $PSScriptRoot 'backups'
New-Item -ItemType Directory -Force -Path $backupDir | Out-Null
$backupFile = Join-Path $backupDir ("Dash2A_LocalProdLike_pre_merge_{0:yyyyMMdd_HHmmss}.bak" -f (Get-Date))
Write-Host "Backup locale -> $backupFile"
$backupConn = Open-Conn $localCs
$backupCmd = $backupConn.CreateCommand()
$backupCmd.CommandText = "BACKUP DATABASE [$LocalDb] TO DISK = @path WITH INIT, STATS = 5"
[void]$backupCmd.Parameters.AddWithValue('@path', $backupFile)
[void]$backupCmd.ExecuteNonQuery()
$backupConn.Close()

$before = @{}
$remoteConn = Open-Conn $remoteCs -ReadOnly
$localConn = Open-Conn $localCs

Write-Host "`n=== CONTEGGI PRIMA ==="
foreach ($t in $tables) {
    $before["local_$t"] = Get-Scalar $localConn "SELECT COUNT(*) FROM dbo.[$t]"
    $before["remote_$t"] = Get-Scalar $remoteConn "SELECT COUNT(*) FROM dbo.[$t]"
    Write-Host ("{0,-28} local={1,6}  remote={2,6}" -f $t, $before["local_$t"], $before["remote_$t"])
}

$importedKeys = @()
$skippedKeys = @()
$diffKeys = @()

foreach ($key in $configKeys) {
    $localRow = Get-ConfigRow $localConn $key
    $remoteRow = Get-ConfigRow $remoteConn $key

    if (-not $remoteRow) {
        $skippedKeys += @{ Key = $key; Reason = 'assente su server' }
        continue
    }
    if ($localRow) {
        if ($localRow.Value -ne $remoteRow.Value) {
            $diffKeys += @{ Key = $key; Local = $localRow.Value; Remote = $remoteRow.Value }
        }
        $skippedKeys += @{ Key = $key; Reason = 'gia presente in locale' }
        continue
    }

    Insert-Config $localConn $remoteRow
    $importedKeys += $key
}

$notifImported = 0
$notifSkipped = 0
foreach ($user in (Get-LocalUsers $localConn)) {
    if (Has-NotificationSetting $localConn $user.Id) {
        $notifSkipped++
        continue
    }
    $safeEmail = if ($user.Email -match '@botdashboard\.local$|@dash2a\.local$') { $user.Email } else { "$($user.UserName)@dash2a.local" }
    Insert-NotificationSetting $localConn $user.Id $safeEmail
    $notifImported++
}

$remoteConn.Close()

Write-Host "`n=== MERGE APPLICATO ==="
Write-Host ("Configurations importate: {0}" -f ($(if ($importedKeys.Count) { $importedKeys -join ', ' } else { '(nessuna)' })))
foreach ($s in $skippedKeys) { Write-Host "  skip $($s.Key): $($s.Reason)" }
foreach ($d in $diffKeys) { Write-Host "  diff $($d.Key): locale='$($d.Local)' server='$($d.Remote)' (non sovrascritto)" }
Write-Host "UserNotificationSettings inseriti: $notifImported, saltati: $notifSkipped"

Write-Host "`n=== CONTEGGI DOPO ==="
$after = @{}
foreach ($t in $tables) {
    $after["local_$t"] = Get-Scalar $localConn "SELECT COUNT(*) FROM dbo.[$t]"
    Write-Host ("{0,-28} local={1,6}  remote={2,6}" -f $t, $after["local_$t"], $before["remote_$t"])
}
$localConn.Close()

$report = [ordered]@{
    backupFile = $backupFile
    before = $before
    after = $after
    importedConfigurationKeys = $importedKeys
    skippedConfigurationKeys = $skippedKeys
    configurationDiffs = $diffKeys
    notificationSettingsInserted = $notifImported
    notificationSettingsSkipped = $notifSkipped
    pcCurrentStatusRecommendation = if ($before['remote_Pc_CurrentStatus'] -gt 0 -and $before['local_Pc_CurrentStatus'] -eq 0) {
        'Snapshot opzionale se serve dashboard demo; default lasciare vuota.'
    } elseif ($before['local_Pc_CurrentStatus'] -eq 0 -and $before['remote_Pc_CurrentStatus'] -eq 0) {
        'Entrambi vuoti - nessun import necessario.'
    } else {
        'Nessun import Pc_CurrentStatus eseguito (solo conteggi).'
    }
    missionRecommendation = "MissionSessions local=$($before['local_MissionSessions']) remote=$($before['remote_MissionSessions']); MissionMarginSamples local=$($before['local_MissionMarginSamples']) remote=$($before['remote_MissionMarginSamples']); locale gia piu ricco - non sovrascritto."
}
$reportPath = Join-Path $PSScriptRoot 'merge-missing-report.json'
($report | ConvertTo-Json -Depth 6) | Set-Content $reportPath -Encoding UTF8
Write-Host "`nReport: $reportPath"
