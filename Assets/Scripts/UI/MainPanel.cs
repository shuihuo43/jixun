using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    [SerializeField] private Button startButton;

    void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(() => SceneManager.LoadScene("games"));
    }
}
