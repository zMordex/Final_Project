using UnityEngine;

/// <summary>
/// Power up de dash. Al tocarlo el jugador desbloquea
/// el dash permanentemente y el objeto desaparece.
///
/// El GameObject necesita:
///   - Collider2D con Is Trigger: ON
///   - El personaje debe tener el tag "Player"
///
/// Opcional:
///   - Efecto de partículas asignable en el Inspector
/// </summary>
public class DashPickup : MonoBehaviour
{
    [Header("Efecto visual (opcional)")]
    [SerializeField] private GameObject pickupEffect;

    [Header("Rotación decorativa")]
    [SerializeField] private bool rotate      = true;
    [SerializeField] private float rotateSpeed = 90f;

    [Header("Flotación decorativa")]
    [SerializeField] private bool  float_         = true;
    [SerializeField] private float floatAmplitude = 0.15f;
    [SerializeField] private float floatSpeed     = 2f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (rotate)
            transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

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

        player.EnableDash();

        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
