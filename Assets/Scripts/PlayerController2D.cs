using UnityEngine;
using UnityEngine.InputSystem;
 
/// <summary>
/// Controlador de personaje para side-scroller 2D.
/// Compatible con el nuevo Input System de Unity.
/// Requiere: Rigidbody2D, Collider2D, Animator, PlayerInput en el mismo GameObject.
///
/// Parámetros del Animator necesarios:
///   - isWalking        (bool)
///   - isGrounded       (bool)
///   - isDoubleJumping  (trigger)
///   - isDead           (bool)
///   - yVelocity        (float) → opcional
///
/// En el componente PlayerInput:
///   - Behavior: Send Messages
///   - Actions: Move (Value, Vector2) y Jump (Button)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController2D : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  INSPECTOR
    // ─────────────────────────────────────────
 
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 6f;
 
    [Header("Salto")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
 
    [Header("Detección de suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;
 
    [Header("Detección de pared")]
    [SerializeField] private Transform wallCheck;       // hijo vacío al costado del pj, a la altura del pecho
    [SerializeField] private float wallCheckRadius = 0.15f;
 
    [Header("Doble Salto (Power-Up)")]
    [SerializeField] private bool canDoubleJump = false;
    [SerializeField] private float doubleJumpForce = 10f;
 
    [Header("Flip")]
    [SerializeField] private bool useSpriteFlip = true;
 
    // ─────────────────────────────────────────
    //  REFERENCIAS PRIVADAS
    // ─────────────────────────────────────────
 
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
 
    // ─────────────────────────────────────────
    //  ESTADO
    // ─────────────────────────────────────────
 
    private bool isGrounded;
    private bool isTouchingWall;
    private bool hasDoubleJump;
    private bool isDead;
    private float horizontalInput;
    private bool jumpPressed;
    private bool jumpHeld;
 
    // Nombres de parámetros del Animator
    private const string ParamIsWalking       = "isWalking";
    private const string ParamIsGrounded      = "isGrounded";
    private const string ParamIsDoubleJumping = "isDoubleJumping";
    private const string ParamIsDead          = "isDead";
    private const string ParamYVelocity       = "yVelocity";
 
    // ─────────────────────────────────────────
    //  UNITY CALLBACKS
    // ─────────────────────────────────────────
 
    private void Awake()
    {
        rb             = GetComponent<Rigidbody2D>();
        anim           = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
 
    private void Update()
    {
        if (isDead) return;
 
        HandleJump();
        FlipSprite();
        UpdateAnimator();
    }
 
    private void FixedUpdate()
    {
        if (isDead) return;
 
        CheckGround();
        CheckWall();
        Move();
        ApplyBetterJumpPhysics();
    }
 
    // ─────────────────────────────────────────
    //  INPUT SYSTEM CALLBACKS
    // ─────────────────────────────────────────
 
    private void OnMove(InputValue value)
    {
        horizontalInput = value.Get<Vector2>().x;
    }
 
    private void OnJump(InputValue value)
    {
        if (isDead) return;
 
        if (value.isPressed)
        {
            jumpPressed = true;
            jumpHeld    = true;
        }
        else
        {
            jumpHeld = false;
        }
    }
 
    // ─────────────────────────────────────────
    //  MOVIMIENTO
    // ─────────────────────────────────────────
 
    private void Move()
    {
        // Si está en el aire tocando una pared y empuja hacia ella, bloquear movimiento horizontal
        if (!isGrounded && isTouchingWall && horizontalInput != 0)
        {
            // Detectar si está empujando hacia la pared comparando la dirección del input
            // con la dirección en que mira el wallCheck respecto al personaje
            float wallDirection = wallCheck.localPosition.x > 0 ? 1f : -1f;
            if (Mathf.Sign(horizontalInput) == wallDirection)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }
        }
 
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }
 
    // ─────────────────────────────────────────
    //  SALTO Y DOBLE SALTO
    // ─────────────────────────────────────────
 
    private void HandleJump()
    {
        if (!jumpPressed) return;
        jumpPressed = false;
 
        if (isGrounded)
        {
            Jump(jumpForce);
            if (canDoubleJump)
                hasDoubleJump = true;
        }
        else if (canDoubleJump && hasDoubleJump)
        {
            Jump(doubleJumpForce);
            hasDoubleJump = false;
            anim.SetTrigger(ParamIsDoubleJumping);
        }
    }
 
    private void Jump(float force)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
    }
 
    private void ApplyBetterJumpPhysics()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !jumpHeld)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
 
    // ─────────────────────────────────────────
    //  DETECCIÓN DE SUELO Y PARED
    // ─────────────────────────────────────────
 
    private void CheckGround()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
 
        if (!wasGrounded && isGrounded && canDoubleJump)
            hasDoubleJump = true;
    }
 
    private void CheckWall()
    {
        if (wallCheck == null) return;
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, groundLayer);
    }
 
    // ─────────────────────────────────────────
    //  FLIP DEL SPRITE
    // ─────────────────────────────────────────
 
    private void FlipSprite()
    {
        if (horizontalInput == 0) return;
 
        if (useSpriteFlip && spriteRenderer != null)
        {
            spriteRenderer.flipX = horizontalInput < 0;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (horizontalInput < 0 ? -1 : 1);
            transform.localScale = scale;
        }
 
        // Actualizar la posición del wallCheck según la dirección
        if (wallCheck != null)
        {
            Vector3 pos = wallCheck.localPosition;
            pos.x = Mathf.Abs(pos.x) * (horizontalInput < 0 ? -1 : 1);
            wallCheck.localPosition = pos;
        }
    }
 
    // ─────────────────────────────────────────
    //  ANIMATOR
    // ─────────────────────────────────────────
 
    private void UpdateAnimator()
    {
        anim.SetBool(ParamIsWalking,  horizontalInput != 0 && isGrounded);
        anim.SetBool(ParamIsGrounded, isGrounded);
        anim.SetFloat(ParamYVelocity, rb.linearVelocity.y);
    }
 
    // ─────────────────────────────────────────
    //  MUERTE
    // ─────────────────────────────────────────
 
    public void Die()
    {
        if (isDead) return;
 
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
 
        anim.SetBool(ParamIsDead, true);
 
        // Avisar al RespawnManager para que inicie el respawn con delay
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.StartRespawn();
        else
            Debug.LogWarning("PlayerController2D: no hay un RespawnManager en la escena.");
    }
 
    // ─────────────────────────────────────────
    //  RESPAWN
    // ─────────────────────────────────────────
 
    /// <summary>
    /// Llamado por RespawnManager cuando termina el delay de muerte.
    /// Resetea todo el estado del personaje.
    /// </summary>
    public void Respawn()
    {
        isDead          = false;
        isGrounded      = false;
        hasDoubleJump   = false;
        jumpPressed     = false;
        jumpHeld        = false;
        horizontalInput = 0f;
 
        rb.bodyType       = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
 
        // Resetear el Animator
        anim.SetBool(ParamIsDead,     false);
        anim.SetBool(ParamIsWalking,  false);
        anim.SetBool(ParamIsGrounded, false);
        anim.Play("Idle"); // el nombre debe coincidir exactamente con tu estado Idle en el Animator
    }
 
    // ─────────────────────────────────────────
    //  POWER-UP: DOBLE SALTO
    // ─────────────────────────────────────────
 
    public void EnableDoubleJump()
    {
        canDoubleJump = true;
        hasDoubleJump = !isGrounded;
        Debug.Log("¡Doble salto desbloqueado!");
    }
 
    public void DisableDoubleJump()
    {
        canDoubleJump = false;
        hasDoubleJump = false;
    }
 
    // ─────────────────────────────────────────
    //  GIZMOS
    // ─────────────────────────────────────────
 
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
 
        if (wallCheck != null)
        {
            Gizmos.color = isTouchingWall ? Color.blue : Color.cyan;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }
    }
}