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
    // 0°   = derecha      180° = izquierda
    // 90°  = arriba       270° = abajo
    // 45°  = diagonal arriba-derecha
    // 135° = diagonal arriba-izquierda
 
    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool goingForward = true;
 
    // Caché: umbral al cuadrado para evitar sqrt en el chequeo de llegada
    private const float ArrivalThresholdSqr = 0.01f * 0.01f;
 
    private void Start()
    {
        startPosition = transform.position;
 
        // DirectionFromAngle se llama una sola vez en Start, no cada frame
        endPosition = startPosition + DirectionFromAngle() * moveDistance;
    }
 
    private void Update()
    {
        Vector3 target = goingForward ? endPosition : startPosition;
 
        // Cachear moveSpeed * deltaTime para no multiplicarlo dos veces
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target, step);
 
        // sqrMagnitude en lugar de Vector3.Distance (evita raíz cuadrada)
        if ((transform.position - target).sqrMagnitude < ArrivalThresholdSqr)
            goingForward = !goingForward;
    }
 
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
 
        PlayerController2D player = other.GetComponent<PlayerController2D>();
        if (player != null)
            player.Die();
    }
 
    /// <summary>
    /// Convierte el ángulo en grados a un vector de dirección normalizado.
    /// Solo se llama en Start y en OnDrawGizmos (editor), nunca en Update.
    /// </summary>
    private Vector3 DirectionFromAngle()
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
    }
 
    // Dibuja el recorrido en el editor
    private void OnDrawGizmos()
    {
        Vector3 origin    = Application.isPlaying ? startPosition : transform.position;
        Vector3 direction = DirectionFromAngle();
        Vector3 end       = origin + direction * moveDistance;
 
        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, end);
        Gizmos.DrawWireSphere(origin, 0.15f);
        Gizmos.DrawWireSphere(end,    0.15f);
 
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + direction * 0.4f);
    }
}