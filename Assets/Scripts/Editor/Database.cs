using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CodeBlue
{
    public enum DatabaseType
    {
        DepartmentDatabase,
        GameEventDatabase,
        MedicalCaseDatabase,
        MedicalSupplyDatabase
    }

    public class Database : EditorWindow
    {
        Action<DatabaseType> _onConfirm;
        DatabaseType _database;

        static void ShowWindow(Action<DatabaseType> onConfirm)
        {
            Database window = GetWindow<Database>("Rebuild Game Database");
            window._onConfirm = onConfirm;
            window.minSize = new(300, 50);
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Database", EditorStyles.boldLabel);
            _database = (DatabaseType) EditorGUILayout.EnumPopup("Database Type", _database);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Yuh")) _onConfirm.Invoke(_database);
            else if (GUILayout.Button("nuh uh")) Close();
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
        }

        [MenuItem("Tools/Code Blue/Rebuild Game Database")]
        public static void ShowRebuildWindow()
        {
            ShowWindow(onConfirm: CreateDatabase);
        }


        /* Only use when updating the Database */
        #region Create Database
        private static void CreateDatabase(DatabaseType database)
        {
            Dictionary<DatabaseType, string> databases = new()
            {
                { DatabaseType.DepartmentDatabase, "Departments" },
                { DatabaseType.GameEventDatabase, "Game Events" },
                { DatabaseType.MedicalCaseDatabase, "Medical Cases" },
                { DatabaseType.MedicalSupplyDatabase, "Medical Supplies" },
            };

            string _csvFilePath = Application.dataPath + $"/Database/{database}.csv";
            string _dataFilePath = $"Assets/Data/{databases[database]}/";

            if (!File.Exists(_csvFilePath)) return;

            using StreamReader reader = new(_csvFilePath);

            reader.ReadLine();

            while (!reader.EndOfStream)
            {
                string[] values = reader.ReadLine().Split(',');

                CreateAsset(values: values, dataFilePath: _dataFilePath);
            }

            AssetDatabase.SaveAssets();
        }

        private static void CreateAsset(string[] values, string dataFilePath)
        {
            if (dataFilePath.Contains("Medical Supplies"))
            {
                MedicalSupplyData medicalSupplyData = ScriptableObject.CreateInstance<MedicalSupplyData>();
                medicalSupplyData.ID = int.Parse(values[0]);
                medicalSupplyData.Name = values[1];
                medicalSupplyData.Description = values[2].Replace("|", ",");
                medicalSupplyData.Location = (values[3] == "Pharmacy") ? MedicalSupplyLocation.Pharmacy : MedicalSupplyLocation.SupplyRoom;
                medicalSupplyData.Cost = float.Parse(values[4]);
                medicalSupplyData.IsShared = true;
                medicalSupplyData.name = medicalSupplyData.Name.Replace(" ", "");
                medicalSupplyData.UITexture = MedicalSupplyDatabase.Instance.GetSprite(medicalSupplyData.name);

                AssetDatabase.CreateAsset(medicalSupplyData, dataFilePath + $"{medicalSupplyData.name}.asset");
            }
            else if (dataFilePath.Contains("Events"))
            {
                GameEventData eventData = ScriptableObject.CreateInstance<GameEventData>();
                eventData.ID = int.Parse(values[0]);
                eventData.Name = values[1];
                eventData.Description = values[2].Replace("|", ",");
                eventData.DurationText = values[3];
                eventData.DurationDay = int.Parse(values[4]);
                eventData.SpawnRate = int.Parse(values[5]);
                eventData.name = eventData.Name.Replace(" ", "");

                AssetDatabase.CreateAsset(eventData, dataFilePath + $"{eventData.name}.asset");
            }
            else if (dataFilePath.Contains("Departments"))
            {
                DepartmentData deparmentData = ScriptableObject.CreateInstance<DepartmentData>();
                deparmentData.ID = int.Parse(values[0]);
                deparmentData.Name = values[1];
                deparmentData.Description = values[2];
                deparmentData.Department = Enum.Parse<Department>(values[1].Replace(" ", ""));
                deparmentData.name = deparmentData.Name.Replace(" ", "");

                AssetDatabase.CreateAsset(deparmentData, dataFilePath + $"{deparmentData.Name.Replace(" ", "")}.asset");
            }
            else if (dataFilePath.Contains("Medical Cases"))
            {
                MedicalCaseData medicalCaseData = ScriptableObject.CreateInstance<MedicalCaseData>();
                medicalCaseData.ID = int.Parse(values[0]);
                medicalCaseData.Name = values[1];
                medicalCaseData.RequiredDeparment = Enum.Parse<Department>(values[2].Replace(" ", ""));
                medicalCaseData.TriageLevel = Enum.Parse<TriageLevel>(values[3].Replace("-", ""));
                medicalCaseData.TriageLevelColor = MedicalCaseDatabase.Instance.TriageLevelColors[medicalCaseData.TriageLevel];
                medicalCaseData.PatienceDuration = float.Parse(values[4]);
                medicalCaseData.PatienceReset = bool.Parse(values[5]);
                medicalCaseData.SpawnRate = int.Parse(values[6]);
                medicalCaseData.WheelChairUsageRate = int.Parse(values[7]);
                medicalCaseData.FatalityRate = int.Parse(values[8]);

                string[] treatments = values[9].Split(" | ");
                medicalCaseData.TreatmentPlan = new int[treatments.Length];
                for (int i = 0; i < treatments.Length; i++)
                {
                    medicalCaseData.TreatmentPlan[i] = MedicalSupplyDatabase.Instance.GetDataID(treatments[i].Trim());
                }

                AssetDatabase.CreateAsset(medicalCaseData, dataFilePath + $"{medicalCaseData.Name.Replace(" ", "")}.asset");
            }
        }
        #endregion
    }
}