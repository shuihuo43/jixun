using UnityEngine;

public class BloodFlyEntity : Entity
{
    [SerializeField] private float speed = 60f;
    [SerializeField] private float speedOffset = 30f;
    [SerializeField] private float angleOffset = 30f;
    [SerializeField] private float stampDuration = 0.3f;

    private Vector2 flyDirection;
    private float flySpeed;
    private float stampTimer;

    public override void EntityBorn(Vector2 position, Vector2 direction, GameObject bornRoot, Vector2? scale = null, bool flipY = false, System.Action onDestroy = null)
    {
        base.EntityBorn(position, direction, bornRoot, scale, flipY, onDestroy);

        // 随机角度偏移
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float randomAngle = baseAngle + Random.Range(-angleOffset, angleOffset);
        float rad = randomAngle * Mathf.Deg2Rad;
        flyDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        // 随机速度偏移
        flySpeed = speed + Random.Range(-speedOffset, speedOffset);

        stampTimer = stampDuration;
    }

    private void Update()
    {
        // 飞行
        transform.Translate(flyDirection * flySpeed * Time.deltaTime, Space.World);

        // 每隔 stampDuration 生成一个血迹
        stampTimer -= Time.deltaTime;
        if (stampTimer <= 0f)
        {
            SpawnBloodStamp();
            stampTimer = stampDuration;
        }
    }

    private void SpawnBloodStamp()
    {
        if (BloodCanvas.Instance == null) return;

        float baseAngle = Mathf.Atan2(flyDirection.y, flyDirection.x) * Mathf.Rad2Deg;
        float randomAngle = baseAngle + Random.Range(-angleOffset, angleOffset);
        BloodCanvas.Instance.DrawStamp(transform.position, Quaternion.Euler(0, 0, randomAngle), Random.Range(0.8f, 1.2f));
    }
}
