using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

/// <summary>
/// Reset: Ausgangszustand
/// Running: Simulation läuft
/// Stopped: Simulation Angehalten
/// Finished: Datenreihe zu Ende
/// </summary>
public enum SimulationState { SimulationReset, SimulationRunning, SimulationStopped, SimulationFinished }

public class Manager : MonoBehaviour
{

	public static Manager Instance;

    private bool _manualRun;
    private List<Data> _data;
    private SimulationState _simulationState;
    private int _selectedFileIndex1;
    private int _selectedFileIndex2;
    private string _resourcePath;
    private float _simulationTime;
    private List<float[]> _lookUpTable = new List<float[]>();
    private Listtype _graphDataListtype;
    private bool[] _machinesFinished = new bool[]{false, false};
    private bool _twoMachinesRunning;

    //Events
    public static event Action<SimulationState> OnSimulationStateChanged; 
    
    //Getter & Setter
    public bool IsManualRun()
    {
        return _manualRun;
    }
    public void SetManualRun(bool manualRun)
    {
        _manualRun = manualRun;
        UpdateSimulationState(SimulationState.SimulationRunning);
    }
    public List<Data> GetData()
    {
        return _data;
    }

    public Data[] GetCurrentData()
    {
        return new Data[2] {_data[_selectedFileIndex1], _data[_selectedFileIndex2]};
    }

    public void SetCurrentData1(int index)
    {
        _selectedFileIndex1 = index;
    }
    
    public void SetCurrentData2(int index)
    {
        _selectedFileIndex2 = index;
    }

    public float GetSimulationTime()
    {
        return _simulationTime;
    }

    public void SetSimulationTime(float time)
    {
        this._simulationTime = time;
    }

    public void SetGraphdataListtype(Listtype listtype)
    {
        _graphDataListtype = listtype;
    }

    public Listtype GetGraphdataListtype()
    {
        return _graphDataListtype;
    }

    public float[] GetLookUpTable(Listtype listtype)
    {
        switch (listtype)
        {               
            case Listtype.Pressure:
                return _lookUpTable[0];
            case Listtype.Flow:
                return _lookUpTable[1];
            case Listtype.BasketTemperature:
                return _lookUpTable[2];
            case Listtype.MixTemperature:
                return _lookUpTable[3];
            default:
                Debug.LogError(" Kein Valider Listtype");
                return _lookUpTable[0];
        }

    }
    
    /// <summary>
    /// Add Entry to Data-Object List
    /// </summary>
    /// <param name="e"></param>
    public void AddData(Data e)
    {
        _data.Add(e);
    }
    
    /// <summary>
    /// Get Current SimulationState
    /// </summary>
    /// <returns></returns>
    public SimulationState GetSimulationState()
    {
        return _simulationState;
    }

    public float GetMaxTimeElapsed()
    {

        if (IsTwoMachinesRunning())
        {
            float maxTime1 = GetCurrentData()[0].getList(Listtype.Elapsed).Last();
            float maxTime2 = GetCurrentData()[1].getList(Listtype.Elapsed).Last();
            return ((maxTime1 > maxTime2) ? maxTime1 : maxTime2);    
        }
        else
        {
            return GetCurrentData()[0].getList(Listtype.Elapsed).Last();
        }
        
    }
    
    //Unity Event Functions
    private void Awake()
    {
        //Init Data-Object List
        _data = new List<Data>();
        _resourcePath = "./Assets/Scripts/src/resources/";
        _selectedFileIndex1 = 0;
        _graphDataListtype = Listtype.Flow;
        OnSimulationStateChanged += OnManagerSimulationStateChanged;
        
        //Make sure only One Instance of Manager Class exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            //If Instance already exists, destroy
            Destroy(this.gameObject);
            Debug.Log("Duplicate Manager Instance. Did you attach the Manager script to multiple GameObjects?");
        }
        InitData();
    }

    private void OnDestroy()
    {
        OnSimulationStateChanged -= OnManagerSimulationStateChanged;
    }

    void Start()
    {
        //Init Simulationstate
        UpdateSimulationState(SimulationState.SimulationReset);
    }

    private void FixedUpdate()
    {
        //TODO: Testen ob die Zeit abgelaufen ist und Simulation Finished feuern (wenn kein Manualrun)
        if (GetSimulationState() == SimulationState.SimulationRunning && ! _manualRun)
        {
            _simulationTime += 1f / 50f;

            if (_simulationTime > GetMaxTimeElapsed())
            {
                UpdateSimulationState(SimulationState.SimulationFinished);
            }
        }
    }

    private void OnManagerSimulationStateChanged(SimulationState newState)
    {
        //Reset Simulationtime
        if (newState == SimulationState.SimulationReset)
        {
            _simulationTime = 0;
        }
    }

    /// <summary>
    /// Update SimulationState.
    /// </summary>
    /// <param name="newState"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void UpdateSimulationState(SimulationState newState)
    {
        switch (newState)
        {
            case SimulationState.SimulationReset: 
                break;
            case SimulationState.SimulationRunning: 
                break;
            case SimulationState.SimulationStopped: 
                break;
            case SimulationState.SimulationFinished:
                if (IsManualRun())
                {
                    return;
                }
                break;
            default: 
                throw new ArgumentOutOfRangeException(nameof(newState), newState, "Invalid SimulationState or SimulationState not implemented.");
        }
        //assign new SimulationState
        _simulationState = newState;
        
        //Trigger SimulationChangedEvent (if anyone subscribed) 
        OnSimulationStateChanged?.Invoke(newState);
    }

    /// <summary>
    /// Create Data Objects from all JSON Files and add them to the _data list
    /// </summary>
    private void InitData()
    {
        string[] files = Directory.GetFiles(_resourcePath, "*.json");
        foreach (var file in files)
        {
            if (! file.Contains("Prep"))
            {
                AddData(new Data(file));
            }
        }
        initLookUpTable();
    }

    /// <summary>
    /// Gives back the global Min and Max of a specified listtype
    /// </summary>
    /// <param name="listtype"></param>
    /// <returns></returns>
    public static float[] getGlobalMinMaxOf(Listtype listtype)
    {
        float[] minMax = new float[2];
        minMax[0] = Manager.Instance.GetData()[0].getMinMax(listtype)[0];
        minMax[1] = 0;

        foreach (Data data in Manager.Instance.GetData())
        {
            if (data.getMinMax(listtype)[0] < minMax[0])
            {
                minMax[0] = data.getMinMax(listtype)[0];
            }

            if (data.getMinMax(listtype)[1] > minMax[1])
            {
                minMax[1] = data.getMinMax(listtype)[1];
                //Debug.Log(data.GetName());
            }
        }
        return minMax;
    }

    /// <summary>
    /// gives All Global Min and Max Values back
    /// </summary>
    /// <returns>List of Float Arrays: Pressure, Flow, BasketTemperature, Mix Temperature</returns>
    public static List<float[]> getGlobalMinMax()
    {
        List<float[]> globalMinMax = new List<float[]>();
        List<Listtype> typeArr = new List<Listtype> { Listtype.Pressure, Listtype.Flow, Listtype.BasketTemperature, Listtype.MixTemperature };
        foreach (Listtype type  in typeArr)
        {
            globalMinMax.Add(getGlobalMinMaxOf(type));
        }

        return globalMinMax;
    }

    /// <summary>
    /// Initialisiert eine Liste von transformierten Werten eines Listtyps.
    /// </summary>
    /// <param name="listtype"></param>
    /// <returns>Dabei gilt: Index = Wert - Min (Kaufmännisch gerundet)</returns>
    private void initLookUpTable()
    {
        List<Listtype> typeArray = new List<Listtype> { Listtype.Pressure, Listtype.Flow, Listtype.BasketTemperature, Listtype.MixTemperature };
        foreach (Listtype listtype in typeArray)
        {
            float[] globalMinMax = getGlobalMinMaxOf(listtype);

            //Erstellt Float Array mit allen möglichen DISKRETEN Werten.
            int min = (int)(globalMinMax[0] + 0.5f);
            int max = (int)(globalMinMax[1] + 0.5f);

           

            int[] countList = new int[max - min + 1];
            int count = 0;
            float[] freqList = new float[max - min + 1];


            /*Debug.Log("Listtype: " + listtype);
            Debug.Log("Min: " + min + " \nMax: " + max);
            Debug.Log("Size of Array:" + countList.Length +  "," + freqList.Length);
            Debug.Log("GlobalMinMax: " + globalMinMax[1] + " " + globalMinMax[0]);*/

            // Berechnet die Absoluten Häufigkeiten 
            foreach (Data data in Manager.Instance.GetData())
            {
                foreach (float el in data.getList(listtype))
                {
                    // Debug.Log("Weird Value: " + (((int)(el + 0.5f)) - min));
                    countList[((int)(el + 0.5f)) - min]++;
                    count++;
                }
            }

            // Debug.Log("Count: " + count);

            // Berechnet die kummulierten relativen Häufigkeiten
            int index = 0;
            foreach (int c in countList)
            {
               //  Debug.Log("c: " + c + "index: " + index);
                if(index == 0)
                {
                    freqList[index] = (float)c / (float)count;
                    // Debug.Log("fequency: " + (float)c / (float)count);
                }
                else
                {
                    freqList[index] = freqList[index-1] + (float)c / (float)count;
                    // Debug.Log("FreqList: " + freqList[index]);
                }
                index++;
            }

            // Berechnet die neuen Werte
            for (int i = 0; i < freqList.Length; i++)
            {
                freqList[i] *= globalMinMax[1] - globalMinMax[0];
                freqList[i] += globalMinMax[0];
            }
            _lookUpTable.Add(freqList);
        }
    }

    private static float getScaledData(float val, Listtype listtype)
    {
        int index = (int)(val + 0.5f) - (int)(getGlobalMinMaxOf(listtype)[0] + 0.5f);
        // Debug.Log("ScaledData: " + Instance.GetLookUpTable(listtype)[index]);
        return Instance.GetLookUpTable(listtype)[index];
    }
    /// <summary>
    /// Bekommt den Datensatz, die Zeit und den Typen des gewünschten Wertes und gibt einen interpolierten Wert zurück.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="time"></param>
    /// <param name="listtype"></param>
    /// <returns></returns>
    public static float getValue(Data data, float time, Listtype listtype, bool scaled = true)
    {
        float[] timeInterval = data.getTimeInterval(time);
        if (timeInterval[1] == -1)
            return -1;

        // Identify time interval
        int[] index = new int[2];
        index[0] = data.getList(Listtype.Elapsed).IndexOf(timeInterval[0]);
        index[1] = data.getList(Listtype.Elapsed).IndexOf(timeInterval[1]);

        float normTime = (time - timeInterval[0]) / (timeInterval[1] - timeInterval[0]);

        /*
        Debug.Log("normTime: " + normTime);
        Debug.Log("ListValues: " + data.getList(listtype)[index[0]] + " " + data.getList(listtype)[index[1]]);
        */
        // Interval of type 
        float lowerBound, upperBound;
        if (scaled){
            lowerBound = getScaledData(data.getList(listtype)[index[0]], listtype);
            upperBound = getScaledData(data.getList(listtype)[index[1]], listtype);
        }else{
            lowerBound = data.getList(listtype)[index[0]];
            upperBound = data.getList(listtype)[index[1]];
        }


        float value = normTime * (upperBound - lowerBound) + lowerBound;
        // Debug.Log("Bounds: " + upperBound + " " + lowerBound + "\nValue: " + value);


        return value;
    }

    public void SetTwoMachinesRunning(bool val)
    {
        _twoMachinesRunning = val;
    }

    public bool IsTwoMachinesRunning()
    {
        return _twoMachinesRunning;
    }
}
