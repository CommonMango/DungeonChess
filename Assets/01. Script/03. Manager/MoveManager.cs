using System.Collections.Generic;

/// <summary>
/// 요청자들에게 가야할 노드를 순서대로 알려주는 매니저
/// </summary>
public class MoveManager : SingleTon<MoveManager>
{
    private Queue<Unit> noneReadyRequests = new Queue<Unit>();
    private Queue<Unit> ReadyRequests = new Queue<Unit>();
    private Unit curRequest;

    public void AddRequest(Unit unit)
    {
        if(!noneReadyRequests.Contains(unit))
            noneReadyRequests.Enqueue(unit);
    }
    void Update()
    {
        while(noneReadyRequests.Count > 0)
        {
            curRequest = noneReadyRequests.Dequeue();
            SetNextNode(curRequest);
            ReadyRequests.Enqueue(curRequest);
        }
        while(ReadyRequests.Count > 0)
        {
            curRequest = ReadyRequests.Dequeue();
            curRequest.isMoveReady = true;   
        }
    }
    
    public void SetNextNode(Unit request)
    {
        request.nextNodeIndex = PathFinder.Instance.FindPath(request.NodeIndex, request.TargetIndex, request.prvNodeIndex);
        ReserveTile(request.nextNodeIndex);
    }

    public void ReserveTile(int tileIndex)
    {
        PathFinder.Instance.SetPosCost(tileIndex, PathFinder.Instance.ObstacleCost);
    }
    public void EmeptyTile(int tileIndex)
    {
        PathFinder.Instance.SetPosCost(tileIndex, 0);
    }

}
