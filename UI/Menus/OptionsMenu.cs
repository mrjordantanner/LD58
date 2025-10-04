using UnityEngine;
using System.Linq;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;


public class OptionsMenu : MenuPanel
{
    public Toggle autoDrillToggle;
    public Toggle allowHorizDrillingToggle;

    // TODO move these to config
    //Setting AllowHorizDrilling, AutoDrill;

    private void Start()
    {
        //AllowHorizDrilling = Config.Instance.GetSettingByName(nameof(AllowHorizDrilling));
        //AutoDrill = Config.Instance.GetSettingByName(nameof(AutoDrill));

        ApplySettings();
    }

    public void ToggleOnClick()
    {
        SaveToConfig();
        ApplySettings();
    }

    public void RefreshMenu()
    {
        //autoDrillToggle.isOn = AutoDrill.Value == 1;
        //allowHorizDrillingToggle.isOn = AllowHorizDrilling.Value == 1;
    }

    float inputFieldValue;
    public void ApplySettings()
    {
        //DrillController.Instance.autoDrill = AutoDrill.Value == 1;
        //DrillController.Instance.allowHorizontalDrilling = AllowHorizDrilling.Value == 1;
    }

    public void SaveToConfig()
    {
        //AutoDrill.Value = autoDrillToggle.isOn ? 1 : 0;
        //AllowHorizDrilling.Value = allowHorizDrillingToggle.isOn ? 1 : 0;

        //AutoDrill.SaveToPlayerPrefs();
        //AllowHorizDrilling.SaveToPlayerPrefs();
    }

}
