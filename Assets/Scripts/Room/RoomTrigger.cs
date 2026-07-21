using UnityEngine;


public class RoomTrigger : MonoBehaviour
{

    public RoomSpawner spawner;


    public GameObject[] walls;



    private bool started = false;



    void OnTriggerEnter2D(Collider2D other)
    {


        if (other.CompareTag("Player") && !started)
        {

            started = true;


            LockRoom();


            spawner.StartRoom();

        }

    }




    void LockRoom()
    {

        foreach (GameObject wall in walls)
        {

            if (wall != null)
            {
                wall.SetActive(true);
            }

        }


        Debug.Log("房间锁定");

    }

}