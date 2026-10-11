using System;
using System.Collections.Generic;
using UnityEngine;

public static class SkillManager
{
    private static readonly Dictionary<string, Func<SkillBase>> factories =
        new Dictionary<string, Func<SkillBase>>(StringComparer.Ordinal);

    public static IReadOnlyList<string> RegisteredIds
    {
        get
        {
            var ids = new List<string>(factories.Keys);
            ids.Sort(StringComparer.Ordinal);
            return ids.AsReadOnly();
        }
    }

    // Also runs when Enter Play Mode has domain reload disabled.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRegistry() => factories.Clear();

    public static void Register<T>(string id) where T : SkillBase, new()
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("스킬 구현 ID가 필요합니다.", nameof(id));
        if (factories.ContainsKey(id)) throw new InvalidOperationException("이미 등록된 스킬 구현 ID: " + id);
        factories.Add(id, () => new T());
    }

    public static bool IsRegistered(string id) => id != null && factories.ContainsKey(id);

    public static bool TryCreate(Skill data, out SkillBase skill)
    {
        skill = null;
        if (data == null || string.IsNullOrWhiteSpace(data.implementationId) ||
            !factories.TryGetValue(data.implementationId, out var factory)) return false;
        skill = factory();
        skill.Initialize(data);
        return true;
    }
}
