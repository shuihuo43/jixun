using UnityEngine;
using UnityEngine.SceneManagement;


public class ExitPortal : MonoBehaviour
{

    public string nextSceneName;



    public void OpenPortal()
    {
        gameObject.SetActive(true);
    }



    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {

            SceneManager.LoadScene(nextSceneName);

        }

    }

}