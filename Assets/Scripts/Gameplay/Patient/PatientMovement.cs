using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBlue
{
    public class PatientMovement : NetworkBehaviour
    {
        [SerializeField] private NavMeshAgent _patient;

        public bool IsMoving { private set; get; }

        public void WalkTo(Vector3 position)
        {
            IsMoving = true;
            _patient.SetDestination(position);
        }

        public void GoToSpawn()
        {
            var pos = PatientSpawner.Instance.GetPatientSpawnAreaPosition();
            WalkTo(pos);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Bench") && !other.CompareTag("Hospital Bed")) return;

            IsMoving = false;
        }
    }
}