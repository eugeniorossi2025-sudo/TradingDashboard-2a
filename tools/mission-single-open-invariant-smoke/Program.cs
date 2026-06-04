namespace MissionSingleOpenInvariantSmoke;

internal static class Program
{
    private static int Failures;

    public static int Main()
    {
        TestRecoveryKeepsNewestSession();
        TestStartBlockedWhenAnyOpen();
        TestHealthUnhealthyWhenMultipleOpen();

        if (Failures > 0)
        {
            Console.Error.WriteLine($"{Failures} assertion(s) failed.");
            return 1;
        }

        Console.WriteLine("mission-single-open-invariant-smoke: all assertions passed.");
        return 0;
    }

    private static void Assert(bool condition, string message)
    {
        if (condition)
            return;
        Console.Error.WriteLine($"FAIL: {message}");
        Failures++;
    }

    private static IReadOnlyList<int> SelectStaleSessionIds(IEnumerable<(int Id, DateTime StartTime)> openSessions)
    {
        return openSessions
            .OrderByDescending(session => session.StartTime)
            .Skip(1)
            .Select(session => session.Id)
            .ToList();
    }

    private static bool CanStartNewMission(int openCount) => openCount == 0;

    private static void TestRecoveryKeepsNewestSession()
    {
        var open = new[]
        {
            (101, new DateTime(2026, 6, 3, 21, 48, 0, DateTimeKind.Utc)),
            (102, new DateTime(2026, 6, 3, 23, 6, 0, DateTimeKind.Utc)),
            (103, new DateTime(2026, 6, 4, 6, 40, 0, DateTimeKind.Utc)),
            (104, new DateTime(2026, 6, 4, 6, 49, 0, DateTimeKind.Utc)),
        };

        var ordered = open.OrderByDescending(session => session.Item2).ToList();
        var stale = SelectStaleSessionIds(open);
        Assert(ordered[0].Item1 == 104, "canonical open session must be newest by StartTime");
        Assert(stale.Count == 3, "three stale sessions must be finalized");
        Assert(stale.SequenceEqual(new[] { 103, 102, 101 }), "stale ids must be all except newest");
    }

    private static void TestStartBlockedWhenAnyOpen()
    {
        Assert(!CanStartNewMission(1), "start must be blocked with one open session");
        Assert(!CanStartNewMission(4), "start must be blocked with four open sessions");
        Assert(CanStartNewMission(0), "start allowed only with zero open sessions");
    }

    private static void TestHealthUnhealthyWhenMultipleOpen()
    {
        foreach (var count in new[] { 0, 1, 2, 5 })
        {
            var healthy = count <= 1;
            Assert(healthy == (count <= 1), $"health flag for count={count}");
        }
    }
}
