using System;
using UnityEngine;

public class BossInitTrigger : MonoBehaviour
{
    public event Action onTriggered;

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Trigger Entered");
        if (other.CompareTag("Player"))
        {
            onTriggered?.Invoke();
            //Debug.Log("111");
        }
    }
}
