using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectorRange : MonoBehaviour
{
    public enum WarningShape { Sector, Box }

    [Header("形状")]
    [SerializeField] private WarningShape warningShape = WarningShape.Sector;
    [SerializeField][Range(1f, 360f)] private float angle = 360;
    [SerializeField][Range(0.1f, 20f)] private float radius = 3f;
    [SerializeField][Range(0f, 20f)]  private float innerRadius = 0f;
    [SerializeField][Range(1, 60)]    private int quality = 6;

    /// <summary>与 ShapeArea.ShapeType 同步</summary>
    public WarningShape Shape
    {
        get => warningShape;
        set => warningShape = value;
    }

    public void SyncFromShapeArea(ShapeArea sa)
    {
        if (sa == null) return;
        warningShape = (int)sa.Shape == 0 ? WarningShape.Sector : WarningShape.Box;
        angle = sa.Angle;
        radius = sa.Radius;
        boxWidth = sa.BoxWidth;
        boxHeight = sa.BoxHeight;
    }

    [Header("进度")]
    [SerializeField][Range(0f, 1f)] private float process = 0.5f;

    /// <summary>前摇进度 0→1（公开给 Enemy 驱动）</summary>
    public float Process
    {
        get => process;
        set => process = Mathf.Clamp01(value);
    }

    public float SectorAngle
    {
        get => angle;
        set => angle = Mathf.Clamp(value, 1f, 360f);
    }

    public float SectorRadius
    {
        get => radius;
        set => radius = value;
    }

    [Header("外层")]
    [SerializeField] private Material outerMaterial;
    [SerializeField] private Color outerColor = new Color(1, 1, 1, 0.3f);

    [Header("内层")]
    [SerializeField] private Material innerMaterial;
    [SerializeField] private Color innerColor = new Color(1, 0, 0, 0.6f);

    // 外层
    private GameObject outerObj;
    private MeshFilter outerFilter;
    private MeshRenderer outerRenderer;
    private Material outerInst;

    // 内层
    private GameObject innerObj;
    private MeshFilter innerFilter;
    private MeshRenderer innerRenderer;
    private Material innerInst;

    [Header("矩形")]
    [SerializeField] private float boxWidth = 2f;
    [SerializeField] private float boxHeight = 1f;

    public void Update()
    {
        EnsureObjects();

        if (warningShape == WarningShape.Box)
        {
            float fillWidth = boxWidth * process;
            ApplyBoxMesh(outerFilter, outerRenderer, ref outerInst, outerMaterial, outerColor, boxWidth, boxHeight, 0);
            ApplyBoxMesh(innerFilter, innerRenderer, ref innerInst, innerMaterial, innerColor, fillWidth, boxHeight, 1);
        }
        else
        {
            float innerLayerRadius = Mathf.Lerp(innerRadius, radius, process);
            ApplyLayer(outerObj, outerFilter, outerRenderer, ref outerInst, outerMaterial, outerColor, angle, radius);
            ApplyLayer(innerObj, innerFilter, innerRenderer, ref innerInst, innerMaterial, innerColor, angle, innerLayerRadius);
        }
    }

    private void EnsureObjects()
    {
        if (outerObj == null)
        {
            outerObj = new GameObject("SectorOuter");
            outerObj.transform.SetParent(transform, false);
            outerFilter = outerObj.AddComponent<MeshFilter>();
            outerRenderer = outerObj.AddComponent<MeshRenderer>();
        }
        if (innerObj == null)
        {
            innerObj = new GameObject("SectorInner");
            innerObj.transform.SetParent(transform, false);
            innerFilter = innerObj.AddComponent<MeshFilter>();
            innerRenderer = innerObj.AddComponent<MeshRenderer>();
        }
    }

    private void ApplyLayer(GameObject obj, MeshFilter filter, MeshRenderer renderer,
                            ref Material inst, Material src, Color color,
                            float layerAngle, float layerRadius)
    {
        bool visible = layerRadius > innerRadius + 0.001f && layerAngle > 0f;
        obj.SetActive(visible);

        if (!visible) return;

        // 材质实例只创建一次
        // 注意：Unlit/Transparent 没有 _Color 属性，需要用 Sprites/Default
        if (inst == null)
        {
            if (src != null)
                inst = new Material(src);
            else
                inst = new Material(Shader.Find("Sprites/Default"));

            // 如果源材质 Shader 没有 _Color，补一个
            if (!inst.HasProperty("_Color"))
                inst.shader = Shader.Find("Sprites/Default");
        }

        Mesh mesh = BuildMesh(layerAngle, layerRadius);
        filter.mesh = mesh;
        inst.color = color;
        renderer.sharedMaterial = inst;

        // 内层渲染在外层之上
        renderer.sortingOrder = (renderer == innerRenderer) ? 1 : 0;
    }

    private Mesh BuildMesh(float meshAngle, float meshRadius)
    {
        int segCount = quality;
        float eachAngle = meshAngle / segCount;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        if (innerRadius <= 0f)
        {
            vertices.Add(Vector3.zero);
            uvs.Add(new Vector2(0.5f, 0f));

            for (int i = 0; i <= segCount; i++)
            {
                Vector3 dir = Quaternion.Euler(0, 0, meshAngle / 2 - eachAngle * i) * Vector2.up;
                vertices.Add(dir * meshRadius);
                uvs.Add(new Vector2((float)i / segCount, 1f));
            }
        }
        else
        {
            for (int i = 0; i <= segCount; i++)
            {
                Vector3 dir = Quaternion.Euler(0, 0, meshAngle / 2 - eachAngle * i) * Vector2.up;
                vertices.Add(dir * innerRadius);
                uvs.Add(new Vector2((float)i / segCount, 0f));
            }
            for (int i = 0; i <= segCount; i++)
            {
                Vector3 dir = Quaternion.Euler(0, 0, meshAngle / 2 - eachAngle * i) * Vector2.up;
                vertices.Add(dir * meshRadius);
                uvs.Add(new Vector2((float)i / segCount, 1f));
            }
        }

        List<int> triangles = new List<int>();
        if (innerRadius <= 0f)
        {
            for (int i = 0; i < segCount; i++)
            {
                triangles.Add(0);
                triangles.Add(i + 1);
                triangles.Add(i + 2);
            }
        }
        else
        {
            int n = segCount + 1;
            for (int i = 0; i < segCount; i++)
            {
                int i0 = i, i1 = i + 1, o0 = n + i, o1 = n + i + 1;
                triangles.Add(i0); triangles.Add(o0); triangles.Add(i1);
                triangles.Add(o0); triangles.Add(o1); triangles.Add(i1);
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private void ApplyBoxMesh(MeshFilter filter, MeshRenderer renderer, ref Material inst,
                               Material src, Color color, float w, float h, int sortingOrder)
    {
        bool visible = w > 0.001f && h > 0.001f && boxWidth > 0.001f && boxHeight > 0.001f;
        if (filter == innerFilter) innerObj.SetActive(visible);
        else outerObj.SetActive(visible);

        if (!visible) return;

        if (inst == null)
        {
            inst = src != null ? new Material(src) : new Material(Shader.Find("Sprites/Default"));
            if (!inst.HasProperty("_Color"))
                inst.shader = Shader.Find("Sprites/Default");
        }

        Mesh mesh = BuildBoxMesh(w, h);
        filter.mesh = mesh;
        inst.color = color;
        renderer.sharedMaterial = inst;
        renderer.sortingOrder = sortingOrder;
    }

    /// <summary>构建矩形 mesh，从左到右填充，中心和 Sector 默认方向一致（朝上）</summary>
    private Mesh BuildBoxMesh(float w, float h)
    {
        float hw = w / 2f;
        float hh = h / 2f;

        // 矩形顶点：左右翻转使推进方向和扇形朝向一致（从前方开始）
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-hh, -hw),   // 左下
            new Vector3( hh, -hw),   // 右下
            new Vector3( hh,  hw),   // 右上
            new Vector3(-hh,  hw),   // 左上
        };
        mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
        mesh.uv = new Vector2[] { new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1) };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private void OnDrawGizmos()
    {
        float eachAngle = angle / quality;
        Vector3 center = transform.position;

        Vector3 startDir = Quaternion.Euler(0, 0, angle / 2) * transform.up;
        Vector3 endDir   = Quaternion.Euler(0, 0, -angle / 2) * transform.up;

        // ---- 外层 ----
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(center, center + startDir * radius);
        Gizmos.DrawLine(center, center + endDir * radius);

        Gizmos.color = Color.green;
        Vector3 last = center + startDir * radius;
        for (int i = 1; i <= quality; i++)
        {
            Vector3 dir = Quaternion.Euler(0, 0, angle / 2 - eachAngle * i) * transform.up;
            Vector3 next = center + dir * radius;
            Gizmos.DrawLine(last, next);
            last = next;
        }

        // ---- 内层进度弧（径向填充） ----
        float innerLayerR = Mathf.Lerp(innerRadius, radius, process);
        if (innerLayerR > innerRadius + 0.001f)
        {
            Gizmos.color = Color.red;
            Vector3 lastInner = center + startDir * innerLayerR;
            for (int i = 1; i <= quality; i++)
            {
                Vector3 dir = Quaternion.Euler(0, 0, angle / 2 - eachAngle * i) * transform.up;
                Vector3 next = center + dir * innerLayerR;
                Gizmos.DrawLine(lastInner, next);
                lastInner = next;
            }
            Gizmos.DrawLine(center + startDir * innerRadius, center + startDir * innerLayerR);
            Gizmos.DrawLine(center + endDir * innerRadius, center + endDir * innerLayerR);
        }

        // ---- 内半径弧 ----
        if (innerRadius > 0f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(center, center + startDir * innerRadius);
            Gizmos.DrawLine(center, center + endDir * innerRadius);

            Gizmos.color = Color.green;
            last = center + startDir * innerRadius;
            for (int i = 1; i <= quality; i++)
            {
                Vector3 dir = Quaternion.Euler(0, 0, angle / 2 - eachAngle * i) * transform.up;
                Vector3 next = center + dir * innerRadius;
                Gizmos.DrawLine(last, next);
                last = next;
            }
        }
    }

    /// <summary>渐隐后销毁，供 Enemy 攻击完成时调用</summary>
    public void DestroyWithFade(float fadeDuration = 0.15f)
    {
        StopAllCoroutines();
        StartCoroutine(FadeAndDestroy(fadeDuration));
    }

    private IEnumerator FadeAndDestroy(float duration)
    {
        // 记录起始 alpha
        float outerStartAlpha = outerInst != null ? outerInst.color.a : 0f;
        float innerStartAlpha = innerInst != null ? innerInst.color.a : 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float invT = 1f - t;

            if (outerInst != null)
            {
                Color c = outerInst.color;
                c.a = outerStartAlpha * invT;
                outerInst.color = c;
            }

            if (innerInst != null)
            {
                Color c = innerInst.color;
                c.a = innerStartAlpha * invT;
                innerInst.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
