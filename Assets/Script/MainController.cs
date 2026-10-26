using UnityEngine;
using UnityEngine.InputSystem;

public class MainController : MonoBehaviour
{
    Rigidbody2D playerBody;
    Animator playerAnimator;
    Collider2D playerCollider;

    float horizontalMove;
    bool onGround;

    int usedJumpCount;
    float remainingCoyoteTime;
    float remainingJumpBufferTime;
    float remainingJumpLockTime;

    public float movementSpeed = 5f;
    public float jumpSpeed = 14f;
    public float defaultGravityScale = 4.5f;
    public float fallGravityMultiplier = 2.5f;
    public int maxJumpCount = 2;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    public float jumpLockTime = 0.08f;

    void Start()
    {
        playerBody = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();

        playerBody.gravityScale = defaultGravityScale;
        playerBody.freezeRotation = true;
        playerBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        playerBody.interpolation = RigidbodyInterpolation2D.Interpolate;

        PhysicsMaterial2D noStickMaterial = new PhysicsMaterial2D
        {
            friction = 0f,
            bounciness = 0f
        };
        playerBody.sharedMaterial = noStickMaterial;
    }

    void Update()
    {
        ReadMoveInput();
        ReadJumpInput();
        UpdateMoveAnimation();
    }

    void FixedUpdate()
    {
        UpdateGroundAndJumpCount();
        MoveHorizontally();
        TryJump();
        ApplyFallGravity();
    }

    void ReadMoveInput()
    {
        horizontalMove = 0f;

        if (Keyboard.current == null)
            return;

        bool leftPressed = Keyboard.current.aKey.isPressed;
        bool rightPressed = Keyboard.current.dKey.isPressed;

        if (leftPressed && !rightPressed)
            horizontalMove = -1f;
        else if (rightPressed && !leftPressed)
            horizontalMove = 1f;
    }

    void ReadJumpInput()
    {
        if (Keyboard.current != null && Keyboard.current.wKey.wasPressedThisFrame)
            remainingJumpBufferTime = jumpBufferTime;

        if (remainingJumpBufferTime > 0f)
            remainingJumpBufferTime -= Time.deltaTime;

        if (remainingJumpLockTime > 0f)
            remainingJumpLockTime -= Time.deltaTime;
    }

    void UpdateMoveAnimation()
    {
        if (playerAnimator == null)
            return;

        playerAnimator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if (horizontalMove != 0f)
        {
            playerAnimator.SetFloat("Look X", horizontalMove);
            playerAnimator.SetFloat("Look Y", 0f);
        }
    }

    void MoveHorizontally()
    {
        playerBody.linearVelocity = new Vector2(
            horizontalMove * movementSpeed,
            playerBody.linearVelocity.y
        );
    }

    void TryJump()
    {
        bool jumpBuffered = remainingJumpBufferTime > 0f;
        bool canJump = usedJumpCount < maxJumpCount;

        if (!jumpBuffered || !canJump)
            return;

        playerBody.linearVelocity = new Vector2(
            playerBody.linearVelocity.x,
            jumpSpeed
        );

        usedJumpCount++;
        remainingJumpBufferTime = 0f;
        remainingCoyoteTime = 0f;
        remainingJumpLockTime = jumpLockTime;
        onGround = false;
    }

    void ApplyFallGravity()
    {
        bool isFalling = !onGround && playerBody.linearVelocity.y < 0f;

        if (isFalling)
            playerBody.gravityScale = defaultGravityScale * fallGravityMultiplier;
        else
            playerBody.gravityScale = defaultGravityScale;
    }

    void UpdateGroundAndJumpCount()
    {
        bool touchingGround = IsStandingOnGround();
        bool jumpJustStarted = remainingJumpLockTime > 0f;

        if (touchingGround && !jumpJustStarted)
        {
            onGround = true;
            usedJumpCount = 0;
            remainingCoyoteTime = coyoteTime;
            return;
        }

        onGround = false;
        remainingCoyoteTime -= Time.fixedDeltaTime;

        // ?????????? ???????? ? ?????? ??? ?? ?????? ???
        if (usedJumpCount == 0 && remainingCoyoteTime <= 0f)
            usedJumpCount = 1;
    }

    bool IsStandingOnGround()
    {
        if (playerCollider == null)
            return false;

        Bounds colliderBounds = playerCollider.bounds;
        Vector2 checkCenter = new Vector2(colliderBounds.center.x, colliderBounds.min.y);
        Vector2 checkSize = new Vector2(colliderBounds.size.x * 0.9f, 0.08f);

        Collider2D[] groundHits = Physics2D.OverlapBoxAll(checkCenter, checkSize, 0f);

        for (int i = 0; i < groundHits.Length; i++)
        {
            Collider2D hit = groundHits[i];

            if (hit == null)
                continue;

            if (hit == playerCollider)
                continue;

            if (hit.isTrigger)
                continue;

            return true;
        }

        return false;
    }
}
