using System;
using UnityEngine;

// Runtime data, independent of scenes and MonoBehaviours.
// Future roster, augment and progression data belong here, not in the scene controller.
[Serializable]
public sealed partial class RunState
{
    [SerializeField] private string runId;
    [SerializeField] private int currentWill;
    [SerializeField] private int maxWill;
    public string RunId => runId;
    public int CurrentWill => currentWill;
    public int MaxWill => maxWill;
    public bool IsWillDepleted => currentWill == 0;

    public RunState(int initialWill, int maximumWill)
    {
        if (maximumWill <= 0) throw new ArgumentOutOfRangeException(nameof(maximumWill));
        if (initialWill <= 0 || initialWill > maximumWill) throw new ArgumentOutOfRangeException(nameof(initialWill));
        runId = Guid.NewGuid().ToString("N");
        currentWill = initialWill;
        maxWill = maximumWill;
    }

    // Returns the amount actually consumed; excess damage cannot make will negative.
    public int ReduceWill(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        int applied = Math.Min(amount, currentWill);
        currentWill -= applied;
        return applied;
    }

    // A failed expedition cannot be revived by healing; start a new run instead.
    public int RecoverWill(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (IsWillDepleted) return 0;
        int applied = Math.Min(amount, maxWill - currentWill);
        currentWill += applied;
        return applied;
    }
}
