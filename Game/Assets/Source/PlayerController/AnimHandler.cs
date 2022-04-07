using System;
using UnityEngine;

namespace Source.PlayerController
{
    public class AnimHandler : MonoBehaviour
    {
        // a higher priority anim can always interrupt a lower priority anim
        // a lower priority anim waits until the higher priority one finishes
        // same priority anims can interrupt each other
        public struct AnimStruct
        {
            public int Priority;
            public int Hash;
            public string Name;

            public AnimStruct(int priority, string name)
            {
                Priority = priority;
                Hash = Animator.StringToHash(name);
                Name = name;
            }
        }
        
        private PlayerController _playerController;
        private Animator _animator;
        private bool _isMovingLeft;

        public AnimHandler(PlayerController playerController)
        {
            _playerController = playerController;
            _animator = playerController.GetComponent<Animator>();
        }

        private void Start()
        {
            _playerController = GetComponent<PlayerController>();
            _animator = GetComponent<Animator>();
        }

        enum AnimPriorityFlags
        {
            // inferred from the context
            // idle, jumping, walljumping, falling
            // running, wallslide
            None = 0,

            // minimal player input
            Low = 1 << 0,

            // kinda important player actions
            // start jump, start wall jump
            Medium = 1 << 1,

            // important player actions
            // attacks (low, high, overhead)
            High = 1 << 2,

            // Critical actions
            // hitstun
            Critical = 1 << 3,
        }

        public void OnMeleeAttack()
        {
            _animator.Play(AnimClips.MeleeAtkOverhead);
        }

        static class AnimClips
        {
            public static readonly int Idle = Animator.StringToHash("Player_Idle");
            public static readonly int Run = Animator.StringToHash("Player_Run");
            public static readonly int HitStun = Animator.StringToHash("Player_HitStun");
            public static readonly int MeleeAtkHigh = Animator.StringToHash("Player_MeleeAtkHigh");
            public static readonly int MeleeAtkLow = Animator.StringToHash("Player_MeleeAtkLow");
            public static readonly int MeleeAtkOverhead = Animator.StringToHash("Player_MeleeAtkOverhead");
            public static readonly int StartJump = Animator.StringToHash("Player_StartJump");
            public static readonly int StartWallJump = Animator.StringToHash("Player_StartWallJump");
            public static readonly int Jumping = Animator.StringToHash("Player_Jumping");
            public static readonly int WallJumping = Animator.StringToHash("Player_WallJumping");
            public static readonly int WallSlide = Animator.StringToHash("Player_WallSlide");
            public static readonly int Falling = Animator.StringToHash("Player_Falling");
        }
    }
}