using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;

[Serializable]
public class SyncDictionaryRoundRatings : SyncDictionary<string, int> {}
[Serializable]
public class GamePlayer : NetworkBehaviour
{
    public GameObject gameUI = null;
    public Timer myTimer;
    public Slider mySlider;
    public Text mySliderText;
    public Text phaseText;
    public Slider Slider1;
    public Slider Slider2;
    public Slider Slider3;
    public Slider Slider4;
    public Slider Slider5;
    public Slider Slider6;
    public Slider Slider7;
    public Text SliderNameText1;
    public Text SliderNameText2;
    public Text SliderNameText3;
    public Text SliderNameText4;
    public Text SliderNameText5;
    public Text SliderNameText6;
    public Text SliderNameText7;


    public string currentPhase;
    [SyncVar]
    public bool influenceBool;
    [SyncVar]
    public bool hotseatBool;
    [SyncVar]
    public string currentPlayer;
    [SerializeField] 
    public SyncDictionaryRoundRatings roundRatings;

    int phasePrompt;
    int phaseAction;
    int phaseInfluence;
    int phaseHotseat;
    int influenceCount;
    int hotseatCount;
    public int round;
    public Text influenceCountText;
    public Text hotseatCountText;
    public Text playerNameText;
    [SyncVar]

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

    public override void OnStartAuthority()
    {
        gameUI.SetActive(true);
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

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

    public async Task Start()
    {
        await Task.Delay(100);
        round = 0;
        // Starts the timer automatically
        for (int i = 0; i < 5; i++)
        {
            await GameRound();  
            round++;          
        }
    }

    public async Task GameRound() {
        roundRatings = new SyncDictionaryRoundRatings();
        phasePrompt = 4;
        phaseAction = 6;
        phaseInfluence = 8;
        phaseHotseat = 10;
        hotseatCount = 1;
        influenceCount = 2;

        currentPhase = "prompt";
        mySlider.enabled = true;
        myTimer.SetTimer(phasePrompt);
        phaseText.text = "Phase:\nPrompt";
        hotseatCountText.text = "Hotseats: " + Convert.ToString(hotseatCount);
        influenceCountText.text = "Influences: " + Convert.ToString(influenceCount);
        playerNameText.text = displayName;
        // this is in milliseconds so must be multipied by 1000
        await Task.Delay(phasePrompt * 1000);
        CollectRating();
        await ActionLoop();
        GameControl.control.SaveRoundRatings(roundRatings.Keys.ToList(), roundRatings.Values.ToList());
    }

    public async Task ActionLoop()
    {
        currentPhase = "action";
        SetOtherSlidersActive();
        //AllOtherSliderChange();
        mySlider.enabled = false;
        myTimer.SetTimer(phaseAction);
        phaseText.text = "Phase:\nPower";
        tokenSource = new CancellationTokenSource();
        try {
            await Task.Delay(phaseAction * 1000, tokenSource.Token);
            SetOtherSlidersInactive();
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

    // Update is called once per frame
    void Update()
    {

        if (influenceBool || hotseatBool) 
        {
            tokenSource.Cancel();
        }

    }
    [Command]
    void ResetPowerBools()
    {
        Room.ResetPowerBools();
    }

    
    [Command]
    public void CmdInfluencePhase()
    {
        if (influenceCount > 0)
        {
            influenceCount --;
            influenceCountText.text = "Influences: " + Convert.ToString(influenceCount);
            currentPlayer = displayName;
            Room.InfluencePhase(displayName);
        }

    }

    [Command]
    public void CmdHotseatPhase()
    {
        if (hotseatCount > 0)
        {
            hotseatCount --;
            hotseatCountText.text = "Hotseats: " + Convert.ToString(hotseatCount);
            currentPlayer = displayName;
            Room.HotseatPhase(displayName);
        }

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

        UpdateDisplay();
    }

    public void DisplayRating(int rating, Text sliderText) 
    {
        switch (rating)
        {
            case -5:
                sliderText.text = "-5 - Super Strong A";
                break;
            case -4:
                sliderText.text = "-4 - Strong A";
                break;
            case -3:
                sliderText.text = "-3 - High Moderate A";
                break;
            case -2:
                sliderText.text = "-2 - Low Moderate A";
                break;
            case -1:
                sliderText.text = "-1 - Slight A";
                break;
            case 0:
                sliderText.text = "0 - Neutral";
                break;
            case 1:
                sliderText.text = "1 - Slight B";
                break;
            case 2:
                sliderText.text = "2 - Low Moderate B";
                break;
            case 3:
                sliderText.text = "3 - High Moderate B";
                break;
            case 4:
                sliderText.text = "4 - Strong B";
                break;
            case 5:
                sliderText.text = "5 - Super Strong B";
                break;
        }
    }

    // this needs to be linked to the dictionary Ratings and playername needs to be added
    public void SliderChange(Text sliderText) 
    {
        DisplayRating(Convert.ToInt32(mySlider.value), sliderText);
    }

    public void AllOtherSliderChange()
    {
        // something wrong here
        // check if the correct ratings are being recorded in Ratings dictionary
        List<Slider> Sliders = new List<Slider>() {Slider1, Slider2, Slider3, Slider4, Slider5, Slider6, Slider7};
        List<Text> SliderNameTexts = new List<Text>() {SliderNameText1, SliderNameText2,SliderNameText3,SliderNameText4,SliderNameText5,SliderNameText6,SliderNameText7};
        List<string> ratingKeys = roundRatings.Keys.ToList();

        for (int i = 0; i < Room.GamePlayers.Count - 1; i++)
        {
            if (displayName == ratingKeys[0])
            {
                ratingKeys.RemoveAt(0);
            }
            SliderNameTexts[i].text = ratingKeys[0];
            Sliders[i].value = roundRatings[ratingKeys[0]];
            ratingKeys.RemoveAt(0);
        }

        for (int i = 6; i > Room.GamePlayers.Count - 2; i--)
        {
            Sliders[i].gameObject.SetActive(false);
        }
    }

    public void SetOtherSlidersActive()
    {
        Slider1.gameObject.SetActive(true);
        Slider2.gameObject.SetActive(true);
        Slider3.gameObject.SetActive(true);
        Slider4.gameObject.SetActive(true);
        Slider5.gameObject.SetActive(true);
        Slider6.gameObject.SetActive(true);
        Slider7.gameObject.SetActive(true);

        Slider1.enabled = false;
        Slider2.enabled = false;
        Slider3.enabled = false;
        Slider4.enabled = false;
        Slider5.enabled = false;
        Slider6.enabled = false;
        Slider7.enabled = false;
    }

    public void SetOtherSlidersInactive()
    {
        Slider1.gameObject.SetActive(false);
        Slider2.gameObject.SetActive(false);
        Slider3.gameObject.SetActive(false);
        Slider4.gameObject.SetActive(false);
        Slider5.gameObject.SetActive(false);
        Slider6.gameObject.SetActive(false);
        Slider7.gameObject.SetActive(false);
    }

    public void CollectRating()
    {
        string pName = displayName;
        int rating = Convert.ToInt32(mySlider.value);
        if (roundRatings.ContainsKey(pName))
        {
            roundRatings[pName] = rating;
        }
        else
        {
            roundRatings.Add(pName, rating);
        }
    }
}

