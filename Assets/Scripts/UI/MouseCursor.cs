using UnityEngine;
using UnityEngine.UI;

public class MouseCursor : MonoBehaviour
{
    [SerializeField] private Image cursorImage;
    [SerializeField] private Sprite gameCursor;
    [SerializeField] private Sprite pauseCursor;

    [Header("索敌辅助线")]
    [SerializeField] private float searchRange = 10f;
    [SerializeField, Range(1, 360)] private float searchAngle = 120f;

    void Start()
    {
        Cursor.visible = false;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += Refresh;
            Refresh(GameManager.Instance.CurrentState);
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= Refresh;
    }

    void Update()
    {
        if (cursorImage != null)
            cursorImage.rectTransform.position = Input.mousePosition;
    }

    void Refresh(GameManager.GameState state)
    {
        if (cursorImage != null)
            cursorImage.sprite = state == GameManager.GameState.Paused ? pauseCursor : gameCursor;
    }

    void DrawSector(Vector3 center, Vector3 dir, float radius, float angle)
    {
        float half = angle / 2f;
        int steps = 20;

        Vector3 last = center + Quaternion.Euler(0, 0, -half) * dir * radius;
        for (int i = 1; i <= steps; i++)
        {
            float a = -half + (angle / steps) * i;
            Vector3 next = center + Quaternion.Euler(0, 0, a) * dir * radius;
            Gizmos.DrawLine(center, next);
            Gizmos.DrawLine(last, next);
            last = next;
        }
    }

    void OnDrawGizmos()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 playerPos = player.transform.position;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;

        Vector3 dir = (mouseWorld - playerPos).normalized;

        // 朝向半径线
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(playerPos, playerPos + dir * searchRange);

        float closestDist = float.MaxValue;
        Vector3 closestPos = Vector3.zero;
        float halfAngle = searchAngle / 2f;

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
            if (a > halfAngle) continue;

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
            if (Vector3.Angle(dir, playerToEnemy.normalized) > halfAngle) continue;

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
