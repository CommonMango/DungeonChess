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
            isBattlePhaseStart = true;
            BTree.SetVariableValue<bool>("IsBattlePhase", isBattlePhaseStart);
            if(detcoroutine == null)
            {
                StartCoroutine(DetectTarget());
                Debug.Log(MoveSpeed);
            }
        else
            isBattlePhaseStart = false;
            BTree.SetVariableValue<bool>("IsBattlePhase", isBattlePhaseStart);
            if(detcoroutine != null)
                StopCoroutine(DetectTarget());
    }

    public override void BaseAttack()
    {
        StopCoroutine(DetectTarget());
        Debug.Log("기본 공격함");
    }

    public override void SkillAttack()
    {
       Debug.Log ("스킬 공격함");
       //그냥 좀 더 쎈 공격 베이스랑 같음 
    }

    
}
