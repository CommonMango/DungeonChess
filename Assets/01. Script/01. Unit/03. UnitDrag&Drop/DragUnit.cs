using UnityEngine;
using UnityEngine.EventSystems;

public class DragUnit : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,IObserveStageChange
{
    Transform originalParent;
    bool isDragable = true;
    Unit unit;

    void Awake()
    {
        unit = GetComponent<Unit>();    
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(!isDragable)
            return;

        originalParent = transform.parent;
        transform.SetParent(originalParent.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(!isDragable)
            return;
        
        transform.position = eventData.position;   
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(!isDragable)
            return;
        
        MoveUnit(eventData.position); //유닛 해당 위치로 옮기기 

        transform.SetParent(originalParent); 
    }

    public void MoveUnit(Vector2 movePos)
    {
        transform.position = PathFinder.Instance.ConvertPositionToCloseNode(movePos); 
        int index = PathFinder.Instance.GetIndexByVector2(transform.position);
        unit.SetCurTile(index);
    }

    public void InChangeStageState(PhaseState changeState)
    {
        if(changeState == PhaseState.Placement)
        {
            isDragable = true;
        }   
        else
        {
            isDragable = false;
        } 
    }
}
