using UnityEngine;
using UnityEngine.SceneManagement;

// 기존 근접 유닛의 등록·이동·피격·사망 흐름을 재사용하고 탐지와 기본 공격만 교체합니다.
public class RangedAttacker : MeleeAttacker
{
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField, Min(0.1f)] private float arrowSpeed = 20f;
    private float nextShotTime;

    public override bool DetectTarget()
    {
        targetUnit = null;
        isTargetInInteractRange = false;
        if (isActiveAndEnabled && HP > 0 && isBattlePhaseStart &&
            PhaseManager.Instance != null && PhaseManager.Instance.CurrentState == PhaseState.Battle)
        {
            float range = TileManager.Instance.TileGap * AttackRange;
            float nearest = float.PositiveInfinity;
            // 중심 간 거리로 사거리를 판정하여 큰 콜라이더 때문에 사거리가 늘어나지 않게 합니다.
            foreach (var hit in Physics2D.OverlapCircleAll(transform.position, range, unitData.targetLayer))
            {
                var candidate = hit.GetComponentInParent<Unit>();
                if (candidate == null || candidate == this || !candidate.isActiveAndEnabled ||
                    candidate.HP <= 0 || candidate.unitData.isAlly == unitData.isAlly) continue;
                float distance = ((Vector2)(candidate.transform.position - transform.position)).sqrMagnitude;
                if (distance > range * range || distance >= nearest) continue;
                nearest = distance;
                targetUnit = candidate;
            }
            if (targetUnit != null)
            {
                targetPosition = targetUnit.transform.position;
                targetIndex = targetUnit.NodeIndex;
                isTargetInInteractRange = true;
            }
        }
        BTree.SetVariableValue<bool>("IsTargetDetected", isTargetInInteractRange);
        return isTargetInInteractRange;
    }

    public override void BaseAttack()
    {
        if (Time.time < nextShotTime || !DetectTarget() || arrowPrefab == null) return;
        nextShotTime = Time.time + 1f / Mathf.Max(0.01f, AttackSpeed);
        var arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
        // 화살을 발사한 전투 씬에 두어 전투 종료 시 함께 정리합니다.
        SceneManager.MoveGameObjectToScene(arrow, gameObject.scene);
        var projectile = arrow.GetComponent<ArrowProjectile>();
        if (projectile == null) projectile = arrow.AddComponent<ArrowProjectile>();
        projectile.Initialize(targetUnit, Power, unitData.isAlly, arrowSpeed);
    }

    public override void SkillAttack() { }
}
