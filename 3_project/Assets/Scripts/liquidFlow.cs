using System.Collections;
using System.Diagnostics.Tracing;
using System.Linq;
using UnityEngine;

public class LiquidFlow : MonoBehaviour
{
    public ParticleSystem flowParticleSystemLeft;
    public ParticleSystem flowParticleSystemRight;

    private EspressoMachine espressoMachine;
    private bool _running = false;
    private int count = 0;
    private float _minFlow;
    private float _maxFlow;
    private bool wasRunningBefore = false;
    float simulationLength = 0;
    
    private void Awake()
    {
        espressoMachine = GetComponentInParent<EspressoMachine>();

        _minFlow = -120;
        _maxFlow = 0;

        //subscribe to Event
        Manager.OnSimulationStateChanged += OnManagerSimulationStateChanged;
        // Check if the ParticleSystem component is not null
        if (flowParticleSystemLeft == null)
        {
            Debug.LogError("Particle System component not found!");
        }
    }

    void Start()
    {
        //Get Global Min & Max
        float[] minmaxFlow = Manager.getGlobalMinMaxOf(Listtype.Flow);
        _minFlow = minmaxFlow[0];
        _maxFlow = minmaxFlow[1];
    }
    private void OnDestroy()
    {
        //Unsubscribe from Event and reset Hue
        Manager.OnSimulationStateChanged -= OnManagerSimulationStateChanged;
        //SetHue(0);
    }

    void FixedUpdate()
    {
        //increase _simulationTime 
        if (_running)
        {
            float flowRate = Manager.getValue(Manager.Instance.GetCurrentData()[espressoMachine.machineID], Manager.Instance.GetSimulationTime(), Listtype.Flow);
            if (flowRate  >= 0)
            {
                if (count % 5 == 0)
                {
                    float particleSize = normaliseValue(flowRate)*2f; 
                    //print("Particle Size:" + particleSize);
                    //print("Flow Rate: " + flowRate);
                    changeParticleSize(particleSize, particleSize, particleSize);
                    changeFlowSpeed(flowRate*3);              
                }
                count++;   
            }
            else
            {   
                EndBrew();
                count = 0;
            }
        }
    }

    private float normaliseValue(float val)
        {
            return (val - _minFlow) / (_maxFlow - _minFlow);
        }

    void changeFlowSpeed(float newSpeed)
    {
        // Check if the ParticleSystem component is not null
        if (flowParticleSystemLeft != null)
        {
            // Get the main module of the Particle System
            var mainModuleLeft = flowParticleSystemLeft.main;
            var mainModuleRight = flowParticleSystemRight.main;

            // Set the new start speed
            mainModuleLeft.startSpeed = newSpeed;
            mainModuleRight.startSpeed = newSpeed;
        }
    }

    void changeBrewTime(float brewTime)
    {
        // Check if the ParticleSystem component is not null
        if (flowParticleSystemLeft != null)
        {
            // Get the main module of the Particle System
            var mainModuleLeft = flowParticleSystemLeft.main;
            var mainModuleRight = flowParticleSystemRight.main;

            // Set the new start speed
            mainModuleLeft.duration = brewTime;
            mainModuleRight.duration = brewTime;
        }
    }

    void changeParticleSize(float particleSizeX, float particleSizeY, float particleSizeZ)
    {
        // Check if the ParticleSystem component is not null
        if (flowParticleSystemLeft != null && flowParticleSystemRight != null)
        {
            // Get the main module of the Particle System
            var mainModuleLeft = flowParticleSystemLeft.main;
            var mainModuleRight = flowParticleSystemRight.main;

            // Set the new start speed
            mainModuleLeft.startSizeXMultiplier = particleSizeX;
            mainModuleLeft.startSizeYMultiplier = particleSizeY;
            mainModuleLeft.startSizeZMultiplier = particleSizeZ;

            mainModuleRight.startSizeXMultiplier = particleSizeX;
            mainModuleRight.startSizeYMultiplier = particleSizeY;
            mainModuleRight.startSizeZMultiplier = particleSizeZ;
        }
    }
    private void StartBrew()
    {
        //print("Brew Started!");

        //Debug.Log("BrewTime: " + flowParticleSystemLeft.main.duration);
        if (!wasRunningBefore){
            changeBrewTime(simulationLength);
        }
        
        flowParticleSystemLeft.Play();
        flowParticleSystemRight.Play();
        //Debug.Log("BrewTime: " + flowParticleSystemLeft.main.duration);
    }

    private void EndBrew()
    {
        //print("Brew Ended!");
        flowParticleSystemLeft.Stop();
        flowParticleSystemRight.Stop();

    }

    private float CalcBrewRate()
    {
        return transform.forward.y * Mathf.Rad2Deg;
    }

    private void OnManagerSimulationStateChanged(SimulationState newState)
    {
        _running = newState == SimulationState.SimulationRunning;
        
        if(_running)
        {
            float maxTime1 = Manager.Instance.GetCurrentData()[0].getList(Listtype.Elapsed).Last();
            float maxTime2 = Manager.Instance.GetCurrentData()[1].getList(Listtype.Elapsed).Last();
            if (simulationLength == 0)
            {
                simulationLength = Manager.Instance.GetMaxTimeElapsed();
            }
                
            StartBrew();
        }
        
        if (newState == SimulationState.SimulationStopped)
        {
            flowParticleSystemLeft.Stop();
            flowParticleSystemRight.Stop();
            wasRunningBefore = true;
            //Data timeLength = Manager.Instance.GetCurrentData();
            //simulationLength = timeLength.getList(Listtype.Elapsed).Last() - Manager.Instance.GetSimulationTime();

        }



        if (newState == SimulationState.SimulationReset)
        {
            simulationLength = 0;
            count = 0;
            flowParticleSystemLeft.Stop();
            wasRunningBefore = false; 
        }




    }
}
