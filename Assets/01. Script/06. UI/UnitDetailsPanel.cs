using System.Text;
using TMPro;
using UnityEngine;

public sealed class UnitDetailsPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text unitNameText;
    [SerializeField] private TMP_Text teamText;
    [SerializeField] private UnityEngine.UI.Image portrait;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private UnityEngine.UI.Image healthFill;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text skillsText;
    [SerializeField] private UnityEngine.UI.ScrollRect skillsScroll;
    [SerializeField] private UnityEngine.UI.Button closeButton;
    private Unit selected;
    public Unit SelectedUnit => selected;

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
        Hide();
    }

    private void OnDestroy() => closeButton.onClick.RemoveListener(Hide);

    public void Show(Unit unit)
    {
        if (unit == null || !unit.isActiveAndEnabled || unit.unitData == null) return;
        selected = unit;
        var data = unit.unitData;
        unitNameText.text = data.unitName;
        teamText.text = data.isAlly ? "아군" : "적군";
        teamText.color = data.isAlly ? new Color(.4f, .85f, .7f) : new Color(1f, .5f, .45f);
        var sprite = unit.GetComponentInChildren<SpriteRenderer>();
        portrait.sprite = sprite != null ? sprite.sprite : null;
        portrait.enabled = portrait.sprite != null;
        portrait.preserveAspect = true;

        var description = new StringBuilder();
        if (data.useSkill != null)
            foreach (var skill in data.useSkill)
            {
                if (skill == null) continue;
                if (description.Length > 0) description.AppendLine().AppendLine();
                description.AppendLine(skill.Skillname);
                description.Append(string.IsNullOrWhiteSpace(skill.SkillDescription) ? "설명 없음" : skill.SkillDescription);
            }
        skillsText.text = description.Length > 0 ? description.ToString() : "등록된 스킬이 없습니다.";
        panel.SetActive(true);
        RefreshStats();
        Canvas.ForceUpdateCanvases();
        skillsScroll.StopMovement();
        skillsScroll.verticalNormalizedPosition = 1f;
    }

    private void LateUpdate()
    {
        if (selected == null || !selected.isActiveAndEnabled || selected.HP <= 0)
        {
            if (panel.activeSelf) Hide();
            return;
        }
        RefreshStats();
    }

    private void RefreshStats()
    {
        healthText.text = $"체력  {selected.HP:0.#} / {selected.MaxHP:0.#}";
        healthFill.fillAmount = selected.MaxHP > 0 ? Mathf.Clamp01(selected.HP / selected.MaxHP) : 0;
        // Match the existing basic attack fallback for unfinished attack-speed data.
        float attackSpeed = selected.AttackSpeed > 0 ? selected.AttackSpeed : 1f;
        statsText.text = $"공격력   {selected.Power:0.#}\n공격 속도   {attackSpeed:0.##}회/초\n이동 속도   {selected.MoveSpeed:0.##}\n공격 범위   {selected.AttackRange:0.#}칸";
    }

    public void Hide()
    {
        selected = null;
        panel.SetActive(false);
    }
}