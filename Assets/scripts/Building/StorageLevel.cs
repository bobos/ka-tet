using System.Collections.Generic;
using UnityEngine;
using UnitNS;
using MapTileNS;
using MonoNS;
using FieldNS;
using TextNS;

public class StorageLevel {
  TextLib textLib = Cons.GetTextLib();
  int level = 1;
  public StorageLevel(int level) {
    // 1,2,3
    this.level = level > 3 ? 3 : (level < 1 ? 1 : level);
  }

  public int MaxStorage() {
    int turns = 6;
    if (level == 1) {
      // support 10000*3 men for 6 turns for defenders, or 3 turns intruders
      return (int)((Infantry.MaxTroopNum * 3 * turns) / 10);
    }
    
    if (level == 2) {
      // support 10000*6 men for 6 turns for defenders, or 3 turns intruders
      return (int)((Infantry.MaxTroopNum * 6 * turns) / 10);
    }

    // for city and strategy base
    // 10000*3 men for 60 turns(half-year) for defenders
    return (int)((Infantry.MaxTroopNum * 3 * turns * 10) / 10);
  }

  public string GetLevelTxt() {
    if (level == 1) {
      return textLib.get("settlement_storageLvl1");
    }

    if (level == 2) {
      return textLib.get("settlement_storageLvl2");
    }

    return textLib.get("settlement_storageLvl3");
  }

}
