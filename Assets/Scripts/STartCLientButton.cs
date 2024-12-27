using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class STartCLientButton : MonoBehaviour
{
    public Button btn;
    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(ClickButton); // ��ư Ŭ�� �� ClickButton ȣ��
    }

    void ClickButton()
    {
        // ��ư Ŭ�� �� ������ �ڵ�
        Debug.Log("Button Clicked");
        NetworkManager.Singleton.StartClient();
    }
}