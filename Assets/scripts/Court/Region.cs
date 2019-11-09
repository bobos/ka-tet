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
        return 80;
      return 200;
    }

    public override int Def(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 100;
      return 50;
    }

    public override int Mov(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 100;
      return 180;
    }

    public override int Will()
    {
      return 65;
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
        return 100;
      return 150;
    }

    public override int Def(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 150;
      return 50;
    }

    public override int Mov(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 100;
      return 160;
    }

    public override int Will()
    {
      return 60;
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
        return 90;
      return 130;
    }

    public override int Def(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 100;
      return 50;
    }

    public override int Mov(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 100;
      return 130;
    }

    public override int Will()
    {
      return 60;
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
        return 120;
      return 160;
    }

    public override int Def(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 180;
      return 50;
    }

    public override int Mov(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 100;
      return 150;
    }

    public override int Will()
    {
      return 75;
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
        return 80;
      return 200;
    }

    public override int Def(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 80;
      return 80;
    }

    public override int Mov(Type unitType)
    {
      if (unitType == Type.Infantry)
        return 80;
      return 200;
    }

    public override int Will()
    {
      return 65;
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