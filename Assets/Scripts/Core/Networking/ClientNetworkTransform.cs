using Unity.Netcode.Components;
using UnityEngine;

namespace CodeBlue
{
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative() => false;
    }
}