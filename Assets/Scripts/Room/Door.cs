using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private float openRange = 3f;

    [Header("破门实体")]
    [SerializeField] private GameObject entityPrefab;
    [SerializeField] private int entityCount = 5;

    private Transform player;

    void Start()
    {
        var obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null) player = obj.transform;
    }

    void Update()
    {
        if (player == null || doorCollider == null || !doorCollider.enabled) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= openRange && Input.GetMouseButtonDown(2))
        {
            doorCollider.enabled = false;
            SpawnEntities();
        }
    }

    void SpawnEntities()
    {
        if (entityPrefab == null || entityCount <= 0) return;

        // 门正面朝上，反面朝下（-up），±60° = 120° 扇形
        Vector2 backDir = (Vector2)(-transform.up);
        float baseAngle = Mathf.Atan2(backDir.y, backDir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < entityCount; i++)
        {
            float t = entityCount == 1 ? 0f : (float)i / (entityCount - 1);
            float angle = Mathf.Lerp(-60f, 60f, t) + baseAngle;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            var obj = Instantiate(entityPrefab, transform.position, Quaternion.identity);
            var entity = obj.GetComponent<Entity>();
            if (entity != null)
                entity.EntityBorn(Vector2.zero, dir, null);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, openRange);
    }
}
