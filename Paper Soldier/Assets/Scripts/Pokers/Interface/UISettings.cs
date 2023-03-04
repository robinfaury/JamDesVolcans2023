using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu (menuName = "UISettings", fileName = "UISettings")]
public class UISettings : ScriptableObject
{

    // =========================================== VARIABLES 

    [Header("BUMP")]
    public float bumpDuration;
    public AnimationCurve bumpCurve;

    [Header ("FADE")]
    public float fadeDuration;
    public AnimationCurve fadeCurve;


    // =========================================== STATIC

    static UISettings _current;

    public static UISettings current { 
        get {
            if (_current == null) _current = (UISettings)Resources.Load<UISettings>("UISettings");
            return _current;
        } 
    }

}