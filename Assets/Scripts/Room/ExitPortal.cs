using UnityEngine;
using System.Collections;


public class ExitPortal : MonoBehaviour
{

    public Transform nextRoomPoint;

    public GameObject player;


    public SceneFader fader;


    private bool used = false;



    public void OpenPortal()
    {
        gameObject.SetActive(true);
    }



    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player") && !used)
        {

            used = true;


            StartCoroutine(ChangeRoom());

        }

    }



    IEnumerator ChangeRoom()
    {


        //黑屏
        yield return StartCoroutine(
            fader.FadeOut()
        );



        //传送玩家
        player.transform.position =
            nextRoomPoint.position;



        //等待一下
        yield return new WaitForSeconds(0.2f);



        //亮屏
        yield return StartCoroutine(
            fader.FadeIn()
        );


    }

}