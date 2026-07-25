using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("组件")]
    public EnemyStateMachine stateMachine;
    public EnemyPathfinding pathfinding;
    [SerializeField] private Collider2D hitCollider;

    [Header("资源")]
    public EnemyResource resource;

    [Header("朝向")]
    public Transform rotateRoot;

    [Header("属性")]
    public float moveSpeed = 3f;
    public float turnSpeed = 360f;

    [Header("音效")]
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private float hurtVolume = 0.8f;
    public float knockbackForce = 5f;
    public float knockbackAngleOffset = 15f;


    [Header("索敌")]
    public float detectionRadius = 8f;


    [Header("攻击")]
    public ShapeArea attackShape;
    public float attackCooldown = 1f;


    public bool IsAttackColdDown { get; private set; }
    public bool IsPlayerInAttackRange { get; private set; }


    private float attackCooldownTimer;


    public bool IsPlayerDetected { get; private set; }

    public Transform Player { get; private set; }


    [HideInInspector]
    public Vector2 moveDir;

    [HideInInspector]
    public Vector2 faceDir;


    private Rigidbody2D rb;

    internal Rigidbody2D Rigidbody2D => rb;


    private readonly Dictionary<string, float> hitRecords = new();



    void Awake()
    {
        if (stateMachine == null)
            stateMachine = GetComponent<EnemyStateMachine>();

        rb = GetComponent<Rigidbody2D>();

        if (resource != null)
        {
            resource = Instantiate(resource);
            resource.currentHealth = resource.maxHealth;
            resource.OnDeath += () =>
            {
                Debug.Log("Enemy: OnDeath fired, switching to Death state");
                stateMachine.ChangeToState("Death");
            };
        }

        GameObject obj =
            GameObject.FindGameObjectWithTag("Player");


        if (obj != null)
            Player = obj.transform;
    }



    void Update()
    {
        if (stateMachine.CurrentStateName == "Death") return;

        UpdateAttackCooldown();

        UpdateRotation();


        if (Player != null)
        {
            float distance =
                Vector2.Distance(
                    transform.position,
                    Player.position);


            IsPlayerDetected =
                distance <= detectionRadius &&
                HasLineOfSight();


            IsPlayerInAttackRange =
                CheckAttackRange();


            if (IsPlayerDetected)
                OnPlayerDetected();
        }
    }



    void UpdateAttackCooldown()
    {
        if (!IsAttackColdDown)
            return;


        attackCooldownTimer -= Time.deltaTime;


        if (attackCooldownTimer <= 0)
        {
            attackCooldownTimer = 0;
            IsAttackColdDown = false;
        }
    }



    public void StartAttackCooldown()
    {
        IsAttackColdDown = true;
        attackCooldownTimer = attackCooldown;
    }



    bool CheckAttackRange()
    {
        if (attackShape == null || Player == null)
            return false;


        Vector2 dir =
            Player.position -
            transform.position;


        float distance =
            dir.magnitude;



        if (attackShape.Shape ==
           ShapeArea.ShapeType.Sector)
        {
            if (distance > attackShape.Radius)
                return false;


            float targetAngle =
                Mathf.Atan2(
                    dir.y,
                    dir.x)
                * Mathf.Rad2Deg;


            float currentAngle =
                rotateRoot.eulerAngles.z;


            return Mathf.Abs(
                Mathf.DeltaAngle(
                    currentAngle,
                    targetAngle))
                <= attackShape.Angle / 2f;
        }



        if (attackShape.Shape ==
           ShapeArea.ShapeType.Box)
        {
            return
                Mathf.Abs(dir.x)
                <= attackShape.BoxWidth / 2f
                &&
                Mathf.Abs(dir.y)
                <= attackShape.BoxHeight / 2f;
        }


        return false;
    }




    bool HasLineOfSight()
    {
        Vector2 dir =
            Player.position -
            transform.position;


        RaycastHit2D hit =
            Physics2D.Raycast(
                transform.position,
                dir.normalized,
                dir.magnitude,
                LayerMask.GetMask("Wall"));


        return hit.collider == null;
    }




    void UpdateRotation()
    {
        if (rotateRoot == null)
            return;


        if (faceDir.sqrMagnitude < 0.001f)
            return;



        float target =
            Mathf.Atan2(
                faceDir.y,
                faceDir.x)
            * Mathf.Rad2Deg;


        float current =
            rotateRoot.eulerAngles.z;



        rotateRoot.rotation =
            Quaternion.Euler(
                0,
                0,
                Mathf.MoveTowardsAngle(
                    current,
                    target,
                    turnSpeed *
                    Time.deltaTime));
    }




    protected virtual void OnPlayerDetected()
    {

    }





    public void TakeDamage(float damage)
    {
        resource?.ChangeHealth(-damage);
    }




    void OnTriggerEnter2D(Collider2D other)
    {
        if (stateMachine.CurrentStateName == "Death") return;

        int layer = other.gameObject.layer;
        bool isPlayerAtk = layer == LayerMask.NameToLayer("PlayerAttack");
        bool isElementAtk = layer == LayerMask.NameToLayer("ElementAttack");
        if (!isPlayerAtk && !isElementAtk) return;

        string key = other.tag;
        if (hitRecords.TryGetValue(key, out float t) && Time.time < t) return;
        hitRecords[key] = Time.time + 0.05f;

        Entity source = other.GetComponent<Entity>();
        float dmg = source?.damageResource != null ? source.damageResource.baseDamageValue : 1f;
        TakeDamage(dmg);

        if (source?.owner != null)
        {
            var p = source.owner.GetComponent<Player>();
            if (p != null) { p.combo++; p.comboTime = p.comboMaxTime; }
        }

        AudioManager.Instance?.PlaySFX(hurtClip, hurtVolume);

        if (!isPlayerAtk) return;

        if (rb != null)
        {
            Vector2 away = ((Vector2)transform.position - (Vector2)Player.position).normalized;
            float angle = Mathf.Atan2(away.y, away.x) * Mathf.Rad2Deg;
            angle += Random.Range(-knockbackAngleOffset, knockbackAngleOffset);
            float rad = angle * Mathf.Deg2Rad;
            away = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            rb.velocity = away * knockbackForce;
        }

        stateMachine.ChangeToState("Hurt");
    }
}