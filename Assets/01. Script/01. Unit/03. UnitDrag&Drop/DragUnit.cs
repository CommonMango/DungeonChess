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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(!isDraggable) return;
        transform.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!isDraggable) return;
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(!isDraggable) return;
    }

    // void OnMouseDown() //마우스로 클릭한 순간
    // {
    //     if (!isDraggable) return;

    //     Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //     mousePos.z = transform.position.z;

    //     offset = transform.position - mousePos;
    //     Debug.Log("마우스 클릭");
    // }

    // void OnMouseDrag() //마우스로 끄는 순간 
    // {
    //     if (!isDraggable) return;

    //     Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //     mousePos.z = transform.position.z;

    //     transform.position = mousePos + offset;

    //     Debug.Log("마우스 드래그");
    // }

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