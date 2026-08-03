using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("音乐库")]
    [SerializeField] private AudioClip defaultMusic;
    [SerializeField] private MusicEntry[] musicLibrary;

    [System.Serializable]
    public struct MusicEntry
    {
        public string name;
        public AudioClip clip;
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (defaultMusic != null)
        {
            musicSource.clip = defaultMusic;
            musicSource.Play();
        }
    }

    public void SetMasterVolume(float v)  => mixer.SetFloat("MasterVolume", ToDB(v));
    public void SetSFXVolume(float v)    => mixer.SetFloat("SFXVolume", ToDB(v));
    public void SetMusicVolume(float v)  => mixer.SetFloat("MusicVolume", ToDB(v));

    public float GetMasterVolume()  { mixer.GetFloat("MasterVolume", out float v); return FromDB(v); }
    public float GetSFXVolume()     { mixer.GetFloat("SFXVolume", out float v); return FromDB(v); }
    public float GetMusicVolume()   { mixer.GetFloat("MusicVolume", out float v); return FromDB(v); }

    private Dictionary<AudioClip, int> activeCount = new();

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        activeCount.TryGetValue(clip, out int c);
        c++;
        activeCount[clip] = c;
        volume /= c; // 叠放均分音量

        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.PlayOneShot(clip, volume);

        StartCoroutine(DecayCount(clip, clip.length / Mathf.Abs(sfxSource.pitch) + 0.05f));
    }

    System.Collections.IEnumerator DecayCount(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        activeCount.TryGetValue(clip, out int c);
        if (c > 0) activeCount[clip] = c - 1;
    }

    public void PlayMusic(string name, float volume = 1f)
    {
        foreach (var m in musicLibrary)
            if (m.name == name)
            {
                musicSource.clip = m.clip;
                musicSource.volume = volume;
                musicSource.Play();
                return;
            }
    }

    static float ToDB(float t) => Mathf.Lerp(-80f, 0f, Mathf.Clamp01(t));
    static float FromDB(float db) => Mathf.InverseLerp(-80f, 0f, db);
}
