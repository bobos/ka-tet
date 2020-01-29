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
    // Min atk or def point must be x * 0.05f = 1 --> 20
    public abstract int Atk(Type unitType);
    public abstract int Def(Type unitType);
    public abstract int Mov(Type unitType);
    public abstract int Will();
    public abstract int RetreatThreshold();
    public abstract int ExtraSupplySlot();
  }

  public class Upland : Region
  {

    public override string Name()
    {
      return textLib.get("region_upland");
    }

    public override string Description()
    {
      return textLib.get("region_upland_description");
    }

    public override int Atk(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 40;
      if (unitType == Type.Scout)
        return 150;
      return 100;
    }

    public override int Def(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 50;
      if (unitType == Type.Scout)
        return 50;
      return 20;
    }

    public override int Mov(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 100;
      return 170;
    }

    public override int Will()
    {
      return 85;
    }

    public override int RetreatThreshold()
    {
      return 45;
    }

    public override int ExtraSupplySlot()
    {
      return 0;
    }
  }

  public class Plain : Region
  {

    public override string Name()
    {
      return textLib.get("region_plain");
    }

    public override string Description()
    {
      return textLib.get("region_plain_description");
    }

    public override int Atk(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 50;
      if (unitType == Type.Scout)
        return 130;
      return 80;
    }

    public override int Def(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 80;
      if (unitType == Type.Scout)
        return 80;
      return 30;
    }

    public override int Mov(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 100;
      return 160;
    }

    public override int Will()
    {
      return 80;
    }

    public override int RetreatThreshold()
    {
      return 40;
    }

    public override int ExtraSupplySlot()
    {
      return 1;
    }
  }

  public class Lowland : Region
  {

    public override string Name()
    {
      return textLib.get("region_lowland");
    }

    public override string Description()
    {
      return textLib.get("region_lowland_description");
    }

    public override int Atk(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 40;
      if (unitType == Type.Scout)
        return 120;
      return 70;
    }

    public override int Def(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 50;
      if (unitType == Type.Scout)
        return 50;
      return 30;
    }

    public override int Mov(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 100;
      return 140;
    }

    public override int Will()
    {
      return 80;
    }

    public override int RetreatThreshold()
    {
      return 40;
    }

    public override int ExtraSupplySlot()
    {
      return 0;
    }
  }

  public class Hillland : Region
  {

    public override string Name()
    {
      return textLib.get("region_hillland");
    }

    public override string Description()
    {
      return textLib.get("region_hillland_description");
    }

    public override int Atk(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 60;
      if (unitType == Type.Scout)
        return 130;
      return 80;
    }

    public override int Def(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 90;
      if (unitType == Type.Scout)
        return 90;
      return 30;
    }

    public override int Mov(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 100;
      return 150;
    }

    public override int Will()
    {
      return 90;
    }

    public override int RetreatThreshold()
    {
      return 35;
    }

    public override int ExtraSupplySlot()
    {
      return 0;
    }
  }

  public class Grassland : Region
  {

    public override string Name()
    {
      return textLib.get("region_grassland");
    }

    public override string Description()
    {
      return textLib.get("region_grassland_description");
    }

    public override int Atk(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 40;
      if (unitType == Type.Scout)
        return 150;
      return 100;
    }

    public override int Def(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 40;
      if (unitType == Type.Scout)
        return 60;
      return 40;
    }

    public override int Mov(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 80;
      return 180;
    }

    public override int Will()
    {
      return 85;
    }

    public override int RetreatThreshold()
    {
      return 40;
    }

    public override int ExtraSupplySlot()
    {
      return 0;
    }
  }

}