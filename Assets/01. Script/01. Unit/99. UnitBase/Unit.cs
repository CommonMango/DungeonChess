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
    
    //▼상호작용 범위
    protected float interactRange; 
    
    //▼ 타깃 위치 변수와 프로퍼티 
    protected int targetIndex;
    public int  TargetIndex => targetIndex;
    //▼ 전투 시작 전달용 변수
    protected bool isBattlePhaseStart;
    public bool IsBattlePhaseStart => isBattlePhaseStart;
    //▼ SP활성화 여부 전달용 변수
    protected bool isSPFull;
    public bool IsSPFull => isSPFull;
    //▼ 현재 위치의 노드인덱스
    protected int curNodeIndex;
    public int NodeIndex => curNodeIndex;

    //움직일 준비가 되었는지 확인하는 변수 
    public bool isMoveReady = false; 

    //다음에 이동할 노드 인덱스를 받는 변수
    public int nextNodeIndex = 0; 

    //유닛이 아군이라면 비용
    public float cost;

    //타깃 유닛
    protected Unit targetUnit;


    protected virtual void Awake()
    {
        UnitInit();
        InitMaxStat();
        
    }
    
    protected void UnitInit()
    {
        useSkill = (Skill[])unitData.useSkill.Clone();
        interactRange = unitData.interactRange;
        curNodeIndex = PathFinder.Instance.GetIndexByVector2(transform.position);
    }

    public abstract void BaseAttack();// 기본 공격
    public abstract void SkillAttack();// 스킬 공격

    public void SetTarget() //타겟 노드 설정
    {
        targetIndex = UnitManager.Instance.SetTargetIndex(curNodeIndex,unitData.isAlly);
        targetUnit = UnitManager.Instance.FindTargetIndex(TargetIndex,!unitData.isAlly); 
    }

    public void SetCurTile(int changeIndex) //노드 인덱스를 새로 설정 
    {
        curNodeIndex = changeIndex;
        UnitManager.Instance.updateIndex(this);

        if (PathFinder.Instance.GetAroundIndex(targetIndex).Contains(changeIndex))
        {
            BTree.SetVariableValue<bool>("IsTargetDetected",true);
        }
    }
    
}




