using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace CodeBlue
{
    public class LobbyManager : SingletonBehaviourNonNetworked<LobbyManager>
    {
        public const int MAX_CONNECTIONS = 4;
        public string JoinCode { get; private set; }

        protected override void PostAwake()
        {
            DontDestroyOnLoad(this);
        }

        public IEnumerator SignIn()
        {
            UIManager.Instance.SetNetworkStatus("logging in...");
            var servTask = UnityServices.InitializeAsync();
            while (!servTask.IsCompleted)
                yield return null;

            var signInTask = AuthenticationService.Instance.SignInAnonymouslyAsync();
            while (!signInTask.IsCompleted)
                yield return null;

            UIManager.Instance.SetNetworkStatus(null);
        }

        public IEnumerator StartServer(System.Action onServerStart)
        {
            yield return SignIn();
            UIManager.Instance.SetNetworkStatus("Creating lobby...");
            var allocTask = RelayService.Instance.CreateAllocationAsync(MAX_CONNECTIONS);
            while (!allocTask.IsCompleted)
                yield return null;

            if (allocTask.IsFaulted)
            {
                Debug.LogError(allocTask.Exception.Message);
                UIManager.Instance.SetNetworkStatus(allocTask.Exception.Message, StatusTextType.Error);
                yield break;
            }

            var alloc = allocTask.Result;

            var relay = alloc.ToRelayServerData("dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relay);

            var joinCode = RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            while (!joinCode.IsCompleted)
                yield return null;

            if (joinCode.IsFaulted)
            {
                Debug.LogError("Failed to request join code");
                yield break;
            }

            if (NetworkManager.Singleton.StartHost())
            {
                JoinCode = joinCode.Result;
                onServerStart.Invoke();
                UIManager.Instance.SetNetworkStatus(null);
            }
        }

        public IEnumerator JoinClient(string joinCode, System.Action onJoin)
        {
            yield return SignIn();
            UIManager.Instance.SetNetworkStatus("Joining lobby...");
            var joinTask = RelayService.Instance.JoinAllocationAsync(joinCode);
            while (!joinTask.IsCompleted)
                yield return null;

            if (joinTask.IsFaulted)
            {
                var errorMsg = "Failed to join";
                joinTask.Exception.Handle((x) =>
                {
                    if (x is System.ArgumentNullException)
                    {
                        errorMsg = "Please enter a join code";
                        return true;
                    }
                    errorMsg = "Failed to join. Please check your join code";
                    return true;
                });
                UIManager.Instance.SetNetworkStatus(errorMsg, StatusTextType.Error);
                yield break;
            }

            var alloc = joinTask.Result;
            var relay = alloc.ToRelayServerData("dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relay);

            if (NetworkManager.Singleton.StartClient())
            {
                onJoin.Invoke();
                UIManager.Instance.SetNetworkStatus(null);
            }
        }
    }
}
