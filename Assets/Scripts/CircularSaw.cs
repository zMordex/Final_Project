using UnityEngine;
 
/// <summary>
/// Sierra circular que se mueve en ida y vuelta en cualquier dirección,
/// incluyendo diagonal con ángulo graduable.
///
/// Configuración en el Inspector:
///   - Move Distance  → distancia total que recorre desde su posición inicial
///   - Move Speed     → velocidad de movimiento
///   - Angle          → ángulo en grados (0 = derecha, 90 = arriba, 45 = diagonal)
///
/// El GameObject necesita:
///   - Collider2D con Is Trigger: ON
///   - El personaje debe tener el tag "Player"
/// </summary>
public class CircularSaw : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveDistance = 4f;
    [SerializeField] private float moveSpeed    = 3f;
 
    [Header("Dirección")]
    [Range(0f, 360f)]
    [SerializeField] private float angle = 0f;
    // 0°   = derecha
    // 90°  = arriba
    // 180° = izquierda
    // 270° = abajo
    // 45°  = diagonal arriba-derecha
    // 135° = diagonal arriba-izquierda
 
    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool goingForward = true;
 
    private void Start()
    {
        startPosition = transform.position;
        endPosition   = startPosition + DirectionFromAngle() * moveDistance;
    }
 
    private void Update()
    {
        Vector3 target = goingForward ? endPosition : startPosition;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
 
        if (Vector3.Distance(transform.position, target) < 0.01f)
            goingForward = !goingForward;
    }
 
    /// <summary>
    /// Convierte el ángulo en grados a un vector de dirección normalizado.
    /// </summary>
    private Vector3 DirectionFromAngle()
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
    }
 
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
 
        PlayerController2D player = other.GetComponent<PlayerController2D>();
        if (player != null)
            player.Die();
    }
 
    // Dibuja el recorrido en el editor y se actualiza en tiempo real al cambiar el ángulo
    private void OnDrawGizmos()
    {
        Vector3 origin    = Application.isPlaying ? startPosition : transform.position;
        Vector3 direction = DirectionFromAngle();
        Vector3 end       = origin + direction * moveDistance;
 
        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, end);
        Gizmos.DrawWireSphere(origin, 0.15f);
        Gizmos.DrawWireSphere(end,    0.15f);
 
        // Flecha indicando la dirección inicial
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + direction * 0.4f);
    }
}