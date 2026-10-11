# 증강 시스템

## 편집

Dungeon Chess > 스킬 · 증강 관리의 증강 탭을 사용합니다.
SO 파일은 Assets/02. Resources/08. Augments에 있습니다.
카탈로그 등록은 설계 데이터 등록이며 실제 획득/적용과 다릅니다. 예시 SO는 자동 지급되지 않습니다.

- AllAllies: 현재와 이후 생성되는 모든 아군.
- UnitType: targetUnit과 같은 UnitData SO를 사용하는 모든 아군.
- Tag: 유닛 SO의 augmentTags에 targetTag가 있는 아군. 대소문자 구분.
- SelectedUnit: 획득 시 전달한 한 개체. targetUnit/targetTag를 지정하면 선택 가능한 종류도 제한.
- AttackPower / AttackSpeed / MoveSpeed: 공격력 / 초당 공격 횟수 / 이동 속도.
- SkillHitCount: 스킬 타격 횟수 증가. Add와 정수만 사용. skillImplementationId를 비우면 전체 스킬, double_slash면 두번 베기만.
- Add: 고정값 합산. Percent: 0.2는 20% 증가. 계산은 (기본값 + 고정값 합계) × (1 + 비율 합계).
- maxStacks: 동일 증강 최대 획득 횟수. 선택 개체 증강은 각 유닛별로 계산.

예시: KnightAttack은 기사 SO를 쓰는 아군 전체 공격력 +20%. SelectedExtraSlash는 선택한 한 명의 두번 베기 +1타.
현재 최대 체력 변경, 조건부 발동, 별도 증강 효과 상속/등록 체계는 포함하지 않았습니다.

## 획득과 해제

```csharp
// 전체 / 종류 / 태그 대상은 selectedUnit에 null 전달.
if (AugmentManager.TryAcquire(augmentSO, selectedUnit, out var acquired, out var error))
{
    // 획득 성공. Unit의 능력치 및 두번 베기에 즉시 반영됩니다.
}
else
{
    Debug.LogWarning(error);
}

AugmentManager.Remove(acquired); // 해당 획득 항목의 모든 중첩 해제
var owned = AugmentManager.Acquired; // 획득 목록 조회
AugmentManager.ClearRun(); // 새로운 도전
```

원본 UnitData/Skill SO는 수정하지 않습니다. 능력치 getter와 스킬 실행 시 획득 목록을 계산하므로 새 유닛도 종류/태그 증강을 적용받습니다.
추가하는 다단 타격 스킬도 실행 시 AugmentManager.GetHitCount(caster, implementationId, 기본횟수)를 사용해야 추가 타격이 반영됩니다.

현재 획득 정보는 메모리에만 존재합니다. 노드맵 전투 왕복 시 전체/종류/태그 증강은 유지되고, 새 도전과 Play 시작 시 초기화됩니다.
SelectedUnit은 현재 실제 개체에 연결됩니다. 현재 전투 시스템은 전투마다 새 유닛을 만들기 때문에 이전 개체가 사라지면 새 개체에 자동 이전하지 않습니다. 이후 파티 보존 시스템을 만들 때 파티 개체 고유 ID로 연결해야 합니다.

보상 선택 화면과 CSV 연동은 아직 연결하지 않았습니다. 향후 보상 선택 완료 시 TryAcquire를 호출하면 됩니다.

## 검증

전체 아군 / 특정 종류 / 태그 / 선택 개체 필터, 적군 선택 거부, 중첩 제한 및 해제, 원본 SO 불변을 확인했습니다.
같은 기사 두 명 중 한 명에만 추가 타격을 주고 실제 두번 베기의 3타/2타를 비교했습니다.
