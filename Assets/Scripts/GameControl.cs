using System;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class GameControl : MonoBehaviour
{
    public static GameControl control;

    // sets up singleton control object
    void Awake()
    {
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
}
