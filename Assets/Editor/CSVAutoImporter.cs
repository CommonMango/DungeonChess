using System;
using UnityEditor;
using UnityEngine;

// CSV 변경 감지는 에디터에서만 수행하고 기존 파싱·적용 로직을 재사용합니다.
[InitializeOnLoad]
public sealed class CSVAutoImporter : AssetPostprocessor
{
    private static readonly string[] Paths =
    {
        "Assets/02. Resources/Resources/Data/Ally.csv",
        "Assets/02. Resources/Resources/Data/Enemy.csv"
    };
    private const string PendingPrefix = "DungeonChess.CSVAutoImporter.Pending.";
    private static double nextAttempt;
    private static bool applying;

    static CSVAutoImporter()
    {
        // SessionState의 대기 상태는 Play 전환과 스크립트 재컴파일 후에도 유지됩니다.
        EditorApplication.update += ProcessPending;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
        string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets) QueueIfSupported(path);
        foreach (string path in movedAssets) QueueIfSupported(path);
    }

    private static void QueueIfSupported(string path)
    {
        if (Array.IndexOf(Paths, path) < 0) return;
        SessionState.SetBool(PendingPrefix + path, true);
        // 연속 저장을 모으고 에셋 가져오기 콜백 밖에서 SO를 저장합니다.
        nextAttempt = EditorApplication.timeSinceStartup + 0.3;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            nextAttempt = EditorApplication.timeSinceStartup + 0.3;
    }

    private static void ProcessPending()
    {
        if (applying || EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorApplication.isCompiling || EditorApplication.isUpdating ||
            EditorApplication.timeSinceStartup < nextAttempt) return;

        applying = true;
        try
        {
            foreach (string path in Paths)
            {
                if (!SessionState.GetBool(PendingPrefix + path, false)) continue;
                // 오류는 한 번만 알리고 다음 CSV 저장 또는 재가져오기 때 다시 시도합니다.
                SessionState.EraseBool(PendingPrefix + path);
                try
                {
                    var source = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    var plan = CSVSOImporter.Parse(source);
                    int count = CSVSOImporter.Apply(plan);
                    Debug.Log($"CSV 자동 적용 완료: {path} → SO {count}개", source);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"CSV 자동 적용 실패: {path}\n기존 SO 값을 유지합니다. CSV를 수정하고 다시 저장해주세요.\n{exception.Message}");
                }
            }
        }
        finally { applying = false; }
    }
}
