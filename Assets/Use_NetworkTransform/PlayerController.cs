using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;

public class PlayerController : NetworkBehaviour
{
    public float speed = 5f;

    private MyClientNetworkTransform m_NetworkTransform;

    private void Start()
    {
        m_NetworkTransform = GetComponent<MyClientNetworkTransform>();
    }

    private void Update()
    {
        if(!IsOwner) return;
        var movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        m_NetworkTransform.transform.position += movement * speed * Time.deltaTime;
    }
}
