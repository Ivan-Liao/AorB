using UnityEngine;
using UnityEngine.UI;
using System;

public class UIController : MonoBehaviour
{
    public GamePlayer myGamePlayer;
    public Slider ratingSlider;
    public Text ratingSliderText;
    public Text phaseText;

    public Slider Slider1;
    public Slider Slider2;
    public Slider Slider3;
    public Slider Slider4;
    public Slider Slider5;
    public Slider Slider6;
    public Slider Slider7;
    
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

    // unimplemented rating step button code, had some bug where it makes slider value jump
    // public void SliderStepUp()
    // {
    //     if (ratingSlider.value < 5)
    //     {
    //         ratingSlider.value ++;
    //         DisplayRating(Convert.ToInt32(ratingSlider.value));
    //     }

    // }

    // public void SliderStepDown()
    // {
    //     if (ratingSlider.value > -5)
    //     {
    //         ratingSlider.value --;
    //         DisplayRating(Convert.ToInt32(ratingSlider.value));
    //     }
    // }

    public void SliderChange(Text sliderText) 
    {
        DisplayRating(Convert.ToInt32(ratingSlider.value), sliderText);
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


}
