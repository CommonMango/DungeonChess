using UnityEngine;

public sealed class ArrowProjectile : MonoBehaviour
{
    [SerializeField, Min(0f)] private float hitOffset = 0.5f;
    private Unit target;
    private Unit source;
    private float damage;
    private float speed;
    private bool sourceIsAlly;
    private float expiresAt;
    private bool initialized;

    public void Initialize(Unit victim, float attackPower, bool ally, float moveSpeed, Unit attacker = null)
    {
        target = victim;
        source = attacker;
        damage = attackPower;
        sourceIsAlly = ally;
        speed = Mathf.Max(0.1f, moveSpeed);
        expiresAt = Time.time + 5f;
        initialized = true;
        FaceTarget();
    }

    private void FaceTarget()
    {
        if (target == null) return;
        Vector2 direction = target.transform.position - transform.position;
        // 원본 화살의 가로축(+X)을 진행 방향으로 돌립니다.
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    private void Update()
    {
        if (!initialized) return;
        if (target == null || !target.isActiveAndEnabled || target.HP <= 0 ||
            target.unitData.isAlly == sourceIsAlly || Time.time >= expiresAt ||
            PhaseManager.Instance == null || PhaseManager.Instance.CurrentState != PhaseState.Battle)
        {
            Destroy(gameObject);
            return;
        }
        FaceTarget();
        // 적 중심보다 화살이 날아오는 방향으로 조금 앞에서 명중합니다.
        Vector3 toTarget = target.transform.position - transform.position;
        Vector3 destination = target.transform.position - toTarget.normalized * Mathf.Min(hitOffset, toTarget.magnitude);
        float step = speed * Time.deltaTime;
        if (Vector2.Distance(transform.position, destination) <= step)
        {
            // 충돌 이벤트 중복 없이 도착한 화살 하나당 한 번만 피해를 줍니다.
            initialized = false;
            target.TakeDamage(damage, source);
            Destroy(gameObject);
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, destination, step);
    }
}
