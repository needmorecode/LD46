﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartMachine : Singleton<HeartMachine>
{
    public bool Paused { get; set; }
    public float Vitality { get; private set; }

    [SerializeField]
    private float vitalityDecayRate;

    [SerializeField]
    private float deathTime = 5f;

    [SerializeField]
    private AudioSource flatlineSource;

    private float beepTimer;

    private float deathTimer;

    private void Start()
    {
        Vitality = 1f;
        PlayerStateManager.Instance.StateChanged += OnStateChanged;
    }

    void Update()
    {
        if (!Paused)
        {
            Vitality = Mathf.Clamp(Vitality - (vitalityDecayRate * Time.deltaTime), 0f, 1f);
        }

        if (Vitality <= 0f)
        {
            flatlineSource.enabled = true;

            deathTimer += Time.deltaTime;
            if (deathTimer > deathTime)
            {
                Die();
            }
        }
        else
        {
            deathTimer = 0f;
            flatlineSource.enabled = false;

            beepTimer += Time.deltaTime;
            if (beepTimer > Mathf.Max(0.1f, Vitality * 2f))
            {
                beepTimer = 0f;
                AudioController.Instance.PlaySound2D("short_beep", volume: 0.1f);
            }
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.H))
        {
            Boost();
        }
#endif
    }

    public void Boost()
    {
        Vitality = Mathf.Clamp(Vitality + 0.1f, 0f, 1f);
    }

    private void Die()
    {
        AudioController.Instance.PlaySound2D("blast");
        flatlineSource.enabled = false;
        enabled = false;
    }

    private void OnStateChanged(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.PumpDown:
                TrainProgressManager.Instance.Anim.Play("pump", 0, 0f);
                AudioController.Instance.PlaySound2D("crunch_short_1", pitch: new AudioParams.Pitch(0.6f + (Vitality * 0.6f)));
                Boost();
                break;
        }
    }
}
