using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillCatalog", menuName = "Dungeon Chess/Skills/Catalog")]
public class SkillCatalog : ScriptableObject
{
    [SerializeField] private List<Skill> skills = new List<Skill>();
    public IReadOnlyList<Skill> Skills => skills;
    public bool TryGet(string id, out Skill skill)
    {
        skill = skills.Find(s => s != null && s.skillId == id);
        return skill != null;
    }
    public void Register(Skill skill)
    {
        if (skill == null) throw new ArgumentNullException(nameof(skill));
        if (string.IsNullOrWhiteSpace(skill.skillId)) throw new InvalidOperationException("스킬 ID를 입력하세요.");
        if (skills.Contains(skill)) return;
        if (TryGet(skill.skillId, out _)) throw new InvalidOperationException("중복 스킬 ID: " + skill.skillId);
        skills.Add(skill);
    }
    public void Unregister(Skill skill) => skills.Remove(skill);
    public void Validate()
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var skill in skills)
        {
            if (skill == null) throw new InvalidOperationException("비어 있는 스킬 참조가 있습니다.");
            if (string.IsNullOrWhiteSpace(skill.skillId) || !ids.Add(skill.skillId))
                throw new InvalidOperationException("스킬 ID가 없거나 중복됩니다: " + skill.name);
            if (string.IsNullOrWhiteSpace(skill.Skillname)) throw new InvalidOperationException("스킬 이름을 입력하세요: " + skill.name);
        }
    }
}
