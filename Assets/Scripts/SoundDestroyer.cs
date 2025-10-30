using System.Collections;
using UnityEngine;

public class SoundDestroyer : MonoBehaviour
{
   private AudioSource audioSource;
    private float _clipLength;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private IEnumerator Start()
    {
        _clipLength = audioSource.clip.length;
        yield return new WaitForSeconds(_clipLength);
        Destroy(audioSource.gameObject); 


    }
}
