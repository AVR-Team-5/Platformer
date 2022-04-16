using UnityEngine;


// it is the rigidbody who gets the collider events
// https://docs.unity3d.com/Manual/CollidersOverview.html#CollisionCallbacks
[RequireComponent(typeof(Rigidbody2D))]
public class Attackable : MonoBehaviour {
    public delegate void GetHitHandler();
    event GetHitHandler GetHitEvent;

    void OnTriggerEnter(Collider other) {
        // TODO: various checks that it doesn't hit itself, it gets hit by an actual attack collider, etc
        // also look into compound colliders and their behaviour

        GetHitEvent?.Invoke();
    }

    // debug stuff

    void LogHit() {
        print(this.name + " got hit");
    }

    void Start() {
        GetHitEvent += LogHit;
    }
}