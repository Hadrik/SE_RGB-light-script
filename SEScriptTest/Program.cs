using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
  partial class Program : MyGridProgram
  {



    //===========Change===========//

    const string LightName  = "RGB Light";

    const bool UseStatLCD = true;
    const string StatLCDName = "Log LCD";

    // from 0 to 100
    const int _speed       = 100;
    const int _saturation  = 100;
    const int _value       = 100;

    //============================//
    


    IMyInteriorLight Light;
    IMyTextPanel _logOutput;
    float SpeedMultiplier;
    string LastArgs = "";
    public struct A
    {
      public float Speed;
      public float Saturation;
      public float Value;
    }
    A a = new A();
    

    public void Save()
    {
      Storage = a.Speed + ";" + a.Saturation + ";" + a.Value;
    }

    public Program()
    {

      Light = GridTerminalSystem.GetBlockWithName(LightName) as IMyInteriorLight;
      if (UseStatLCD)
      {
        _logOutput = GridTerminalSystem.GetBlockWithName(StatLCDName) as IMyTextPanel;
      }
      a.Speed = _speed / 100f;
      a.Saturation = _saturation / 100f;
      a.Value = _value / 100f;
      if (Storage.Length > 0)
      {
        string[] Stored = Storage.Split(';');
        a.Speed = float.Parse(Stored[0]);
        a.Saturation = float.Parse(Stored[1]);
        a.Value = float.Parse(Stored[2]);
      }
      SetUpdateFrequency();
    }

    public void Main(string arguments, UpdateType updateType)
    {
      if (updateType != UpdateType.Update1 && updateType != UpdateType.Update10)
      {
        ParseArguments(arguments);
      }

      a.Speed = Clamp(a.Speed, 0.0f, 1.0f);
      a.Saturation = Clamp(a.Saturation, 0.0f, 1.0f);
      a.Value = Clamp(a.Value, 0.0f, 1.0f);
      SetUpdateFrequency();

      Vector3 colorHSV = Light.Color.ColorToHSV();
      
      colorHSV.X += (0.005f * a.Speed * SpeedMultiplier) % 1;
      colorHSV.Y = a.Saturation;
      colorHSV.Z = a.Value;

      if(arguments.Length != 0)
      {
        LastArgs = arguments;
      }
      Log($"Hue:    {HueDecToDeg(colorHSV.X)} ({colorHSV.X})" +
        $"\nSaturation: {colorHSV.Y}" +
        $"\nValue:      {colorHSV.Z}" +
        $"\nSpeed:      {a.Speed}" +
        $"\n\nLast argument:\n  {LastArgs}");


      Light.Color = colorHSV.HSVtoColor();
    }

    private float CutAndParse(string s)
    {
      return float.Parse(s.Substring(0, s.IndexOf(';')));
    }

    private static float Clamp(float value, float min, float max)
    {
      return (value < min) ? min : (value > max) ? max : value;
    }

    private void ParseArguments(string arguments)
    {
      if (arguments.Length != 0)
      {
        int SpeedIdx = arguments.IndexOf("Speed");
        int SaturationIdx = arguments.IndexOf("Saturation");
        int ValueIdx = arguments.IndexOf("Value");
        Log(SpeedIdx.ToString());
        Log(SaturationIdx.ToString());
        Log(ValueIdx.ToString());

        if (SpeedIdx != -1)
        {
          Log("Running speed");
          string Ss = arguments.Substring(SpeedIdx + 5);
          float v = CutAndParse(Ss) / 100;
          if (Ss[0] == '+' || Ss[0] == '-')
          {
            a.Speed += v;
          }
          else
          {
            a.Speed = v;
          }
        }

        if (SaturationIdx != -1)
        {
          Log("Running sat");
          string Ss = arguments.Substring(SaturationIdx + 10);
          float v = CutAndParse(Ss) / 100;
          if (Ss[0] == '+' || Ss[0] == '-')
          {
            a.Saturation += v;
          }
          else
          {
            a.Saturation = v;
          }
        }

        if (ValueIdx != -1)
        {
          Log("Running val");
          string Ss = arguments.Substring(ValueIdx + 5);
          float v = CutAndParse(Ss) / 100;
          if (Ss[0] == '+' || Ss[0] == '-')
          {
            a.Value += v;
          }
          else
          {
            a.Value = v;
          }
        }
      }
    }

    private void SetUpdateFrequency()
    {
      if (a.Speed < 0.1f)
      {
        Runtime.UpdateFrequency = UpdateFrequency.Update10;
        SpeedMultiplier = 10.0f;
      }
      else
      {
        Runtime.UpdateFrequency = UpdateFrequency.Update1;
        SpeedMultiplier = 1.0f;
      }
    }

    public void Log(string t)
    {
      Echo(t);
      if (UseStatLCD)
      {
      _logOutput?.WriteText($"{t}\n");
      }
    }

    private int HueDecToDeg(float dec)
    {
      return (int)Math.Floor((dec - 0f) * (360 - 0) / (1f - 0f) + 0);
    }
  }//^STOP
}
