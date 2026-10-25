using UnityEngine;

// 모든 전투 유닛이 공유하는 SP 상태입니다. 스킬 발동과 소비는 아직 연결하지 않습니다.
public abstract partial class Unit
{
    [SerializeField] private int currentSP;
    [SerializeField, Min(1)] private int maxSP = 100;
    public int CurrentSP => currentSP;
    public int MaxSP => Mathf.Max(1, maxSP);

    protected void ResetSP() => currentSP = 0;

    private void GainSP(int amount)
    {
        if (HP <= 0 || !isActiveAndEnabled || PhaseManager.Instance == null ||
            PhaseManager.Instance.CurrentState != PhaseState.Battle) return;
        currentSP = Mathf.Min(MaxSP, currentSP + amount);
    }
}