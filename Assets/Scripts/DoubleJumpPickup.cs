using UnityEngine;

/// <summary>
/// Power up de doble salto. Al tocarlo el jugador desbloquea
/// el doble salto permanentemente y el objeto desaparece.
///
/// El GameObject necesita:
///   - Collider2D con Is Trigger: ON
///   - El personaje debe tener el tag "Player"
///
/// Opcional:
///   - Animator con parámetro isFloating (bool) para animación de flotación
///   - Un efecto de partículas asignable en el Inspector
/// </summary>
public class DoubleJumpPickup : MonoBehaviour
{
    [Header("Efecto visual (opcional)")]
    [SerializeField] private GameObject pickupEffect; // partículas o flash al recogerlo

    [Header("Rotación decorativa")]
    [SerializeField] private bool rotate = true;
    [SerializeField] private float rotateSpeed = 90f;

    [Header("Flotación decorativa")]
    [SerializeField] private bool float_ = true;
    [SerializeField] private float floatAmplitude = 0.15f;
    [SerializeField] private float floatSpeed     = 2f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // Rotación
        if (rotate)
            transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        // Flotación suave en Y
        if (float_)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController2D player = other.GetComponent<PlayerController2D>();
        if (player == null) return;

        // Desbloquear el doble salto permanentemente
        player.EnableDoubleJump();

        // Spawnear efecto visual si hay uno asignado
        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        // Destruir el power up
        Destroy(gameObject);
    }
}
