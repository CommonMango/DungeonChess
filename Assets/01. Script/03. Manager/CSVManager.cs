using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using UnityEngine;

public class CSVManager : MonoBehaviour
{
    public static CSVManager Instance { get; private set; }
    [SerializeField] private TextAsset[] csvFiles = Array.Empty<TextAsset>();
    private readonly Dictionary<string, Table> tables = new Dictionary<string, Table>(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, Table> Tables => new ReadOnlyDictionary<string, Table>(tables);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Reload();
    }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    [ContextMenu("Reload CSV")]
    public void Reload()
    {
        // Build every table first: a failure must not replace only half the dataset.
        var loaded = new Dictionary<string, Table>(StringComparer.Ordinal);
        try
        {
            foreach (var file in csvFiles)
            {
                if (file == null) throw new FormatException("CSV Files에 비어 있는 항목이 있습니다.");
                if (loaded.ContainsKey(file.name)) throw new FormatException($"중복 테이블 이름: {file.name}");
                loaded.Add(file.name, Parse(file.text, file.name));
            }
            foreach (var pair in loaded) tables[pair.Key] = pair.Value;
        }
        catch (Exception ex) when (ex is FormatException || ex is ArgumentException)
        { Debug.LogError($"CSV 로드 실패: {ex.Message}", this); }
    }

    // Loading one table never discards other tables. Failure preserves the old table.
    public bool Load(TextAsset source, string tableName = null)
    {
        if (TryLoad(source, out string error, tableName)) return true;
        Debug.LogError(error, this);
        return false;
    }
    public bool TryLoad(TextAsset source, out string error, string tableName = null)
    {
        try
        {
            if (source == null) throw new ArgumentException("CSV 파일이 지정되지 않았습니다.");
            string name = tableName ?? source.name;
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("테이블 이름이 비어 있습니다.");
            var parsed = Parse(source.text, name);
            tables[name] = parsed;
            error = null;
            return true;
        }
        catch (Exception ex) when (ex is FormatException || ex is ArgumentException)
        { error = $"CSV 로드 실패: {ex.Message}"; return false; }
    }
    public Table GetTable(string name) => tables.TryGetValue(name, out var table) ? table :
        throw new KeyNotFoundException($"CSV 테이블을 찾을 수 없습니다: {name}");
    public bool TryGetTable(string name, out Table table) => tables.TryGetValue(name, out table);

    public sealed class Table
    {
        public string Name { get; }
        public IReadOnlyDictionary<string, Type> ColumnTypes { get; }
        public IReadOnlyList<Row> Rows { get; }
        private readonly Dictionary<object, Row> byId;
        internal Table(string name, Dictionary<string, Type> types, List<Row> rows, Dictionary<object, Row> ids)
        { Name = name; ColumnTypes = new ReadOnlyDictionary<string, Type>(types); Rows = rows.AsReadOnly(); byId = ids; }
        public Row GetById(object id) => id != null && byId.TryGetValue(id, out var row) ? row :
            throw new KeyNotFoundException($"CSV '{Name}': ID '{id}'를 찾을 수 없습니다. ID 자료형도 일치해야 합니다.");
        public bool TryGetById(object id, out Row row)
        { row = null; return id != null && byId.TryGetValue(id, out row); }
    }
    public sealed class Row
    {
        public int Line { get; }
        public IReadOnlyDictionary<string, object> Values { get; }
        internal Row(int line, Dictionary<string, object> values)
        { Line = line; Values = new ReadOnlyDictionary<string, object>(values); }
        public object this[string column] => Values[column];
        // Values are already converted during Parse; this only checks the requested C# type.
        public T Get<T>(string column)
        {
            object value = Values[column];
            if (value is T typed) return typed;
            throw new InvalidCastException($"CSV {Line}행 '{column}': 실제 {value?.GetType().Name ?? "null"}, 요청 {typeof(T).Name}");
        }
    }

    private sealed class Converter
    {
        public Type Type;
        public Func<string, object> Parse;
    }
    private static readonly Dictionary<string, Converter> converters = CreateConverters();
    public static void RegisterType<T>(string name, Func<string, T> parser)
    {
        if (string.IsNullOrWhiteSpace(name) || name != name.Trim()) throw new ArgumentException("자료형 이름이 올바르지 않습니다.");
        if (parser == null) throw new ArgumentNullException(nameof(parser));
        if (converters.ContainsKey(name)) throw new ArgumentException($"이미 등록된 자료형: {name}");
        converters.Add(name, new Converter { Type = typeof(T), Parse = text => parser(text) });
    }
    private static Dictionary<string, Converter> CreateConverters()
    {
        var result = new Dictionary<string, Converter>(StringComparer.OrdinalIgnoreCase);
        result.Add("string", new Converter { Type = typeof(string), Parse = s => s });
        result.Add("int", new Converter { Type = typeof(int), Parse = s => int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture) });
        result.Add("long", new Converter { Type = typeof(long), Parse = s => long.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture) });
        result.Add("float", new Converter { Type = typeof(float), Parse = s => ParseFloat(s) });
        result.Add("double", new Converter { Type = typeof(double), Parse = s => {
            double v = double.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
            if (double.IsNaN(v) || double.IsInfinity(v)) throw new FormatException("유한한 실수가 필요합니다.");
            return v;
        }});
        result.Add("bool", new Converter { Type = typeof(bool), Parse = s => {
            s = s.Trim(); if (s == "1") return true; if (s == "0") return false; return bool.Parse(s);
        }});
        result.Add("vector2", new Converter { Type = typeof(Vector2), Parse = s => {
            var parts = s.Split(';'); if (parts.Length != 2) throw new FormatException("x;y 형식이 필요합니다.");
            return new Vector2(ParseFloat(parts[0]), ParseFloat(parts[1]));
        }});
        return result;
    }
    private static float ParseFloat(string text)
    {
        float value = float.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
        if (float.IsNaN(value) || float.IsInfinity(value)) throw new FormatException("유한한 실수가 필요합니다.");
        return value;
    }

    // Row 1: names. Row 2: types. Row 3: documentation only (discarded). Row 4 onward: data.
    public static Table Parse(string text, string tableName = "CSV")
    {
        if (text == null) throw new ArgumentNullException(nameof(text));
        var records = ReadRecords(text);
        if (records.Count < 3) throw new FormatException($"'{tableName}': 헤더, 자료형, 설명 행이 필요합니다.");
        string[] header = records[0].Fields;
        string[] declared = records[1].Fields;
        if (declared.Length != header.Length) throw new FormatException($"'{tableName}': 헤더와 자료형 개수가 다릅니다.");
        var types = new Dictionary<string, Type>(StringComparer.Ordinal);
        var parsers = new Converter[header.Length];
        int idColumn = -1;
        for (int i = 0; i < header.Length; i++)
        {
            header[i] = header[i].Trim();
            if (header[i].Length == 0 || types.ContainsKey(header[i])) throw new FormatException($"'{tableName}': 빈 헤더 또는 중복 헤더 '{header[i]}'");
            if (!converters.TryGetValue(declared[i].Trim(), out parsers[i]))
                throw new FormatException($"'{tableName}' {records[1].Line}행 '{header[i]}': 등록되지 않은 자료형 '{declared[i]}'");
            types.Add(header[i], parsers[i].Type);
            if (header[i] == "id") idColumn = i;
        }
        var rows = new List<Row>();
        var ids = new Dictionary<object, Row>();
        for (int r = 3; r < records.Count; r++)
        {
            var record = records[r];
            if (record.Fields.Length != header.Length)
                throw new FormatException($"'{tableName}' {record.Line}행: 열 개수 {record.Fields.Length}, 헤더는 {header.Length}개입니다.");
            var values = new Dictionary<string, object>(StringComparer.Ordinal);
            for (int c = 0; c < header.Length; c++)
            {
                try
                {
                    object value = parsers[c].Parse(record.Fields[c]);
                    if (value == null || !parsers[c].Type.IsInstanceOfType(value)) throw new FormatException("변환 결과의 자료형이 올바르지 않습니다.");
                    values.Add(header[c], value);
                }
                catch (Exception ex)
                { throw new FormatException($"'{tableName}' {record.Line}행 '{header[c]}' ({declared[c]}), 값 '{record.Fields[c]}': {ex.Message}", ex); }
            }
            var row = new Row(record.Line, values);
            if (idColumn >= 0)
            {
                object id = values[header[idColumn]];
                if ((id is string s && string.IsNullOrWhiteSpace(s)) || ids.ContainsKey(id))
                    throw new FormatException($"'{tableName}' {record.Line}행: 빈 ID 또는 중복 ID '{id}'");
                ids.Add(id, row);
            }
            rows.Add(row);
        }
        return new Table(tableName, types, rows, ids);
    }

    private sealed class Record
    {
        public int Line;
        public string[] Fields;
    }

    private static List<Record> ReadRecords(string text)
    {
        var records = new List<Record>();
        var fields = new List<string>();
        var field = new StringBuilder();
        bool inQuotes = false, closedQuote = false, touched = false;
        int line = 1, recordLine = 1;
        Action finishRecord = () =>
        {
            // Ignore physically blank lines, but retain explicit empty CSV fields.
            if (records.Count < 3 || touched || fields.Count > 0 || field.Length > 0)
            {
                fields.Add(field.ToString());
                if (records.Count < 3 || !(fields.Count == 1 && string.IsNullOrWhiteSpace(fields[0]) && !touched))
                    records.Add(new Record { Line = recordLine, Fields = fields.ToArray() });
            }
            fields.Clear(); field.Clear(); closedQuote = false; touched = false;
        };
        for (int i = text.Length > 0 && text[0] == '\uFEFF' ? 1 : 0; i < text.Length; i++)
        {
            char ch = text[i];
            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else { inQuotes = false; closedQuote = true; }
                }
                else
                {
                    field.Append(ch);
                    if (ch == '\n' || (ch == '\r' && (i + 1 == text.Length || text[i + 1] != '\n'))) line++;
                }
                continue;
            }
            if (ch == ',')
            {
                fields.Add(field.ToString()); field.Clear(); closedQuote = false; touched = true;
            }
            else if (ch == '\r' || ch == '\n')
            {
                finishRecord();
                if (ch == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                line++; recordLine = line;
            }
            else if (closedQuote)
            {
                if (ch != ' ' && ch != '\t') throw new FormatException($"CSV {line}행: 닫는 따옴표 뒤에 잘못된 문자가 있습니다.");
            }
            else if (ch == '"')
            {
                if (field.Length != 0) throw new FormatException($"CSV {line}행: 필드 중간에 따옴표가 있습니다.");
                inQuotes = true; touched = true;
            }
            else
            {
                field.Append(ch);
                if (!char.IsWhiteSpace(ch)) touched = true;
            }
        }
        if (inQuotes) throw new FormatException($"CSV {recordLine}행: 닫히지 않은 따옴표가 있습니다.");
        finishRecord();
        return records;
    }
}
