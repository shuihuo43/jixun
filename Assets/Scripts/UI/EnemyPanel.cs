using UnityEngine;
using UnityEngine.UI;

public class EnemyPanel : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private Image healthProcessImg;

    private EnemyResource resource;

    void Start()
    {
        if (enemy != null && enemy.resource != null)
        {
            resource = enemy.resource;
            resource.OnHealthChanged += Refresh;
            resource.OnDeath += () => gameObject.SetActive(false);
            Refresh();
        }
    }

    void OnDestroy()
    {
        if (resource != null)
            resource.OnHealthChanged -= Refresh;
    }

    void Refresh()
    {
        if (healthProcessImg != null && resource.maxHealth > 0f)
            healthProcessImg.fillAmount = resource.currentHealth / resource.maxHealth;
    }
}
