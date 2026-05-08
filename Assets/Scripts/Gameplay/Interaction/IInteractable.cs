using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public enum InteractableType
    {
        Item,   // Item pickup
        World,  // Rooms/Buttons/World interaction
        Entity,  // Patients, other players, etc
        Volume
    };

    [System.Flags]
    public enum InteractionType
    {
        Click = 1 << 1,  // On interact clicked
        Hold = 1 << 2,   // On interact hold
        Enter = 1 << 3,  // On player enter
    };

    /// <summary>
    /// interface used to define which objects are interactable, no need to set tags
    /// </summary>
    // NOTE: (zsh) i miss using base classes...
    // NOTE: (zsh feb21) we're finally using base classes again
    public interface IInteractable
    {
        InteractableType Type { get; set; }
        InteractionType Interaction { get; set; }
        NetworkVariable<bool> IsInteractionDisabled { get; set; }

        PlayerRoles RoleRestriction { get; set; }
        GamePhase GamePhaseRestriction { get; set; }

        /// <summary>
        /// Is executed when interacted
        /// </summary>
        /// <param name="interactedBy">Player who interacted</param>
        void Interact(GameObject interactedBy);
        void InteractHold(GameObject interactedBy);

    }

    public abstract class BaseInteractable : NetworkBehaviour, IInteractable
    {
        [field: SerializeField] public InteractableType Type { get; set; }
        [field: SerializeField] public InteractionType Interaction { get; set; }
        public NetworkVariable<bool> IsInteractionDisabled { get; set; } = new();
        [field: SerializeField] public PlayerRoles RoleRestriction { get; set; }
        [field: SerializeField] public GamePhase GamePhaseRestriction { get; set; }

        public virtual void Interact(GameObject interactedBy) { }
        public virtual void InteractHold(GameObject interactedBy) { }

        public bool IsRestricted(GameObject player)
        {
            return player.GetComponent<PlayerRole>().CurrentRole.Value.HasFlag(RoleRestriction) && GameManager.Instance.IsPhase(GamePhaseRestriction);
        }
    }
}
