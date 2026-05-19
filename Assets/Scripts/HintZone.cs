using UnityEngine;

/// <summary>
/// Asset que muestra un cuadro de texto al acercarse el jugador
/// y lo oculta al alejarse. No detiene al personaje.
///
/// Configuración en el Inspector:
///   - Message        → texto que se mostrará en el cuadro
///   - Trigger Range  → radio de detección (se puede ver como círculo verde en el editor)
///
/// El GameObject necesita:
///   - El personaje debe tener el tag "Player"
///
/// No necesita Collider2D propio, usa Physics2D.OverlapCircle para detectar
/// al jugador, así no interfiere con la física del personaje.
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

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("HintZone: no se encontró un GameObject con tag 'Player'.");
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (!isShowing && distance <= triggerRange)
        {
            isShowing = true;
            HintUIManager.Instance.ShowHint(message);
        }
        else if (isShowing && distance > triggerRange)
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
