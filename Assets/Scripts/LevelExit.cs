using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Trigger de fin de nivel. Al tocarlo el personaje, hace un fade a negro
/// y carga la siguiente escena.
///
/// Configuración en el Inspector:
///   - Next Scene     → nombre exacto de la escena a cargar
///   - Fade Duration  → duración del fade a negro en segundos
///
/// El GameObject necesita:
///   - Collider2D con Is Trigger: ON
///   - El personaje debe tener el tag "Player"
///
/// Jerarquía del Canvas necesaria (se crea automáticamente si no existe):
///   Canvas (Screen Space - Overlay)
///   └── FadePanel (Image negra, color #000000, alpha 0 al inicio)
/// </summary>
public class LevelExit : MonoBehaviour
{
    [Header("Escena")]
    [SerializeField] private string nextScene = "";

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 1f;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;

        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        // Crear el panel de fade en runtime
        Canvas canvas = CreateFadeCanvas();
        Image fadePanel = canvas.GetComponentInChildren<Image>();

        float elapsed = 0f;
        Color color   = Color.black;

        // Fade a negro
        while (elapsed < fadeDuration)
        {
            elapsed     += Time.deltaTime;
            color.a      = Mathf.Clamp01(elapsed / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }

        fadePanel.color = Color.black;

        // Cargar la siguiente escena
        if (!string.IsNullOrEmpty(nextScene))
            SceneManager.LoadScene(nextScene);
        else
            Debug.LogWarning("LevelExit: no hay ninguna escena asignada en 'Next Scene'.");
    }

    /// <summary>
    /// Crea un Canvas con un panel negro en runtime para el fade.
    /// Si ya existe uno en la escena, lo reutiliza.
    /// </summary>
    private Canvas CreateFadeCanvas()
    {
        // Buscar si ya existe un Canvas de fade
        GameObject existing = GameObject.Find("FadeCanvas");
        if (existing != null)
            return existing.GetComponent<Canvas>();

        // Crear el Canvas
        GameObject canvasObj  = new GameObject("FadeCanvas");
        Canvas canvas         = canvasObj.AddComponent<Canvas>();
        canvas.renderMode     = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder   = 999; // encima de todo
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Crear el panel negro
        GameObject panelObj   = new GameObject("FadePanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image image           = panelObj.AddComponent<Image>();
        image.color           = new Color(0f, 0f, 0f, 0f); // transparente al inicio

        // Estirar el panel a toda la pantalla
        RectTransform rt      = panelObj.GetComponent<RectTransform>();
        rt.anchorMin          = Vector2.zero;
        rt.anchorMax          = Vector2.one;
        rt.offsetMin          = Vector2.zero;
        rt.offsetMax          = Vector2.zero;

        return canvas;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (TryGetComponent<Collider2D>(out var col))
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
    }
}
