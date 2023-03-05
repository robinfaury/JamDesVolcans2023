using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score
{
    public Dictionary<Level, int> scoresByLevel = new Dictionary<Level, int>();

    public void NewGame()
    {
        scoresByLevel.Clear();
    }

    
}
