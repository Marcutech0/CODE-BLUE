using System;
using UnityEngine;

namespace CodeBlue 
{
    public class HideOnPhase : MonoBehaviour
    {
        [SerializeField] Renderer[] _disableRendWhenNotPhase;
        [SerializeField] Collider[] _disableColsWhenNotPhase;

        [SerializeField] GamePhase _phase;

        private void Start()
        {
            GameManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
        }

        private void OnPhaseChanged(GamePhase phase)
        {
            var isPhase = _phase.HasFlag(phase);
            foreach (var b in _disableColsWhenNotPhase)
            {
                b.enabled = isPhase;
            }

            foreach (var b in _disableRendWhenNotPhase)
            {
                b.enabled = isPhase;
            }
        }
    }
}
