using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public string localPlayerName;

    public static GameManager Instance { get; set; }
    private void Awake()
    {
        DontDestroyOnLoad(this);

        if (Instance != null)
            Destroy(gameObject);

        Instance = this;
    }

    public void SetName(InputField p_name) => localPlayerName = p_name.text;
}
