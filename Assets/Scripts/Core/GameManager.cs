using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public PanelData panelData;

    public enum GameState { Playing, Paused }
    public GameState CurrentState { get; private set; } = GameState.Playing;
    public event Action<GameState> OnStateChanged;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CurrentState == GameState.Playing) Pause();
            else Resume();
        }
    }

    public void Pause()
    {
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
        OnStateChanged?.Invoke(CurrentState);
    }

    /// <summary>获取锁定的敌人（鼠标方向 cone 内垂距最近），没找到返回 null</summary>
    public Transform GetLockedEnemy(Vector3 playerPos, Vector3 mouseDir, float range, float angle)
    {
        float half = angle / 2f;
        Transform best = null;
        float bestDist = float.MaxValue;

        foreach (var e in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Vector3 toEnemy = e.transform.position - playerPos;
            float dist = toEnemy.magnitude;
            var en = e.GetComponent<Enemy>();
            if (en == null || (en.resource != null && en.resource.currentHealth <= 0f)) continue;
            if (dist > range) continue;
            if (Vector3.Angle(mouseDir, toEnemy.normalized) > half) continue;

            float perp = Vector3.Cross(mouseDir, toEnemy).magnitude;
            if (perp < bestDist) { bestDist = perp; best = e.transform; }
        }
        return best;
    }

    public void Resume()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        OnStateChanged?.Invoke(CurrentState);
    }
}
