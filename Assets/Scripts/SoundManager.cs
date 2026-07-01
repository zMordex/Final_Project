using UnityEngine;
 
/// <summary>
/// Maneja los efectos de sonido del personaje (walk, jump, dash).
/// Colocalo en el mismo GameObject que el AudioManager.
///
/// El panel y la barrera manejan su propio audio espacial
/// localmente, no usan este SoundManager.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
 
    [Header("Personaje")]
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip dashClip;
 
    [Header("Configuración")]
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;
 
    // AudioSource dedicado a SFX (separado del AudioSource de música)
    private AudioSource sfxSource;
 
    // AudioSource dedicado al walk (loop separado)
    private AudioSource walkSource;
 
    // ─────────────────────────────────────────
    //  UNITY CALLBACKS
    // ─────────────────────────────────────────
 
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
 
        Instance = this;
        DontDestroyOnLoad(gameObject);
 
        // SFX general (one-shot)
        sfxSource             = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop        = false;
        sfxSource.volume      = sfxVolume;
 
        // Walk en loop (AudioSource separado para no interrumpir otros sfx)
        walkSource             = gameObject.AddComponent<AudioSource>();
        walkSource.clip        = walkClip;
        walkSource.loop        = true;
        walkSource.playOnAwake = false;
        walkSource.volume      = sfxVolume;
    }
 
    // ─────────────────────────────────────────
    //  PERSONAJE
    // ─────────────────────────────────────────
 
    /// <summary>
    /// Llamar cuando el personaje empieza a caminar.
    /// </summary>
    public void PlayWalk()
    {
        if (walkSource == null || walkClip == null) return;
        if (!walkSource.isPlaying)
            walkSource.Play();
    }
 
    /// <summary>
    /// Llamar cuando el personaje deja de caminar.
    /// </summary>
    public void StopWalk()
    {
        if (walkSource != null && walkSource.isPlaying)
            walkSource.Stop();
    }
 
    public void PlayJump()  => PlaySFX(jumpClip);
    public void PlayDash()  => PlaySFX(dashClip);
 
    // ─────────────────────────────────────────
    //  VOLUMEN
    // ─────────────────────────────────────────
 
    public void SetSFXVolume(float newVolume)
    {
        sfxVolume        = Mathf.Clamp01(newVolume);
        sfxSource.volume = sfxVolume;
        walkSource.volume = sfxVolume;
    }
 
    // ─────────────────────────────────────────
    //  INTERNO
    // ─────────────────────────────────────────
 
    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}