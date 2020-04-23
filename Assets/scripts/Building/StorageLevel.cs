using TextNS;

public class StorageLevel {
  TextLib textLib = Cons.GetTextLib();
  int level = 1;
  public StorageLevel(int level) {
    // 1,2,3,4
    this.level = level > 4 ? 4 : (level < 1 ? 1 : level);
  }

  public int LastingTurnsUnderSiege() {
    if (level == 1) {
      return 4;
    }
    
    if (level == 2) {
      return 8;
    }

    if (level == 3) {
      // small city and strategy base
      return 12;
    }

    // large city
    return 30;
  }

  public string GetLevelTxt() {
    return textLib.get("settlement_storageLvl"+level);
  }

}
