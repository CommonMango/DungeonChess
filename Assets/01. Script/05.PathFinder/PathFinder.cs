using UnityEngine;
using System.Collections.Generic;

public struct TileNode
{
    public Vector2 pos; // 해당 위치 
    public float cost; // 해당 좌표에 무언가 있으면 cost가 늘어남 
    public float distance; // 실제 거리 
    public float fCost => cost + distance;//최종 비용

    public TileNode(Vector2 Pos, float Cost = 0, float Distance = 0)
    {
        pos = Pos;
        cost = Cost;
        distance = Distance;
    }
}

public class DefaultTiles : SingleTon<DefaultTiles>
{
    public Dictionary<int , TileNode> standAbleTiles = new(); //갈 수 있는 위치정보와 비용 리스트 테스트용으로 public
    [SerializeField] int maxX = 11;
    [SerializeField] int maxY = 21;
    [SerializeField] int minX = -11;
    [SerializeField] int minY = -25;
 
    void Awake()
    {
        InitTiles();
    }
    
    //Tiles 첫 초기화 
    public void InitTiles()
    {
        int index = 1;
        for (int y = minY; y <= maxY; y += 2)
        {
            for(int x = minX; x <= maxX; x += 2 )
            {
                standAbleTiles[index++] = new TileNode(new Vector2(x,y));
            }
        }  
    }

    //주변 8방향의 Index가져오기 
    public List<int> GetAroundIndex(int curIndex)
    {
        List<int>aroundNode = new List<int>();
        int gridHeight = (maxY - minY + 2)/2;
        int gridWidth = (maxX - minX + 2)/2;
        int calNode;//계산된 노드
        
        for (int dy = gridWidth; dy >= 0 - gridWidth; dy -= gridWidth )
        {
            for (int dx = -1; dx <= 1; dx++)
            {   
                calNode = dx + dy + curIndex;
                
                if(calNode <= 0 || calNode > gridHeight * gridWidth) //위 아래 범위 밖 노드 제외 
                    continue;

                else if(dx == -1  && (curIndex - 1) % gridWidth == 0) //왼쪽 범위 밖노드 제외
                    continue;
                
                else if(dx == 1 && curIndex % gridWidth == 0 )//오른쪽 범위 밖 노드 제외
                    continue;
                
                else if(dx + dy == 0) //자기 자신제외
                    continue;

                else aroundNode.Add(calNode); 
            }
        }
        return aroundNode;
    }
    
    /// <summary>
    /// Vector2를 통해서 Index받기
    /// /2는 좌표간 간격만큼 나눠준 것
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public int GetIndexByVector2(Vector2 target)
    {
    
        float width = (maxX - minX + 2)/2; 
        float col = (target.x - minX) / 2; 
        float row = (maxY - target.y) / 2; 
        float index = row * width + col + 1;
        
        return (int)index;
    }

    // 원하는 타일 비용 변경 
    public void SetPosCost(int index, float cost)
    {
        var temp = standAbleTiles[index];
        temp.cost = cost;
        standAbleTiles[index] = temp;
    }

    //H값 구하기 
    /// <summary>
    /// 주변 노드의 H값을 갱신해준다. 
    /// </summary>
    /// <param name="aroundNode">주변 노드의 index를 담은 리스트</param>
    /// <param name="targetindex">목적지의 index값</param>
    /// <returns></returns>
    //  갈 노드 결정하면서 H값 초기화 해야한다.
    public void CalcH (List<int> aroundNode, int targetindex)
    {
        Vector2 targetVector2 = standAbleTiles[targetindex].pos;
        
        for(int i = 0; i < aroundNode.Count; i++)
        {
            var temp = standAbleTiles[aroundNode[i]] ;
            temp.distance = (targetVector2 - temp.pos).sqrMagnitude;
            standAbleTiles[aroundNode[i]] = temp;
        }
    }

    //fCost를 바탕으로 가야할 인덱스를 결정 
    public int FinalDesNode(List<int>aroundNode)
    {   
        float minCost = float.MaxValue;
        int resultIndex = 0;

        foreach(var element in aroundNode)
        {
            var temp = standAbleTiles[element];
            if(temp.fCost < minCost)
            {
                minCost = temp.fCost;
                resultIndex = element;
            }
        }
        return resultIndex; 
    }

}