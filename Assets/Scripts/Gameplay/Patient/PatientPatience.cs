using AYellowpaper.SerializedCollections;
using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public enum CameraMode
    {
        LookAt,
        LookAtInverted,
        CameraForward,
        CameraForwardInverted
    }

    public class PatientPatience : NetworkBehaviour
    {
        [Header("Patience Bar")]
        [SerializeField] private Transform _patienceBar;
        [SerializeField] private Transform _patienceBarFillHolder;
        [SerializeField] private GameObject _patienceBarFill;
        [SerializeField] private CameraMode _cameraMode;

        private float _patience, _patienceTimer;
        private bool _patienceReset, _hasSetPatienceBar;

        private NetworkVariable<float> _patienceBarFillHolderSize = new();

        [Rpc(SendTo.Server)]
        public void Sv_SetPatienceBarRpc(TriageLevel triageLevel, Color triageLevelColor, float patience, bool reset)
        {
            _patience = patience;
            _patienceReset = reset;
            _hasSetPatienceBar = true;
            Cl_SetPatienceBarFillRpc(triageLevelColor);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_SetPatienceBarFillRpc(Color triageLevelColor)
        {
            _patienceBarFill.GetComponent<Renderer>().materials[0].color = triageLevelColor;
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_SetPatienceRpc()
        {
            if (_patienceBar.gameObject.activeInHierarchy) return;

            _patienceBar.gameObject.SetActive(true);
        }

        [Rpc(SendTo.Server)]
        public void Sv_ResetPatienceRpc()
        {
            Cl_ResetPatienceRpc();

            if (!_patienceReset) return;
            _patienceTimer = 0;
            _patienceBarFillHolderSize.Value = 1f;
            Cl_UpdatePatienceBarRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_ResetPatienceRpc()
        {
            _patienceBar.gameObject.SetActive(false);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_UpdatePatienceBarRpc()
        {
            _patienceBarFillHolder.localScale = new Vector3(_patienceBarFillHolderSize.Value, 1f, 1f);
        }

        void Update()
        {
            if (!_hasSetPatienceBar) return;

            if (!_patienceBar.gameObject.activeInHierarchy) return;

            if (IsServer)
            {
                var state = GetComponent<PatientBehaviour>().State;

                if (state == PatientState.Waiting ||
                state == PatientState.CheckedIn ||
                state == PatientState.Diagnosed)
                    _patienceTimer += Time.deltaTime;
                else
                    _patienceTimer = 0;

                _patienceBarFillHolderSize.Value = Mathf.Clamp01(1 - (_patienceTimer / _patience));
                Cl_UpdatePatienceBarRpc();

                if (_patienceTimer >= _patience)
                {
                    Cl_ResetPatienceRpc();
                    GetComponent<PatientBehaviour>().ChangeState(new PatientLostPatienceState());
                }
            }
        }

        void LateUpdate()
        {
            Transform cameraTransform = Camera.main.transform;

            switch (_cameraMode)
            {
                case CameraMode.LookAt:
                    _patienceBar.LookAt(cameraTransform);
                    break;
                case CameraMode.LookAtInverted:
                    Vector3 dirFromCamera = _patienceBar.position - cameraTransform.position;

                    _patienceBar.LookAt(transform.position + dirFromCamera);
                    break;
                case CameraMode.CameraForward:
                    _patienceBar.forward = cameraTransform.forward;
                    break;
                case CameraMode.CameraForwardInverted:
                    _patienceBar.forward = -cameraTransform.forward;
                    break;
            }
        }
    }
}
