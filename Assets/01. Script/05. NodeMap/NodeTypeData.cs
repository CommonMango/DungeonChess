using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon Chess/Node Type")]
public class NodeTypeData : ScriptableObject
{
    public string displayName = "전투";
    public Color color = new Color(.86f, .65f, .3f);
    public NodeEncounterHandler handler;
}
