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
// WheelMenu combined with WheelInput & InputScan provides a solution for integrating DirectInput & Force Feedback functions with a car controller
// These classes can be used as is or modified for your custom requirements.
//
// This class will:
//          - automaticlly attach to the first gaming device available
//          - provides functions to send force feedback to the device
//          - Save/Load mapping to common driving functions
//          - Functions to access current input values
//
//========================================================================================================================
using UnityEngine;
using System.Collections;

public class WheelMenu : MonoBehaviour
{

    // Enable the menu to be displayed
    public bool enableMenu = true;

    // Show menu at startup
    public bool showAtStartup = false;

    // Show the Force feedback test functions on menu
    public bool showFFBTest = false;

    // Show the raw input of the controller instead of mapping menu
    public bool showRawInput = false;

    bool isEnabled = false;

    // Variables to control Menu
    Rect GridRect;
    int GridInt;
    int oldGridInt;
    string[] GridEntrys;
    Rect ToggleRect;
    Vector2 StartSize = new Vector2(1024.0f, 768.0f); //start resolution of window
    Vector2 MenuSize = new Vector2(800.0f, 600.0f);

    // Reference to WheelInput class
    public WheelInput dinput;

    // Reference to inputScan class
    InputScan inputScan;

    // Menu layout Rect's
    Rect failRect = new Rect();
    Rect controllerRect = new Rect();
    Rect controllerPrevRect = new Rect();
    Rect controllerNextRect = new Rect();
    Rect[] labelRect;
    Rect[] inputRect1;
    Rect[] inputRect2;
    Rect[] rawRect;
    Rect[] ffRect;

    Rect SettingsWindowRect;
    Rect WindowFooterRect;

    int i, floor, top;
    int entrysCount;
    bool showPopupDialog = false;
    string message;

    // Variables to control force feedback testing
    bool bumpyRoad = false;
    bool oldBumpyRoad = false;

    bool slipperyRoad = false;
    bool oldSlipperyRoad = false;

    bool damper = false;
    bool oldDamper = false;

    bool dirtRoad = false;
    bool oldDirtRoad = false;

    bool springForce = false;
    bool oldSpringForce = false;

    bool airbourne = false;
    bool oldAirbourne = false;

    bool constForce = false;
    bool oldConstantForce = false;
    float force = 0f;
    float oldForce = 0f;

    float led = 0f;
    float oldLed = 0f;


    void Awake()
    {
        labelRect = new Rect[30];
        inputRect1 = new Rect[30];
        inputRect2 = new Rect[30];
        rawRect = new Rect[90];
        ffRect = new Rect[30];
        GridInt = 0;
        StartSize = new Vector2(Screen.width, Screen.height);
        RectCalculation(StartSize);
        GridEntrys = new string[] { "Controller Setup", "Save" };
        entrysCount = 2;

        try
        {
            inputScan = gameObject.AddComponent<InputScan>();
            dinput = WheelInput.Instance;
        }
        catch { print("Error instantiating WheelInput"); }

        if (showAtStartup) IsEnabled = true;

    }

    public bool IsEnabled
    {
        get { return isEnabled; }
        set
        {
            if (isEnabled != value)
            {
                isEnabled = value;
            }
        }

    }

    void OnDestroy()
    {
        IsEnabled = false;
        if (dinput != null)
        {
            dinput.SelfDestruct();
            dinput = null;
        }
    }

    void Update()
    {

        // Display menu when "M" pressed
        if (Input.GetKeyUp(KeyCode.M) && enableMenu)
        {
            if (dinput != null) IsEnabled = !IsEnabled;
        }

        // Get current input values for default controller
        if (IsEnabled && dinput != null) dinput.GetInput();
    }


    /// <summary>
    /// Display Confirmation message popup
    /// </summary>
    /// <param name="windowID"></param>
    void ShowPopupDialog(int windowID)
    {
        GUI.Label(new Rect(20, 20, 200, 40), message);
        if (GUI.Button(new Rect(80, 60, 60, 30), "OK")) showPopupDialog = !showPopupDialog;
    }

    /// <summary>
    /// GUI event.
    /// </summary>
    void OnGUI()
    {

        // Show the confirmation popup window
        if (showPopupDialog)
        {
            GUI.Window(1, new Rect(Screen.width / 2 - 110, Screen.height / 2 - 40, 220, 100), ShowPopupDialog, "Wheel Setup");
            GUI.BringWindowToFront(1);
        }

        // Show the menu
        if (IsEnabled == true)
        {
            if (showRawInput)
            {
                SettingsWindowRect = new Rect((StartSize.x - MenuSize.x) / 2, (StartSize.y - MenuSize.y) / 2, MenuSize.x, MenuSize.y);
                // Show rawinput menu
                if (dinput != null)
                    SettingsWindowRect = GUI.Window(10, SettingsWindowRect, RawInputWindow, "Raw Input");
                else
                    SettingsWindowRect = GUI.Window(10, SettingsWindowRect, DinputFail, "Raw Input");

            }
            else
            {
                SettingsWindowRect = new Rect((StartSize.x - MenuSize.x) / 2, (StartSize.y - MenuSize.y) / 2, MenuSize.x, MenuSize.y);
                // Show mapping menu
                if (dinput != null)
                    SettingsWindowRect = GUI.Window(10, SettingsWindowRect, SettingsWindow, "Wheel Settings");
                else
                    SettingsWindowRect = GUI.Window(10, SettingsWindowRect, DinputFail, "Raw Input");
            }
        }

    }

    void DinputFail(int windowID)
    {
        if (IsEnabled == true)
        {
            GUI.Label(failRect, "Force Feedback unavailable. Check FFWheelInput.dll is in root directory");
        }
        string text = (IsEnabled == true ? "Close Window" : "Show Input Window");
        IsEnabled = GUI.Toggle(ToggleRect, IsEnabled, text);
    }

    void RawInputWindow(int windowID)
    {
        if (IsEnabled == true)
        {
            RawInput();
        }
        string text = (IsEnabled == true ? "Close Window" : "Show Input Window");
        IsEnabled = GUI.Toggle(ToggleRect, IsEnabled, text);
    }

    void SettingsWindow(int windowID)
    {
        if (IsEnabled == true)
        {
            oldGridInt = GridInt;
            GridInt = GUI.SelectionGrid(GridRect, GridInt, GridEntrys, entrysCount);
            switch (GridInt)
            {
                case 0: GridInt = 0;
                    InputMapping();
                    if (showFFBTest) FFTest();
                    break;
                case 1: GridInt = 0; SaveSetup(); break;
            }
        }
        string text = (IsEnabled == true ? "Close Window" : "Show Setup Window");
        IsEnabled = GUI.Toggle(ToggleRect, IsEnabled, text);
    }

    void SaveSetup()
    {
        GridInt = oldGridInt;
        WheelInput.Instance.SaveInputSettings();
        message = "Setup succesfully saved.";
        showPopupDialog = true;
    }


    // GUI Display for Force Feedback testing
    void FFTest()
    {
        int p = 0;

        bumpyRoad = GUI.Toggle(ffRect[p], bumpyRoad, "Bumpy Road");
        if (bumpyRoad != oldBumpyRoad)
        {
            if (bumpyRoad) dinput.PlayBumpyRoadEffect(50);
            else dinput.StopBumpyRoadEffect();
            oldBumpyRoad = bumpyRoad;
        }

        p++;
        slipperyRoad = GUI.Toggle(ffRect[p], slipperyRoad, "Slippery Road");
        if (slipperyRoad != oldSlipperyRoad)
        {
            if (slipperyRoad) dinput.PlaySlipperyRoadEffect(50);
            else dinput.StopSlipperyRoadEffect();
            oldSlipperyRoad = slipperyRoad;
        }

        p++;
        damper = GUI.Toggle(ffRect[p], damper, "Damper Force");
        if (damper != oldDamper)
        {
            if (damper) dinput.PlayDamperForce(90);
            else dinput.StopDamperForce();
            oldDamper = damper;
        }

        p++;
        dirtRoad = GUI.Toggle(ffRect[p], dirtRoad, "Dirt Road");
        if (dirtRoad != oldDirtRoad)
        {
            if (dirtRoad) dinput.PlayDirtRoadEffect(50);
            else dinput.StopDirtRoadEffect();
            oldDirtRoad = dirtRoad;
        }


        p++;
        springForce = GUI.Toggle(ffRect[p], springForce, "Spring Force");
        if (springForce != oldSpringForce)
        {
            if (springForce) dinput.PlaySpringForce(30, 20, 80);
            else dinput.StopSpringForce();
            oldSpringForce = springForce;
        }

        p++;
        airbourne = GUI.Toggle(ffRect[p], airbourne, "Airbourne");
        if (airbourne != oldAirbourne)
        {
            if (airbourne) dinput.PlayAirbourne();
            else dinput.StopAirbourne();
            oldAirbourne = airbourne;
        }

        p++;
        constForce = GUI.Toggle(ffRect[p], constForce, "Constant Force");
        p++;
        force = GUI.HorizontalSlider(ffRect[p], force, -100f, 100f);
        if (force != oldForce || constForce != oldConstantForce)
        {
            if (constForce)
            {
                dinput.PlayConstantForce((int)force);
            }
            else
            {
                dinput.StopConstantForce();
            }
            oldConstantForce = constForce;
            oldForce = force;
        }

        p = 10;
        if (GUI.Button(ffRect[p], "Front Collision")) dinput.PlayFrontalCollisionForce(80);

        p++;
        if (GUI.Button(ffRect[p], "Side Collision")) dinput.PlaySideCollisionForce(-80);

        p++;
        GUI.Label(ffRect[p], "LED: ");
        p++;
        led = GUI.HorizontalSlider(ffRect[p], led, 0f, 5000f);
        if (led != oldLed)
        {
            dinput.SetLeds(led, 0, 5000);
            oldLed = led;
        }

    }

    // Keyboard and Controller input mapping and setup
    void InputMapping()
    {

        GUI.Label(controllerRect, dinput.GetControllerName() + "(" + dinput.currentDeviceID + ")");
        if (GUI.Button(controllerPrevRect, "Prev")) dinput.PrevController();
        if (GUI.Button(controllerNextRect, "Next")) dinput.NextController();

        int pos = 0;
        GUI.Label(labelRect[pos], "Steering: " + dinput.getName(GameOptions.InputType.Steer) + "  (" + Mathf.Round(dinput.GetSteer(1) * 1000) + ")");
        if (inputScan.scanType == GameOptions.InputType.None)
        {
            if (GUI.Button(inputRect1[pos], "Set")) inputScan.SetInput(GameOptions.InputType.Steer);
            if (GUI.Button(inputRect2[pos], "Clear")) dinput.ClearInput(GameOptions.InputType.Steer);
        }
        else if (inputScan.scanType == GameOptions.InputType.Steer) GUI.Label(inputRect1[pos], "Scanning.....");

        pos++;
        GUI.Label(labelRect[pos], "Throttle: " + dinput.getName(GameOptions.InputType.Throttle) + "(" + dinput.GetThrottle() + ")");
        if (inputScan.scanType == GameOptions.InputType.None)
        {
            if (GUI.Button(inputRect1[pos], "Set")) inputScan.SetInput(GameOptions.InputType.Throttle);
            if (GUI.Button(inputRect2[pos], "Clear")) dinput.ClearInput(GameOptions.InputType.Throttle);
        }
        else if (inputScan.scanType == GameOptions.InputType.Throttle) GUI.Label(inputRect1[pos], "Scanning.....");

        pos++;
        dinput.SetCombined(GUI.Toggle(labelRect[pos], dinput.Combined(), "Combined") ? 1 : 0);

        pos++;
        if (dinput.Combined())
        {
            GUI.Label(labelRect[pos], "Brake: " + dinput.getName(GameOptions.InputType.Throttle) + "(" + dinput.GetBrake() + ")");
        }
        else
        {
            GUI.Label(labelRect[pos], "Brake: " + dinput.getName(GameOptions.InputType.Brake) + "(" + dinput.GetBrake() + ")");
            if (inputScan.scanType == GameOptions.InputType.None)
            {
                if (GUI.Button(inputRect1[pos], "Set")) inputScan.SetInput(GameOptions.InputType.Brake);
                if (GUI.Button(inputRect2[pos], "Clear")) dinput.ClearInput(GameOptions.InputType.Brake);
            }
            else if (inputScan.scanType == GameOptions.InputType.Brake) GUI.Label(inputRect1[pos], "Scanning.....");
        }

        pos++;
        GUI.Label(labelRect[pos], "Clutch: " + dinput.getName(GameOptions.InputType.Clutch) + "(" + dinput.GetClutch() + ")");
        if (inputScan.scanType == GameOptions.InputType.None)
        {
            if (GUI.Button(inputRect1[pos], "Set")) inputScan.SetInput(GameOptions.InputType.Clutch);
            if (GUI.Button(inputRect2[pos], "Clear")) dinput.ClearInput(GameOptions.InputType.Clutch);
        }
        else if (inputScan.scanType == GameOptions.InputType.Clutch) GUI.Label(inputRect1[pos], "Scanning.....");

        pos++;
        GUI.Label(labelRect[pos], "Handbrake: " + dinput.getName(GameOptions.InputType.Handbrake) + "(" + dinput.GetHandbrake() + ")");
        if (inputScan.scanType == GameOptions.InputType.None)
        {
            if (GUI.Button(inputRect1[pos], "Set")) inputScan.SetInput(GameOptions.InputType.Handbrake);
            if (GUI.Button(inputRect2[pos], "Clear")) dinput.ClearInput(GameOptions.InputType.Handbrake);
        }
        else if (inputScan.scanType == GameOptions.InputType.Handbrake) GUI.Label(inputRect1[pos], "Scanning.....");

        pos++;
        GUI.Label(labelRect[pos], "Shift Up: " + dinput.getName(GameOptions.InputType.ShiftUp) + "(" + dinput.getInputStatus(GameOptions.InputType.ShiftUp) + ")");
        if (inputScan.scanType == GameOptions.InputType.None)
        {
            if (GUI.Button(inputRect1[pos], "Set")) inputScan.SetInput(GameOptions.InputType.ShiftUp);
            if (GUI.Button(inputRect2[pos], "Clear")) dinput.ClearInput(GameOptions.InputType.ShiftUp);
        }
        else if (inputScan.scanType == GameOptions.InputType.ShiftUp) GUI.Label(inputRect1[pos], "Scanning.....");

        pos++;
        GUI.Label(labelRect[pos], "Shift Down: " + dinput.getName(GameOptions.InputType.ShiftDown) + "(" + dinput.getInputStatus(GameOptions.InputType.ShiftDown) + ")");
        if (inputScan.scanType == GameOptions.InputType.None)
        {
            if (GUI.Button(inputRect1[pos], "Set")) inputScan.SetInput(GameOptions.InputType.ShiftDown);
            if (GUI.Button(inputRect2[pos], "Clear")) dinput.ClearInput(GameOptions.InputType.ShiftDown);
        }
        else if (inputScan.scanType == GameOptions.InputType.ShiftDown) GUI.Label(inputRect1[pos], "Scanning.....");


        GUI.Label(labelRect[10], "First Gear: " + dinput.getName(GameOptions.InputType.FirstGear) + "(" + dinput.getInputStatus(GameOptions.InputType.FirstGear) + ")");
        if (inputScan.scanType == GameOptions.InputType.None)
        {
            if (GUI.Button(inputRect1[10], "Set")) inputScan.SetInput(GameOptions.InputType.FirstGear);
            if (GUI.Button(inputRect2[10], "Clear")) dinput.ClearInput(GameOptions.InputType.FirstGear);
        }
        else if (inputScan.scanType == GameOptions.InputType.FirstGear) GUI.Label(inputRect1[10], "Scanning.....");

        GUI.Label(labelRect[11], "Second Gear: " + dinput.getName(GameOptions.InputType.SecondGear) + "(" + dinput.getInputStatus(GameOptions.InputType.SecondGear) + ")");
        if (inputScan.scanType == GameOptions.InputType.None)
        {
            if (GUI.Button(inputRect1[11], "Set")) inputScan.SetInput(GameOptions.InputType.SecondGear);
            if (GUI.Button(inputRect2[11], "Clear")) dinput.ClearInput(GameOptions.InputType.SecondGear);
        }
        else if (inputScan.scanType == GameOptions.InputType.SecondGear) GUI.Label(inputRect1[11], "Scanning.....");

        GUI.Label(labelRect[12], "Third Gear: " + dinput.getName(GameOptions.InputType.ThirdGear) + "(" + dinput.getInputStatus(GameOptions.InputType.ThirdGear) + ")");
        if (inputScan.scanType == GameOptions.InputType.None)
        {
            if (GUI.Button(inputRect1[12], "Set")) inputScan.SetInput(GameOptions.InputType.ThirdGear);
            if (GUI.Button(inputRect2[12], "Clear")) dinput.ClearInput(GameOptions.InputType.ThirdGear);
        }
        else if (inputScan.scanType == GameOptions.InputType.ThirdGear) GUI.Label(inputRect1[12], "Scanning.....");

        GUI.Label(labelRect[13], "Fourth Gear: " + dinput.getName(GameOptions.InputType.FourthGear) + "(" + dinput.getInputStatus(GameOptions.InputType.FourthGear) + ")");
        if (inputScan.scanType == GameOptions.InputType.None)
        {
            if (GUI.Button(inputRect1[13], "Set")) inputScan.SetInput(GameOptions.InputType.FourthGear);
            if (GUI.Button(inputRect2[13], "Clear")) dinput.ClearInput(GameOptions.InputType.FourthGear);
        }
        else if (inputScan.scanType == GameOptions.InputType.FourthGear) GUI.Label(inputRect1[13], "Scanning.....");

        GUI.Label(labelRect[14], "Fifth Gear: " + dinput.getName(GameOptions.InputType.FifthGear) + "(" + dinput.getInputStatus(GameOptions.InputType.FifthGear) + ")");
        if (inputScan.scanType == GameOptions.InputType.None)
        {
            if (GUI.Button(inputRect1[14], "Set")) inputScan.SetInput(GameOptions.InputType.FifthGear);
            if (GUI.Button(inputRect2[14], "Clear")) dinput.ClearInput(GameOptions.InputType.FifthGear);
        }
        else if (inputScan.scanType == GameOptions.InputType.FifthGear) GUI.Label(inputRect1[14], "Scanning.....");

        GUI.Label(labelRect[15], "Sixth Gear: " + dinput.getName(GameOptions.InputType.SixthGear) + "(" + dinput.getInputStatus(GameOptions.InputType.SixthGear) + ")");
        if (inputScan.scanType == GameOptions.InputType.None)
        {
            if (GUI.Button(inputRect1[15], "Set")) inputScan.SetInput(GameOptions.InputType.SixthGear);
            if (GUI.Button(inputRect2[15], "Clear")) dinput.ClearInput(GameOptions.InputType.SixthGear);
        }
        else if (inputScan.scanType == GameOptions.InputType.SixthGear) GUI.Label(inputRect1[15], "Scanning.....");

        GUI.Label(labelRect[16], "Reverse Gear: " + dinput.getName(GameOptions.InputType.ReverseGear) + "(" + dinput.getInputStatus(GameOptions.InputType.ReverseGear) + ")");
        if (inputScan.scanType == GameOptions.InputType.None)
        {
            if (GUI.Button(inputRect1[16], "Set")) inputScan.SetInput(GameOptions.InputType.ReverseGear);
            if (GUI.Button(inputRect2[16], "Clear")) dinput.ClearInput(GameOptions.InputType.ReverseGear);
        }
        else if (inputScan.scanType == GameOptions.InputType.ReverseGear) GUI.Label(inputRect1[16], "Scanning.....");

        dinput.SetInverted(GUI.Toggle(labelRect[17], dinput.GetInverted(), "Force Inverted  ==> Gain") ? 1 : 0);
        dinput.SetGain((int)GUI.HorizontalSlider(inputRect1[17], dinput.GetGain(), 1f, 30f));


    }

    void RawInput()
    {
        for (int i = 0; i < 25; i++)
        {
            GUI.Label(rawRect[i], dinput.getAnalogueName(i + 100) + "  (" + Mathf.Round(dinput.getAnalogue(dinput.currentDeviceID, i + 100)) + ")");

        }

        for (int i = 0; i < 25; i++)
        {
            GUI.Label(rawRect[i + 30], dinput.getButtonName(i) + "  (" + (dinput.getButton(dinput.currentDeviceID, i) ? "On" : "Off") + ")");

        }
    }


    void OnApplicationFocus(bool focus)
    {
        // If constant force was running when we lost focus, we need to stop it now. Otherwise it will stop working
        if (focus)
        {
            if (WheelInput.Instance != null)
                WheelInput.Instance.StopConstantForce();
        }
    }


    void OnApplicationQuit()
    {
    }

    // Generate Rects's for displaying information on GUI
    void RectCalculation(Vector2 Size)
    {
        ToggleRect = new Rect(8.0f, 0.0f, 200.0f, 20.0f);

        SettingsWindowRect = new Rect((StartSize.x - MenuSize.x) / 2, (StartSize.y - MenuSize.y) / 2, MenuSize.x, MenuSize.y);
        GridRect = new Rect(10.0f, 20.0f, SettingsWindowRect.width - 20.0f, 20.0f);

        float LeftOffset = ((SettingsWindowRect.height - 50.0f) / 20.0f) / 5.0f;

        failRect = new Rect(10.0f, 50.0f, 600f, 22f);
        controllerRect = new Rect(10.0f, 50.0f, 220f, 22f);
        controllerPrevRect = new Rect(controllerRect.x + 230f, controllerRect.y, 100f, 22f);
        controllerNextRect = new Rect(controllerPrevRect.x + 110f, controllerRect.y, 100f, 22f);

        for (int a = 0; a < 10; a++)
        {
            labelRect[a] = new Rect(10.0f, 100.0f + (a * (25.0f + LeftOffset)), 160f, 22f);
            inputRect1[a] = new Rect(labelRect[a].x + 170f, labelRect[a].y, 100f, 22f);
            inputRect2[a] = new Rect(inputRect1[a].x + 110f, labelRect[a].y, 100f, 22f);
        }

        for (int b = 0; b < 10; b++)
        {
            labelRect[b + 10] = new Rect(SettingsWindowRect.width / 2.0f + 10.0f, 100.0f + (b * (25.0f + LeftOffset)), 160f, 22f);
            inputRect1[b + 10] = new Rect(labelRect[b + 10].x + 170f, labelRect[b + 10].y, 100f, 22f);
            inputRect2[b + 10] = new Rect(inputRect1[b + 10].x + 110f, labelRect[b + 10].y, 100f, 22f);
        }

        for (int a = 0; a < 30; a++)
        {
            rawRect[a] = new Rect(10.0f, 100.0f + (a * (15.0f + LeftOffset)), 160f, 22f);
            rawRect[a + 30] = new Rect(160.0f, 100.0f + (a * (15.0f + LeftOffset)), 160f, 22f);
            rawRect[a + 60] = new Rect(310.0f, 100.0f + (a * (15.0f + LeftOffset)), 160f, 22f);
        }

        for (int a = 0; a < 10; a++)
        {
            ffRect[a] = new Rect(10.0f, 350.0f + (a * (25.0f + LeftOffset)), 160f, 22f);
        }

        for (int a = 0; a < 10; a++)
        {
            ffRect[a + 10] = new Rect(SettingsWindowRect.width / 2.0f + 10.0f, 350.0f + (a * (25.0f + LeftOffset)), 160f, 22f);
        }

    }


}

