using UnityEngine;
using UnityEngine.EventSystems;

public class DragUnit : MonoBehaviour, IObserveStageChange, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    
    private bool isDraggable = true;//드래그가 가능여부에 따라 바뀜 나중에 프로퍼티 추가할지 생각 
    private Unit unit;

    void Awake()
    {
        unit = GetComponent<Unit>();
    }
   
    private void OnEnable() => PhaseManager.Instance.AddSubScriber(this);
    private void OnDisable() => PhaseManager.Instance.RmvSubScriber(this);
    private bool isDragging;
    private Vector3 offset;
    private int currentTileIndex = -1; // 현재 유닛이 위치한 타일의 인덱스
    

    public void OnBeginDrag(PointerEventData eventData) //현재 위치 인덱스 저장 후 이동  
    {
        if(!isDraggable || PhaseManager.Instance.CurrentState != PhaseState.Placement) return;
        isDragging = true;
        currentTileIndex = unit.NodeIndex;
        Vector3 tempPos = TileManager.Instance.ConvertCanvasPosToWorldPos(eventData.position);
        offset = transform.position - tempPos;
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!isDraggable || PhaseManager.Instance.CurrentState != PhaseState.Placement) return;
        Vector3 tempPos = TileManager.Instance.ConvertCanvasPosToWorldPos(eventData.position);
        transform.position = tempPos + offset;
    }

    public void OnEndDrag(PointerEventData eventData) //드래그 끝나고 배치 될 수 있는 위치면 배치 UI쪽으로 끌어가면 배치 취소 그 이외에는 배치 복귀 
    {
        if(!isDraggable || PhaseManager.Instance.CurrentState != PhaseState.Placement) return;
        
        if (!isDragging) return;
        isDragging = false;
        bool isInUI = RectTransformUtility.RectangleContainsScreenPoint(UnitManager.Instance.UIRectTransform, eventData.position, null);
        Debug.Log($"isInUI: {isInUI}, isDraggable: {isDraggable}, currentTileIndex: {currentTileIndex}");
        if(isInUI) // 배치 UI 안에 들어왔을 때
        {
            // 배치 취소 로직
            Destroy(gameObject); // 유닛 제거
            TileManager.Instance.SetPosCost(currentTileIndex, 0); // 이전 타일의 장애물 비용을 초기화
            return;
        }
        else if(!TileManager.Instance.CheckInMap(transform.position)) // 맵 밖에 있을 때
        {
            // 배치 불가 피드백 출력
            Debug.Log("맵 밖에 유닛을 배치할 수 없습니다.");
            // 원래 위치로 복귀
            transform.position = TileManager.Instance.GetTileNodeByIndex(currentTileIndex).pos; 
        }
        else 
        {
            // 배치 가능한 위치인지 확인
            Vector2 TileVector = TileManager.Instance.ConvertPositionToCloseNode(transform.position);
            int newTileIndex = TileManager.Instance.GetIndexByVector2(TileVector);

            if (newTileIndex != -1 && TileManager.Instance.IsTileStandable(newTileIndex))
            {
                // 배치 가능하면 새로운 위치로 이동
                TileManager.Instance.SetPosCost(currentTileIndex, 0); // 이전 타일의 장애물 비용 초기화
                transform.position = TileManager.Instance.GetTileNodeByIndex(newTileIndex).pos;
                currentTileIndex = newTileIndex; // 현재 타일 인덱스 업데이트
            }
            else
            {
                // 배치 불가하면 원래 위치로 복귀
                transform.position = TileManager.Instance.GetTileNodeByIndex(currentTileIndex).pos;
            }
            TileManager.Instance.SetPosCost(currentTileIndex, TileManager.Instance.ObstacleCost); // 현재 타일의 장애물 비용 설정
            unit.SetCurTile(currentTileIndex); // 유닛의 현재 타일 인덱스 업데이트
        }
    }
    


    public void InChangeStageState(PhaseState changeState)
    {
        if (changeState != PhaseState.Placement && isDragging)
        {
            transform.position = TileManager.Instance.GetTileNodeByIndex(currentTileIndex).pos;
            isDragging = false;
        }
        if(changeState == PhaseState.Placement)
            isDraggable = true;
        else
            isDraggable = false;
    }

    
}