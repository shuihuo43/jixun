using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("常态波动（底层）")]
    [SerializeField] private float wavePosAmp = 0.05f;
    [SerializeField] private float waveRotAmp = 2f;
    [SerializeField] private float waveFrequency = 1f;

    private CinemachineVirtualCamera vcam;
    private CinemachineFramingTransposer framing;
    private float shakeTimer;
    private float shakeDuration;
    private float shakePos;
    private float shakeRot;

    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        if (vcam == null) vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam != null)
            framing = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    void Update()
    {
        if (vcam == null) return;

        // ── 底层 wave（始终运行）──
        float wt = Time.time * waveFrequency;
        float wavePosX = Mathf.Sin(wt) * wavePosAmp;
        float wavePosY = Mathf.Cos(wt * 1.3f + 0.5f) * wavePosAmp * 0.6f;
        float waveRot = Mathf.Sin(wt) * waveRotAmp
                      + Mathf.Cos(wt * 1.3f + 0.5f) * waveRotAmp * 0.3f;

        // ── 上层 shake（叠加到 wave）──
        float shakePosX = 0f, shakePosY = 0f, shakeRotVal = 0f;
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(shakeTimer / shakeDuration);
            float decay = t * t;
            float seed = Time.time * 40f;

            shakePosX = (Mathf.PerlinNoise(seed, 0f) - 0.5f) * 2f * shakePos * decay;
            shakePosY = (Mathf.PerlinNoise(0f, seed) - 0.5f) * 2f * shakePos * decay;

            shakeRotVal = Mathf.Sin(Time.time * 35f) * shakeRot * decay
                        + Mathf.Cos(Time.time * 53f) * shakeRot * decay * 0.3f;

            if (shakeTimer <= 0f)
                shakePos = shakeRot = 0f;
        }

        // ── 输出：wave + shake ──
        vcam.m_Lens.Dutch = waveRot + shakeRotVal;
        if (framing != null)
            framing.m_TrackedObjectOffset = new Vector3(wavePosX + shakePosX, wavePosY + shakePosY, 0f);
    }

    public void Shake(float posIntensity, float rotIntensity, float duration)
    {
        shakePos = posIntensity;
        shakeRot = rotIntensity;
        shakeDuration = duration;
        shakeTimer = duration;
    }
}
