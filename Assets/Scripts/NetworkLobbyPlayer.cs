using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;


public class NetworkLobbyPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private Text[] playerNameTexts = new Text[8];
    [SerializeField] private Text[] playerReadyTexts = new Text[8];
    [SerializeField] private Button startGameButton = null;


    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;
    private bool isLeader;
    public bool IsLeader
    {
        set{
            isLeader = value;
            startGameButton.gameObject.SetActive(value);
        }
    }

    private NetworkManagerLobby room;
    private NetworkManagerLobby Room
    {
        get{
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerLobby;
        }
    }

    public override void OnStartAuthority()
    {
        CmdSetDisplayName(MainMenu.DisplayName);
        lobbyUI.SetActive(true);
    }

    public override void OnStartClient()
    {
        Room.LobbyPlayers.Add(this);
        UpdateDisplay();
    }

    public override void OnStopClient()
    {
        Room.LobbyPlayers.Remove(this);

        UpdateDisplay();
    }

    public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();
    public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();

    private void UpdateDisplay()
    {
        if (!hasAuthority)
        {
            foreach (var player in Room.LobbyPlayers)
            {
                if (player.hasAuthority)
                {
                    player.UpdateDisplay();
                    break;
                }
            }
            return;
        }

        for (int i = 0; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting ...";
            playerReadyTexts[i].text = string.Empty;
        }

        for (int i = 0; i < Room.LobbyPlayers.Count; i++)
        {
            playerNameTexts[i].text = Room.LobbyPlayers[i].DisplayName;
            playerReadyTexts[i].text = Room.LobbyPlayers[i].IsReady ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
        }
    }

    public void HandleReadyToStart(bool readyToStart)
    {
        if (!isLeader) {return;}

        startGameButton.interactable = readyToStart;
    }

    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }

    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;

        Room.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdStartGame()
    {
        if (Room.LobbyPlayers[0].connectionToClient != connectionToClient) {return;}

        Room.StartGame();
    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Debug.Log("Lobby Player Count" + Room.LobbyPlayers.Count);
        }
    }
}

