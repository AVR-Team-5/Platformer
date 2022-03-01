using UnityEngine;

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

        // public void Start()
        // {
        //     lmao, character is always running so no need for a start function
        // }

        public void FixedUpdate()
        {
            // TODO: replace all of these fucking Mathf function calls with something better

            var runDirection = _playerController.TargetMoveDir.x; // -1, 0, 1
            
            _playerController.currentVelocity.x += GetAddedVelocity(runDirection);

            float GetAddedVelocity(float runDirection)
            {
                if (Mathf.Abs(runDirection) < _playerController.valueCloseToZero)
                {
                    // subtract just enough speed so it stops at 0
                    return -Mathf.Min(Mathf.Abs(_playerController.currentVelocity.x),
                        Mathf.Abs(_playerController.stopAcceleration * Time.deltaTime)) * Mathf.Sign(_playerController.currentVelocity.x);
                }


                if (Mathf.Abs(_playerController.currentVelocity.x) < _playerController.valueCloseToZero // if the player is stationary (horizontally)
                    || runDirection * _playerController.currentVelocity.x > 0) // or if he continues to run in the same direction as before
                {
                    if (_groundController.IsGrounded)
                    {
                        // brake if current velocity is greater than max
                        if (Mathf.Abs(_playerController.currentVelocity.x) > _playerController.maxRunningSpeed)
                        {
                            return Mathf.Max(-_playerController.brakeAcceleration * Time.deltaTime, _playerController.maxRunningSpeed - Mathf.Abs(_playerController.currentVelocity.x)) * runDirection;
                        }
                        return Mathf.Min(_playerController.runAcceleration * Time.deltaTime, _playerController.maxRunningSpeed - Mathf.Abs(_playerController.currentVelocity.x)) * runDirection;
                    }
                    // conserve velocity
                    return Mathf.Abs(_playerController.currentVelocity.x) > _playerController.maxRunningSpeed
                        ? 0f
                        : _playerController.runAcceleration * Time.deltaTime * runDirection;

                    // return Mathf.Min(runAcceleration * Time.deltaTime, maxRunningSpeed - Mathf.Abs(currentVelocity.x)) * runDirection;
                }
                return _playerController.brakeAcceleration * runDirection * Time.deltaTime;
            }
        }
    }
}