using UnityEngine;

public class BloodStampEntity : Entity
{
    [SerializeField] private Sprite[] stampSprites;
    [SerializeField] private Vector2 positionOffset = new Vector2(0.5f, 0.5f);
    [SerializeField] private float angleOffset = 30f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void EntityBorn(Vector2 position, Vector2 direction, GameObject bornRoot, Vector2? scale = null, bool flipY = false, System.Action onDestroy = null)
    {
        // 随机位置偏移
        Vector2 finalPos = position + new Vector2(
            Random.Range(-positionOffset.x, positionOffset.x),
            Random.Range(-positionOffset.y, positionOffset.y));

        // 随机角度偏移
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float randomAngle = baseAngle + Random.Range(-angleOffset, angleOffset);
        float rad = randomAngle * Mathf.Deg2Rad;
        Vector2 finalDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        base.EntityBorn(finalPos, finalDir, bornRoot, scale, flipY, onDestroy);

        // 随机选一张血迹贴图
        if (stampSprites != null && stampSprites.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = stampSprites[Random.Range(0, stampSprites.Length)];
        }
    }
}
