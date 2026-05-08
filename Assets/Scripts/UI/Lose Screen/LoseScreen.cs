using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class LoseScreen : UIScreenBase
    {
        [SerializeField] private TextMeshProUGUI _treatedText;
        [SerializeField] private TextMeshProUGUI _summaryText;

        [Header("Components")]
        [SerializeField] private Button _restart;

        void OnEnable()
        {
            _restart.onClick.AddListener(() => GameManager.Instance.Cl_ChangeGameStateRpc(GamePhase.Lobby));
        }

        void OnDisable()
        {
            _restart.onClick.RemoveAllListeners();
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_ShowLoseRpc()
        {
            int treated = 0;
            int nonUrgSev = 0, semiUrgSev = 0, critSev = 0;

            foreach (var tran in SharedEconomy.Instance.Transactions)
            {
                if (tran.Type != TransactionType.Treatment) continue;

                treated++;
                if (tran.Details == (int)TriageLevel.Critical) critSev++;
                else if (tran.Details == (int)TriageLevel.SemiUrgent) semiUrgSev++;
                else if (tran.Details == (int)TriageLevel.NonUrgent) nonUrgSev++;
            }

            _treatedText.text = $"You have treated {treated} patient" + (treated > 1 ? "s" : "");
            _summaryText.text = $"{critSev} critical\n{semiUrgSev} semi-urgent\n{nonUrgSev} non-urgent";
        }
    }
}
