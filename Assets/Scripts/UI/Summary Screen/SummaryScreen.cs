using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class SummaryScreen : UIScreenBase
    {
        [Header("Components")]
        [SerializeField] TextMeshProUGUI _patientsTreatedText;
        [SerializeField] TextMeshProUGUI _triageCountText;
        [SerializeField] TextMeshProUGUI _expendituresText;
        [SerializeField] TextMeshProUGUI _totalText;
        [SerializeField] Button _okButton;

        void Start()
        {
            _okButton.onClick.AddListener(
                    () => GameManager.Instance.Cl_ChangeGameStateRpc(GamePhase.Card)
            );
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_ShowSummaryRpc()
        {
            UIManager.Instance.Cl_LoadScreenRpc(UIScreenType.Summary);

            // i would use GroupBy,,, but this is fine for now
            float treatmentTotal = 0, expendituresTotal = 0;
            float penalties = 0;

            int nonUrgSev = 0, semiUrgSev = 0, critSev = 0;

            foreach (var tran in SharedEconomy.Instance.Transactions)
            {
                if (tran.Type == TransactionType.Treatment)
                {
                    if (tran.Details == (int)TriageLevel.Critical) critSev++;
                    else if (tran.Details == (int)TriageLevel.SemiUrgent) semiUrgSev++;
                    else if (tran.Details == (int)TriageLevel.NonUrgent) nonUrgSev++;

                    treatmentTotal += tran.Amount;
                }
                else if (tran.Type == TransactionType.Expenditure) expendituresTotal += tran.Amount;
                else if (tran.Type == TransactionType.Penalty) penalties += tran.Amount;
            }

            _patientsTreatedText.text = $"${treatmentTotal:F2}";
            _triageCountText.text = $"> Non-Urgent\t\t\t<b>{nonUrgSev}</b>\n> Semi-Urgent\t\t\t<b>{semiUrgSev}</b>\n> Non-Urgent\t\t\t<b>{nonUrgSev}</b>";
            _expendituresText.text = $"${expendituresTotal:F2}";

            var total = treatmentTotal + expendituresTotal - penalties;
            _totalText.text = $"${total:F2}";
        }
    }
}
