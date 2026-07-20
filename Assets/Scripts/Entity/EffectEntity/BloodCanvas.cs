using System.Collections.Generic;
using UnityEngine;

public class BloodCanvas : MonoBehaviour
{
    [Header("血迹贴图")]
    [SerializeField] private Sprite[] bloodSprites;
    [SerializeField] private Vector2 positionOffset = new Vector2(0.3f, 0.3f);
    [SerializeField] private float angleOffset = 15f;

    [Header("渲染")]
    [SerializeField, Range(0f, 1f)] private float threshold = 0.4f;
    [SerializeField, Range(0f, 1f)] private float contrast = 0.3f;
    [SerializeField, Range(0f, 1f)] private float edgeGlow = 0.2f;
    [SerializeField] private int sortingOrder = 0;

    [Header("性能")]
    [SerializeField] private int maxStamps = 200;

    private class Batch
    {
        public List<Vector3> verts = new List<Vector3>();
        public List<int> tris = new List<int>();
        public List<Vector2> uvs = new List<Vector2>();
        public Mesh mesh;
        public MeshFilter filter;
        public MeshRenderer renderer;
        public bool dirty;
    }

    private Dictionary<Texture2D, Batch> batches = new Dictionary<Texture2D, Batch>();
    private int stampCount;

    public static BloodCanvas Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void DrawStamp(Vector2 worldPos, Quaternion worldRot, float scale = 1f)
    {
        if (bloodSprites.Length == 0) return;
        if (stampCount >= maxStamps) return;

        Sprite sprite = bloodSprites[Random.Range(0, bloodSprites.Length)];
        Texture2D tex = sprite.texture;

        if (!batches.TryGetValue(tex, out var b))
        {
            b = CreateBatch(tex);
            batches[tex] = b;
        }

        worldPos += new Vector2(
            Random.Range(-positionOffset.x, positionOffset.x),
            Random.Range(-positionOffset.y, positionOffset.y));
        worldRot *= Quaternion.Euler(0, 0, Random.Range(-angleOffset, angleOffset));

        float pw = sprite.rect.width / sprite.pixelsPerUnit * scale;
        float ph = sprite.rect.height / sprite.pixelsPerUnit * scale;

        Vector3[] corners = new Vector3[4]
        {
            worldRot * new Vector3(-pw / 2f, -ph / 2f),
            worldRot * new Vector3( pw / 2f, -ph / 2f),
            worldRot * new Vector3( pw / 2f,  ph / 2f),
            worldRot * new Vector3(-pw / 2f,  ph / 2f),
        };

        Rect r = sprite.rect;
        float tw = tex.width, th = tex.height;
        Vector2 uv0 = new Vector2(r.x / tw, r.y / th);
        Vector2 uv1 = new Vector2((r.x + r.width) / tw, (r.y + r.height) / th);

        int vi = b.verts.Count;
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        b.verts.Add(localPos + corners[0]);
        b.verts.Add(localPos + corners[1]);
        b.verts.Add(localPos + corners[2]);
        b.verts.Add(localPos + corners[3]);

        b.uvs.Add(new Vector2(uv0.x, uv0.y));
        b.uvs.Add(new Vector2(uv1.x, uv0.y));
        b.uvs.Add(new Vector2(uv1.x, uv1.y));
        b.uvs.Add(new Vector2(uv0.x, uv1.y));

        b.tris.Add(vi); b.tris.Add(vi + 2); b.tris.Add(vi + 1);
        b.tris.Add(vi); b.tris.Add(vi + 3); b.tris.Add(vi + 2);

        stampCount++;
        b.dirty = true;
    }

    private Batch CreateBatch(Texture2D tex)
    {
        var go = new GameObject("Blood_" + tex.name);
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;

        var b = new Batch
        {
            filter = go.AddComponent<MeshFilter>(),
            renderer = go.AddComponent<MeshRenderer>(),
            mesh = new Mesh(),
        };
        b.mesh.MarkDynamic();
        var mat = new Material(Shader.Find("Custom/BloodImprint")) { mainTexture = tex };
        mat.SetFloat("_Threshold", threshold);
        mat.SetFloat("_Contrast", contrast);
        mat.SetFloat("_EdgeGlow", edgeGlow);
        b.renderer.sharedMaterial = mat;
        b.renderer.sortingOrder = sortingOrder;
        return b;
    }

    private void LateUpdate()
    {
        foreach (var b in batches.Values)
        {
            if (!b.dirty) continue;
            b.dirty = false;
            b.mesh.Clear();
            b.mesh.vertices = b.verts.ToArray();
            b.mesh.triangles = b.tris.ToArray();
            b.mesh.uv = b.uvs.ToArray();
            b.mesh.RecalculateBounds();
            b.filter.mesh = b.mesh;
        }
    }
}
