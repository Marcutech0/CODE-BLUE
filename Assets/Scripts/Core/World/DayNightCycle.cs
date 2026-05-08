using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.Events;
using AYellowpaper.SerializedCollections;

namespace CodeBlue
{
    public enum DayNightPhase
    {
        Day,
        Night
    }

    public class DayNightCycle : SingletonBehaviour<DayNightCycle>
    {
        [Header("Booleans")]
        private bool _isDayOver;

        [Header("Skybox Materials")]
        [SerializeField] private Material _morningSkybox;
        [SerializeField] private Material _afternoonSkybox;
        [SerializeField] private Material _nightSkybox;

        [Header("Directional Light Settings")]
        [SerializeField] private Light _directionalLight;
        [SerializeField] private Gradient _lightColorTransitions;
        [SerializeField] private AnimationCurve _lightIntensityCurve;

        [Header("Cycle Settings")]
        [field: SerializeField] public float DayDuration { get; private set; } = 60f * 5f; // 5 minutes per full cycle
        [SerializeField, Range(0.1f, 10f)] private float _timeMultiplier = 1f; // Speed slider

        [Header("UI Display")]
        [SerializeField] private float _timeOfDay = 0f;
        [SerializeField] private int _dayCount = 1;

        [field: SerializeField, SerializedDictionary("Time of Day", "Color")]
        public SerializedDictionary<float, Color> TimesOfDay = new()
        {
            { 0f, new Color() },
            { 0.125f, new Color() },
            { 0.25f, new Color() },
            { 0.375f, new Color() },
            { 0.5f, new Color() },
            { 0.625f, new Color() },
            { 0.75f, new Color() },
            { 0.875f, new Color() },
            { 1f, new Color() },
        };

        [field: SerializeField, SerializedDictionary("Day Night Phase", "Color")]
        public SerializedDictionary<DayNightPhase, Color> DayNightPhases = new()
        {
            { DayNightPhase.Day, new Color() },
            { DayNightPhase.Night, new Color() }
        };

        [SerializeField] private DayNightPhase _dayNightPhase;

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
                return;

            if (_morningSkybox == null || _afternoonSkybox == null || _nightSkybox == null)
            {
                Debug.LogError("Please assign all skybox materials in the Inspector.");
                return;
            }

            if (_directionalLight == null)
            {
                Debug.LogError("No directional light assigned! Drag a directional light to the script.");
                return;
            }

            DynamicGI.UpdateEnvironment();
        }

        [Rpc(SendTo.Server)]
        public void Sv_StartDayRpc()
        {
            _timeOfDay = 0;
            _isDayOver = false;
            UIManager.Instance.GameScreen.Clock.Cl_UpdateDayCountRpc(_dayCount);
        }

        private void Update()
        {
            if (!IsServer) return;

            if (!GameManager.Instance.IsPhase(GamePhase.Work)) return;

            if (!_isDayOver)
            {
                _timeOfDay += Time.deltaTime / DayDuration * _timeMultiplier;

                if (_timeOfDay <= 0.25f || _timeOfDay > 0.75f)
                    _dayNightPhase = DayNightPhase.Night;
                else if (_timeOfDay > 0.25f && _timeOfDay <= 0.75f)
                    _dayNightPhase = DayNightPhase.Day;

                SetClockAndSkyBox();

                if (_timeOfDay < 1f) return;
                EndDay();
            }
        }

        private void SetClockAndSkyBox()
        {
            UIManager.Instance.GameScreen.Clock.Cl_UpdateClockRpc(_timeOfDay);
            UIManager.Instance.GameScreen.Clock.Cl_UpdateClockFillRTFRpc(_timeOfDay);
            UIManager.Instance.GameScreen.Clock.Cl_UpdateClockFillIMGRpc(_dayNightPhase);

            Cl_UpdateSkyboxRpc(_timeOfDay);
            Cl_UpdateLightingRpc(_timeOfDay);
        }

        [Rpc(SendTo.Server)]
        public void Sv_EndDayRpc()
        {
            EndDay();
        }

        private void EndDay()
        {
            _timeOfDay = 0f;
            _dayCount++;
            _isDayOver = true;
            GameManager.Instance.Cl_ChangeGameStateRpc(GamePhase.EndOfDay);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_UpdateSkyboxRpc(float timeOfDay)
        {
            if (timeOfDay > 0.25f && timeOfDay <= 0.5f)
                RenderSettings.skybox = _morningSkybox;
            else if (timeOfDay > 0.5f && timeOfDay <= 0.75f)
                RenderSettings.skybox = _afternoonSkybox;
            else if (timeOfDay <= 0.25f || timeOfDay > 0.75f)
                RenderSettings.skybox = _nightSkybox;

            DynamicGI.UpdateEnvironment();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_UpdateLightingRpc(float timeOfDay)
        {
            float normalizedTime = timeOfDay; // Keeps time in 0-1 range

            // Smooth color transition using gradient
            _directionalLight.color = _lightColorTransitions.Evaluate(normalizedTime);

            // Smooth intensity transition using curve
            _directionalLight.intensity = _lightIntensityCurve.Evaluate(normalizedTime);

            var xRot = Mathf.Lerp(160, 20, normalizedTime);
            _directionalLight.transform.rotation = Quaternion.Euler(new Vector3(xRot, 0f, 0f));
        }

        public int GetDayCount()
        {
            return _dayCount;
        }
    }
}
