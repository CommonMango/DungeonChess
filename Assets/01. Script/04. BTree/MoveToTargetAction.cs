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
    [SerializeReference] public BlackboardVariable<Vector2> targetdirection; //바꿔야하는데 건들이기 무섭다..
    [SerializeReference] public BlackboardVariable <Rigidbody2D> Rigid;
    private bool isRequest = false; 
    private Vector2 targetVector;
    private Vector2 curVector;
    private Vector2 nextPos;
    private bool isMoving;
    protected override Status OnStart()
    {
        curVector = Self.Value.transform.position;

        if(!isRequest)
        {
            MoveManager.Instance.AddRequest(meleeAttacker.Value); 
            isRequest = true;  
        }
        return Status.Running;
    }

    protected override Status OnUpdate()
    { 
        if(!meleeAttacker.Value.isMoveReady)
            return Status.Running;
        
        else if(meleeAttacker.Value.nextNodeIndex != meleeAttacker.Value.NodeIndex)//이동을 아직 마치지 못했다면 
        {  
            if(!isMoving)
            {
                isMoving = true;
                targetVector = PathFinder.Instance.GetTileNodeByIndex(meleeAttacker.Value.nextNodeIndex).pos;
                targetdirection.Value = (targetVector - curVector).normalized; 
            }
            curVector = Self.Value.transform.position;
            nextPos = curVector + targetdirection.Value * meleeAttacker.Value.MoveSpeed * Time.deltaTime;
            if(Vector2.Distance(curVector, targetVector) <= meleeAttacker.Value.MoveSpeed * Time.deltaTime)
            {
                nextPos = targetVector;
                meleeAttacker.Value.SetCurTile(meleeAttacker.Value.nextNodeIndex);
            }

            Rigid.Value.MovePosition(nextPos);
            
            meleeAttacker.Value.isMoveReady = false;
            return Status.Running;
        }
        else
        {
            isMoving = false;
            return Status.Success; 
        }
    }

    protected override void OnEnd()
    {
        isRequest = false;
    }
}

