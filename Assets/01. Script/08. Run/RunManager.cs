using UnityEngine;

// Owns one run across scene loads. This is session memory, not a disk-save system.
public static class RunManager
{
    // Temporary development values, not final balance decisions.
    public const int TestInitialWill = 100;
    public const int TestMaxWill = 100;
    public static RunState Current { get; private set; }
    public static bool HasRun => Current != null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession() => Current = null;

    public static RunState StartNewRun(int initialWill = TestInitialWill, int maximumWill = TestMaxWill)
    {
        // Construct before replacing so invalid settings cannot discard the existing run.
        var next = new RunState(initialWill, maximumWill);
        Current = next;
        return next;
    }

    public static RunState EnsureRun(int initialWill = TestInitialWill, int maximumWill = TestMaxWill)
    {
        return Current ?? StartNewRun(initialWill, maximumWill);
    }
}
