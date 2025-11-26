using UnityEngine;

public class MeleeAttacker : Unit, IObserveStageChange
{
    private void OnEnable()
    {
        PhaseManager.Instance.AddSubScriber(this);
        if(unitData.isAlly)
        {
            UnitManager.Instance.AddAlly(this);
        }
        else
        UnitManager.Instance.AddEnemy(this);
    }
    protected override void Start()
    {
        base.Start();
        SetTarget();
    }
    void OnDisable()
    {
        PhaseManager.Instance.RmvSubScriber(this);
         if(unitData.isAlly)
        {
            UnitManager.Instance.RmvAlly(this);   
        }
        else
        UnitManager.Instance.RmvEnemy(this);
    }
    
    public void InChangeStageState(PhaseState changeState)
    {
        if(changeState == PhaseState.Battle)
            {
                isBattlePhaseStart = true;
                BTree.SetVariableValue<bool>("IsBattlePhase", isBattlePhaseStart);
                SetTarget();
            }
        else
        {
            isBattlePhaseStart = false;
            BTree.SetVariableValue<bool>("IsBattlePhase", isBattlePhaseStart);
            SetTarget();
        }
    }

    public override void BaseAttack()
    {
        
        Debug.Log("기본 공격함");
    }

    public override void SkillAttack()
    {
       Debug.Log ("스킬 공격함");
       //그냥 좀 더 쎈 공격 베이스랑 같음 
    }

    
}
