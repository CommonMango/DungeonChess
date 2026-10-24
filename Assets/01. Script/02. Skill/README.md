# 코드 스킬 등록 구조

Skill(SO)은 이름·설명·수치 설정, SkillBase는 상속할 구현 클래스, SkillManager는 구현 ID별 생성 함수를 보관합니다.
SkillCatalog는 데이터 SO 목록이며 SkillManager의 코드 구현 목록과 별개입니다.

## 새로운 구현 추가

아래는 새로운 스킬의 등록 방법을 보여주는 예시입니다.

```csharp
using UnityEngine;

public sealed class MySkill : SkillBase
{
    public const string Id = "my_skill";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSelf()
    {
        SkillManager.Register<MySkill>(Id);
    }

    public override void Execute(Unit caster, Unit target)
    {
        // 나중에 이곳에 실제 효과를 구현합니다.
        // 설정값은 Data.skillDamage, Data.cooldown 등으로 읽습니다.
    }
}
```

1. SkillBase를 상속하고 Execute를 구현합니다.
2. 위 RegisterSelf 메서드를 넣습니다. 상속만으로는 등록되지 않습니다.
3. 스킬 SO의 implementationId(관리 창의 코드 구현 ID)를 my_skill로 지정합니다.
4. SO를 유닛의 useSkill 목록에 연결합니다.

실행 시 Unity가 각 클래스의 RegisterSelf를 호출합니다. 매니저 코드를 수정하거나 씬에 매니저 오브젝트를 배치할 필요가 없습니다.
씬의 Awake 이후 등록된 구현을 조회할 수 있습니다. 다른 BeforeSceneLoad 콜백끼리의 실행 순서는 보장하지 않으므로 그 단계에서 조회하지 마세요.

```csharp
foreach (string id in SkillManager.RegisteredIds)
    Debug.Log(id);

if (SkillManager.TryCreate(skillSO, out SkillBase instance))
{
    // 유닛별로 인스턴스를 보관하고 필요한 시점에 호출합니다.
    // instance.Execute(caster, target);
}
```

매번 새 객체를 생성하므로 쿨타임 등 실행 상태를 파생 클래스 필드에 두어도 유닛끼리 공유되지 않습니다.
SO의 skillId는 데이터 고유 ID이고 implementationId는 구현 종류 ID입니다. 수치가 다른 여러 SO가 동일한 implementationId를 사용할 수 있습니다.
동일한 구현 ID를 두 클래스가 등록하면 예외로 알립니다. 구현이 없거나 SO가 비어 있으면 TryCreate는 false를 반환합니다.
Domain Reload를 꺼도 Play 시작마다 목록을 초기화한 뒤 다시 등록합니다.

현재 double_slash(두번 베기)가 등록됩니다. DoubleSlash.asset의 공격력 배율은 0.8, 타격 간격은 0.15초입니다. 시전 순간의 현재 공격력을 기준으로 같은 적에게 두 번 피해를 주며, 전투 종료·시전자 또는 대상 사망 시 후속 타격을 중단합니다. 쿨타임 관리, 자동 발동, 스킬 사용 UI, 기존 전투의 SkillAttack 연결은 아직 구현하지 않았습니다.
