using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("生命值")]
    public int health = 50;


    [Header("移动与追击")]
    public float moveSpeed = 3f;                // 靠近速度
    public float detectionRange = 10f;          // 仇恨范围
    public float attackRange = 2f;              // 停止移动距离


    [Header("攻击属性")]
    public float damage = 10f;                  // 攻击伤害
    public float attackCooldown = 1f;           // 攻击间隔


    [Header("攻击范围指示器")]
    public GameObject[] attackRangeObjects;     // 扇形、矩形等攻击范围


    private Transform player;

    private float attackTimer = 0f;

    private bool isAttacking = false;



    void Start()
    {
        //寻找玩家
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("没有找到Player标签对象！");
        }


        //隐藏攻击范围
        foreach (GameObject obj in attackRangeObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }



    void Update()
    {
        if (player == null)
            return;


        //攻击冷却
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }



        float distance = Vector2.Distance(
            transform.position,
            player.position
        );



        //=====================
        // 超出仇恨范围
        //=====================
        if (distance > detectionRange)
        {
            SetAttackState(false);
        }



        //=====================
        // 追击
        //=====================
        else if (distance > attackRange)
        {
            SetAttackState(false);


            Vector2 direction =
                (player.position - transform.position).normalized;


            transform.position +=
                (Vector3)direction * moveSpeed * Time.deltaTime;



            //2D朝向玩家
            float angle =
                Mathf.Atan2(direction.y, direction.x)
                * Mathf.Rad2Deg;


            transform.rotation =
                Quaternion.Euler(
                    0,
                    0,
                    angle
                );
        }



        //=====================
        // 攻击状态
        //=====================
        else
        {
            SetAttackState(true);
        }

    }





    //显示/隐藏攻击范围
    void SetAttackState(bool attacking)
    {
        if (isAttacking == attacking)
            return;


        isAttacking = attacking;



        foreach (GameObject obj in attackRangeObjects)
        {
            if (obj != null)
            {
                obj.SetActive(attacking);
            }
        }
    }





    //由攻击范围Collider2D调用
    public void TryAttack()
    {
        if (!isAttacking)
            return;


        if (attackTimer > 0)
            return;



        Player p =
            player.GetComponent<Player>();


        if (p != null)
        {
            p.TakeDamage(damage);

            attackTimer = attackCooldown;

            Debug.Log(
                gameObject.name +
                "攻击玩家，造成伤害：" +
                damage
            );
        }

    }





    //敌人受伤
    public void TakeDamage(int amount)
    {
        health -= amount;


        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }





    //Scene窗口显示范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;


        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );


        Gizmos.color = Color.red;


        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}