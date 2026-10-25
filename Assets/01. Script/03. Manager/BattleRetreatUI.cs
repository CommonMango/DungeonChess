using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 기존 전투 흐름에 후퇴와 첫 패배 선택만 표시하는 기본 UI입니다.
public sealed class BattleRetreatUI : MonoBehaviour
{
    private PhaseManager phase;
    private GameObject beforeBattle, choice;
    private TMP_Text beforeLabel, choiceLabel;
    private TMP_FontAsset font;

    private void Start()
    {
        phase = GetComponent<PhaseManager>();
        foreach (var text in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
            if (text.gameObject.scene == gameObject.scene && text.font != null) { font = text.font; break; }
        var root = new GameObject("후퇴 선택 UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(transform, false);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.matchWidthOrHeight = 0.5f;
        beforeBattle = new GameObject("전투 전 후퇴", typeof(RectTransform));
        beforeBattle.transform.SetParent(root.transform, false);
        var rect = beforeBattle.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-170, -45);
        beforeLabel = MakeButton(beforeBattle.transform, "후퇴", Vector2.zero, phase.Retreat);
        choice = new GameObject("첫 패배 선택", typeof(RectTransform), typeof(Image));
        choice.transform.SetParent(root.transform, false);
        rect = choice.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        choice.GetComponent<Image>().color = new Color(0, 0, 0, 0.85f);
        choiceLabel = MakeText(choice.transform, "", new Vector2(0, 75), new Vector2(700, 100));
        MakeButton(choice.transform, "재정비 후 재도전", new Vector2(-170, -35), phase.ChooseReformation);
        MakeButton(choice.transform, "후퇴", new Vector2(170, -35), phase.Retreat);
        Refresh();
    }

    private void Update() { if (phase != null) Refresh(); }

    private void Refresh()
    {
        bool before = phase.CurrentState == PhaseState.Placement && phase.BattleAttempt == 0;
        bool deciding = phase.CurrentState == PhaseState.DefeatChoice;
        beforeBattle.SetActive(before);
        choice.SetActive(deciding);
        if (before) beforeLabel.text = "후퇴 (의지 -" + phase.RetreatDamage + ")";
        if (deciding) choiceLabel.text = "첫 전투 패배 · 의지는 아직 감소하지 않았습니다.\n재정비는 1회 가능 · 후퇴 시 의지 -" + phase.RetreatDamage;
    }

    private TMP_Text MakeButton(Transform parent, string title, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject(title, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>(); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(300, 60);
        go.GetComponent<Image>().color = new Color(0.18f, 0.23f, 0.3f);
        go.GetComponent<Button>().onClick.AddListener(action);
        return MakeText(go.transform, title, Vector2.zero, new Vector2(290, 58));
    }

    private TMP_Text MakeText(Transform parent, string title, Vector2 position, Vector2 size)
    {
        var go = new GameObject("안내", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>(); rect.anchoredPosition = position; rect.sizeDelta = size;
        var text = go.GetComponent<TextMeshProUGUI>(); text.font = font; text.fontSize = 22;
        text.alignment = TextAlignmentOptions.Center; text.text = title; text.color = Color.white; text.raycastTarget = false;
        return text;
    }
}