using System.Collections.Generic;
using PathFind;
using MapTileNS;
using MonoNS;
using UnityEngine;
using CourtNS;
using FieldNS;

namespace UnitNS
{
  public class ArmorRemEvent
  {
    Unit unit;
    public ArmorRemEvent(Unit unit) {
      this.unit = unit;
      this.unit.ListenOnHeat(OnHeat);
    }

    public void OnHeat() {
      // TODO: interactive event
      unit.RemoveOnHeatListener(OnHeat);
    }

  }
}