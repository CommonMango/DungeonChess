using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 요청자들에게 가야할 노드를 순서대로 알려주는 매니저
/// </summary>
public class MoveManager : MonoBehaviour
{
    private Queue<Unit> noneReadyRequests = new Queue<Unit>();
    private Queue<Unit> ReadyRequests = new Queue<Unit>();
    private Unit curRequest;
    private static MoveManager instance;
    public static MoveManager Instance { get => instance; private set => instance = value; }

    void Awake()
    {
       if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void AddRequest(Unit unit)
    {
        if (unit == null || !unit.isActiveAndEnabled || unit.HP <= 0 ||
            PhaseManager.Instance.CurrentState != PhaseState.Battle) return;
        if(!noneReadyRequests.Contains(unit))
            noneReadyRequests.Enqueue(unit);
    }
    void Update()
    {
        while(noneReadyRequests.Count > 0)
        {
            curRequest = noneReadyRequests.Dequeue();
            if (curRequest == null || !curRequest.isActiveAndEnabled || curRequest.HP <= 0 ||
                PhaseManager.Instance.CurrentState != PhaseState.Battle) continue;
            SetNextNode(curRequest);
            ReadyRequests.Enqueue(curRequest);
        }
        while(ReadyRequests.Count > 0)
        {
            curRequest = ReadyRequests.Dequeue();
            if (curRequest != null && curRequest.isActiveAndEnabled && curRequest.HP > 0 &&
                PhaseManager.Instance.CurrentState == PhaseState.Battle)
                curRequest.isMoveReady = true;
        }
    }
    
    public void SetNextNode(Unit request)
    {
        request.SetTarget();
        request.nextNodeIndex = request.NodeIndex;
        if (request.TargetIndex == 0 || request.DetectTarget()) return;
        int nextIndex = TileManager.Instance.FindPath(request.NodeIndex, request.TargetIndex, request.prvNodeIndex);
        if (nextIndex == 0 || !TileManager.Instance.IsTileStandable(nextIndex)) return;
        request.nextNodeIndex = nextIndex;
        ReserveTile(nextIndex);
    }

    public void CancelRequest(Unit unit)
    {
        if (unit.isMoveReady && unit.nextNodeIndex != 0 && unit.nextNodeIndex != unit.NodeIndex)
            EmeptyTile(unit.nextNodeIndex);
        unit.isMoveReady = false;
        unit.nextNodeIndex = unit.NodeIndex;
        int count = noneReadyRequests.Count;
        for (int i = 0; i < count; i++)
        {
            Unit queued = noneReadyRequests.Dequeue();
            if (queued != unit) noneReadyRequests.Enqueue(queued);
        }
    }

    public void ReserveTile(int tileIndex)
    {
        TileManager.Instance.SetPosCost(tileIndex, TileManager.Instance.ObstacleCost);
    }
    public void EmeptyTile(int tileIndex)
    {
        TileManager.Instance.SetPosCost(tileIndex, 0);
    }

}
