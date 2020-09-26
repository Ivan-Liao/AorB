using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

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

    public void SaveRoundRatings(List<string> pNames, List<int> pRatings)
    {
        for (int i = 0; i < pNames.Count; i++)
        {
            if (AllRatings.ContainsKey(pNames[i]))
            {
                AllRatings[pNames[i]].Add(pRatings[i]);
            }
            else
            {
                AllRatings.Add(pNames[i], new List<int>());
                AllRatings[pNames[i]].Add(pRatings[i]);
            }
        }
    }
}
