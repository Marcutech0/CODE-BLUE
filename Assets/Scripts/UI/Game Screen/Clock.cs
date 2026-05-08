using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace CodeBlue
{
    public class Clock : NetworkBehaviour
    {
        [SerializeField] private Image _clock;
        [SerializeField] private RectTransform _clockFillRTF;
        [SerializeField] private Image _clockFillIMG;
        [SerializeField] private TextMeshProUGUI _dayCount;
        private bool _hasUpdatedClock, _hasUpdatedClockFill;
        private DayNightPhase _dayNightPhase;

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_StartClockFillRpc()
        {
            _clockFillRTF.sizeDelta = new Vector2(250f, 250f);
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_UpdateClockRpc(float timeOfDay)
        {
            timeOfDay = Mathf.Floor(timeOfDay * 1000)/1000;
            var timesOfDay = DayNightCycle.Instance.TimesOfDay;

            if (!timesOfDay.Keys.Contains(timeOfDay)) return;
            _hasUpdatedClock = false;

            if (_hasUpdatedClock) return;
            _hasUpdatedClock = true;

            Color clockColor = timesOfDay.FirstOrDefault(pair => pair.Key == timeOfDay).Value;

            _clock.DOColor(clockColor, 0.125f);
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_UpdateClockFillRTFRpc(float timeOfDay)
        {
            Vector2 fillSize = Vector2.one;
            if (timeOfDay <= 0.25f)
                fillSize *= 250 - (250 * timeOfDay / 0.25f);
            else if (timeOfDay <= 0.5f)
                fillSize *= 250 * ((timeOfDay - 0.25f) / 0.25f);
            else if (timeOfDay <= 0.75f)
                fillSize *= 250 - (250 * (timeOfDay - 0.5f) / 0.25f);
            else if (timeOfDay <= 1f)
                fillSize *= 250 * ((timeOfDay - 0.75f) / 0.25f);

            _clockFillRTF.sizeDelta = fillSize;
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_UpdateClockFillIMGRpc(DayNightPhase dayNightPhase)
        {
            var dayNightPhases = DayNightCycle.Instance.DayNightPhases;

            if (_dayNightPhase == dayNightPhase) return;
            _dayNightPhase = dayNightPhase;
            _hasUpdatedClockFill = false;

            if (_hasUpdatedClockFill) return;
            _hasUpdatedClockFill = true;

            Color fillColor = dayNightPhases.FirstOrDefault(pair => pair.Key == dayNightPhase).Value;

            _clockFillIMG.color = fillColor;
        }
        
        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_UpdateDayCountRpc(int dayCount)
        {
            _dayCount.text = $"Day {dayCount}";
        }
    }
}