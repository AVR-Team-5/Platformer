using UnityEngine;

// TODO: make sure floor trigger gets out of ground at the first frame of the jump
// otherwise it might get fucky 
// as the state machine will end up in an infinite cycle

// TODO: rewrite the HELL out of this script

enum PlayerState
{
    Grounded,
    StartedJump,
    Jumping,
    Falling
}

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // TODO: reinit these values if they're switched in editor
    public float jumpHeight;
    public float timeTillJumpPeak;
    public float timeTillFallPeak;

    private float startVerticalSpeedUp;
    private float jumpGravity;
    private float fallGravity;

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

    GroundController _groundControl;
    Rigidbody2D _rigidbody;
    
    bool pressedJump;
    
    // float currentHorizontalSpeed;
    private Vector3 currentVelocity;
    
    PlayerState state = PlayerState.Falling;

    bool pressedDash;
    bool isDashing;
    float currentDashDuration;
    private Vector3 dashDirection;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _groundControl = GetComponentsInChildren<GroundController>()[0];

        jumpGravity = timeTillJumpPeak * timeTillJumpPeak / (2 * jumpHeight);
        fallGravity = timeTillJumpPeak * timeTillJumpPeak / (2 * jumpHeight);
        startVerticalSpeedUp = jumpHeight / timeTillJumpPeak - jumpGravity * timeTillJumpPeak / 2;
    }

    void Update()
    {
        pressedJump = Input.GetAxisRaw("Jump") > 0.5f;
        
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown("joystick button 1"))
        // if (Input.GetAxisRaw("Dash") > 0.5f)
            pressedDash = true;
    }

    void FixedUpdate()
    {
        if (pressedDash)
        {
            pressedDash = false;
            isDashing = true;
            
            currentVelocity = Vector3.zero;
            
            currentDashDuration = 0f;
            dashDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        }
        else if (isDashing)
        {
            currentDashDuration += Time.fixedDeltaTime;
            isDashing = currentDashDuration <= dashDuration;

            // leave some momentum after dashing
            if (!isDashing)
            {
                state = PlayerState.Jumping;
                currentVelocity = dashDirection * afterDashMomentum;
                // print(dashDirection + " * " + afterDashMomentum + " = " + currentVelocity);
            }
        }


        if (isDashing)
        {
            _rigidbody.MovePosition(transform.position + dashSpeed * Time.fixedDeltaTime * dashDirection);
            return;
        }


        // jumping stuff
        switch (state)
        {
            case PlayerState.Grounded:
                // no gravity calc
                // exit condition
                currentVelocity.y = 0f;
                if (pressedJump)
                {
                    state = PlayerState.StartedJump;
                    goto case PlayerState.StartedJump;
                }
                else if (!_groundControl.IsGrounded)
                {
                    state = PlayerState.Falling;
                    goto case PlayerState.Falling;
                }
                else
                    break;

            case PlayerState.StartedJump:
                // init velocity
                currentVelocity.y = startVerticalSpeedUp;

                // continue calculations
                state = PlayerState.Jumping;
                goto case PlayerState.Jumping;

            case PlayerState.Jumping:
                // add jump acceleration to current jump velocity
                if (!pressedJump || currentVelocity.y < 0f)
                {
                    state = PlayerState.Falling;
                    goto case PlayerState.Falling;
                }

                currentVelocity.y += jumpGravity * Time.fixedDeltaTime;

                goto default;

            case PlayerState.Falling:
                // add fall acceleration to current jump velocity
                currentVelocity.y += fallGravity * Time.fixedDeltaTime;

                goto default;

            default:
                // make the player grounded
                if (_groundControl.IsGrounded)
                    state = PlayerState.Grounded;

                break;
        }
        

        // running stuff
        // TODO: replace all of these fucking Mathf function calls with something better
        var runDirection = Input.GetAxisRaw("Horizontal"); // -1, 0, 1, why not give out an int then :<
        
        currentVelocity.x += GetAddedVelocity(runDirection);

        _rigidbody.MovePosition(transform.position + currentVelocity * Time.fixedDeltaTime);


        float GetAddedVelocity(float runDirection)
        {
            if (Mathf.Abs(runDirection) < valueCloseToZero)
            {
                // subtract just enough speed so it stops at 0
                return -Mathf.Min(Mathf.Abs(currentVelocity.x),
                    Mathf.Abs(stopAcceleration * Time.deltaTime)) * Mathf.Sign(currentVelocity.x);
            }


            if (Mathf.Abs(currentVelocity.x) < valueCloseToZero // if the player is stationary (horizontally)
                            || runDirection * currentVelocity.x > 0) // or if he continues to run in the same direction as before
            {
                if (_groundControl.IsGrounded)
                {
                    // brake if current velocity is greater than max
                    if (Mathf.Abs(currentVelocity.x) > maxRunningSpeed)
                    {
                        return Mathf.Max(-brakeAcceleration * Time.deltaTime, maxRunningSpeed - Mathf.Abs(currentVelocity.x)) * runDirection;
                    }
                    return Mathf.Min(runAcceleration * Time.deltaTime, maxRunningSpeed - Mathf.Abs(currentVelocity.x)) * runDirection;
                }
                // conserve velocity
                return Mathf.Min(runAcceleration * Time.deltaTime, maxRunningSpeed - Mathf.Abs(currentVelocity.x)) * runDirection;
            }
            return brakeAcceleration * runDirection * Time.deltaTime;
        }
    }

}
