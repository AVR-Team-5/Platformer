using UnityEngine;

namespace Source.PlayerController
{
    public class DashHandler
    {
        private readonly PlayerController _playerController;
        private readonly Rigidbody2D _playerRb;


        private bool _isDashing;
        private float _currentDashDuration;
        private Vector3 _dashDirection;

        public DashHandler(PlayerController playerController)
        {
            _playerController = playerController;
            _playerRb = playerController.playerRb;
        }

        public void Start(Vector2 direction)
        {
            _isDashing = true;
            _dashDirection = direction.normalized;
            _currentDashDuration = 0f;

            _playerController.currentVelocity = _playerController.dashSpeed * _dashDirection;
        }

        // this one should go in FixedUpdate
        public bool FixedUpdate()
        {
            if (_isDashing)
            {
                _currentDashDuration += Time.fixedDeltaTime;

                // somehow get _playerController.dashDuration inside this class
                // so it doesn't have to peek outside each frame
                _isDashing = _currentDashDuration <= _playerController.dashDuration;

                if (!_isDashing)
                {
                    // the jump script should figure out for itself if the char is jumping after dash or not
                    // _playerController.state = PlayerState.Jumping;
                    _playerController.currentVelocity = _dashDirection * _playerController.afterDashMomentum;
                }
            }

            if (_isDashing)
            {
                _playerController.currentVelocity = _playerController.dashSpeed * _dashDirection;
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