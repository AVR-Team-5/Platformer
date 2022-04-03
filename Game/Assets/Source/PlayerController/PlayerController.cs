using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Source.PlayerController
{
    enum AnimPriorityFlags {
        // inferred from the context
        // idle, jumping, walljumping, falling
        // running, wallslide
        None = 0,

        // minimal player input
        Low = 1 << 0,

        // kinda important player actions
        // start jump, start wall jump
        Medium = 1 << 1,

        // important player actions
        // attacks (low, high, overhead)
        High = 1 << 2,

        // Critical actions
        // hitstun
        Critical = 1 << 3,
    }


    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        public float jumpHeight;
        public float timeTillJumpPeak;
        public float timeTillFallPeak;

        public float startVerticalSpeedUp;
        public float jumpGravity;
        public float fallGravity;

        [Space(10)]
        public float maxRunningSpeed;
        public float runAcceleration;
        public float brakeAcceleration;
        public float stopAcceleration;
        public float valueCloseToZero;

        [Space(10)]
        public float dashDuration;
        public float dashSpeed;
        public float afterDashMomentum;

        public GroundController groundController;
        public Rigidbody2D playerRb;
        public PlayerInput playerInput;
        public Animator animator;

        public Vector2 currentVelocity;
        private readonly List<RaycastHit2D> _raycastHits = new List<RaycastHit2D>();
        private int _frameCounter = 0;

        private JumpHandler _jumpHandler;
        private DashHandler _dashHandler;
        private RunHandler _runHandler;

        public Vector2 TargetMoveDir { get; private set; } = Vector2.zero;
        bool IsRunning
        {
            get { return TargetMoveDir.magnitude > valueCloseToZero; }
        }
        public bool IsJumping { get; private set; }
        public bool IsMeleeAttacking = false;
        public bool IsRangedAttacking = false;
        private bool _isMovingLeft = false;
        private int _animPriorityFlag = 0;



        private void Start()
        {
            playerRb = GetComponent<Rigidbody2D>();
            groundController = GetComponentInChildren<GroundController>();
            playerInput = GetComponent<PlayerInput>();
            animator = GetComponent<Animator>();

            _jumpHandler = new JumpHandler(playerController: this);
            _dashHandler = new DashHandler(playerController: this);
            _runHandler = new RunHandler(playerController: this);


            InitPhysicsValues();
        }

        public void OnMovement(InputValue value)
        {
            TargetMoveDir = value.Get<Vector2>();
        }

        public void OnJump()
        {
            _jumpHandler.Start();
        }

        public void OnJumpContinuous(InputValue value)
        {
            IsJumping = value.isPressed;
        }

        public void OnDash()
        {
            _dashHandler.Start(TargetMoveDir);
        }

        public void OnMeleeAttack(InputValue value)
        {
            IsMeleeAttacking = value.isPressed;

            if (IsMeleeAttacking)
                _animPriorityFlag |= (int)AnimPriorityFlags.High;
            print(IsMeleeAttacking);
        }

        public void OnRangedAttack(InputValue value)
        {
            IsRangedAttacking = value.isPressed;

            if (IsRangedAttacking)
                _animPriorityFlag |= (int)AnimPriorityFlags.High;
            print(IsRangedAttacking);
        }

        private void OnValidate()
        {
            InitPhysicsValues();
        }

        private void InitPhysicsValues()
        {
            jumpGravity = -(2 * jumpHeight) / (timeTillJumpPeak * timeTillJumpPeak);
            fallGravity = -(2 * jumpHeight) / (timeTillFallPeak * timeTillFallPeak);
            startVerticalSpeedUp = jumpHeight / timeTillJumpPeak - jumpGravity * timeTillJumpPeak / 2;
        }

        private void FixedUpdate()
        {
            _frameCounter++;
            var isDashing = _dashHandler.FixedUpdate();

            if (!isDashing)
            {
                _jumpHandler.FixedUpdate();
                _runHandler.FixedUpdate();
            }

            // TODO: cast rigidbody onto new position before moving

            // if (currentVelocity.magnitude > valueCloseToZero)
            // {
            //     var velocityDelta = currentVelocity * Time.fixedDeltaTime;
            //     var distance = velocityDelta.magnitude;
            //
            //     playerRb.Cast(currentVelocity, _raycastHits, distance);
            //
            //     foreach (var raycastHit in _raycastHits)
            //     {
            //         var currentNormal = raycastHit.normal;
            //         // rotate the normal by 90 degrees to get the slope vector
            //         // var slopeVector = new Vector2(currentNormal.y, -currentNormal.x);
            //
            //         // TODO: actually implement the slide on steep angles
            //         // if (currentNormal.y > maxNormalYToSlide)
            //         // {
            //         //     //grounded!
            //         // }
            //
            //         var projection = Vector2.Dot(currentVelocity, currentNormal);
            //         
            //         // print(_frameCounter + ": Dot product of " + currentVelocity + " and " + currentNormal + " is " + projection );
            //         print(_frameCounter + ": collision with " + raycastHit.collider.name);
            //
            //         if (projection < 0)
            //             currentVelocity -= projection * currentNormal;
            //     }
            //     
            //     _raycastHits.Clear();
            // }

            if (currentVelocity.magnitude > valueCloseToZero)
                playerRb.MovePosition(transform.position + (Vector3)currentVelocity * Time.fixedDeltaTime);
        }

        private void Update()
        {
            if (TargetMoveDir.x > valueCloseToZero)
            {
                transform.localScale = Vector3.one;
                print("mirrored right");
            }
            else if (TargetMoveDir.x < -valueCloseToZero)
            {
                transform.localScale = new Vector3(-1, 1, 1);
                print("mirrored left");
            }

            animator.SetFloat("XVelocity", Mathf.Abs(currentVelocity.x));
            animator.SetFloat("YVelocity", currentVelocity.y);
            animator.SetBool("IsGrounded", groundController.isGrounded);
            animator.SetBool("IsRunning", IsRunning);
            animator.SetBool("IsMeleeAttacking", IsMeleeAttacking);
            animator.SetBool("IsRangedAttacking", IsRangedAttacking);
            animator.SetInteger("AnimPriorityFlag", _animPriorityFlag);
        }

        void OnDrawGizmos()
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, 0.1f);
        }

        public void AnimationEndHandler() {
            ResetAnimPriority();
        }

        public void ResetAnimPriority() {
            _animPriorityFlag = 0;
        }

        
    }
}