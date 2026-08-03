using UnityEngine;
using UnityEngine.UI;

public class MouseCursor : MonoBehaviour
{
    [Header("光标")]
    [SerializeField] private Image cursorImage;
    [SerializeField] private Sprite gameCursor;
    [SerializeField] private Sprite pauseCursor;

    [Header("索敌辅助线")]
    [SerializeField] private float searchRange = 15f;
    [SerializeField, Range(1, 360)] private float searchAngle = 120f;

    public Transform lockedEnemy { get; private set; }
    private Transform player;

    void Start()
    {
        Cursor.visible = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += Refresh;
            Refresh(GameManager.Instance.CurrentState);
        }

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= Refresh;
    }

    void Update()
    {
        // 调试：F6 + 点击 → 玩家移动到点击处
        if (Input.GetKey(KeyCode.F6) && Input.GetMouseButtonDown(0) && player != null)
        {
            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0;
            player.position = world;
        }

        if (cursorImage == null) return;

        lockedEnemy = null;
        var pd = GameManager.Instance?.panelData;

        if (pd != null)
        {
            if (pd.lockNearestEnemy)
            {
                lockedEnemy = FindNearestWithLOS();
                if (lockedEnemy != null)
                    cursorImage.rectTransform.position = Camera.main.WorldToScreenPoint(lockedEnemy.position);
                else
                    cursorImage.rectTransform.position = Input.mousePosition;
            }
            else if (pd.lockDirectional)
            {
                lockedEnemy = FindDirectional();
                if (lockedEnemy != null)
                    cursorImage.rectTransform.position = Camera.main.WorldToScreenPoint(lockedEnemy.position);
                else
                    cursorImage.rectTransform.position = Input.mousePosition;
            }
            else
            {
                cursorImage.rectTransform.position = Input.mousePosition;
            }
        }
        else
        {
            cursorImage.rectTransform.position = Input.mousePosition;
        }

        // 锁敌视觉反馈
        if (lockedEnemy != null)
        {
            float s = 1f + Mathf.Sin(Time.unscaledTime * 8f) * 0.25f; // 1 ~ 1.5 正弦波动
            cursorImage.rectTransform.localScale = Vector3.one * s;
            var c = cursorImage.color;
            c.a = 0.5f;
            cursorImage.color = c;
        }
        else
        {
            cursorImage.rectTransform.localScale = Vector3.one;
            var c = cursorImage.color;
            c.a = 1f;
            cursorImage.color = c;
        }
    }

    Transform FindNearestWithLOS()
    {
        if (player == null) return null;

        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        // 按距离排序找第一个 LOS 通的
        var list = new System.Collections.Generic.List<Transform>();
        foreach (var e in enemies)
        {
            var en = e.GetComponent<Enemy>();
            if (en == null || (en.resource != null && en.resource.currentHealth <= 0f)) continue;
            list.Add(e.transform);
        }
        list.Sort((a, b) =>
            Vector2.Distance(player.position, a.position)
            .CompareTo(Vector2.Distance(player.position, b.position)));

        foreach (var t in list)
        {
            Vector2 dir = t.position - player.position;
            var hit = Physics2D.Raycast(player.position, dir.normalized, dir.magnitude, LayerMask.GetMask("Wall"));
            if (hit.collider == null) return t;
        }
        return null;
    }

    Transform FindDirectional()
    {
        if (player == null) return null;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseDir = ((Vector2)mouseWorld - (Vector2)player.position).normalized;
        float half = searchAngle / 2f;

        Transform best = null;
        float bestDist = float.MaxValue;

        foreach (var e in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            var en = e.GetComponent<Enemy>();
            if (en == null || (en.resource != null && en.resource.currentHealth <= 0f)) continue;

            Vector3 toEnemy = e.transform.position - player.position;
            float dist = toEnemy.magnitude;
            if (dist > searchRange) continue;
            if (Vector3.Angle(mouseDir, toEnemy.normalized) > half) continue;

            float perp = Vector3.Cross(mouseDir, toEnemy).magnitude;
            if (perp < bestDist) { bestDist = perp; best = e.transform; }
        }
        return best;
    }

    void Refresh(GameManager.GameState state)
    {
        if (cursorImage != null)
            cursorImage.sprite = state == GameManager.GameState.Paused ? pauseCursor : gameCursor;
    }

    void OnDrawGizmos()
    {
        if (player == null) return;

        Vector3 playerPos = player.position;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector3 dir = (mouseWorld - playerPos).normalized;

        // 朝向半径线
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(playerPos, playerPos + dir * searchRange);

        float half = searchAngle / 2f;
        float closestDist = float.MaxValue;
        Vector3 closestPos = Vector3.zero;

        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var e in enemies)
        {
            var en = e.GetComponent<Enemy>();
            if (en == null || (en.resource != null && en.resource.currentHealth <= 0f)) continue;

            Vector3 enemyPos = e.transform.position;
            Vector3 playerToEnemy = enemyPos - playerPos;
            float dist = playerToEnemy.magnitude;
            if (dist > searchRange) continue;

            float a = Vector3.Angle(dir, playerToEnemy.normalized);
            if (a > half) continue;

            float t = Vector3.Dot(playerToEnemy, dir);
            Vector3 foot = playerPos + dir * t;
            float perpDist = Vector3.Distance(enemyPos, foot);
            if (perpDist < closestDist) { closestDist = perpDist; closestPos = enemyPos; }
        }

        foreach (var e in enemies)
        {
            var en = e.GetComponent<Enemy>();
            if (en == null || (en.resource != null && en.resource.currentHealth <= 0f)) continue;

            Vector3 enemyPos = e.transform.position;
            Vector3 playerToEnemy = enemyPos - playerPos;
            float dist = playerToEnemy.magnitude;
            if (dist > searchRange) continue;
            if (Vector3.Angle(dir, playerToEnemy.normalized) > half) continue;

            float t = Vector3.Dot(playerToEnemy, dir);
            Vector3 foot = playerPos + dir * t;

            bool isClosest = enemyPos == closestPos;
            Gizmos.color = isClosest ? Color.green : Color.red;
            Gizmos.DrawWireSphere(enemyPos, isClosest ? 0.4f : 0.3f);
            Gizmos.color = isClosest ? Color.yellow : Color.gray;
            Gizmos.DrawLine(enemyPos, foot);
        }
    }
}
