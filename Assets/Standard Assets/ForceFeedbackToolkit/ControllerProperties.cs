using System;
using System.Runtime.InteropServices;

namespace DInputProxy
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ControllerPropertiesData
    {
        public int forceEnable; // byte
        public int overallGain;
        public int springGain;
        public int damperGain;
        public int defaultSpringEnabled; // byte
        public int defaultSpringGain;
        public int combinePedals; // byte
        public int wheelRange;
        public int gameSettingsEnabled; // byte
        public int allowGameSettings; // byte
    };

}
