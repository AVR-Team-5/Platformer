using UnityEngine;
using UnityEngine.InputSystem;

namespace Source.PlayerController
{
    public class DashHandler : MonoBehaviour
    {
        private PlayerController _playerController;

        [SerializeField] private float valueCloseToZero;
        [SerializeField] private float dashDuration;
        [SerializeField] private float dashDistance;
        [SerializeField] private Vector2 afterDashMomentumMultiplier;
        
        private float _dashSpeed;
        private Vector2 _afterDashMomentum;
        // vertical inertia needs to be higher to be noticeable after gravity
        // maybe set dashSpeed as vector2 as well for different diagonal dash angles 

        private bool _isDashing;
        private float _currentDashDuration;
        private Vector2 _dashDirection;
        private float _playerFacingDirection = 1;

        private Vector2 TargetMoveDir { get; set; } = Vector2.zero;

        public void OnMovement(InputValue value)
        {
            TargetMoveDir = value.Get<Vector2>();

            if (Mathf.Abs(TargetMoveDir.x) > valueCloseToZero)
                _playerFacingDirection = TargetMoveDir.x > 0 ? 1 : -1;
            // takes care of edge cases when the player inputs a vertical dir as well
        }


        public void OnDash()
        {
            StartDash();
        }

        private void Start()
        {
            _playerController = GetComponent<PlayerController>();

            InitPhysicsValues();
        }

        private void OnValidate()
        {
            InitPhysicsValues();
        }

        private void InitPhysicsValues()
        {
            _dashSpeed = dashDistance / dashDuration;
            _afterDashMomentum = _dashSpeed * afterDashMomentumMultiplier;
        }

        public void StartDash()
        {
            var direction = TargetMoveDir.magnitude > valueCloseToZero
                ? TargetMoveDir / TargetMoveDir.magnitude
                : Vector2.right * _playerFacingDirection;
            
            _isDashing = true;
            _dashDirection = direction.normalized;
            _currentDashDuration = 0f;

            _playerController.currentVelocity = _dashSpeed * _dashDirection;
        }

        // this one should go in FixedUpdate
        public bool CycleDash()
        {
            if (_isDashing)
            {
                _currentDashDuration += Time.fixedDeltaTime;

                // somehow get _playerController.dashDuration inside this class
                // so it doesn't have to peek outside each frame
                _isDashing = _currentDashDuration <= dashDuration;

                if (!_isDashing)
                {
                    // the jump script should figure out for itself if the char is jumping after dash or not
                    // _playerController.state = PlayerState.Jumping;
                    _playerController.currentVelocity = Vector2.Scale(_dashDirection, _afterDashMomentum);
                }
            }

            if (_isDashing)
            {
                _playerController.currentVelocity = _dashSpeed * _dashDirection;
                // Debug.Log("Dashing to " + _playerController.dashSpeed * Time.fixedDeltaTime * _dashDirection);

                // _playerRb.MovePosition(_playerController.transform.position +
                // _playerController.dashSpeed * Time.fixedDeltaTime * _dashDirection);

                // if this executes then make it cancel any other movement from _playerController
                // kinda if this would straight up do a return in _playerController's FixedUpdate
            }

            return _isDashing;
        }
    }
}