using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    [SerializeField, SyncVar] private new string name;
    [SerializeField] private Text _name;

    [Command]
    void CmdSetName(string p_name)
    {
        name = p_name;
    }


    public override void OnStartLocalPlayer() 
    {
        string t_name = UIManager.Instance.localPlayer;
        CmdSetName(t_name);
    }

    private void Update()
    {
        _name.text = name;

        if (!isLocalPlayer) return;

        var t_players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject t_player in t_players) 
            t_player.transform.Find("Canvas").LookAt(transform.Find("Camera"));
    }
}
