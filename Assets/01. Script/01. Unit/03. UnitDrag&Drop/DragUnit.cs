using UnityEngine;

//마우스 드래그하는 클래스인데 일단 나중에..
public class DragUnit : MonoBehaviour, IObserveStageChange
{
    private Camera cam;
    private Vector3 dragOffset;
    private bool isDragable = true;

    private Unit unit;

    void Awake()
    {
        cam = Camera.main;
        unit = GetComponent<Unit>();
    }

    void OnMouseDown()
    {
        if (!isDragable) return;

        Vector3 mousePos = GetMouseWorldPos();
        dragOffset = transform.position - mousePos;
    }

    void OnMouseDrag()
    {
        if (!isDragable) return;

        Vector3 mousePos = GetMouseWorldPos();
        mousePos.z = 0;

        transform.position = mousePos + dragOffset;
    }

    void OnMouseUp()
{
    if (!isDragable) return;

    Vector3 mousePos = GetMouseWorldPos();
    Vector2 snap = PathFinder.Instance.ConvertPositionToCloseNode(mousePos);
    transform.position = snap;

    int newIndex = PathFinder.Instance.GetIndexByVector2(snap);

    // 기존 타일 비우기
    PathFinder.Instance.SetPosCost(unit.NodeIndex, 0);

    // NodeIndex + BTree 조건 처리 + UnitManager update 전부 내부에서 처리됨
    unit.SetCurTile(newIndex);

    // 새 타일 점유
    PathFinder.Instance.SetPosCost(newIndex, PathFinder.Instance.ObstacleCost);
}

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        return mousePos;
    }

    public void InChangeStageState(PhaseState changeState)
    {
        if(changeState == PhaseState.Placement)
            isDragable = true;
        else
            isDragable = false;
    }
}