using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 유닛과 해당 유닛위치의 인덱스를 관리하는 매니저 
/// </summary>

public class UnitManager : SingleTon<UnitManager>
{
    [SerializeField] private Dictionary<Unit, int> AllyUnits = new();
    [SerializeField] private Dictionary<Unit, int> EnemyUnits = new();
   
    [SerializeField] private GameObject Ally;
    [SerializeField] private GameObject Enemy;
    public void RmvAlly(Unit ally) => AllyUnits.Remove(ally); 
    public void RmvEnemy(Unit enemy) => AllyUnits.Remove(enemy);
        
    void Awake()
    {
        SingletonInit();
    }
   
    public void AddAlly(Unit ally)
    {
        AllyUnits[ally] = ally.NodeIndex;
        ally.transform.SetParent(Ally.transform);
    }
    public void AddEnemy(Unit enemy)
    {
        EnemyUnits[enemy] = enemy.NodeIndex; 
        enemy.transform.SetParent(Enemy.transform);
    }
    public void updateIndex(Unit unit)
    {
        if(AllyUnits.ContainsKey(unit))
        {
            AllyUnits[unit] = unit.NodeIndex;
        }
        else
        {
            EnemyUnits[unit] = unit.NodeIndex;
        }
    } 

    public int SetTargetIndex(int selfIndex, bool isAlly)
    {
        float distance;
        float minDistance = float.MaxValue; 
        var selfUnit = PathFinder.Instance.GetTileNodeByIndex(selfIndex);
        int targetIndex = 0;
        
        if(EnemyUnits.Count == 0 || AllyUnits.Count == 0) //둘 중 하나의 딕셔너리가 비어있으면 
        {
            return targetIndex; //0을 반환
        }
        
        if (isAlly) //아군이면
        {
            foreach(var index in EnemyUnits.Values)
            {
                var targetUnit = PathFinder.Instance.GetTileNodeByIndex(index);
                distance = (targetUnit.pos - selfUnit.pos).sqrMagnitude; 
                
                if(minDistance > distance);
                {
                    targetIndex = index; 
                }
            }
        } 
        else //적군이면
        {
            foreach(var index in AllyUnits.Values)
            {
                var targetUnit = PathFinder.Instance.GetTileNodeByIndex(index);
                distance = (targetUnit.pos - selfUnit.pos).sqrMagnitude; 
                
                if(minDistance > distance);
                {
                    targetIndex = index; 
                }
            }
        }
        return targetIndex;
        
    }     
}
