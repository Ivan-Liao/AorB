using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Mirror;

public class GameControl : MonoBehaviour
{
    public static GameControl control;
    public Dictionary<string, List<int>> AllRatings;

    // sets up singleton control object
    void Awake()
    {
        AllRatings = new Dictionary<string, List<int>>();
        if (control == null)
        {
            DontDestroyOnLoad(gameObject);
            control = this;
        }
        else if (control != this)
        {
            Destroy(gameObject);
        }
    }

    public void UpdateAllRatings(string pName, int rating)
    {
        if (AllRatings.ContainsKey(pName))
        {
            AllRatings[pName].Add(rating);
        }
        else
        {
            AllRatings.Add(pName, new List<int>());
            AllRatings[pName].Add(rating);
        }
        foreach (string key in GameControl.control.AllRatings.Keys.ToList())
        {
            Debug.Log("key= " + key);
            Debug.Log("value =" + GameControl.control.AllRatings[key][0]);
        }
    }
}
