using System.Collections;
using UnityEngine;

public sealed class DoubleSlashSkill : SkillBase
{
    public const string Id = "double_slash";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSelf() => SkillManager.Register<DoubleSlashSkill>(Id);

    public override void Execute(Unit caster, Unit target)
    {
        if (!(Data is DoubleSlashData settings))
        {
            Debug.LogError("두번 베기는 DoubleSlashData SO가 필요합니다.");
            return;
        }
        if (!CanHit(caster, target)) return;
        // Both hits use the caster's current attack power at the start of the skill.
        float damage = Mathf.Max(0, caster.Power * settings.attackMultiplier);
        int hits = AugmentManager.GetHitCount(caster, Id, 2);
        caster.StartCoroutine(SlashTwice(caster, target, damage, Mathf.Max(0.01f, settings.hitInterval), hits));
    }

    private static IEnumerator SlashTwice(Unit caster, Unit target, float damage, float interval, int hits)
    {
        for (int i = 0; i < hits; i++)
        {
            if (!CanHit(caster, target)) yield break;
            target.TakeDamage(damage);
            if (i + 1 < hits) yield return new WaitForSeconds(interval);
        }
    }

    private static bool CanHit(Unit caster, Unit target)
    {
        return caster != null && target != null && caster != target &&
            caster.isActiveAndEnabled && target.isActiveAndEnabled &&
            caster.HP > 0 && target.HP > 0 &&
            caster.unitData != null && target.unitData != null &&
            caster.unitData.isAlly != target.unitData.isAlly &&
            PhaseManager.Instance != null && PhaseManager.Instance.CurrentState == PhaseState.Battle;
    }
}
