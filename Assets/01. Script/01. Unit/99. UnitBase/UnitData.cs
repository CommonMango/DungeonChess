using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "Unit")]
public class UnitData : ScriptableObject
{
    //▼ 이름 
    [SerializeField] public string unitName; 
    [Tooltip("증강 대상 분류. 예: melee, ranged. 대소문자를 구분합니다.")]
    public string[] augmentTags = new string[0];
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
    //▼ 기본 공격 속도
    [SerializeField] public float unitMaxAttackSpeed;
    [Min(0), Tooltip("패배 시 이 적이 생존해 있으면 받는 의지 피해. 일반 5, 원거리/마법사 7, 강한 전열 8, 엘리트 15.")]
    public int willDamage = 5;
    [Tooltip("보스는 일반 생존 적 의지 피해 합산에서 제외합니다. 별도 피해 규칙은 아직 미구현입니다.")]
    public bool isBoss;
    //▼ 동료 여부
    [SerializeField] public bool isAlly;
    //▼ 아군이라면 비용
    [SerializeField] public float cost;
    [SerializeField] public LayerMask targetLayer;
 

    
    
}




