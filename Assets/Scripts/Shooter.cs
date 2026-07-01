using UnityEngine;
 
/// <summary>
/// Obstáculo que dispara proyectiles en una dirección fija cada X segundos.
/// No requiere Animator — la animación corre en loop por su cuenta.
/// El Fire Delay sincroniza el spawn del proyectil con el frame correcto
/// de la animación.
///
/// El GameObject necesita:
///   - Tag: "Shooter" (para que el proyectil lo ignore al nacer)
///
/// Configuración en el Inspector:
///   - Projectile Prefab → prefab del proyectil
///   - Fire Point        → hijo vacío desde donde sale el proyectil
///   - Fire Direction    → dirección del disparo (ej: (1,0) = derecha)
///   - Fire Rate         → segundos entre disparos (ajustalo a la duración
///                         del ciclo de tu animación)
///   - Fire Delay        → segundos desde el inicio del ciclo hasta que
///                         aparece el proyectil (ajustalo al frame correcto)
/// </summary>
public class Shooter : MonoBehaviour
{
    [Header("Proyectil")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform  firePoint;
 
    [Header("Disparo")]
    [SerializeField] private Vector2 fireDirection = Vector2.right;
    [SerializeField] private float   fireRate      = 2f;
    [SerializeField] private float   fireDelay     = 0.2f;
 
    private float timer;
    private void Start()
    {
        Debug.Log("Shooter iniciado correctamente");
        timer = fireRate;
    }
 
    private void Awake()
    {
        timer = fireRate;
    }
 
    private void Update()
    {
        timer += Time.deltaTime;
 
        if (timer >= fireRate)
        {
            timer = 0f;
            Invoke(nameof(SpawnProjectile), fireDelay);
        }
    }
 
    private void SpawnProjectile()
    {
        Debug.Log("SpawnProjectile llamado");
        
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("Shooter: falta asignar el Projectile Prefab o el Fire Point.");
            return;
        }
 
        GameObject obj        = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projectile = obj.GetComponent<Projectile>();
 
        if (projectile != null)
            projectile.Initialize(fireDirection);
        else
            Debug.LogWarning("Shooter: el prefab no tiene el componente Projectile.");
    }
 
    private void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;
 
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(firePoint.position, 0.1f);
 
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(firePoint.position, (Vector3)firePoint.position + (Vector3)fireDirection.normalized * 1.5f);
    }
    
}