using UnityEngine;


public class EnemyAttackEntity : MonoBehaviour
{

    public enum AttackState
    {
        Ready,
        Windup,
        Attack,
        Cooldown
    }


    [Header("攻击属性")]
    public float damage = 10;
    public float attackRange = 3;
    public float cooldown = 1;


    [Header("预警")]
    public GameObject sectorRangePrefab;
    public float windupTime = 0.8f;


    public ShapeArea shapeArea;


    private SectorRange activeWarning;


    private float timer;


    private Transform player;


    private AttackState state = AttackState.Ready;


    //锁定方向
    private float lockedDirection;



    public bool IsAttacking
    {
        get
        {
            return state == AttackState.Windup ||
                   state == AttackState.Attack;
        }
    }



    public float AttackRange => attackRange;



    void Start()
    {
        player =
        GameObject.FindGameObjectWithTag("Player")
        .transform;
    }



    void Update()
    {

        switch (state)
        {

            case AttackState.Windup:

                timer += Time.deltaTime;


                if (activeWarning)
                {
                    activeWarning.Process =
                    timer / windupTime;
                }


                if (timer >= windupTime)
                {
                    DoAttack();
                }


                break;



            case AttackState.Cooldown:

                timer -= Time.deltaTime;


                if (timer <= 0)
                {
                    state = AttackState.Ready;
                }

                break;

        }

    }





    public void TryAttack()
    {

        if (state != AttackState.Ready)
            return;



        StartWindup();

    }





    void StartWindup()
    {

        state = AttackState.Windup;


        timer = 0;



        //========================
        // 锁方向
        //========================

        Vector2 dir =
        player.position - transform.position;


        lockedDirection =
        Mathf.Atan2(dir.y, dir.x)
        * Mathf.Rad2Deg;



        shapeArea.Direction =
        lockedDirection;



        //========================
        //生成预警
        //========================

        if (sectorRangePrefab)
        {

            GameObject obj =
Instantiate(
    sectorRangePrefab,
    transform.position,
    Quaternion.Euler(0, 0, lockedDirection)
);


            obj.transform.SetParent(transform);



            activeWarning =
            obj.GetComponent<SectorRange>();



            activeWarning.SyncFromShapeArea(shapeArea);


            // 不再设置 Direction
            activeWarning.Direction = 0;

        }


    }






    void DoAttack()
    {


        state = AttackState.Attack;



        //攻击检测

        if (shapeArea.CheckTarget(player))
        {

            Player p =
            player.GetComponent<Player>();


            if (p)
                p.TakeDamage(damage);

        }



        EndAttack();

    }





    void EndAttack()
    {


        if (activeWarning)
        {

            activeWarning.DestroyWithFade();

            activeWarning = null;

        }



        state = AttackState.Cooldown;


        timer = cooldown;


    }



}