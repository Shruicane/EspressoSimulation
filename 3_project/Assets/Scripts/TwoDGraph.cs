using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class TwoDGraph : MonoBehaviour
{
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private int simulationGranularity = 1;
    [SerializeField] private Color lineColorMashine1 = new Color(0.3f, 0.8f, 0.2f, 0.5f);
    [SerializeField] private Color lineColorMashine2 = new Color(0.3f, 0.2f, 0.9f, 0.5f);
    //[SerializeField] private Listtype graphData = Listtype.Pressure;
    private RectTransform graphContainer;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform legendTemplate;
    private bool _running = false;
    float minValue;
    float maxValue;
    float graphHeight;
    float simulationLength;
    float graphWidth;
    private int fixedUpdateCounter = 0;
    GameObject lastCircleGameObject1 = null;
    GameObject lastCircleGameObject2 = null;
    private void Awake()
    {
        graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();
        legendTemplate = graphContainer.Find("legendTemplate").GetComponent<RectTransform>();
        graphHeight = graphContainer.sizeDelta.y;
        graphWidth = graphContainer.sizeDelta.x;
        Manager.OnSimulationStateChanged += OnManagerSimulationStateChanged;
    }
    private GameObject CreateCircle(Vector2 anchoredPosition) {
        GameObject gameObject = new GameObject("circle", typeof(Image)){tag = "circle"};
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(5, 5);
        rectTransform.anchorMin = new Vector2(0,0);
        rectTransform.anchorMax = new Vector2(0,0);
        return gameObject;

    }

/* Following Method Currently not used

    private void ShowGraph(List<int> valueList)
    {

        float yMax = 100f;
        GameObject lastCircleGameObject = null;
        for (int i = 0; i < valueList.Count; i++){
            float xPosition = i * (graphWidth/valueList.Count);
            float yPosition = (valueList[i]/yMax) * graphHeight;
            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition)); 
            if (lastCircleGameObject != null){
                DrawLine(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);     
            }
            lastCircleGameObject = circleGameObject;
        }
    }

*/

    private void drawLegend()
    {
        string legendString = "";
        Listtype listtype = Manager.Instance.GetGraphdataListtype();
        if (listtype == Listtype.Pressure)
        {
            legendString = "Pressure (bar)";
        } else if (listtype == Listtype.BasketTemperature | listtype == Listtype.MixTemperature)
        {
            legendString = "Temperature (°C)";
        } else if (listtype == Listtype.Flow)
        {
            legendString = "Flow (mL/s)";
        }else
        {
            Debug.LogError(" Kein Valider Listtype");
        }
            
            RectTransform legend = Instantiate(legendTemplate);
            legend.SetParent(graphContainer);
            legend.tag = "label"; 
            legend.gameObject.SetActive(true);
            legend.anchoredPosition = new Vector2(680, 415);
            legend.GetComponent<Text>().text = legendString;
    }
    

    private void drawSeparators()
    {
        for (int i = 0; i <= 10; i++)
        {
            RectTransform labelX = Instantiate(labelTemplateX);
            labelX.SetParent(graphContainer);
            labelX.tag = "label"; 
            labelX.gameObject.SetActive(true);
            labelX.anchoredPosition = new Vector2(graphWidth/10*i, 0);
            labelX.GetComponent<Text>().text = Math.Round(simulationLength/10 * i, 1).ToString();
            if (i>0)
            {
                RectTransform labelY = Instantiate(labelTemplateY);
                labelY.SetParent(graphContainer);
                labelY.tag = "label"; 
                labelY.gameObject.SetActive(true);
                labelY.anchoredPosition = new Vector2(-15, graphHeight/10*i);
                labelY.GetComponent<Text>().text = Math.Round(minValue+(maxValue-minValue)/10*i, 1).ToString();
            }

        }


    }

    private void drawNextDataPoint(Vector2 position1, Vector2 position2)
    {
        if (position2 == new Vector2(-1, -1))
        {
            float xPosition = position1[0];
            float yPosition = position1[1];
            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition)); 
                if (lastCircleGameObject1 != null){
                    DrawLine(lastCircleGameObject1.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition, lineColorMashine1);     
                }
            lastCircleGameObject1 = circleGameObject;
        }else
        {
            float xPosition = position1[0];
            float yPosition1 = position1[1];
            float yPosition2 = position2[1];

            if (yPosition1 < 0)
            {
                GameObject circleGameObject2 = CreateCircle(new Vector2(xPosition, yPosition2)); 
                    if (lastCircleGameObject2 != null){  
                        DrawLine(lastCircleGameObject2.GetComponent<RectTransform>().anchoredPosition, circleGameObject2.GetComponent<RectTransform>().anchoredPosition, lineColorMashine2);   
                    }
                lastCircleGameObject2 = circleGameObject2;
            }else if (yPosition2 < 0)
            {
                GameObject circleGameObject1 = CreateCircle(new Vector2(xPosition, yPosition1)); 
                    if (lastCircleGameObject1){
                        DrawLine(lastCircleGameObject1.GetComponent<RectTransform>().anchoredPosition, circleGameObject1.GetComponent<RectTransform>().anchoredPosition, lineColorMashine1);      
                    }
                lastCircleGameObject1 = circleGameObject1;
            }else
            {
                GameObject circleGameObject1 = CreateCircle(new Vector2(xPosition, yPosition1)); 
                GameObject circleGameObject2 = CreateCircle(new Vector2(xPosition, yPosition2)); 
                    if (lastCircleGameObject1 != null && lastCircleGameObject2 != null){
                        DrawLine(lastCircleGameObject1.GetComponent<RectTransform>().anchoredPosition, circleGameObject1.GetComponent<RectTransform>().anchoredPosition, lineColorMashine1);     
                        DrawLine(lastCircleGameObject2.GetComponent<RectTransform>().anchoredPosition, circleGameObject2.GetComponent<RectTransform>().anchoredPosition, lineColorMashine2);   
                    }
                lastCircleGameObject1 = circleGameObject1;
                lastCircleGameObject2 = circleGameObject2;
            }


        }
        }


    public static float GetAngleFromVectorFloat(Vector3 dir) {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;
        return n;
        }

    private void DrawLine(Vector2 dotAPosition, Vector2 dotBPosition, Color lineColor)
    {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image)){tag = "line"};
        gameObject.transform.SetParent(graphContainer, false);

        gameObject.GetComponent<Image>().color = lineColor;

        Vector2 dir = (dotBPosition - dotAPosition).normalized;
        float distance = Vector2.Distance(dotAPosition, dotBPosition);

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0,0);
        rectTransform.anchorMax = new Vector2(0,0);
        rectTransform.sizeDelta = new Vector2(distance, 3f);
        rectTransform.anchoredPosition = dotAPosition + dir * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(dir));
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_running && !Manager.Instance.IsTwoMachinesRunning())
        {
            // Increment the counter every time FixedUpdate is called
            fixedUpdateCounter++;

            // Execute the code only every fourth time
            if (fixedUpdateCounter % simulationGranularity == 0)
            {
                float simTime = Manager.Instance.GetSimulationTime();
                float xScale = graphWidth/simulationLength;
                float xPosition = simTime * xScale;

                float yValue = Manager.getValue(Manager.Instance.GetCurrentData()[0], simTime, Manager.Instance.GetGraphdataListtype(), false);
                //print("y-Value" + yValue);

                float yPosition = (graphHeight - 10) / (maxValue - minValue) * (yValue - minValue);

                drawNextDataPoint(new Vector2(xPosition, yPosition), new Vector2(-1, -1));
            }
        }

        else if (_running && Manager.Instance.IsTwoMachinesRunning())
        {
            // Increment the counter every time FixedUpdate is called
            fixedUpdateCounter++;

            // Execute the code only every fourth time
            if (fixedUpdateCounter % simulationGranularity == 0)
            {
                float simTime = Manager.Instance.GetSimulationTime();
                float xScale = graphWidth/simulationLength;
                float xPosition = simTime * xScale;
                
                // KOMMENTAR FÜR DANIEL: Hier Manager.Instance.GetCurrentData()[0] für Graph 1 verwenden und Manager.Instance.GetCurrentData()[1] f
                float yValue1 = Manager.getValue(Manager.Instance.GetCurrentData()[0], simTime, Manager.Instance.GetGraphdataListtype(), false);
                float yValue2 = Manager.getValue(Manager.Instance.GetCurrentData()[1], simTime, Manager.Instance.GetGraphdataListtype(), false);
                //print("y-Value" + yValue);

                float yPosition1 = (graphHeight - 10) / (maxValue - minValue) * (yValue1 - minValue);
                float yPosition2 = (graphHeight - 10) / (maxValue - minValue) * (yValue2 - minValue);

                drawNextDataPoint(new Vector2(xPosition, yPosition1), new Vector2(xPosition, yPosition2));
            }
        }
    }


    private void OnManagerSimulationStateChanged(SimulationState newState)
    {
        _running = (newState == SimulationState.SimulationRunning) && (!Manager.Instance.IsManualRun());

        if(_running)
        {

            float[] minMaxValues = Manager.getGlobalMinMaxOf(Manager.Instance.GetGraphdataListtype());
            //print(graphData);


            //print("Selected Package: " + timeLength.GetName());
            simulationLength = Manager.Instance.GetMaxTimeElapsed();
            minValue = minMaxValues[0];
            maxValue = minMaxValues[1];
            //print(minValue);
            drawSeparators();
            drawLegend();


        }else{
            if (newState == SimulationState.SimulationReset)
            {
                fixedUpdateCounter = 0;
                GameObject[] circles;
                GameObject[] lines;
                GameObject[] labels;
                circles = GameObject.FindGameObjectsWithTag("circle");
                lines = GameObject.FindGameObjectsWithTag("line");
                labels = GameObject.FindGameObjectsWithTag("label");

                foreach(GameObject circle in circles)
                {
                    Destroy(circle);
                }
                foreach(GameObject line in lines)
                {
                    Destroy(line);
                }   
                foreach(GameObject label in labels)
                {
                    Destroy(label);
                }   
        }



        }
}
}
