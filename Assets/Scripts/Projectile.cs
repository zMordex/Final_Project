using System.Collections;
using UnityEngine;
 
/// <summary>
/// Proyectil disparado por el Shooter.
/// Se destruye al tocar al jugador o cualquier objeto con tag "Ground".
/// El lifetime es un respaldo de seguridad para proyectiles que se pierdan.
///
/// El GameObject necesita:
///   - Rigidbody2D (Gravity Scale = 0)
///   - Collider2D con Is Trigger: ON
///   - El personaje debe tener el tag "Player"
///   - El Tilemap/suelo/paredes deben tener el tag "Ground"
///
/// Configuración en el Inspector:
///   - Speed          → velocidad del proyectil
///   - Max Lifetime   → lifetime de seguridad (valor alto, ej: 20)
///   - Collider Delay → delay antes de activar el collider
///   - Hit Effect     → efecto de partículas opcional al impactar
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed         = 8f;
    [SerializeField] private float maxLifetime   = 20f;  // solo seguridad
    [SerializeField] private float colliderDelay = 0.05f;
 
    [Header("Efecto al impactar (opcional)")]
    [SerializeField] private GameObject hitEffect;
 
    private Rigidbody2D rb;
    private Collider2D  col;
 
    private void Awake()
    {
        rb              = GetComponent<Rigidbody2D>();
        col             = GetComponent<Collider2D>();
        rb.gravityScale = 0f;
        col.enabled     = false;
    }
 
    private void Start()
    {
        Destroy(gameObject, maxLifetime);
        StartCoroutine(EnableColliderAfterDelay());
    }
 
    public void Initialize(Vector2 dir)
    {
        rb.linearVelocity = dir.normalized * speed;
    }
 
    private IEnumerator EnableColliderAfterDelay()
    {
        yield return new WaitForSeconds(colliderDelay);
        col.enabled = true;
    }
 
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Matar al jugador
        if (other.CompareTag("Player"))
        {
            PlayerController2D player = other.GetComponent<PlayerController2D>();
            if (player != null)
                player.Die();
 
            Impact();
            return;
        }
 
        // Destruirse al tocar el suelo o paredes
        if (other.CompareTag("Ground"))
        {
            Impact();
        }
    }
 
    private void Impact()
    {
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);
 
        Destroy(gameObject);
    }
}