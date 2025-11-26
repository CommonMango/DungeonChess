using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill")]
public class Skill : ScriptableObject
{
    public string Skillname; //이름 
    public string SkillDescription; //설명
    public float skillDamage; //데미지         
}
