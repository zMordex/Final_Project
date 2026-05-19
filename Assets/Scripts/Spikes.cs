using UnityEngine;

/// <summary>
/// Asignalo al Tilemap de pinchos o a cualquier GameObject con Collider2D.
/// Mata al jugador instantáneamente al tocarlo.
///
/// Configuración:
///   - El Collider2D del Tilemap de pinchos debe tener Is Trigger: ON
///   - El personaje debe tener el tag "Player"
/// </summary>
public class Spikes : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController2D player = other.GetComponent<PlayerController2D>();
        if (player != null)
            player.Die();
    }
}
