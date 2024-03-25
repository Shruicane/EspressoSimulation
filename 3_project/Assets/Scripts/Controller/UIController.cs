using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

namespace Controller
{
    public class UIController : MonoBehaviour
    {
        public GameObject dropDownMenuMachine1;
        public GameObject dropDownMenuMachine2;
        public GameObject dropDownMenuGraphData;
        public GameObject slider;
        public GameObject checkBoxManualRun;
        public GameObject sliderAndLabelParentObejct;
        public GameObject cameraDropDown;
        public GameObject oneCamera;
        public GameObject twoCamerasOneMachine;
        public GameObject twoCamerasTwoMachines;
        public GameObject fourCameras;
        public GameObject splitscreenVertBorder;
        public GameObject splitscreenHorBorder;
        public GameObject sidebarPanel;
        public GameObject graphContainer;
        public GameObject playPauseButton;
        public GameObject playPauseButtonPlayIcon;
        public GameObject playPauseButtonPauseIcon;
        public GameObject resetButton;
        
        private bool _isSliding;
        private bool _showSettings;
        
        //Unity Event Functions
        private void Awake()
        {
            // subscribe to Event
            Manager.OnSimulationStateChanged += OnManagerSimulationStateChanged;
        }
        
        private void OnDestroy()
        {
            //unsubscribe from event
            Manager.OnSimulationStateChanged -= OnManagerSimulationStateChanged;
        }

        private void Start()
        {
            //Add All Data Entries to the Dropdown
            List<string> dataNames = new List<string>();
            foreach (Data data in Manager.Instance.GetData())
            {
                dataNames.Add(data.GetName());
            }
            dropDownMenuMachine1.GetComponent<TMP_Dropdown>().AddOptions(dataNames);
            dropDownMenuMachine2.GetComponent<TMP_Dropdown>().AddOptions(dataNames);
            //Select First Entry to avoid having nothing selected after startup
            dropDownMenuMachine1.GetComponent<TMP_Dropdown>().Select();
            dropDownMenuMachine2.GetComponent<TMP_Dropdown>().Select();
            dropDownMenuGraphData.GetComponent<TMP_Dropdown>().Select();
            //Set Slider Max Value
            float max1 = Manager.Instance.GetCurrentData()[0].getMinMax(Listtype.Elapsed)[1];
            float max2 = Manager.Instance.GetCurrentData()[1].getMinMax(Listtype.Elapsed)[1];
            slider.GetComponent<Slider>().maxValue = max1 > max2 ? max1 : max2;
            slider.GetComponent<Slider>().value = 0;
        }

        // ButtonClick Functions
        public void OnSelectDropdownMachine1Item()
        {
            Manager.Instance.SetCurrentData1(dropDownMenuMachine1.GetComponent<TMP_Dropdown>().value);
            //Get Maximum Time Elapsed for slider 
            slider.GetComponent<Slider>().maxValue = Manager.Instance.GetMaxTimeElapsed();
            slider.GetComponent<Slider>().value = 0;
        }
        
        public void OnSelectDropdownMachine2Item()
        {
            Manager.Instance.SetCurrentData2(dropDownMenuMachine2.GetComponent<TMP_Dropdown>().value);
            //Get Maximum Time Elapsed for slider 
            slider.GetComponent<Slider>().maxValue = Manager.Instance.GetMaxTimeElapsed();
            slider.GetComponent<Slider>().value = 0;
        }

        public void OnSelectDropdownGraphData()
        {
            switch (dropDownMenuGraphData.GetComponent<TMP_Dropdown>().value)
            {
                case 0:
                    Manager.Instance.SetGraphdataListtype(Listtype.Flow);
                    break;
                case 1:
                    Manager.Instance.SetGraphdataListtype(Listtype.Pressure);
                    break;
                case 2:
                    Manager.Instance.SetGraphdataListtype(Listtype.BasketTemperature);
                    break;
                default:
                    Manager.Instance.SetGraphdataListtype(Listtype.Flow);
                    break;
            }

        }

        public void OnClickOpenSettings()
        {
            //Einstellungen öffnen
            if (_showSettings == false)
            {
                _showSettings = true;
                _isSliding = true; 
            }
            else
            {
                _showSettings = false;
                _isSliding = true;
            }
        }

        public void OnClickCloseSettings()
        {
            //Settings schließen
            _showSettings = false;
            _isSliding = true;
        }

        private void FixedUpdate()
        {
            //Sidebar Animation
            //X = -145: Sidebar ausgefahren 
            //X = +180: Sidebar eingefahren 
            float maxX = 180;
            float minX = -145;
            if (_isSliding)
            {
                Vector3 currentPos = sidebarPanel.GetComponent<RectTransform>().anchoredPosition;
                if (_showSettings)
                {
                    if (currentPos.x - 20f > minX)
                    {
                        sidebarPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(currentPos.x - 40f, currentPos.y, currentPos.z);    
                    }
                    else
                    {
                        sidebarPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(minX, currentPos.y, currentPos.z);
                        _isSliding = false;
                    }
                }
                else
                {
                    if (currentPos.x + 20f < maxX)
                    {
                        sidebarPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(currentPos.x + 40f, currentPos.y, currentPos.z);    
                    }
                    else
                    {
                        sidebarPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(maxX, currentPos.y, currentPos.z);
                        _isSliding = false;
                    }
                }
            }
        }

        /// <summary>
        /// Executed when clicking on SimulationControlButton.
        /// Update SimulationState according to Button Click.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void OnClickSimulationControlButton()
        {
            switch (Manager.Instance.GetSimulationState())
            {
                case SimulationState.SimulationReset:
                    Manager.Instance.UpdateSimulationState(SimulationState.SimulationRunning);
                    break;
                case SimulationState.SimulationRunning:
                    Manager.Instance.UpdateSimulationState(SimulationState.SimulationStopped);
                    break;
                case SimulationState.SimulationStopped:
                    Manager.Instance.UpdateSimulationState(SimulationState.SimulationRunning);
                    break;
                case SimulationState.SimulationFinished:
                    Manager.Instance.UpdateSimulationState(SimulationState.SimulationReset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Manager.Instance.GetSimulationState), Manager.Instance.GetSimulationState(), "Invalid SimulationState or SimulationState not implemented.");
            }
        }
        
        public void OnClickResetSimulationButton()
        {
            //Update SimulationState
            Manager.Instance.UpdateSimulationState(SimulationState.SimulationReset);
        }
        
        public void OnClickCloseButton()
        {
            Application.Quit();
        }
        
        public void OnSelectCameraDropdown()
        {
            int dropDownValue = cameraDropDown.GetComponent<TMP_Dropdown>().value;

            Vector2 posBottomLeft = new Vector2(350f, 190f);
            Vector2 posBottomCenter = new Vector2(960f, 190f);
            Vector2 posBottomCenterShifted = new Vector2(840f, 190f);
            
            switch (dropDownValue)
            {
                case 0:
                    //1 camera, 1 machine
                    Manager.Instance.SetTwoMachinesRunning(false);
                    oneCamera.SetActive(true);
                    twoCamerasOneMachine.SetActive(false);
                    twoCamerasTwoMachines.SetActive(false);
                    fourCameras.SetActive(false);
                    splitscreenVertBorder.SetActive(false);
                    splitscreenHorBorder.SetActive(false);
                    graphContainer.GetComponent<RectTransform>().anchoredPosition = posBottomLeft;
                    break;
                case 1:
                    //2 cameras, 1 machine
                    Manager.Instance.SetTwoMachinesRunning(false);
                    oneCamera.SetActive(false);
                    twoCamerasOneMachine.SetActive(true);
                    twoCamerasTwoMachines.SetActive(false);
                    fourCameras.SetActive(false);
                    splitscreenVertBorder.SetActive(true);
                    splitscreenHorBorder.SetActive(false);
                    graphContainer.GetComponent<RectTransform>().anchoredPosition = posBottomCenter;
                    break;
                case 2:
                    //2 cameras, 2 machines
                    Manager.Instance.SetTwoMachinesRunning(true);
                    oneCamera.SetActive(false);
                    twoCamerasOneMachine.SetActive(false);
                    twoCamerasTwoMachines.SetActive(true);
                    fourCameras.SetActive(false);
                    splitscreenVertBorder.SetActive(true);
                    splitscreenHorBorder.SetActive(false);
                    graphContainer.GetComponent<RectTransform>().anchoredPosition = posBottomCenter;
                    break;
                case 3:
                    //4 cameras, 2 machines
                    Manager.Instance.SetTwoMachinesRunning(true);
                    oneCamera.SetActive(false);
                    twoCamerasOneMachine.SetActive(false);
                    twoCamerasTwoMachines.SetActive(false);
                    fourCameras.SetActive(true);
                    splitscreenVertBorder.SetActive(true);
                    splitscreenHorBorder.SetActive(true);
                    graphContainer.GetComponent<RectTransform>().anchoredPosition = posBottomCenterShifted;
                    break;
                default:
                    throw new NotImplementedException("Invalid Camera Dropdown Value: " + dropDownValue);
            }
                
        }
        
        public void OnClickManualRunCheckBox()
        {
            if (checkBoxManualRun.GetComponent<Toggle>().isOn)
            {
                Manager.Instance.UpdateSimulationState(SimulationState.SimulationReset);
                Manager.Instance.UpdateSimulationState(SimulationState.SimulationRunning);
                Manager.Instance.SetManualRun(true);
                slider.GetComponent<Slider>().SetValueWithoutNotify(0);
                
                sliderAndLabelParentObejct.SetActive(true);
                playPauseButton.SetActive(false);
                resetButton.SetActive(false);
                dropDownMenuMachine1.GetComponent<TMP_Dropdown>().enabled = true;
            }
            else
            {
                Manager.Instance.SetManualRun(false);
                sliderAndLabelParentObejct.SetActive(false);
                Manager.Instance.UpdateSimulationState(SimulationState.SimulationReset);
            }
            print(Manager.Instance.IsManualRun());
        }

        public void OnSliderValueChanged()
        {
            Manager.Instance.SetSimulationTime(slider.GetComponent<Slider>().value);
        }
        
        /// <summary>
        /// Executed when SimulationState Changed.
        /// Adjust Button Text &amp; Button visbility according to SimulationState.
        /// </summary>
        /// <param name="newState"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void OnManagerSimulationStateChanged(SimulationState newState)
        {
            switch (newState)
            {
                case SimulationState.SimulationReset: 
                    playPauseButton.SetActive(true);
                    playPauseButton.GetComponent<Button>().interactable = true;
                    playPauseButtonPlayIcon.SetActive(true);
                    playPauseButtonPauseIcon.SetActive(false);
                    resetButton.SetActive(false);
                    dropDownMenuMachine1.GetComponent<TMP_Dropdown>().interactable = true;
                    dropDownMenuMachine2.GetComponent<TMP_Dropdown>().interactable = true;
                    dropDownMenuGraphData.GetComponent<TMP_Dropdown>().interactable = true;
                    cameraDropDown.GetComponent<TMP_Dropdown>().interactable = true;
                    graphContainer.SetActive(! Manager.Instance.IsManualRun());
                    break;
                case SimulationState.SimulationRunning: 
                    playPauseButton.SetActive(true);
                    playPauseButton.GetComponent<Button>().interactable = true;
                    playPauseButtonPlayIcon.SetActive(false);
                    playPauseButtonPauseIcon.SetActive(true);
                    resetButton.SetActive(false);
                    dropDownMenuMachine1.GetComponent<TMP_Dropdown>().interactable = false;
                    dropDownMenuMachine2.GetComponent<TMP_Dropdown>().interactable = false;
                    dropDownMenuGraphData.GetComponent<TMP_Dropdown>().interactable = false;
                    cameraDropDown.GetComponent<TMP_Dropdown>().interactable = false;
                    graphContainer.SetActive(! Manager.Instance.IsManualRun());
                    break;
                case SimulationState.SimulationStopped: 
                    playPauseButton.SetActive(true);
                    playPauseButton.GetComponent<Button>().interactable = true;
                    playPauseButtonPlayIcon.SetActive(true);
                    playPauseButtonPauseIcon.SetActive(false);
                    resetButton.SetActive(true);
                    dropDownMenuMachine1.GetComponent<TMP_Dropdown>().interactable = false;
                    dropDownMenuMachine2.GetComponent<TMP_Dropdown>().interactable = false;
                    dropDownMenuGraphData.GetComponent<TMP_Dropdown>().interactable = false;
                    cameraDropDown.GetComponent<TMP_Dropdown>().interactable = false;
                    graphContainer.SetActive(! Manager.Instance.IsManualRun());
                    break;
                case SimulationState.SimulationFinished:
                    playPauseButton.SetActive(true);
                    playPauseButton.GetComponent<Button>().interactable = false;
                    playPauseButtonPlayIcon.SetActive(true);
                    playPauseButtonPauseIcon.SetActive(false);
                    resetButton.SetActive(true);
                    dropDownMenuMachine1.GetComponent<TMP_Dropdown>().interactable = false;
                    dropDownMenuMachine2.GetComponent<TMP_Dropdown>().interactable = false;
                    dropDownMenuGraphData.GetComponent<TMP_Dropdown>().interactable = false;
                    cameraDropDown.GetComponent<TMP_Dropdown>().interactable = false;
                    graphContainer.SetActive(! Manager.Instance.IsManualRun());
                    break;
                default: 
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, "Invalid SimulationState or SimulationState not implemented.");
            }
        }
    }
}
