using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyClock : MonoBehaviour
{
    //计时器应该有的功能

    private float currenttime;
    public float duration;

    public enum State
    {
        Idle,
        Start,
        Finished
    }
    public State state { get; set; }

    private void Start()
    {
        state = State.Idle;
    }

    private void Update()
    {
        if (state == State.Idle)
        {
            currenttime = 0f;
        }
        else if (state == State.Start)
        {
            currenttime += Time.deltaTime;
            if (currenttime > duration)
            {
                ChangeState(State.Finished);
            }
        }
        else if (state == State.Finished)
        {
            ChangeState(State.Idle);
        }

    }

    public void ChangeState(State state)
    {
        this.state = state;
    }



    public void StartClock(float duration)
    {
        this.duration = duration;
        this.ChangeState(State.Start);
    }

}
