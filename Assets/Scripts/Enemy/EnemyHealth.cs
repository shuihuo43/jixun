using UnityEngine;


public class EnemyHealth : MonoBehaviour
{
    public System.Action<GameObject> OnDeath;
    public int hp = 50;


    public void TakeDamage(int damage)
    {

        hp -= damage;


        if (hp <= 0)
        {
            Die();
        }

    }


    void Die()
    {
        OnDeath?.Invoke(gameObject);
        Destroy(gameObject);
    }

}