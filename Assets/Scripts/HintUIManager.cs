using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla el cuadro de texto de los hints en pantalla.
/// Colocalo en el GameObject del Canvas que contiene el panel del hint.
///
/// Jerarquía esperada en el Canvas:
///   Canvas
///   └── HintPanel (GameObject con este script)
///       ├── Background (Image)
///       └── HintText   (TextMeshProUGUI)
///
/// Configuración en el Inspector:
///   - Hint Panel → el panel raíz del hint (se activa/desactiva)
///   - Hint Text  → el componente TextMeshProUGUI con el mensaje
/// </summary>
public class HintUIManager : MonoBehaviour
{
    public static HintUIManager Instance { get; private set; }

    [Header("Referencias UI")]
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI hintText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Asegurarse de que el panel empiece oculto
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    /// <summary>
    /// Muestra el cuadro de texto con el mensaje dado.
    /// </summary>
    public void ShowHint(string message)
    {
        if (hintPanel == null || hintText == null) return;

        hintText.text = message;
        hintPanel.SetActive(true);
    }

    /// <summary>
    /// Oculta el cuadro de texto.
    /// </summary>
    public void HideHint()
    {
        if (hintPanel == null) return;
        hintPanel.SetActive(false);
    }
}
