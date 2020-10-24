using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    void Start()
    {
        phasePrompt = 10;
        phaseAction = 10;
        phaseInfluence = 30;
        phaseHotseat = 30;
        phaseVoteAgain = 7;
        hotseatCount = 1;
        influenceCount = 2;
        currentPromptDictKeyList = GameControl.control.currentPromptsDict.Keys.ToList();
        // this method sets slider parent, slider name text
        SetPlayerParent();
        // reference to local player object
        player = NetworkClient.connection.identity.GetComponent<GamePlayer>();
        // sets ui name text (bottom left)
        playerNameText.text = player.displayName;
        round = 0;

        // initial set up for first round
        currentPhase = "prompt";
        phaseText.text = "Phase:\nPrompt";
        promptText.text = GameControl.control.currentPromptsDict[currentPromptDictKeyList[round]][0];
        AText.text = GameControl.control.currentPromptsDict[currentPromptDictKeyList[round]][1];
        BText.text = GameControl.control.currentPromptsDict[currentPromptDictKeyList[round]][2];
        phasePrompt += (promptText.text.Length + AText.text.Length + BText.text.Length) / 400 * 15;
        Debug.Log("phasePrompt time: " + phasePrompt);
        hotseatCountText.text = "Hotseats: " + Convert.ToString(hotseatCount);
        influenceCountText.text = "Influences: " + Convert.ToString(influenceCount);
        myTimer.SetTimer(phasePrompt);
        SetOtherPlayersInactive();
        GameControl.control.phaseOver = false;
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
    IEnumerator PauseCoroutinePrompt()
    {
        GameControl.control.phaseOver = false;
        player.CmdRate(player.displayName, Convert.ToInt32(player.mySlider.value));
        yield return new WaitForSeconds(0.5f);
        UpdateRatings();
        currentPhase = "action";
        myTimer.SetTimer(phaseAction);
        SetOtherPlayersActive();
        phaseText.text = "Phase:\nPower";
    }

    IEnumerator PauseCoroutineVoteAgain()
    {
        GameControl.control.phaseOver = false;
        player.CmdRate(player.displayName, Convert.ToInt32(player.mySlider.value));
        yield return new WaitForSeconds(0.5f);
        UpdateRatings(round);
        currentPhase = "action";
        myTimer.SetTimer(phaseAction);
        SetOtherPlayersActive();
        phaseText.text = "Phase:\nAction";
    }

    void Update()
    {

        if (influenceBool) 
        {
            phaseText.text = "Phase:\n" + currentPlayer + " is influencing";
            currentPhase = "influence";
            myTimer.SetTimer(phaseInfluence);
            ResetPowerBools();
        }
        else if (hotseatBool)
        {
            phaseText.text = "Phase:\n" + currentPlayer + " is hotseating someone";;
            currentPhase = "hotseat";
            myTimer.SetTimer(phaseHotseat);
            ResetPowerBools();
        }

        if (GameControl.control.phaseOver == true)
        {
            if (currentPhase == "prompt")
            {
                StartCoroutine(PauseCoroutinePrompt());
            }
            else if (currentPhase == "action")
            {
                round++;
                currentPhase = "prompt";
                phaseText.text = "Phase:\nPrompt";
                phasePrompt = 10;
                promptText.text = GameControl.control.currentPromptsDict[currentPromptDictKeyList[round]][0];
                AText.text = GameControl.control.currentPromptsDict[currentPromptDictKeyList[round]][1];
                BText.text = GameControl.control.currentPromptsDict[currentPromptDictKeyList[round]][2];
                phasePrompt += ((promptText.text.Length + AText.text.Length + BText.text.Length) / 400 * 15);
                hotseatCountText.text = "Hotseats: " + Convert.ToString(hotseatCount);
                influenceCountText.text = "Influences: " + Convert.ToString(influenceCount);
                myTimer.SetTimer(phasePrompt);
                SetOtherPlayersInactive();
                GameControl.control.phaseOver = false;
            }
            else if  (currentPhase == "influence")
            {
                currentPhase = "vote again";
                phaseText.text = "Phase:\n Change Vote";
                myTimer.SetTimer(phaseVoteAgain);
                GameControl.control.phaseOver = false;
                SetOtherPlayersInactive();
            }
            else if (currentPhase == "hotseat")
            {
                currentPhase = "action";
                myTimer.SetTimer(phaseAction);
                SetOtherPlayersActive();
                GameControl.control.phaseOver = false;
                phaseText.text = "Phase:\nAction";
            }
            else if (currentPhase == "vote again")
            {
                StartCoroutine(PauseCoroutineVoteAgain());
            }
        }

        if (round > 4)
        {
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
        if (influenceCount > 0 && currentPhase == "action" & myTimer.timeRemaining > 0.5)
        {
            player.CmdSendPower("influence");
            influenceCount --;
            influenceCountText.text = "Influences: " + Convert.ToString(influenceCount);
        }
        
    }

    public void sendPowerHotseat()
    {
        if (hotseatCount > 0 && currentPhase == "action" & myTimer.timeRemaining > 0.5)
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
