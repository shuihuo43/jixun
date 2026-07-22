using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>预警范围：继承 Entity，Born 传入 ShapeArea + windupTime 后自动推进并生成攻击实体</summary>
public class SectorRange : Entity
{
    public enum WarningShape { Sector, Box }

    [Header("攻击实体（推进完成后生成）")]
    [SerializeField] private GameObject entityPrefab;

    [Header("调试")]
    [SerializeField] private bool isDebug;
    [SerializeField] private float directionOffset = -90f;  // mesh 沿 up，实体朝 right，补 90°
    public ShapeArea shapeArea;

    // 运行时状态
    private WarningShape warningShape;
    private float angle, radius, innerRadius, boxWidth, boxHeight;
    private int quality = 24;
    private float windupTime;
    private float timer;
    private Vector2 bornDirection;
    private bool running;

    // Mesh
    private GameObject outerObj, innerObj;
    private MeshFilter outerFilter, innerFilter;
    private MeshRenderer outerRenderer, innerRenderer;
    private Material outerInst, innerInst;

    [Header("材质")]
    [SerializeField] private Material outerMaterial;
    [SerializeField] private Color outerColor = new Color(1, 1, 1, 0.3f);
    [SerializeField] private Material innerMaterial;
    [SerializeField] private Color innerColor = new Color(1, 0, 0, 0.6f);

    void Start()
    {
        if (isDebug)
        {
            EntityBorn(Vector2.zero, Vector2.right, transform.parent != null ? transform.parent.gameObject : gameObject);
            if (shapeArea == null) shapeArea = gameObject.AddComponent<ShapeArea>();
            StartWindup(shapeArea, 1f);
        }
    }

    /// <summary>开始预警：传入形状和持续时间，自动推进并在结束时生成 entityPrefab</summary>
    public void StartWindup(ShapeArea sa, float windupTime)
    {
        if (sa == null)
        {
            Debug.LogWarning("SectorRange.StartWindup: ShapeArea is null, aborting");
            return;
        }
        if (entityPrefab == null)
            Debug.LogWarning("SectorRange.StartWindup: entityPrefab is null, 不会生成攻击实体");

        // 从 ShapeArea 同步形状参数
        warningShape = (int)sa.Shape == 0 ? WarningShape.Sector : WarningShape.Box;
        angle = sa.Angle;
        radius = sa.Radius;
        boxWidth = sa.BoxWidth;
        boxHeight = sa.BoxHeight;
        innerRadius = 0f;

        this.windupTime = windupTime;
        timer = 0f;
        running = true;

        // 记录朝向（EntityBorn 已设置 rotation）
        float rad = transform.eulerAngles.z * Mathf.Deg2Rad;
        bornDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    void Update()
    {
        if (!running) return;

        EnsureObjects();

        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / windupTime);

        if (warningShape == WarningShape.Box)
        {
            float fillWidth = boxWidth * progress;
            ApplyBoxMesh(outerFilter, outerRenderer, ref outerInst, outerMaterial, outerColor, boxWidth, boxHeight, 0);
            ApplyBoxMesh(innerFilter, innerRenderer, ref innerInst, innerMaterial, innerColor, fillWidth, boxHeight, 1);
        }
        else
        {
            float innerLayerRadius = Mathf.Lerp(innerRadius, radius, progress);
            ApplyLayer(outerObj, outerFilter, outerRenderer, ref outerInst, outerMaterial, outerColor, angle, radius);
            ApplyLayer(innerObj, innerFilter, innerRenderer, ref innerInst, innerMaterial, innerColor, angle, innerLayerRadius);
        }

        if (progress >= 1f)
        {
            running = false;
            OnWindupComplete();
        }
    }

    void OnWindupComplete()
    {
        if (entityPrefab != null)
        {
            var obj = Instantiate(entityPrefab);
            var entity = obj.GetComponent<Entity>();
            if (entity != null)
            {
                GameObject root = GameObject.FindGameObjectWithTag("EnemyAttackEntityRoot");
                if (root == null) root = gameObject;

                // SectorRange 世界坐标转 root 本地坐标，朝向保持 SectorRange 自身
                Vector2 localPos = root.transform.InverseTransformPoint(transform.position);
                entity.EntityBorn(localPos, bornDirection, root);
            }
        }

        DestroyWithFade();
    }

    // ===== Mesh 生成（不变）=====

    void EnsureObjects()
    {
        if (outerObj == null)
        {
            outerObj = new GameObject("SectorOuter") { transform = { parent = transform, localPosition = Vector3.zero, localRotation = Quaternion.Euler(0, 0, directionOffset) } };
            outerFilter = outerObj.AddComponent<MeshFilter>();
            outerRenderer = outerObj.AddComponent<MeshRenderer>();
        }
        if (innerObj == null)
        {
            innerObj = new GameObject("SectorInner") { transform = { parent = transform, localPosition = Vector3.zero, localRotation = Quaternion.Euler(0, 0, directionOffset) } };
            innerFilter = innerObj.AddComponent<MeshFilter>();
            innerRenderer = innerObj.AddComponent<MeshRenderer>();
        }
    }

    void ApplyLayer(GameObject obj, MeshFilter filter, MeshRenderer renderer,
                    ref Material inst, Material src, Color color, float layerAngle, float layerRadius)
    {
        bool visible = layerRadius > 0.001f && layerAngle > 0f;
        obj.SetActive(visible);
        if (!visible) return;

        if (inst == null)
        {
            inst = src != null ? new Material(src) : new Material(Shader.Find("Sprites/Default"));
            if (!inst.HasProperty("_Color")) inst.shader = Shader.Find("Sprites/Default");
        }

        filter.mesh = BuildMesh(layerAngle, layerRadius);
        inst.color = color;
        renderer.sharedMaterial = inst;
        renderer.sortingOrder = (renderer == innerRenderer) ? 1 : 0;
    }

    Mesh BuildMesh(float meshAngle, float meshRadius)
    {
        int segCount = quality;
        float eachAngle = meshAngle / segCount;
        var verts = new List<Vector3>();
        var uvs = new List<Vector2>();

        if (innerRadius <= 0f)
        {
            verts.Add(Vector3.zero);
            uvs.Add(new Vector2(0.5f, 0f));
            for (int i = 0; i <= segCount; i++)
            {
                var dir = Quaternion.Euler(0, 0, meshAngle / 2 - eachAngle * i) * Vector2.up;
                verts.Add(dir * meshRadius);
                uvs.Add(new Vector2((float)i / segCount, 1f));
            }
        }
        else
        {
            for (int i = 0; i <= segCount; i++)
            {
                var dir = Quaternion.Euler(0, 0, meshAngle / 2 - eachAngle * i) * Vector2.up;
                verts.Add(dir * innerRadius);
                uvs.Add(new Vector2((float)i / segCount, 0f));
            }
            for (int i = 0; i <= segCount; i++)
            {
                var dir = Quaternion.Euler(0, 0, meshAngle / 2 - eachAngle * i) * Vector2.up;
                verts.Add(dir * meshRadius);
                uvs.Add(new Vector2((float)i / segCount, 1f));
            }
        }

        var tris = new List<int>();
        if (innerRadius <= 0f)
        {
            for (int i = 0; i < segCount; i++)
            { tris.Add(0); tris.Add(i + 1); tris.Add(i + 2); }
        }
        else
        {
            int n = segCount + 1;
            for (int i = 0; i < segCount; i++)
            {
                int i0 = i, i1 = i + 1, o0 = n + i, o1 = n + i + 1;
                tris.Add(i0); tris.Add(o0); tris.Add(i1);
                tris.Add(o0); tris.Add(o1); tris.Add(i1);
            }
        }

        var mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    void ApplyBoxMesh(MeshFilter filter, MeshRenderer renderer, ref Material inst,
                       Material src, Color color, float w, float h, int sortingOrder)
    {
        bool visible = w > 0.001f && h > 0.001f;
        (filter == innerFilter ? innerObj : outerObj).SetActive(visible);
        if (!visible) return;

        if (inst == null)
        {
            inst = src != null ? new Material(src) : new Material(Shader.Find("Sprites/Default"));
            if (!inst.HasProperty("_Color")) inst.shader = Shader.Find("Sprites/Default");
        }

        filter.mesh = BuildBoxMesh(w, h);
        inst.color = color;
        renderer.sharedMaterial = inst;
        renderer.sortingOrder = sortingOrder;
    }

    Mesh BuildBoxMesh(float w, float h)
    {
        float hw = w / 2f, hh = h / 2f;
        var mesh = new Mesh();
        mesh.vertices = new Vector3[] {
            new(-hh, -hw), new(hh, -hw), new(hh, hw), new(-hh, hw)
        };
        mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
        mesh.uv = new Vector2[] { new(0, 0), new(1, 0), new(1, 1), new(0, 1) };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public void DestroyWithFade(float fadeDuration = 0.15f)
    {
        StopAllCoroutines();
        StartCoroutine(FadeAndDestroy(fadeDuration));
    }

    IEnumerator FadeAndDestroy(float duration)
    {
        float outerA = outerInst != null ? outerInst.color.a : 0f;
        float innerA = innerInst != null ? innerInst.color.a : 0f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float inv = 1f - t / duration;
            if (outerInst != null) { var c = outerInst.color; c.a = outerA * inv; outerInst.color = c; }
            if (innerInst != null) { var c = innerInst.color; c.a = innerA * inv; innerInst.color = c; }
            yield return null;
        }
        EntityDestroy();
    }

    // ===== 保留 Gizmo 调试 =====

    void OnDrawGizmosSelected()
    {
        if (shapeArea == null) return;
        float a = shapeArea.Angle, r = shapeArea.Radius;
        Vector3 center = transform.position;
        Vector3 up = transform.up;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(center, center + Quaternion.Euler(0, 0, a / 2) * up * r);
        Gizmos.DrawLine(center, center + Quaternion.Euler(0, 0, -a / 2) * up * r);
    }
}
