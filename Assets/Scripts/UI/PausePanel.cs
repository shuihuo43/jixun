using UnityEngine;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Toggle isDashAttackHelpToggle;
    [SerializeField] private PanelData panelData;

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += OnStateChanged;

        if (isDashAttackHelpToggle != null && panelData != null)
        {
            isDashAttackHelpToggle.isOn = panelData.isDashAttackHelp;
            isDashAttackHelpToggle.onValueChanged.AddListener(v => panelData.isDashAttackHelp = v);
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
