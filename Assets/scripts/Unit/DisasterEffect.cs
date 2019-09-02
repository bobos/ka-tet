using System.Collections.Generic;
using PathFind;
using MapTileNS;
using MonoNS;
using UnityEngine;
using CourtNS;
using FieldNS;

namespace UnitNS
{
  public static class DisasterEffect
  {
    public static void Apply(DisasterType type, Unit unit) {
      if (type == DisasterType.WildFire) {
        unit.TakeEffect(8, 1f, 0.025f, 0.0125f);
      }

      if (type == DisasterType.WildFire) {
        unit.TakeEffect(8, 1f, 0.05f, 0.025f);
      }

      if (type == DisasterType.LandSlide) {
        unit.TakeEffect(30, 1f, 0f, 0.3f);
      }
    }
  }
}