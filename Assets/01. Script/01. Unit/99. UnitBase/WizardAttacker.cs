using UnityEngine;

// 기존 원거리 탐색과 이동을 재사용하고 단일 대상 기본 공격만 처리합니다.
public sealed class WizardAttacker : RangedAttacker
{
    private float nextAttackTime;

    public override void BaseAttack()
    {
        if (Time.time < nextAttackTime || !DetectTarget()) return;
        nextAttackTime = Time.time + 1f / Mathf.Max(0.01f, AttackSpeed);
        targetUnit.TakeDamage(Power);
    }
}