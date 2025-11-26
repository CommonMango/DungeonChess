using Unity.Behavior;
using UnityEngine;


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
    protected int targetIndex;
    public int  TargetIndex => targetIndex;
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
    //▼ 현재 위치의 노드인덱스
    protected int curNodeIndex;
    public int NodeIndex => curNodeIndex;

    //움직일 준비가 되었는지 확인하는 변수 
    public bool isMoveReady = false; 

    public int nextNodeIndex = 0; 


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
    }

    public abstract void BaseAttack();// 기본 공격
    public abstract void SkillAttack();// 스킬 공격

    public void SetTarget() //타겟 노드 설정
    {
        targetIndex = UnitManager.Instance.SetTargetIndex(curNodeIndex,unitData.isAlly);
    }

    public void SetCurTile(int changeIndex) //노드 인덱스를 새로 설정 
    {
        curNodeIndex = changeIndex;
        UnitManager.Instance.updateIndex(this);
        if (PathFinder.Instance.GetAroundIndex(targetIndex).Contains(changeIndex))
        {
            BTree.SetVariableValue<bool>("IsDetected",true);
        }
    }
    
}




