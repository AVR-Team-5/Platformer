using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: make sure floor trigger gets out of ground at the first frame of the jump
// otherwise it might get fucky with jump cancels

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
    public float horizontalSpeed;
    public float startVerticalSpeedUp;
    public float jumpGravity;
    public float fallGravity;

    GroundController gc;
    Rigidbody2D rb;
    Vector2 horizontalDirection;
    bool isJumping = false;
    float currentVerticalSpeed;
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
        Vector2 finalFrameMovement = Vector2.zero;

        // print("Starting switch");

        switch (state)
        {
            case PlayerState.Grounded:
                // no gravity calc
                // exit condition
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

        // print("Left switch");


        rb.MovePosition(transform.position + Vector3.up * currentVerticalSpeed * Time.fixedDeltaTime);
        print(state);


        // var finalMovement = horizontalDirection.normalized * horizontalSpeed;

        // if (gc.IsGrounded) {

        // } else {

        // }

        // if (isJumping) {
        //     if (wasJumpingLastFrame == false)
        //         currentVerticalSpeed = startVerticalSpeedUp;

        //     currentVerticalSpeed -= jumpGravity * Time.fixedDeltaTime;
        //     wasJumpingLastFrame = true;
        // } else {
        //     currentVerticalSpeed -= fallGravity * Time.fixedDeltaTime;
        //     wasJumpingLastFrame = false;
        // }
    }
}
