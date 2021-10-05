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

    GroundController gc;
    Rigidbody2D rb;
    Vector2 horizontalDirection;
    bool isJumping;
    float currentVerticalSpeed;
    float currentHorizontalSpeed;
    PlayerState state = PlayerState.Falling;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gc = GetComponentsInChildren<GroundController>()[0];
    }

    void Update()
    {
        horizontalDirection += Vector2.right * Input.GetAxisRaw("Horizontal");

        isJumping = Input.GetKey(KeyCode.Space);
    }

    void FixedUpdate()
    {
        // jumping stuff
        switch (state)
        {
            case PlayerState.Grounded:
                // no gravity calc
                // exit condition
                currentVerticalSpeed = 0f;
                if (isJumping) {
                    state = PlayerState.StartedJump;
                    goto case PlayerState.StartedJump;
                }
                else
                    break;

            case PlayerState.StartedJump:
                // init velocity
                currentVerticalSpeed = startVerticalSpeedUp;

                // continue calculations
                state = PlayerState.Jumping;
                goto case PlayerState.Jumping;

            case PlayerState.Jumping:
                // add jump acceleration to current jump velocity
                currentVerticalSpeed += jumpGravity * Time.fixedDeltaTime;

                if (!isJumping || currentVerticalSpeed < 0f)
                    goto case PlayerState.Falling;

                goto default;

            case PlayerState.Falling:
                // add fall acceleration to current jump velocity
                currentVerticalSpeed += fallGravity * Time.fixedDeltaTime;

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
            if (Mathf.Abs(currentHorizontalSpeed) < valueCloseToZero // if the player is stationary (horizontally)
                || runDirection * currentHorizontalSpeed > 0) // or if he continues to run in the same direction as before
            {
                currentHorizontalSpeed += runAcceleration * runDirection * Time.deltaTime; //accelerate further
            }
            else //otherwise the player is braking
            {
                currentHorizontalSpeed += brakeAcceleration * runDirection * Time.deltaTime;
            }
        }
        else    // if no input
        {
            // subtract just enough speed so it stops at 0
            currentHorizontalSpeed -= Mathf.Min(Mathf.Abs(currentHorizontalSpeed), 
                Mathf.Abs(stopAcceleration * Time.deltaTime)) * Mathf.Sign(currentHorizontalSpeed);
        }

        // limiting the speed to its scripted max
        currentHorizontalSpeed = Mathf.Clamp(currentHorizontalSpeed, -maxRunningSpeed, maxRunningSpeed);
        

        rb.MovePosition(transform.position + currentVerticalSpeed * Time.fixedDeltaTime * Vector3.up
                                                 + currentHorizontalSpeed * Time.fixedDeltaTime * Vector3.right);
    }
}
