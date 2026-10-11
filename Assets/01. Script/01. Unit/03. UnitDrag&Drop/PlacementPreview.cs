using UnityEngine;

// Render-only previews never register a unit or occupy a tile.
public sealed class PlacementPreview : System.IDisposable
{
    private readonly GameObject root;
    private readonly Material material;
    private readonly Mesh mesh;

    public PlacementPreview(GameObject unitPrefab)
    {
        root = new GameObject("PlacementPreview");
        var visual = CopyVisual(unitPrefab.transform, root.transform);
        visual.localPosition = Vector3.zero;
        var marker = new GameObject("PlacementTile", typeof(MeshFilter), typeof(MeshRenderer));
        marker.transform.SetParent(root.transform, false);
        marker.transform.localScale = Vector3.one * TileManager.Instance.TileGap;
        mesh = new Mesh { name = "PlacementTile" };
        mesh.vertices = new[] { new Vector3(-.5f,-.5f), new Vector3(.5f,-.5f), new Vector3(.5f,.5f), new Vector3(-.5f,.5f) };
        mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
        mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateBounds();
        marker.GetComponent<MeshFilter>().sharedMesh = mesh;
        material = new Material(Resources.Load<Shader>("PlacementTile"));
        var renderer = marker.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.sortingOrder = 100;
        root.SetActive(false);
    }

    private static Transform CopyVisual(Transform source, Transform parent)
    {
        var copy = new GameObject(source.name).transform;
        copy.SetParent(parent, false);
        copy.localPosition = source.localPosition;
        copy.localRotation = source.localRotation;
        copy.localScale = source.localScale;
        var original = source.GetComponent<SpriteRenderer>();
        if (original != null)
        {
            var sprite = copy.gameObject.AddComponent<SpriteRenderer>();
            sprite.sprite = original.sprite;
            sprite.sharedMaterial = original.sharedMaterial;
            sprite.color = new Color(original.color.r, original.color.g, original.color.b, .75f);
            sprite.flipX = original.flipX;
            sprite.flipY = original.flipY;
            sprite.drawMode = original.drawMode;
            sprite.size = original.size;
            sprite.sortingLayerID = original.sortingLayerID;
            sprite.sortingOrder = 101 + original.sortingOrder;
            sprite.enabled = original.enabled;
        }
        foreach (Transform child in source) CopyVisual(child, copy);
        copy.gameObject.SetActive(source.gameObject.activeSelf);
        return copy;
    }

    public void Show(int index, bool valid)
    {
        root.transform.position = TileManager.Instance.GetTileNodeByIndex(index).pos;
        material.SetColor("_Color", valid ? new Color(.15f, 1f, .45f, .8f) : new Color(1f, .18f, .18f, .8f));
        root.SetActive(true);
    }

    public void Hide() => root.SetActive(false);

    public static bool IsOverUI(Vector2 screenPosition)
    {
        var rect = UnitManager.Instance.UIRectTransform;
        if (rect == null) return false;
        var canvas = rect.GetComponentInParent<Canvas>();
        var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition, camera);
    }

    public void Dispose()
    {
        root.SetActive(false);
        Object.Destroy(root);
        Object.Destroy(material);
        Object.Destroy(mesh);
    }
}
