using UnityEngine;

namespace Source.PlayerController
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class GroundController : MonoBehaviour
    {
        public bool IsGrounded { get; private set; }

        private void OnTriggerEnter2D(Collider2D col)
        {
            IsGrounded = true;
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            IsGrounded = false;
        }
    }
}
