using System.Collections.Generic;

namespace UnitNS
{
  public class Level {
    int exp = 0;
    public Dictionary<int, int> lvlMap = new Dictionary<int, int> {
      {1, 10000},
      {2, 50000},
      {3, -1}};

    public bool GainExp(int currentLvl, int points) {
      int nextExp = lvlMap[currentLvl];
      if (nextExp < 0) { return false; }
      exp += points;
      if (exp >= nextExp) {
        exp = 0;
        return true;
      }
      return false;
    }
  }
}