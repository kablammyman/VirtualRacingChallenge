using System;
using System.Runtime.InteropServices;

namespace DInputProxy
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct RawJoystickState
    {
        public int AccelerationSliders0; 
        public int AccelerationSliders1; 
        public int AccelerationX;    
        public int AccelerationY;   
        public int AccelerationZ;  
        public int AngularAccelerationX; 
        public int AngularAccelerationY; 
        public int AngularAccelerationZ; 
        public int AngularVelocityX;  
        public int AngularVelocityY;  
        public int AngularVelocityZ; 

        public byte Buttons0;
        public byte Buttons1;
        public byte Buttons2;
        public byte Buttons3;
        public byte Buttons4;
        public byte Buttons5;
        public byte Buttons6;
        public byte Buttons7;
        public byte Buttons8;
        public byte Buttons9;

        public byte Buttons10;
        public byte Buttons11;
        public byte Buttons12;
        public byte Buttons13;
        public byte Buttons14;
        public byte Buttons15;
        public byte Buttons16;
        public byte Buttons17;
        public byte Buttons18;
        public byte Buttons19;

        public byte Buttons20;
        public byte Buttons21;
        public byte Buttons22;
        public byte Buttons23;
        public byte Buttons24;
        public byte Buttons25;
        public byte Buttons26;
        public byte Buttons27;
        public byte Buttons28;
        public byte Buttons29;

        public byte Buttons30;
        public byte Buttons31;
        public byte Buttons32;
        public byte Buttons33;
        public byte Buttons34;
        public byte Buttons35;
        public byte Buttons36;
        public byte Buttons37;
        public byte Buttons38;
        public byte Buttons39;

        public byte Buttons40;
        public byte Buttons41;
        public byte Buttons42;
        public byte Buttons43;
        public byte Buttons44;
        public byte Buttons45;
        public byte Buttons46;
        public byte Buttons47;
        public byte Buttons48;
        public byte Buttons49;

        public byte Buttons50;
        public byte Buttons51;
        public byte Buttons52;
        public byte Buttons53;
        public byte Buttons54;
        public byte Buttons55;
        public byte Buttons56;
        public byte Buttons57;
        public byte Buttons58;
        public byte Buttons59;

        public byte Buttons60;
        public byte Buttons61;
        public byte Buttons62;
        public byte Buttons63;
        public byte Buttons64;
        public byte Buttons65;
        public byte Buttons66;
        public byte Buttons67;
        public byte Buttons68;
        public byte Buttons69;

        public byte Buttons70;
        public byte Buttons71;
        public byte Buttons72;
        public byte Buttons73;
        public byte Buttons74;
        public byte Buttons75;
        public byte Buttons76;
        public byte Buttons77;
        public byte Buttons78;
        public byte Buttons79;

        public byte Buttons80;
        public byte Buttons81;
        public byte Buttons82;
        public byte Buttons83;
        public byte Buttons84;
        public byte Buttons85;
        public byte Buttons86;
        public byte Buttons87;
        public byte Buttons88;
        public byte Buttons89;

        public byte Buttons90;
        public byte Buttons91;
        public byte Buttons92;
        public byte Buttons93;
        public byte Buttons94;
        public byte Buttons95;
        public byte Buttons96;
        public byte Buttons97;
        public byte Buttons98;
        public byte Buttons99;

        public byte Buttons100;
        public byte Buttons101;
        public byte Buttons102;
        public byte Buttons103;
        public byte Buttons104;
        public byte Buttons105;
        public byte Buttons106;
        public byte Buttons107;
        public byte Buttons108;
        public byte Buttons109;

        public byte Buttons110;
        public byte Buttons111;
        public byte Buttons112;
        public byte Buttons113;
        public byte Buttons114;
        public byte Buttons115;
        public byte Buttons116;
        public byte Buttons117;
        public byte Buttons118;
        public byte Buttons119;

        public byte Buttons120;
        public byte Buttons121;
        public byte Buttons122;
        public byte Buttons123;
        public byte Buttons124;
        public byte Buttons125;
        public byte Buttons126;
        public byte Buttons127;

        public int ForceSliders0; 
        public int ForceSliders1;  
        public int ForceX; 
        public int ForceY; 
        public int ForceZ; 
        public int PointOfViewControllers0; //
        public int PointOfViewControllers1; //
        public int PointOfViewControllers2; //
        public int PointOfViewControllers3; //
        public int RotationX; //
        public int RotationY; //
        public int RotationZ; //
        public int Sliders0; //
        public int Sliders1; //
        public int TorqueX; 
        public int TorqueY;
        public int TorqueZ; 
        public int VelocitySliders0; 
        public int VelocitySliders1; 
        public int VelocityX;
        public int VelocityY;
        public int VelocityZ;
        public int X; //
        public int Y; //
        public int Z; //
    }

    public struct JoystickUpdate : IStateUpdate
    {

        public JoystickOffset Offset { get; set; }
        public int RawOffset { get; set; }
        public int Sequence { get; set; }
        public int Timestamp { get; set; }
        public int Value { get; set; }

        public override string ToString()
        {
            return "";
        }
    }

    public interface IStateUpdate
    {
        int RawOffset { get; set; }
        int Sequence { get; set; }
        int Timestamp { get; set; }
        int Value { get; set; }
    }

    public enum JoystickOffset
    {
        X = 0,
        Y = 4,
        Z = 8,
        RotationX = 12,
        RotationY = 16,
        RotationZ = 20,
        Sliders0 = 24,
        Sliders1 = 28,
        PointOfViewControllers0 = 32,
        PointOfViewControllers1 = 36,
        PointOfViewControllers2 = 40,
        PointOfViewControllers3 = 44,
        Buttons0 = 48,
        Buttons1 = 49,
        Buttons2 = 50,
        Buttons3 = 51,
        Buttons4 = 52,
        Buttons5 = 53,
        Buttons6 = 54,
        Buttons7 = 55,
        Buttons8 = 56,
        Buttons9 = 57,
        Buttons10 = 58,
        Buttons11 = 59,
        Buttons12 = 60,
        Buttons13 = 61,
        Buttons14 = 62,
        Buttons15 = 63,
        Buttons16 = 64,
        Buttons17 = 65,
        Buttons18 = 66,
        Buttons19 = 67,
        Buttons20 = 68,
        Buttons21 = 69,
        Buttons22 = 70,
        Buttons23 = 71,
        Buttons24 = 72,
        Buttons25 = 73,
        Buttons26 = 74,
        Buttons27 = 75,
        Buttons28 = 76,
        Buttons29 = 77,
        Buttons30 = 78,
        Buttons31 = 79,
        Buttons32 = 80,
        Buttons33 = 81,
        Buttons34 = 82,
        Buttons35 = 83,
        Buttons36 = 84,
        Buttons37 = 85,
        Buttons38 = 86,
        Buttons39 = 87,
        Buttons40 = 88,
        Buttons41 = 89,
        Buttons42 = 90,
        Buttons43 = 91,
        Buttons44 = 92,
        Buttons45 = 93,
        Buttons46 = 94,
        Buttons47 = 95,
        Buttons48 = 96,
        Buttons49 = 97,
        Buttons50 = 98,
        Buttons51 = 99,
        Buttons52 = 100,
        Buttons53 = 101,
        Buttons54 = 102,
        Buttons55 = 103,
        Buttons56 = 104,
        Buttons57 = 105,
        Buttons58 = 106,
        Buttons59 = 107,
        Buttons60 = 108,
        Buttons61 = 109,
        Buttons62 = 110,
        Buttons63 = 111,
        Buttons64 = 112,
        Buttons65 = 113,
        Buttons66 = 114,
        Buttons67 = 115,
        Buttons68 = 116,
        Buttons69 = 117,
        Buttons70 = 118,
        Buttons71 = 119,
        Buttons72 = 120,
        Buttons73 = 121,
        Buttons74 = 122,
        Buttons75 = 123,
        Buttons76 = 124,
        Buttons77 = 125,
        Buttons78 = 126,
        Buttons79 = 127,
        Buttons80 = 128,
        Buttons81 = 129,
        Buttons82 = 130,
        Buttons83 = 131,
        Buttons84 = 132,
        Buttons85 = 133,
        Buttons86 = 134,
        Buttons87 = 135,
        Buttons88 = 136,
        Buttons89 = 137,
        Buttons90 = 138,
        Buttons91 = 139,
        Buttons92 = 140,
        Buttons93 = 141,
        Buttons94 = 142,
        Buttons95 = 143,
        Buttons96 = 144,
        Buttons97 = 145,
        Buttons98 = 146,
        Buttons99 = 147,
        Buttons100 = 148,
        Buttons101 = 149,
        Buttons102 = 150,
        Buttons103 = 151,
        Buttons104 = 152,
        Buttons105 = 153,
        Buttons106 = 154,
        Buttons107 = 155,
        Buttons108 = 156,
        Buttons109 = 157,
        Buttons110 = 158,
        Buttons111 = 159,
        Buttons112 = 160,
        Buttons113 = 161,
        Buttons114 = 162,
        Buttons115 = 163,
        Buttons116 = 164,
        Buttons117 = 165,
        Buttons118 = 166,
        Buttons119 = 167,
        Buttons120 = 168,
        Buttons121 = 169,
        Buttons122 = 170,
        Buttons123 = 171,
        Buttons124 = 172,
        Buttons125 = 173,
        Buttons126 = 174,
        Buttons127 = 175,
        VelocityX = 176,
        VelocityY = 180,
        VelocityZ = 184,
        AngularVelocityX = 188,
        AngularVelocityY = 192,
        AngularVelocityZ = 196,
        VelocitySliders0 = 200,
        VelocitySliders1 = 204,
        AccelerationX = 208,
        AccelerationY = 212,
        AccelerationZ = 216,
        AngularAccelerationX = 220,
        AngularAccelerationY = 224,
        AngularAccelerationZ = 228,
        AccelerationSliders0 = 232,
        AccelerationSliders1 = 236,
        ForceX = 240,
        ForceY = 244,
        ForceZ = 248,
        TorqueX = 252,
        TorqueY = 256,
        TorqueZ = 260,
        ForceSliders0 = 264,
        ForceSliders1 = 268,
    }
}
