using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip hitClip;
    public AudioClip winClip;
    public AudioClip loseClip;

    void Awake()
    {
        instance = this;
    }

    public void PlayHit()
    {
        if (hitClip != null) sfxSource.PlayOneShot(hitClip);
    }

    public void PlayEnd(bool won)
    {
        musicSource.Stop();
        AudioClip clip = won ? winClip : loseClip;
        if (clip != null) sfxSource.PlayOneShot(clip);
    }
}