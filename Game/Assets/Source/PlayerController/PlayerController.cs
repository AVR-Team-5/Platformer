using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Source.PlayerController
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(JumpHandler))]
    [RequireComponent(typeof(DashHandler))]
    [RequireComponent(typeof(RunHandler))]
    public class PlayerController : MonoBehaviour
    {
        [Serializable]
        public struct TransitionStruct
        {
            public bool canHitStun;
            public bool canMeleeAtkHigh;
            public bool canMeleeAtkLow;
            public bool canMeleeAtkOverhead;
            public bool canStartJump;
            public bool canStartWallJump;
        }

        public TransitionStruct transitions;
        
        public float valueCloseToZero;

        public GroundController groundController;
        public GroundController leftWallController;
        public GroundController rightWallController;
        
        public Rigidbody2D playerRb;

        public Vector2 currentVelocity;

        private JumpHandler _jumpHandler;
        private DashHandler _dashHandler;
        private RunHandler _runHandler;
        
        private void Start()
        {
            playerRb = GetComponent<Rigidbody2D>();
            // groundController = GetComponentInChildren<GroundController>();

            _jumpHandler = GetComponent<JumpHandler>();
            _dashHandler = GetComponent<DashHandler>();
            _runHandler = GetComponent<RunHandler>();
        }


        private void FixedUpdate()
        {
            var isDashing = _dashHandler.CycleDash();

            if (!isDashing)
            {
                _jumpHandler.CycleJump();
                _runHandler.FixedUpdate();
            }
            
            if (currentVelocity.magnitude > valueCloseToZero)
                playerRb.MovePosition(transform.position + (Vector3)currentVelocity * Time.fixedDeltaTime);
        }
        
        public void AnimationEndHandler() {}
    }
}