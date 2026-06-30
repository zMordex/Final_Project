using UnityEngine;
using UnityEngine.InputSystem;
 
/// <summary>
/// Panel interactivo compatible con el nuevo Input System.
/// El fade de audio por distancia lo maneja el AudioSource en 3D (Spatial Blend = 1).
///
/// El GameObject del panel necesita:
///   - Animator con parámetros: isIdle (bool), isDeactivated (bool)
///   - AudioSource configurado en 3D (Spatial Blend = 1, Logarithmic Rolloff)
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class InteractablePanel : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Barrier barrier;
 
    [Header("Interacción")]
    [SerializeField] private float interactionRange = 1.5f;
 
    [Header("Sonido")]
    [SerializeField] private AudioClip idleClip;
    [SerializeField] private AudioClip activateClip;
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;
 
    [Header("Animator")]
    [SerializeField] private string idleParam        = "isIdle";
    [SerializeField] private string deactivatedParam = "isDeactivated";
 
    private Animator anim;
    private AudioSource audioSource;
    private Transform player;
    private bool activated = false;
 
    // Caché
    private float sqrInteractionRange;
    private Vector3 cachedPosition;
 
    private InputAction interactAction;
 
    private void Awake()
    {
        anim        = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
 
        // El rolloff y spatial blend se configuran en el Inspector del AudioSource
        audioSource.loop        = true;
        audioSource.playOnAwake = false;
        audioSource.volume      = volume;
 
        sqrInteractionRange = interactionRange * interactionRange;
        cachedPosition      = transform.position;
 
        interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
        interactAction.AddBinding("<Gamepad>/buttonWest");
    }
 
    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("InteractablePanel: no se encontró un GameObject con tag 'Player'.");
 
        anim.SetBool(idleParam, true);
 
        // Arrancar el idle — el AudioSource 3D maneja el volumen por distancia
        if (idleClip != null)
        {
            audioSource.clip = idleClip;
            audioSource.Play();
        }
    }
 
    private void OnEnable()  => interactAction.Enable();
    private void OnDisable() => interactAction.Disable();
 
    private void Update()
    {
        if (activated || player == null) return;
 
        float sqrDistance = (cachedPosition - player.position).sqrMagnitude;
 
        if (sqrDistance <= sqrInteractionRange && interactAction.WasPressedThisFrame())
            Activate();
    }
 
    private void Activate()
    {
        activated = true;
 
        // Detener idle y reproducir sonido de activación
        audioSource.Stop();
        audioSource.loop = false;
        if (activateClip != null)
            audioSource.PlayOneShot(activateClip, volume);
 
        anim.SetBool(idleParam, false);
        anim.SetBool(deactivatedParam, true);
 
        if (barrier != null)
            barrier.Deactivate();
        else
            Debug.LogWarning("InteractablePanel: no hay ninguna barrera asignada.");
    }
 
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}