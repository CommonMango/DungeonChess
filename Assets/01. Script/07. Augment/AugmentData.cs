using System;
using UnityEngine;

public enum AugmentTarget { AllAllies, UnitType, Tag, SelectedUnit }
public enum AugmentStat { AttackPower, AttackSpeed, MoveSpeed, SkillHitCount }
public enum AugmentOperation { Add, Percent }

[CreateAssetMenu(fileName = "NewAugment", menuName = "Dungeon Chess/Augments/Augment")]
public class AugmentData : ScriptableObject
{
    public string augmentId;
    public string displayName;
    [TextArea(3, 8)] public string description;
    public Sprite icon;
    [Header("적용 대상")]
    public AugmentTarget targetScope;
    [Tooltip("UnitType이면 필수. SelectedUnit이면 선택 가능한 유닛 종류를 제한합니다.")]
    public UnitData targetUnit;
    [Tooltip("Tag이면 필수. SelectedUnit이면 선택 가능한 태그를 제한합니다.")]
    public string targetTag;
    [Header("효과")]
    public AugmentStat stat;
    public AugmentOperation operation;
    [Tooltip("Percent는 0.2 = 20%. 타격 횟수는 Add와 정수만 사용합니다.")]
    [Min(0)] public float value;
    [Tooltip("타격 횟수 효과의 대상 스킬 구현 ID. 비어 있으면 모든 스킬.")]
    public string skillImplementationId;
    [Min(1)] public int maxStacks = 1;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(augmentId) || string.IsNullOrWhiteSpace(displayName)) throw new InvalidOperationException("증강 ID와 이름을 입력하세요.");
        if (maxStacks < 1 || float.IsNaN(value) || float.IsInfinity(value) || value < 0) throw new InvalidOperationException("증강 수치 및 최대 중첩을 확인하세요.");
        if (targetScope == AugmentTarget.UnitType && targetUnit == null) throw new InvalidOperationException("대상 유닛 SO를 지정하세요.");
        if (targetScope == AugmentTarget.Tag && string.IsNullOrWhiteSpace(targetTag)) throw new InvalidOperationException("대상 태그를 입력하세요.");
        if (stat == AugmentStat.SkillHitCount && (operation != AugmentOperation.Add || value != Mathf.Floor(value))) throw new InvalidOperationException("타격 횟수는 Add 방식의 정수여야 합니다.");
    }
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(augmentId)) augmentId = Guid.NewGuid().ToString("N");
    }
}
