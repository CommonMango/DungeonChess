using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Dungeon Chess/Handlers/Battle")]
public class BattleNodeHandler : NodeEncounterHandler
{
    [SerializeField] private string scenePath = "Assets/05. Scenes/Play.unity";
    public override IEnumerator Execute(NodeRunController run, NodeMapData.Node node)
    {
        if (!Application.CanStreamedLevelBeLoaded(scenePath)) throw new InvalidOperationException("Build Settings에 전투 씬이 없습니다: " + scenePath);
        Scene mapScene = run.gameObject.scene;
        run.View.SetMapVisible(false);
        yield return SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
        Scene battle = SceneManager.GetSceneByPath(scenePath);
        SceneManager.SetActiveScene(battle);
        // Allow battle components to finish Start before checking the result.
        yield return null;
        while (PhaseManager.Instance != null && PhaseManager.Instance.CurrentState != PhaseState.Reward && PhaseManager.Instance.CurrentState != PhaseState.Lose)
            yield return null;
        bool success = PhaseManager.Instance != null && PhaseManager.Instance.CurrentState == PhaseState.Reward;
        run.View.ShowResult(success);
        while (!run.ReturnRequested) yield return null;
        // Stop combat before units disable, so scene cleanup cannot generate a second result.
        if (PhaseManager.Instance != null) PhaseManager.Instance.ChangeState(PhaseState.Placement);
        foreach (var unit in UnityEngine.Object.FindObjectsByType<Unit>(FindObjectsSortMode.None))
            if (unit.gameObject.scene == battle) unit.gameObject.SetActive(false);
        SceneManager.SetActiveScene(mapScene);
        yield return SceneManager.UnloadSceneAsync(battle);
        run.FinishEncounter(success);
    }
}
