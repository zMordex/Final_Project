using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menú principal del juego.
/// Colocalo en un GameObject vacío en la escena del menú.
///
/// Configuración en el Inspector:
///   - Scene To Load → nombre exacto de la escena del nivel (ej: "Nivel1")
///
/// Los botones del Canvas deben llamar a los métodos de este script
/// desde el evento OnClick().
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string sceneToLoad = "Nivel1";

    /// <summary>
    /// Asignar al botón "Jugar" en el evento OnClick().
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    /// <summary>
    /// Asignar al botón "Salir" en el evento OnClick().
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();

        // Solo se ve en el editor, en el build Application.Quit() cierra directo
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
