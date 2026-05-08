using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBlue
{
    [RequireComponent(typeof(InputManager))]
    public class Movement : NetworkBehaviour
    {
        NetworkVariable<bool> _isFeetParticlesRunning = new(false);

        public bool IsFrozen { get; set; }

        [Header("Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 0.2f;
        [field: SerializeField, Range(1, 2)] public float Multiplier = 1f;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private float _footstepsDuration = 0.5f;
        [SerializeField] private AudioClip[] _footstepsSfx;

        [Header("References")]
        [SerializeField] InputManager _input;
        [SerializeField] AudioSource _audio;
        [SerializeField] ParticleSystem _footstepsVfx;

        private float _footstepsCooldown;
        private Rigidbody _rb;
        private Vector3 _direction = Vector3.zero;
        private bool _isGrounded = false;

        Queue<AudioClip> _footstepsQueue = new();

        private void Start()
        {
            ResetFootstepQueue();
        }

        public override void OnNetworkSpawn()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

            if (IsOwner)
            {
                _input = GetComponent<InputManager>();
                _input.Player.Movement.performed += OnMovePerformed;
                _input.Player.Movement.canceled += (_) => _direction = Vector3.zero;
            }
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;
            if (IsFrozen)
            {
                _rb.linearVelocity = Vector3.zero;
                return;
            }

            var vel = Vector3.ClampMagnitude(_direction.normalized * _moveSpeed * Multiplier, _moveSpeed * Multiplier);
            vel.y = _isGrounded ? 0 : Physics.gravity.y;
            _rb.linearVelocity = vel;
        }

        void Update()
        {
            if (_isFeetParticlesRunning.Value && !_footstepsVfx.isPlaying)
                _footstepsVfx.Play();
            else if (!_isFeetParticlesRunning.Value && _footstepsVfx.isPlaying)
                _footstepsVfx.Stop();

            if (!IsOwner) return;
            if (IsFrozen)
            {
                _direction = Vector3.zero;
                return;
            }

            bool isMoving = _direction.normalized.sqrMagnitude > 0;
            if (isMoving)
            {
                if (_direction.normalized.sqrMagnitude > 0)
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_direction.normalized), _rotationSpeed * Time.deltaTime);

                _footstepsCooldown -= Time.deltaTime;
                if (_footstepsCooldown <= 0)
                {
                    if (!_footstepsQueue.TryPeek(out _))
                        ResetFootstepQueue();

                    _audio.PlayOneShot(_footstepsQueue.Dequeue());
                    _footstepsCooldown = _footstepsDuration;
                }
                if (!_isFeetParticlesRunning.Value)
                    Sv_SetIsRunningRpc(true);
            }
            else
            {
                _footstepsCooldown = _footstepsDuration;
                if (_isFeetParticlesRunning.Value)
                    Sv_SetIsRunningRpc(false);
            }

            _isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, _groundMask);
        }

        [Rpc(SendTo.Server)]
        void Sv_SetIsRunningRpc(bool isRunning) => _isFeetParticlesRunning.Value = isRunning;

        void ResetFootstepQueue()
        {
            foreach (var clip in _footstepsSfx.Shuffle())
                _footstepsQueue.Enqueue(clip);
        }

        public void OnMovePerformed(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;

            Vector2 input = context.ReadValue<Vector2>();
            _direction.x = input.x;
            _direction.z = input.y;
        }
    }
}
