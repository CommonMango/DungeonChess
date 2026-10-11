using UnityEngine;

// Each unit should keep its own instance; the SO only supplies shared configuration.
public abstract class SkillBase
{
    public Skill Data { get; private set; }
    internal void Initialize(Skill data) => Data = data;
    public abstract void Execute(Unit caster, Unit target);
}
