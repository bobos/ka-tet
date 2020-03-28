using TextNS;

public class WallDefense {
  public const int DefenseDrop = 10;
  TextLib textLib = Cons.GetTextLib();
  int level = 1;
  public int defensePoint = 0;
  public WallDefense(int level) {
    // 1,2,3
    this.level = level > 3 ? 3 : (level < 1 ? 1 : level);
    defensePoint = MaxDefensePoint();
  }

  public int MaxDefensePoint() {
    if (level == 1) {
      return 50;
    }
    
    if (level == 2) {
      return 100;
    }

    return 150;
  }

  public void DepleteDefense() {
    defensePoint -= DefenseDrop;
    defensePoint = defensePoint < 0 ? 0 : defensePoint;
  }

  public void RepairDefense() {
    defensePoint += (int)(DefenseDrop / 2);
    defensePoint = defensePoint > MaxDefensePoint() ? MaxDefensePoint() : defensePoint;
  }

  public string GetLevelTxt() {
    if (level == 1) {
      return textLib.get("settlement_wallLvl1");
    }

    if (level == 2) {
      return textLib.get("settlement_wallLvl2");
    }

    return textLib.get("settlement_wallLvl3");
  }

}
