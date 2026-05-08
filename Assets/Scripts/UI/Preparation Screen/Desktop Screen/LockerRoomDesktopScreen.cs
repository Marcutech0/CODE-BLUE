using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public class LockerRoomDesktopScreen : NetworkBehaviour
    {
        public void SetScreenVisibility(Vector3 newPos)
        {
            var cg = GetComponent<CanvasGroup>();
            cg.interactable = newPos == Vector3.zero;
            transform.localPosition = newPos;
        }
    }
}
