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
    public virtual int CombatPoint(Type unitType)
    {
      if (unitType == Type.Infantry) return 10;
      return 30;
    }
    public virtual int DefaultOrganizationPoint() {
      return 35;
    }
    public virtual int MaxOrganizationPoint() {
      return 65;
    }
    public abstract List<Region> GetConflictRegions();
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

    public override List<Region> GetConflictRegions() {
      return new List<Region>(){Cons.nvzhen, Cons.han};
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

    public override List<Region> GetConflictRegions() {
      return new List<Region>();
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

    public override List<Region> GetConflictRegions() {
      return new List<Region>(){Cons.tubo, Cons.qidan};
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

    public override List<Region> GetConflictRegions() {
      return new List<Region>(){Cons.dangxiang};
    }

  }

  public class NvZhenRegion : Region
  {

    public override string Name()
    {
      return textLib.get("region_nvZhenRegion");
    }

    public override string Description()
    {
      return textLib.get("region_nvZhenRegion_description");
    }

    public override int CombatPoint(Type unitType)
    {
      if (unitType == Type.Infantry) return 10;
      return 40;
    }

    public override int DefaultOrganizationPoint()
    {
      return 50;
    }

    public override int MaxOrganizationPoint() {
      return 75;
    }

    public override List<Region> GetConflictRegions() {
      return new List<Region>();
    }
  }

}