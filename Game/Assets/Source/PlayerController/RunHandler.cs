using UnityEngine;
using static UnityEngine.Mathf;

namespace Source.PlayerController
{
    public class RunHandler
    {
        private readonly PlayerController _playerController;
        private readonly Rigidbody2D _rigidbody;
        private readonly Transform _transform;
        private readonly GroundController _groundController;


        public RunHandler(PlayerController playerController)
        {
            _playerController = playerController;
            _rigidbody = playerController.playerRb;
            _transform = playerController.transform;
            _groundController = playerController.groundController;
        }

        public void FixedUpdate()
        {
            // TODO: replace all of these fucking Mathf function calls with something better

            var runDirection = _playerController.TargetMoveDir.x; // -1, 0, 1
            
            _playerController.currentVelocity.x += GetAddedVelocity(runDirection);

            float GetAddedVelocity(float runDirection)
            {
                if (Abs(runDirection) < _playerController.valueCloseToZero)
                {
                    // subtract just enough speed so it stops at 0
                    return -Min(Abs(_playerController.currentVelocity.x),
                        Abs(_playerController.stopAcceleration * Time.deltaTime)) * Sign(_playerController.currentVelocity.x);
                }


                if (Abs(_playerController.currentVelocity.x) < _playerController.valueCloseToZero // if the player is stationary (horizontally)
                    || runDirection * _playerController.currentVelocity.x > 0) // or if he continues to run in the same direction as before
                {
                    if (_groundController.IsGrounded)
                    {
                        // brake if current velocity is greater than max
                        if (Abs(_playerController.currentVelocity.x) > _playerController.maxRunningSpeed)
                        {
                            return Max(-_playerController.brakeAcceleration * Time.deltaTime, _playerController.maxRunningSpeed - Abs(_playerController.currentVelocity.x)) * runDirection;
                        }
                        return Min(_playerController.runAcceleration * Time.deltaTime, _playerController.maxRunningSpeed - Abs(_playerController.currentVelocity.x)) * runDirection;
                    }
                    // conserve velocity
                    return Abs(_playerController.currentVelocity.x) > _playerController.maxRunningSpeed
                        ? 0f
                        : _playerController.runAcceleration * Time.deltaTime * runDirection;

                    // return Min(runAcceleration * Time.deltaTime, maxRunningSpeed - Abs(currentVelocity.x)) * runDirection;
                }
                return _playerController.brakeAcceleration * runDirection * Time.deltaTime;
            }
        }
    }
}