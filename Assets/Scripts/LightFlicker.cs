using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Parpadeo suave para luces 2D de URP.
/// Colocalo en el mismo GameObject que el componente Light 2D.
///
/// Configuración en el Inspector:
///   - Min Intensity   → intensidad mínima del parpadeo
///   - Max Intensity   → intensidad máxima del parpadeo
///   - Flicker Speed   → qué tan rápido oscila la luz
///   - Smoothness      → qué tan suave es la transición (mayor = más suave)
/// </summary>
[RequireComponent(typeof(Light2D))]
public class LightFlicker : MonoBehaviour
{
    [Header("Intensidad")]
    [SerializeField] private float minIntensity = 0.8f;
    [SerializeField] private float maxIntensity = 1.2f;

    [Header("Velocidad")]
    [SerializeField] private float flickerSpeed = 1.5f;
    [SerializeField] private float smoothness   = 8f;

    private Light2D light2D;
    private float targetIntensity;
    private float noiseOffset;   // offset único por objeto para que no todas las luces parpadeen igual

    private void Awake()
    {
        light2D     = GetComponent<Light2D>();
        noiseOffset = Random.Range(0f, 100f); // cada luz tiene su propio ritmo
        targetIntensity = light2D.intensity;
    }

    private void Update()
    {
        // Perlin noise para un parpadeo orgánico (no aleatorio abrupto)
        float noise = Mathf.PerlinNoise(noiseOffset + Time.time * flickerSpeed, 0f);
        targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

        // Suavizar la transición
        light2D.intensity = Mathf.Lerp(light2D.intensity, targetIntensity, Time.deltaTime * smoothness);
    }
}
