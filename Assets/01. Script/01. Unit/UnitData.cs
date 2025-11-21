using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "Unit")]
public class UnitData : ScriptableObject
{
    //▼ 이름 
    [SerializeField] public string unitName; 
    //▼최대 체력
    [SerializeField] public float unitMaxHp;
    //▼ 이동 속도
    [SerializeField] public float maxMoveSpeed; 
    //▼ 행동 범위
    [SerializeField] public float interactRange;  
    //▼ 기본 공격력
    [SerializeField] public float maxBaseAttackPower;
    //▼ 사용 가능한 스킬 리스트
    [SerializeField] public Skill[] useSkill;
    //▼ 유닛의 모습을 담은 프리팹 
    [SerializeField] public GameObject gameObject;
    
}




