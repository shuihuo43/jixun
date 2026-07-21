using UnityEngine;
using System.Collections;


public class ExitPortal : MonoBehaviour
{

    [Header("目标点")]
    public Transform nextRoomPoint;


    [Header("黑屏")]
    public SceneFader fader;


    private bool used = false;


    private GameObject player;



    public void OpenPortal()
    {
        used = false;
        gameObject.SetActive(true);
    }



    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player") && !used)
        {

            used = true;

            player = other.gameObject;


            StartCoroutine(ChangeRoom());

        }

    }



    IEnumerator ChangeRoom()
    {


        //====================
        // 黑屏
        //====================

        yield return StartCoroutine(
            fader.FadeOut()
        );



        //====================
        // 玩家停止移动
        //====================

        Rigidbody2D rb =
        player.GetComponent<Rigidbody2D>();


        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }



        //====================
        // 传送
        //====================

        player.transform.position =
            nextRoomPoint.position;



        //====================
        // 等待房间稳定
        //====================

        yield return new WaitForSeconds(0.1f);



        //====================
        // 亮屏
        //====================

        yield return StartCoroutine(
            fader.FadeIn()
        );


    }

}