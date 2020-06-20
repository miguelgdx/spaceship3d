using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This class is used to control the behaviour in the first scene.
 * After pressing the Settings button.
 */
public class OptionsMenu : MonoBehaviour
{
    public int SelectedQuality = 0;
    public int SelectedDifficulty = 0;
    public GameObject LQButton;
    public GameObject MQButton;
    public GameObject HQButton;
    public GameObject EButton;
    public GameObject HButton;

    // Start is called before the first frame update
    void Start()
    {
        LoadPlayerPrefs();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Loads prefs and select UI buttons according to Player prefs.
    public void LoadPlayerPrefs()
    {
        SelectedQuality = PlayerPrefs.GetInt("Quality", 0);

        // Select quality button accordingly.
        switch (SelectedQuality)
        {
            case 0:
                LQButton.GetComponent<UIButtonController>().Select();
                break;
            case 1:
                MQButton.GetComponent<UIButtonController>().Select();
                break;
            case 2:
                HQButton.GetComponent<UIButtonController>().Select();
                break;
        };
        SetQuality(SelectedQuality);

        SelectedDifficulty = PlayerPrefs.GetInt("Difficulty", 0);

        // Select difficulty button accordingly.
        switch (SelectedDifficulty)
        {
            case 0:
                EButton.GetComponent<UIButtonController>().Select();
                break;
            case 1:
                HButton.GetComponent<UIButtonController>().Select();
                break;
        };
        SetDifficulty(SelectedDifficulty);
    }

    public void SetQuality(int value)
    {
        SelectedQuality = value;
        QualitySettings.SetQualityLevel(SelectedQuality, true);
        // Store in preferences.
        PlayerPrefs.SetInt("Quality", value);
    }
    public void SetDifficulty(int value)
    {
        SelectedDifficulty = value;
        // Store in preferences.
        PlayerPrefs.SetInt("Difficulty", value);
    }

}
