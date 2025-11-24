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
            if(detcoroutine == null)
            {
                StartCoroutine(DetectTarget());
                Debug.Log("시작함");
            }
        else
            isBattlePhaseStart = false;
            if(detcoroutine != null)
                StopCoroutine(DetectTarget());
    }

    public override void BaseAttack()
    {
       Debug.Log("기본 공격함");
    }

    public override void SkillAttack()
    {
       Debug.Log ("스킬 공격함");
    }

    
}
