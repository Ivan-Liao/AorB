using System;
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
    public Image gameLogScreen;
    public Text gameLogText;

    public string currentPhase;
    public bool influenceBool;
    public bool hotseatBool;
    public string currentPlayer;

    int phasePrompt;
    int phaseAction;
    int phaseInfluence;
    int phaseHotseat;
    int phaseVoteAgain;
    int influenceCount;
    int hotseatCount;
    public Text influenceCountText;
    public Text hotseatCountText;
    public Text playerNameText;
    public Text pointText;
    public Text promptText;
    public Text AText;
    public Text BText;
    CancellationTokenSource tokenSource;
    List<int> currentPromptDictKeyList;

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
        GamePlayer.OnPower += OnPlayerPower;
    }
    public async Task Start()
    {
        await Task.Delay(500);
        // variable initializations
        phasePrompt = 40;
        phaseAction = 10;
        phaseInfluence = 30;
        phaseHotseat = 30;
        phaseVoteAgain = 7;
        hotseatCount = 1;
        influenceCount = 2;
        Debug.Log("executes 6");
        currentPromptDictKeyList = GameControl.control.currentPromptsDict.Keys.ToList();
        Debug.Log("executes 7");
        // this method sets slider parent, slider name text
        SetPlayerParent();
        // reference to local player object
        player = NetworkClient.connection.identity.GetComponent<GamePlayer>();
        // sets ui name text (bottom left)
        playerNameText.text = player.displayName;
        await Task.Delay(100);
        round = 0;
        // Starts the timer automatically
        for (int i = 0; i < GameControl.control.numCurrentPrompts; i++)
        {
            await GameRound();  
            round++;          
        }

        gameLogScreen.gameObject.SetActive(true);
        gameLogText.text = "";
        int tempCount = 0;
        List<string> allRatingsNameList = GameControl.control.AllRatings.Keys.ToList();
        foreach (int key in GameControl.control.currentPromptsDict.Keys.ToList())
        {
            gameLogText.text += "\n\n\n\n";
            gameLogText.text += GameControl.control.currentPromptsDict[key][0];
            gameLogText.text += "\nA - ";
            gameLogText.text += GameControl.control.currentPromptsDict[key][1];
            gameLogText.text += "\nB - ";
            gameLogText.text += GameControl.control.currentPromptsDict[key][2];
            for (int nameInt = 0; nameInt < GameControl.control.AllRatings.Count; nameInt++)
            {
                gameLogText.text += "\n\n";
                gameLogText.text += allRatingsNameList[nameInt];
                gameLogText.text += " ";
                gameLogText.text += GameControl.control.AllRatings[allRatingsNameList[nameInt]][tempCount];
                gameLogText.text += " ";
                gameLogText.text += RatingToWord(GameControl.control.AllRatings[allRatingsNameList[nameInt]][tempCount]);
            }
            tempCount++;
        }
    }

    string RatingToWord(int rating)
    {
        string result = "";
        switch (rating)
        {
            case -5:
                result = "- Super Strong A";
                break;
            case -4:
                result = "- Strong A";
                break;
            case -3:
                result =  "- High Moderate A";
                break;
            case -2:
                result =  "- Low Moderate A";
                break;
            case -1:
                result =  "- Slight A";
                break;
            case 0:
                result =  "- Neutral";
                break;
            case 1:
                result =  "- Slight B";
                break;
            case 2:
                result =  "- Low Moderate B";
                break;
            case 3:
                result =  "- High Moderate B";
                break;
            case 4:
                result =  "- Strong B";
                break;
            case 5:
                result =  "- Super Strong B";
                break;
        }
        return result;
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
        Debug.Log("executes 1");
        promptText.text = GameControl.control.currentPromptsDict[currentPromptDictKeyList[round]][0];
        Debug.Log("executes 2");
        AText.text = GameControl.control.currentPromptsDict[currentPromptDictKeyList[round]][1];
        Debug.Log("executes 3");
        BText.text = GameControl.control.currentPromptsDict[currentPromptDictKeyList[round]][2];
        Debug.Log("executes 4");
        hotseatCountText.text = "Hotseats: " + Convert.ToString(hotseatCount);
        influenceCountText.text = "Influences: " + Convert.ToString(influenceCount);
        myTimer.SetTimer(phasePrompt);
        SetOtherPlayersInactive();
        // this is in milliseconds so must be multipied by 1000
        await Task.Delay(phasePrompt * 1000);
        player.CmdRate(player.displayName, Convert.ToInt32(player.mySlider.value));
        await Task.Delay(500);
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
    // this overload method replaces the rating for the specific round, used in vote again phase
    void UpdateRatings(int round)
    {
        foreach (GamePlayer player in Room.GamePlayers)
        {
            GameControl.control.UpdateAllRatings(player.displayName, player.rating, round);
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
                await VoteAgain();
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
    
    public async Task VoteAgain()
    {
        currentPhase = "vote again";
        phaseText.text = "Phase:\n Change Vote";
        SetOtherPlayersInactive();
        myTimer.SetTimer(phaseVoteAgain);
        await Task.Delay(phaseVoteAgain * 1000);
        player.CmdRate(player.displayName, Convert.ToInt32(player.mySlider.value));
        await Task.Delay(500);
        UpdateRatings(round);
    }

    void SetPlayerParent()
    {
        foreach (GamePlayer player in Room.GamePlayers)
        {
            player.transform.SetParent(GameObject.Find("GameUI/Canvas/Background/SliderPanel").transform);
            player.MySliderNameText.text = player.displayName;
        }
    }
    // also disables all other sliders AND your own slider
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
            else
            {
                player.mySlider.enabled = false;
            }
        }
    
    }

    // also enables your own slider
    void SetOtherPlayersInactive()
    {
        foreach (GamePlayer player in Room.GamePlayers)
        {
            if (!player.hasAuthority)
            {
                player.gameObject.SetActive(false);
            }
            else{
                player.mySlider.enabled = true;
            }
        }
    }
    void ResetPowerBools()
    {
        hotseatBool = false;
        influenceBool = false;
    }

    public void sendPowerInfluence()
    {
        if (influenceCount > 0 && currentPhase == "action")
        {
            player.CmdSendPower("influence");
            influenceCount --;
            influenceCountText.text = "Influences: " + Convert.ToString(influenceCount);
        }
        
    }

    public void sendPowerHotseat()
    {
        if (hotseatCount > 0 && currentPhase == "action")
        {
            player.CmdSendPower("hotseat");
            hotseatCount --;
            hotseatCountText.text = "Hotseats: " + Convert.ToString(hotseatCount);
        }
        
    }

    void OnPlayerPower(GamePlayer player, string powerType)
    {
        if (powerType == "influence")
        {
            influenceBool = true;
            currentPlayer = player.displayName;
        }
        else if (powerType == "hotseat")
        {
            hotseatBool = true;
            currentPlayer = player.displayName;
        }
    }
}
