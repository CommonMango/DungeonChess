using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitImage : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private UnitData unitData;
    [SerializeField] private TextMeshProUGUI tmpro;
    [SerializeField] private Coroutine coroutine;
    WaitForSeconds wfs = new WaitForSeconds(1f);
    private PlacementPreview preview;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private int originalSibling;
    private float originalAlpha;
    private bool originalRaycasts;

    private void Awake()
    {
        // The shop canvas must stay visible while only this card is being dragged.
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (originalParent != null || PhaseManager.Instance.IsReformation || PhaseManager.Instance.CurrentState != PhaseState.Placement) return;
        originalParent = transform.parent;
        originalPosition = transform.localPosition;
        originalScale = transform.localScale;
        originalSibling = transform.GetSiblingIndex();
        originalAlpha = canvasGroup.alpha;
        originalRaycasts = canvasGroup.blocksRaycasts;
        transform.SetParent(GetComponentInParent<Canvas>().rootCanvas.transform, true);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
        preview = new PlacementPreview(unitPrefab);
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (originalParent == null) return;
        if (PhaseManager.Instance.CurrentState != PhaseState.Placement) { FinishDrag(); return; }
        transform.position = eventData.position;
        Vector2 worldPos = TileManager.Instance.ConvertCanvasPosToWorldPos(eventData.position);
        bool valid = TileManager.Instance.TryGetPlacementTile(worldPos, out int index);
        if (index != 0 && !PlacementPreview.IsOverUI(eventData.position))
        {
            canvasGroup.alpha = 0f;
            preview.Show(index, valid);
        }
        else
        {
            canvasGroup.alpha = originalAlpha;
            preview.Hide();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (originalParent == null) return;
        if (PhaseManager.Instance.CurrentState == PhaseState.Placement && !PlacementPreview.IsOverUI(eventData.position))
            SpawnUnit(TileManager.Instance.ConvertCanvasPosToWorldPos(eventData.position));
        FinishDrag();
    }

    private void Update()
    {
        if (originalParent != null && PhaseManager.Instance.CurrentState != PhaseState.Placement) FinishDrag();
    }

    private void OnDisable() => FinishDrag();

    private void FinishDrag()
    {
        preview?.Dispose();
        preview = null;
        if (originalParent == null) return;
        var parent = originalParent;
        originalParent = null;
        transform.SetParent(parent, false);
        transform.SetSiblingIndex(originalSibling);
        transform.localPosition = originalPosition;
        transform.localScale = originalScale;
        canvasGroup.alpha = originalAlpha;
        canvasGroup.blocksRaycasts = originalRaycasts;
    }

    private void SpawnUnit(Vector2 pos)
    {
        if (!TileManager.Instance.TryGetPlacementTile(pos, out int index))
        {
            if (tmpro != null)
            {
                tmpro.text = "아래쪽의 비어 있는 칸에 배치해주세요.";
                if (coroutine != null) StopCoroutine(coroutine);
                coroutine = StartCoroutine(StartTxtDelay());
            }
            return;
        }
        var spawned = Instantiate(unitPrefab, TileManager.Instance.GetTileNodeByIndex(index).pos, Quaternion.identity);
        UnitManager.Instance.updateIndex(spawned.GetComponent<Unit>());
    }

    private IEnumerator StartTxtDelay()
    {
        yield return wfs;
        tmpro.text = "";
    }
}
