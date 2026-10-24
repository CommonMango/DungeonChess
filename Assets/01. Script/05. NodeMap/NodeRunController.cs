using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeRunController : MonoBehaviour
{
    [SerializeField] private NodeMapData map;
    [SerializeField] private NodeMapView view;
    private readonly HashSet<string> completed = new HashSet<string>();
    public NodeMapData Map => map;
    public string CurrentId { get; private set; }
    public string PendingId { get; private set; }
    public bool IsBusy { get; private set; }
    public bool IsFailed { get; private set; }
    public bool IsComplete { get; private set; }
    public int CompletedCount => completed.Count;
    public bool ReturnRequested { get; private set; }
    public NodeMapView View => view;
    private bool encounterFinished;

    private void Start()
    {
        RunManager.EnsureRun();
        AugmentManager.ClearRun();
        try { map.Validate(); view.Initialize(this); view.Refresh(); }
        catch (Exception ex) { Debug.LogException(ex); enabled = false; }
    }
    public bool IsVisited(string id) => completed.Contains(id);
    public bool CanSelect(string id)
    {
        if (IsBusy || IsFailed || IsComplete || map.Find(id) == null || completed.Contains(id)) return false;
        return string.IsNullOrEmpty(CurrentId) ? map.startNodes.Contains(id) : map.Find(CurrentId).next.Contains(id);
    }
    public void Select(string id)
    {
        if (!CanSelect(id)) return;
        PendingId = id; IsBusy = true; ReturnRequested = false; encounterFinished = false;
        view.Refresh();
        StartCoroutine(RunEncounter(map.Find(id)));
    }
    private IEnumerator RunEncounter(NodeMapData.Node node)
    {
        // Flatten nested coroutines so a handler error cannot silently leave the map locked.
        var stack = new Stack<IEnumerator>();
        stack.Push(node.type.handler.Execute(this, node));
        while (stack.Count > 0)
        {
            object next = null; bool running = false; Exception error = null;
            try { running = stack.Peek().MoveNext(); if (running) next = stack.Peek().Current; }
            catch (Exception ex) { error = ex; }
            if (error != null)
            {
                Debug.LogException(error); IsBusy = false; PendingId = null;
                view.SetMapVisible(true); view.Refresh(); view.SetStatus("진입 실패: " + error.Message); yield break;
            }
            if (!running) { stack.Pop(); continue; }
            if (next is IEnumerator nested) stack.Push(nested); else yield return next;
        }
        if (!encounterFinished)
        {
            IsBusy = false; PendingId = null; view.SetMapVisible(true); view.Refresh();
            view.SetStatus("노드 처리기가 완료 결과를 전달하지 않았습니다.");
        }
    }
    public void RequestReturn() => ReturnRequested = true;
    public void FinishEncounter(bool success)
    {
        if (!IsBusy || encounterFinished) return;
        encounterFinished = true;
        // 패배해도 의지가 남아 있으면 해당 노드를 지나 다음 경로로 진행합니다.
        bool canContinue = RunManager.HasRun && !RunManager.Current.IsWillDepleted;
        if ((success || canContinue) && !(RunManager.HasRun && RunManager.Current.IsWillDepleted))
        {
            completed.Add(PendingId); CurrentId = PendingId;
            IsComplete = map.Find(CurrentId).next.Count == 0;
        }
        else IsFailed = true;
        PendingId = null; IsBusy = false;
        view.SetMapVisible(true); view.Refresh();
    }
    public void ResetRun()
    {
        if (IsBusy) return;
        RunManager.StartNewRun();
        AugmentManager.ClearRun();
        completed.Clear(); CurrentId = null; PendingId = null;
        IsFailed = false; IsComplete = false; ReturnRequested = false;
        view.Refresh();
    }
}
