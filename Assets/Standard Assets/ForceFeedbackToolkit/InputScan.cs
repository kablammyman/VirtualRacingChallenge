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
// InputScan
//
// Scans input for activity and return source of first activity 
//
//========================================================================================================================
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class InputScan : MonoBehaviour
{

    private Dictionary<int, float> scanInputMin = new Dictionary<int, float>();
    private Dictionary<int, float> scanInputMax = new Dictionary<int, float>();
    private Int64 scanCount = 0;
    public GameOptions.InputType scanType = GameOptions.InputType.None;

    /// <summary>
    /// Update the input source for a given input type.
    /// </summary>
    /// <param name="t">The input type to change input source for</param>
    public void SetInput(GameOptions.InputType t)
    {
        scanInputMax.Clear();
        scanInputMin.Clear();
        StartCoroutine(WaitForInput(t));
    }

    /// <summary>
    /// Wait for user input and update input source for an input type
    /// </summary>
    /// <param name="t">The input type to change the input source for</param>
    /// <returns>Nothing</returns>
    IEnumerator WaitForInput(GameOptions.InputType t)
    {
        DateTime waitTime = DateTime.Now;
        int scanFound = -1;

        // Wait until user inputs something
        do
        {

            scanFound = ScanInput(t);
            if (scanFound < 0)
                yield return new WaitForSeconds(0.1f);
            else
            {
                WheelInput.Instance.inputMap[WheelInput.Instance.currentDeviceID][t] = scanFound;
                scanType = GameOptions.InputType.None;
            }

        } while (scanFound < 0 && (DateTime.Now - waitTime).Seconds < 7);



    }

    public int ScanInput(GameOptions.InputType iType)
    {

        int scanFound = -1;

        if (scanType != iType)
        {
            scanType = iType;
        }
        else
        {

            // Scan through last input and see if it has changed
            for (int i = 100; i < 125; i++)
            {
                if (scanInputMin.ContainsKey(i))
                    if (scanCount > 3 && (Abs(scanInputMin[i] - scanInputMax[i]) > 10000)) scanFound = i;
            }
            for (int i = 0; i < 25; i++)
            {
                if (scanInputMin.ContainsKey(i))
                    if (scanCount > 3 && (scanInputMin[i] != scanInputMax[i])) scanFound = i;
            }
        }


        // If we haven't found an input yet update dictionary with latest value
        if (scanFound < 0)
        {
            for (int i = 100; i < 125; i++)
            {
                if (scanInputMin.ContainsKey(i))
                {
                    scanInputMin[i] = Mathf.Min(scanInputMin[i], WheelInput.Instance.getAnalogue(WheelInput.Instance.currentDeviceID, i));
                    scanInputMax[i] = Mathf.Max(scanInputMax[i], WheelInput.Instance.getAnalogue(WheelInput.Instance.currentDeviceID, i));
                }
                else
                {
                    scanInputMin.Add(i, WheelInput.Instance.getAnalogue(WheelInput.Instance.currentDeviceID, i));
                    scanInputMax.Add(i, WheelInput.Instance.getAnalogue(WheelInput.Instance.currentDeviceID, i));
                }
            }
            for (int i = 0; i < 25; i++)
            {
                if (scanInputMin.ContainsKey(i))
                {
                    scanInputMin[i] = Mathf.Min(scanInputMin[i], WheelInput.Instance.getButton(WheelInput.Instance.currentDeviceID, i) ? -1 : 0);
                    scanInputMax[i] = Mathf.Max(scanInputMin[i], WheelInput.Instance.getButton(WheelInput.Instance.currentDeviceID, i) ? -1 : 0);
                }
                else
                {
                    scanInputMin.Add(i, WheelInput.Instance.getButton(WheelInput.Instance.currentDeviceID, i) ? -1 : 0);
                    scanInputMax.Add(i, WheelInput.Instance.getButton(WheelInput.Instance.currentDeviceID, i) ? -1 : 0);
                }
            }
            scanCount++;
        }
        return scanFound;

    }

    private float Abs(float x)
    {
        if (x > 0) return x;
        else return -x;
    }
}
