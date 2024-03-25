using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TemperatureColor : MonoBehaviour
{
    public Material matBruehkopf;

    private EspressoMachine espressoMachine;
    private bool _running;
    private static readonly int Hue = Shader.PropertyToID("_Hue");
    private static readonly int Active = Shader.PropertyToID("_Active");

    //Global Min & Max of all available Datasets
    private float _minTemp;
    private float _maxTemp;
    private float _minHue;

    //Unity Event Methods
    private void Awake()
    {
        espressoMachine = GetComponentInParent<EspressoMachine>();
        _minHue = -120;

        //subscribe to Event
        Manager.OnSimulationStateChanged += OnManagerSimulationStateChanged;
    }

    void Start()
    {
        ResetHue();
        
        //Get Global Min & Max
        float[] minTemp = Manager.getGlobalMinMaxOf(Listtype.BasketTemperature);
        _minTemp = minTemp[0];
        _maxTemp = minTemp[1];

    }

    private void OnDestroy()
    {
        //Unsubscribe from Event and reset Hue
        Manager.OnSimulationStateChanged -= OnManagerSimulationStateChanged;
        ResetHue();
    }

    //Setter & Getter
    private void SetHue(float hue)
    {
        matBruehkopf.SetFloat(Active, 1f);
        matBruehkopf.SetFloat(Hue, hue);
    }

    private void ResetHue()
    {
        matBruehkopf.SetFloat(Active, 0f);
        matBruehkopf.SetFloat(Hue, 0f);
    }

    //Methode wird aufgerufen wenn sich der SimulationState ändert
    private void OnManagerSimulationStateChanged(SimulationState newState)
    {
        _running = newState == SimulationState.SimulationRunning;

        if (newState == SimulationState.SimulationReset)
        {
            ResetHue();
        }
    }

    // FixedUpdate is called 50 times per second
    void FixedUpdate()
    {
        //increase _simulationTime 
        if (_running)
        {
            float temperature = Manager.getValue(Manager.Instance.GetCurrentData()[espressoMachine.machineID], Manager.Instance.GetSimulationTime(), Listtype.BasketTemperature);
            
            if (temperature  >= 0)
            {
                //nur wenn Temperatur nicht -1
                SetHue(CalculateHue(temperature));
            }
            else
            {
                matBruehkopf.SetFloat(Active, 0f);
            }
        }
    }

    private float CalculateHue(float val)
    {
        //0 = Grün - -120 = Rot
        float res = ((val - _minTemp) / (_maxTemp - _minTemp)) * _minHue;
        //Debug.Log("Min: " + _minTemp + " Max: " + _maxTemp + " Val: " + val + " Res: " + res);
        return res;
    }
}