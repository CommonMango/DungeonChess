using UnityEngine;


public abstract class Unit : MonoBehaviour
{
    //▼ 이름 
    protected string unitName; 
    //▼ 행동 범위
    protected float interactRange;  
    //▼ 사용 가능한 스킬 리스트
    protected Skill[] useSkill;
    //▼ 유닛의 데이터를 담은 SO  
    [SerializeField] protected UnitData unitData;
    //▼최대 체력
    protected float unitMaxHp;
    //▼현재 체력
    protected float unitCurHp;
    //▼ 최대 공격력 
    protected float maxBaseAttackPower;
    //▼ 현재 공격력 
    protected float curBaseAttackPower;
    //▼ 최대 이동속도
    protected float unitMaxmoveSpeed; 
    //▼ 현재 이동속도
    protected float unitCurmoveSpeed; 
    //▼ 현재 유닛 상태 
    protected UnitState currentState;
    //▼ 타깃 위치
    protected Vector2 targetPosition;

    protected virtual void Awake()
    {
        unitName = unitData.unitName;
        unitMaxHp = unitData.unitMaxHp;
        maxBaseAttackPower = unitData.maxBaseAttackPower;
        unitMaxmoveSpeed = unitData.maxMoveSpeed;
        interactRange = unitData.interactRange;
    }

    /// <summary>
    /// 공격 메서드들 
    /// </summary>
    protected abstract void BaseAttack();

    protected abstract void skillAttack();
    

    /// <summary>
    /// 일시적으로 스텟을 바꿔주는 메서드 
    /// </summary>
    /// <param name="changeStat"> 바꿀 스텟</param>
    /// <param name="changeAmount"> 변경될 양</param>
    public void ChangeBattleStat(statType changeStat, float changeAmount)
    {
        switch (changeStat)
        {
            case statType.moveSpeed:
                unitCurmoveSpeed += changeAmount;
                break;
            case statType.baseAttackPower:
                curBaseAttackPower += changeAmount;
                break;
            case statType.HP:
                unitCurHp += changeAmount;
                break;
        }
    }

    /// <summary>
    /// 영구적으로 스텟을 바꿔주는 메서드 
    /// </summary>
    /// <param name="changeStat"> 바꿀 스텟</param>
    /// <param name="changeAmount">변경될 양</param>
    public void UpgradeState(statType changeStat, float changeAmount)
    {
         switch (changeStat)
        {
            case statType.moveSpeed:
                unitMaxmoveSpeed += changeAmount;
                break;
            case statType.baseAttackPower:
                maxBaseAttackPower += changeAmount;
                break;
            case statType.HP:
                unitMaxHp += changeAmount;
                break;
        }
    }
}




