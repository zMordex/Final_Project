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
 
    // Acción del Input System
    private InputAction interactAction;
 
    private void Awake()
    {
        anim = GetComponent<Animator>();
 
        // Crear la acción de interacción directamente por código
        // Bindings: tecla E en teclado, botón Oeste (cuadrado/X) en gamepad
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
 
    private void OnEnable()
    {
        interactAction.Enable();
    }
 
    private void OnDisable()
    {
        interactAction.Disable();
    }
 
    private void Update()
    {
        if (activated || player == null) return;
 
        float distance = Vector2.Distance(transform.position, player.position);
 
        if (distance <= interactionRange && interactAction.WasPressedThisFrame())
        {
            Activate();
        }
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

