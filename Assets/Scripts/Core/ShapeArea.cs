using UnityEngine;

public class ShapeArea : MonoBehaviour
{
    public enum ShapeType { Sector, Box }

    [Header("形状")]
    [SerializeField] private ShapeType shapeType = ShapeType.Sector;

    [Header("扇形")]
    [SerializeField, Range(1f, 360f)] private float angle = 90f;
    [SerializeField, Range(0.1f, 500f)]  private float radius = 3f;

    [Header("矩形")]
    [SerializeField] private float width = 2f;
    [SerializeField] private float height = 1f;

    [Header("中心与朝向")]
    [SerializeField] private Vector2 centerOffset = Vector2.zero;   // 图形位移
    [SerializeField, Range(0f, 360f)] private float direction = 90f;
    [SerializeField, Range(3, 60)]      private int quality = 30;

    [Header("调试颜色")]
    [SerializeField] private Color gizmoColor = new Color(1f, 0f, 0f, 0.5f);

    public ShapeType Shape => shapeType;

    public float Direction
    {
        get => direction;
        set => direction = value;
    }

    public Vector2 CenterOffset
    {
        get => centerOffset;
        set => centerOffset = value;
    }

    public float Radius => radius;
    public float Angle => angle;
    public float BoxWidth => width;
    public float BoxHeight => height;

    /// <summary>检测范围内是否有指定 tag 的物体，返回第一个或 null</summary>
    public GameObject DetectTag(string tag)
    {
        Vector3 worldCenter = LocalToWorld(centerOffset);
        float checkRadius = shapeType == ShapeType.Sector ? radius : Mathf.Max(width, height);

        var hits = Physics2D.OverlapCircleAll(worldCenter, checkRadius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag(tag)) continue;
            if (!IsInsideShape(hit.transform.position)) continue;
            return hit.gameObject;
        }
        return null;
    }

    bool IsInsideShape(Vector3 worldPos)
    {
        Vector3 local = transform.InverseTransformPoint(worldPos);
        local -= (Vector3)centerOffset;

        float dist = local.magnitude;
        if (dist < 0.01f) return true;

        if (shapeType == ShapeType.Sector)
        {
            if (dist > radius) return false;
            float a = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
            float half = angle / 2f;
            return Mathf.Abs(Mathf.DeltaAngle(0f, a)) <= half;
        }
        else
        {
            return Mathf.Abs(local.x) <= width / 2f && Mathf.Abs(local.y) <= height / 2f;
        }
    }

    /// <summary>将图形局部坐标转为世界坐标（绕实体位置旋转 direction°，再经实体 transform）</summary>
    private Vector3 LocalToWorld(Vector2 localPoint)
    {
        // 加到中心偏移
        Vector2 totalLocal = centerOffset + localPoint;

        // 绕实体位置（局部原点）旋转 direction
        float rad = direction * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        float rx = totalLocal.x * cos - totalLocal.y * sin;
        float ry = totalLocal.x * sin + totalLocal.y * cos;

        // 实体位移 + 旋转
        return transform.TransformPoint(new Vector3(rx, ry, 0));
    }

    private void OnDrawGizmosSelected() => DrawGizmo();
    private void OnDrawGizmos() => DrawGizmo();

    private void DrawGizmo()
    {
        switch (shapeType)
        {
            case ShapeType.Sector: DrawSector(); break;
            case ShapeType.Box:    DrawBox();    break;
        }
    }

    private void DrawSector()
    {
        Vector3 center = LocalToWorld(Vector2.zero);
        Gizmos.color = gizmoColor;

        float halfAngle = angle / 2f;
        float eachAngle = angle / quality;

        // 两侧边界（局部方向默认朝右 = 0°）
        for (int sign = -1; sign <= 1; sign += 2)
        {
            float rad = halfAngle * sign * Mathf.Deg2Rad;
            Vector2 localEdge = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            Gizmos.DrawLine(center, LocalToWorld(localEdge));
        }

        // 弧线
        float startRad = halfAngle * Mathf.Deg2Rad;
        Vector2 localStart = new Vector2(Mathf.Cos(startRad), Mathf.Sin(startRad)) * radius;
        Vector3 lastPoint = LocalToWorld(localStart);
        for (int i = 1; i <= quality; i++)
        {
            float a = (halfAngle - eachAngle * i) * Mathf.Deg2Rad;
            Vector2 localPt = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
            Vector3 nextPoint = LocalToWorld(localPt);
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }

    private void DrawBox()
    {
        Gizmos.color = gizmoColor;

        float hw = width / 2f;
        float hh = height / 2f;

        Vector3[] corners = new Vector3[4]
        {
            LocalToWorld(new Vector2(-hw,  hh)),
            LocalToWorld(new Vector2( hw,  hh)),
            LocalToWorld(new Vector2( hw, -hh)),
            LocalToWorld(new Vector2(-hw, -hh)),
        };

        for (int i = 0; i < 4; i++)
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
    }
}
