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

    [Header("WandSprung")]
    [SerializeField] private  float SetWallSlidingSpeed;
    [SerializeField] private  float MaxWallSlidingSpeed;
    private float WallSlidingSpeed;
    private bool Wallfound;
    private float WallDirection;
    private bool IsWallStick;
    [SerializeField] private  float SetWallJumpRecoverTime;
    private float WallJumpRecoverTime;
    [SerializeField] private float HorizontalWJumpStrength;
    private  float VerticalWJumpStrength;
    [SerializeField] private  float SetVerticalWJumpStrength;

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
    [SerializeField] private Transform wallCheckA;
    [SerializeField] private Transform wallStickA;
    [SerializeField] private Transform wallCheckB;
    [SerializeField] private Transform wallStickB;

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

        animator.SetBool("IsWallSticking", IsWallStick);
        animator.SetBool("IsDashing", isDashing);

        dashInput = dashAction.ReadValue<float>();
        horizontalInput = horizontalAction.ReadValue<float>();
        jumpInput = jumpAction.ReadValue<float>();

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
            if (WallJumpRecoverTime < 0 && !IsWallStick)
            {
                transform.localScale = new Vector3(directionFacing, 1, 1);
            }

            if (directionFacing != horizontalInput && horizontalInput != 0)
            {
                directionFacing = horizontalInput;
            }

            if (WallJumpRecoverTime < 0 && WallDirection * -1 != horizontalInput)
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

        WallJumpRecoverTime -= Time.deltaTime;

        Wallfound = Physics2D.OverlapArea(wallCheckA.position,wallCheckB.position,wallLayer);
        IsWallStick = Physics2D.OverlapArea(wallStickA.position,wallStickB.position, wallLayer);

        if(!IsWallStick && !Wallfound && WallJumpRecoverTime < 0) {
            WallDirection = 0;
        
        }





        if(Wallfound && !isDashing && !isGrounded) {
            WallSlidingSpeed = SetWallSlidingSpeed;
            WallJumpRecoverTime = 0;
            directionFacing = directionFacing * -1;
            WallDirection = directionFacing;
            transform.localScale = new Vector3(1 * WallDirection, 1, 1);
            IsWallStick = true;
        }

        if(IsWallStick && !isGrounded) {
            if(WallSlidingSpeed < MaxWallSlidingSpeed)  WallSlidingSpeed += Time.deltaTime * 8;

            rigidBody2D.linearVelocity = new Vector2(WallDirection * 0.2f * -1, -WallSlidingSpeed);

            if(WallJumpRecoverTime < 0 && jumpBufferCounter > 0 ) {
                jumpBufferCounter = 0;
                WallJumpRecoverTime = SetWallJumpRecoverTime;
                VerticalWJumpStrength = SetVerticalWJumpStrength;
                rigidBody2D.linearVelocity = new Vector2(HorizontalWJumpStrength * WallDirection,VerticalWJumpStrength);
                
            }

        }

        if(WallJumpRecoverTime > 0) {
            VerticalWJumpStrength -= 0.3f;
            rigidBody2D.linearVelocity = new Vector2(HorizontalWJumpStrength * WallDirection, VerticalWJumpStrength);
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
            }
        }
    }

    private void SetGravityScale(float scale)
    {
        rigidBody2D.gravityScale = scale;
    }
}

