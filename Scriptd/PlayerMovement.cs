using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    #region Horizontal
    [Header("Horizontal")]
    [SerializeField] private float moveSpeed;
    private float horizontalInput;
    private float directionFacing = 1f;
    private float timeSinceLastMove;
    #endregion

    #region Jump
    [Header("Jump")]
    [SerializeField] private float coyoteTimeDuration;
    private bool isGrounded;
    [SerializeField] private float SetJumpBufferDuration;
    [SerializeField] private float jumpStrength;
    [SerializeField] private float longerJumpDuration;
    private float coyoteTimeCounter;
    private float JumpBufferDuration;
    private bool hasJumped = false;
    private float longerJumpCounter;
    private float jumpInput;
    #endregion

    #region Wandsprung
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
    #endregion

    #region Dash
    [Header("Dash")]
    [SerializeField] private float dashStrength;
    private float dashInput;
    private bool canDash;
    public float SetDashCooldown;
    private float DashCooldown;
    private bool hasDashed;
    private bool isDashing;
    private float dashDuration;
    private float DashDelay;
    [SerializeField] private float SetdashDuration;
    [SerializeField] private float SetDashDelay;
    #endregion

    #region Sterbung
    [Header("Sterbung")]
    private float SetDieDuration = 2;
    private float DieDuration;
    private int CheckpointNumber;
    public bool Dying;
    public bool Died;
    #endregion

    #region Referenzen
    [Header("References")]
    [SerializeField] private Rigidbody2D rigidBody2D;
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Transform wallCheckA;
    [SerializeField] private Transform wallStickA;
    [SerializeField] private Transform wallCheckB;
    [SerializeField] private Transform wallStickB;
    [SerializeField] private Transform LastCheckpointPosition;

    private InputAction dashAction;
    private InputAction horizontalAction;
    private InputAction jumpAction;
    #endregion

    private void Start()
    {
        dashAction = InputSystem.actions.FindAction("Dash");
        horizontalAction = InputSystem.actions.FindAction("Horizontal");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    private void Update() {
        
        DieDuration -= Time.deltaTime;
        animator.SetFloat("NMS", timeSinceLastMove);
        animator.SetFloat("Y_Movement", rigidBody2D.linearVelocity.y);

        animator.SetBool("IsWallSticking", IsWallStick);
        animator.SetBool("IsDashing", isDashing);
        animator.SetBool("Dying",Dying);

        dashInput = dashAction.ReadValue<float>();
        horizontalInput = horizontalAction.ReadValue<float>();
        jumpInput = jumpAction.ReadValue<float>();

    }

    private void FixedUpdate()
    {
        
        
        if(Dying){
            HandleDeath();
            return;
        } 

        HandleWallJump();
        HandleJump();
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
            JumpBufferDuration = SetJumpBufferDuration;
            hasJumped = true;
        }
        else
        {
            JumpBufferDuration -= Time.deltaTime;
        }

        if (jumpInput <= 0.1f)
        {
            longerJumpCounter = 0;
            hasJumped = false;
        }

        if (!isDashing)
        {
            if (coyoteTimeCounter > 0 && JumpBufferDuration > 0)
            {
                rigidBody2D.linearVelocity = new Vector2(rigidBody2D.linearVelocity.x, jumpStrength);
                coyoteTimeCounter = 0;
                JumpBufferDuration = 0;
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
    
    private void HandleWallJump() {

        WallJumpRecoverTime -= Time.deltaTime;

        Wallfound = Physics2D.OverlapArea(wallCheckA.position,wallCheckB.position,wallLayer);
        IsWallStick = Physics2D.OverlapArea(wallStickA.position,wallStickB.position,wallLayer);

        if(!IsWallStick && !Wallfound && WallJumpRecoverTime < 0) {
            WallDirection = 0;
        }
        



        if(Wallfound && !isDashing && !isGrounded) {
            WallSlidingSpeed = SetWallSlidingSpeed;
            canDash = true;
            WallJumpRecoverTime = 0;
            directionFacing = directionFacing * -1;
            WallDirection = directionFacing;
            transform.localScale = new Vector3(1f * WallDirection, 1f, 1);
            IsWallStick = true;
        }

        if(IsWallStick && !isGrounded) {
            if(WallSlidingSpeed < MaxWallSlidingSpeed)  WallSlidingSpeed += Time.deltaTime * 8;

            rigidBody2D.linearVelocity = new Vector2(WallDirection * 0.2f * -1, -WallSlidingSpeed);

            if(WallJumpRecoverTime < 0 && JumpBufferDuration > 0 ) {
                JumpBufferDuration = 0;
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
        DashDelay -= Time.deltaTime;
        dashDuration -= Time.deltaTime;
        DashCooldown -= Time.deltaTime;

        if (DashCooldown < 0)
            canDash = true;

        if (canDash && dashInput > 0 && !hasDashed && DashDelay < 0)
        {
            isDashing = true;
            canDash = false;
            hasDashed = true;
            DashCooldown = SetDashCooldown;
            DashDelay = SetDashDelay;
            dashDuration = SetdashDuration;
            longerJumpCounter = 0;

            animator.SetBool("IsDashing", true);
            SetGravityScale(0);
            rigidBody2D.linearVelocity = new Vector2(dashStrength * directionFacing, 0);
        }

        if (dashDuration <= 0 && isDashing)
        {
            isDashing = false;
            SetGravityScale(4);
            rigidBody2D.linearVelocity = new Vector2(3 * directionFacing, 0);
            animator.SetBool("IsDashing", false);
        }

        if (dashInput <= 0)
            hasDashed = false;
    }

    private void HandleDeath(){
       
        if(Died){
            DieDuration = SetDieDuration;
            Died = false;
            SetGravityScale(0);
        }
        
        if(DieDuration <= 1.03 && DieDuration >= 0.97) transform.position = LastCheckpointPosition.position;

        if(DieDuration <= 0) Dying = false;
        rigidBody2D.linearVelocity = new Vector2(0,0);
        if(!Dying) SetGravityScale(4);
    }

    public void GetCheckpoint(int newCheckpointNumber,Vector3 newCheckpointPosition){
        if(newCheckpointNumber > CheckpointNumber){
            LastCheckpointPosition.position = newCheckpointPosition;
            CheckpointNumber = newCheckpointNumber;
        }
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

    void OnTriggerEnter2D(Collider2D other)
    {
         if(other.gameObject.tag == "Laser"){
            Dying = true;
            Died = true;

        }
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
    public void SetJumpInput(float value) {
        jumpInput = value;
    }
    public void SetDashInput(float value) {
        dashInput = value;
    }
    public void SetHorizontalInput(float value) {
        horizontalInput = value;
    }

}

