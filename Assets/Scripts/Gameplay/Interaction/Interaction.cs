using NaughtyAttributes;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace CodeBlue
{
    public class Interaction : NetworkBehaviour
    {
        [SerializeField] private LayerMask _interactionMask;
        [SerializeField] InteractionPromptUI _interactUI;

        private bool _canInteract = true;
        private bool _isInteractionCooldown = false;
        private float _currCooldown = 0;

        public GameObject CarriedItem { get; set; }

        private Movement _movement;
        private InputManager _input;
        private PlayerRole _role;

        private IInteractable _currentInteractable;

        #region Unity Functions
        void Start()
        {
            _movement = GetComponent<Movement>();
            _input = GetComponent<InputManager>();
            _role = GetComponent<PlayerRole>();

            _input.OnInteract.AddListener(OnInteract);
            _input.OnInteractHold.AddListener(OnInteractHold);
        }

        void Update()
        {
            if (_movement.IsFrozen) return;
            CheckForInteractables();
            CheckInteractionCooldown();
        }
        void OnTriggerStay(Collider col)
        {
            if (!_canInteract) return;

            if (col.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                if (interactable.Type != InteractableType.Volume) return;

                _currentInteractable = interactable;
            }
        }

        private void OnTriggerExit(Collider col)
        {
            if (col.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                if (interactable.Type != InteractableType.Volume) return;
                _currentInteractable = null;
            }
        }
        #endregion

        public void FreezeInteraction(bool frozen)
        {
            _movement.IsFrozen = frozen;
            _canInteract = false;
            _isInteractionCooldown = true;
        }

        private void CheckForInteractables()
        {
            var box = (Vector3.one * 1.1f) + (Vector3.forward * 3.1f);
            if (Physics.BoxCast(transform.position, box / 2, transform.forward, out RaycastHit hit, Quaternion.identity, 3.1f, _interactionMask, QueryTriggerInteraction.Collide))
            {
                if (hit.collider != null && hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    _currentInteractable = interactable;
                }
            }
            else
                _currentInteractable = null;

            _input.CanHold = _currentInteractable != null && _currentInteractable.Interaction.HasFlag(InteractionType.Hold);
        }

        void OnInteract()
        {
            if (!IsInteractionPossible(_currentInteractable)) return;
            if (_currentInteractable.Interaction.HasFlag(InteractionType.Click))
            {
                _currentInteractable.Interact(gameObject);
                _canInteract = false;
                _isInteractionCooldown = true;
            }
        }

        void OnInteractHold()
        {
            if (!IsInteractionPossible(_currentInteractable)) return;
            if (_currentInteractable.Interaction.HasFlag(InteractionType.Hold))
            {
                _currentInteractable.InteractHold(gameObject);
                _canInteract = false;
                _isInteractionCooldown = true;
            }
        }

        // FIXME: please please please i hate how this looks
        void UpdateInteractionText(IInteractable i)
        {
            _interactUI.SetInteractingWith(i?.Type == InteractableType.Item ? i.ToString() : null);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void Cl_CarryItemRpc(ulong itemId, RpcParams rpcParams)
        {
            if (CarriedItem != null)
            {
                AudioManager.Instance.PlayUISfx("error");
                return;
            }
            var item = NetworkManager.SpawnManager.SpawnedObjects[itemId].gameObject;
            CarryItem(item);
        }

        public void CarryItem(GameObject itemToCarry)
        {
            CarriedItem = itemToCarry;
            _currentInteractable = null;
            AudioManager.Instance.PlayGameSfx("carry");

            Assert.IsNotNull(CarriedItem.GetComponent<Carryable>(), $"{itemToCarry.name} does not inherit from carryable!");

            CarriedItem.GetComponent<Carryable>().Carry(transform);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void Cl_DropItemRpc(Vector3 position, Vector3 offset, RpcParams rpcParams)
        {
            var dropped = DropItem(position);
            dropped.transform.position += offset;
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void Cl_DropItemAtTransformRpc(ulong networkObjectId, Vector3 offset, RpcParams rpcParams)
        {
            var obj = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];
            var dropped = DropItem(obj.transform.position, owner: obj.transform);
            dropped.transform.position += offset;
        }

        public GameObject DropItem(Vector3 dropSpot, bool destroyItem = false, Transform owner = null)
        {
            if (destroyItem)
            {
                AudioManager.Instance.PlayGameSfx("trash");
                Sv_DestroyItemRpc(CarriedItem.GetComponent<NetworkObject>().NetworkObjectId);
                CarriedItem = null;
                return null;
            }

            var droppedItem = CarriedItem;
            droppedItem.GetComponent<Collider>().enabled = false;
            CarriedItem.GetComponent<Carryable>().Drop(dropSpot, owner: owner);
            CarriedItem = null;

            AudioManager.Instance.PlayGameSfx("drop");
            return droppedItem;
        }

        void CheckInteractionCooldown()
        {
            if (!_isInteractionCooldown) return;

            _currCooldown += Time.deltaTime;
            if (_currCooldown >= 1)
            {
                _canInteract = true;
                _currCooldown = 0;
                _isInteractionCooldown = false;
            }
        }

        [Rpc(SendTo.Server)]
        void Sv_DestroyItemRpc(ulong itemId)
        {
            NetworkManager.SpawnManager.SpawnedObjects[itemId].Despawn();
        }

        bool IsInteractionPossible(IInteractable interactable)
        {
            if (interactable == null) return false;

            var phaseRes = (interactable.GamePhaseRestriction & GameManager.Instance.CurrentPhaseEnum) != 0;
            var roleRes = (interactable.RoleRestriction & _role.CurrentRole.Value) != 0;

            return phaseRes && roleRes;
        }
    }
}
