using System.Collections;
using UnityEngine;

/// <summary>
/// Maneja el respawn del jugador en un punto fijo del nivel.
/// Colocalo en un GameObject vacío en la escena.
///
/// Configuración en el Inspector:
///   - Spawn Point   → GameObject vacío que marca el punto de reaparición
///   - Respawn Delay → tiempo en segundos antes de reaparecer (usá la
///                     duración de la animación de muerte)
/// </summary>
public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Respawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float respawnDelay = 1.5f;

    private GameObject playerObj;
    private PlayerController2D playerController;

    private void Awake()
    {
        // Singleton para que PlayerController2D pueda encontrarlo fácilmente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        playerObj        = GameObject.FindGameObjectWithTag("Player");
        playerController = playerObj.GetComponent<PlayerController2D>();

        if (spawnPoint == null)
            Debug.LogWarning("RespawnManager: no hay un Spawn Point asignado.");
    }

    /// <summary>
    /// Llamado por PlayerController2D.Die() automáticamente.
    /// </summary>
    public void StartRespawn()
    {
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        // Esperar que termine la animación de muerte
        yield return new WaitForSeconds(respawnDelay);

        // Mover al jugador al spawn point
        playerObj.transform.position = spawnPoint.position;

        // Reiniciar el estado del personaje
        playerController.Respawn();
    }

    private void OnDrawGizmos()
    {
        if (spawnPoint == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(spawnPoint.position, 0.2f);
        Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up * 0.6f);
    }
}
