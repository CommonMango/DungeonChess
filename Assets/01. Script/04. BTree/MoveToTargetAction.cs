using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;


[Serializable, GeneratePropertyBag] 
[NodeDescription(name: "MoveToTarget", story: "[Self] Move To [meleeAttacker] targetdirection with [Rigid]", category: "Action", id: "3a37c984bafbb91816a6766b4564a974")]
public partial class MoveToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<MeleeAttacker> meleeAttacker;
    [SerializeReference] public BlackboardVariable<Vector2> targetdirection;
    [SerializeReference] public BlackboardVariable <Rigidbody2D> Rigid;
    
    protected override Status OnStart()
    {
        targetdirection.Value = meleeAttacker.Value.TargetPosition.normalized;
        return Status.Running;
    }

    protected override Status OnUpdate()
    { 
        Rigid.Value.MovePosition
        (Rigid.Value.position + targetdirection.Value * meleeAttacker.Value.Stats.MoveSpeed * Time.deltaTime);

        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

