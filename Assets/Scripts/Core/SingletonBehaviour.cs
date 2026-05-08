using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public class SingletonBehaviourNonNetworked<T> : MonoBehaviour where T : Component
    {
        public static T Instance { get; private set; }

        public virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
            }
            else
            {
                Destroy(gameObject);
            }

            PostAwake();
        }

        protected virtual void PostAwake() { }
    }

    public class SingletonBehaviour<T> : NetworkBehaviour where T : Component
    {
        public static T Instance { get; private set; }

        public virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
            }
            else
            {
                Destroy(gameObject);
            }

            PostAwake();
        }
        protected virtual void PostAwake() { }
    }
}
