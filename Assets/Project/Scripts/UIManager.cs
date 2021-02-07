using kcp2k;
using Mirror;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; set; }

    [SerializeField] private InputField _ipAdress;
    [SerializeField] private InputField _port;

    [SerializeField] private UnityEvent _onConnect;
    [SerializeField] private UnityEvent _onFailed;

    [HideInInspector] public string localPlayer;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    private void Initialize()
    {
        NetworkManager.Instance.networkAddress = _ipAdress.text;
        NetworkManager.Instance.gameObject.GetComponent<KcpTransport>().Port = ushort.Parse(_port.text);
    }

    public async void StartClient() 
    {
        Initialize();
        _onConnect.Invoke();

        Task<bool> t_await = ServerStatus(3000);
        bool t_result = await t_await;

        if (!t_result) _onFailed.Invoke();
    }

    private async Task<bool> ServerStatus(int t_wait) 
    {
        NetworkManager.Instance.StartClient();

        await Task.Delay(t_wait);

        if (SceneManager.GetActiveScene().name == "Offline") return false;

        return true;
    }

    public void Disconnect()
    {
        NetworkManager.Instance.StopClient();
        Application.Quit();
    }

    public void SetName(InputField p_name) => localPlayer = p_name.text;
}
