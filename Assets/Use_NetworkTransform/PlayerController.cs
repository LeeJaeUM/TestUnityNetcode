using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;
using System.Collections;

public class PlayerController : NetworkBehaviour
{
    public float speed = 5f;
    public float jumpForce = 7f;
    public float gravityMultiplier = 2f; // �߷� ����
    private bool isGrounded = true;
    private bool isStabbing = false;

    [SerializeField]    
    private bool isDied = false;
    [SerializeField]    
    private NetworkVariable<bool> isDiedNetVar = new NetworkVariable<bool>(false);


    private Rigidbody rb;
    private Animator animator;

    private MyClientNetworkTransform m_NetworkTransform;

    // ���� �ݶ��̴�
    public BoxCollider stabCollider;  // �ڽ� ������Ʈ�� �ִ� BoxCollider�� ����
    // ť���� ������
    public Renderer cubeRenderer;

    // ��Ƽ���� �迭 (0: �⺻, 1: ���� ��)
    public Material[] materials;

    private void Start()
    {
        m_NetworkTransform = GetComponent<MyClientNetworkTransform>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // �ݶ��̴� ��Ȱ��ȭ
        stabCollider.enabled = false;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if(!isDiedNetVar.Value)
        {

            if (!isStabbing)
            {
                Move();

                if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
                {
                    Jump();
                }

                if (Input.GetKeyDown(KeyCode.X))
                {
                    Stab();
                }
            }
        }
        else
        {

        }

    }

    void Move()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");

        if (moveHorizontal != 0)
        {
            // �÷��̾ �����̴� ���⿡ ���� ȸ��
            if (moveHorizontal < 0)  // �������� �̵�
            {
                transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            }
            else if (moveHorizontal > 0)  // ���������� �̵�
            {
                transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            }

            animator.SetBool("isWalking", true); // �ȱ� �ִϸ��̼� ����
        }
        else
        {
            animator.SetBool("isWalking", false); // �ȱ� �ִϸ��̼� ����
        }

        // �̵�
        Vector3 movement = new Vector3(moveHorizontal, 0f, 0f);
        m_NetworkTransform.transform.position += movement * speed * Time.deltaTime;
    }

    void Jump()
    {
        rb.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Impulse);
        isGrounded = false;
    }

    void Stab()
    {
        animator.SetTrigger("Stab");
        StartCoroutine(ActivateStabCollider()); // ���� �ݶ��̴� Ȱ��ȭ �ڷ�ƾ ����
    }

    private IEnumerator ActivateStabCollider()
    {
        isStabbing = true;

        // ��� �ִϸ��̼��� ���� ���� ���� �ð� ���� ������ ���� (���÷� 0.5�ʷ� ����)
        yield return new WaitForSeconds(0.3f);

        // �ִϸ��̼� ���� �� ��� ��ٸ��� �ݶ��̴� Ȱ��ȭ
        stabCollider.enabled = true;
        cubeRenderer.material = materials[1];
        yield return new WaitForSeconds(0.1f);

        // ������ ���� �� �ݶ��̴� ��Ȱ��ȭ

        cubeRenderer.material = materials[0];
        stabCollider.enabled = false;
        yield return new WaitForSeconds(0.3f);

        isStabbing = false;
    }


    private void FixedUpdate()
    {
        // �߰� �߷� ����
        if (!isGrounded)
        {
            rb.AddForce(new Vector3(0f, -gravityMultiplier * Physics.gravity.y, 0f), ForceMode.Acceleration);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Attack"))
        {
            Debug.Log("���ݿ� ����");

            if (IsOwner) // ���ݹ��� �÷��̾ ���� �÷��̾��� ���
            {
                // ���� �÷��̾ ���ݹ����� ������ ���� ó�� ��û
                DieServerRpc();
            }
            else
            {
                // �ٸ� �÷��̾ ������ ������ �������� ó���ϵ��� ��
                DieOtherPlayerServerRpc();
            }
        }
    }
    void Die()
    {
        isDiedNetVar.Value = true;
        animator.SetTrigger("Die");
    }


    // ���� �÷��̾ ���� �� ������ ��û�ϴ� RPC
    [ServerRpc(RequireOwnership = false)] // �ٸ� Ŭ���̾�Ʈ�� ȣ���� �� �ֵ��� ����
    void DieServerRpc()
    {
        // �������� ��� ó��
        isDiedNetVar.Value = true; // ��Ʈ��ũ ���� ���� �����Ͽ� ��� Ŭ���̾�Ʈ�� �ݿ�
        animator.SetTrigger("Die");

        // �������� ��� Ŭ���̾�Ʈ���� ��� ���� ����
        UpdateDieStateClientRpc();
    }

    // �ٸ� �÷��̾ ���� �� �������� ó���ϴ� RPC
    [ServerRpc(RequireOwnership = false)]
    void DieOtherPlayerServerRpc()
    {
        // �������� ���� ó��
        Die();

        // �������� ��� Ŭ���̾�Ʈ���� ��� ���� ����
        UpdateDieStateClientRpc();
    }

    // Ŭ���̾�Ʈ���� ��� ���¸� ������Ʈ�ϴ� RPC
    [ClientRpc]
    void UpdateDieStateClientRpc()
    {
        // �������� ��� Ŭ���̾�Ʈ�� ��� ���� ������Ʈ
        Die();
    }

}