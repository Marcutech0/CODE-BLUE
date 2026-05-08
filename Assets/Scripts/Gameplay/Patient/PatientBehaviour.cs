using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CodeBlue
{
    public class PatientBehaviour : BaseInteractable
    {
        private StateMachine<PatientBehaviour> _states = new();
        [field: SerializeField] public PatientState State { private set; get; }
        [field: SerializeField] public bool IsCheckedIn { private set; get; }
        [field: SerializeField] public bool IsDiagnosed { private set; get; }

        private readonly Dictionary<System.Type, PatientState> _stateMapping = new() {
            { typeof(PatientQueueState), PatientState.Queue },
            { typeof(PatientWaitingState), PatientState.Waiting },
            { typeof(PatientCheckedInState), PatientState.CheckedIn },
            { typeof(PatientDiagnosedState), PatientState.Diagnosed},
            { typeof(PatientTreatedState), PatientState.Treated },
            { typeof(PatientLostPatienceState), PatientState.LostPatience },
            { typeof(PatientLeaveState), PatientState.Leave},
        };

        [Header("References")]
        [SerializeField] PatientData _data;
        public PatientChair OwnedChair { get; set; }

        void Update()
        {
            if (!IsServer) return;

            _states.UpdateState(this);
        }

        public void ChangeState(IState<PatientBehaviour> state)
        {
            _states.SetState(state, this);
            Cl_SetPatientStateRpc(_stateMapping[state.GetType()]);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_SetPatientStateRpc(PatientState state)
        {
            State = state;

            if (!IsServer) return;
            Sv_UpdateBooleansRpc(state);
        }

        [Rpc(SendTo.Server)]
        private void Sv_UpdateBooleansRpc(PatientState state)
        {
            IsCheckedIn = !IsCheckedIn && state == PatientState.CheckedIn;
            IsDiagnosed = !IsDiagnosed && state == PatientState.CheckedIn;
        }

        public override void Interact(GameObject interactedBy)
        {
<<<<<<< HEAD
            
            /*if (IsRestricted(interactedBy)) return;*/

            if (_states.CurrentState is PatientCheckedInState
                && interactedBy.GetComponent<PlayerRole>().Role == PlayerRoles.Doctor)
=======
            Sv_CheckInteractionRpc(NetworkManager.LocalClientId);
        }

        [Rpc(SendTo.Server)]
        void Sv_CheckInteractionRpc(ulong playerId, RpcParams rpcParams = default)
        {
            var currState = State;
            if (currState is PatientState.CheckedIn)
>>>>>>> beta
                ChangeState(new PatientDiagnosedState());

            if (currState is PatientState.Diagnosed)
                Cl_TreatRpc(playerId, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void Cl_TreatRpc(ulong playerId, RpcParams rpcParams)
        {
            var interactedBy = NetworkManager.ConnectedClients[playerId].PlayerObject;

            if (!interactedBy.TryGetComponent(out Interaction inter)) return;
            var supply = inter.CarriedItem;
            if (!supply) return;

            var item = supply.GetComponent<MedicalSupply>().Data;
            if (_data.TryTreatment(item.ID))
                inter.DropItem(transform.position, destroyItem: true);
        }
    }

    public class PatientLeaveState : IState<PatientBehaviour>
    {
        public void OnEnter(PatientBehaviour owner)
        {
            owner.GetComponent<PatientMovement>().GoToSpawn();
        }

        public void OnExecute(PatientBehaviour owner)
        {
        }

        public void OnExit(PatientBehaviour owner)
        {
        }
    }


    public class PatientQueueState : IState<PatientBehaviour>
    {
        Vector3 _markerPos;
        public PatientQueueState() { }
        public void SetMarker(Vector3 marker)
        {
            _markerPos = marker;
        }

        public void OnEnter(PatientBehaviour owner)
        {
            owner.GetComponent<PatientMovement>().WalkTo(_markerPos);
        }

        public void OnExecute(PatientBehaviour owner)
        {
            if (owner.GetComponent<PatientMovement>().IsMoving) return;

            owner.ChangeState(new PatientWaitingState());
        }

        public void OnExit(PatientBehaviour owner)
        {
            MedicalCaseData medicalCase = owner.GetComponent<PatientData>().MedicalCase;
            owner.GetComponent<PatientPatience>().Sv_SetPatienceBarRpc(
                medicalCase.TriageLevel,
                medicalCase.TriageLevelColor,
                medicalCase.PatienceDuration,
                medicalCase.PatienceReset
            );
        }
    }

    public class PatientWaitingState : IState<PatientBehaviour>
    {
        public PatientWaitingState() { }

        public void OnEnter(PatientBehaviour owner)
        {
            if (owner.State == PatientState.LostPatience) return;

            owner.GetComponent<PatientPatience>().Cl_SetPatienceRpc();
        }

        public void OnExecute(PatientBehaviour owner) { }

        public void OnExit(PatientBehaviour owner)
        {
            owner.GetComponent<PatientPatience>().Sv_ResetPatienceRpc();
        }
    }

    public class PatientCheckedInState : IState<PatientBehaviour>
    {
        PatientChair _conRmChair;
        public PatientCheckedInState()
        {
        }

        public void SetChair(PatientChair chair)
        {
            _conRmChair = chair;
        }

        public void OnEnter(PatientBehaviour owner)
        {
            owner.GetComponent<PatientMovement>().WalkTo(_conRmChair.transform.position);
            _conRmChair.Sv_ClaimChairRpc(owner.NetworkObjectId);
            owner.OwnedChair = _conRmChair;
        }

        public void OnExecute(PatientBehaviour owner)
        {
            if (owner.GetComponent<PatientMovement>().IsMoving) return;

            owner.GetComponent<PatientPatience>().Cl_SetPatienceRpc();
        }

        public void OnExit(PatientBehaviour owner)
        {
            owner.GetComponent<PatientPatience>().Sv_ResetPatienceRpc();
        }

    }

    public class PatientDiagnosedState : IState<PatientBehaviour>
    {
        PatientData _data;
        public PatientDiagnosedState() { }

        public void OnEnter(PatientBehaviour owner)
        {
            _data = owner.GetComponent<PatientData>();
            var medCase = _data.MedicalCase;
            var _treatmentsLeft = medCase.TreatmentPlan.ToList();

            if (medCase.RequiredDeparment != Department.ConsultationRoom)
            {
                var deptChair = WorldDepartments.Instance.Departments[medCase.RequiredDeparment].GetChairInDepartment();
                owner.GetComponent<PatientMovement>().WalkTo(deptChair.transform.position);
                deptChair.Sv_ClaimChairRpc(owner.NetworkObjectId);
                owner.OwnedChair = deptChair;
            }

            owner.GetComponent<PatientDiagnosisUI>().Cl_UpdateTreatmentPlanUIRpc(medCase.ID, _treatmentsLeft.ToArray());
            owner.GetComponent<PatientPatience>().Cl_SetPatienceRpc();

            AudioManager.Instance.Cl_PlayGameSfxRpc("voice_" + Random.Range(1, 7));
        }

        public void OnExecute(PatientBehaviour owner)
        {
            if (!owner.IsServer) return;

            if (_data.TreatmentsLeft.Count == 0)
            {
                owner.ChangeState(new PatientTreatedState());
            }
        }

        public void OnExit(PatientBehaviour owner)
        {
            owner.GetComponent<PatientDiagnosisUI>().Cl_CloseRpc();
        }

    }

    public class PatientTreatedState : IState<PatientBehaviour>
    {
        public PatientTreatedState() { }
        public void OnEnter(PatientBehaviour owner)
        {
            owner.GetComponent<PatientMovement>().GoToSpawn();

            // unclaim the chair
            owner.OwnedChair?.Sv_ClaimChairRpc(NetworkManager.ServerClientId);

            var triageLevel = owner.GetComponent<PatientData>().MedicalCase.TriageLevel;
            float salary = triageLevel switch
            {
                TriageLevel.NonUrgent => 100,
                TriageLevel.SemiUrgent => 200,
                TriageLevel.Critical => 500,
                _ => 0f,
            };

            SharedEconomy.Instance.Sv_AddSalaryRpc(salary, (int)triageLevel);
            GameManager.Instance.Sv_PatientTreatedRpc();
            owner.GetComponent<PatientPatience>().Cl_ResetPatienceRpc();

            AudioManager.Instance.Cl_PlayGameSfxRpc("voice_happy_" + Random.Range(1, 5));
        }

        public void OnExecute(PatientBehaviour owner) { }

        public void OnExit(PatientBehaviour owner) { }

    }

    public class PatientLostPatienceState : IState<PatientBehaviour>
    {
        public PatientLostPatienceState() { }

        public void OnEnter(PatientBehaviour owner)
        {
            GameManager.Instance.Cl_ChangeGameStateRpc(GamePhase.Lose);
        }

        public void OnExecute(PatientBehaviour owner) { }

        public void OnExit(PatientBehaviour owner) { }
    }
}
