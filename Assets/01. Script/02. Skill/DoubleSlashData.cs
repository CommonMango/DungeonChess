using UnityEngine;

[CreateAssetMenu(fileName = "DoubleSlash", menuName = "Dungeon Chess/Skills/Double Slash")]
public class DoubleSlashData : Skill
{
    [Tooltip("한 번의 타격에 적용할 시전자 공격력 비율")]
    [Min(0)] public float attackMultiplier = 0.8f;
    [Tooltip("첫 타격과 두 번째 타격 사이의 시간 (초)")]
    [Min(0.01f)] public float hitInterval = 0.15f;
}
