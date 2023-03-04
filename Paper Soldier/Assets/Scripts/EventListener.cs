using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class EventListener : MonoBehaviour
{

    public System.Action<string> onEvent;

    public void Event (AnimationEvent animEvent)
    {
        onEvent?.Invoke(animEvent.stringParameter);
    }
}