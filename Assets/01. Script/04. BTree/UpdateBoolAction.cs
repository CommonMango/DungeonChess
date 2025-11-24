using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "UpdateBool", story: "Update [IsBattlePhase] [IsSPFull] [IsTargetDetected] with [meleeAttacker]", category: "Action", id: "1021b5ccf1f5ade2f96bf7ec06dbd2b3")]
public partial class UpdateBoolAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> IsBattlePhase;
    [SerializeReference] public BlackboardVariable<bool> IsSPFull;
    [SerializeReference] public BlackboardVariable<bool> IsTargetDetected;
    [SerializeReference] public BlackboardVariable<MeleeAttacker> MeleeAttacker;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        IsBattlePhase.Value = MeleeAttacker.Value.IsBattlePhaseStart;
        IsSPFull.Value = MeleeAttacker.Value.IsSPFull;
        IsTargetDetected.Value = MeleeAttacker.Value.IsTargetInInteractRange;
        
        Debug.Log("가져온 값"+ MeleeAttacker.Value.IsTargetInInteractRange); 
        Debug.Log("변한 값" + IsBattlePhase.Value);  
        
        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

