using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CSVSOImporter
{
    public sealed class Plan
    {
        public string SourcePath;
        public string SourceText;
        public readonly List<Entry> Entries = new List<Entry>();
    }
    public sealed class Entry
    {
        public ScriptableObject Target;
        public string Snapshot;
        public CSVManager.Row Row;
        public readonly List<string> Changes = new List<string>();
    }
    public static Plan Parse(TextAsset source)
    {
        if (source == null) throw new InvalidOperationException("CSV 파일을 선택해주세요.");
        string path = AssetDatabase.GetAssetPath(source);
        if (string.IsNullOrEmpty(path)) throw new InvalidOperationException("프로젝트에 저장된 CSV를 선택해주세요.");
        string text = File.ReadAllText(path);
        var table = CSVManager.Parse(text, source.name);
        if (!table.ColumnTypes.TryGetValue("assetPath", out var pathType) || pathType != typeof(string))
            throw new FormatException("assetPath 열(string)에 적용할 SO 경로가 필요합니다.");
        var plan = new Plan { SourcePath = path, SourceText = text };
        var targets = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in table.Rows)
        {
            string targetPath = row.Get<string>("assetPath").Trim().Replace('\\', '/');
            if (!targetPath.StartsWith("Assets/", StringComparison.Ordinal) || targetPath.Split('/').Contains("..") || !targetPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                throw new FormatException($"{row.Line}행: 프로젝트 Assets 안의 .asset 경로가 필요합니다.");
            if (!targets.Add(targetPath)) throw new FormatException($"중복 적용 대상: {targetPath}");
            var target = AssetDatabase.LoadAssetAtPath<ScriptableObject>(targetPath);
            if (target == null) throw new FormatException($"SO를 찾을 수 없습니다: {targetPath}");
            var entry = new Entry { Target = target, Snapshot = EditorJsonUtility.ToJson(target), Row = row };
            using (var serialized = new SerializedObject(target))
            {
                foreach (var pair in row.Values)
                {
                    if (pair.Key == "id" || pair.Key == "assetPath") continue;
                    var property = serialized.FindProperty(pair.Key);
                    if (property == null || pair.Key == "m_Script") throw new FormatException($"{row.Line}행 {target.name}: 적용 가능한 필드 '{pair.Key}'가 없습니다.");
                    string before = ValueText(property);
                    SetValue(property, pair.Value, false);
                    entry.Changes.Add($"{pair.Key}: {before} → {Convert.ToString(pair.Value, CultureInfo.InvariantCulture)}");
                }
            }
            if (entry.Changes.Count == 0) throw new FormatException($"{row.Line}행: 적용할 필드가 없습니다.");
            plan.Entries.Add(entry);
        }
        if (plan.Entries.Count == 0) throw new FormatException("적용할 데이터 행이 없습니다.");
        return plan;
    }
    public static int Apply(Plan plan)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("플레이 모드를 종료한 뒤 적용해주세요.");
        if (plan == null) throw new InvalidOperationException("먼저 파싱 버튼을 눌러주세요.");
        if (!File.Exists(plan.SourcePath) || File.ReadAllText(plan.SourcePath) != plan.SourceText)
            throw new InvalidOperationException("파싱 후 CSV가 변경됐습니다. 다시 파싱해주세요.");
        foreach (var entry in plan.Entries)
        {
            if (entry.Target == null || EditorJsonUtility.ToJson(entry.Target) != entry.Snapshot)
                throw new InvalidOperationException("파싱 후 대상 SO가 변경됐습니다. 다시 파싱해주세요.");
            using (var serialized = new SerializedObject(entry.Target))
                foreach (var pair in entry.Row.Values)
                    if (pair.Key != "id" && pair.Key != "assetPath") SetValue(serialized.FindProperty(pair.Key), pair.Value, false);
        }
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("CSV를 SO에 적용");
        try
        {
            foreach (var entry in plan.Entries)
            {
                Undo.RecordObject(entry.Target, "CSV를 SO에 적용");
                using (var serialized = new SerializedObject(entry.Target))
                {
                    foreach (var pair in entry.Row.Values)
                        if (pair.Key != "id" && pair.Key != "assetPath") SetValue(serialized.FindProperty(pair.Key), pair.Value, true);
                    serialized.ApplyModifiedProperties();
                }
                EditorUtility.SetDirty(entry.Target);
            }
            Undo.CollapseUndoOperations(group);
            foreach (var entry in plan.Entries) AssetDatabase.SaveAssetIfDirty(entry.Target);
            return plan.Entries.Count;
        }
        catch
        {
            Undo.RevertAllDownToGroup(group);
            foreach (var entry in plan.Entries) if (entry.Target != null) AssetDatabase.SaveAssetIfDirty(entry.Target);
            throw;
        }
    }
    private static string ValueText(SerializedProperty p)
    {
        switch (p.propertyType)
        {
            case SerializedPropertyType.String: return p.stringValue;
            case SerializedPropertyType.Boolean: return p.boolValue.ToString();
            case SerializedPropertyType.Integer: return p.longValue.ToString(CultureInfo.InvariantCulture);
            case SerializedPropertyType.LayerMask: return p.intValue.ToString();
            case SerializedPropertyType.Float: return p.doubleValue.ToString(CultureInfo.InvariantCulture);
            case SerializedPropertyType.Vector2: return p.vector2Value.ToString();
            case SerializedPropertyType.Enum: return p.enumNames[p.enumValueIndex];
            default: return p.propertyType.ToString();
        }
    }
    private static void SetValue(SerializedProperty p, object value, bool write)
    {
        if (p == null) throw new FormatException("SO 필드가 없습니다.");
        switch (p.propertyType)
        {
            case SerializedPropertyType.String when value is string s:
                if (write) p.stringValue = s; return;
            case SerializedPropertyType.Boolean when value is bool b:
                if (write) p.boolValue = b; return;
            case SerializedPropertyType.LayerMask when value is int mask:
                if (write) p.intValue = mask; return;
            case SerializedPropertyType.Integer when value is int number:
                if (write) p.longValue = number; return;
            case SerializedPropertyType.Integer when value is long number && p.type == "long":
                if (write) p.longValue = number; return;
            case SerializedPropertyType.Float when value is float number:
                if (write) p.doubleValue = number; return;
            case SerializedPropertyType.Float when value is double number && p.type == "double":
                if (write) p.doubleValue = number; return;
            case SerializedPropertyType.Vector2 when value is Vector2 vector:
                if (write) p.vector2Value = vector; return;
            case SerializedPropertyType.Enum when value is string name:
                int index = Array.IndexOf(p.enumNames, name);
                if (index < 0) throw new FormatException($"{p.propertyPath}: 없는 enum 값 '{name}'");
                if (write) p.enumValueIndex = index; return;
            default: throw new FormatException($"{p.propertyPath}: SO 자료형 {p.type}에 CSV {value.GetType().Name}을 적용할 수 없습니다.");
        }
    }
}
