using UnityEngine;

public class AttackRange : MonoBehaviour
{
    private Enemy enemy;

    void Start()
    {
        enemy = GetComponentInParent<Enemy>();

        if (enemy == null)
            Debug.LogError("找不到Enemy");
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enemy?.TryAttack();
        }
    }


    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enemy?.TryAttack();
        }
    }
}