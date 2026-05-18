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
///   - Behavior: Send Messages  ← importante
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
 
    [Header("Doble Salto (Power-Up)")]
    [SerializeField] private bool canDoubleJump = false;
 
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
    private bool hasDoubleJump;
    private bool isDead;
    private float horizontalInput;
    private bool jumpPressed;       // se activa en OnJump, se consume en Update
    private bool jumpHeld;          // true mientras el botón esté apretado
 
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
        Move();
        ApplyBetterJumpPhysics();
    }
 
    // ─────────────────────────────────────────
    //  CALLBACKS DEL INPUT SYSTEM
    //  (PlayerInput con Behavior: Send Messages
    //   llama a estos métodos automáticamente)
    // ─────────────────────────────────────────
 
    // Se llama cuando cambia el eje de movimiento
    private void OnMove(InputValue value)
    {
        horizontalInput = value.Get<Vector2>().x;
    }
 
    // Se llama al presionar/soltar el botón Jump
    private void OnJump(InputValue value)
    {
        if (isDead) return;
 
        if (value.isPressed)
        {
            jumpPressed = true;   // consumido en HandleJump()
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
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }
 
    // ─────────────────────────────────────────
    //  SALTO Y DOBLE SALTO
    // ─────────────────────────────────────────
 
    private void HandleJump()
    {
        if (!jumpPressed) return;
        jumpPressed = false;   // consumir el evento
 
        if (isGrounded)
        {
            Jump();
            if (canDoubleJump)
                hasDoubleJump = true;
        }
        else if (canDoubleJump && hasDoubleJump)
        {
            Jump();
            hasDoubleJump = false;
            anim.SetTrigger(ParamIsDoubleJumping);
        }
    }
 
    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
 
    private void ApplyBetterJumpPhysics()
    {
        if (rb.linearVelocity.y < 0)
        {
            // Caída más pesada
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !jumpHeld)
        {
            // Salto corto si soltaste el botón
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
 
    // ─────────────────────────────────────────
    //  DETECCIÓN DE SUELO
    // ─────────────────────────────────────────
 
    private void CheckGround()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
 
        if (!wasGrounded && isGrounded && canDoubleJump)
            hasDoubleJump = true;
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
        Debug.Log("Player ha muerto.");
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
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}