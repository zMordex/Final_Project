using UnityEngine;
 
/// <summary>
/// Sierra circular que se mueve en ida y vuelta en cualquier dirección,
/// incluyendo diagonal con ángulo graduable.
/// El audio espacial 3D lo maneja el AudioSource (Spatial Blend = 1).
///
/// Configuración en el Inspector:
///   - Move Distance  → distancia total que recorre desde su posición inicial
///   - Move Speed     → velocidad de movimiento
///   - Angle          → ángulo en grados (0 = derecha, 90 = arriba, 45 = diagonal)
///   - Saw Clip       → sonido en loop de la sierra (ej: sonido de motor/corte)
///
/// El GameObject necesita:
///   - Collider2D con Is Trigger: ON
///   - AudioSource configurado en 3D (Spatial Blend = 1, Logarithmic Rolloff)
///   - El personaje debe tener el tag "Player"
/// </summary>
[RequireComponent(typeof(AudioSource))]
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
 
    [Header("Sonido")]
    [SerializeField] private AudioClip sawClip;
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;
 
    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool goingForward = true;
    private AudioSource audioSource;
 
    // Caché: umbral al cuadrado para evitar sqrt en el chequeo de llegada
    private const float ArrivalThresholdSqr = 0.01f * 0.01f;
 
    private void Awake()
    {
        audioSource             = GetComponent<AudioSource>();
        audioSource.loop        = true;
        audioSource.playOnAwake = false;
        audioSource.volume      = volume;
    }
 
    private void Start()
    {
        startPosition = transform.position;
        endPosition   = startPosition + DirectionFromAngle() * moveDistance;
 
        // Arrancar el sonido en loop — el rolloff 3D maneja el volumen por distancia
        if (sawClip != null)
        {
            audioSource.clip = sawClip;
            audioSource.Play();
        }
    }
 
    private void Update()
    {
        Vector3 target = goingForward ? endPosition : startPosition;
 
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target, step);
 
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
 
    private Vector3 DirectionFromAngle()
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
    }
 
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
