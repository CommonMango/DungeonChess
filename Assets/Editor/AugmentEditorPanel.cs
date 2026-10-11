using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public static class AugmentEditorPanel
{
    public const string Folder = "Assets/02. Resources/08. Augments";
    public const string CatalogPath = Folder + "/AugmentCatalog.asset";
    public static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/02. Resources", "08. Augments");
    }
    public static void Build(VisualElement parent)
    {
        var content = new ScrollView(); content.style.flexGrow = 1;
        content.style.paddingLeft = content.style.paddingRight = 12;
        parent.Add(content);
        content.Add(new HelpBox("SO는 증강 정의이며 획득 상태는 AugmentManager가 관리합니다.\n전체 아군 / 유닛 종류 / 태그 / 선택 개체를 지정할 수 있습니다.", HelpBoxMessageType.Info));
        var catalog = AssetDatabase.LoadAssetAtPath<AugmentCatalog>(CatalogPath);
        AugmentData selected = null;
        var status = new HelpBox("목록 등록만으로 게임에 자동 적용되지는 않습니다.", HelpBoxMessageType.Info);
        var list = new ScrollView(); list.style.height = 150;
        var editor = new VisualElement();
        var catalogField = new ObjectField("증강 카탈로그") { objectType = typeof(AugmentCatalog), allowSceneObjects = false, value = catalog };
        var selection = new ObjectField("증강 SO") { objectType = typeof(AugmentData), allowSceneObjects = false };
        void Run(Action action)
        {
            try { action(); status.text = "완료했습니다."; status.messageType = HelpBoxMessageType.Info; }
            catch (Exception ex) { status.text = ex.Message; status.messageType = HelpBoxMessageType.Error; }
        }
        void Edit()
        {
            editor.Unbind(); editor.Clear(); if (selected == null) return;
            var so = new SerializedObject(selected);
            string[] fields = { "augmentId", "displayName", "description", "icon", "targetScope", "targetUnit", "targetTag", "stat", "operation", "value", "skillImplementationId", "maxStacks" };
            string[] labels = { "고유 ID", "이름", "설명", "아이콘", "적용 범위", "대상 유닛 SO", "대상 태그", "변경할 수치", "계산 방식", "효과량", "대상 스킬 구현 ID", "최대 중첩" };
            for (int i = 0; i < fields.Length; i++) editor.Add(new PropertyField(so.FindProperty(fields[i]), labels[i]));
            editor.Bind(so);
        }
        void Refresh()
        {
            list.Clear(); if (catalog == null) return;
            foreach (var data in catalog.Augments)
            {
                var item = data;
                list.Add(new Button(() => { selected = item; selection.SetValueWithoutNotify(item); Edit(); })
                { text = item != null ? item.displayName + "  [" + item.targetScope + "]" : "누락된 SO" });
            }
        }
        catalogField.RegisterValueChangedCallback(e => { catalog = e.newValue as AugmentCatalog; Refresh(); });
        selection.RegisterValueChangedCallback(e => { selected = e.newValue as AugmentData; Edit(); });
        content.Add(catalogField);
        content.Add(new Button(() => Run(() => {
            EnsureFolder(); catalog = AssetDatabase.LoadAssetAtPath<AugmentCatalog>(CatalogPath);
            if (catalog == null) { catalog = ScriptableObject.CreateInstance<AugmentCatalog>(); AssetDatabase.CreateAsset(catalog, CatalogPath); }
            catalogField.SetValueWithoutNotify(catalog); Refresh();
        })) { text = "기본 카탈로그 생성 / 열기" });
        content.Add(list); content.Add(selection);
        var row = new VisualElement(); row.style.flexDirection = FlexDirection.Row;
        row.Add(new Button(() => Run(() => {
            if (catalog == null) throw new InvalidOperationException("카탈로그를 선택하세요.");
            EnsureFolder(); selected = ScriptableObject.CreateInstance<AugmentData>(); selected.augmentId = Guid.NewGuid().ToString("N"); selected.displayName = "새 증강";
            AssetDatabase.CreateAsset(selected, AssetDatabase.GenerateUniqueAssetPath(Folder + "/NewAugment.asset")); Undo.RegisterCreatedObjectUndo(selected, "Create augment");
            Undo.RecordObject(catalog, "Register augment"); catalog.Register(selected); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            selection.SetValueWithoutNotify(selected); Edit(); Refresh();
        })) { text = "새 증강 SO 생성 + 등록" });
        row.Add(new Button(() => Run(() => {
            if (catalog == null || selected == null) throw new InvalidOperationException("카탈로그와 증강 SO를 선택하세요.");
            Undo.RecordObject(catalog, "Register augment"); catalog.Register(selected); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets(); Refresh();
        })) { text = "등록" });
        row.Add(new Button(() => Run(() => {
            if (catalog == null) throw new InvalidOperationException("카탈로그를 선택하세요.");
            Undo.RecordObject(catalog, "Unregister augment"); catalog.Unregister(selected); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets(); Refresh();
        })) { text = "목록에서 해제" }); content.Add(row); content.Add(editor);
        content.Add(new HelpBox("Percent: 0.2 = 20% / SkillHitCount: Add + 정수\nTag는 유닛 SO의 augmentTags와 정확히 일치해야 합니다.\nSelectedUnit의 실제 대상은 획득 시 선택합니다.", HelpBoxMessageType.Info));
        content.Add(new Button(() => Run(() => {
            if (catalog == null) throw new InvalidOperationException("카탈로그를 선택하세요."); catalog.Validate(); AssetDatabase.SaveAssets(); Refresh();
        })) { text = "검증 및 저장" }); content.Add(status);
        Refresh(); content.SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);
    }
}
