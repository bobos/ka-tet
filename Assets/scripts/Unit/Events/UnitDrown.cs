using System.Collections.Generic;
using PathFind;
using MapTileNS;
using MonoNS;
using UnityEngine;
using CourtNS;
using FieldNS;

namespace UnitNS
{
  public static class UnitDrown
  {
    public static void Drown(Unit unit) {
      // TODO: emit event
      int drownNum = Util.Rand(16, 40);
      unit.rf.soldiers -= drownNum;
      unit.labor -= drownNum;
      unit.kia += drownNum;
      unit.rf.morale -= 4;
    }
  }

}