using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionContorller : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;

    public Toggle fullToggle; // 전체 화면 모드
    public Toggle windowToggle; // 창 모드 
    private Toggle activatedToggle; // 활성화된 모드

    private Resolution[] resolutions;

    enum ScreenMode
    {
        Full,
        Window
    }

    ScreenMode screenMode;

    private void Start()
    {
        GetAllResolutions();
        SetUpDropdown();
        SetUpToggles();
    }

    void GetAllResolutions()
    {
        resolutions = Screen.resolutions;
    }

    void SetUpDropdown()
    {
        resolutionDropdown.ClearOptions();

        HashSet<string> options = new HashSet<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " X " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(new List<string>(options));
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution()
    {
        int resolutionIndex = resolutionDropdown.value;
        Resolution resolution = resolutions[resolutionIndex];

        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        Debug.Log(Screen.width + " X " + Screen.height);
    }

    void SetUpToggles()
    {
        fullToggle.onValueChanged.AddListener(delegate { ToggleChanged(fullToggle); });
        windowToggle.onValueChanged.AddListener(delegate { ToggleChanged(windowToggle); });

        if (Screen.fullScreen)
        {
            fullToggle.isOn = true;
            activatedToggle = fullToggle;

        }
        else
        {
            windowToggle.isOn = true;
            activatedToggle = windowToggle;
        }
    }

    void ToggleChanged(Toggle changedToggle)
    {
        if (changedToggle.isOn)
        {
            activatedToggle = changedToggle;

            if (changedToggle == fullToggle)
            {
                windowToggle.isOn = false;
                screenMode = ScreenMode.Full;
            }
            else
            {
                fullToggle.isOn = false;
                screenMode = ScreenMode.Window;
            }
        }
        else
        {
            if (activatedToggle == changedToggle)
            {
                activatedToggle.isOn = true;
            }
        }

        SetScreenMode();
    }

    public void SetScreenMode()
    {
        if (screenMode == ScreenMode.Full)
        {
            Screen.SetResolution(Screen.width, Screen.height, true);
        }
        else
        {
            Screen.SetResolution(Screen.width, Screen.height, false);
        }
    }
}