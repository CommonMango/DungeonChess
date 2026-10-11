using UnityEngine;

public class MeleeAttacker : Unit, IObserveStageChange
{
    private float nextAttackTime;
    private void OnEnable()
    {
        if(unitData.isAlly)
        {
            UnitManager.Instance.AddAlly(this);
        
        }
        else
        {
            UnitManager.Instance.AddEnemy(this);
            
        }
        PhaseManager.Instance.AddSubScriber(this);
    }
    protected override void Start()
    {
        base.Start();
    }
    void OnDisable()
    {
        MoveManager.Instance.CancelRequest(this);
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
                nextAttackTime = Time.time;
                BTree.SetVariableValue<bool>("IsBattlePhase", isBattlePhaseStart);
                unitCol.isTrigger = true;
                SetTarget();

            }
        else
        {
            isBattlePhaseStart = false;
            BTree.SetVariableValue<bool>("IsBattlePhase", isBattlePhaseStart);
            unitCol.isTrigger = false;
            MoveManager.Instance.CancelRequest(this);
            // Cancel the pending physics step without snapping back to the previous tile.
            if (TryGetComponent<Rigidbody2D>(out var rigid))
            {
                rigid.MovePosition(rigid.position);
                rigid.linearVelocity = Vector2.zero;
            }
        }
        DetectTarget();
    }

    public override void BaseAttack()
    {   
        if (Time.time < nextAttackTime || !DetectTarget()) return;

        // A zero attack speed in unfinished unit data uses one attack per second.
        float attacksPerSecond = AttackSpeed > 0f ? AttackSpeed : 1f;
        nextAttackTime = Time.time + 1f / attacksPerSecond;
        targetUnit.TakeDamage(Power);
    }

     public override void SkillAttack()
     {
        Debug.Log ("스킬 공격함");
        //그냥 좀 더 쎈 공격 베이스랑 같음 
     }

    
}
