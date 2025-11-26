using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

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

public class PathFinder : SingleTon<PathFinder>
{
    private Dictionary<int , TileNode> standAbleTiles = new(); //노드인덱스를 키로 노드정보를 가져오는 딕셔너리 
    [SerializeField] int maxX = 11; //x최대 좌표 
    public int MaxX => maxX;
    [SerializeField] int maxY = 21; //y최대 좌표
    public int MaxY => maxY;
    [SerializeField] int minX = -11; //x최소 좌표
    public int MinX => minX;
    [SerializeField] int minY = -25; //y최소 좌표
    public int MinY => minY; 
    [SerializeField] int startIndex = 1; // 시작 인덱스 
    [SerializeField] int tileGap = 2; //인접한 타일간 좌표 차이
    private float obstacleCost = float.MaxValue/2;
    public float ObstacleCost => obstacleCost;
    void Awake()
    {
        InitTiles();
    }
    
    //Tiles 첫 초기화 
    public void InitTiles()
    {
        int index = startIndex;

        for (int y = maxY; y >= minY; y -= tileGap)
        {
            for(int x = minX; x <= maxX; x += tileGap )
            {
                standAbleTiles[index++] = new TileNode(new Vector2(x,y));
            }
        }  
    }
    
    /// <summary>
    /// 다음에 가야할 노드를 찾는 메서드
    /// </summary>
    /// <param name="curIndex">움직일 객체의 노드 인덱스</param>
    /// <param name="targetIndex">가야할 곳의 노드 인덱스</param>
    /// <returns></returns>
    public int FindPath(int curIndex, int targetIndex)
    {
        List<int>aroundList = GetAroundIndex(curIndex);
        CalcH(aroundList,targetIndex);
        int resultIndex = FinalDesNode(aroundList);
        InitHValue(aroundList);
        return resultIndex;
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
    /// 모든 Vector2를 리스트로 가져옴 
    /// </summary>
    /// <returns></returns>
    public List<Vector2> GetAllVector2List()
    {
        List<Vector2> vecPosition  = new();
        foreach(var tiles in standAbleTiles.Values)
        {
            vecPosition.Add(tiles.pos);
        } 
        return vecPosition;
    }

    /// <summary>
    /// Vector2를 통해서 Index받기
    /// /2는 좌표간 간격만큼 나눠준 것
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public int GetIndexByVector2(Vector2 target)
    {
        float width = (maxX - minX + tileGap)/tileGap; 
        float col = (target.x - minX) / tileGap; 
        float row = (maxY - target.y) / tileGap; 
        float index = row * width + col + startIndex;
        
        return (int)index;
    }

    //원하는 타일의 비용을 가져옴 
    public TileNode GetTileNodeByIndex(int index)
    {
        var target = standAbleTiles[index];
        return target;
    }

    /// <summary>
    /// 원하는 타일 비용 변경
    /// </summary>
    /// <param name="index">//원하는 타일 인덱스</param>
    /// <param name="cost">//변경될 비용 </param>
    public void SetPosCost(int index, float cost)
    {
        var temp = standAbleTiles[index];
        temp.cost = cost;
        standAbleTiles[index] = temp;
    }

    public Vector2 ConvertPositionToCloseNode(Vector2 pos)
    {
        float x = Mathf.Round((pos.x - minX) / tileGap) * tileGap + minX;
        float y = Mathf.Round((pos.x - minY) / tileGap) * tileGap + minY;
        Vector2 CloseNode = new Vector2(x,y);
        return CloseNode;
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

    public void InitHValue(List<int>aroundNode)
    {
        foreach(var element in aroundNode)
        {
            var temp = standAbleTiles[element];
            if(temp.distance != 0)
            {
                temp.distance = 0;
            }
        }
    }

}