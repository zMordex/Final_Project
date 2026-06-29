using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Maneja el soundtrack del juego con fade out/in entre escenas.
/// Colocalo en un GameObject vacío llamado "AudioManager" en la primera escena.
/// Se mantiene entre escenas automáticamente (DontDestroyOnLoad).
///
/// Configuración en el Inspector:
///   - Tracks        → lista de tracks, una por escena
///   - Fade Duration → duración del fade out/in en segundos
///   - Volume        → volumen general de la música (0 a 1)
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SceneTrack
    {
        public string sceneName;   // nombre exacto de la escena
        public AudioClip clip;     // canción asignada
    }

    [Header("Tracks por escena")]
    [SerializeField] private SceneTrack[] tracks;

    [Header("Configuración")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    // ─────────────────────────────────────────
    //  UNITY CALLBACKS
    // ─────────────────────────────────────────

    private void Awake()
    {
        // Singleton persistente entre escenas
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource          = GetComponent<AudioSource>();
        audioSource.loop     = true;
        audioSource.volume   = 0f;
        audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ─────────────────────────────────────────
    //  CAMBIO DE ESCENA
    // ─────────────────────────────────────────

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioClip newClip = GetClipForScene(scene.name);

        if (newClip == null)
        {
            // No hay track para esta escena: fade out y detener
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeOut(() => audioSource.Stop()));
            return;
        }

        // Si es la misma canción que ya está sonando, no interrumpir
        if (audioSource.clip == newClip && audioSource.isPlaying) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeToNewTrack(newClip));
    }

    private AudioClip GetClipForScene(string sceneName)
    {
        foreach (SceneTrack track in tracks)
        {
            if (track.sceneName == sceneName)
                return track.clip;
        }
        return null;
    }

    // ─────────────────────────────────────────
    //  FADE
    // ─────────────────────────────────────────

    private IEnumerator FadeToNewTrack(AudioClip newClip)
    {
        // Fade out si hay algo sonando
        if (audioSource.isPlaying)
            yield return StartCoroutine(FadeOut(null));

        // Cambiar clip y hacer fade in
        audioSource.clip = newClip;
        audioSource.Play();
        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator FadeOut(System.Action onComplete)
    {
        float startVolume = audioSource.volume;
        float elapsed     = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed            += Time.deltaTime;
            audioSource.volume  = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        onComplete?.Invoke();
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed            += Time.deltaTime;
            audioSource.volume  = Mathf.Lerp(0f, volume, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.volume = volume;
    }

    // ─────────────────────────────────────────
    //  CONTROL DE VOLUMEN (opcional, para un slider de opciones)
    // ─────────────────────────────────────────

    public void SetVolume(float newVolume)
    {
        volume                = Mathf.Clamp01(newVolume);
        audioSource.volume    = volume;
    }
}
