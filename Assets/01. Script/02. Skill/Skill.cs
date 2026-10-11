using UnityEngine;

public enum SkillKind { Active, Passive }

[CreateAssetMenu(fileName = "NewSkill", menuName = "Dungeon Chess/Skills/Skill")]
public class Skill : ScriptableObject
{
    [Tooltip("카탈로그에서 찾는 고유 ID. 복제한 SO는 다른 ID를 지정하세요.")]
    public string skillId;
    [Tooltip("SkillManager에 등록한 코드 구현 ID. 같은 구현을 여러 스킬 SO에서 사용할 수 있습니다.")]
    public string implementationId;
    public string Skillname;
    [TextArea(3, 8)] public string SkillDescription;
    public SkillKind kind;
    public Sprite icon;
    [Min(0)] public float cooldown;
    [Min(0)] public float skillDamage;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(skillId)) skillId = System.Guid.NewGuid().ToString("N");
    }
}
