using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class AcquiredAugment
{
    public AugmentData Data { get; }
    public Unit SelectedUnit { get; }
    public int Stacks { get; internal set; }
    internal AcquiredAugment(AugmentData data, Unit selected) { Data = data; SelectedUnit = selected; Stacks = 1; }
}

public static class AugmentManager
{
    private static readonly List<AcquiredAugment> acquired = new List<AcquiredAugment>();
    public static IReadOnlyList<AcquiredAugment> Acquired => acquired.AsReadOnly();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => ClearRun();
    public static void ClearRun() => acquired.Clear();
    public static bool Remove(AcquiredAugment entry) => acquired.Remove(entry);

    public static bool TryAcquire(AugmentData data, Unit selected, out AcquiredAugment entry, out string error)
    {
        entry = null; error = null;
        if (data == null) { error = "증강 SO가 없습니다."; return false; }
        try { data.Validate(); } catch (Exception ex) { error = ex.Message; return false; }
        if (data.targetScope == AugmentTarget.SelectedUnit &&
            (selected == null || !selected.isActiveAndEnabled || selected.HP <= 0 || !MatchesFilters(data, selected)))
        { error = "적용 가능한 아군 유닛을 선택하세요."; return false; }
        if (data.targetScope != AugmentTarget.SelectedUnit) selected = null;
        // ReferenceEquals prevents destroyed Unity objects comparing equal to an unselected entry.
        entry = acquired.Find(a => a.Data != null && a.Data.augmentId == data.augmentId && ReferenceEquals(a.SelectedUnit, selected));
        if (entry != null)
        {
            if (entry.Data != data) { entry = null; error = "다른 SO에 동일한 증강 ID가 사용됐습니다."; return false; }
            if (entry.Stacks >= data.maxStacks) { entry = null; error = "최대 중첩에 도달했습니다."; return false; }
            entry.Stacks++; return true;
        }
        entry = new AcquiredAugment(data, selected); acquired.Add(entry); return true;
    }

    private static bool MatchesFilters(AugmentData data, Unit unit)
    {
        if (unit == null || unit.unitData == null || !unit.unitData.isAlly) return false;
        if ((data.targetScope == AugmentTarget.UnitType || data.targetScope == AugmentTarget.SelectedUnit) &&
            data.targetUnit != null && unit.unitData != data.targetUnit) return false;
        if (data.targetScope == AugmentTarget.Tag || (data.targetScope == AugmentTarget.SelectedUnit && !string.IsNullOrWhiteSpace(data.targetTag)))
            return unit.unitData.augmentTags != null && Array.IndexOf(unit.unitData.augmentTags, data.targetTag) >= 0;
        return true;
    }
    public static bool AppliesTo(AcquiredAugment entry, Unit unit)
    {
        if (entry == null || entry.Data == null || !MatchesFilters(entry.Data, unit)) return false;
        return entry.Data.targetScope != AugmentTarget.SelectedUnit ||
            (entry.SelectedUnit != null && entry.SelectedUnit == unit);
    }
    public static float Evaluate(Unit unit, AugmentStat stat, float baseValue)
    {
        float add = 0, percent = 0;
        foreach (var entry in acquired)
        {
            if (!AppliesTo(entry, unit) || entry.Data.stat != stat) continue;
            float amount = entry.Data.value * entry.Stacks;
            if (entry.Data.operation == AugmentOperation.Percent) percent += amount; else add += amount;
        }
        return Mathf.Max(0, (baseValue + add) * (1 + percent));
    }
    public static int GetHitCount(Unit unit, string implementationId, int baseCount)
    {
        int extra = 0;
        foreach (var entry in acquired)
        {
            if (!AppliesTo(entry, unit) || entry.Data.stat != AugmentStat.SkillHitCount) continue;
            string filter = entry.Data.skillImplementationId;
            if (!string.IsNullOrEmpty(filter) && filter != implementationId) continue;
            extra += Mathf.Max(0, Mathf.FloorToInt(entry.Data.value)) * entry.Stacks;
        }
        return Mathf.Max(1, baseCount + extra);
    }
}
