﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Mirror;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public int round;
    public GamePlayer player;
    public Timer myTimer;
    public Text phaseText;

    public string currentPhase;
    public bool influenceBool;
    public bool hotseatBool;
    public string currentPlayer;

    int phasePrompt;
    int phaseAction;
    int phaseInfluence;
    int phaseHotseat;
    int influenceCount;
    int hotseatCount;
    public Text influenceCountText;
    public Text hotseatCountText;
    public Text playerNameText;
    public Text pointText;
    CancellationTokenSource tokenSource;

    private NetworkManagerLobby room;
    private NetworkManagerLobby Room
    {
        get{
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerLobby;
        }
    }

    // sets up singleton control object
    void Awake()
    {
        //GamePlayer.OnRate += OnPlayerRate;
    }
    public async Task Start()
    {
        // variable initializations
        phasePrompt = 4;
        phaseAction = 6;
        phaseInfluence = 8;
        phaseHotseat = 10;
        hotseatCount = 1;
        influenceCount = 2;
        // this method sets slider parent, slider name text
        SetPlayerParent();
        // reference to local player object
        player = NetworkClient.connection.identity.GetComponent<GamePlayer>();
        // sets ui name text (bottom left)
        playerNameText.text = player.displayName;
        await Task.Delay(100);
        round = 0;
        // Starts the timer automatically
        for (int i = 0; i < 5; i++)
        {
            await GameRound();  
            round++;          
        }
    }
    void Update()
    {

        if (influenceBool || hotseatBool) 
        {
            tokenSource.Cancel();
        }

    }

    public async Task GameRound() 
    {
        currentPhase = "prompt";
        phaseText.text = "Phase:\nPrompt";
        hotseatCountText.text = "Hotseats: " + Convert.ToString(hotseatCount);
        influenceCountText.text = "Influences: " + Convert.ToString(influenceCount);
        myTimer.SetTimer(phasePrompt);
        SetOtherPlayersInactive();
        // this is in milliseconds so must be multipied by 1000
        await Task.Delay(phasePrompt * 1000);
        player.CmdRate(player.displayName, Convert.ToInt32(player.mySlider.value));
        await Task.Delay(2000);
        UpdateRatings();
        await ActionLoop();
    }

    void UpdateRatings()
    {
        foreach (GamePlayer player in Room.GamePlayers)
        {
            GameControl.control.UpdateAllRatings(player.displayName, player.rating);
        }
    }

    public async Task ActionLoop()
    {
        currentPhase = "action";
        myTimer.SetTimer(phaseAction);
        SetOtherPlayersActive();
        phaseText.text = "Phase:\nPower";
        tokenSource = new CancellationTokenSource();
        try {
            await Task.Delay(phaseAction * 1000, tokenSource.Token);
        }
        catch (TaskCanceledException) {
            if (influenceBool) {
                phaseText.text = "Phase:\n" + currentPlayer + " is influencing";
                currentPhase = "influence";
                myTimer.SetTimer(phaseInfluence);
                await Task.Delay(phaseInfluence * 1000);
                ResetPowerBools();
                await ActionLoop();
            }
            else if (hotseatBool)
            {
                phaseText.text = "Phase:\n" + currentPlayer + " is hotseating someone";;
                currentPhase = "hotseat";
                myTimer.SetTimer(phaseHotseat);
                await Task.Delay(phaseHotseat * 1000);
                ResetPowerBools();
                await ActionLoop();
            }
            tokenSource = new CancellationTokenSource();
        }
    }

    void SetPlayerParent()
    {
        foreach (GamePlayer player in Room.GamePlayers)
        {
            player.transform.SetParent(GameObject.Find("GameUI/Canvas/Background/SliderPanel").transform);
            player.MySliderNameText.text = player.displayName;
        }
    }

    void SetOtherPlayersActive()
    {
        foreach (GamePlayer player in Room.GamePlayers)
        {
            if (!player.hasAuthority)
            {
                player.gameObject.SetActive(true);
                player.mySlider.enabled = false;
                player.mySlider.value = GameControl.control.AllRatings[player.displayName][round];
            }
        }
    
    }

    void SetOtherPlayersInactive()
    {
        foreach (GamePlayer player in Room.GamePlayers)
        {
            if (!player.hasAuthority)
            {
                player.gameObject.SetActive(false);
            }
        }
    }
    void ResetPowerBools()
    {
        hotseatBool = false;
        influenceBool = false;
    }
}
