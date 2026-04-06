using UnityEngine;

/// <summary>
/// ScriptableObject singleton that handles pooled/one-shot sound effect playback.
/// Place the asset at <c>Resources/Sound Manager</c> so it can be loaded on demand.
/// Create via <c>Assets → Create → Audio → Sound Manager</c>.
/// </summary>
[CreateAssetMenu(menuName = "Audio/Sound Manager", fileName = "Sound Manager")]
public class SoundManagerSO : ScriptableObject
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------

    private static SoundManagerSO _instance;

    public static SoundManagerSO Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<SoundManagerSO>("Sound Manager");

            if (_instance == null)
                Debug.LogError("[SoundManagerSO] Could not load 'Sound Manager' from Resources.");

            return _instance;
        }
    }

    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Tooltip("Prefab with an AudioSource component. Destroyed automatically after the clip finishes.")]
    public AudioSource SoundObjectPrefab;

    // -------------------------------------------------------------------------
    // Configuration
    // -------------------------------------------------------------------------

    [SerializeField, Range(0f, 0.5f)]
    private float pitchVariance = 0.1f;

    [SerializeField, Range(0f, 0.5f)]
    private float volumeVariance = 0.15f;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Instantiates a one-shot audio source at <paramref name="worldPosition"/> and
    /// plays <paramref name="clip"/> with subtle pitch/volume randomisation.
    /// </summary>
    public void PlaySoundFX(AudioClip clip, Vector3 worldPosition, float baseVolume)
    {
        if (clip == null || SoundObjectPrefab == null) return;

        float randVolume = Mathf.Clamp01(Random.Range(baseVolume - volumeVariance, baseVolume));
        float randPitch = Random.Range(1f - pitchVariance, 1f + pitchVariance);

        AudioSource source = Instantiate(SoundObjectPrefab, worldPosition, Quaternion.identity);
        source.clip = clip;
        source.volume = randVolume;
        source.pitch = randPitch;
        source.Play();
    }
}