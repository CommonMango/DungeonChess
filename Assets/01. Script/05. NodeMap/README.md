# 노드 진행 맵 사용법

## 실행

1. Assets/05. Scenes/NodeMap.unity를 열고 Play를 누릅니다. UI는 실행 시 생성됩니다.
2. 아래의 금색 전투 노드를 선택합니다. 기존 Play 씬으로 들어갑니다.
3. 아군 배치 후 기존 전투 시작 버튼을 누릅니다.
4. 결과 화면의 경로로 돌아가기를 누릅니다. 승리하면 연결된 다음 노드만 선택할 수 있습니다.
5. 패배하면 진행이 종료됩니다. 새 도전으로 처음부터 시작합니다. 맨 위 노드 승리 시 완료됩니다.

현재는 5개 층, 9개 노드의 고정 분기 맵입니다. 한 경로에서 전투 5회를 진행합니다.
모든 노드는 같은 Play 씬을 사용하며 매번 새 전투로 시작합니다. 유닛·체력·재화의 전투 간 유지, 보상, 랜덤 맵 생성, 저장/불러오기는 이번 구현에 포함하지 않았습니다.

## 맵 편집

Assets/02. Resources/06. NodeMap/BattleRoute.asset을 Inspector에서 편집합니다.

- startNodes: 처음 선택할 수 있는 노드의 ID 목록.
- nodes/id: 중복되지 않는 노드 ID. 연결은 배열 순서 대신 이 ID를 사용합니다.
- nodes/position: 맵 내부 UI 좌표. 위쪽이 다음 층입니다.
- nodes/type: 노드 종류 SO. 현재 모두 BattleNode입니다.
- nodes/next: 이 노드를 완료한 뒤 선택할 수 있는 노드 ID 목록. 비어 있으면 마지막 노드입니다.

시작 시 중복 ID, 없는 연결, 순환 연결, 도달 불가능한 노드, 처리기 누락을 검사합니다.
SO는 설계 데이터이며 완료 상태를 기록하지 않습니다. 런타임 진행은 NodeRunController에만 저장됩니다.

## 구조

- NodeMapData: 노드와 연결 데이터.
- NodeTypeData: 표시 이름, 선택 가능 색상, 실행 처리기 참조.
- NodeEncounterHandler: 노드 동작의 공통 추상 클래스.
- BattleNodeHandler: 전투 씬 추가 로드 → 결과 대기 → 복귀 → 전투 씬 해제.
- NodeRunController: 선택 가능 여부와 진행 상태 관리.
- NodeMapView: 노드 버튼, 연결선, 결과 화면 표시.

맵 씬을 유지한 채 전투 씬을 Additive로 불러옵니다. 전투 중 맵 카메라와 EventSystem은 꺼집니다.
Unit/Phase/Tile/Move/MoneyManager는 전투 씬과 함께 해제되도록 변경했습니다. CSVManager는 기존처럼 유지됩니다.
Build Settings에는 NodeMap이 첫 씬이고 Play도 포함되어 있습니다.

## 전투 이외의 노드 추가

1. NodeEncounterHandler를 상속하는 처리기를 만듭니다. 이벤트 노드라면 선택지를 표시하고 결과를 적용합니다.
2. Execute(NodeRunController run, NodeMapData.Node node)에서 처리를 기다린 뒤 run.FinishEncounter(true)를 호출합니다. false는 도전 실패를 뜻합니다.
3. CreateAssetMenu를 붙인 처리기 SO를 생성합니다.
4. Create > Dungeon Chess > Node Type으로 종류 SO를 만들고 displayName, color, handler를 지정합니다.
5. BattleRoute의 원하는 노드 type에 새 종류 SO를 할당합니다. 선택 규칙과 연결선 코드는 수정할 필요가 없습니다.

처리기 사용 예시(설명용, 현재 이벤트 기능은 구현하지 않음):

```csharp
[CreateAssetMenu(menuName = "Dungeon Chess/Handlers/Event")]
public class EventNodeHandler : NodeEncounterHandler
{
    public override IEnumerator Execute(NodeRunController run, NodeMapData.Node node)
    {
        // 런타임 UI를 열고 선택 완료까지 기다린 뒤 보상을 적용합니다.
        // 필요하면 run.View.SetMapVisible(false)로 맵을 숨깁니다.
        // 새 씬/오브젝트를 만들었다면 완료 전에 직접 정리합니다.
        yield return null;
        run.FinishEncounter(true); // 진행 확정과 맵 복원
    }
}
```

예시에는 System.Collections와 UnityEngine using이 필요합니다.
처리기 SO에는 공유 설정만 저장하고, 방문별 임시 상태는 Execute의 지역 변수 또는 런타임 객체에 둡니다.
이벤트 보상 CSV는 추후 이벤트 처리기에서 기존 CSVManager로 읽도록 연결할 수 있습니다. 현재 맵 연결 데이터는 SO에서 편집합니다.

## 검증

- 시작 시 start만 선택 가능.
- 아군 3개 배치 후 실제 전투 승리, 결과 화면과 맵 복귀 확인.
- 승리 후 연결된 두 분기만 선택 가능하며 이전/먼 노드는 잠김.
- 다음 전투에 아군 없이 시작하여 패배, 진행 잠금과 새 도전 초기화 확인.
- 승리 상태를 주입한 5개 노드 연속 진행으로 마지막 노드 완료 확인.
- 복귀 후 전투 씬과 매니저 해제, 재진입 시 매니저/EventSystem 중복 없음 확인.