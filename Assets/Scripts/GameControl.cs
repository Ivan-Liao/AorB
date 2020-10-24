using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Mirror;
using System.IO;

public class GameControl : MonoBehaviour
{
    public static GameControl control;
    public Dictionary<string, List<int>> AllRatings;

    public Dictionary<int, List<string>> AllPromptsDict;
    public Dictionary<int, List<string>> currentPromptsDict;
    public int promptCount = 12;
    public int numCurrentPrompts = 5;
    public bool phaseOver = false;


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

        NetworkLobbyPlayer.OnPromptLoad += LoadCurrentPrompts;

        TSVtoDict();
    }

    // Needs to be optimized currently does this multiple times once for each client
    public void LoadCurrentPrompts(NetworkLobbyPlayer player, List<int> randIntList)
    {
        Debug.Log("gamecontrol loadprompt executes " + randIntList.Count);
        currentPromptsDict = new Dictionary<int, List<string>>();
        foreach (int randInt in randIntList)
        {
            currentPromptsDict.Add(randInt, AllPromptsDict[randInt]);  
        }
    }


    void TSVtoDict()
    {
        TextAsset allPromptsTextAsset = Resources.Load<TextAsset>("AorB");
        if (allPromptsTextAsset != null)
        {
            AllPromptsDict = new Dictionary<int, List<string>>();
            using (StreamReader sr = new StreamReader(new MemoryStream(allPromptsTextAsset.bytes)))
            {
                for (int i = 0; i < promptCount; i++)
                {
                    string line = sr.ReadLine();
                    // processes line 
                    string[] splitLine = line.Split("\t".ToCharArray());
                    AllPromptsDict.Add(i,new List<string>());
                    AllPromptsDict[i].Add(splitLine[0]);
                    AllPromptsDict[i].Add(splitLine[1]);
                    AllPromptsDict[i].Add(splitLine[2]);
                }
            }
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

    // this overload method replaces the rating for the specific round, used in vote again phase
    public void UpdateAllRatings(string pName, int rating, int round)
    {
        AllRatings[pName][round] = rating;
        foreach (string key in GameControl.control.AllRatings.Keys.ToList())
        {
            Debug.Log("key= " + key);
            Debug.Log("value =" + GameControl.control.AllRatings[key][0]);
        }
    }
}
