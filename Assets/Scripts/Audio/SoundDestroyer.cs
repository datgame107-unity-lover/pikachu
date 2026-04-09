using System.Collections;
using UnityEngine;

/// <summary>
/// Self-destructs a GameObject once its <see cref="AudioSource"/> finishes playing.
/// Attach to the one-shot sound prefab used by <see cref="SoundManagerSO"/>.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SoundDestroyer : MonoBehaviour
{
    private AudioSource _audioSource;

    private void Awake() => _audioSource = GetComponent<AudioSource>();

    private IEnumerator Start()
    {
        if (_audioSource.clip != null)
            yield return new WaitForSeconds(_audioSource.clip.length);

        Destroy(gameObject);
    }
}