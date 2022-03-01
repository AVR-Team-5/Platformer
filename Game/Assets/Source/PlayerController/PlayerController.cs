using System;
using System.Collections.Generic;
using System.Linq;
using Source.MenuSystem;
using UnityEngine;

namespace Source.PlayerController
{
    // TODO: make sure floor trigger gets out of ground at the first frame of the jump
    // otherwise it might get fucky 
    // as the state machine will end up in an infinite cycle

    // TODO: rewrite the HELL out of this script
    
    public struct KbInputEvent
    {
        private KeyCode _keyCode;
        private EventType _eventType;

        public KbInputEvent(KeyCode keyCode, EventType eventType)
        {
            _keyCode = keyCode;
            _eventType = eventType;
        }
    }

    public class KbInputBuffer
    {
        public bool IsUpHeld;
        public bool IsDownHeld;
        public bool IsLeftHeld;
        public bool IsRightHeld;
        public bool IsJumpHeld;
        
        private readonly EventType[] _acceptedTypes = {EventType.KeyDown, EventType.KeyUp};
        
        public void RegisterInput(KeyCode key, EventType type)
        {
            if (!_acceptedTypes.Contains(type))
            {
                Debug.Log("WARNING: not accepted input type was sent to register: " + type);
                return;
            }

            var result = type == EventType.KeyDown;
            
            switch (key)
            {
                case KeyCode.W:
                    IsUpHeld = result;
                    break;
                case KeyCode.S:
                    IsDownHeld = result;
                    break;
                case KeyCode.A:
                    IsLeftHeld = result;
                    break;
                case KeyCode.D:
                    IsRightHeld = result;
                    break;
                case KeyCode.Space:
                    IsJumpHeld = result;
                    break;
                
                default:
                    Debug.Log("WARNING: not accepted input key was sent to register: " + key);
                    break;
            }
        }

        public Vector2 DesiredMoveDir()
        {
            var result = Vector2.zero;
            
            if (IsUpHeld)
                result += Vector2.up;
            if (IsDownHeld)
                result += Vector2.down;
            if (IsLeftHeld)
                result += Vector2.left;
            if (IsRightHeld)
                result += Vector2.right;
            
            return result;
        }
    }

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

        private readonly Dictionary<KbInputEvent, Action> _inputMappings = new Dictionary<KbInputEvent, Action>();
        public readonly KbInputBuffer KbInputBuffer = new KbInputBuffer();
        
        public Vector3 currentVelocity;
        
        private JumpHandler _jumpHandler;
        private DashHandler _dashHandler;
        private RunHandler _runHandler;

        public Vector2 DesiredMovementDirection
        {
            get;
            private set;
        }

        private void InitInputMapping()
        {
            _inputMappings.Add(new KbInputEvent(KeyCode.Space, EventType.KeyDown), () =>
            {
                _jumpHandler.Start();
                KbInputBuffer.RegisterInput(KeyCode.Space, EventType.KeyDown);
            });
            _inputMappings.Add(new KbInputEvent(KeyCode.Space, EventType.KeyUp), () => KbInputBuffer.RegisterInput(KeyCode.Space, EventType.KeyUp));
            
            _inputMappings.Add(new KbInputEvent(KeyCode.LeftShift, EventType.KeyDown), () => _dashHandler.Start(KbInputBuffer.DesiredMoveDir()));

            // this seems kinda shitty and error prone
            // maybe somehow replace each KeyDown/KeyUp pair with a single statement
            _inputMappings.Add(new KbInputEvent(KeyCode.W, EventType.KeyDown), () => KbInputBuffer.RegisterInput(KeyCode.W, EventType.KeyDown));
            _inputMappings.Add(new KbInputEvent(KeyCode.W, EventType.KeyUp), () => KbInputBuffer.RegisterInput(KeyCode.W, EventType.KeyUp));

            _inputMappings.Add(new KbInputEvent(KeyCode.S, EventType.KeyDown), () => KbInputBuffer.RegisterInput(KeyCode.S, EventType.KeyDown));
            _inputMappings.Add(new KbInputEvent(KeyCode.S, EventType.KeyUp), () => KbInputBuffer.RegisterInput(KeyCode.S, EventType.KeyUp));

            _inputMappings.Add(new KbInputEvent(KeyCode.A, EventType.KeyDown), () => KbInputBuffer.RegisterInput(KeyCode.A, EventType.KeyDown));
            _inputMappings.Add(new KbInputEvent(KeyCode.A, EventType.KeyUp), () => KbInputBuffer.RegisterInput(KeyCode.A, EventType.KeyUp));

            _inputMappings.Add(new KbInputEvent(KeyCode.D, EventType.KeyDown), () => KbInputBuffer.RegisterInput(KeyCode.D, EventType.KeyDown));
            _inputMappings.Add(new KbInputEvent(KeyCode.D, EventType.KeyUp), () => KbInputBuffer.RegisterInput(KeyCode.D, EventType.KeyUp));
        }

        public void HandleInput(InputEvent inputEvent)
        {
            if (_inputMappings.TryGetValue(new KbInputEvent(inputEvent.key, inputEvent.type), out var action))
            {
                action.Invoke();
                print(KbInputBuffer.DesiredMoveDir());
            }
        }

        private void Start()
        {
            playerRb = GetComponent<Rigidbody2D>();
            groundController = GetComponentInChildren<GroundController>();

            _jumpHandler = new JumpHandler(playerController: this);
            _dashHandler = new DashHandler(playerController: this);
            _runHandler = new RunHandler(playerController: this);

            InitInputMapping();
            InitPhysicsValues();
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
        }

        // void FixedUpdate()
        // {
        //     // if (isDashing)
        //     // {
        //     //     // leave some momentum after dashing
        //     //     if (!isDashing)
        //     //     {
        //     //         state = PlayerState.Jumping;
        //     //         currentVelocity = dashDirection * afterDashMomentum;
        //     //         // print(dashDirection + " * " + afterDashMomentum + " = " + currentVelocity);
        //     //     }
        //     // }
        //
        //     // if (isDashing)
        //     // {
        //     //     playerRb.MovePosition(transform.position + dashSpeed * Time.fixedDeltaTime * dashDirection);
        //     //     return;
        //     // }
        //
        //
        //     // jumping stuff
        //     // switch (state)
        //     // {
        //     //     case PlayerState.Grounded:
        //     //         // no gravity calc
        //     //         // exit condition
        //     //         currentVelocity.y = 0f;
        //     //         if (pressedJump)
        //     //         {
        //     //             state = PlayerState.StartedJump;
        //     //             goto case PlayerState.StartedJump;
        //     //         }
        //     //         else if (!groundControl.IsGrounded)
        //     //         {
        //     //             state = PlayerState.Falling;
        //     //             goto case PlayerState.Falling;
        //     //         }
        //     //         else
        //     //             break;
        //     //
        //     //     case PlayerState.StartedJump:
        //     //         // init velocity
        //     //         currentVelocity.y = startVerticalSpeedUp;
        //     //
        //     //         // continue calculations
        //     //         state = PlayerState.Jumping;
        //     //         goto case PlayerState.Jumping;
        //     //
        //     //     case PlayerState.Jumping:
        //     //         // add jump acceleration to current jump velocity
        //     //         if (!pressedJump || currentVelocity.y < 0f)
        //     //         {
        //     //             state = PlayerState.Falling;
        //     //             goto case PlayerState.Falling;
        //     //         }
        //     //
        //     //         currentVelocity.y += jumpGravity * Time.fixedDeltaTime;
        //     //
        //     //         goto default;
        //     //
        //     //     case PlayerState.Falling:
        //     //         // add fall acceleration to current jump velocity
        //     //         currentVelocity.y += fallGravity * Time.fixedDeltaTime;
        //     //
        //     //         goto default;
        //     //
        //     //     default:
        //     //         // make the player grounded
        //     //         if (groundControl.IsGrounded)
        //     //             state = PlayerState.Grounded;
        //     //
        //     //         break;
        //     // }
        //
        //
        //     // // running stuff
        //     // // replace all of these fucking Mathf function calls with something better
        //     // var runDirection = Input.GetAxisRaw("Horizontal"); // -1, 0, 1, why not give out an int then :<
        //     //
        //     // currentVelocity.x += GetAddedVelocity(runDirection);
        //     //
        //     // playerRb.MovePosition(transform.position + currentVelocity * Time.fixedDeltaTime);
        //     //
        //     //
        //     // float GetAddedVelocity(float runDirection)
        //     // {
        //     //     if (Mathf.Abs(runDirection) < valueCloseToZero)
        //     //     {
        //     //         // subtract just enough speed so it stops at 0
        //     //         return -Mathf.Min(Mathf.Abs(currentVelocity.x),
        //     //             Mathf.Abs(stopAcceleration * Time.deltaTime)) * Mathf.Sign(currentVelocity.x);
        //     //     }
        //     //
        //     //
        //     //     if (Mathf.Abs(currentVelocity.x) < valueCloseToZero // if the player is stationary (horizontally)
        //     //         || runDirection * currentVelocity.x > 0) // or if he continues to run in the same direction as before
        //     //     {
        //     //         if (groundControl.IsGrounded)
        //     //         {
        //     //             // brake if current velocity is greater than max
        //     //             if (Mathf.Abs(currentVelocity.x) > maxRunningSpeed)
        //     //             {
        //     //                 return Mathf.Max(-brakeAcceleration * Time.deltaTime, maxRunningSpeed - Mathf.Abs(currentVelocity.x)) * runDirection;
        //     //             }
        //     //             return Mathf.Min(runAcceleration * Time.deltaTime, maxRunningSpeed - Mathf.Abs(currentVelocity.x)) * runDirection;
        //     //         }
        //     //         // conserve velocity
        //     //         return Mathf.Abs(currentVelocity.x) > maxRunningSpeed
        //     //             ? 0f
        //     //             : runAcceleration * Time.deltaTime * runDirection;
        //     //
        //     //         // return Mathf.Min(runAcceleration * Time.deltaTime, maxRunningSpeed - Mathf.Abs(currentVelocity.x)) * runDirection;
        //     //     }
        //     //     return brakeAcceleration * runDirection * Time.deltaTime;
        //     // }
        //     //
        // }
    }
}