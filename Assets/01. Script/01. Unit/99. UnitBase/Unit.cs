using Unity.Behavior;
using UnityEngine;

public abstract partial class Unit : MonoBehaviour
{
    //▼ 사용 가능한 스킬 리스트
    private Skill[] useSkill;
    //▼ 행동트리 관리용 
    [SerializeField] protected BehaviorGraphAgent BTree;
    //▼ 유닛의 데이터를 담은 SO  
    [SerializeField] public UnitData unitData; //잠깐 열음 
    
    [SerializeField] public Collider2D[] colliders;
    [SerializeField] protected CapsuleCollider2D unitCol; 
    //▼상호작용 범위
    protected float interactRange; 
    
    //▼ 타깃 위치 변수와 프로퍼티 
    protected int targetIndex;
    public int  TargetIndex => targetIndex;
    //▼ 전투 시작 전달용 변수
    protected bool isBattlePhaseStart;
    public bool IsBattlePhaseStart => isBattlePhaseStart;
    //▼ 현재 위치의 노드인덱스
    protected int curNodeIndex;
    public int NodeIndex => curNodeIndex;

    //움직일 준비가 되었는지 확인하는 변수 
    public bool isMoveReady = false; 

    //다음에 이동할 노드 인덱스를 받는 변수
    public int nextNodeIndex = 0; 

    //유닛이 아군이라면 비용
    public float cost;

    //바로 직전에 간 타일을 기억하는 변수
    public int prvNodeIndex;

    //타깃 유닛
    protected Unit targetUnit;
    private LayerMask targetLayerMask;
    protected Vector2 targetPosition;
    protected bool isTargetInInteractRange;

    protected virtual void Awake()
    {
        UnitInit();
        InitMaxStat();
    }
    
    protected void UnitInit()
    {
        useSkill = (Skill[])unitData.useSkill.Clone();
        interactRange = unitData.interactRange;
        curNodeIndex = TileManager.Instance.GetIndexByVector2(transform.position);
        isTargetInInteractRange = false;
        targetLayerMask = unitData.targetLayer;
        prvNodeIndex = 0;
    }

    public abstract void BaseAttack();// 기본 공격
    public abstract void SkillAttack();// 스킬 공격

    public void SetTarget() //타겟 노드 설정
    {
        targetIndex = UnitManager.Instance.SetTargetIndex(curNodeIndex, unitData.isAlly);
        //targetUnit = UnitManager.Instance.FindTargetIndex(TargetIndex,!unitData.isAlly); 
    }

    public void SetCurTile(int changeIndex) //노드 인덱스를 새로 설정 
    {
        prvNodeIndex = curNodeIndex;
        curNodeIndex = changeIndex;
        UnitManager.Instance.updateIndex(this);

    }

    /// <summary>
    /// 주변 콜라이더 찾는 메서드 
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
             if (col == null) continue;
             Unit candidate = col.GetComponentInParent<Unit>();
             if (candidate == null || candidate == this || !candidate.isActiveAndEnabled ||
                 candidate.HP <= 0 || candidate.unitData.isAlly == unitData.isAlly) continue;
             distance = (candidate.transform.position - transform.position).sqrMagnitude;
           
            if (distance < minDistance)
           {
                minCol = col; 
               minDistance = distance;     
             }
        }
        return minCol;
     }
     

    /// <summary>
    /// 타깃 감지용 코루틴 함수
    /// </summary>
    /// <returns></returns>
    public virtual bool DetectTarget()
    {
        targetUnit = null;
        isTargetInInteractRange = false;
        if (isActiveAndEnabled && HP > 0 && isBattlePhaseStart &&
            PhaseManager.Instance.CurrentState == PhaseState.Battle)
        {
            float boxCoor = TileManager.Instance.TileGap * (interactRange * 2 + 1);
            colliders = Physics2D.OverlapBoxAll(transform.position,
                new Vector2(boxCoor, boxCoor), 0, targetLayerMask);
            Collider2D near = FindNearestCollider(colliders);
            if (near != null)
            {
                targetUnit = near.GetComponentInParent<Unit>();
                targetPosition = targetUnit.transform.position;
                isTargetInInteractRange = true;
            }
        }
        BTree.SetVariableValue<bool>("IsTargetDetected", isTargetInInteractRange);
        return isTargetInInteractRange;
    }
}
