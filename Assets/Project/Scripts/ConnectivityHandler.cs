using kcp2k;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnectivityHandler : MonoBehaviour
{
    [HideInInspector] public string localPlayer;

    [Header("IP Adress / Port")]
    [SerializeField] private InputField _ipAdress;
    [SerializeField] private InputField _port;

    [Header("Login Information")]
    [SerializeField] private InputField _username;
    [SerializeField] private InputField _password;

    [SerializeField] private UnityEvent _onConnect;
    [SerializeField] private UnityEvent _onFailed;

    private void Initialize()
    {
        NetworkManager.Instance.networkAddress = _ipAdress.text;
        NetworkManager.Instance.gameObject.GetComponent<KcpTransport>().Port = ushort.Parse(_port.text);
    }

    public async void StartClient() 
    {
        DatabaseManager t_databaseManager = DatabaseManager.Instance;

        Initialize();
        _onConnect.Invoke();

        Task<bool> t_await = WaitForServerResponse(3000);
        bool t_result = await t_await;

        Task<string> t_onlineawait = t_databaseManager.PullValueFromDatabase(_username.text);
        string t_onlineresult = await t_onlineawait;

        if (!t_result) _onFailed.Invoke();
        else if (t_onlineresult.Equals("")) t_databaseManager.PushToDatabase(_username.text, "online");
        else t_databaseManager.ChangeValueOnDatabase(_username.text, "online");
    }

    private async Task<bool> WaitForServerResponse(int t_wait) 
    {
        NetworkManager.Instance.StartClient();

        await Task.Delay(t_wait);

        if (SceneManager.GetActiveScene().name == "Offline") return false;

        return true;
    }

    public async void Login()
    {
        DatabaseManager t_databaseManager = DatabaseManager.Instance;

        t_databaseManager.InitializeCollection("Accounts");

        Task<Dictionary<string, string>> t_accountawait = t_databaseManager.PullFromDatabase();
        Dictionary<string, string> t_accountresult = await t_accountawait;

        for (int i = 0; i < t_accountresult.Count; i++)
        {
            if (t_accountresult.Keys.ToList()[i] == _username.text && t_accountresult.Values.ToList()[i] == _password.text)
            {
                t_databaseManager.InitializeCollection("Connectivity");

                Task<string> t_onlineawait = t_databaseManager.PullValueFromDatabase(_username.text);
                string t_onlineresult = await t_onlineawait;

                if (t_onlineresult.Equals("online")) return;

                StartClient();

                return;
            }
        }
    }
}
