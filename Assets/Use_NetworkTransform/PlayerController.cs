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
    [SerializeField] private float preAttackDelay = 0.3f; // �ִϸ��̼� ���� �� Ȱ��ȭ���� �ð�
    [SerializeField] private float activeDuration = 0.1f; // ���� ���� Ȱ��ȭ �ð�
    [SerializeField] private float postAttackDelay = 0.3f; // ��Ȱ��ȭ �� ��� �ð�

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
        //isDiedNetVar.OnValueChanged += OnDiedStatusChanged;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (!isDiedNetVar.Value)
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
            if (!isDied)
            {
                Debug.Log("UpdateDieTest");
                isDied = true;
            }
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

            MoveAnimToServerRpc(true); // �ȱ� �ִϸ��̼� ����
        }
        else
        {
            MoveAnimToServerRpc(false); // �ȱ� �ִϸ��̼� ����
        }

        // �̵�
        Vector3 movement = new Vector3(moveHorizontal, 0f, 0f);
        m_NetworkTransform.transform.position += movement * speed * Time.deltaTime;
    }

    [ServerRpc]
    void MoveAnimToServerRpc(bool isTrue)
    {
        animator.SetBool("isWalking", isTrue);
    }
    void Jump()
    {
        rb.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Impulse);
        isGrounded = false;
    }

    void Stab()
    {
        TriggerStabServerRpc();// ���� �ݶ��̴� Ȱ��ȭ �ڷ�ƾ ����
    }

    [ServerRpc]
    void TriggerStabServerRpc()
    {
        animator.SetTrigger("Stab");
        StartCoroutine(ActivateStabCollider());
    }
    private IEnumerator ActivateStabCollider()
    {
        isStabbing = true;

        // �ִϸ��̼��� �ʱ� ���� �ð�
        yield return new WaitForSeconds(preAttackDelay);

        // ���� ���� Ȱ��ȭ �� �ð��� ȿ��
        stabCollider.enabled = true;
        cubeRenderer.material = materials[1];

        // Ȱ��ȭ ���� �ð�
        yield return new WaitForSeconds(activeDuration);

        // ���� ���� ��Ȱ��ȭ �� �ð��� ȿ�� ����
        stabCollider.enabled = false;
        cubeRenderer.material = materials[0];

        // �ļ� ��� �ð�
        yield return new WaitForSeconds(postAttackDelay);

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
            //if(IsOwner)
            //{
            //    Debug.Log($"{gameObject.name}�� ���ݿ� ����");
            //    DieServerRpc();
            //}
            //else
            if (IsServer)
            {
                isDiedNetVar.Value = true;
                Debug.Log("�������� ������ ó��");
                UpdateDieStateClientRpc();
            }
        }
    }
    // Ŭ���̾�Ʈ���� ������ ��� ó�� �� ���� ����
    [ServerRpc]
    void DieServerRpc()
    {
        // �������� ��� ó��
        //Die();
        isDiedNetVar.Value = true;
        animator.SetTrigger("Die");

        // �������� ��� Ŭ���̾�Ʈ���� ��� ���� ����
        UpdateDieStateClientRpc();
    }

    // Ŭ���̾�Ʈ���� ��� ���� ������Ʈ
    [ClientRpc]
    void UpdateDieStateClientRpc()
    {
        // �������� ��� Ŭ���̾�Ʈ�� ��� ���� ������Ʈ
        // �ٸ� Ŭ���̾�Ʈ���� isDiedNetVar.Value�� true�� ���ŵ�

        if (isDiedNetVar.Value)
        {
            // �ٸ� Ŭ���̾�Ʈ�� �׾��� �� ó���� ����
            animator.SetTrigger("Die");
        }
        // Destroy(gameObject, 1.5f);
    }
}