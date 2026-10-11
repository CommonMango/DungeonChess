using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillManagementWindow : EditorWindow
{
    public const string Folder = "Assets/02. Resources/07. Skills";
    public const string CatalogPath = Folder + "/SkillCatalog.asset";
    [SerializeField] private SkillCatalog catalog;
    [SerializeField] private Skill selectedSkill;
    [SerializeField] private UnitData selectedUnit;
    [SerializeField] private bool showAugments;
    private VisualElement skillEditor, unitEditor;
    private ScrollView registered;
    private HelpBox status;

    [MenuItem("Dungeon Chess/스킬 · 증강 관리")]
    public static void Open()
    {
        var window = GetWindow<SkillManagementWindow>("스킬 · 증강 관리");
        window.minSize = new Vector2(650, 680); window.Show();
    }
    public void CreateGUI()
    {
        rootVisualElement.Clear();
        var tabs = new VisualElement(); tabs.style.flexDirection = FlexDirection.Row;
        tabs.Add(new Button(() => { showAugments = false; CreateGUI(); }) { text = "스킬" });
        tabs.Add(new Button(() => { showAugments = true; CreateGUI(); }) { text = "증강" });
        rootVisualElement.Add(tabs);
        if (showAugments) { AugmentEditorPanel.Build(rootVisualElement); return; }
        var content = new ScrollView(); content.style.flexGrow = 1;
        content.style.paddingLeft = content.style.paddingRight = 12;
        content.style.paddingTop = content.style.paddingBottom = 12;
        rootVisualElement.Add(content);
        content.Add(new HelpBox("스킬 SO 생성 → 목록 등록 → 유닛 연결\n설정 데이터만 관리합니다. 실제 스킬 효과는 실행하지 않습니다.", HelpBoxMessageType.Info));
        if (catalog == null) catalog = AssetDatabase.LoadAssetAtPath<SkillCatalog>(CatalogPath);
        var catalogField = new ObjectField("스킬 카탈로그") { objectType = typeof(SkillCatalog), allowSceneObjects = false, value = catalog };
        catalogField.RegisterValueChangedCallback(e => { catalog = e.newValue as SkillCatalog; RefreshList(); }); content.Add(catalogField);
        content.Add(new Button(() => Run(() => {
            EnsureFolder(); catalog = AssetDatabase.LoadAssetAtPath<SkillCatalog>(CatalogPath);
            if (catalog == null) { catalog = CreateInstance<SkillCatalog>(); AssetDatabase.CreateAsset(catalog, CatalogPath); }
            catalogField.SetValueWithoutNotify(catalog); RefreshList();
        })) { text = "기본 카탈로그 생성 / 열기" });
        registered = new ScrollView(); registered.style.height = 120; content.Add(registered);
        var skillField = new ObjectField("편집할 스킬 SO") { objectType = typeof(Skill), allowSceneObjects = false, value = selectedSkill };
        skillField.RegisterValueChangedCallback(e => { selectedSkill = e.newValue as Skill; RefreshSkill(); }); content.Add(skillField);
        var actions = new VisualElement(); actions.style.flexDirection = FlexDirection.Row;
        actions.Add(new Button(() => Run(() => { selectedSkill = CreateSkill(catalog); skillField.SetValueWithoutNotify(selectedSkill); RefreshSkill(); RefreshList(); })) { text = "새 스킬 SO 생성 + 등록" });
        actions.Add(new Button(() => Run(() => { RegisterSkill(catalog, selectedSkill); RefreshList(); })) { text = "선택 스킬 등록" });
        actions.Add(new Button(() => Run(() => {
            RequireCatalog(); Undo.RecordObject(catalog, "Unregister skill"); catalog.Unregister(selectedSkill);
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets(); RefreshList();
        })) { text = "목록에서 해제" }); content.Add(actions);
        content.Add(new Label("목록 해제는 SO 파일과 유닛의 기존 연결을 삭제하지 않습니다."));
        skillEditor = new VisualElement(); content.Add(skillEditor);
        content.Add(new Button(() => Run(() => { RequireCatalog(); catalog.Validate(); AssetDatabase.SaveAssets(); RefreshList(); })) { text = "검증 및 저장" });
        var unitField = new ObjectField("연결할 유닛 SO") { objectType = typeof(UnitData), allowSceneObjects = false, value = selectedUnit };
        unitField.RegisterValueChangedCallback(e => { selectedUnit = e.newValue as UnitData; RefreshUnit(); }); content.Add(unitField);
        var unitActions = new VisualElement(); unitActions.style.flexDirection = FlexDirection.Row;
        unitActions.Add(new Button(() => Run(() => { AssignSkill(catalog, selectedUnit, selectedSkill); RefreshUnit(); })) { text = "유닛에 선택 스킬 추가" });
        unitActions.Add(new Button(() => Run(() => {
            if (selectedUnit == null || selectedSkill == null) throw new InvalidOperationException("유닛과 스킬을 선택하세요.");
            Undo.RecordObject(selectedUnit, "Remove unit skill");
            selectedUnit.useSkill = (selectedUnit.useSkill ?? Array.Empty<Skill>()).Where(s => s != selectedSkill).ToArray();
            EditorUtility.SetDirty(selectedUnit); AssetDatabase.SaveAssets(); RefreshUnit();
        })) { text = "유닛에서 선택 스킬 해제" }); content.Add(unitActions);
        unitEditor = new VisualElement(); content.Add(unitEditor);
        status = new HelpBox("스킬을 생성하거나 기존 SO를 선택하세요.", HelpBoxMessageType.Info); content.Add(status);
        RefreshList(); RefreshSkill(); RefreshUnit();
        content.SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);
    }
    private void OnEnable() { Undo.undoRedoPerformed += RefreshAll; EditorApplication.playModeStateChanged += OnPlayMode; }
    private void OnDisable() { Undo.undoRedoPerformed -= RefreshAll; EditorApplication.playModeStateChanged -= OnPlayMode; }
    private void OnPlayMode(PlayModeStateChange state) => CreateGUI();
    private void RefreshAll() { if (showAugments) { CreateGUI(); return; } if (registered != null) { RefreshList(); RefreshSkill(); RefreshUnit(); } }
    private void RequireCatalog() { if (catalog == null) throw new InvalidOperationException("카탈로그를 먼저 선택하세요."); }
    private void Run(Action action)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        try { action(); status.text = "완료했습니다."; status.messageType = HelpBoxMessageType.Info; }
        catch (Exception ex) { status.text = ex.Message; status.messageType = HelpBoxMessageType.Error; }
    }
    private void RefreshList()
    {
        if (registered == null) return;
        registered.Clear(); if (catalog == null) return;
        foreach (var skill in catalog.Skills)
        {
            var item = skill;
            registered.Add(new Button(() => { selectedSkill = item; CreateGUI(); if (item != null) EditorGUIUtility.PingObject(item); })
                { text = item != null ? $"{item.Skillname}  ({item.kind})  [{item.skillId}]" : "누락된 SO 참조" });
        }
    }
    private void RefreshSkill()
    {
        skillEditor.Unbind(); skillEditor.Clear(); if (selectedSkill == null) return;
        var serialized = new SerializedObject(selectedSkill);
        string[] fields = { "skillId", "implementationId", "Skillname", "SkillDescription", "kind", "icon", "cooldown", "skillDamage" };
        string[] labels = { "고유 ID", "코드 구현 ID", "이름", "설명", "종류", "아이콘", "재사용 대기시간 (초)", "데미지 설정값" };
        for (int i = 0; i < fields.Length; i++) skillEditor.Add(new PropertyField(serialized.FindProperty(fields[i]), labels[i]));
        if (selectedSkill is DoubleSlashData)
        {
            skillEditor.Add(new PropertyField(serialized.FindProperty("attackMultiplier"), "타격당 공격력 배율"));
            skillEditor.Add(new PropertyField(serialized.FindProperty("hitInterval"), "타격 간격 (초)"));
            skillEditor.Add(new HelpBox("두번 베기는 고정 데미지 설정값 대신 현재 유닛 공격력 × 배율을 사용합니다.", HelpBoxMessageType.Info));
        }
        skillEditor.Bind(serialized);
    }
    private void RefreshUnit()
    {
        unitEditor.Unbind(); unitEditor.Clear(); if (selectedUnit == null) return;
        var serialized = new SerializedObject(selectedUnit);
        unitEditor.Add(new PropertyField(serialized.FindProperty("useSkill"), "등록된 유닛 스킬")); unitEditor.Bind(serialized);
    }
    public static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/02. Resources", "07. Skills");
    }
    public static Skill CreateSkill(SkillCatalog target)
    {
        if (target == null) throw new InvalidOperationException("카탈로그를 먼저 선택하세요.");
        EnsureFolder(); var skill = CreateInstance<Skill>(); skill.skillId = Guid.NewGuid().ToString("N"); skill.Skillname = "새 스킬";
        AssetDatabase.CreateAsset(skill, AssetDatabase.GenerateUniqueAssetPath(Folder + "/NewSkill.asset"));
        Undo.RegisterCreatedObjectUndo(skill, "Create skill"); RegisterSkill(target, skill); return skill;
    }
    public static void RegisterSkill(SkillCatalog target, Skill skill)
    {
        if (target == null || skill == null || !AssetDatabase.Contains(skill)) throw new InvalidOperationException("카탈로그와 저장된 스킬 SO를 선택하세요.");
        Undo.RecordObject(target, "Register skill"); target.Register(skill); EditorUtility.SetDirty(target); AssetDatabase.SaveAssets();
    }
    public static void AssignSkill(SkillCatalog target, UnitData unit, Skill skill)
    {
        if (target == null || unit == null || skill == null) throw new InvalidOperationException("카탈로그·유닛·스킬을 선택하세요.");
        target.Validate(); if (!target.Skills.Contains(skill)) throw new InvalidOperationException("카탈로그에 스킬을 먼저 등록하세요.");
        var existing = unit.useSkill ?? Array.Empty<Skill>(); if (existing.Contains(skill)) return;
        Undo.RecordObject(unit, "Assign unit skill"); unit.useSkill = existing.Concat(new[] { skill }).ToArray();
        EditorUtility.SetDirty(unit); AssetDatabase.SaveAssets();
    }
}
