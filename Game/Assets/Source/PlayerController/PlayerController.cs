using System;
using System.Collections.Generic;
using System.Linq;
using Source.MenuSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Source.PlayerController
{
    // TODO: make sure floor trigger gets out of ground at the first frame of the jump
    // otherwise it might get fucky 
    // as the state machine will end up in an infinite cycle

    // TODO: rewrite the HELL out of this script
    
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        // TODO: reinit these values if they're switched in editor
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
        
        public Vector3 currentVelocity;
        
        private JumpHandler _jumpHandler;
        private DashHandler _dashHandler;
        private RunHandler _runHandler;

        public Vector2 TargetMoveDir { get; private set; } = Vector2.zero;
        public bool IsJumping { get; private set; } = false;
        
        private void Start()
        {
            playerRb = GetComponent<Rigidbody2D>();
            groundController = GetComponentInChildren<GroundController>();

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
            _dashHandler.FixedUpdate();
            _jumpHandler.FixedUpdate();
            _runHandler.FixedUpdate();
            
            playerRb.MovePosition(transform.position + currentVelocity * Time.fixedDeltaTime);
            
            print(TargetMoveDir + " " + IsJumping);
        }
    }
}