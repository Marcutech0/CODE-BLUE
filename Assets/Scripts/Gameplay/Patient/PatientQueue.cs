using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public class PatientQueue : SingletonBehaviour<PatientQueue>
    {
        [SerializeField] private Transform _standingQueue;
        [SerializeField] private List<Transform> _benches = new List<Transform>();
        [SerializeField] private PatientChair[] _consultRoomChairs;

        [SerializeField, SerializedDictionary("Bench Queue", "Patient")] private SerializedDictionary<Transform, Transform> _patientQueue = new();

        public override void OnNetworkSpawn()
        {
            CreatePatientQueue();
            CvarRegistry.RegisterCommands(this);
        }

        public bool HasAvailableSlot() => _patientQueue.Values.Any(p => p == null);
        public bool IsAnyChairOccupied() => _consultRoomChairs.Any(c => c.PatientOwnerId.Value != 99);

        public void CreatePatientQueue()
        {
            _patientQueue.Clear();

            foreach (Transform bench in _benches)
            {
                foreach (Transform marker in bench.GetComponentsInChildren<Transform>().Where(marker => marker.name.Contains("Bench Marker")))
                {
                    if (!marker.gameObject.activeInHierarchy) continue;
                    _patientQueue.Add(marker, null);
                }
            }

            foreach (Transform marker in _standingQueue.GetComponentsInChildren<Transform>().Skip(1))
            {
                _patientQueue.Add(marker, null);
            }
        }

        // this runs on the server only, but no need to make it rpc
        public void ClearChairs()
        {
            foreach (var chair in _consultRoomChairs)
                chair.PatientOwnerId.Value = 99;
        }

        [Rpc(SendTo.Server)]
        [ConFunc("check_in", "Checks-in the patient at the top of the queue")]
        public void Sv_CheckInPatientRpc()
        {
            var top = _patientQueue.First().Value;
            if (top.TryGetComponent(out PatientBehaviour patient))
            {
                var chair = _consultRoomChairs.Where(c => c.PatientOwnerId.Value == 0).FirstOrDefault();

                if (!chair)
                {
                    Debug.LogError("No chair!");
                    return;
                }

                if (patient.GetComponent<PatientMovement>().IsMoving) return;

                var state = new PatientCheckedInState();
                state.SetChair(chair);
                patient.ChangeState(state);
                DequeuePatient(0);
                ShiftPatientBenches();
            }
        }

        void DequeuePatient(int startingIndex)
        {
            for (int i = startingIndex; i < _patientQueue.Count; i++)
            {
                var curr = _patientQueue.ElementAtOrDefault(i);
                var next = _patientQueue.ElementAtOrDefault(i + 1);

                _patientQueue[curr.Key] = next.Value ?? null;
            }
        }

        public void ShiftPatientBenches()
        {
            foreach (var e in _patientQueue)
                e.Value?.GetComponent<PatientMovement>().WalkTo(e.Key.position);
        }

        public void QueuePatient(Transform patient)
        {
            if (_patientQueue.ContainsValue(null))
            {
                foreach (Transform marker in _patientQueue.Keys)
                {
                    if (_patientQueue[marker] == null && marker.gameObject.activeInHierarchy)
                    {
                        _patientQueue[marker] = patient;
                        var state = new PatientQueueState();
                        state.SetMarker(marker.position);
                        patient.GetComponent<PatientBehaviour>().ChangeState(state);

                        return;
                    }
                }
            }
        }

        [ConFunc("dequeue_all", "kick out all patients")]
        [Rpc(SendTo.Server)]
        public void Sv_AllPatientsInQueueLeaveRpc()
        {
            foreach (var patient in _patientQueue.Values)
            {
                patient.GetComponent<PatientMovement>().GoToSpawn();
                patient.GetComponent<PatientPatience>().Sv_ResetPatienceRpc();
                patient.GetComponent<PatientBehaviour>().Cl_SetPatientStateRpc(PatientState.Leave);
            }
        }

        public int PatientsLeft() => _patientQueue.Count((p) => p.Value != null);
    }
}
