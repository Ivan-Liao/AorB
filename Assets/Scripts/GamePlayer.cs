using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;
using System.Threading.Tasks;


public class GamePlayer : NetworkBehaviour
{

    public Timer myTimer;
    public UIController myUIController;
    public string currentPhase;
    [SyncVar]
    public bool influenceBool;
    [SyncVar]
    public bool hotseatBool;
    int phasePrompt;
    int phaseAction;
    int phaseInfluence;
    int phaseHotseat;
    [SyncVar]
    public int playerNo;

    CancellationTokenSource tokenSource;


    [SyncVar]
    private string displayName = "Loading...";

    private NetworkManagerLobby room;
    private NetworkManagerLobby Room
    {
        get{
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerLobby;
        }
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        playerNo = Room.GamePlayers.Count;
        Room.GamePlayers.Add(this);
    }

    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this);
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    public void SetPlayerNo(int num)
    {
        this.playerNo = num;
    }

    public async Task Start()
    {
        await Task.Delay(100);
        foreach (var player in Room.GamePlayers)
        {
            if (!player.hasAuthority)
            {
                player.gameObject.SetActive(false);
            }
        }
        // Starts the timer automatically
        for (int i = 0; i < 5; i++)
        {
            await GameRound();            
        }
    }

    public async Task GameRound() {
        phasePrompt = 4;
        phaseAction = 6;
        phaseInfluence = 8;
        phaseHotseat = 10;

        currentPhase = "prompt";
        myUIController.ratingSlider.enabled = true;
        myTimer.SetTimer(phasePrompt);
        myUIController.phaseText.text = "Phase:\nPrompt";
        // this is in milliseconds so must be multipied by 1000
        await Task.Delay(phasePrompt * 1000);

        await ActionLoop();
    }

    public async Task ActionLoop()
    {
        currentPhase = "action";
        myUIController.SetOtherSlidersActive();
        myUIController.ratingSlider.enabled = false;
        myTimer.SetTimer(phaseAction);
        myUIController.phaseText.text = "Phase:\nPower";
        tokenSource = new CancellationTokenSource();
        try {
            await Task.Delay(phaseAction * 1000, tokenSource.Token);
            myUIController.SetOtherSlidersInactive();
        }
        catch (TaskCanceledException) {
            if (influenceBool) {
                myUIController.phaseText.text = "Phase:\n" + Room.GamePlayers[Room.playerNo].displayName + " is influencing";
                currentPhase = "influence";
                myTimer.SetTimer(phaseInfluence);
                await Task.Delay(phaseInfluence * 1000);
                ResetPowerBools();
                await ActionLoop();
            }
            else if (hotseatBool)
            {
                myUIController.phaseText.text = "Phase:\n" + Room.GamePlayers[Room.playerNo].displayName + " is hotseating someone";;
                currentPhase = "hotseat";
                myTimer.SetTimer(phaseHotseat);
                await Task.Delay(phaseHotseat * 1000);
                ResetPowerBools();
                await ActionLoop();
            }
            tokenSource = new CancellationTokenSource();
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (influenceBool || hotseatBool) 
        {
            tokenSource.Cancel();
        }


        if (Input.GetKeyDown("space"))
        {
            Debug.Log(playerNo);
            Debug.Log(displayName);
        }
    }

    void ResetPowerBools()
    {
        for (int i = 0; i < Room.GamePlayers.Count; i++)
        {
            Room.GamePlayers[i].influenceBool = false;
            Room.GamePlayers[i].hotseatBool = false;
        }
    }

    
    [Command]
    public void CmdInfluencePhase()
    {
        Room.InfluencePhase(playerNo);
    }

    [Command]
    public void CmdHotseatPhase()
    {
        Room.HotseatPhase(playerNo);
    }

    public void UpdateDisplay()
    {
        if (!hasAuthority)
        {
            foreach (var player in Room.GamePlayers)
            {
                if (player.hasAuthority)
                {
                    player.UpdateDisplay();
                    break;
                }
            }
            return;
        }
    }
}

