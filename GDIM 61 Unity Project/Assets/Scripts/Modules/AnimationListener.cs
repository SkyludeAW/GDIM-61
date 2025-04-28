using System.Collections.Generic;
using UnityEngine;

public class AnimationListener : MonoBehaviour {
    public delegate void AnimationKeypoint();
    public event AnimationKeypoint AnimationBegin;
    public event AnimationKeypoint AnimationEnd;
    public event AnimationKeypoint AttackTrigger;
    public event AnimationKeypoint AttackTriggerBegin;
    public event AnimationKeypoint AttackTriggerEnd;

    private void InvokeEvent(string eventName) {
        AnimationKeypoint eventToInvoke = null;

        // Determine which actual event field to invoke based on the string name
        switch (eventName) {
            case "AnimationBegin":
                eventToInvoke = AnimationBegin;
                // actionToInvoke = AnimationBeginAction;
                break;
            case "AnimationEnd":
                eventToInvoke = AnimationEnd;
                // actionToInvoke = AnimationEndAction;
                break;
            case "AttackTrigger":
                eventToInvoke = AttackTrigger;
                // actionToInvoke = AttackTriggerAction;
                break;
            case "AttackTriggerBegin":
                eventToInvoke = AttackTriggerBegin;
                // actionToInvoke = AttackTriggerBeginAction;
                break;
            case "AttackTriggerEnd":
                eventToInvoke = AttackTriggerEnd;
                // actionToInvoke = AttackTriggerEndAction;
                break;
            default:
                Debug.LogWarning($"[{gameObject.name}] Received unknown animation event name: {eventName}", gameObject);
                return; // Exit if the name is not recognized
        }
        eventToInvoke?.Invoke();
    }
}
