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
    [SerializeField] private float jumpForce       = 12f;
    [SerializeField] private float fallMultiplier  = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
 
    [Header("Detección de suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;
 
    [Header("Detección de pared")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckRadius = 0.15f;
 
    [Header("Doble Salto (Power-Up)")]
    [SerializeField] private bool canDoubleJump   = false;
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
 
    // ── Caché para evitar writes innecesarios al Animator ──
    private bool  cachedIsWalking;
    private bool  cachedIsGrounded;
    private float cachedYVelocity;
 
    // ── Caché para evitar flip innecesario ──
    private float lastFacingDirection = 1f; // 1 = derecha, -1 = izquierda
 
    // ── Caché para evitar recalcular multiplicadores cada FixedUpdate ──
    private float fallExtra;     // gravity.y * (fallMultiplier - 1)
    private float lowJumpExtra;  // gravity.y * (lowJumpMultiplier - 1)
 
    // ── Caché de la dirección del wallCheck ──
    private float wallCheckSide; // 1 = derecha, -1 = izquierda
 
    // Parámetros del Animator
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
 
        // Precalcular multiplicadores de salto (solo se calculan una vez)
        fallExtra    = Physics2D.gravity.y * (fallMultiplier    - 1);
        lowJumpExtra = Physics2D.gravity.y * (lowJumpMultiplier - 1);
 
        // Cachear la dirección del wallCheck
        if (wallCheck != null)
            wallCheckSide = wallCheck.localPosition.x > 0 ? 1f : -1f;
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
        // Usa wallCheckSide cacheado en lugar de leer localPosition cada frame
        if (!isGrounded && isTouchingWall && horizontalInput != 0)
        {
            if (Mathf.Sign(horizontalInput) == wallCheckSide)
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
        float vy = rb.linearVelocity.y;
 
        if (vy < 0)
        {
            // Usa fallExtra precalculado en Awake
            rb.linearVelocity += Vector2.up * fallExtra * Time.fixedDeltaTime;
        }
        else if (vy > 0 && !jumpHeld)
        {
            // Usa lowJumpExtra precalculado en Awake
            rb.linearVelocity += Vector2.up * lowJumpExtra * Time.fixedDeltaTime;
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
 
        float direction = horizontalInput < 0 ? -1f : 1f;
 
        // Solo aplicar flip si la dirección realmente cambió
        if (direction == lastFacingDirection) return;
        lastFacingDirection = direction;
 
        if (useSpriteFlip && spriteRenderer != null)
        {
            spriteRenderer.flipX = direction < 0;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            transform.localScale = scale;
        }
 
        // Actualizar wallCheck y su caché
        if (wallCheck != null)
        {
            Vector3 pos = wallCheck.localPosition;
            pos.x = Mathf.Abs(pos.x) * direction;
            wallCheck.localPosition = pos;
            wallCheckSide = direction; // actualizar caché
        }
    }
 
    // ─────────────────────────────────────────
    //  ANIMATOR
    // ─────────────────────────────────────────
 
    private void UpdateAnimator()
    {
        bool  isWalking = horizontalInput != 0 && isGrounded;
        float yVelocity = rb.linearVelocity.y;
 
        // Solo llamar SetBool/SetFloat si el valor realmente cambió
        if (isWalking != cachedIsWalking)
        {
            anim.SetBool(ParamIsWalking, isWalking);
            cachedIsWalking = isWalking;
        }
 
        if (isGrounded != cachedIsGrounded)
        {
            anim.SetBool(ParamIsGrounded, isGrounded);
            cachedIsGrounded = isGrounded;
        }
 
        // Para el float usamos una tolerancia pequeña para evitar updates por micro-variaciones
        if (Mathf.Abs(yVelocity - cachedYVelocity) > 0.01f)
        {
            anim.SetFloat(ParamYVelocity, yVelocity);
            cachedYVelocity = yVelocity;
        }
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
 
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.StartRespawn();
        else
            Debug.LogWarning("PlayerController2D: no hay un RespawnManager en la escena.");
    }
 
    // ─────────────────────────────────────────
    //  RESPAWN
    // ─────────────────────────────────────────
 
    public void Respawn()
    {
        isDead          = false;
        isGrounded      = false;
        hasDoubleJump   = false;
        jumpPressed     = false;
        jumpHeld        = false;
        horizontalInput = 0f;
 
        // Resetear cachés para que UpdateAnimator vuelva a escribir los valores
        cachedIsWalking  = true;  // forzar diferencia en el próximo Update
        cachedIsGrounded = true;
        cachedYVelocity  = float.MaxValue;
 
        rb.bodyType       = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
 
        anim.SetBool(ParamIsDead,     false);
        anim.SetBool(ParamIsWalking,  false);
        anim.SetBool(ParamIsGrounded, false);
        anim.Play("Idle-character", 0, 0f);
        anim.Update(0f);
    }
 
    // ─────────────────────────────────────────
    //  POWER-UP: DOBLE SALTO
    // ─────────────────────────────────────────
 
    public void EnableDoubleJump()
    {
        canDoubleJump = true;
        hasDoubleJump = !isGrounded;
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