using UnityEngine;
using UnityEngine.EventSystems;

public class DragUnit : MonoBehaviour, IObserveStageChange, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private bool isDraggable = true;
    private Unit unit;
    private bool isDragging;
    private int currentTileIndex = -1;
    private PlacementPreview preview;
    private SpriteRenderer[] sprites;
    private bool[] spriteVisibility;

    void Awake() => unit = GetComponent<Unit>();
    private void OnEnable() => PhaseManager.Instance.AddSubScriber(this);
    private void OnDisable()
    {
        FinishDrag();
        if (PhaseManager.Instance != null) PhaseManager.Instance.RmvSubScriber(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable || isDragging || !unit.unitData.isAlly || PhaseManager.Instance.CurrentState != PhaseState.Placement) return;
        isDragging = true;
        currentTileIndex = unit.NodeIndex;
        preview = new PlacementPreview(gameObject);
        sprites = GetComponentsInChildren<SpriteRenderer>(true);
        spriteVisibility = new bool[sprites.Length];
        for (int i = 0; i < sprites.Length; i++) { spriteVisibility[i] = sprites[i].enabled; sprites[i].enabled = false; }
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        if (!isDraggable || PhaseManager.Instance.CurrentState != PhaseState.Placement) { FinishDrag(); return; }
        Vector2 pos = TileManager.Instance.ConvertCanvasPosToWorldPos(eventData.position);
        bool valid = TileManager.Instance.TryGetPlacementTile(pos, out int index, currentTileIndex);
        if (index != 0 && !PlacementPreview.IsOverUI(eventData.position)) preview.Show(index, valid);
        else preview.Hide();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        if (isDraggable && PhaseManager.Instance.CurrentState == PhaseState.Placement)
        {
            if (PlacementPreview.IsOverUI(eventData.position))
            {
                FinishDrag();
                // 재정비에서는 기존 용병의 위치만 바꿀 수 있습니다.
                if (PhaseManager.Instance.IsReformation) return;
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }
            Vector2 pos = TileManager.Instance.ConvertCanvasPosToWorldPos(eventData.position);
            if (TileManager.Instance.TryGetPlacementTile(pos, out int index, currentTileIndex))
            {
                Vector2 destination = TileManager.Instance.GetTileNodeByIndex(index).pos;
                transform.position = destination;
                if (TryGetComponent<Rigidbody2D>(out var rigid)) rigid.position = destination;
                unit.SetCurTile(index);
            }
        }
        FinishDrag();
    }

    private void FinishDrag()
    {
        preview?.Dispose();
        preview = null;
        if (sprites != null)
            for (int i = 0; i < sprites.Length; i++) if (sprites[i] != null) sprites[i].enabled = spriteVisibility[i];
        sprites = null;
        spriteVisibility = null;
        isDragging = false;
    }

    public void InChangeStageState(PhaseState changeState)
    {
        isDraggable = changeState == PhaseState.Placement;
        if (!isDraggable) FinishDrag();
    }
}
