using Unity.Mathematics;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Horizontal")]
    [SerializeField] private float moveSpeed;
    private float horizontalInput;
    private float directionFacing = 1f;
    private float timeSinceLastMove;

    [Header("Jump")]
    private bool isNormalGravity;
    [SerializeField] private float coyoteTimeDuration;
    private bool isGrounded;
    [SerializeField] private float jumpBufferDuration;
    [SerializeField] private float jumpStrength;
    [SerializeField] private float longerJumpDuration;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool hasJumped = false;
    private float longerJumpCounter;
    private float jumpInput;

    [Header("Wall Jump")]
    [SerializeField] private float wallSlideSpeed;
    [SerializeField] private float maxWallSlideSpeed;
    private float currentWallSlideSpeed;
    private bool isWallDetected;
    private float wallDirection;
    private bool isWallSticking;
    [SerializeField] private float wallJumpRecoveryDuration;
    private float wallJumpRecoveryCounter;
    [SerializeField] private float horizontalWallJumpStrength;
    private float verticalWallJumpStrength;
    [SerializeField] private float verticalWallJumpStrengthSet;

    [Header("Attack")]
    [SerializeField] private float attackBufferDuration;
    private float attackInput;
    private float attackBufferCounter;
    private bool hasAttacked;
    private bool isAttacking;
    private float attackDelayCounter;
    private float attackDelaySet = 0.5f;

    [Header("Dash")]
    [SerializeField] private float dashStrength;
    private float dashInput;
    private bool canDash;
    public float dashCooldownDuration;
    private float dashCooldownCounter;
    private bool hasDashed;
    private bool isDashing;
    private float dashDurationCounter;
    private float dashDelayCounter;
    [SerializeField] private float dashDuration;
    [SerializeField] private float dashDelay;

    [Header("References")]
    [SerializeField] private Rigidbody2D rigidBody2D;
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Transform wallCheckStart;
    [SerializeField] private Transform wallStickStart;
    [SerializeField] private Transform wallCheckEnd;
    [SerializeField] private Transform wallStickEnd;

    private InputAction dashAction;
    private InputAction horizontalAction;
    private InputAction jumpAction;

    private void Start()
    {
        dashAction = InputSystem.actions.FindAction("Dash");
        horizontalAction = InputSystem.actions.FindAction("Horizontal");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    private void Update()
    {
        animator.SetFloat("NMS", timeSinceLastMove);
        animator.SetFloat("Y_Movement", rigidBody2D.linearVelocity.y);

        animator.SetBool("IsWallSticking", isWallSticking);
        animator.SetBool("IsDashing", isDashing);

        dashInput = dashAction.ReadValue<float>();
        horizontalInput = horizontalAction.ReadValue<float>();
        jumpInput = jumpAction.ReadValue<float>();

        if (!isGrounded && !isNormalGravity || rigidBody2D.linearVelocity.y != 0)
        {
            SetGravityScale(4);
            isNormalGravity = false;
        }
        else
        {
            SetGravityScale(0);
        }
    }

    private void FixedUpdate()
    {
        HandleJump();
        HandleWallJump();
        HandleHorizontalMovement();
        HandleDash();
    }

    private void HandleHorizontalMovement()
    {
        if (rigidBody2D.linearVelocity.x == 0)
        {
            timeSinceLastMove += Time.deltaTime;
        }
        else
        {
            timeSinceLastMove = 0;
        }

        if (horizontalInput != 0 && !isDashing)
        {
            if (!isAttacking && wallJumpRecoveryCounter < 0 && !isWallSticking)
            {
                transform.localScale = new Vector3(directionFacing, 1, 1);
            }

            if (directionFacing != horizontalInput && horizontalInput != 0)
            {
                directionFacing = horizontalInput;
            }

            if (wallJumpRecoveryCounter < 0 && wallDirection * -1 != horizontalInput)
            {
                rigidBody2D.linearVelocity = new Vector2(moveSpeed * directionFacing, rigidBody2D.linearVelocity.y);
            }
        }
    }

    private void HandleJump()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTimeDuration;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (jumpInput > 0 && !hasJumped)
        {
            jumpBufferCounter = jumpBufferDuration;
            hasJumped = true;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpInput <= 0.1f)
        {
            longerJumpCounter = 0;
            hasJumped = false;
        }

        if (!isDashing)
        {
            if (coyoteTimeCounter > 0 && jumpBufferCounter > 0)
            {
                rigidBody2D.linearVelocity = new Vector2(rigidBody2D.linearVelocity.x, jumpStrength);
                coyoteTimeCounter = 0;
                jumpBufferCounter = 0;
                longerJumpCounter = longerJumpDuration;
                isGrounded = false;
            }
            else if (jumpInput > 0 && longerJumpCounter > 0)
            {
                if (rigidBody2D.linearVelocity.y < 0.1f)
                {
                    rigidBody2D.linearVelocity = new Vector2(rigidBody2D.linearVelocity.x, 0);
                    longerJumpCounter = 0;
                }

                rigidBody2D.linearVelocity = new Vector2(rigidBody2D.linearVelocity.x, jumpStrength);
                longerJumpCounter -= Time.deltaTime;
            }
            else if (rigidBody2D.linearVelocity.y <= 0)
            {
                SetGravityScale(4);
                longerJumpCounter = 0;
            }
        }
    }

     private void HandleWallJump()
    {
        wallJumpRecoveryCounter -= Time.deltaTime;

        isWallDetected = Physics2D.OverlapArea(wallCheckStart.position, wallCheckEnd.position, wallLayer);
        isWallSticking = Physics2D.OverlapArea(wallStickStart.position, wallStickEnd.position, wallLayer);

        if (!isWallSticking && !isWallDetected && wallJumpRecoveryCounter < 0)
        {
            wallDirection = 0;
        }

        if (isWallDetected && !isDashing && !isAttacking && !isGrounded)
        {
            currentWallSlideSpeed = wallSlideSpeed;
            wallJumpRecoveryCounter = 0;
            directionFacing *= -1;
            wallDirection = directionFacing;
            transform.localScale = new Vector3(wallDirection, 1, 1);
            isWallSticking = true;
        }

        if (isWallSticking && !isGrounded)
        {
            if (currentWallSlideSpeed < maxWallSlideSpeed)
                currentWallSlideSpeed += Time.deltaTime * 8;

            rigidBody2D.linearVelocity = new Vector2(wallDirection * -0.2f, -currentWallSlideSpeed);

            if (wallJumpRecoveryCounter < 0 && jumpBufferCounter > 0)
            {
                jumpBufferCounter = 0;
                wallJumpRecoveryCounter = wallJumpRecoveryDuration;
                verticalWallJumpStrength = verticalWallJumpStrengthSet;
                rigidBody2D.linearVelocity = new Vector2(horizontalWallJumpStrength * wallDirection, verticalWallJumpStrength);
            }
        }

        if (wallJumpRecoveryCounter > 0)
        {
            verticalWallJumpStrength -= 0.3f;
            rigidBody2D.linearVelocity = new Vector2(horizontalWallJumpStrength * wallDirection, verticalWallJumpStrength);
        }
    }

    private void HandleDash()
    {
        dashDelayCounter -= Time.deltaTime;
        dashDurationCounter -= Time.deltaTime;
        dashCooldownCounter -= Time.deltaTime;

        if (dashCooldownCounter < 0)
            canDash = true;

        if (canDash && dashInput > 0 && !hasDashed && dashDelayCounter < 0)
        {
            isDashing = true;
            canDash = false;
            hasDashed = true;
            dashCooldownCounter = dashCooldownDuration;
            dashDelayCounter = dashDelay;
            dashDurationCounter = dashDuration;
            longerJumpCounter = 0;

            animator.SetBool("IsDashing", true);
            SetGravityScale(0);
            rigidBody2D.linearVelocity = new Vector2(dashStrength * directionFacing, 0);
        }

        if (dashDurationCounter <= 0 && isDashing)
        {
            isDashing = false;
            SetGravityScale(4);
            rigidBody2D.linearVelocity = new Vector2(3 * directionFacing, 0);
            animator.SetBool("IsDashing", false);
        }

        if (dashInput <= 0)
            hasDashed = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleGroundCollision(collision, true);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleGroundCollision(collision, true);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        HandleGroundCollision(collision, false);
    }

    private void HandleGroundCollision(Collision2D collision, bool isTouching)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = isTouching;
            animator.SetBool("Grounded", isTouching);
            if (isTouching)
            {
                canDash = true;
                isNormalGravity = true;
            }
        }
    }

    private void SetGravityScale(float scale)
    {
        rigidBody2D.gravityScale = scale;
    }
}

