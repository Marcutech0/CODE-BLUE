using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class Loader : NetworkBehaviour
    {
        Slider _slider;

        private bool _isLoading;

        // NOTE: something like plateup
        private float _speedMultiplier = 1f;
        private float _loadTimer;
        private NetworkVariable<float> _sliderValue = new();

        void Start()
        {
            _slider = GetComponent<Slider>();
        }

        // Update is called once per frame
        void Update()
        {            
            if (!IsServer) return;
            
            if (_isLoading)
                _loadTimer += Time.deltaTime;
            else
                _loadTimer = 0f;

            _sliderValue.Value = _slider.maxValue * _loadTimer / _slider.maxValue * _speedMultiplier;
            Cl_UpdateLoadingBarRpc();

            if (_loadTimer < _slider.maxValue) return;
            
            UIManager.Instance.CardSelectionScreen.OnVoteComplete();
            _isLoading = false;
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_UpdateLoadingBarRpc()
        {
            _slider.value = _sliderValue.Value;
        }

        public void StartLoadingTimer(float endValueSeconds = 5f, float multiplier = 1f)
        {
            _loadTimer = 0f;
            _slider.maxValue = endValueSeconds;
            _speedMultiplier = multiplier;
            _isLoading = true;
        }

        public void ResetLoadingTimer()
        {
            _isLoading = false;
        }
    }
}