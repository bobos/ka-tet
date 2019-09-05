namespace UnitNS
{
  public static class UnitDehydrate
  {
    public static void Dehydrate(Unit unit) {
      // TODO: emit event
      int dehydrateNum = (int)(unit.rf.soldiers * Util.Rand(0.0025f, 0.004f));
      unit.rf.soldiers -= dehydrateNum;
      unit.rf.wounded += dehydrateNum;
      unit.labor -= (int)(dehydrateNum / 4);
      unit.rf.morale -= 6;
      unit.movementRemaining = (int)(unit.movementRemaining * Util.Rand(0.4f, 0.7f)); 
    }
  }

}