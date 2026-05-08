using UnityEngine;

namespace CodeBlue
{
    public class SuppliesComputer : BaseInteractable
    {
        public override void InteractHold(GameObject interactedBy)
        {
            UIManager.Instance.LoadScreen(UIScreenType.Preparation);
        }
    }
}
