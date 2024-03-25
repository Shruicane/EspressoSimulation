using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tank_Size : MonoBehaviour
{

    private EspressoMachine espressoMachine;
    
    private bool _running;

    //Global Min & Max of all available Datasets
    private float _minPressure;
    private float _maxPressure;

    //Unity Event Methods
    private void Awake()
    {
        espressoMachine = GetComponentInParent<EspressoMachine>();
        //subscribe to Event
        Manager.OnSimulationStateChanged += OnManagerSimulationStateChanged;
    }

    void Start()
    {
        //Reset Size
        SetSize(0);
        float[] minMaxPressure = Manager.getGlobalMinMaxOf(Listtype.Pressure);
        _minPressure = minMaxPressure[0];
        _maxPressure = minMaxPressure[1];
        
//        Debug.Log("TankSize: Min: " + _minPressure + " \nMax: " + _maxPressure);
    }

    private void OnDestroy()
    {
        //Unsubscribe from Event and reset Hue
        Manager.OnSimulationStateChanged -= OnManagerSimulationStateChanged;
        SetSize(0);
    }

    /// <summary>
    /// Gets a scale between 0 and 1
    /// </summary>
    /// <param name="scale"></param>
    private void SetSize(float scale)
    {
        // Debug.Log("Scaled Pressure: " + scale );
        if (scale < 0 || scale > 1)
        {
            Console.WriteLine("ERROR: No Valid scale!");
        }
        // Scale from 0.5 to 1
        transform.localScale = new Vector3((scale * 9 / 10) + 0.1f, 1, (scale * 9 / 10) + 0.1f);
    }

    private Vector3 GetSize() => transform.localScale;

    // Methode wird aufgerufen wenn sich der SimulationState �ndert
    private void OnManagerSimulationStateChanged(SimulationState newState)
    {
        _running = newState == SimulationState.SimulationRunning;

        if (newState == SimulationState.SimulationReset)
        {
            SetSize(0);
        }
    }

    // FixedUpdate is called 50 times per second
    void FixedUpdate()
    {
        //increase _simulationTime 
        if (_running)
        {
            float pressure = Manager.getValue(Manager.Instance.GetCurrentData()[espressoMachine.machineID], Manager.Instance.GetSimulationTime(), Listtype.Pressure);
           // Debug.Log("Pressure: " + pressure);
            if (pressure >= 0)
                SetSize(normaliseValue(pressure));
        }
    }
    private float normaliseValue(float val)
    {
        return ((val - _minPressure) / (_maxPressure - _minPressure));
    }




}
