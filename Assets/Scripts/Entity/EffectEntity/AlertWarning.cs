using UnityEngine;

/// <summary>
/// 感叹号预警：0.1s 从 (2,0) 缩到 (1,1) → 0.1s 保持 → 0.1s 淡出消失，总计 0.3s
/// </summary>
public class AlertWarning : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;

    private float age;

    void Start()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector3(2f, 0f, 1f);
    }

    void Update()
    {
        age += Time.deltaTime;

        if (age < 0.1f)
        {
            // 弹出： (2,0) → (1,1)
            float t = age / 0.1f;
            float sx = Mathf.Lerp(2f, 1f, t);
            float sy = Mathf.Lerp(0f, 1f, t);
            transform.localScale = new Vector3(sx, sy, 1f);
        }
        else if (age < 0.2f)
        {
            // 保持 (1,1)
            transform.localScale = Vector3.one;
        }
        else if (age < 0.3f)
        {
            // 淡出
            float t = (age - 0.2f) / 0.1f;
            if (sr != null)
            {
                var c = sr.color;
                c.a = 1f - t;
                sr.color = c;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
