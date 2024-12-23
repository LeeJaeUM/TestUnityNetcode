using UnityEngine;
using Unity.Netcode.Components;

[DisallowMultipleComponent] // Prevents you from adding the ClientNetworkTransform to the object more than once.
public class MyClientNetworkTransform : NetworkTransform
{
    /// <summary>
    /// Used to determine who can write to this transform. Owner client only.
    /// This imposes state to the server. This is putting trust on your clients. Make sure no security-sensitive features use this transform.
    /// </summary>
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}