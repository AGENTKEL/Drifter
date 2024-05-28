using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class Timer : NetworkBehaviour
{
    public TextMeshProUGUI countDownText;
    [Networked]
    byte countDown {get; set;}
    public bool isSessionEnded = false;

    public TickTimer countDownTickTimer = TickTimer.None;

    ChangeDetector _changeDetector;

    private void Start() 
    {
        countDownText.text = " ";
    }

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        countDownTickTimer = TickTimer.CreateFromSeconds(Runner, 180);
    }

    public override void FixedUpdateNetwork()
    {
        if (countDownTickTimer.Expired(Runner))
        {
            countDownTickTimer = TickTimer.None;
            if (Runner.IsServer)
            {
                RPC_TriggerResults();
            }
        }
        else if (countDownTickTimer.IsRunning)
        {
            countDown = (byte)countDownTickTimer.RemainingTime(Runner);
        }
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(countDown):
                    OnCountDownChanged();
                    break;

            }
        }
    }

    private void OnCountDownChanged()
    {
        if (countDown == 0)
        {
            countDownText.text = $"";
        }
        else
        {
            int minutes = countDown / 60;
            int seconds = countDown % 60;
            countDownText.text = $"{minutes}:{seconds:00}";
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TriggerResults()
    {
        ActivateCarControllers();
    }

    private void ActivateCarControllers()
    {
        CarController[] carControllers = FindObjectsOfType<CarController>();
        foreach (CarController carController in carControllers)
        {
            carController.ConvertDriftPointsToMoney();
        }
    }

}
