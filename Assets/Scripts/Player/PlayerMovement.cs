using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator animator;
    bool isFacingRight = true;
    public ParticleSystem smokeFX;
    public ParticleSystem speedFX;
    public ParticleSystem statusFX;
    BoxCollider2D playerCollider;
    TrailRenderer trailRenderer;
    private bool hasAirDashed = false;

    [Header("Movement")]
    public float moveSpeed = 5f;
    float horizontalMovement;
    float speedMultiplyer = 1f;

    [Header("Dashing")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.1f;
    bool isDashing;
    bool canDash = true;
    private GameObject dashHitbox;

    [Header("Post-Dash Speed")]
    public float postDashSpeedMultiplier = 2.083333f;
    private bool postDashBoost = false;

    [Header("Jumping")]
    public float jumpPower = 16f;
    public int maxJumps = 2;
    private int jumpsRemaining;
    private float coyoteTime = 0.1f;
    private float coyoteTimeCounter;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.49f, 0.03f);
    public LayerMask groundLayer;
    bool isGrounded;
    bool isOnPlatform;

    [Header("Gravity")]
    public float baseGravity = 4f;
    public float maxFallSpeed = 10f;
    public float fallGravityMult = 2f;

    [Header("WallCheck")]
    public Transform wallCheckPos;
    public Vector2 wallCheckSize = new Vector2(0.49f, 0.03f);
    public LayerMask wallLayer;

    [Header("WallMovement")]
    public float wallSlideSpeed = 2;
    bool isWallSliding;

    bool isWallJumping;
    float wallJumpDirection;
    float wallJumpTime = 0.5f;
    float wallJumpTimer;
    public Vector2 wallJumpPower = new Vector2(5f, 10f);

    private bool isPlayingFootsteps = false;

    [HideInInspector] public float defaultMoveSpeed;
    [HideInInspector] public float defaultJumpPower;

    [Header("Debuff")]
    public bool isDebuffActive = false;
    public GameObject dashHitboxPrefab;
    public GameObject wallBreakEffect;
    public GameObject debuffEffectPrefab;

    private GameObject activeDebuffEffect;

    public Transform attackOrigin;
    public float attackRadius = 1f;
    public LayerMask enemyMask;

    public float cooldownTime = .5f;
    private float cooldownTimer = 0f;

    public int attackDamage = 25;

    [Header("Player Clone")]
    public GameObject playerClonePrefab;
    private GameObject activeClone;


    private void Start()
    {
        defaultMoveSpeed = moveSpeed;
        defaultJumpPower = jumpPower;
        trailRenderer = GetComponent<TrailRenderer>();
        playerCollider = GetComponent<BoxCollider2D>();
        SpeedItem.OnSpeedCollected += StartSpeedBoost;
        speedFX.Stop();
    }

    void StartSpeedBoost(float multiplier)
    {
        StartCoroutine(SpeedBoostCoroutine(multiplier));
    }

    private IEnumerator SpeedBoostCoroutine(float multiplier)
    {
        speedMultiplyer = multiplier;
        speedFX.Play();
        yield return new WaitForSeconds(2f);
        speedMultiplyer = 1f;
        speedFX.Stop();
    }

    void Update()
    {
     animator.SetFloat("yVelocity", rb.velocity.y);
    animator.SetFloat("magnitude", rb.velocity.magnitude > 0.1 ? 1 : 0);
    animator.SetBool("isWallSliding", isWallSliding);

    if (isDebuffActive && Keyboard.current.cKey.wasPressedThisFrame)
    {
        SpawnPlayerClone();
    }

    if (isDashing) return;

    GroundCheck();

    // Reset post-dash boost when grounded and not dashing
    if (isGrounded && !isDashing && postDashBoost)
    {
        postDashBoost = false;
    }

    ProcessGravity();
    ProcessWallSlide();
    ProcessWallJump();
    ProcessMovement();
    HandleFootstepSounds();
    }

private void ProcessMovement()
{
    if (!isWallJumping)
    {
        float currentSpeed = moveSpeed * speedMultiplyer;
        if (postDashBoost)
            currentSpeed *= postDashSpeedMultiplier;
        rb.velocity = new Vector2(horizontalMovement * currentSpeed, rb.velocity.y);
        Flip();
    }
}
    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash && (isGrounded || !hasAirDashed))
        {
            if (!isGrounded)
            {
                hasAirDashed = true; // Mark air dash as used
            }
            StartCoroutine(DashCoroutine());
        }
    }

    public void Drop(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded && isOnPlatform)
        {
            StartCoroutine(DisablePlayerCollider(0.35f));
        }
    }

    private IEnumerator DisablePlayerCollider(float disableTime)
    {
        playerCollider.enabled = false;
        yield return new WaitForSeconds(disableTime);
        playerCollider.enabled = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnPlatform = false;
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (jumpsRemaining > 0)
        {
            if (context.performed)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpPower);
                jumpsRemaining--;
                JumpFX();
            }
            else if (context.canceled && rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
                jumpsRemaining--;
                JumpFX();
            }
        }

        if (context.performed && wallJumpTimer > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
            wallJumpTimer = 0;
            JumpFX();
            Flip(); // Flip immediately when wall jumping
            Invoke(nameof(CancelWallJump), wallJumpTime + 0.1f);
        }
    }

    private IEnumerator DashCoroutine()
    {
        Physics2D.IgnoreLayerCollision(7, 8, true);
        canDash = false;
        isDashing = true;
        trailRenderer.emitting = true;

        // Reset wall jump state to allow movement after dash
        isWallJumping = false;
        CancelInvoke(nameof(CancelWallJump));

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
        yield return null;

        float dashDirection = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(dashDirection * dashSpeed, 0f);

         // Enable dash hitbox during debuff
        if (isDebuffActive == true && dashHitboxPrefab != null)
        {
            Debug.Log("Dash Hit");
            PlayerMelee.AttackEnemy(attackOrigin, attackRadius, "Dash", enemyMask, attackDamage, transform.position, gameObject, cooldownTime, out cooldownTimer);
        }

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        rb.velocity = new Vector2(0f, rb.velocity.y);

        isDashing = false;
        trailRenderer.emitting = false;

         if (dashHitbox != null)
            Destroy(dashHitbox);

        Physics2D.IgnoreLayerCollision(7, 8, false);

        postDashBoost = true; // Enable post-dash speed boost

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }   

    private void JumpFX()
    {
        animator.SetTrigger("jump");
        smokeFX.Play();
    }

    private void GroundCheck()
    {
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            jumpsRemaining = maxJumps;
            isGrounded = true;
            coyoteTimeCounter = coyoteTime;
            hasAirDashed = false; // Reset air dash when grounded
        }
        else
        {
            isGrounded = false;
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, wallLayer);
    }

    private void ProcessGravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallGravityMult;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    private void ProcessWallSlide()
    {
        if (!isGrounded && WallCheck() && horizontalMovement != 0)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void ProcessWallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpTimer = wallJumpTime + 0.1f;
        }
        else if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }
    }

    private void CancelWallJump()
    {
        isWallJumping = false;
    }

    

    private void HandleFootstepSounds()
    {
        bool isMovingHorizontally = Mathf.Abs(horizontalMovement) > 0.1f;
        bool isOnGroundAndNotDashing = isGrounded && !isDashing && Mathf.Abs(rb.velocity.y) < 0.1f;

        if (isMovingHorizontally && isOnGroundAndNotDashing)
        {
            if (!isPlayingFootsteps)
            {
                SoundEffectManager.Play("Walk");
                isPlayingFootsteps = true;
            }
        }
        else
        {
            if (isPlayingFootsteps)
            {
                SoundEffectManager.Play("Walk");
                isPlayingFootsteps = false;
            }
        }
    }


    private void Flip()
    {
        if (isFacingRight && horizontalMovement < 0 || !isFacingRight && horizontalMovement > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
            speedFX.transform.localScale = ls;

            if (rb.velocity.y == 0)
            {
                smokeFX.Play();
            }
        }
    }

    private void SpawnPlayerClone()
    {
        if (activeClone != null)
        {
            Destroy(activeClone);
        }

        if (playerClonePrefab != null)
        {
            Vector3 spawnPosition = transform.position;
            Quaternion rotation = Quaternion.identity;
            activeClone = Instantiate(playerClonePrefab, spawnPosition, rotation);
        }
    }

    public bool IsDebuffActive()
    {
        return isDebuffActive;
    }

    public void ActivateDebuff()
    {
        if (isDebuffActive) return;

        isDebuffActive = true;
        moveSpeed *= 0.75f;
        jumpPower *= 0.75f;

        if (debuffEffectPrefab != null)
        {   
            statusFX.Play();
        }

        StartCoroutine(EndDebuffAfterDelay(5f));
    }

    private IEnumerator EndDebuffAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        moveSpeed = 5f;
        jumpPower = 16f;
        isDebuffActive = false;
        statusFX.Stop();
        if (activeDebuffEffect != null)
            Destroy(activeDebuffEffect);
            
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    }
}