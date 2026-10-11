using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public enum statType
{
    HP,
    moveSpeed,
    baseAttackPower,
    baseAttackSpeed
}

public abstract partial class Unit
{
    //▼ 이름 
    private string unitName; 
    //▼최대 체력
    private float maxHp;
    //▼현재 체력
    private float curHp;
    public float HP => curHp;
    //▼ 최대 공격력 
    private float maxBaseAttackPower;
    //▼ 현재 공격력 
    private float curBaseAttackPower;
    public float Power => curBaseAttackPower; 
    //▼ 최대 이동속도
    private float maxmoveSpeed; 
    //▼ 현재 이동속도
    private float curmoveSpeed; 
    public float MoveSpeed => curmoveSpeed;
    //▼ 최대 공격속도
    private float maxAttackSpeed; 
    //▼ 현재 공격속도
    private float curAttackSpeed; 
    public float AttackSpeed => curAttackSpeed;
    
    protected virtual void Start() 
    {
        InitCurStat(); 

    }
    
    private void InitMaxStat()
    {
        maxmoveSpeed = unitData.maxMoveSpeed;
        maxAttackSpeed = unitData.unitMaxAttackSpeed;
        maxBaseAttackPower = unitData.maxBaseAttackPower;
        maxHp = unitData.unitMaxHp;
        curHp = maxHp;
        curBaseAttackPower = maxBaseAttackPower;
        name = unitData.unitName;
        cost = unitData.cost;
    }
    
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
                curmoveSpeed += changeAmount;
                break;
            case statType.baseAttackPower:
                curBaseAttackPower += changeAmount;
                break;
            case statType.HP:
                curHp += changeAmount;
                break;
            case statType.baseAttackSpeed:
                curAttackSpeed += changeAmount;
                break;
            default:
                Debug.LogError("잘못된 스텟타입");
                break;
        }
    }
    /// <summary>
    /// 영구적으로 스텟을 바꿔주는 메서드 
    /// </summary>
    /// <param name="changeStat"> 바꿀 스텟</param>
    /// <param name="changeAmount">변경될 양</param>
    public void UpgradeStat(statType changeStat, float changeAmount)
    {
         switch (changeStat)
        {
            case statType.moveSpeed:
                maxmoveSpeed += changeAmount;
                break;
            case statType.baseAttackPower:
                maxBaseAttackPower += changeAmount;
                break;
            case statType.HP:
                maxHp += changeAmount;
                break;
            case statType.baseAttackSpeed:
                curAttackSpeed += changeAmount;
                break;
            default:
                Debug.LogError("잘못된 스텟타입");
                break;
            
        }
    }

    public void TakeDamage(float damage)
    {
        if (HP <= 0 || !isActiveAndEnabled ||
            PhaseManager.Instance.CurrentState != PhaseState.Battle || damage <= 0) return;

        curHp = Mathf.Max(0, curHp - damage);
        if (curHp == 0)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    public void InitCurStat()
    {
        curAttackSpeed = maxAttackSpeed;
        curBaseAttackPower = maxBaseAttackPower;
        curHp = maxHp;
        curmoveSpeed = maxmoveSpeed;
    }



}
