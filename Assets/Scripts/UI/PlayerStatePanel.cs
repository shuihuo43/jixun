using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatePanel : MonoBehaviour
{
    [SerializeField] private Image healthProcessImg;
    [SerializeField] private Image energyProcessImg;

    public void UpdateHealth(float health, float maxHealth)
    {
        healthProcessImg.fillAmount = health / maxHealth;
    }

    public void UpdateEnergy(float energy, float maxEnergy)
    {
        energyProcessImg.fillAmount = energy / maxEnergy;
    }
}
