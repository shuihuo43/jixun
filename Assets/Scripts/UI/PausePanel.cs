using UnityEngine;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Toggle lockNearestToggle;
    [SerializeField] private Toggle lockDirectionalToggle;
    [SerializeField] private PanelData panelData;

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += OnStateChanged;

        if (panelData != null)
        {
            if (lockNearestToggle != null)
            {
                lockNearestToggle.isOn = panelData.lockNearestEnemy;
                lockNearestToggle.onValueChanged.AddListener(v =>
                {
                    panelData.lockNearestEnemy = v;
                    if (v && lockDirectionalToggle != null) lockDirectionalToggle.isOn = false;
                });
            }
            if (lockDirectionalToggle != null)
            {
                lockDirectionalToggle.isOn = panelData.lockDirectional;
                lockDirectionalToggle.onValueChanged.AddListener(v =>
                {
                    panelData.lockDirectional = v;
                    if (v && lockNearestToggle != null) lockNearestToggle.isOn = false;
                });
            }
        }

        panelRoot.SetActive(false);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
    }

    void OnStateChanged(GameManager.GameState state)
    {
        panelRoot?.SetActive(state == GameManager.GameState.Paused);
    }

    public void OnResumeButton()
    {
        GameManager.Instance?.Resume();
    }
}
