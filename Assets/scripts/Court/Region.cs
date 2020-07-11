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
      if (unitType == Type.LightCavalry) return 20;
      return 50;
    }
    public virtual float LevelBuf(Type unitType) {
      if (unitType == Type.HeavyCavalry) return 0f;
      return 0.2f;
    }
    public abstract int Will();
    public abstract int RetreatThreshold();
    public abstract int MoralePunishLine();
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

    public override int Will()
    {
      return 90;
    }

    public override int MoralePunishLine()
    {
      return 55;
    }

    public override int RetreatThreshold()
    {
      return 35;
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

    public override int Will()
    {
      return 90;
    }

    public override int MoralePunishLine()
    {
      return 55;
    }

    public override int RetreatThreshold()
    {
      return 35;
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

    public override int Will()
    {
      return 80;
    }

    public override int MoralePunishLine()
    {
      return 55;
    }

    public override int RetreatThreshold()
    {
      return 40;
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

    public override int CombatPoint(Type unitType)
    {
      if (unitType == Type.Infantry) return 10;
      if (unitType == Type.LightCavalry) return 20;
      return 40;
    }

    public override int Will()
    {
      return 85;
    }

    public override int MoralePunishLine()
    {
      return 50;
    }

    public override int RetreatThreshold()
    {
      return 40;
    }

    public override List<Region> GetConflictRegions() {
      return new List<Region>(){Cons.dangxiang};
    }

    public override float LevelBuf(Type unitType) {
      if (unitType == Type.HeavyCavalry) return 0f;
      return 0.1f;
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
      if (unitType == Type.LightCavalry) return 20;
      return 60;
    }

    public override int Will()
    {
      return 100;
    }

    public override int MoralePunishLine()
    {
      return 30;
    }

    public override int RetreatThreshold()
    {
      return 20;
    }

    public override List<Region> GetConflictRegions() {
      return new List<Region>();
    }
  }

}