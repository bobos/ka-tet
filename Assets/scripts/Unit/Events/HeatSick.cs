using System.Collections.Generic;
using PathFind;
using MapTileNS;
using MonoNS;
using UnityEngine;
using CourtNS;
using FieldNS;

namespace UnitNS
{
  public class HeatSick
  {
    float __illDisableRate = 0f;
    float disableRatio
    {
      get
      {
        return __illDisableRate;
      }
      set
      {
        __illDisableRate = value < 0 ? 0 : value;
      }
    }
    float __illDeathRate = 0f;
    float killRatio
    {
      get
      {
        return __illDeathRate;
      }
      set
      {
        __illDeathRate = value < 0 ? 0 : value;
      }
    }
    Unit unit;
    public HeatSick(Unit unit) {
      this.unit = unit;
    }

    public void Worsen() {
      // TODO: emit event
      disableRatio += GetDisableRatio();
      killRatio += GetKillRatio();
    }

    public bool IsValid() {
      return ;
    }

    public void Apply() {
       if (illDisableRate > 0)
      {
        int woundedNum = GetIllDisableNum();
        rf.wounded += woundedNum;
        rf.soldiers -= woundedNum;
        labor -= (int)(woundedNum / 4);
        illDisableRate -= 0.005f;
      }

      if (illDeathRate > 0)
      {
        rf.morale -= 2;
        int kiaNum = GetIllKillNum();
        kia += kiaNum;
        rf.soldiers -= kiaNum;
        labor -= kiaNum;
        illDeathRate -= 0.0025f;
      }
    }

    public int GetIllTurns()
    {
      return (int)(disableRatio * 100);
    }

    public int GetIllDisableNum()
    {
      return (int)(unit.rf.soldiers * disableRatio);
    }

    public int GetIllKillNum()
    {
      return (int)(unit.rf.soldiers * killRatio);
    }

    float GetDisableRatio() {
      return Util.Rand(2, 5) * 0.01f;
    }

    float GetKillRatio() {
      return Util.Rand(1, 2) * 0.01f;
    }

  }
}