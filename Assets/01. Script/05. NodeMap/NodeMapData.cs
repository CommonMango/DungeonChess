using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon Chess/Node Map")]
public class NodeMapData : ScriptableObject
{
    [Serializable]
    public class Node
    {
        public string id;
        public Vector2 position;
        public NodeTypeData type;
        public List<string> next = new List<string>();
    }
    public List<string> startNodes = new List<string>();
    public List<Node> nodes = new List<Node>();
    public Node Find(string id) => nodes.Find(n => n.id == id);
    public void Validate()
    {
        var ids = new HashSet<string>();
        foreach (var n in nodes)
            if (n == null || string.IsNullOrWhiteSpace(n.id) || !ids.Add(n.id) || n.type == null || n.type.handler == null)
                throw new InvalidOperationException("노드 ID 중복/누락 또는 노드 종류·처리기 누락입니다.");
        if (startNodes.Count == 0) throw new InvalidOperationException("시작 노드가 필요합니다.");
        foreach (var id in startNodes) if (!ids.Contains(id)) throw new InvalidOperationException("없는 시작 노드: " + id);
        foreach (var n in nodes)
        {
            var edges = new HashSet<string>();
            foreach (var id in n.next)
                if (!ids.Contains(id) || id == n.id || !edges.Add(id)) throw new InvalidOperationException("잘못된 연결: " + n.id + " → " + id);
        }
        var visited = new HashSet<string>(); var active = new HashSet<string>();
        foreach (var id in startNodes) Visit(id, visited, active);
        if (visited.Count != nodes.Count) throw new InvalidOperationException("시작점에서 도달할 수 없는 노드가 있습니다.");
    }
    private void Visit(string id, HashSet<string> visited, HashSet<string> active)
    {
        if (active.Contains(id)) throw new InvalidOperationException("노드 경로에 순환이 있습니다.");
        if (!visited.Add(id)) return;
        active.Add(id);
        foreach (var next in Find(id).next) Visit(next, visited, active);
        active.Remove(id);
    }
}
