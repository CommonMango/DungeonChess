using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AugmentCatalog", menuName = "Dungeon Chess/Augments/Catalog")]
public class AugmentCatalog : ScriptableObject
{
    [SerializeField] private List<AugmentData> augments = new List<AugmentData>();
    public IReadOnlyList<AugmentData> Augments => augments.AsReadOnly();
    public void Register(AugmentData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (augments.Contains(data)) return;
        if (string.IsNullOrWhiteSpace(data.augmentId) || augments.Exists(a => a != null && a.augmentId == data.augmentId)) throw new InvalidOperationException("증강 ID가 없거나 중복됩니다.");
        augments.Add(data);
    }
    public void Unregister(AugmentData data) => augments.Remove(data);
    public void Validate()
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var data in augments)
        {
            if (data == null) throw new InvalidOperationException("누락된 증강 SO가 있습니다.");
            data.Validate();
            if (!ids.Add(data.augmentId)) throw new InvalidOperationException("중복 증강 ID: " + data.augmentId);
        }
    }
}
