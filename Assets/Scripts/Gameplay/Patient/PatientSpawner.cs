using Unity.Netcode;
using UnityEngine.AI;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CodeBlue
{
    public enum PatientState
    {
        Queue,
        Waiting,
        CheckedIn,
        Diagnosed,
        Treated,
        LostPatience,
        Leave
    }

    public class PatientSpawner : SingletonBehaviour<PatientSpawner>
    {
        [field: SerializeField] public List<GameObject> Patients = new();
        [SerializeField] private BoxCollider _patientSpawnArea;
        [SerializeField] private GameObject _patientPrefab;
        public bool _isAllowedToSpawn;

        private string[] _firstNames =
        {
            "John", "Sarah", "Alex", "Emma", "Michael", "Olivia", "James", "Sophia", "Liam", "Isabella",
            "Benjamin", "Ava", "Lucas", "Mia", "Elijah", "Amelia", "William", "Evelyn", "Jack", "Harper",
            "Daniel", "Ella", "Matthew", "Chloe", "Henry", "Grace", "Joseph", "Lily", "David", "Zoe"
        };
        private string[] _lastNames =
        {
            "Smith", "Johnson", "Williams", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor", "Anderson",
            "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson", "Garcia", "Martinez", "Roberts", "Lopez",
            "Walker", "Allen", "Young", "King", "Wright", "Scott", "Adams", "Baker", "Gonzalez", "Nelson", "Carter"
        };
        private float _patientSpawnTimer;
        private Vector3 _minSpawnArea, _maxSpawnArea;
        private float _xSpawnArea, _zSpawnArea;
        private LevelData _levelData;
        private PatientQueue _patientQueue;
        private float _patientSpawnTimeInterval, _patientSpawnTime;

        private void Start()
        {
            _levelData = LevelData.Instance;
            _minSpawnArea = _patientSpawnArea.bounds.min;
            _maxSpawnArea = _patientSpawnArea.bounds.max;
            _patientQueue = GetComponent<PatientQueue>();

            CvarRegistry.RegisterCommands(this);
        }

        [Rpc(SendTo.Server)]
        public void Sv_StartSpawningRpc()
        {
            ResetPatientSpawner();
            _isAllowedToSpawn = true;
        }

        private void ResetPatientSpawner()
        {
            Patients.Clear();
            _patientSpawnTimeInterval = DayNightCycle.Instance.DayDuration / _levelData.MaxPatientCount * 0.5f;
            _patientSpawnTime = 2.5f;
        }

        [Rpc(SendTo.Server)]
        public void Sv_StopSpawningRpc()
        {
            _isAllowedToSpawn = false;
        }

        private void Update()
        {
            if (!IsServer) return;

            if (!_isAllowedToSpawn) return;

            if (Patients.Count < _levelData.MaxPatientCount && PatientQueue.Instance.HasAvailableSlot())
            {
                _patientSpawnTimer += Time.deltaTime;

                if (_patientSpawnTimer >= _patientSpawnTime)
                {
                    _xSpawnArea = Random.Range(_minSpawnArea.x, _maxSpawnArea.x);
                    _zSpawnArea = Random.Range(_minSpawnArea.z, _maxSpawnArea.z);

                    var pos = new Vector3(_xSpawnArea, _patientSpawnArea.transform.position.y, _zSpawnArea);

                    List<int> medicalCaseIDs = LevelData.Instance.MedicalCases.Select(medicalCase => medicalCase.DataID).ToList();
                    Sv_SpawnPatientRpc(pos, medicalCaseIDs.SelectRandom(), Patients.Count + 1);

                    _patientSpawnTimer = 0;
                    _patientSpawnTime = Random.Range(
                        _patientSpawnTimeInterval * _levelData.SpawnIntervalVariance.x,
                        _patientSpawnTimeInterval * _levelData.SpawnIntervalVariance.y
                    );
                }
            }
        }

        [Rpc(SendTo.Server)]
        private void Sv_SpawnPatientRpc(Vector3 pos, int medicalCaseID, int count)
        {
            var patientClone = NetworkObjectPool.Singleton.GetNetworkObject(PrefabDatabase.Instance.CommonPrefabs[PrefabType.Patient], pos, Quaternion.identity);
            patientClone.GetComponent<NavMeshAgent>().enabled = true;

            patientClone.GetComponent<NetworkObject>().Spawn();
            patientClone.GetComponent<PatientData>().Cl_SetMedicalCaseRpc(medicalCaseID);
            patientClone.GetComponent<PatientData>().UpdateTreatmentPlan();
            patientClone.GetComponent<PatientData>().ID.Value = count;
            patientClone.name = GenerateRandomName();

            Patients.Add(patientClone.gameObject);
            _patientQueue.QueuePatient(patientClone.transform);
        }

        private string GenerateRandomName()
        {
            string firstName = _firstNames[Random.Range(0, _firstNames.Length)];
            string lastName = _lastNames[Random.Range(0, _lastNames.Length)];
            return $"{firstName} {lastName}";
        }

        public Vector3 GetPatientSpawnAreaPosition()
        {
            return _patientSpawnArea.transform.position;
        }

        [ConFunc("forceclose", "kick out all patient")]
        [Rpc(SendTo.Server)]
        public void Sv_AllPatientsLeaveRpc()
        {
            foreach (var patient in Patients)
            {
                patient.GetComponent<PatientMovement>().GoToSpawn();
                patient.GetComponent<PatientPatience>().Sv_ResetPatienceRpc();
                patient.GetComponent<PatientBehaviour>().Cl_SetPatientStateRpc(PatientState.LostPatience);
            }
        }

        [ConFunc("treat_all", "Treat all patients")]
        [Rpc(SendTo.Server)]
        void _TreatAllRpc()
        {
            foreach (var patient in Patients)
            {
                patient.GetComponent<PatientBehaviour>().ChangeState(new PatientTreatedState());
                _isAllowedToSpawn = false;
            }
            GameManager.Instance.PatientsTreated.Value = LevelData.Instance.MaxPatientCount;
        }
    }
}
