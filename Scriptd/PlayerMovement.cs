using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{


    [Header("Horizontal")]
    [SerializeField] private  float MoveSpeed;
    float horizontalInput;
    [SerializeField] private  float  DirectionFacing = 1;

    
    [Header("Sprung")]
    [SerializeField] private float SetKyoteTime;
    private bool Grounded;
    [SerializeField] private float SetJumpBuffering;
    [SerializeField] private  float JumpStrength;
    [SerializeField] private  float SetLongerJumpTime; 
    private float kyoteTime;
    private float jumpBuffering;
    private bool didJump = false;
    private float longerJumpTime;
    float jumpInput;

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


    [Header("Attack")]
    [SerializeField] private  float SetAttackBuffer;
    private float AttackInput;
    private float AttackBuffer;
    private bool didAttack;
    private bool isAttacking;
    private float AttackDelay;
    private float SetAttackDelay = 0.5f;


    [Header("Dash")]
    [SerializeField] private  float DashStrength;
    private float DashInput;
    private bool CanDash;
    public float SetCanDashTimer;
    private float CanDashTimer;
    private bool didDash;
    private bool isDashing;
    private float DashTime;
    private float DashDelay;
    [SerializeField] private float SetDashTime;
    [SerializeField] private float SetDashDelay;
   
    

    [Header("Referenzen")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask WallLayer;
    [SerializeField] private Transform WallSeekA;
    [SerializeField] private Transform WallStickA;
    [SerializeField] private Transform WallSeekB;
    [SerializeField] private Transform WallStickB;

    private InputAction DashAction;
    private InputAction horizontalAction;
    private InputAction jumpAction;
    private InputAction AttackAction;


    //
    private void Start()
    {
        DashAction = InputSystem.actions.FindAction("Dash");
        horizontalAction = InputSystem.actions.FindAction("Horizontal");
        jumpAction = InputSystem.actions.FindAction("Jump");
        AttackAction = InputSystem.actions.FindAction("Attack");



    }

    private void Update() {


        animator.SetFloat("X_Movement", rb.linearVelocityX);
        animator.SetFloat("Y_Movement", rb.linearVelocityY);


        if(IsWallStick) {
            animator.SetBool("IsWalling", true);
        }
        else {
            animator.SetBool("IsWalling", false);
        }

        if(isDashing) {
            animator.SetBool("Is_Dashing", true);
        }
        else {
            animator.SetBool("Is_Dashing", false);
        }
        
    }

    private void FixedUpdate()
    {
        DashInput = DashAction.ReadValue<float>();
        AttackInput = AttackAction.ReadValue<float>();
        horizontalInput = horizontalAction.ReadValue<float>();
        jumpInput = jumpAction.ReadValue<float>();



        Jump();
        WallJump();
        Horizontal();
        // Attack();
        Dash();


    }

    private void Horizontal()
    {

        if(horizontalInput != 0 && !isDashing) {
        

            if(!isAttacking && WallJumpRecoverTime < 0 && !IsWallStick) {
                transform.localScale = new Vector3((float)(DirectionFacing ), 1, 1);
            }

            if(DirectionFacing != horizontalInput && horizontalInput != 0 ) {

                    DirectionFacing = horizontalInput;
            }

            
            if(WallJumpRecoverTime < 0 && WallDirection*-1 != horizontalInput) {
                
                rb.linearVelocity = new Vector2(MoveSpeed * DirectionFacing, rb.linearVelocity.y);

            }

        }
    }

    private void Jump(){
        
        //KyoteTime
        if(Grounded){
            kyoteTime = SetKyoteTime;
        }
        else{
            kyoteTime -= Time.deltaTime;
        }

        // Jump Buffering
        if(jumpInput > 0 && !didJump){
            jumpBuffering = SetJumpBuffering;
            didJump = true;
        }
        else{
            jumpBuffering -= Time.deltaTime;
        }

        if (jumpInput <= 0.1 ){
            longerJumpTime = 0;
            didJump = false;
        }

        //Jump
        if(!isDashing) {

            if(kyoteTime > 0 && jumpBuffering > 0) {

                rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpStrength);
                kyoteTime = 0;
                jumpBuffering = 0;
                longerJumpTime = SetLongerJumpTime;
                Grounded = false;

            }
            else if(jumpInput > 0 && longerJumpTime > 0) {

                if(rb.linearVelocity.y < 0.1) {

                    rb.linearVelocityY = 0;
                    longerJumpTime = 0;
                }

                rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpStrength);
                longerJumpTime -= Time.deltaTime;

            }
            else if(rb.linearVelocity.y <= 0) {


                rb.gravityScale = 4;
                longerJumpTime = 0;
            }
        }
        
            
    }

    private void WallJump() {

        WallJumpRecoverTime -= Time.deltaTime;

        Wallfound = Physics2D.OverlapArea(WallSeekA.position,WallSeekB.position,WallLayer);
        IsWallStick = Physics2D.OverlapArea(WallStickA.position,WallStickB.position, WallLayer);
        if(!IsWallStick && !Wallfound && WallJumpRecoverTime < 0) {
            WallDirection = 0;
        
        }





        if(Wallfound && !isDashing && !isAttacking && !Grounded) {
            WallSlidingSpeed = SetWallSlidingSpeed;
            WallJumpRecoverTime = 0;
            DirectionFacing = DirectionFacing * -1;
            WallDirection = DirectionFacing;
            transform.localScale = new Vector3(1 * WallDirection, 1, 1);
            IsWallStick = true;
        }

        if(IsWallStick && !Grounded) {
            if(WallSlidingSpeed < MaxWallSlidingSpeed)  WallSlidingSpeed += Time.deltaTime * 8;

            rb.linearVelocity = new Vector2(WallDirection * 0.2f * -1, -WallSlidingSpeed);

            if(WallJumpRecoverTime < 0 && jumpBuffering > 0 ) {
                jumpBuffering = 0;
                WallJumpRecoverTime = SetWallJumpRecoverTime;
                VerticalWJumpStrength = SetVerticalWJumpStrength;
                rb.linearVelocity = new Vector2(HorizontalWJumpStrength * WallDirection,VerticalWJumpStrength);
                
            }

        }

        if(WallJumpRecoverTime > 0) {
            VerticalWJumpStrength -= 0.3f;
            rb.linearVelocity = new Vector2(HorizontalWJumpStrength * WallDirection, VerticalWJumpStrength);
        }
       






    }

    private void Dash() {

        DashDelay -= Time.deltaTime;
        DashTime -= Time.deltaTime;
        CanDashTimer -= Time.deltaTime;

        if(CanDashTimer < 0) {
            CanDash = true;
        
        }

        //Dash Start
        if(CanDash && DashInput > 0 && !didDash && DashDelay < 0) {

            isDashing = true;
            CanDashTimer = SetCanDashTimer;
            DashDelay = SetDashDelay;
            DashTime = SetDashTime;
            CanDash = false;
            animator.SetBool("Is_Dashing",true);


            longerJumpTime = 0;
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(DashStrength * DirectionFacing, 0);

        }

        //Dash Ende
        if(DashTime <= 0 && isDashing) {
            isDashing = false;
            rb.gravityScale = 4;
            rb.linearVelocity = new Vector2(3 * DirectionFacing, 0);
            animator.SetBool("Is_Dashing",false);
        
        }

        //NonConstantDash
        if(DashInput <= 0) {
            didDash = false;
        }
        if(DashInput > 0) {

            didDash = true;
        }






    }

    private void Attack() {

        AttackDelay -= Time.deltaTime;

        if(AttackDelay <= 0 && isAttacking) {
        
            isAttacking = false;
            MoveSpeed = 5;
        }

        //NonConstantAttack
        if(AttackInput > 0 && !didAttack) {
            AttackBuffer = SetAttackBuffer;
            didAttack = true;
        }
        else {

            AttackBuffer -= Time.deltaTime;
        }

        if(AttackInput <= 0) {
            didAttack = false;
        }

        //Attack
        if(AttackBuffer > 0 && !isDashing ) {
            AttackBuffer = 0;
            isAttacking = true;
            AttackDelay = SetAttackDelay;
            MoveSpeed = MoveSpeed / 2;

        }





        
        


    
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground")){
            Grounded = true;
            CanDash = true;

            animator.SetBool("Grounded",true);
        }

    }

    private void OnCollisionStay2D(Collision2D collision) {

        if(collision.gameObject.CompareTag("Ground")) {
            Grounded = true;
            CanDash = true;
            animator.SetBool("Grounded",true);
        }

    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ground")){
            Grounded = false;
            animator.SetBool("Grounded", false); ;
        }

    }

    
}
