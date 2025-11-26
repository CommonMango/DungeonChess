using System.Collections;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public abstract partial class Unit : MonoBehaviour
{
    //▼ 사용 가능한 스킬 리스트
    private Skill[] useSkill;
    //▼ 행동트리 관리용 
    [SerializeField] protected BehaviorGraphAgent BTree;
    //▼ 유닛의 데이터를 담은 SO  
    [SerializeField] protected UnitData unitData;
    
    //▼ 감지 범위
    protected float detectRange; 
    
    //▼상호작용 범위
    protected float interactRange;  
    
    //▼ 감지 코루틴 변수
    protected Coroutine detcoroutine;
    //▼ 타깃 위치 변수와 프로퍼티 
    protected Vector2 targetPosition;
    public Vector2 TargetPosition => targetPosition;
    //▼ 감지된 콜라이더 배열
    [SerializeField]protected Collider2D[] colliders;
    //▼ 타깃 레이어 
    protected LayerMask targetLayerMask;
    //▼ 전투 시작 전달용 변수
    protected bool isBattlePhaseStart;
    public bool IsBattlePhaseStart => isBattlePhaseStart;
    //▼ 타겟 감지 여부 전달용 변수
    protected bool isTargetInInteractRange;
    public bool IsTargetInInteractRange => isTargetInInteractRange;
    //▼ SP활성화 여부 전달용 변수
    protected bool isSPFull;
    public bool IsSPFull => isSPFull;

    protected int curNodeIndex;
    public int NodeIndex => curNodeIndex;

    protected virtual void Awake()
    {
        UnitInit();
        InitMaxStat();
    }
    
    protected void UnitInit()
    {
        useSkill = (Skill[])unitData.useSkill.Clone();
        detectRange = 10f;
        interactRange = unitData.interactRange;
        targetLayerMask = unitData.targetLayer;
        curNodeIndex = PathFinder.Instance.GetIndexByVector2(transform.position);
        Debug.Log(curNodeIndex);
    }

   

    public abstract void BaseAttack();// 기본 공격
    public abstract void SkillAttack();// 스킬 공격

    /// <summary>
    /// 가장 가까운 콜라이더 찾는 메서드
    /// </summary>
    /// <param name="colliders">감지할 콜라이더 배열</param>
    /// <returns></returns>
     protected Collider2D FindNearestCollider(Collider2D[] colliders)
     {
         float minDistance = float.MaxValue;
         float distance;
         Collider2D minCol = null;

        foreach(var col in colliders)
         {
             distance = (col.transform.position - transform.position).sqrMagnitude;
           
            if (distance < minDistance)
           {
                minCol = col; 
               minDistance = distance;     
             }
        }
        return minCol;
     }
     
    public void SetTarget(Vector2 target)
    {
        targetPosition = target;
    }

    /// <summary>
    /// 타깃 감지용 코루틴 함수
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator DetectTarget()
    {
        float detectDelay = 1f;
        WaitForSeconds wfs = new WaitForSeconds(detectDelay);
        while (true)
        {
            colliders = Physics2D.OverlapCircleAll
            (  
                transform.position, 
                detectRange, 
                targetLayerMask
            );

            Collider2D near;
            float nearDistance;
            float magnitinteractRange;

            if(colliders.Length > 0)
            {
                near = FindNearestCollider(colliders);
                nearDistance = (near.transform.position - transform.position).sqrMagnitude;
                magnitinteractRange = interactRange * interactRange;
                targetPosition = near.transform.position;

                if (nearDistance <= magnitinteractRange && isTargetInInteractRange == false)
                {
                    isTargetInInteractRange = true;
                    BTree.SetVariableValue<bool>("IsTargetDetected",isTargetInInteractRange);
                    targetPosition = near.transform.position; 
                }
    
                else if (nearDistance > magnitinteractRange && isTargetInInteractRange == true)
                {
                    isTargetInInteractRange = false;
                    BTree.SetVariableValue<bool>("IsTargetDetected",isTargetInInteractRange);
                }
            }
            else
            {
                isTargetInInteractRange = false;
                BTree.SetVariableValue<bool>("IsTargetDetected",false);
            }
           
           
            
            yield return wfs;
        }       
    }    
}




