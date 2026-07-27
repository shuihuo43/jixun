using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Throw : EnemyState
{
    [Header("间隔")]
    [SerializeField] private float throwInterval = 1.5f;
    [SerializeField] private int throwCount = 5;
    [SerializeField] private float targetSpread = 2f;
    [SerializeField] private float predictionTime = 0.3f;

    private int thrown;

    [Header("投掷物")]
    [SerializeField] private GameObject throwObjPrefab;
    [SerializeField] private GameObject landingEntityPrefab;
    [SerializeField] private float throwRadius = 5f;

    [Header("抛物线")]
    [SerializeField] private float flightTime = 0.5f;
    [SerializeField] private float minArcHeight = 1f;
    [SerializeField] private float maxArcHeight = 3f;

    private class FlyingObj
    {
        public GameObject obj;
        public Vector2 start;
        public Vector2 end;
        public float arcHeight;
        public float elapsed;
    }

    private List<FlyingObj> flying = new();
    private float throwTimer;

    public override void StateEnter()
    {
        throwTimer = 0f;
        thrown = 0;
        enemy.moveDir = Vector2.zero;
    }

    public override void StateUpdate()
    {
        throwTimer -= Time.deltaTime;
        if (throwTimer <= 0f && thrown < throwCount)
        {
            throwTimer = throwInterval;
            TrySpawnThrow();
            thrown++;
        }

        if (thrown >= throwCount && flying.Count == 0)
            stateMachine.ChangeToState(enemy.EngageStateName);

        // 更新飞行物
        for (int i = flying.Count - 1; i >= 0; i--)
        {
            var f = flying[i];
            f.elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(f.elapsed / flightTime);
            f.obj.transform.position = Parabola(f.start, f.end, f.arcHeight, t);
            if (t >= 1f) { Destroy(f.obj); flying.RemoveAt(i); }
        }
    }

    void TrySpawnThrow()
    {
        for (int attempt = 0; attempt < 10; attempt++)
            {
                // 预判玩家位置 + 随机散布
                Vector3 predicted = enemy.Player.position;
                var p = enemy.Player.GetComponent<Player>();
                if (p != null && p.MoveInput.sqrMagnitude > 0.01f)
                    predicted += (Vector3)(p.MoveInput * p.MoveSpeed * predictionTime);
                Vector2 target = (Vector2)predicted + Random.insideUnitCircle * targetSpread;

                // LOS
                Vector2 dir = target - (Vector2)enemy.transform.position;
                var hit = Physics2D.Raycast(enemy.transform.position, dir.normalized, dir.magnitude, LayerMask.GetMask("Wall"));
                if (hit.collider != null) continue;

                // 落点生成实体
                if (landingEntityPrefab != null)
                {
                    var ent = Instantiate(landingEntityPrefab, target, Quaternion.identity);
                    var e = ent.GetComponent<Entity>();
                    if (e != null)
                    {
                        var root = GameObject.FindGameObjectWithTag("EnemyAttackEntityRoot");
                        if (root == null) root = enemy.gameObject;
                        var lp = root.transform.InverseTransformPoint(target);
                        e.EntityBorn(lp, Vector2.up, root);
                    }
                }

                // 飞行精灵
                var obj = Instantiate(throwObjPrefab, enemy.transform.position, Quaternion.identity);
                flying.Add(new FlyingObj
                {
                    obj = obj,
                    start = enemy.transform.position,
                    end = target,
                    arcHeight = Random.Range(minArcHeight, maxArcHeight),
                    elapsed = 0f
                });
                break;
            }
        }

    Vector3 Parabola(Vector2 a, Vector2 b, float arc, float t)
    {
        Vector2 mid = Vector2.Lerp(a, b, t);
        float h = 4f * arc * t * (1f - t);
        return new Vector3(mid.x, mid.y + h, 0);
    }
}
