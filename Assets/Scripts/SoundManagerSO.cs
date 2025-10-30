using UnityEngine;

[CreateAssetMenu(menuName ="Audio/Sound Manager",fileName ="Sound Manager")]
public class SoundManagerSO : ScriptableObject
{   
    private static SoundManagerSO instance;
    public static SoundManagerSO Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<SoundManagerSO>("Sound Manager");
            }

            return instance;
        }
    }
    private static float _pitchChangeMultiplier = 0.1f;
    private static float _volumnChangeMultiplier = 0.15f;
    public AudioSource SoundObject;
    public void PlaySOundFXClip(AudioClip clip,Vector3 SoundPos, float volumn
        )
    {
        float randVolumn = Random.Range(volumn - _volumnChangeMultiplier, volumn);
        float randPitch = Random.Range(1 - _pitchChangeMultiplier, 1 + _pitchChangeMultiplier);

        AudioSource a = Instantiate(Instance.SoundObject, SoundPos, Quaternion.identity);
        a.clip = clip;
        a.volume = randVolumn;
        a.pitch = randPitch;
        a.Play();
    }


}
