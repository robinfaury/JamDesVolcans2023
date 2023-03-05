using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour 
{


    public class Data
    {
        public int bulletUsed;
    }


    public Dictionary<Level, int> scoresByLevel = new Dictionary<Level, int>();

    public void NewGame()
    {
        scoresByLevel.Clear();
    }
        
}
