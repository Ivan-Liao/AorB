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
public class GamePlayer : NetworkBehaviour
{
    public static event Action<GamePlayer, int> OnRate;


    [SyncVar]
    public string displayName = "Loading...";
    [SyncVar]
    public int rating;
    [SyncVar]
    public double points;
    public Slider mySlider;
    public Text mySliderRatingText;
    public Text MySliderNameText;

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

    // Update is called once per frame

    [Command]
    public void CmdRate()
    {
        Debug.Log("cmdrate executes");
        RpcReceiveRate(Convert.ToInt32(rating));
    }

    [ClientRpc]
    public void RpcReceiveRate(int rating)
    {
        OnRate?.Invoke(this, rating);
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
    public void SliderChange(Text sliderText) 
    {
        DisplayRating(Convert.ToInt32(mySlider.value), sliderText);
        mySlider.value = mySlider.value;
    }
}

