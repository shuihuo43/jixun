using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>可序列化的伤害类型→颜色字典</summary>
[Serializable]
public class DamageTypeColorDict
{
    [SerializeField] private Color physics = Color.red;
    [SerializeField] private Color ghost = Color.gray;
    [SerializeField] private Color bleed = new Color(0.7f, 0.1f, 0.1f); // 暗红

    public Color this[DamageType type]
    {
        get => type switch
        {
            DamageType.Physics => physics,
            DamageType.Ghost => ghost,
            DamageType.Bleed => bleed,
            _ => Color.white,
        };
    }
}

/// <summary>
/// 伤害跳字管理器 — 对标 BloodCanvas 的动态 Mesh 合批渲染
/// </summary>
public class DamageNumberCanvas : MonoBehaviour
{
    [Header("数字素材")]
    [SerializeField] private Sprite[] digitSprites = new Sprite[10];

    [Header("跳字参数")]
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float fadeDuration = 0.6f;
    [SerializeField] private float digitSpacing = 0.3f;
    [SerializeField] private float scale = 0.5f;
    [SerializeField] private float offsetDistance = 1.5f;
    [SerializeField] private float randomJitter = 0.4f;

    [Header("伤害类型颜色")]
    [SerializeField] private DamageTypeColorDict damageColors = new();

    [Header("渲染")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 999;

    // ---- 活跃条目 ----
    private class Entry
    {
        public Vector2 worldPos;
        public int damage;
        public float age;
        public DamageType damageType;
        public float jitterX;
    }
    private readonly List<Entry> entries = new();

    // ---- Mesh 合批 ----
    private class Batch
    {
        public List<Vector3> verts = new();
        public List<int> tris = new();
        public List<Vector2> uvs = new();
        public List<Color> colors = new();
        public Mesh mesh;
        public MeshFilter filter;
        public MeshRenderer renderer;
    }
    private readonly Dictionary<Texture2D, Batch> batches = new();

    public static DamageNumberCanvas Instance { get; private set; }
    public float OffsetDistance => offsetDistance;
    public Color GetColor(DamageType t) => damageColors[t];

    void Awake()
    {
        Instance = this;
        int filled = 0;
        for (int i = 0; i < digitSprites.Length; i++)
            if (digitSprites[i] != null) filled++;
        if (filled < 10)
            Debug.LogWarning($"[DNC] digitSprites 只填了 {filled}/10 个");
    }

    public void Spawn(Vector2 worldPos, int damage, DamageType damageType)
    {
        if (damage <= 0) return;
        if (digitSprites.Length < 10 || digitSprites[0] == null) return;

        entries.Add(new Entry
        {
            worldPos = worldPos,
            damage = damage,
            damageType = damageType,
            jitterX = UnityEngine.Random.Range(-randomJitter, randomJitter),
        });
    }

    void LateUpdate()
    {
        foreach (var b in batches.Values)
        {
            b.verts.Clear(); b.tris.Clear(); b.uvs.Clear(); b.colors.Clear();
        }

        float dt = Time.unscaledDeltaTime;

        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            e.age += dt;
            e.worldPos.y += floatSpeed * dt;
        }

        for (int i = entries.Count - 1; i >= 0; i--)
        {
            if (entries[i].age >= fadeDuration)
                entries.RemoveAt(i);
        }

        for (int i = 0; i < entries.Count; i++)
        {
            float alpha = CalcAlpha(entries[i].age);
            BuildDigitQuads(entries[i], alpha);
        }

        foreach (var b in batches.Values)
        {
            b.mesh.Clear();
            if (b.verts.Count == 0) continue;
            b.mesh.vertices = b.verts.ToArray();
            b.mesh.triangles = b.tris.ToArray();
            b.mesh.uv = b.uvs.ToArray();
            b.mesh.colors = b.colors.ToArray();
            b.mesh.RecalculateBounds();
            b.filter.mesh = b.mesh;
        }
    }

    float CalcAlpha(float age)
    {
        float t = age / fadeDuration;
        float fadeStart = 2f / 3f;
        if (t < fadeStart) return 1f;
        return 1f - (t - fadeStart) / (1f - fadeStart);
    }

    /// <summary>缩放动画：初始最大，0.3s 后缩小到 0</summary>
    float CalcScaleAnim(float age)
    {
        const float shrinkStart = 0.3f;
        if (age < shrinkStart) return 1f;

        float shrinkT = (age - shrinkStart) / (fadeDuration - shrinkStart);
        return Mathf.Max(0f, 1f - shrinkT);
    }

    void BuildDigitQuads(Entry e, float alpha)
    {
        string digits = e.damage.ToString();
        int len = digits.Length;
        float totalWidth = (len - 1) * digitSpacing;
        float startX = -totalWidth / 2f;
        Color baseColor = damageColors[e.damageType];
        Color color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

        for (int i = 0; i < len; i++)
        {
            int d = digits[i] - '0';
            if (d < 0 || d >= digitSprites.Length) continue;

            Sprite sprite = digitSprites[d];
            if (sprite == null || sprite.texture == null) continue;

            Texture2D tex = sprite.texture;
            if (!batches.TryGetValue(tex, out var batch))
                batch = CreateBatch(tex);

            float anim = CalcScaleAnim(e.age);
            float pw = sprite.rect.width / sprite.pixelsPerUnit * scale * anim;
            float ph = sprite.rect.height / sprite.pixelsPerUnit * scale * anim;
            float cx = e.worldPos.x + e.jitterX + startX + i * digitSpacing;
            float cy = e.worldPos.y;

            Vector3 c0 = new Vector3(cx - pw / 2f, cy - ph / 2f, 0f);
            Vector3 c1 = new Vector3(cx + pw / 2f, cy - ph / 2f, 0f);
            Vector3 c2 = new Vector3(cx + pw / 2f, cy + ph / 2f, 0f);
            Vector3 c3 = new Vector3(cx - pw / 2f, cy + ph / 2f, 0f);

            Rect r = sprite.rect;
            float tw = tex.width, th = tex.height;
            float u0 = r.x / tw, v0 = r.y / th;
            float u1 = (r.x + r.width) / tw, v1 = (r.y + r.height) / th;

            int vi = batch.verts.Count;
            batch.verts.Add(c0); batch.verts.Add(c1);
            batch.verts.Add(c2); batch.verts.Add(c3);

            batch.uvs.Add(new Vector2(u0, v0)); batch.uvs.Add(new Vector2(u1, v0));
            batch.uvs.Add(new Vector2(u1, v1)); batch.uvs.Add(new Vector2(u0, v1));

            batch.colors.Add(color); batch.colors.Add(color);
            batch.colors.Add(color); batch.colors.Add(color);

            batch.tris.Add(vi); batch.tris.Add(vi + 2); batch.tris.Add(vi + 1);
            batch.tris.Add(vi); batch.tris.Add(vi + 3); batch.tris.Add(vi + 2);
        }
    }

    Batch CreateBatch(Texture2D tex)
    {
        var go = new GameObject("DmgNum_" + tex.name);
        go.transform.position = Vector3.zero;

        var b = new Batch
        {
            filter = go.AddComponent<MeshFilter>(),
            renderer = go.AddComponent<MeshRenderer>(),
            mesh = new Mesh(),
        };
        b.mesh.MarkDynamic();

        var mat = new Material(Shader.Find("Custom/DamageNumber"));
        mat.mainTexture = tex;
        b.renderer.sharedMaterial = mat;
        b.renderer.sortingLayerName = sortingLayerName;
        b.renderer.sortingOrder = sortingOrder;

        batches[tex] = b;
        return b;
    }
}
