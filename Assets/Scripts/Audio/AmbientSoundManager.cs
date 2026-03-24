using UnityEngine;

/// <summary>
/// Quản lý ambient sound trong PlayScene.
/// Hỗ trợ cả 2D (nền) và 3D (spatial) audio.
/// </summary>
public class AmbientSoundManager : MonoBehaviour
{
    public static AmbientSoundManager Instance { get; private set; }

    [Header("Ambient Audio Sources")]
    [Tooltip("AudioSource phát tiếng chim hót")]
    public AudioSource birdSource;

    [Tooltip("AudioSource phát tiếng chợ ồn ào")]
    public AudioSource marketSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float birdVolume = 0.3f;
    [Range(0f, 1f)] public float marketVolume = 0.4f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        PlayAmbient(birdSource, birdVolume);
        PlayAmbient(marketSource, marketVolume);
    }

    private void PlayAmbient(AudioSource source, float volume)
    {
        if (source == null) return;
        source.volume = volume;
        source.loop = true;
        source.Play();
    }

    // --- Public API ---

    public void SetBirdVolume(float vol)
    {
        birdVolume = Mathf.Clamp01(vol);
        if (birdSource != null) birdSource.volume = birdVolume;
    }

    public void SetMarketVolume(float vol)
    {
        marketVolume = Mathf.Clamp01(vol);
        if (marketSource != null) marketSource.volume = marketVolume;
    }

    public void ToggleBird(bool on)
    {
        if (birdSource == null) return;
        if (on) birdSource.Play(); else birdSource.Stop();
    }

    public void ToggleMarket(bool on)
    {
        if (marketSource == null) return;
        if (on) marketSource.Play(); else marketSource.Stop();
    }
}
