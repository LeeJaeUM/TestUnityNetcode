using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;
using System.Collections;

public class PlayerController : NetworkBehaviour
{
    public float speed = 5f;
    public float jumpForce = 7f;
    public float gravityMultiplier = 2f; // 중력 배율
    private bool isGrounded = true;
    private bool isStabbing = false;

    [SerializeField]    
    private bool isDied = false;
    [SerializeField]    
    private NetworkVariable<bool> isDiedNetVar = new NetworkVariable<bool>(false);


    private Rigidbody rb;
    private Animator animator;

    private MyClientNetworkTransform m_NetworkTransform;

    // 공격 콜라이더
    public BoxCollider stabCollider;  // 자식 오브젝트에 있는 BoxCollider를 참조
    // 큐브의 렌더러
    public Renderer cubeRenderer;

    // 머티리얼 배열 (0: 기본, 1: 공격 중)
    public Material[] materials;

    private void Start()
    {
        m_NetworkTransform = GetComponent<MyClientNetworkTransform>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // 콜라이더 비활성화
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
            // 플레이어가 움직이는 방향에 따라 회전
            if (moveHorizontal < 0)  // 왼쪽으로 이동
            {
                transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            }
            else if (moveHorizontal > 0)  // 오른쪽으로 이동
            {
                transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            }

            animator.SetBool("isWalking", true); // 걷기 애니메이션 시작
        }
        else
        {
            animator.SetBool("isWalking", false); // 걷기 애니메이션 중지
        }

        // 이동
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
        StartCoroutine(ActivateStabCollider()); // 공격 콜라이더 활성화 코루틴 시작
    }

    private IEnumerator ActivateStabCollider()
    {
        isStabbing = true;

        // 찌르기 애니메이션의 절반 정도 지속 시간 동안 공격을 유지 (예시로 0.5초로 설정)
        yield return new WaitForSeconds(0.3f);

        // 애니메이션 시작 후 잠시 기다리고 콜라이더 활성화
        stabCollider.enabled = true;
        cubeRenderer.material = materials[1];
        yield return new WaitForSeconds(0.1f);

        // 공격이 끝날 때 콜라이더 비활성화

        cubeRenderer.material = materials[0];
        stabCollider.enabled = false;
        yield return new WaitForSeconds(0.3f);

        isStabbing = false;
    }


    private void FixedUpdate()
    {
        // 추가 중력 적용
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
            Debug.Log("공격에 닿음");

            if (IsOwner) // 공격받은 플레이어가 로컬 플레이어인 경우
            {
                // 로컬 플레이어가 공격받으면 서버로 죽음 처리 요청
                DieServerRpc();
            }
            else
            {
                // 다른 플레이어가 공격을 받으면 서버에서 처리하도록 함
                DieOtherPlayerServerRpc();
            }
        }
    }
    void Die()
    {
        isDiedNetVar.Value = true;
        animator.SetTrigger("Die");
    }


    // 로컬 플레이어가 죽을 때 서버에 요청하는 RPC
    [ServerRpc(RequireOwnership = false)] // 다른 클라이언트도 호출할 수 있도록 설정
    void DieServerRpc()
    {
        // 서버에서 사망 처리
        isDiedNetVar.Value = true; // 네트워크 변수 값을 설정하여 모든 클라이언트에 반영
        animator.SetTrigger("Die");

        // 서버에서 모든 클라이언트에게 사망 상태 전파
        UpdateDieStateClientRpc();
    }

    // 다른 플레이어가 죽을 때 서버에서 처리하는 RPC
    [ServerRpc(RequireOwnership = false)]
    void DieOtherPlayerServerRpc()
    {
        // 서버에서 죽음 처리
        Die();

        // 서버에서 모든 클라이언트에게 사망 상태 전파
        UpdateDieStateClientRpc();
    }

    // 클라이언트에서 사망 상태를 업데이트하는 RPC
    [ClientRpc]
    void UpdateDieStateClientRpc()
    {
        // 서버에서 모든 클라이언트에 사망 상태 업데이트
        Die();
    }

}