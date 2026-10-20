using UnityEngine;

public enum Sfx { Hit, Coin, Chest, PowerUp, ShieldUp, ShieldBreak, Cannon, Explosion, Splash, WaveStart, GameOver, Click }

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip hitClip;
    public AudioClip coinClip;
    public AudioClip chestClip;
    public AudioClip powerUpClip;
    public AudioClip shieldUpClip;
    public AudioClip shieldBreakClip;
    public AudioClip cannonClip;
    public AudioClip explosionClip;
    public AudioClip splashClip;
    public AudioClip waveStartClip;
    public AudioClip gameOverClip;
    public AudioClip clickClip;
    public float hearingDistance = 14f;

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public static void Play(Sfx sfx, float volume = 1f)
    {
        if (instance == null) return;
        AudioClip clip = instance.ClipFor(sfx);
        if (clip != null) instance.sfxSource.PlayOneShot(clip, volume);
    }

    // Sons do mundo (canhões, explosões) ficam mais baixos longe da câmera.
    public static void PlayAt(Sfx sfx, Vector3 position)
    {
        if (instance == null || Camera.main == null) return;
        float distance = Vector2.Distance(Camera.main.transform.position, position);
        float volume = Mathf.Clamp01(1f - distance / instance.hearingDistance);
        if (volume > 0.02f) Play(sfx, volume);
    }

    public void PlayEnd()
    {
        musicSource.Stop();
        Play(Sfx.GameOver);
    }

    public void SetMusicPaused(bool paused)
    {
        if (paused) musicSource.Pause();
        else musicSource.UnPause();
    }

    AudioClip ClipFor(Sfx sfx)
    {
        switch (sfx)
        {
            case Sfx.Hit: return hitClip;
            case Sfx.Coin: return coinClip;
            case Sfx.Chest: return chestClip;
            case Sfx.PowerUp: return powerUpClip;
            case Sfx.ShieldUp: return shieldUpClip;
            case Sfx.ShieldBreak: return shieldBreakClip;
            case Sfx.Cannon: return cannonClip;
            case Sfx.Explosion: return explosionClip;
            case Sfx.Splash: return splashClip;
            case Sfx.WaveStart: return waveStartClip;
            case Sfx.GameOver: return gameOverClip;
            case Sfx.Click: return clickClip;
        }
        return null;
    }
}
