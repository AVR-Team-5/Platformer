using UnityEngine;

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

    public class JumpHandler
    {
        private readonly PlayerController _playerController;
        private readonly Rigidbody2D _playerRb;
        private readonly GroundController _groundController;
        
        private JumpState _state = JumpState.Falling;
        
        public JumpHandler(PlayerController playerController)
        {
            _playerController = playerController;
            
            _groundController = playerController.groundController;
            _playerRb = playerController.playerRb;
        }

        public void Start()
        {
            // init velocity
            _playerController.currentVelocity.y = _playerController.startVerticalSpeedUp;

            // continue calculations
            _state = JumpState.Jumping;
        }

        // this one should go in FixedUpdate
        public void FixedUpdate()
        {
            switch (_state)
            {
                case JumpState.Grounded:
                {
                    // no gravity calc
                    // exit condition
                    
                    if (!_groundController.IsGrounded)
                        if (_playerController.currentVelocity.y > _playerController.valueCloseToZero)
                        {
                            _state = JumpState.Jumping;
                            goto case JumpState.Jumping;
                        }
                        else
                        {
                            _state = JumpState.Falling;
                            goto case JumpState.Falling;
                        }
                    
                    _playerController.currentVelocity.y = 0f;
                    
                    break;
                }

                case JumpState.Jumping:
                {
                    // add jump acceleration to current jump velocity
                    // TODO: implement hold to jump higher by keeping a jump input buffer
                    // if (!pressedJump || currentVelocity.y < 0f)
                    
                    if (!_playerController.IsJumping || _playerController.currentVelocity.y < 0f)
                    {
                        _state = JumpState.Falling;
                        goto case JumpState.Falling;
                    }
                    
                    _playerController.currentVelocity.y += _playerController.jumpGravity * Time.fixedDeltaTime;
                    goto default;
                }

                case JumpState.Falling:
                {
                    // add fall acceleration to current jump velocity
                    _playerController.currentVelocity.y += _playerController.fallGravity * Time.fixedDeltaTime;
                    
                    goto default;
                }

                default:
                {
                    // make the player grounded
                    if (_groundController.IsGrounded)
                        _state = JumpState.Grounded;
                    
                    break;
                }
            }
        }
    }
}