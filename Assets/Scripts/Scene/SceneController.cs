using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    //点击开始按钮调用
    public void StartGame()
    {
        SceneManager.LoadScene(2);
    }


    //返回开始界面
    public void BackToStart()
    {
        SceneManager.LoadScene(1);
    }
}