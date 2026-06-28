using UnityEngine;
using UnityEngine.InputSystem;
 
/// <summary>
/// Panel interactivo compatible con el nuevo Input System.
/// Cuando el jugador está cerca y presiona la tecla de interacción,
/// reproduce su animación de desactivado y desactiva la barrera.
///
/// El GameObject del panel necesita:
///   - Animator con parámetros: isIdle (bool), isDeactivated (bool)
/// </summary>
public class InteractablePanel : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Barrier barrier;
 
    [Header("Interacción")]
    [SerializeField] private float interactionRange = 1.5f;
 
    [Header("Animator")]
    [SerializeField] private string idleParam        = "isIdle";
    [SerializeField] private string deactivatedParam = "isDeactivated";
 
    private Animator anim;
    private Transform player;
    private bool activated = false;
 
    // Caché: rango al cuadrado para evitar sqrt cada frame
    private float sqrInteractionRange;
 
    // Caché: posición del panel (no se mueve)
    private Vector3 cachedPosition;
 
    private InputAction interactAction;
 
    private void Awake()
    {
        anim = GetComponent<Animator>();
 
        // Precalcular rango al cuadrado una sola vez
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
    }
 
    private void OnEnable()  => interactAction.Enable();
    private void OnDisable() => interactAction.Disable();
 
    private void Update()
    {
        // Una vez activado, el Update no hace nada más
        if (activated || player == null) return;
 
        // sqrMagnitude en lugar de Vector2.Distance
        float sqrDistance = (cachedPosition - player.position).sqrMagnitude;
 
        if (sqrDistance <= sqrInteractionRange && interactAction.WasPressedThisFrame())
            Activate();
    }
 
    private void Activate()
    {
        activated = true;
 
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

