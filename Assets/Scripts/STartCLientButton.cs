using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class STartCLientButton : MonoBehaviour
{
    public Button btn;
    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(ClickButton); // 버튼 클릭 시 ClickButton 호출
    }

    void ClickButton()
    {
        // 버튼 클릭 시 실행할 코드
        Debug.Log("Button Clicked");
        NetworkManager.Singleton.StartClient();
    }
}