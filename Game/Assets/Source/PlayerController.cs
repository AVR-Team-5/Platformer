using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: make sure floor trigger gets out of ground at the first frame of the jump
// otherwise it might get fucky 
// as the state machine will end up in an infinite cycle

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

    GroundController gc;
    Rigidbody2D rb;
    
    Vector2 horizontalDirection;
    bool pressedJump;
    
    // float currentHorizontalSpeed;
    private Vector3 currentVelocity;
    
    PlayerState state = PlayerState.Falling;

    bool pressedDash;
    bool isDashing = false;
    float currentDashDuration;
    private Vector3 dashDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gc = GetComponentsInChildren<GroundController>()[0];
    }

    void Update()
    {
        horizontalDirection += Vector2.right * Input.GetAxisRaw("Horizontal");

        pressedJump = Input.GetKey(KeyCode.Space);
        
        if (Input.GetKeyDown(KeyCode.LeftShift))
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
            dashDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
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
                print(dashDirection + " * " + afterDashMomentum + " = " + currentVelocity);
            }
        }


        if (isDashing)
        {
            rb.MovePosition(transform.position + dashSpeed * Time.fixedDeltaTime * dashDirection);
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
                else if (!gc.IsGrounded)
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
                currentVelocity.y += jumpGravity * Time.fixedDeltaTime;

                if (!pressedJump || currentVelocity.y < 0f)
                    goto case PlayerState.Falling;

                goto default;

            case PlayerState.Falling:
                // add fall acceleration to current jump velocity
                currentVelocity.y += fallGravity * Time.fixedDeltaTime;

                goto default;

            default:
                // make the player grounded
                if (gc.IsGrounded)
                    state = PlayerState.Grounded;

                break;
        }


        // running stuff
        // TODO: replace all of these fucking Mathf function calls with something better
        var runDirection = Input.GetAxisRaw("Horizontal"); // -1, 0, 1, why not give out an int then :<
        
        if (Mathf.Abs(runDirection) > valueCloseToZero)   // if there is an input to run
        {
            if (Mathf.Abs(currentVelocity.x) < valueCloseToZero // if the player is stationary (horizontally)
                || runDirection * currentVelocity.x > 0) // or if he continues to run in the same direction as before
            {
                currentVelocity.x += runAcceleration * runDirection * Time.deltaTime; //accelerate further
            }
            else //otherwise the player is braking
            {
                currentVelocity.x += brakeAcceleration * runDirection * Time.deltaTime;
            }
        }
        else    // if no input
        {
            // subtract just enough speed so it stops at 0
            currentVelocity.x -= Mathf.Min(Mathf.Abs(currentVelocity.x),
                Mathf.Abs(stopAcceleration * Time.deltaTime)) * Mathf.Sign(currentVelocity.x);
        }

        // limiting the speed to its scripted max
        // TODO: don't clamp if the speed is received from an outside source (such as dashing)
        currentVelocity.x = Mathf.Clamp(currentVelocity.x, -maxRunningSpeed, maxRunningSpeed);


        rb.MovePosition(transform.position + currentVelocity * Time.fixedDeltaTime);
    }

}
