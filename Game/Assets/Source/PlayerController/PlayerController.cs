using System;
using System.Collections.Generic;
using System.Linq;
using Source.MenuSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Source.PlayerController
{
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
        public bool IsJumping { get; private set; }
        private bool _isMovingLeft = false;


        
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

        private void Update() {
            if (currentVelocity.x > valueCloseToZero) {
                transform.localScale = Vector3.one;
                print("mirrored right");
            }
            else if (currentVelocity.x < -valueCloseToZero) {
                transform.localScale = new Vector3(-1, 1, 1);
                print("mirrored left");
            }

            animator.SetFloat("XVelocity", Mathf.Abs(currentVelocity.x));
            animator.SetFloat("YVelocity", currentVelocity.y);
            animator.SetBool("IsGrounded", groundController.isGrounded);
            // animator.SetBool("IsMovingLeft", _isMovingLeft);
        }
    }
}