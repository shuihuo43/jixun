using UnityEngine;
using System.Collections;


public class RoomTrigger : MonoBehaviour
{

    public RoomSpawner spawner;


    [Header("房间墙")]
    public GameObject[] walls;


    [Header("进入后多久封锁")]
    public float lockDelay = 1f;


    private bool started = false;



    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player") && !started)
        {
            started = true;


            StartCoroutine(StartRoomSequence());

        }

    }



    IEnumerator StartRoomSequence()
    {
        Debug.Log("玩家进入房间");

        yield return new WaitForSeconds(lockDelay);

        Debug.Log("开始生成墙");

        LockRoom();

        spawner.StartRoom();
    }




    void LockRoom()
    {

        foreach (GameObject wall in walls)
        {
            if (wall != null)
                wall.SetActive(true);
        }


        Debug.Log("房间封锁");

    }

}