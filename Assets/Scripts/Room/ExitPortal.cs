using UnityEngine;

public class ExitPortal : MonoBehaviour
{
    [Header("出生点")]
    public Transform bornPos;

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && bornPos != null)
            player.transform.position = bornPos.position;
    }

    void OnDrawGizmos()
    {
        if (bornPos != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(bornPos.position, 0.5f);
        }
    }
}
