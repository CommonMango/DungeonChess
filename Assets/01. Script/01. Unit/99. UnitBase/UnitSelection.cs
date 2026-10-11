using UnityEngine;
using UnityEngine.EventSystems;

public abstract partial class Unit : IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // DragUnit shares this object: a placement drag must not also select it.
        if (eventData.button != PointerEventData.InputButton.Left || eventData.dragging) return;
        int threshold = EventSystem.current != null ? EventSystem.current.pixelDragThreshold : 5;
        if ((eventData.position - eventData.pressPosition).sqrMagnitude > threshold * threshold) return;
        foreach (var details in Object.FindObjectsByType<UnitDetailsPanel>(FindObjectsSortMode.None))
        {
            if (details.gameObject.scene != gameObject.scene) continue;
            details.Show(this);
            break;
        }
    }
}