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
// DInputProxy
//
// Proxies information between the FFWheelInput external DLL and Unity 
//
//========================================================================================================================

using System;
using System.Runtime.InteropServices;

namespace DInputProxy
{

    class Imports
    {

        [DllImport("FFWheelInput")]
        public static extern IntPtr UWheelCreate();

        [DllImport("FFWheelInput")]
        public static extern bool InitWheel(IntPtr pUWheel, IntPtr hDlg);

        [DllImport("FFWheelInput")]
        public static extern bool HasForceFeedback(IntPtr pUWheel, int index);

        [DllImport("FFWheelInput")]
        public static extern bool SetRumble(IntPtr pUWheel, int index, int leftMotor, int rightMotor);

        [DllImport("FFWheelInput")]
        public static extern bool UnloadWheel(IntPtr pUWheel);

        [DllImport("FFWheelInput", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 GetJoystickState(IntPtr pUWheel, int index, IntPtr rawJoystick);

        [DllImport("FFWheelInput")]
        public static extern IntPtr GetFriendlyProductName(IntPtr pUWheel, int index);

        [DllImport("FFWheelInput")]
        public static extern uint GetPID(IntPtr pUWheel, int index);

        [DllImport("FFWheelInput")]
        public static extern bool IsConnected(IntPtr pUWheel, int index);

        [DllImport("FFWheelInput")]
        public static extern bool IsGamepad(IntPtr pUWheel, int index);

        [DllImport("FFWheelInput")]
        public static extern uint PlayConstant2DForce(IntPtr pUWheel, int index, int x, int y);

        [DllImport("FFWheelInput")]
        public static extern uint StopConstant2DForce(IntPtr pUWheel, int index);

        [DllImport("FFWheelInput")]
        public static extern uint PlayConstantForce(IntPtr pUWheel, int index, int x);

        [DllImport("FFWheelInput")]
        public static extern uint StopConstantForce(IntPtr pUWheel, int index);

        [DllImport("FFWheelInput")]
        public static extern uint PlayDamperForce(IntPtr pUWheel, int index, int x);

        [DllImport("FFWheelInput")]
        public static extern uint StopDamperForce(IntPtr pUWheel, int index);

        [DllImport("FFWheelInput")]
        public static extern uint PlayBumpyRoadEffect(IntPtr pUWheel, int index, int x);

        [DllImport("FFWheelInput")]
        public static extern uint StopBumpyRoadEffect(IntPtr pUWheel, int index);

        [DllImport("FFWheelInput")]
        public static extern uint PlayDirtRoadEffect(IntPtr pUWheel, int index, int x);

        [DllImport("FFWheelInput")]
        public static extern uint StopDirtRoadEffect(IntPtr pUWheel, int index);

        [DllImport("FFWheelInput")]
        public static extern uint PlaySlipperyRoadEffect(IntPtr pUWheel, int index, int x);

        [DllImport("FFWheelInput")]
        public static extern uint StopSlipperyRoadEffect(IntPtr pUWheel, int index);

        [DllImport("FFWheelInput")]
        public static extern uint PlaySoftstopForce(IntPtr pUWheel, int index, int x);

        [DllImport("FFWheelInput")]
        public static extern uint StopSoftstopForce(IntPtr pUWheel, int index);

        [DllImport("FFWheelInput")]
        public static extern uint PlaySurfaceEffect(IntPtr pUWheel, int index, int periodicNr, int magnitudePercentage, int period);

        [DllImport("FFWheelInput")]
        public static extern uint StopSurfaceEffect(IntPtr pUWheel, int index);

        [DllImport("FFWheelInput")]
        public static extern uint PlayCarAirborne(IntPtr pUWheel, int index);

        [DllImport("FFWheelInput")]
        public static extern uint StopCarAirborne(IntPtr pUWheel, int index);

        [DllImport("FFWheelInput")]
        public static extern uint PlaySideCollisionForce(IntPtr pUWheel, int index, int x);

        [DllImport("FFWheelInput")]
        public static extern uint PlayFrontalCollisionForce(IntPtr pUWheel, int index, int x);

        [DllImport("FFWheelInput")]
        public static extern uint SetLeds(IntPtr pUWheel, int index, float currentRPM, float rpmFirstLedTurnsOn, float rpmRedLine);

        [DllImport("FFWheelInput", CallingConvention = CallingConvention.StdCall)]
        public static extern bool GetCurrentControllerProperties(IntPtr pUWheel, int index, IntPtr rawControllerProperties);

        [DllImport("FFWheelInput")]
        public static extern uint SetPreferredControllerProperties(IntPtr pUWheel, ControllerPropertiesData properties);

        [DllImport("FFWheelInput")]
        public static extern bool IsPlaying(IntPtr pUWheel, int index, Int32 forceNr);

        [DllImport("FFWheelInput")]
        public static extern uint PlaySpringForce(IntPtr pUWheel, int index, Int32 offsetPercentage, Int32 saturationPercentage, Int32 coefficientPercentage);

        [DllImport("FFWheelInput")]
        public static extern uint StopSpringForce(IntPtr pUWheel, int index);


        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

    }


    public class DWheel
    {

        public enum ForceType
        {
            FORCE_NONE,
            FORCE_SPRING,
            FORCE_CONSTANT,
            FORCE_DAMPER,
            FORCE_SIDE_COLLISION,
            FORCE_FRONTAL_COLLISION,
            FORCE_DIRT_ROAD,
            FORCE_BUMPY_ROAD,
            FORCE_SLIPPERY_ROAD,
            FORCE_SURFACE_EFFECT,
            NUMBER_FORCE_EFFECTS,
            FORCE_SOFTSTOP,
            FORCE_CAR_AIRBORNE,
            FORCE_2DCONSTANT
        };

        public enum PeriodicType
        {
            LG_TYPE_NONE, LG_TYPE_SINE, LG_TYPE_SQUARE, LG_TYPE_TRIANGLE

        };

        private static IntPtr UWheel = IntPtr.Zero;
        private static IntPtr hWnd;
        public static Boolean DirectInputInitialised = false;
        private static int currentIndex = 0;

        public static Boolean InitDirectInput()
        {
            bool result = false;

            try
            {
                // Only initialise first time through.
                if (UWheel == IntPtr.Zero)
                {
                    hWnd = Imports.GetActiveWindow();
                    UWheel = Imports.UWheelCreate();
                    result = Imports.InitWheel(UWheel, hWnd);
                }
                DirectInputInitialised = (result == true);
            }
            catch { }
            return DirectInputInitialised;

        }

        public static int CurrentIndex
        {
            get { return currentIndex; }
            set { currentIndex = value; }
        }

        public static bool SetDeviceID(int index)
        {
            if (IsConnected(index))
            {
                currentIndex = index;
                return true;
            }
            else return false;
        }

        ~DWheel()
        {
            FreeDirectInput();
        }

        public static Boolean AcquireDevice()
        {
            //Imports.AcquireDevice();
            return true;
        }

        public static void FreeDirectInput()
        {
            try
            {
                Imports.UnloadWheel(UWheel);
                UWheel = IntPtr.Zero;
            }
            catch { }
        }

        public static PeriodicType GetPeriodicType(Int32 periodicNr)
        {
            PeriodicType returnVal = PeriodicType.LG_TYPE_NONE;

            switch (periodicNr)
            {
                case 1:
                    returnVal = PeriodicType.LG_TYPE_NONE;
                    break;
                case 2:
                    returnVal = PeriodicType.LG_TYPE_SINE;
                    break;
                case 3:
                    returnVal = PeriodicType.LG_TYPE_SQUARE;
                    break;
                case 4:
                    returnVal = PeriodicType.LG_TYPE_TRIANGLE;
                    break;

            }
            return returnVal;


        }

        public static Int32 GetPeriodicNr(PeriodicType periodicType)
        {
            Int32 returnVal = 1;

            switch (periodicType)
            {
                case PeriodicType.LG_TYPE_NONE:
                    returnVal = 1;
                    break;
                case PeriodicType.LG_TYPE_SINE:
                    returnVal = 2;
                    break;
                case PeriodicType.LG_TYPE_SQUARE:
                    returnVal = 3;
                    break;
                case PeriodicType.LG_TYPE_TRIANGLE:
                    returnVal = 4;
                    break;
            }
            return returnVal;
        }

        public static ForceType GetForceType(Int32 forceNr)
        {
            ForceType returnVal = ForceType.FORCE_NONE;

            switch (forceNr)
            {
                case 1:
                    returnVal = ForceType.FORCE_NONE;
                    break;
                case 2:
                    returnVal = ForceType.FORCE_SPRING;
                    break;
                case 3:
                    returnVal = ForceType.FORCE_CONSTANT;
                    break;
                case 4:
                    returnVal = ForceType.FORCE_DAMPER;
                    break;
                case 5:
                    returnVal = ForceType.FORCE_SIDE_COLLISION;
                    break;
                case 6:
                    returnVal = ForceType.FORCE_FRONTAL_COLLISION;
                    break;
                case 7:
                    returnVal = ForceType.FORCE_DIRT_ROAD;
                    break;
                case 8:
                    returnVal = ForceType.FORCE_BUMPY_ROAD;
                    break;
                case 9:
                    returnVal = ForceType.FORCE_SLIPPERY_ROAD;
                    break;
                case 10:
                    returnVal = ForceType.FORCE_SURFACE_EFFECT;
                    break;
                case 11:
                    returnVal = ForceType.NUMBER_FORCE_EFFECTS;
                    break;
                case 12:
                    returnVal = ForceType.FORCE_SOFTSTOP;
                    break;
                case 13:
                    returnVal = ForceType.FORCE_CAR_AIRBORNE;
                    break;
                case 14:
                    returnVal = ForceType.FORCE_2DCONSTANT;
                    break;
            }
            return returnVal;


        }

        public static Int32 GetForceNr(ForceType forceType)
        {
            Int32 returnVal = 1;

            switch (forceType)
            {
                case ForceType.FORCE_NONE:
                    returnVal = 1;
                    break;
                case ForceType.FORCE_SPRING:
                    returnVal = 2;
                    break;
                case ForceType.FORCE_CONSTANT:
                    returnVal = 3;
                    break;
                case ForceType.FORCE_DAMPER:
                    returnVal = 4;
                    break;
                case ForceType.FORCE_SIDE_COLLISION:
                    returnVal = 5;
                    break;
                case ForceType.FORCE_FRONTAL_COLLISION:
                    returnVal = 6;
                    break;
                case ForceType.FORCE_DIRT_ROAD:
                    returnVal = 7;
                    break;
                case ForceType.FORCE_BUMPY_ROAD:
                    returnVal = 8;
                    break;
                case ForceType.FORCE_SLIPPERY_ROAD:
                    returnVal = 9;
                    break;
                case ForceType.FORCE_SURFACE_EFFECT:
                    returnVal = 10;
                    break;
                case ForceType.NUMBER_FORCE_EFFECTS:
                    returnVal = 11;
                    break;
                case ForceType.FORCE_SOFTSTOP:
                    returnVal = 12;
                    break;
                case ForceType.FORCE_CAR_AIRBORNE:
                    returnVal = 13;
                    break;
            }
            return returnVal;


        }
        public static bool IsConnected()
        {
            return IsConnected(currentIndex);
        }

        public static bool IsConnected(int index)
        {
            try
            {
                return Imports.IsConnected(UWheel, index);
            }
            catch { return false; }
        }

        public static bool IsGamepad()
        {
            return IsGamepad(currentIndex);
        }

        public static bool IsGamepad(int index)
        {
            try
            {
                return Imports.IsGamepad(UWheel, index);
            }
            catch { return false; }
        }

        public static bool HasForceFeedback()
        {
            return HasForceFeedback(currentIndex);
        }
        public static bool HasForceFeedback(int index)
        {
            try
            {
                return Imports.HasForceFeedback(UWheel, index);
            }
            catch { return false; }

        }

        public static bool SetRumble(int leftMotor, int rightMotor)
        {
            return SetRumble(currentIndex, leftMotor, rightMotor);
        }
        public static bool SetRumble(int index, int leftMotor, int rightMotor)
        {
            try
            {
                return Imports.SetRumble(UWheel, index, leftMotor, rightMotor);
            }
            catch { return false; }
        }

        public static bool IsPlaying(ForceType forceType)
        {
            return IsPlaying(currentIndex, forceType);
        }

        public static bool IsPlaying(int index, ForceType forceType)
        {
            return IsPlaying(index, GetForceNr(forceType));
        }

        public static bool IsPlaying(Int32 forceNr)
        {
            return IsPlaying(currentIndex, forceNr);
        }

        public static bool IsPlaying(int index, Int32 forceNr)
        {
            try
            {
                return Imports.IsPlaying(UWheel, index, forceNr);
            }
            catch { return false; }
        }

        public static int PlayConstant2DForce(int x, int y)
        {
            return PlayConstant2DForce(currentIndex, x, y);
        }

        public static int PlayConstant2DForce(int index, int x, int y)
        {
            try
            {
                return Convert.ToInt32(Imports.PlayConstant2DForce(UWheel, index, x, y));
            }
            catch { return 0; }
        }

        public static int StopConstant2DForce()
        {
            return StopConstant2DForce(currentIndex);
        }

        public static int StopConstant2DForce(int index)
        {
            try
            {
                return Convert.ToInt32(Imports.StopConstant2DForce(UWheel, index));
            }
            catch { return 0; }
        }

        public static int SetForceCoordinate(int x, int y)
        {
            return SetForceCoordinate(currentIndex, x, y);
        }
        public static int SetForceCoordinate(int index, int x, int y)
        {
            try
            {
                return Convert.ToInt32(Imports.PlayConstantForce(UWheel, index, x));
            }
            catch { return 0; }
        }

        public static int PlayConstantForce(int x)
        {
            return PlayConstantForce(currentIndex, x);
        }

        public static int PlayConstantForce(int index, int x)
        {
            try
            {
                return Convert.ToInt32(Imports.PlayConstantForce(UWheel, index, x));
            }
            catch { return 0; }
        }

        public static int StopConstantForce()
        {
            return StopConstantForce(currentIndex);
        }

        public static int StopConstantForce(int index)
        {
            try
            {
                return Convert.ToInt32(Imports.StopConstantForce(UWheel, index));
            }
            catch { return 0; }
        }

        public static int PlayDamperForce(int x)
        {
            return PlayDamperForce(currentIndex, x);
        }

        public static int PlayDamperForce(int index, int x)
        {
            try
            {
                return Convert.ToInt32(Imports.PlayDamperForce(UWheel, index, x));
            }
            catch { return 0; }
        }

        public static int StopDamperForce()
        {
            return StopDamperForce(currentIndex);
        }

        public static int StopDamperForce(int index)
        {
            try
            {
                return Convert.ToInt32(Imports.StopDamperForce(UWheel, index));
            }
            catch { return 0; }
        }

        public static int PlayBumpyRoadEffect(int x)
        {
            return PlayBumpyRoadEffect(currentIndex, x);
        }

        public static int PlayBumpyRoadEffect(int index, int x)
        {
            try
            {
                return Convert.ToInt32(Imports.PlayBumpyRoadEffect(UWheel, index, x));
            }
            catch { return 0; }
        }

        public static int StopBumpyRoadEffect()
        {
            return StopBumpyRoadEffect(currentIndex);
        }

        public static int StopBumpyRoadEffect(int index)
        {
            try
            {
                return Convert.ToInt32(Imports.StopBumpyRoadEffect(UWheel, index));
            }
            catch { return 0; }
        }

        public static int PlayDirtRoadEffect(int x)
        {
            return PlayDirtRoadEffect(currentIndex, x);
        }

        public static int PlayDirtRoadEffect(int index, int x)
        {
            try
            {
                return Convert.ToInt32(Imports.PlayDirtRoadEffect(UWheel, index, x));
            }
            catch { return 0;  }
        }

        public static int StopDirtRoadEffect()
        {
            return StopDirtRoadEffect();
        }

        public static int StopDirtRoadEffect(int index)
        {
            try
            {
                return Convert.ToInt32(Imports.StopDirtRoadEffect(UWheel, index));
            }
                catch { return 0;  }
        }

        public static int PlaySlipperyRoadEffect(int x)
        {
            return PlaySlipperyRoadEffect(currentIndex, x);
        }

        public static int PlaySlipperyRoadEffect(int index, int x)
        {
            try
            {
                return Convert.ToInt32(Imports.PlaySlipperyRoadEffect(UWheel, index, x));
            }
            catch { return 0;  }
        }

        public static int StopSlipperyRoadEffect()
        {
            return StopSlipperyRoadEffect(currentIndex);
        }

        public static int StopSlipperyRoadEffect(int index)
        {
            try
            {
                return Convert.ToInt32(Imports.StopSlipperyRoadEffect(UWheel, index));
            }
            catch { return 0; }
        }

        public static int PlaySoftstopForce(int x)
        {
            return PlaySoftstopForce(currentIndex, x);
        }

        public static int PlaySoftstopForce(int index, int x)
        {
            try
            {
                return Convert.ToInt32(Imports.PlaySoftstopForce(UWheel, index, x));
            }
            catch { return 0;  }
        }

        public static int StopSoftstopForce()
        {
            return StopSoftstopForce(currentIndex);
        }

        public static int StopSoftstopForce(int index)
        {
            try
            {
                return Convert.ToInt32(Imports.StopSoftstopForce(UWheel, index));
            }
            catch { return 0;  }
        }

        public static int PlaySurfaceEffect(PeriodicType periodicType, int magnitudePercentage, int period)
        {
            return PlaySurfaceEffect(currentIndex, periodicType, magnitudePercentage, period);
        }

        public static int PlaySurfaceEffect(int index, PeriodicType periodicType, int magnitudePercentage, int period)
        {
            return PlaySurfaceEffect(index, GetPeriodicNr(periodicType), magnitudePercentage, period);
        }

        public static int PlaySurfaceEffect(int periodicNr, int magnitudePercentage, int period)
        {
            return PlaySurfaceEffect(currentIndex, periodicNr, magnitudePercentage, period);
        }

        public static int PlaySurfaceEffect(int index, int periodicNr, int magnitudePercentage, int period)
        {
            try
            {
                return Convert.ToInt32(Imports.PlaySurfaceEffect(UWheel, index, periodicNr, magnitudePercentage, period));
            }
            catch { return 0;  }
        }

        public static int StopSurfaceEffect()
        {
            return StopSurfaceEffect(currentIndex);
        }

        public static int StopSurfaceEffect(int index)
        {
            try
            {
                return Convert.ToInt32(Imports.StopSurfaceEffect(UWheel, index));
            }
            catch { return 0; }
        }

        public static int PlaySideCollisionForce(int x)
        {
            return PlaySideCollisionForce(currentIndex, x);
        }

        public static int PlaySideCollisionForce(int index, int x)
        {
            try
            {
                return Convert.ToInt32(Imports.PlaySideCollisionForce(UWheel, index, x));
            }
            catch { return 0;  }
        }

        public static int PlayFrontalCollisionForce(int x)
        {
            return PlayFrontalCollisionForce(currentIndex, x);
        }

        public static int PlayFrontalCollisionForce(int index, int x)
        {
            try
            {
                return Convert.ToInt32(Imports.PlayFrontalCollisionForce(UWheel, index, x));
            }
            catch { return 0; }
        }

        public static int PlaySpringForce(Int32 offsetPercentage, Int32 saturationPercentage, Int32 coefficientPercentage)
        {
            return PlaySpringForce(currentIndex, offsetPercentage, saturationPercentage, coefficientPercentage);
        }

        public static int PlaySpringForce(int index, Int32 offsetPercentage, Int32 saturationPercentage, Int32 coefficientPercentage)
        {
            try
            {
                return Convert.ToInt32(Imports.PlaySpringForce(UWheel, index, offsetPercentage, saturationPercentage, coefficientPercentage));
            }
            catch { return 0;  }
        }

        public static int StopSpringForce()
        {
            return StopSpringForce(currentIndex);
        }

        public static int StopSpringForce(int index)
        {
            try
            {
                return Convert.ToInt32(Imports.StopSpringForce(UWheel, index));
            }
            catch { return 0;  }
        }

        public static int PlayCarAirborne()
        {
            return PlayCarAirborne(currentIndex);
        }

        public static int PlayCarAirborne(int index)
        {
            try
            {
                return Convert.ToInt32(Imports.PlayCarAirborne(UWheel, index));
            }
            catch { return 0; }
        }

        public static int StopCarAirborne()
        {
            return StopCarAirborne(currentIndex);
        }

        public static int StopCarAirborne(int index)
        {
            try
            {
                return Convert.ToInt32(Imports.StopCarAirborne(UWheel, index));
            }
            catch { return 0; }
        }

        public static void SetLED(float rpm, float minRpm, float maxRpm)
        {
            SetLED(currentIndex, rpm, minRpm, maxRpm);
        }

        public static void SetLED(int index, float rpm, float minRpm, float maxRpm)
        {
            try
            {
                Imports.SetLeds(UWheel, index, rpm, minRpm, maxRpm);
            }
            catch { }
        }

        public static RawJoystickState GetJoystickState()
        {
            return GetJoystickState(currentIndex);
        }

        public static RawJoystickState GetJoystickState(int index)
        {
            RawJoystickState js_buffer = new RawJoystickState();

            try
            {
                int bufferSize = Marshal.SizeOf(js_buffer);
                IntPtr bufferPtr = Marshal.AllocHGlobal(bufferSize);
                Marshal.StructureToPtr(js_buffer, bufferPtr, false);

                Imports.GetJoystickState(UWheel, index, bufferPtr);
                js_buffer = (RawJoystickState)Marshal.PtrToStructure(bufferPtr, typeof(RawJoystickState));

                Marshal.FreeHGlobal(bufferPtr);
                bufferPtr = IntPtr.Zero;

            }
            catch { }
            return js_buffer;
        }

        public static ControllerPropertiesData GetCurrentControllerProperties()
        {
            return GetCurrentControllerProperties(currentIndex);
        }

        public static ControllerPropertiesData GetCurrentControllerProperties(int index)
        {
            ControllerPropertiesData cp_buffer = new ControllerPropertiesData();

            try
            {
                int bufferSize = Marshal.SizeOf(cp_buffer);
                IntPtr bufferPtr = Marshal.AllocHGlobal(bufferSize);
                Marshal.StructureToPtr(cp_buffer, bufferPtr, false);

                Imports.GetCurrentControllerProperties(UWheel, index, bufferPtr);
                cp_buffer = (ControllerPropertiesData)Marshal.PtrToStructure(bufferPtr, typeof(ControllerPropertiesData));

                Marshal.FreeHGlobal(bufferPtr);
                bufferPtr = IntPtr.Zero;
            }
            catch { }
            return cp_buffer;
        }

        public static uint SetPreferredControllerProperties(ControllerPropertiesData properties)
        {
            try
            {
                return Imports.SetPreferredControllerProperties(UWheel, properties);
            }
            catch { return 0;  }
        }

        public static string GetFriendlyProductName()
        {
            return GetFriendlyProductName(currentIndex);
        }

        public static string GetFriendlyProductName(int index)
        {
            try
            {
                IntPtr ptr = Imports.GetFriendlyProductName(UWheel, index);

                String returnVal = Marshal.PtrToStringBSTR(ptr);
                Marshal.FreeBSTR(ptr);
                return returnVal;
            }
            catch { return ""; }
        }

        public static uint GetPID()
        {
            return GetPID(currentIndex);
        }

        public static uint GetPID(int index)
        {
            try
            {
                return Imports.GetPID(UWheel, index);
            }
            catch { return 0;  }
        }

    }
}
