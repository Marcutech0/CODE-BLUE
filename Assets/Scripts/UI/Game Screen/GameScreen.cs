using UnityEngine;
using TMPro;
using DG.Tweening;
using Unity.Netcode;

namespace CodeBlue
{
    public class GameScreen : UIScreenBase
    {
        [field: Header("Functionality")]
        [field: SerializeField] public Clock Clock { private set; get; }
        [field: SerializeField] public ItemIndicator ItemIndicator { private set; get; }
        [field: SerializeField] TextMeshProUGUI _phaseText;
        [field: SerializeField] TextMeshProUGUI _salaryText;

        private void Update()
        {
            _phaseText.text = GameManager.Instance.CurrentPhaseEnum switch
            {
                GamePhase.Prep => "Preparation",
                GamePhase.Work => "Shift started!",
                _ => ""
            };
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_UpdateSalaryTextRpc(float newSalary)
        {
            _salaryText.transform.DOPunchScale(Vector3.one * 1.1f, 0.1f);
            _salaryText.text = $"${Mathf.RoundToInt(newSalary)}";
        }
    }
}
