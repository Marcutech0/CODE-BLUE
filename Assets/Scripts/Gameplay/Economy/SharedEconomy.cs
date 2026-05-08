using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Collections;

namespace CodeBlue
{
    public enum TransactionType
    {
        Treatment,
        Expenditure,
        Upgrade,
        Penalty
    }

    public struct Transaction : INetworkSerializable, IEquatable<Transaction>
    {
        public float Amount;
        public TransactionType Type;

        // for severity and stuff
        // network list doesnt like strings (for performance reasons)
        public int Details;

        public bool Equals(Transaction other)
        {
            return other.Type == Type && other.Amount == Amount && other.Details == Details;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Amount);
            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref Details);
        }
    }

    public class SharedEconomy : SingletonBehaviour<SharedEconomy>
    {
        public NetworkVariable<float> CurrentSalary = new(0);

        [Header("DEBUG")]
        [SerializeField] float DEBUG_startSalary = 10000;

        public NetworkList<Transaction> Transactions { get; private set; } = new();

        protected override void PostAwake()
        {
            CvarRegistry.RegisterCommands(this);
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

#if UNITY_EDITOR
            CurrentSalary.Value = DEBUG_startSalary;
#endif
        }

#if DEBUG
        [ConFunc("add_salary")]
        void _AddSalaryCfunc(int val) => Sv_AddSalaryRpc(val, (int)TriageLevel.NonUrgent);
#endif

        [Rpc(SendTo.Server)]
        public void Sv_AddSalaryRpc(float cost, int details = -1)
        {
            if (!IsServer) return;
            CurrentSalary.Value += cost;

            Sv_LogTransactionRpc(cost, TransactionType.Treatment, details);
            UIManager.GetScreen<GameScreen>().Cl_UpdateSalaryTextRpc(CurrentSalary.Value);
            AudioManager.Instance.PlayGameSfx("coin");
        }

        [Rpc(SendTo.Server)]
        public void Sv_LogTransactionRpc(float amount, TransactionType transType, int Details)
        {
            Transactions.Add(new() { Amount = amount, Type = transType, Details = Details });
        }

        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
        public void Cl_InsufficientBalanceRpc()
        {
            print("insuff");
        }
    }
}
