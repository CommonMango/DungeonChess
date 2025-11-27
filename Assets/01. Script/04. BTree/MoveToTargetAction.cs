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
    [SerializeReference] public BlackboardVariable <Rigidbody2D> Rigid;
    private Vector2 targetdirection; 
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
        {
            return Status.Running;
        }
        
        else if(meleeAttacker.Value.nextNodeIndex != meleeAttacker.Value.NodeIndex)//이동을 아직 마치지 못했다면 
        {  
            if(!isMoving)
            {
                isMoving = true;
                targetVector = PathFinder.Instance.GetTileNodeByIndex(meleeAttacker.Value.nextNodeIndex).pos;
                targetdirection = (targetVector - curVector).normalized; 
            }
            
            curVector = Self.Value.transform.position;
            nextPos = curVector + targetdirection * meleeAttacker.Value.MoveSpeed * Time.deltaTime;
            
            if(Vector2.Distance(curVector, targetVector) <= meleeAttacker.Value.MoveSpeed * Time.deltaTime)
            {
                nextPos = targetVector;
                MoveManager.Instance.EmeptyTile(meleeAttacker.Value.NodeIndex);
                meleeAttacker.Value.SetCurTile(meleeAttacker.Value.nextNodeIndex);
                
                if (!meleeAttacker.Value.DetectTarget())
                {
                    meleeAttacker.Value.SetTarget();
                }
            }

            Rigid.Value.MovePosition(nextPos);
            return Status.Running;
        }
        else
        {
            meleeAttacker.Value.isMoveReady = false;
            isMoving = false;

            return Status.Success; 
        }
    }

    protected override void OnEnd()
    {   
        isRequest = false;
    }
}

