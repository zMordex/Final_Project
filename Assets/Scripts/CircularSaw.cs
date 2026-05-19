using UnityEngine;

/// <summary>
/// Sierra circular que se mueve en ida y vuelta entre dos puntos.
/// Mata al jugador al tocarlo.
///
/// Configuración en el Inspector:
///   - Move Distance  → distancia total que recorre desde su posición inicial
///   - Move Speed     → velocidad de movimiento
///   - Move Direction → eje de movimiento (horizontal o vertical)
///
/// El GameObject necesita:
///   - Collider2D con Is Trigger: ON
///   - El personaje debe tener el tag "Player"
/// </summary>
public class CircularSaw : MonoBehaviour
{
    public enum Direction { Horizontal, Vertical }

    [Header("Movimiento")]
    [SerializeField] private float moveDistance = 4f;
    [SerializeField] private float moveSpeed    = 3f;
    [SerializeField] private Direction moveDirection = Direction.Horizontal;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool goingForward = true;

    private void Start()
    {
        startPosition = transform.position;

        // Calcular el punto de destino según la dirección elegida
        if (moveDirection == Direction.Horizontal)
            endPosition = startPosition + Vector3.right * moveDistance;
        else
            endPosition = startPosition + Vector3.up * moveDistance;
    }

    private void Update()
    {
        // Mover hacia el destino actual
        Vector3 target = goingForward ? endPosition : startPosition;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        // Cambiar dirección al llegar al destino
        if (Vector3.Distance(transform.position, target) < 0.01f)
            goingForward = !goingForward;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController2D player = other.GetComponent<PlayerController2D>();
        if (player != null)
            player.Die();
    }

    // Dibuja el recorrido en el editor
    private void OnDrawGizmos()
    {
        Vector3 origin = Application.isPlaying ? startPosition : transform.position;
        Vector3 end    = moveDirection == Direction.Horizontal
            ? origin + Vector3.right * moveDistance
            : origin + Vector3.up   * moveDistance;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, end);
        Gizmos.DrawWireSphere(origin, 0.15f);
        Gizmos.DrawWireSphere(end,    0.15f);
    }
}
