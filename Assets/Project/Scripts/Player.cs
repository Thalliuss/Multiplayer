using Mirror;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    [SerializeField, SyncVar] private new string name;
    [SerializeField] private Text _name;

    private DatabaseManager _databaseManager;

    private void Start()
    {
        _databaseManager = DatabaseManager.Instance;
    }

    [Command]
    void CmdSetName(string p_name)
    {
        name = p_name;
    }

    public override void OnStartLocalPlayer() 
    {
        CmdSetName(GameManager.Instance.localPlayerName);
    }

    private void Update()
    {
        _name.text = name;

        if (!isLocalPlayer) return;

        var t_players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject t_player in t_players) 
            t_player.transform.Find("Canvas").LookAt(transform.Find("Camera"));
    }

    private void OnDestroy()
    {
        _databaseManager.InitializeCollection("Connectivity");
        _databaseManager.ChangeValueOnDatabase(name, "offline");
    }
}
