using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Source.PlayerController
{
    // TODO: make sure floor trigger gets out of ground at the first frame of the jump
    // otherwise it might get fucky 
    // as the state machine will end up in an infinite cycle

    internal enum JumpState
    {
        Grounded,
        Jumping,
        Falling
    }

    public class JumpHandler : MonoBehaviour
    {
        private PlayerController _player;
        private GroundController _ground;
        private GroundController _left;
        private GroundController _right;

        [SerializeField] private float valueCloseToZero;
        [SerializeField] private float jumpHeight;
        [SerializeField] private float timeTillJumpPeak;
        [SerializeField] private float timeTillFallPeak;
        [SerializeField] private float timeTillSlidePeak;
        [SerializeField] private float minimumDirJumpXVelocity;

        [Space(10)] 
        [SerializeField] private int coyoteTimeFrames;


        private float _startVerticalSpeedUp;
        private float _jumpGravity;
        private float _fallGravity;
        private float _slideGravity;

        [SerializeField] private int _currentCoyoteTimeFrames;

        private JumpState _state = JumpState.Falling;
        private bool _isJumping;

        public bool _isWallSliding = false;
        public int _wallSlideDir = 0;
        // -1 for left, 1 for right
        
        public float TargetMoveDirX { get; private set; }
        
        public void OnMovement(InputValue value)
        {
            TargetMoveDirX = value.Get<Vector2>().x;
            
            
        }

        public void Start()
        {
            _player = GetComponent<PlayerController>();
            _ground = _player.groundController;
            _left = _player.leftWallController;
            _right = _player.rightWallController;

            InitPhysicsValues();
        }

        private void OnValidate()
        {
            InitPhysicsValues();
        }

        private void InitPhysicsValues()
        {
            _jumpGravity = -(2 * jumpHeight) / (timeTillJumpPeak * timeTillJumpPeak);
            _fallGravity = -(2 * jumpHeight) / (timeTillFallPeak * timeTillFallPeak);
            _slideGravity = -(2 * jumpHeight) / (timeTillSlidePeak * timeTillSlidePeak);
            _startVerticalSpeedUp = jumpHeight / timeTillJumpPeak - _jumpGravity * timeTillJumpPeak / 2;
        }

        public void StartJump()
        {
            if (IsGrounded())
            {
                // init velocity
                _player.currentVelocity.y = _startVerticalSpeedUp;

                // add directional velocity in case of movement
                if (Mathf.Abs(TargetMoveDirX) > valueCloseToZero)
                {
                    if (Mathf.Abs(_player.currentVelocity.x) < minimumDirJumpXVelocity)
                        _player.currentVelocity.x = minimumDirJumpXVelocity * TargetMoveDirX;
                }

                _currentCoyoteTimeFrames = 0;

                // continue calculations
                _state = JumpState.Jumping;
            }
            else if (_isWallSliding)
            {
                // init velocity
                _player.currentVelocity.y = _startVerticalSpeedUp;

                // add directional velocity
                _player.currentVelocity.x = -_wallSlideDir * minimumDirJumpXVelocity;

                _currentCoyoteTimeFrames = 0;

                // continue calculations
                _state = JumpState.Jumping;
                _isWallSliding = false;
            }
            
            
        }

        public void OnJump()
        {
            StartJump();
        }

        public void OnJumpContinuous(InputValue value)
        {
            _isJumping = value.isPressed;
        }

        public bool IsGrounded()
        {
            return _ground.IsGrounded || _currentCoyoteTimeFrames > 0;
        }

        // public bool IsWallSliding()
        // {
        //     return StartWallSlideLeft() || StartWallSlideRight();
        // }

        public bool StartWallSlideLeft()
        {
            return _left.IsGrounded && TargetMoveDirX < -valueCloseToZero;
        }
        
        public bool StartWallSlideRight()
        {
            return _right.IsGrounded && TargetMoveDirX > valueCloseToZero;
        }


        public bool StopWallSlide()
        {
            // adding a bit of delta to the input check for stick micro movement
            return _isWallSliding && (_wallSlideDir * TargetMoveDirX < -valueCloseToZero
                                      || IsGrounded()
                                      || (!_left.IsGrounded && !_right.IsGrounded));
        }


        // run each fixed update
        public void CycleJump()
        {
            if (!_isWallSliding && !IsGrounded() && (StartWallSlideRight() || StartWallSlideLeft()))
            {
                // start wall slide
                _isWallSliding = true;
                _wallSlideDir = StartWallSlideRight() ? 1 : -1;

                if (_player.currentVelocity.y < 0)
                    _player.currentVelocity.y = 0;
                // negative velocity reset
                print("started wall slide");
            }
            else if (StopWallSlide())
            {
                _isWallSliding = false;
            }

            switch (_state)
            {
                case JumpState.Grounded:
                {
                    // no gravity calc
                    // exit condition
                    if (!(_ground.IsGrounded || _left.IsGrounded || _right.IsGrounded))
                        if (_player.currentVelocity.y > valueCloseToZero)
                        {
                            _state = JumpState.Jumping;
                            goto case JumpState.Jumping;
                        }
                        // coyote time
                        else if (_currentCoyoteTimeFrames-- <= 0)
                        {
                            _state = JumpState.Falling;
                            goto case JumpState.Falling;
                        }

                    _player.currentVelocity.y = 0f;

                    break;
                }

                case JumpState.Jumping:
                {
                    // start falling if y velocity < 0 or stopped pressing space
                    if (!_isJumping || _player.currentVelocity.y < 0f)
                    {
                        _state = JumpState.Falling;
                        goto case JumpState.Falling;
                    }

                    // add jump acceleration to current jump velocity
                    _player.currentVelocity.y += _jumpGravity * Time.fixedDeltaTime;
                    goto default;
                }

                case JumpState.Falling:
                {
                    // add fall acceleration to current jump velocity
                    _player.currentVelocity.y += _isWallSliding
                        ? _slideGravity * Time.fixedDeltaTime
                        : _fallGravity * Time.fixedDeltaTime;

                    goto default;
                }

                default:
                {
                    // make the player grounded
                    if (IsGrounded() && _player.currentVelocity.y <= 0f)
                    {
                        _state = JumpState.Grounded;
                        _currentCoyoteTimeFrames = coyoteTimeFrames;
                    }

                    break;
                }
            }
            
        }
    }
}