using System.Collections;
using UnityEngine;

public abstract class NodeEncounterHandler : ScriptableObject
{
    public abstract IEnumerator Execute(NodeRunController run, NodeMapData.Node node);
}
