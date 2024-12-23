using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldPlayer : NetworkBehaviour
    {
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

        public override void OnNetworkSpawn()
        {
            Debug.Log(" ");
            Position.OnValueChanged += OnStateChanged;

            if (IsOwner)
            {
                Move();
            }
        }

        public override void OnNetworkDespawn()
        {
            Position.OnValueChanged -= OnStateChanged;
        }

        public void OnStateChanged(Vector3 previous, Vector3 current)
        {
            Debug.Log("OnStateChanged 했어 ");
            // note: `Position.Value` will be equal to `current` here
            if (Position.Value != previous)
            {
                Debug.Log(" OnStateC 조건문했어 ");
                transform.position = Position.Value;
            }
        }

        public void Move()
        {
            Debug.Log(" 나 움직였어");
            SubmitPositionRequestServerRpc();
        }

        [Rpc(SendTo.Server)]
        void SubmitPositionRequestServerRpc(RpcParams rpcParams = default)
        {
            Debug.Log("서버로 보냈어 ");
            var randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;
        }

        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }
    }
}