using UnityEngine;
 
/// <summary>
/// Asset que muestra un cuadro de texto al acercarse el jugador
/// y lo oculta al alejarse. No detiene al personaje.
/// </summary>
public class HintZone : MonoBehaviour
{
    [Header("Mensaje")]
    [TextArea(2, 5)]
    [SerializeField] private string message = "Escribe aquí tu consejo...";
 
    [Header("Detección")]
    [SerializeField] private float triggerRange = 2.5f;
 
    private Transform player;
    private bool isShowing = false;
 
    // Caché: triggerRange al cuadrado para evitar sqrt cada frame
    private float sqrTriggerRange;
 
    // Caché: posición del HintZone (no se mueve, no hay que leerla cada frame)
    private Vector3 cachedPosition;
 
    private void Awake()
    {
        // Precalcular rango al cuadrado una sola vez
        sqrTriggerRange = triggerRange * triggerRange;
        cachedPosition  = transform.position;
    }
 
    private void Start()
    {
        // Guardar solo el Transform, no el GameObject completo
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("HintZone: no se encontró un GameObject con tag 'Player'.");
    }
 
    private void Update()
    {
        if (player == null) return;
 
        // sqrMagnitude evita la raíz cuadrada de Vector2.Distance
        float sqrDistance = (cachedPosition - player.position).sqrMagnitude;
 
        if (!isShowing && sqrDistance <= sqrTriggerRange)
        {
            isShowing = true;
            HintUIManager.Instance.ShowHint(message);
        }
        else if (isShowing && sqrDistance > sqrTriggerRange)
        {
            isShowing = false;
            HintUIManager.Instance.HideHint();
        }
    }
 
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerRange);
    }
}