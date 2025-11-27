using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BaseAttack", story: "[Self] Attack [Target] with [MeleeAttacker]", category: "Action", id: "34c5085075d7153b70675f15634db451")]
public partial class BaseAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<MeleeAttacker> MeleeAttacker;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        MeleeAttacker.Value.BaseAttack();
        return Status.Success;
    }

    protected override void OnEnd()
    {
        MeleeAttacker.Value.DetectTarget();
    }
}

