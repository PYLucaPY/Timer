using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public delegate void OnTimerCompletion();

    public enum TimerType
    {
        Frames, Seconds
    }

    private class TimerContainer
    {
        public float startingTime;
        public float duration;

        public Timer.TimerType timerType = Timer.TimerType.Seconds;
        public bool beenSet = false;

        public bool hasFinishedDelegate;
        public OnTimerCompletion onFinishedTimer;

        public bool previousTimerFinishedState = false;

        public float GetTimeSince()
        {
            float _c = timerType == Timer.TimerType.Frames ? Time.frameCount : Time.time;

            return _c - startingTime;
        }
    }

    private static Dictionary<string, TimerContainer> states = new Dictionary<string, TimerContainer>();
    
    public static void ResetTimer(string name)
    {
        if(!ContainsTimer(name) || !TimerSet(name))
        {
            Debug.LogError(GetInvalidTimer(name, true));
            return;
        }

        states[name].startingTime = 
            states[name].timerType == TimerType.Seconds ? Time.time : Time.frameCount;
    }

    public static bool TimerRunning(string name)
    {
        if(!ContainsTimer(name) || !TimerSet(name))
        {
            Debug.LogError(GetInvalidTimer(name, true));
            return false;
        }

        return states[name].GetTimeSince() < states[name].duration;
    }

    public static float GetTimeSince(string name)
    {
        if(!ContainsTimer(name) || !TimerSet(name))
        {
            Debug.LogError(GetInvalidTimer(name, true));
            return -1;
        }

        return states[name].GetTimeSince();
    }

    public static void RemoveOnTimerFinished(string name)
    {
        if(!ContainsTimer(name) || !TimerSet(name))
        {
            Debug.LogError(GetInvalidTimer(name, true));
            return;
        }

        states[name].hasFinishedDelegate = false;
    }

    public static void SetOnTimerFinished(string name, OnTimerCompletion finishingDelegate)
    {
        if(!ContainsTimer(name) || !TimerSet(name))
        {
            Debug.LogError(GetInvalidTimer(name, true));
            return;
        }

        states[name].hasFinishedDelegate = true;
        states[name].onFinishedTimer = finishingDelegate;
    }

    public static void SetTimer(string name, float timeValue, float duration, 
        TimerType _tt = TimerType.Seconds)
    {
        if(!ContainsTimer(name))
        {
            Debug.LogError(GetInvalidTimer(name));
            return;
        }

        states[name].startingTime = timeValue;
        states[name].duration = duration;
        states[name].timerType = _tt;

        states[name].beenSet = true;
    }

    public static void AddTimer(string name)
    {
        if(ContainsTimer(name))
        {
            Debug.LogError($"Cannot add {name} as new timer. {name} already exists.");
            return;
        }

        TimerContainer tc = new TimerContainer();
        states.Add(name, tc);
    }

    public static bool ContainsTimer(string name)
    {
        return states.ContainsKey(name);
    }

    public static bool TimerSet(string name)
    {
        return states[name].beenSet;
    }

    private static string GetInvalidTimer(string name, bool useSet = false)
    {
        string result = $"Cannot complete operation. \"{name}\" is not a existing timer";
        if(useSet)
        {
            result += " or has not been set.";
        } else {
            result += ".";
        }

        return result;
    }

    private void Update()
    {
        foreach(KeyValuePair<string, TimerContainer> entry in states)
        {
            bool timerFinished = TimerRunning(entry.Key);
            if(!timerFinished &&
                states[entry.Key].previousTimerFinishedState)
            {
                if(entry.Value.hasFinishedDelegate)
                    entry.Value.onFinishedTimer();
            }

            states[entry.Key].previousTimerFinishedState = timerFinished;
        } 
    }
}
