using System.Collections.Generic;
using UnityEngine;

public class UnitManager : SingleTon<UnitManager>
{
   [SerializeField] private List<Unit> AllyUnits;
   [SerializeField] private List<Unit> EnemyUnits;

    public void AddAlly(Unit ally) => AllyUnits.Add(ally);
    public void RmvAlly(Unit ally) => AllyUnits.Remove(ally); 
    public void AddEnemy(Unit enemy) => EnemyUnits.Add(enemy); 
    public void RmvEnemy(Unit enemy) => AllyUnits.Add(enemy);

    void Awake()
    {
        SingletonInit();
    }

}
