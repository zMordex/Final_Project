using System.Collections;
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
///   - isDashing        (trigger)
///   - isDead           (bool)
///   - yVelocity        (float)
///
/// En el componente PlayerInput:
///   - Behavior: Send Messages
///   - Actions: Move (Value, Vector2), Jump (Button), Dash (Button)
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
    [SerializeField] private float jumpForce         = 12f;
    [SerializeField] private float fallMultiplier    = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
 
    [Header("Detección de suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;
 
    [Header("Detección de pared")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckRadius = 0.15f;
 
    [Header("Doble Salto (Power-Up)")]
    [SerializeField] private bool canDoubleJump    = false;
    [SerializeField] private float doubleJumpForce = 10f;
 
    [Header("Dash (Power-Up)")]
    [SerializeField] private bool canDash       = false;
    [SerializeField] private float dashForce    = 18f;
    [SerializeField] private float dashDuration = 0.15f;   // duración del impulso
    [SerializeField] private float dashCooldown = 1f;      // tiempo entre dashes
 
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
 
    // ── Dash ──
    private bool dashPressed;
    private bool isDashing;
    private float dashCooldownTimer;   // tiempo restante de cooldown
 
    // ── Caché Animator ──
    private bool  cachedIsWalking;
    private bool  cachedIsGrounded;
    private float cachedYVelocity;
 
    // ── Caché flip ──
    private float lastFacingDirection = 1f;
 
    // ── Caché física de salto ──
    private float fallExtra;
    private float lowJumpExtra;
 
    // ── Caché wallCheck ──
    private float wallCheckSide;
 
    // Parámetros del Animator
    private const string ParamIsWalking       = "isWalking";
    private const string ParamIsGrounded      = "isGrounded";
    private const string ParamIsDoubleJumping = "isDoubleJumping";
    private const string ParamIsDashing       = "isDashing";
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
 
        fallExtra    = Physics2D.gravity.y * (fallMultiplier    - 1);
        lowJumpExtra = Physics2D.gravity.y * (lowJumpMultiplier - 1);
 
        if (wallCheck != null)
            wallCheckSide = wallCheck.localPosition.x > 0 ? 1f : -1f;
    }
 
    private void Update()
    {
        if (isDead) return;
 
        // Reducir el timer de cooldown del dash
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;
 
        HandleJump();
        HandleDash();
        FlipSprite();
        UpdateAnimator();
    }
 
    private void FixedUpdate()
    {
        if (isDead || isDashing) return;  // durante el dash no aplicar movimiento normal
 
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
 
    private void OnDash(InputValue value)
    {
        if (isDead) return;
        if (value.isPressed)
            dashPressed = true;
    }
 
    // ─────────────────────────────────────────
    //  MOVIMIENTO
    // ─────────────────────────────────────────
 
    private void Move()
    {
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
            SoundManager.Instance?.PlayJump();
            if (canDoubleJump)
                hasDoubleJump = true;
        }
        else if (canDoubleJump && hasDoubleJump)
        {
            Jump(doubleJumpForce);
            hasDoubleJump = false;
            anim.SetTrigger(ParamIsDoubleJumping);
            SoundManager.Instance?.PlayJump();
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
            rb.linearVelocity += Vector2.up * fallExtra * Time.fixedDeltaTime;
        else if (vy > 0 && !jumpHeld)
            rb.linearVelocity += Vector2.up * lowJumpExtra * Time.fixedDeltaTime;
    }
 
    // ─────────────────────────────────────────
    //  DASH
    // ─────────────────────────────────────────
 
    private void HandleDash()
    {
        if (!dashPressed) return;
        dashPressed = false;
 
        if (!canDash || isDashing || dashCooldownTimer > 0f) return;
 
        // Si no hay input, usa la dirección que mira el personaje
        float direction = horizontalInput != 0 ? Mathf.Sign(horizontalInput) : lastFacingDirection;
 
        StartCoroutine(DashRoutine(direction));
    }
 
    private IEnumerator DashRoutine(float direction)
    {
        isDashing         = true;
        dashCooldownTimer = dashCooldown;
 
        // Trigger de animación
        anim.SetTrigger(ParamIsDashing);
 
        // Sonido de dash
        SoundManager.Instance?.PlayDash();
 
        // Guardar y neutralizar la gravedad durante el dash
        float originalGravity = rb.gravityScale;
        rb.gravityScale       = 0f;
 
        // Aplicar impulso horizontal
        rb.linearVelocity = new Vector2(direction * dashForce, 0f);
 
        yield return new WaitForSeconds(dashDuration);
 
        // Restaurar gravedad y detener el impulso
        rb.gravityScale   = originalGravity;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
 
        isDashing = false;
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
        if (horizontalInput == 0 || isDashing) return;
 
        float direction = horizontalInput < 0 ? -1f : 1f;
 
        if (direction == lastFacingDirection) return;
        lastFacingDirection = direction;
 
        if (useSpriteFlip && spriteRenderer != null)
            spriteRenderer.flipX = direction < 0;
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            transform.localScale = scale;
        }
 
        if (wallCheck != null)
        {
            Vector3 pos = wallCheck.localPosition;
            pos.x = Mathf.Abs(pos.x) * direction;
            wallCheck.localPosition = pos;
            wallCheckSide = direction;
        }
    }
 
    // ─────────────────────────────────────────
    //  ANIMATOR
    // ─────────────────────────────────────────
 
    private void UpdateAnimator()
    {
        bool  isWalking = horizontalInput != 0 && isGrounded && !isDashing;
        float yVelocity = rb.linearVelocity.y;
 
        if (isWalking != cachedIsWalking)
        {
            anim.SetBool(ParamIsWalking, isWalking);
            cachedIsWalking = isWalking;
 
            // Sonido de caminata
            if (isWalking)
                SoundManager.Instance?.PlayWalk();
            else
                SoundManager.Instance?.StopWalk();
        }
 
        if (isGrounded != cachedIsGrounded)
        {
            anim.SetBool(ParamIsGrounded, isGrounded);
            cachedIsGrounded = isGrounded;
        }
 
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
 
        isDead    = true;
        isDashing = false;
 
        rb.linearVelocity = Vector2.zero;
        rb.bodyType       = RigidbodyType2D.Static;
 
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
        isDead            = false;
        isDashing         = false;
        isGrounded        = false;
        hasDoubleJump     = false;
        jumpPressed       = false;
        jumpHeld          = false;
        dashPressed       = false;
        dashCooldownTimer = 0f;
        horizontalInput   = 0f;
 
        cachedIsWalking  = true;
        cachedIsGrounded = true;
        cachedYVelocity  = float.MaxValue;
 
        rb.gravityScale   = 1f;
        rb.bodyType       = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
 
        anim.SetBool(ParamIsDead,     false);
        anim.SetBool(ParamIsWalking,  false);
        anim.SetBool(ParamIsGrounded, false);
        anim.Play("Idle", 0, 0f);
        anim.Update(0f);
    }
 
    // ─────────────────────────────────────────
    //  POWER-UPS
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
 
    public void EnableDash()
    {
        canDash = true;
    }
 
    public void DisableDash()
    {
        canDash = false;
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