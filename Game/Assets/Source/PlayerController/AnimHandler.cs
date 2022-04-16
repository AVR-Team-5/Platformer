using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Source.PlayerController
{
    public class AnimHandler : MonoBehaviour
    {
        public float valueCloseToZero = 0.01f;
        public Transform tfToRotate;
        
        private PlayerController _playerController;
        private GroundController _groundController;
        private JumpHandler _jumpHandler;
        private Animator _animator;

        public bool IsLookingRight { get; private set; } = true;
        private int _currentLowPriorityAnim;
        
        public float TargetMoveDirX { get; private set; }
        
        public void OnMovementX(InputValue value)
        {
            TargetMoveDirX = value.Get<float>();

            ResetRotation();
        }

        private void Start()
        {
            _playerController = GetComponent<PlayerController>();
            _groundController = _playerController.groundController;
            _jumpHandler = GetComponent<JumpHandler>();
            _animator = GetComponent<Animator>();
        }

        public void OnMeleeAttack()
        {
            _animator.Play(AnimClips.MeleeAtkOverhead);
        }

        public void RotateCharacter(bool right)
        {
            if (IsLookingRight == right) return;
            
            tfToRotate.Rotate(0f, 180f, 0f);
            IsLookingRight = !IsLookingRight;
        }

        public void ResetRotation()
        {
            if (Mathf.Abs(TargetMoveDirX) > valueCloseToZero)
                if (TargetMoveDirX > 0f != IsLookingRight && !_jumpHandler.IsWallSliding)
                    RotateCharacter(!IsLookingRight);
        }

        private void Update()
        {
            // low priority stuff
            // don't mind the absolute ton of ifs in a performance critical context
            // this is quite literally a decision tree written as code
            if (_groundController.IsGrounded)
            {
                if (Mathf.Abs(TargetMoveDirX) > valueCloseToZero) 
                    StartLowPriorityAnim(AnimClips.Run);
                else
                    StartLowPriorityAnim(AnimClips.Idle);
            }
            else
            {
                if (_jumpHandler.IsWallSliding)
                {
                    StartLowPriorityAnim(AnimClips.WallSlide);
                }
                else
                {
                    if (_playerController.currentVelocity.y > valueCloseToZero)
                        StartLowPriorityAnim(AnimClips.Jumping);
                    else
                        StartLowPriorityAnim(AnimClips.Falling);
                }
            }
        }

        /// <param name="anim">Anim clip hash from the static AnimClips class</param>
        public void StartLowPriorityAnim(int anim)
        {
            if (_currentLowPriorityAnim == anim) return;
            _animator.Play(anim);
            _currentLowPriorityAnim = anim;
        }
        
        /// <summary>
        /// Does exactly what the function name says.
        /// Do check in advance if the animation can really execute, using PlayerController's TransitionStruct
        /// </summary>
        /// <param name="anim">Anim clip hash from the static AnimClips class</param>
        public void StartHighPriorityAnim(int anim)
        {
            _animator.Play(anim);
        }
    }
    
    public static class AnimClips
    {
        public static readonly int Idle = Animator.StringToHash("Player_Idle");
        public static readonly int Run = Animator.StringToHash("Player_Run");
        public static readonly int HitStun = Animator.StringToHash("Player_HitStun");
        public static readonly int MeleeAtkHigh = Animator.StringToHash("Player_MeleeAtkHigh");
        public static readonly int MeleeAtkLow = Animator.StringToHash("Player_MeleeAtkLow");
        public static readonly int MeleeAtkOverhead = Animator.StringToHash("Player_MeleeAtkOverhead");
        public static readonly int StartJump = Animator.StringToHash("Player_StartJump");
        public static readonly int StartWallJump = Animator.StringToHash("Player_StartWallJump");
        public static readonly int Jumping = Animator.StringToHash("Player_Jumping");
        public static readonly int WallJumping = Animator.StringToHash("Player_WallJumping");
        public static readonly int WallSlide = Animator.StringToHash("Player_WallSlideV2");
        public static readonly int Falling = Animator.StringToHash("Player_Falling");
    }

}