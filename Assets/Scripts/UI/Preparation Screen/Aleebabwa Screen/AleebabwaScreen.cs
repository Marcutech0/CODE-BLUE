using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public class AleebabwaScreen : NetworkBehaviour
    {
        [field: Header("Functionality")]
        [field: SerializeField] public ExitDesktopApp ExitDesktopApp { private set; get; }

        public void SetScreenVisibility(Vector3 newPos)
        {
            transform.localPosition = newPos;
        }
    }
}