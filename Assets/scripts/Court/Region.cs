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
    public abstract List<Region> GetConflictRegions();
    public abstract float LevelBuf(Type unitType);
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
      return 40;
    }

    public override int Will()
    {
      return 90;
    }

    public override int RetreatThreshold()
    {
      return 35;
    }

    public override List<Region> GetConflictRegions() {
      return new List<Region>(){Cons.nvzhen, Cons.dangxiang};
    }

    public override float LevelBuf(Type unitType) {
      if (unitType == Type.Cavalry) {
        return 0.25f;
      }
      return 0.1f;
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
      return 90;
    }

    public override int RetreatThreshold()
    {
      return 35;
    }

    public override List<Region> GetConflictRegions() {
      return new List<Region>();
    }

    public override float LevelBuf(Type _unitType) {
      return 0.5f;
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

    public override List<Region> GetConflictRegions() {
      return new List<Region>(){Cons.tubo, Cons.qidan};
    }

    public override float LevelBuf(Type unitType) {
      return 0.5f;
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
      return 85;
    }

    public override int RetreatThreshold()
    {
      return 45;
    }

    public override List<Region> GetConflictRegions() {
      return new List<Region>(){Cons.dangxiang};
    }

    public override float LevelBuf(Type unitType) {
      return 0.3f;
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
      if (unitType == Type.Infantry)
        return 10;
      return 25;
    }

    public override int Will()
    {
      return 100;
    }

    public override int RetreatThreshold()
    {
      return 10;
    }

    public override List<Region> GetConflictRegions() {
      return new List<Region>();
    }

    public override float LevelBuf(Type _unitType) {
      return 1f;
    }
  }

}