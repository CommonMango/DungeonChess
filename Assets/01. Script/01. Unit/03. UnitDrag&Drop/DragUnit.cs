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
   
    private Vector3 offset;
    private int currentTileIndex = -1; // 현재 유닛이 위치한 타일의 인덱스

    public void OnBeginDrag(PointerEventData eventData) //현재 위치 인덱스 저장 후 이동  
    {
        if(!isDraggable) return;
        transform.position = eventData.position;
        currentTileIndex = TileManager.Instance.GetIndexByVector2(transform.position);
        Vector3 tempPos = TileManager.Instance.ConvertCanvasPosToWorldPos(eventData.position);
        offset = transform.position - tempPos;
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!isDraggable) return;
        Vector3 tempPos = TileManager.Instance.ConvertCanvasPosToWorldPos(eventData.position);
        transform.position = tempPos + offset;
    }

    public void OnEndDrag(PointerEventData eventData) //드래그 끝나고 배치 될 수 있는 위치면 배치 UI쪽으로 끌어가면 배치 취소 그 이외에는 배치 복귀 
    {
        if(!isDraggable) return;




    }
    // UI체크 
    // void OnMouseUp() //마우스를 놓는 순간 스냅될 수 있도록 위치 보정
    // {
    //     //새로운 타일 이면 이전 타일 cost 비우고 새 타일 코스트 추가 
    //     //맵 밖의 타일이면 이전 타일로 복귀
    //     //유닛 UI쪽으로 끌었다면 배치 취소 후 이전 유닛 삭제 그 후 cost비우기 
    // }


    public void InChangeStageState(PhaseState changeState)
    {
        if(changeState == PhaseState.Placement)
            isDraggable = true;
        else
            isDraggable = false;
    }

    
}