using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GroundController : MonoBehaviour
{
    public bool IsGrounded { get { return isGrounded; } }
    public bool isGrounded = false;
    void OnTriggerEnter2D(Collider2D col)
    {
        isGrounded = true;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        isGrounded = false;
    }
}
