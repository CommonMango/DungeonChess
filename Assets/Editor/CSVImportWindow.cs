using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class CSVImportWindow : EditorWindow
{
    [SerializeField] private TextAsset csv;
    private CSVSOImporter.Plan plan;
    private HelpBox status;
    private ScrollView preview;
    private Button apply;

    [MenuItem("Dungeon Chess/CSV 가져오기")]
    public static void Open()
    {
        var window = GetWindow<CSVImportWindow>("CSV → SO");
        window.minSize = new Vector2(520, 420);
        window.Show();
    }
    public void CreateGUI()
    {
        rootVisualElement.Clear();
        rootVisualElement.style.paddingLeft = 12;
        rootVisualElement.style.paddingRight = 12;
        rootVisualElement.style.paddingTop = 12;
        rootVisualElement.style.paddingBottom = 12;
        rootVisualElement.Add(new HelpBox("1행 열 이름 · 2행 자료형 · 3행 설명(건너뜀) · 4행부터 데이터\nassetPath에는 적용할 SO 경로를 입력하세요.", HelpBoxMessageType.Info));
        if (csv == null) csv = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/02. Resources/Resources/Data/Ally.csv");
        var file = new ObjectField("CSV 파일") { objectType = typeof(TextAsset), allowSceneObjects = false, value = csv };
        file.RegisterValueChangedCallback(evt => { csv = evt.newValue as TextAsset; Invalidate(); });
        rootVisualElement.Add(file);
        var buttons = new VisualElement(); buttons.style.flexDirection = FlexDirection.Row;
        buttons.Add(new Button(ParseCSV) { text = "1. 파싱" });
        apply = new Button(ApplyToSO) { text = "2. SO에 적용" }; apply.SetEnabled(false); buttons.Add(apply);
        rootVisualElement.Add(buttons);
        status = new HelpBox("CSV를 선택하고 파싱을 누르세요.", HelpBoxMessageType.Info);
        rootVisualElement.Add(status);
        preview = new ScrollView(); preview.style.flexGrow = 1; rootVisualElement.Add(preview);
    }
    private void Invalidate()
    {
        plan = null; apply?.SetEnabled(false); preview?.Clear();
        if (status != null) status.text = "CSV를 다시 파싱해주세요.";
    }
    public void ParseCSV()
    {
        Invalidate();
        try
        {
            plan = CSVSOImporter.Parse(csv);
            foreach (var entry in plan.Entries)
            {
                var group = new Foldout { text = entry.Target.name + " — " + UnityEditor.AssetDatabase.GetAssetPath(entry.Target), value = true };
                foreach (string change in entry.Changes)
                {
                    var label = new Label(change); label.style.whiteSpace = WhiteSpace.Normal; group.Add(label);
                }
                preview.Add(group);
            }
            status.text = $"파싱 완료: SO {plan.Entries.Count}개. 내용을 확인한 뒤 적용하세요.";
            status.messageType = HelpBoxMessageType.Info;
            apply.SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);
        }
        catch (Exception ex) { ShowError(ex); }
    }
    public void ApplyToSO()
    {
        try
        {
            int count = CSVSOImporter.Apply(plan);
            plan = null; apply.SetEnabled(false);
            status.text = $"SO {count}개에 적용하고 저장했습니다. 되돌리기: Ctrl+Z";
            status.messageType = HelpBoxMessageType.Info;
        }
        catch (Exception ex) { Invalidate(); ShowError(ex); }
    }
    private void ShowError(Exception ex)
    {
        status.text = ex.Message; status.messageType = HelpBoxMessageType.Error;
    }
}
