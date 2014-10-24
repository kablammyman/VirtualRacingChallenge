//========================================================================================================================
// FFB Toolkit - (c) Tony Zadravec - Brisbane, Australia
// 
// Terms & Conditions:
//  - Use for unlimited time, any number of projects, royalty-free.
//  - Keep the copyright notices on top of the source files.
//  - Resale or redistribute as anything except a final product to the end user (asset / library / engine / middleware / etc.) is not allowed.
//
// Bug reports, improvements to the code, suggestions on further developments, etc are always welcome.
// Unity forum user: Zaddo67
//========================================================================================================================
//
// WheelInput combined with Wheelmenu & Inputscan provides a solution for integrating DirectInput & Force Feedback functions with a car controller
// These classes can be used as is or modified for your custom requirements.
//
// This class will:
//          - Manage input from attached game controllers
//          - provides functions to send force feedback to the device
//          - Save/Load mapping to common driving functions
//          - Functions to access current input values
//
//========================================================================================================================
using System.Collections;
using System.Collections.Generic;
using DInputProxy;
using System.Diagnostics;

using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using System;
using System.Runtime.Serialization;
using System.Reflection;

using UnityEngine;

public class WheelInput
{

    //public int currentDeviceID = -1;
    public int DeviceCount = 0;
    RawJoystickState[] js;

    // Input map - maps functions to inputs on gaming device
    public Dictionary<GameOptions.InputType, int>[] inputMap;

    // Singlton Class - property for singlton
    private static WheelInput instance;
    public static WheelInput Instance
    {
        get
        {
            if (instance != null) return instance;
            else
            {
                try
                {
                    // Create singlton
                    instance = new WheelInput();

                    // Connect to gaming device
                    if (DWheel.InitDirectInput())
                    {
                        instance.LoadSettings();
                        return instance;
                    }
                    else return null;
                }
                catch
                {
                    return null;
                }

            }
        }
    }

    /// <summary>
    /// Load Saved input maps for devices
    /// </summary>
    private void LoadSettings()
    {
        inputMap = new Dictionary<GameOptions.InputType, int>[4];
        js = new RawJoystickState[4];

        // Initialise each gaming device
        for (int d = 0; d < 4; d++)
        {
            // Is device connected
            if (DWheel.SetDeviceID(d))
            {
                inputMap[d] = new Dictionary<GameOptions.InputType, int>();
                js[d] = new RawJoystickState();

                // Initialise input map (Maps keys to function)
                foreach (GameOptions.InputType iType in Enum.GetValues(typeof(GameOptions.InputType)))
                {
                    inputMap[d].Add(iType, -1);
                }
                LoadInputSettings(d, DWheel.GetPID(d).ToString());
                DeviceCount = d + 1;
            }
            else break;

        }

        if (DeviceCount > 0) currentDeviceID = 0;
    }

    /// <summary>
    /// Get current state of device input for current controller
    /// </summary>
    public void GetInput()
    {
        GetInput(currentDeviceID);

    }

    /// <summary>
    /// Get current state of device input for specific controller
    /// </summary>
    /// <param name="index">The device number</param>
    public void GetInput(int index)
    {
        if (index < DeviceCount)
        {
            js[index] = DWheel.GetJoystickState(index);
        }
    }

    /// <summary>
    /// Housekeeping to clean up references to direct input.
    /// If this doesn't run. Then next time connection is attempted to Direct Input, will crash Unity
    /// </summary>
    public void SelfDestruct()
    {
        try
        {
            instance = null;
            DWheel.FreeDirectInput();
        }
        catch { }
    }

    /// <summary>
    /// Class Initialisation
    /// </summary>
    WheelInput()
    {
    }

    /// <summary>
    /// Class destructor
    /// </summary>
    ~WheelInput()
    {
    }

    /// <summary>
    /// Set default device to previous controller
    /// </summary>
    public void PrevController()
    {
        if (DeviceCount == 0) currentDeviceID = -1;
        else
        {
            currentDeviceID--;
            if (currentDeviceID < 0) currentDeviceID = DeviceCount - 1;
        }
    }

    /// <summary>
    /// Set default device to next controller
    /// </summary>
    public void NextController()
    {
        if (DeviceCount == 0) currentDeviceID = -1;
        else
        {
            currentDeviceID++;
            if (currentDeviceID >= DeviceCount) currentDeviceID = 0;
        }
    }

    public int currentDeviceID
    {
        get { return DWheel.CurrentIndex; }
        set
        {
            DWheel.SetDeviceID(value);
        }
    }

    /// <summary>
    /// Set Accelerator/Brake to combined controller for current device.  This will split input on axis.
    /// </summary>
    /// <param name="value">1=Combined 0=Not combined</param>
    public void SetCombined(int value)
    {
        SetCombined(currentDeviceID, value);
    }


    /// <summary>
    /// Set Accelerator/Brake to combined controller for specific device.  This will split input on axis.
    /// </summary>
    /// <param name="index">Device ID</param>
    /// <param name="value">1=Combined 0=Not combined</param>
    public void SetCombined(int index, int value)
    {
        inputMap[index][GameOptions.InputType.Combined] = value;
    }

    /// <summary>
    /// Invert force feedback
    /// </summary>
    /// <param name="value">1=Inverted 0=Not inverted</param>
    public void SetInverted(int value)
    {
        SetInverted(currentDeviceID, value);
    }

    /// <summary>
    /// Invert Force feedback
    /// </summary>
    /// <param name="index">Device ID</param>
    /// <param name="value">1=Inverted 0=Not inverted</param>
    public void SetInverted(int index, int value)
    {
        inputMap[index][GameOptions.InputType.InvertFFB] = (value == 1 ? -1 : 1);
    }

    /// <summary>
    /// Return inverted setting for controller
    /// </summary>
    /// <returns></returns>
    public bool GetInverted() { return GetInverted(currentDeviceID); }

    /// <summary>
    /// Return inverted setting for controller
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool GetInverted(int index)
    {
        return (inputMap[index][GameOptions.InputType.InvertFFB] == -1);
    }


    /// <summary>
    ///  force feedback gain
    /// </summary>
    /// <param name="value"></param>
    public void SetGain(int value)
    {
        SetGain(currentDeviceID, value);
    }

    /// <summary>
    /// Force feedback gain
    /// </summary>
    /// <param name="index">Device ID</param>
    /// <param name="value">1=Inverted 0=Not inverted</param>
    public void SetGain(int index, int value)
    {
        inputMap[index][GameOptions.InputType.FFBGain] = value;
    }

    /// <summary>
    /// Return inverted setting for controller
    /// </summary>
    /// <returns></returns>
    public int GetGain() { return GetGain(currentDeviceID); }

    /// <summary>
    /// Return inverted setting for controller
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public int GetGain(int index)
    {
        return (inputMap[index][GameOptions.InputType.FFBGain]);
    }

    /// <summary>
    /// Returns combined state of default controller
    /// </summary>
    /// <returns>True if device is using combined brake/accelerator</returns>
    public bool Combined()
    {
        return Combined(currentDeviceID);
    }

    /// <summary>
    /// Returns combined state of specific controller
    /// </summary>
    /// <param name="index">Device ID</param>
    /// <returns>True if device is using combined brake/accelerator</returns>
    public bool Combined(int index)
    {
        if (inputMap[index].ContainsKey(GameOptions.InputType.Combined))
            return (inputMap[index][GameOptions.InputType.Combined] == 1);
        else
            return false;
    }



    //***********************************************************************************************************
    // Mapped input functions
    //***********************************************************************************************************


    /// <summary>
    /// Get the steer value from default device
    /// </summary>
    /// <param name="sensitivity">1=Maximum range, 0=Minimum range</param>
    /// <returns>-1 to 1</returns>
    public float GetSteer(float sensitivity)
    {
        return GetSteer(currentDeviceID, sensitivity);
    }

    /// <summary>
    /// Get the steer value from Specific device
    /// </summary>
    /// <param name="sensitivity">1=Maximum range, 0=Minimum range</param>
    /// <param name="index">Device ID</param>
    /// <returns>-1 to 1</returns>
    public float GetSteer(int index, float sensitivity)
    {
        return ProcessAnalogueInput(index, inputMap[index][GameOptions.InputType.Steer], false, sensitivity);
    }

    /// <summary>
    /// Get throttle input
    /// </summary>
    /// <returns>0 to 1</returns>
    public float GetThrottle()
    {
        return GetThrottle(currentDeviceID);
    }

    /// <summary>
    /// Get throttle input
    /// </summary>
    /// <param name="index">Device ID</param>
    /// <returns>0 to 1</returns>
    public float GetThrottle(int index)
    {
        float rawValue = 0f;
        if (inputMap[currentDeviceID][GameOptions.InputType.Throttle] > 99)
        {
            if (DWheel.IsGamepad())
                rawValue = (1f - ProcessAnalogueInput(index, inputMap[index][GameOptions.InputType.Throttle], true, 1));
            else
                rawValue = ProcessAnalogueInput(index, inputMap[index][GameOptions.InputType.Throttle], true, 1);
        }
        else
        {
            rawValue = getButton(index, inputMap[index][GameOptions.InputType.Throttle]) ? 1 : 0;
        }

        if (Combined(index))
        {
            if (rawValue > 0.5f)
                return 2 * (rawValue - 0.5f);
            else
                return 0;
        }
        else
            return rawValue;
    }


    public float GetBrake()
    {
        return GetBrake(currentDeviceID);
    }

    public float GetBrake(int index)
    {
        float rawValue = 0f;

        GameOptions.InputType inputType = GameOptions.InputType.Brake;
        if (Combined(index)) inputType = GameOptions.InputType.Throttle;

        if (inputMap[currentDeviceID][inputType] > 99)
        {
            rawValue = ProcessAnalogueInput(index, inputMap[index][inputType], true, 1);
        }
        else
            rawValue = getButton(index, inputMap[index][inputType]) ? 1 : 0;

        if (Combined(index))
        {
            if (rawValue < 0.5f)
                return 2 * (0.5f - rawValue);
            else
                return 0;
        }
        else
            return rawValue;

    }

    public float GetClutch()
    {
        return GetClutch(currentDeviceID);
    }

    public float GetClutch(int index)
    {

        if (inputMap[currentDeviceID][GameOptions.InputType.Clutch] > 99)
            return ProcessAnalogueInput(index, inputMap[index][GameOptions.InputType.Clutch], true, 1);
        else
            return getButton(index, inputMap[index][GameOptions.InputType.Clutch]) ? 1 : 0;
    }

    public float GetHandbrake()
    {
        return GetHandbrake(currentDeviceID);
    }

    public float GetHandbrake(int index)
    {
        return getButton(index, inputMap[index][GameOptions.InputType.Handbrake]) ? 1f : 0f;
    }

    public bool GetStartEngine()
    {
        return GetStartEngine(currentDeviceID);
    }

    public bool GetStartEngine(int index)
    {
        return getButton(index, inputMap[index][GameOptions.InputType.StartEngine]);
    }

    /// <summary>
    /// Get controller input analoge input from specified axis 
    /// </summary>
    /// <param name="index">The controller ID</param>
    /// <param name="inputId">The axis number</param>
    /// <param name="offSet">True: set range (-1 to 1), False: set Range (0 to 1)</param>
    /// <param name="sensitivity">Scale output.  Usefulle for steering where you want sensitivity to be reduced at faster speeds</param>
    /// <returns>Contorller value</returns>
    public float ProcessAnalogueInput(int index, int inputId, bool offSet, float sensitivity)
    {
        float retVal;

        if (offSet)
        {
            retVal = (getAnalogue(index, inputId) - 32768f) / -65536f;
        }
        else
        {
            retVal = getAnalogue(index, inputId);
            retVal = retVal / 32768f;
        }
        return (float)Math.Round(retVal * sensitivity, 3);
    }

    /// <summary>
    /// Change up gear
    /// </summary>
    /// <returns></returns>
    public bool ShiftUp()
    {
        return ShiftUp(currentDeviceID);
    }

    /// <summary>
    /// Change up gear
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool ShiftUp(int index)
    {
        return getButton(index, inputMap[index][GameOptions.InputType.ShiftUp]);
    }

    /// <summary>
    /// Change down gear
    /// </summary>
    /// <returns></returns>
    public bool ShiftDown()
    {
        return ShiftDown(currentDeviceID);
    }

    /// <summary>
    /// Change down gear
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool ShiftDown(int index)
    {
        return getButton(index, inputMap[index][GameOptions.InputType.ShiftDown]);
    }

    /// <summary>
    /// Get current gear from H-Shifter
    /// </summary>
    /// <returns></returns>
    public int GetManualGear()
    {
        return GetManualGear(currentDeviceID);
    }

    public int GetManualGear(int index)
    {
        int gearInput = 0;
        if (getButton(index, inputMap[index][GameOptions.InputType.FirstGear])) gearInput = 1;
        if (getButton(index, inputMap[index][GameOptions.InputType.SecondGear])) gearInput = 2;
        if (getButton(index, inputMap[index][GameOptions.InputType.ThirdGear])) gearInput = 3;
        if (getButton(index, inputMap[index][GameOptions.InputType.FourthGear])) gearInput = 4;
        if (getButton(index, inputMap[index][GameOptions.InputType.FifthGear])) gearInput = 5;
        if (getButton(index, inputMap[index][GameOptions.InputType.SixthGear])) gearInput = 6;
        if (getButton(index, inputMap[index][GameOptions.InputType.ReverseGear])) gearInput = -1;
        return gearInput;
    }

    /// <summary>
    /// Get current input (analogue or button) from mapped input type
    /// </summary>
    /// <param name="iType"></param>
    /// <returns></returns>
    public float getInputStatus(GameOptions.InputType iType)
    {
        return getInputStatus(currentDeviceID, iType);
    }

    /// <summary>
    /// Get current input (analogue or button) from mapped input type
    /// </summary>
    /// <param name="index"></param>
    /// <param name="iType"></param>
    /// <returns></returns>
    public float getInputStatus(int index, GameOptions.InputType iType)
    {
        return getInputStatus(inputMap[index][iType]);
    }


    /// <summary>
    /// Get current input from mapped input id
    /// </summary>
    /// <param name="inputId"></param>
    /// <returns></returns>
    public float getInputStatus(int inputId)
    {
        return getInputStatus(currentDeviceID, inputId);
    }

    /// <summary>
    /// Get current input (analogue or button) from mapped input id
    /// </summary>
    /// <param name="index"></param>
    /// <param name="inputId"></param>
    /// <returns></returns>
    public float getInputStatus(int index, int inputId)
    {
        if (inputId > 99) return getAnalogue(index, inputId);
        else return getButton(index, inputId) ? 1 : 0;
    }

    /// <summary>
    /// Get current analogue input from mapped input ID
    /// </summary>
    /// <param name="index"></param>
    /// <param name="inputId"></param>
    /// <returns></returns>
    public float getAnalogue(int index, int inputId)
    {
        float retVal = 0f;
        switch (inputId)
        {
            case 100: retVal = XAxis(index); break;
            case 101: retVal = YAxis(index); break;
            case 102: retVal = ZAxis(index); break;
            case 103: retVal = XRot(index); break;
            case 104: retVal = YRot(index); break;
            case 105: retVal = ZRot(index); break;
            case 106: retVal = Slider0(index); break;
            case 107: retVal = Slider1(index); break;
            case 108: retVal = POV0(index); break;
            case 109: retVal = POV1(index); break;
            case 110: retVal = POV2(index); break;
            case 111: retVal = POV3(index); break;
            case 112: retVal = ForceSlider0(index); break;
            case 113: retVal = ForceSlider1(index); break;
            case 114: retVal = ForceX(index); break;
            case 115: retVal = ForceY(index); break;
            case 116: retVal = ForceZ(index); break;
            case 117: retVal = TorqueX(index); break;
            case 118: retVal = TorqueY(index); break;
            case 119: retVal = TorqueZ(index); break;
            case 120: retVal = VelocitySliders0(index); break;
            case 121: retVal = VelocitySliders1(index); break;
            case 122: retVal = VelocityX(index); break;
            case 123: retVal = VelocityY(index); break;
            case 124: retVal = VelocityZ(index); break;
        }
        return retVal;
    }

    /// <summary>
    /// Get current button input from mapped input ID
    /// </summary>
    /// <param name="index"></param>
    /// <param name="inputId"></param>
    /// <returns></returns>
    public bool getButton(int index, int inputId)
    {
        bool retVal = false;
        switch (inputId)
        {
            case 0: retVal = Button0(index); break;
            case 1: retVal = Button1(index); break;
            case 2: retVal = Button2(index); break;
            case 3: retVal = Button3(index); break;
            case 4: retVal = Button4(index); break;
            case 5: retVal = Button5(index); break;
            case 6: retVal = Button6(index); break;
            case 7: retVal = Button7(index); break;
            case 8: retVal = Button8(index); break;
            case 9: retVal = Button9(index); break;
            case 10: retVal = Button10(index); break;
            case 11: retVal = Button11(index); break;
            case 12: retVal = Button12(index); break;
            case 13: retVal = Button13(index); break;
            case 14: retVal = Button14(index); break;
            case 15: retVal = Button15(index); break;
            case 16: retVal = Button16(index); break;
            case 17: retVal = Button17(index); break;
            case 18: retVal = Button18(index); break;
            case 19: retVal = Button19(index); break;
            case 20: retVal = Button20(index); break;
            case 21: retVal = Button21(index); break;
            case 22: retVal = Button22(index); break;
            case 23: retVal = Button23(index); break;
            case 24: retVal = Button24(index); break;
            case 25: retVal = Button25(index); break;
            case 26: retVal = Button26(index); break;
            case 27: retVal = Button27(index); break;
            case 28: retVal = Button28(index); break;
            case 29: retVal = Button29(index); break;
            case 30: retVal = Button30(index); break;
            case 31: retVal = Button31(index); break;
            case 32: retVal = Button32(index); break;
            case 33: retVal = Button33(index); break;
            case 34: retVal = Button34(index); break;
            case 35: retVal = Button35(index); break;
            case 36: retVal = Button36(index); break;
            case 37: retVal = Button37(index); break;
            case 38: retVal = Button38(index); break;
            case 39: retVal = Button39(index); break;
            case 40: retVal = Button40(index); break;
        }
        return retVal;
    }
	
	//vic i added this since the code doesnt read the gear shift right
	public bool getButton(int inputId)
	{
		return getButton(currentDeviceID, inputId);
	}

    /// <summary>
    /// Get name of Controller input axis/button for mapped value
    /// </summary>
    /// <param name="iType"></param>
    /// <returns></returns>
    public String getName(GameOptions.InputType iType)
    {
        return getName(inputMap[currentDeviceID][iType]);
    }

    /// <summary>
    /// Get name of Controller input axis/button from input ID value
    /// </summary>
    /// <param name="inputId"></param>
    /// <returns></returns>
    public String getName(int inputId)
    {
        if (inputId > 99) return getAnalogueName(inputId);
        else return getButtonName(inputId);
    }

    /// <summary>
    /// Get Name of axis from input ID value
    /// </summary>
    /// <param name="inputId"></param>
    /// <returns></returns>
    public string getAnalogueName(int inputId)
    {
        string retVal = "None";
        switch (inputId)
        {
            case 100: retVal = "XAxis"; break;
            case 101: retVal = "YAxis"; break;
            case 102: retVal = "ZAxis"; break;
            case 103: retVal = "XRot"; break;
            case 104: retVal = "YRot"; break;
            case 105: retVal = "ZRot"; break;
            case 106: retVal = "Slider0"; break;
            case 107: retVal = "Slider1"; break;
            case 108: retVal = "POV0"; break;
            case 109: retVal = "POV1"; break;
            case 110: retVal = "POV2"; break;
            case 111: retVal = "POV3"; break;
            case 112: retVal = "ForceSlider0"; break;
            case 113: retVal = "ForceSlider1"; break;
            case 114: retVal = "ForceX"; break;
            case 115: retVal = "ForceY"; break;
            case 116: retVal = "ForceZ"; break;
            case 117: retVal = "TorqueX"; break;
            case 118: retVal = "TorqueY"; break;
            case 119: retVal = "TorqueZ"; break;
            case 120: retVal = "VelS0"; break;
            case 121: retVal = "VelS1"; break;
            case 122: retVal = "VelX"; break;
            case 123: retVal = "VelY"; break;
            case 124: retVal = "VelZ"; break;
        }
        return retVal;
    }

    /// <summary>
    /// Get name of button from input ID value
    /// </summary>
    /// <param name="inputId"></param>
    /// <returns></returns>
    public string getButtonName(int inputId)
    {
        string retVal = "None";
        switch (inputId)
        {
            case 0: retVal = "Button0"; break;
            case 1: retVal = "Button1"; break;
            case 2: retVal = "Button2"; break;
            case 3: retVal = "Button3"; break;
            case 4: retVal = "Button4"; break;
            case 5: retVal = "Button5"; break;
            case 6: retVal = "Button6"; break;
            case 7: retVal = "Button7"; break;
            case 8: retVal = "Button8"; break;
            case 9: retVal = "Button9"; break;
            case 10: retVal = "Button10"; break;
            case 11: retVal = "Button11"; break;
            case 12: retVal = "Button12"; break;
            case 13: retVal = "Button13"; break;
            case 14: retVal = "Button14"; break;
            case 15: retVal = "Button15"; break;
            case 16: retVal = "Button16"; break;
            case 17: retVal = "Button17"; break;
            case 18: retVal = "Button18"; break;
            case 19: retVal = "Button19"; break;
            case 20: retVal = "Button20"; break;
            case 21: retVal = "Button21"; break;
            case 22: retVal = "Button22"; break;
            case 23: retVal = "Button23"; break;
            case 24: retVal = "Button24"; break;
            case 25: retVal = "Button25"; break;
            case 26: retVal = "Button26"; break;
            case 27: retVal = "Button27"; break;
            case 28: retVal = "Button28"; break;
            case 29: retVal = "Button29"; break;
            case 30: retVal = "Button30"; break;
            case 31: retVal = "Button31"; break;
            case 32: retVal = "Button32"; break;
            case 33: retVal = "Button33"; break;
            case 34: retVal = "Button34"; break;
            case 35: retVal = "Button35"; break;
            case 36: retVal = "Button36"; break;
            case 37: retVal = "Button37"; break;
            case 38: retVal = "Button38"; break;
            case 39: retVal = "Button39"; break;
            case 40: retVal = "Button40"; break;
        }
        return retVal;
    }

    /// <summary>
    /// Load saved input mappings for device
    /// </summary>
    /// <param name="deviceID"></param>
    /// <param name="devicePID"></param>
    public void LoadInputSettings(int deviceID, String devicePID)
    {
        SaveData data = null;
        bool loadDefault = true;
        try
        {
            data = SaveLoad.Load("controllerMap_" + devicePID + ".dta");
            if (!data.error)
            {
                loadDefault = false;
                foreach (GameOptions.InputType iType in Enum.GetValues(typeof(GameOptions.InputType)))
                {
                    inputMap[deviceID][iType] = data.inputMap[iType];
                }
            }
        }
        catch
        {
        }

        if (loadDefault)
        {
            inputMap[deviceID][GameOptions.InputType.Brake] = 105;
            inputMap[deviceID][GameOptions.InputType.Throttle] = 101;
            inputMap[deviceID][GameOptions.InputType.Steer] = 100;
            inputMap[deviceID][GameOptions.InputType.Clutch] = 106;

            inputMap[deviceID][GameOptions.InputType.Handbrake] = 7;
            inputMap[deviceID][GameOptions.InputType.ShiftDown] = 6;
            inputMap[deviceID][GameOptions.InputType.ShiftUp] = 5;
            inputMap[deviceID][GameOptions.InputType.StartEngine] = 1;

            inputMap[deviceID][GameOptions.InputType.FirstGear] = 10;
            inputMap[deviceID][GameOptions.InputType.SecondGear] = 11;
            inputMap[deviceID][GameOptions.InputType.ThirdGear] = 12;
            inputMap[deviceID][GameOptions.InputType.FourthGear] = 13;
            inputMap[deviceID][GameOptions.InputType.FifthGear] = 14;
            inputMap[deviceID][GameOptions.InputType.SixthGear] = 15;
            inputMap[deviceID][GameOptions.InputType.ReverseGear] = 16;

            inputMap[deviceID][GameOptions.InputType.Combined] = 0;
            inputMap[deviceID][GameOptions.InputType.InvertFFB] = -1;
            inputMap[deviceID][GameOptions.InputType.FFBGain] = 18;

        }

        data = null;


    }

    /// <summary>
    /// Save input mappings
    /// </summary>
    public void SaveInputSettings()
    {
        SaveInputSettings(currentDeviceID, DWheel.GetPID(currentDeviceID).ToString());
    }

    /// <summary>
    /// Save input mappings
    /// </summary>
    /// <param name="deviceID"></param>
    /// <param name="devicePID"></param>
    public void SaveInputSettings(int deviceID, String devicePID)
    {
        // Save preferences to disk
        SaveData data = new SaveData();

        foreach (GameOptions.InputType iType in Enum.GetValues(typeof(GameOptions.InputType)))
        {
            if (data.inputMap.ContainsKey(iType)) data.inputMap[iType] = inputMap[deviceID][iType];
            else data.inputMap.Add(iType, inputMap[deviceID][iType]);

        }
        SaveLoad.Save("controllerMap_" + devicePID + ".dta", data);
        data = null;
    }

    /// <summary>
    /// Clear the mapped value of an input map
    /// </summary>
    /// <param name="t"></param>
    public void ClearInput(GameOptions.InputType t)
    {
        ClearInput(currentDeviceID, t);
    }

    /// <summary>
    /// Clear the maped value of an input map
    /// </summary>
    /// <param name="deviceID"></param>
    /// <param name="t"></param>
    public void ClearInput(int deviceID, GameOptions.InputType t)
    {
        inputMap[deviceID][t] = -1;
    }

    //************************************************************************************************************
    // Force Feedback functions
    // Most of these functions are sell explanatory and so comments have not been coded
    //************************************************************************************************************

    public bool IsConnected()
    {
        return IsConnected(currentDeviceID);
    }

    public bool IsConnected(int index)
    {
        return DWheel.IsConnected(index);
    }

    public bool IsGamepad()
    {
        return IsGamepad(currentDeviceID);
    }

    public bool IsGamepad(int index)
    {
        return DWheel.IsGamepad(index);
    }

    public bool HasForceFeedback()
    {
        return HasForceFeedback(currentDeviceID);
    }
    public bool HasForceFeedback(int index)
    {
        return DWheel.HasForceFeedback(index);
    }

    public string GetControllerName()
    {
        return GetControllerName(currentDeviceID);
    }

    public string GetControllerName(int index)
    {
        return DWheel.GetFriendlyProductName(index);
    }

    public void PlayRumble(int leftMotor, int rightMotor)
    {
        PlayRumble(currentDeviceID, leftMotor, rightMotor);
    }

    public void PlayRumble(int index, int leftMotor, int rightMotor)
    {
        DWheel.SetRumble(index, leftMotor, rightMotor);
    }

    public void PlayBumpyRoadEffect(int x)
    {
        PlayBumpyRoadEffect(currentDeviceID, x);
    }

    public void PlayBumpyRoadEffect(int index, int x)
    {
        DWheel.PlayBumpyRoadEffect(index, x);
    }

    public void StopBumpyRoadEffect()
    {
        StopBumpyRoadEffect(currentDeviceID);
    }

    public void StopBumpyRoadEffect(int index)
    {
        DWheel.StopBumpyRoadEffect(index);
    }

    public void PlaySlipperyRoadEffect(int x)
    {
        PlaySlipperyRoadEffect(currentDeviceID, x);
    }

    public void PlaySlipperyRoadEffect(int index, int x)
    {
        DWheel.PlaySlipperyRoadEffect(index, x);
    }

    public void StopSlipperyRoadEffect()
    {
        StopSlipperyRoadEffect(currentDeviceID);
    }

    public void StopSlipperyRoadEffect(int index)
    {
        DWheel.StopSlipperyRoadEffect(index);
    }

    public void PlayDamperForce(int x)
    {
        PlayDamperForce(currentDeviceID, x);
    }

    public void PlayDamperForce(int index, int x)
    {
        DWheel.PlayDamperForce(index, x);
    }

    public void StopDamperForce()
    {
        DWheel.StopDamperForce(currentDeviceID);
    }

    public void StopDamperForce(int index)
    {
        DWheel.StopDamperForce(index);
    }

    public void PlayDirtRoadEffect(int x)
    {
        PlayDirtRoadEffect(currentDeviceID, x);
    }

    public void PlayDirtRoadEffect(int index, int x)
    {
        DWheel.PlayDirtRoadEffect(index, x);
    }

    public void StopDirtRoadEffect()
    {
        StopDirtRoadEffect(currentDeviceID);
    }

    public void StopDirtRoadEffect(int index)
    {
        DWheel.StopDirtRoadEffect(index);
    }

    public void PlaySpringForce(Int32 offsetPercentage, Int32 saturationPercentage, Int32 coefficientPercentage)
    {
        PlaySpringForce(currentDeviceID, offsetPercentage, saturationPercentage, coefficientPercentage);
    }

    public void PlaySpringForce(int index, Int32 offsetPercentage, Int32 saturationPercentage, Int32 coefficientPercentage)
    {
        DWheel.PlaySpringForce(index, offsetPercentage, saturationPercentage, coefficientPercentage);
    }

    public void StopSpringForce()
    {
        StopSpringForce(currentDeviceID);
    }

    public void StopSpringForce(int index)
    {
        DWheel.StopSpringForce(index);
    }

    public void PlayAirbourne()
    {
        DWheel.PlayCarAirborne(currentDeviceID);
    }

    public void PlayAirbourne(int index)
    {
        DWheel.PlayCarAirborne(index);
    }

    public void StopAirbourne()
    {
        DWheel.StopCarAirborne(currentDeviceID);
    }

    public void StopAirbourne(int index)
    {
        DWheel.StopCarAirborne(index);
    }

    public void PlayFrontalCollisionForce(int x)
    {
        DWheel.PlayFrontalCollisionForce(currentDeviceID, x);
    }

    public void PlayFrontalCollisionForce(int index, int x)
    {
        DWheel.PlayFrontalCollisionForce(index, x);
    }

    public void PlaySideCollisionForce(int x)
    {
        PlaySideCollisionForce(currentDeviceID, x);
    }

    public void PlaySideCollisionForce(int index, int x)
    {
        DWheel.PlaySideCollisionForce(index, x);
    }

    public void SetLeds(float value, float min, float max)
    {
        SetLeds(currentDeviceID, value, min, max);
    }

    public void SetLeds(int index, float value, float min, float max)
    {
        DWheel.SetLED(index, value, min, max);
    }

    public void PlayConstantForce(int forceDirection)
    {
        DWheel.PlayConstantForce(currentDeviceID, forceDirection);
    }

    public void PlayConstantForce(int index, int forceDirection)
    {
        DWheel.PlayConstantForce(index, forceDirection);
    }

    public void StopConstantForce()
    {
        StopConstantForce(currentDeviceID);
    }

    public void StopConstantForce(int index)
    {
        DWheel.StopConstantForce();
    }

    public void SetForceCoordinate(float forceDirection)
    {
        SetForceCoordinate(currentDeviceID, forceDirection);
    }

    public void SetForceCoordinate(int index, float forceDirection)
    {
        DWheel.PlayConstantForce(index, (int)(forceDirection * (float)inputMap[index][GameOptions.InputType.InvertFFB] * ((float)inputMap[index][GameOptions.InputType.FFBGain] / 10f)));
    }

    public void SetForce2DCoordinate(float x, float y)
    {
        SetForce2DCoordinate(currentDeviceID, x, y);
    }

    public void SetForce2DCoordinate(int index, float x, float y)
    {
        DWheel.PlayConstant2DForce(index, (int)x, (int)y);
    }



    //public void SetForceCoordinate(int index, float forceDirection)
    //{

    //    try
    //    {
    //        if (DWheel.HasForceFeedback())
    //        {
    //            if (DWheel.IsGamepad())
    //            {
    //                if (Mathf.Abs(forceDirection) < 90f)
    //                    DWheel.SetRumble(0, 0);
    //                else if (forceDirection < 0)
    //                    DWheel.SetRumble(index, (int)(forceDirection * -1000f), 0);
    //                else
    //                    DWheel.SetRumble(index, 0, (int)(forceDirection * 1000f));
    //            }
    //            else
    //            {
    //                DWheel.PlayConstantForce(index, (int)(forceDirection / -1.8f));
    //            }
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        MonoBehaviour.print("Error SetForceCoordinate: " + e.Message);
    //    }

    //}


    //************************************************************************************************************
    // Functions to retrieve input values from gaming device
    //************************************************************************************************************

    public float XAxis(int index)
    {
        return js[index].X;
    }

    public float YAxis(int index)
    {
        return js[index].Y;
    }

    public float ZAxis(int index)
    {
        return js[index].Z;
    }

    public float XRot(int index)
    {
        return js[index].RotationX;
    }

    public float YRot(int index)
    {
        return js[index].RotationY;
    }

    public float ZRot(int index)
    {
        return js[index].RotationZ;
    }

    public float Slider0(int index)
    {
        return js[index].Sliders0;
    }

    public float Slider1(int index)
    {
        return js[index].Sliders1;
    }

    public bool Button0(int index)
    {
        return js[index].Buttons0 != 0;
    }

    public bool Button1(int index)
    {
        return js[index].Buttons1 != 0;
    }
    public bool Button2(int index)
    {
        return js[index].Buttons2 != 0;
    }
    public bool Button3(int index)
    {
        return js[index].Buttons3 != 0;
    }
    public bool Button4(int index)
    {
        return js[index].Buttons4 != 0;
    }
    public bool Button5(int index)
    {
        return js[index].Buttons5 != 0;
    }
    public bool Button6(int index)
    {
        return js[index].Buttons6 != 0;
    }
    public bool Button7(int index)
    {
        return js[index].Buttons7 != 0;
    }
    public bool Button8(int index)
    {
        return js[index].Buttons8 != 0;
    }
    public bool Button9(int index)
    {
        return js[index].Buttons9 != 0;
    }
    public bool Button10(int index)
    {
        return js[index].Buttons10 != 0;
    }
    public bool Button11(int index)
    {
        return js[index].Buttons11 != 0;
    }
    public bool Button12(int index)
    {
        return js[index].Buttons12 != 0;
    }
    public bool Button13(int index)
    {
        return js[index].Buttons13 != 0;
    }
    public bool Button14(int index)
    {
        return js[index].Buttons14 != 0;
    }
    public bool Button15(int index)
    {
        return js[index].Buttons15 != 0;
    }
    public bool Button16(int index)
    {
        return js[index].Buttons16 != 0;
    }
    public bool Button17(int index)
    {
        return js[index].Buttons17 != 0;
    }
    public bool Button18(int index)
    {
        return js[index].Buttons18 != 0;
    }
    public bool Button19(int index)
    {
        return js[index].Buttons19 != 0;
    }
    public bool Button20(int index)
    {
        return js[index].Buttons20 != 0;
    }
    public bool Button21(int index)
    {
        return js[index].Buttons21 != 0;
    }
    public bool Button22(int index)
    {
        return js[index].Buttons22 != 0;
    }
    public bool Button23(int index)
    {
        return js[index].Buttons23 != 0;
    }
    public bool Button24(int index)
    {
        return js[index].Buttons24 != 0;
    }
    public bool Button25(int index)
    {
        return js[index].Buttons25 != 0;
    }
    public bool Button26(int index)
    {
        return js[index].Buttons26 != 0;
    }
    public bool Button27(int index)
    {
        return js[index].Buttons27 != 0;
    }
    public bool Button28(int index)
    {
        return js[index].Buttons28 != 0;
    }
    public bool Button29(int index)
    {
        return js[index].Buttons29 != 0;
    }
    public bool Button30(int index)
    {
        return js[index].Buttons30 != 0;
    }
    public bool Button31(int index)
    {
        return js[index].Buttons31 != 0;
    }
    public bool Button32(int index)
    {
        return js[index].Buttons32 != 0;
    }
    public bool Button33(int index)
    {
        return js[index].Buttons33 != 0;
    }
    public bool Button34(int index)
    {
        return js[index].Buttons34 != 0;
    }
    public bool Button35(int index)
    {
        return js[index].Buttons35 != 0;
    }
    public bool Button36(int index)
    {
        return js[index].Buttons36 != 0;
    }
    public bool Button37(int index)
    {
        return js[index].Buttons37 != 0;
    }
    public bool Button38(int index)
    {
        return js[index].Buttons38 != 0;
    }
    public bool Button39(int index)
    {
        return js[index].Buttons39 != 0;
    }
    public bool Button40(int index)
    {
        return js[index].Buttons40 != 0;
    }

    public int POV0(int index)
    {
        return js[index].PointOfViewControllers0;
    }
    public int POV1(int index)
    {
        return js[index].PointOfViewControllers1;
    }
    public int POV2(int index)
    {
        return js[index].PointOfViewControllers2;
    }
    public int POV3(int index)
    {
        return js[index].PointOfViewControllers3;
    }

    public int ForceSlider0(int index)
    {
        return js[index].ForceSliders0;
    }

    public int ForceSlider1(int index)
    {
        return js[index].ForceSliders1;
    }


    public int ForceX(int index)
    {
        return js[index].ForceX;
    }

    public int ForceY(int index)
    {
        return js[index].ForceY;
    }

    public int ForceZ(int index)
    {
        return js[index].ForceZ;
    }


    public int TorqueX(int index)
    {
        return js[index].TorqueX;
    }

    public int TorqueY(int index)
    {
        return js[index].TorqueY;
    }

    public int TorqueZ(int index)
    {
        return js[index].TorqueZ;
    }


    public int VelocitySliders0(int index)
    {
        return js[index].VelocitySliders0;
    }

    public int VelocitySliders1(int index)
    {
        return js[index].VelocitySliders1;
    }


    public int VelocityX(int index)
    {
        return js[index].VelocityX;
    }

    public int VelocityY(int index)
    {
        return js[index].VelocityY;
    }

    public int VelocityZ(int index)
    {
        return js[index].VelocityZ;
    }

}

//**************************************************************************************************************************
// Disk Save/Load functions for input mapping
//**************************************************************************************************************************

public class GameOptions
{
    public enum InputType { Throttle, Brake, Steer, Clutch, Handbrake, ShiftUp, ShiftDown, StartEngine, FirstGear, SecondGear, ThirdGear, FourthGear, FifthGear, SixthGear, ReverseGear, Combined, InvertFFB, FFBGain, None };

}

// === This is the info container class ===
[Serializable()]
public class SaveData : ISerializable
{

    // === Values ===
    // Edit these during gameplay
    public Dictionary<GameOptions.InputType, int> inputMap = new Dictionary<GameOptions.InputType, int>();
    public bool error = false;
    public string errorMsg = "";

    // The default constructor. Included for when we call it during Save() and Load()
    public SaveData() { }

    public SaveData(SerializationInfo info, StreamingContext ctxt)
    {

        foreach (GameOptions.InputType iType in Enum.GetValues(typeof(GameOptions.InputType)))
        {
            if (inputMap.ContainsKey(iType)) inputMap[iType] = (int)info.GetValue(iType.ToString(), typeof(int));
            else inputMap.Add(iType, (int)info.GetValue(iType.ToString(), typeof(int)));
        }


    }

    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {

        foreach (GameOptions.InputType iType in Enum.GetValues(typeof(GameOptions.InputType)))
        {
            if (inputMap.ContainsKey(iType)) info.AddValue(iType.ToString(), inputMap[iType]);
            else info.AddValue(iType.ToString(), -1);
        }
    }
}

public class SaveLoad
{

    public static string currentFilePath = "ZRacerSaveData.dta";    // Edit this for different save files

    public static void Save(SaveData data)  // Overloaded
    {
        Save(currentFilePath, data);
    }
    public static void Save(string filePath, SaveData data)
    {

        Stream stream = File.Open(filePath, FileMode.Create);
        BinaryFormatter bformatter = new BinaryFormatter();
        bformatter.Binder = new VersionDeserializationBinder();
        bformatter.Serialize(stream, data);
        stream.Close();
    }

    public static SaveData Load() { return Load(currentFilePath); }
    public static SaveData Load(string filePath)
    {
        try
        {
            SaveData data = new SaveData();
            Stream stream = File.Open(filePath, FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Binder = new VersionDeserializationBinder();
            data = (SaveData)bformatter.Deserialize(stream);
            stream.Close();
            return data;
        }
        catch
        {
            SaveData data = new SaveData();
            data.error = true;

            return data;
        }
    }

}

public sealed class VersionDeserializationBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeName))
        {
            Type typeToDeserialize = null;

            assemblyName = Assembly.GetExecutingAssembly().FullName;

            typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));

            return typeToDeserialize;
        }

        return null;
    }
}
