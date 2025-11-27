using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
   [SerializeField] GameObject enemy;

    void Start()
    {
       
            SpawnUnit();
    }
    
    public int GetPossibleindex()
    {
        int startIndex = 1;
        int EndIndex = 12;
        int resultIndex = 0;
        
        for (int i = startIndex; i <= EndIndex; i++)
        {
            var target = PathFinder.Instance.GetTileNodeByIndex(i);

            if( target.cost == 0)
            {
                resultIndex = i;
                break;
            }  
        }
        if(resultIndex == 0)
            Debug.LogError("가능한 위치를 찾지 못함");
        return resultIndex; 
    }

    //스폰 가능한 위치를 찾아서 스폰하는 매서드  
    private void SpawnUnit()
    {
        int spawnIndex = GetPossibleindex();
        var spawnNode = PathFinder.Instance.GetTileNodeByIndex(spawnIndex);
       
        Instantiate(enemy, spawnNode.pos , Quaternion.identity);
        
        PathFinder.Instance.SetPosCost(spawnIndex, PathFinder.Instance.ObstacleCost);        
    }
}
