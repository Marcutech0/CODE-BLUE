using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace CodeBlue
{
    public enum PrefabType
    {
        Player,
        Patient,
        SupplyBox,
        Supply,
    };

    // TODO update scripts to use this when need access to a prefab
    public class PrefabDatabase : SingletonBehaviourNonNetworked<PrefabDatabase>
    {
        [field: SerializeField, SerializedDictionary("Prefab Type", "Prefab")] public SerializedDictionary<PrefabType, GameObject> CommonPrefabs = new();
        [field: SerializeField] public GameObject[] SupplyPrefabs;
    }
}
