using UnityEngine;



public class DragUnit : MonoBehaviour, IObserveStageChange
{
    
    private bool isDragable = true;
    private Unit unit;

    void Awake()
    {
        unit = GetComponent<Unit>();
    }
   
    public bool isDraggable = true; // 특정 조건일 때만 true로

    private Vector3 offset;

    void OnMouseDown()
    {
       
        if (!isDraggable) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = transform.position.z;

        offset = transform.position - mousePos;
    }

    void OnMouseDrag()
    {
        if (!isDraggable) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = transform.position.z;

        transform.position = mousePos + offset;
    }


    public void InChangeStageState(PhaseState changeState)
    {
        if(changeState == PhaseState.Placement)
            isDragable = true;
        else
            isDragable = false;
    } 
}