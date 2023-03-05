using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static GameManager;

public class Score  
{
    public class Data
    {
        public int bulletUsed = 0;
        public int tickCount = 0;
        public int tryCount = 1;
    }


    public Dictionary<Level, Data> scoresByLevel = new Dictionary<Level, Data>();

    public void NewGame()
    {
        scoresByLevel.Clear();
        for(int i = 0; i < g_levels.Count; i++)
        {
            scoresByLevel.Add(g_levels[i], new Data());
        }
        TickManager.onTick += OnTick;
        g_onPlayerDeath += OnResetLevel;
    }

    public void OnBulletUsed()
    {
        if(scoresByLevel.ContainsKey(g_currentLevel))
        {
            scoresByLevel[g_currentLevel].bulletUsed++;
        }
    }

    public void OnTick()
    {
        if (scoresByLevel.ContainsKey(g_currentLevel))
        {
            scoresByLevel[g_currentLevel].tickCount++;
        }
    }

    public void OnResetLevel()
    {
        if (scoresByLevel.ContainsKey(g_currentLevel))
        {
            scoresByLevel[g_currentLevel].tickCount = 0;
            scoresByLevel[g_currentLevel].bulletUsed = 0;
            scoresByLevel[g_currentLevel].tryCount++;
        }
    }


    public void DebugScore(Level lvl = null)
    {
        if (lvl == null)
            lvl = g_currentLevel;

        if (lvl == null
            || !scoresByLevel.ContainsKey(lvl))
            return;

        Debug.Log("Try : " + scoresByLevel[lvl].tryCount);
        Debug.Log("bullet used : " + scoresByLevel[lvl].bulletUsed + "/" + lvl.optimalBulletScore);
        Debug.Log("tick count used : " + scoresByLevel[lvl].tickCount+ "/" + lvl.optimalTravelTickCount);



    }




}
