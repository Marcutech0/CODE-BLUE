using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public class PatientDespawner : NetworkBehaviour
    {
        [SerializeField] private float _despawnDelay;

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Patient Spawn Area")) return;

            var state = GetComponent<PatientBehaviour>().State;

            if (state != PatientState.LostPatience && state != PatientState.Treated) return;

            StartCoroutine(StartDespawn(gameObject));
        }

        IEnumerator StartDespawn(GameObject patient)
        {
            yield return new WaitForSeconds(_despawnDelay);
            patient.GetComponent<PatientBehaviour>().Cl_SetPatientStateRpc(PatientState.Queue);
            patient.GetComponent<NetworkObject>().Despawn();
        }
    }
}