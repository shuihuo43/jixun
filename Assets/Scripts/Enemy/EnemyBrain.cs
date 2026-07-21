using UnityEngine;


public class EnemyBrain : MonoBehaviour
{

    public float moveSpeed = 3;
    public float detectionRange = 10;


    public EnemyAttackEntity attack;


    public Transform enemyRoot;


    private Transform player;


    public bool CanMove = true;



    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }



    void Update()
    {

        if (player == null)
            return;


        float distance =
            Vector2.Distance(enemyRoot.position, player.position);



        if (distance > detectionRange)
            return;



        //攻击中禁止移动
        if (!CanMove)
            return;



        if (distance > attack.AttackRange)
        {
            Move();
        }
        else
        {
            attack.TryAttack();
        }

    }



    void Move()
    {

        Vector2 dir =
        (player.position - enemyRoot.position).normalized;


        enemyRoot.position +=
        (Vector3)dir * moveSpeed * Time.deltaTime;


        enemyRoot.right = dir;

    }


}