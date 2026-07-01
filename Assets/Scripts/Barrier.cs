using UnityEngine;
 
/// <summary>
/// Barrera que puede ser desactivada por un InteractablePanel.
/// El fade de audio por distancia lo maneja el AudioSource en 3D (Spatial Blend = 1).
///
/// El GameObject de la barrera necesita:
///   - Animator con parámetros: isIdle (bool), isDeactivated (bool)
///   - Collider2D
///   - AudioSource configurado en 3D (Spatial Blend = 1, Logarithmic Rolloff)
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Barrier : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private string idleParam        = "isIdle";
    [SerializeField] private string deactivatedParam = "isDeactivated";
 
    [Header("Desactivación")]
    [Tooltip("Debe coincidir con la duración de la animación de desactivado.")]
    [SerializeField] private float deactivationDelay = 1f;
 
    [Header("Sonido")]
    [SerializeField] private AudioClip idleClip;
    [SerializeField] private AudioClip deactivateClip;
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;
 
    private Animator anim;
    private Collider2D col;
    private AudioSource audioSource;
 
    private void Awake()
    {
        anim        = GetComponent<Animator>();
        col         = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
 
        audioSource.loop        = true;
        audioSource.playOnAwake = false;
        audioSource.volume      = volume;
    }
 
    private void Start()
    {
        anim.SetBool(idleParam, true);
 
        // Arrancar el idle — el AudioSource 3D maneja el volumen por distancia
        if (idleClip != null)
        {
            audioSource.clip = idleClip;
            audioSource.Play();
        }
    }
 
    /// <summary>
    /// Llamado por InteractablePanel cuando el jugador activa el panel.
    /// </summary>
    public void Deactivate()
    {
        if (col != null)
            col.enabled = false;
 
        // Detener idle y reproducir sonido de desactivación
        audioSource.Stop();
        audioSource.loop = false;
        if (deactivateClip != null)
            audioSource.PlayOneShot(deactivateClip, volume);
 
        anim.SetBool(idleParam, false);
        anim.SetBool(deactivatedParam, true);
 
        Invoke(nameof(HideBarrier), deactivationDelay);
    }
 
    private void HideBarrier()
    {
        gameObject.SetActive(false);
    }
}
 