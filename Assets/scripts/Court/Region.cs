using UnitNS;
using TextNS;
using System.Collections.Generic;

namespace CourtNS
{
  public abstract class Region
  {
    protected TextLib textLib = Cons.GetTextLib();
    public abstract string Name();
    public abstract string Description();
    public List<Region> UncomfortableRegions = new List<Region>();
    public abstract int CombatPoint(Type unitType);
    public int Mov(Type unitType)
    {
      return 100;
    }
    public abstract int Will();
    public abstract int RetreatThreshold();
  }

  public class QidanRegion : Region
  {

    public override string Name()
    {
      return textLib.get("region_qidanRegion");
    }

    public override string Description()
    {
      return textLib.get("region_qidanRegion_description");
    }

    public override int CombatPoint(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 10;
      return 35;
    }

    public override int Will()
    {
      return 80;
    }

    public override int RetreatThreshold()
    {
      return 35;
    }

  }

  public class HanRegion : Region
  {

    public override string Name()
    {
      return textLib.get("region_hanRegion");
    }

    public override string Description()
    {
      return textLib.get("region_hanRegion_description");
    }

    public override int CombatPoint(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 10;
      return 30;
    }

    public override int Will()
    {
      return 80;
    }

    public override int RetreatThreshold()
    {
      return 35;
    }

  }

  public class DangxiangRegion : Region
  {

    public override string Name()
    {
      return textLib.get("region_dangxiangRegion");
    }

    public override string Description()
    {
      return textLib.get("region_dangxiangRegion_description");
    }

    public override int CombatPoint(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 10;
      return 30;
    }

    public override int Will()
    {
      return 80;
    }

    public override int RetreatThreshold()
    {
      return 45;
    }

  }

  public class TuboRegion : Region
  {

    public override string Name()
    {
      return textLib.get("region_tuboRegion");
    }

    public override string Description()
    {
      return textLib.get("region_tuboRegion_description");
    }

    public override int CombatPoint(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 10;
      return 30;
    }

    public override int Will()
    {
      return 80;
    }

    public override int RetreatThreshold()
    {
      return 45;
    }

  }

}