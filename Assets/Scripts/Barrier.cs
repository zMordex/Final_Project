using UnityEngine;
 
/// <summary>
/// Barrera que puede ser desactivada por un InteractablePanel.
/// Reproduce su animación de desactivado y luego se desactiva
/// completamente (collider + sprite).
///
/// El GameObject de la barrera necesita:
///   - Animator con parámetros: isIdle (bool), isDeactivated (bool)
///   - Collider2D
/// </summary>
public class Barrier : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private string idleParam        = "isIdle";
    [SerializeField] private string deactivatedParam = "isDeactivated";
 
    [Header("Desactivación")]
    [Tooltip("Tiempo en segundos que tarda la animación de desactivado antes de ocultar el objeto.")]
    [SerializeField] private float deactivationDelay = 1f;
 
    private Animator anim;
    private Collider2D col;
 
    private void Awake()
    {
        anim = GetComponent<Animator>();
        col  = GetComponent<Collider2D>();
    }
 
    private void Start()
    {
        anim.SetBool(idleParam, true);
    }
 
    /// <summary>
    /// Llamado por InteractablePanel cuando el jugador activa el panel.
    /// </summary>
    public void Deactivate()
    {
        if (col != null)
        {
            col.enabled = false;
            Physics2D.SyncTransforms(); // fuerza que Unity actualice la física inmediatamente
        }

        anim.SetBool(idleParam, false);
        anim.SetBool(deactivatedParam, true);

        Invoke(nameof(HideBarrier), deactivationDelay);
    }   
 
    private void HideBarrier()
    {
        gameObject.SetActive(false);
    }
}
