using TMPro;
using UnityEngine;

namespace CodeBlue
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] GameObject _interactablePanel;
        [SerializeField] TextMeshProUGUI _currentlyInteractingWith;
        [SerializeField] CameraMode _cameraMode;

        void LateUpdate()
        {
            Transform cameraTransform = Camera.main.transform;

            switch (_cameraMode)
            {
                case CameraMode.LookAt:
                    transform.LookAt(cameraTransform);
                    break;
                case CameraMode.LookAtInverted:
                    Vector3 dirFromCamera = transform.position - cameraTransform.position;

                    transform.LookAt(transform.position + dirFromCamera);
                    break;
                case CameraMode.CameraForward:
                    transform.forward = cameraTransform.forward;
                    break;
                case CameraMode.CameraForwardInverted:
                    transform.forward = -cameraTransform.forward;
                    break;
            }
        }

        public void SetInteractingWith(string interactionName)
        {
            _interactablePanel.SetActive(!string.IsNullOrEmpty(interactionName));
            _currentlyInteractingWith.text = interactionName;
        }
    }
}