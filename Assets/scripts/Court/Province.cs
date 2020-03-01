using System.Collections.Generic;
using UnitNS;
using TextNS;

namespace CourtNS
{
  public abstract class Province
  {
    protected TextLib textLib = Cons.GetTextLib();
    public Region region;
    public abstract string Name();
    public abstract string Description();

    public Faction ownerFaction;
    public Party ownerParty;

    protected Province (Region region) {
      this.region = region;
    }

    public string AssignLegionName(Type unitType) {
      return System.String.Format(
        unitType == Type.Infantry ? textLib.get("u_infantryName") : textLib.get("u_cavalryName"),
        Name()
      );
    }

    public void DeductOneAgriculturePoint() {
      // TODO: update this
    }

  }

  // HeZhong
  public class RiverRun : Province
  {
    private string name;
    private string description;

    public RiverRun(Region region): base(region)
    {
      name = textLib.get("r_riverRun");
      description = textLib.get("r_riverRun_d");
    }
    
    public override string Name()
    {
      return name;
    }

    public override string Description()
    {
      return description;
    }
  }

  // He Xi
  public class RiverWest : Province
  {
    private string name;
    private string description;

    public RiverWest(Region region): base(region)
    {
      name = textLib.get("r_riverWest");
      description = textLib.get("r_riverWest_d");
    }

    public override string Name()
    {
      return name;
    }

    public override string Description()
    {
      return description;
    }
  }

  // He Dong
  public class RiverEast : Province
  {
    private string name;
    private string description;

    public RiverEast(Region region): base(region)
    {
      name = textLib.get("r_riverEast");
      description = textLib.get("r_riverEast_d");
    }

    public override string Name()
    {
      return name;
    }

    public override string Description()
    {
      return description;
    }

  }

  // He Nan
  public class RiverSouth : Province
  {
    private string name;
    private string description;

    public RiverSouth(Region region): base(region)
    {
      name = textLib.get("r_riverSouth");
      description = textLib.get("r_riverSouth_d");
    }

    public override string Name()
    {
      return name;
    }

    public override string Description()
    {
      return description;
    }

  }

  // He Bei
  public class RiverNorth : Province
  {
    private string name;
    private string description;

    public RiverNorth(Region region): base(region)
    {
      name = textLib.get("r_riverNorth");
      description = textLib.get("r_riverNorth_d");
    }

    public override string Name()
    {
      return name;
    }

    public override string Description()
    {
      return description;
    }

  }

  // Mo Bei
  public class FarNorth : Province
  {
    private string name;
    private string description;

    public FarNorth(Region region): base(region)
    {
      name = textLib.get("r_farNorth");
      description = textLib.get("r_farNorth_d");
    }

    public override string Name()
    {
      return name;
    }

    public override string Description()
    {
      return description;
    }
  }

  // Guan Wai
  public class FarWest : Province
  {
    private string name;
    private string description;

    public FarWest(Region region): base(region)
    {
      name = textLib.get("r_farWest");
      description = textLib.get("r_farWest_d");
    }

    public override string Name()
    {
      return name;
    }

    public override string Description()
    {
      return description;
    }

  }

  // Guan Zhong
  public class MiddleEarth : Province
  {
    private string name;
    private string description;

    public MiddleEarth(Region region): base(region)
    {
      name = textLib.get("r_middleEarth");
      description = textLib.get("r_middleEarth_d");
    }

    public override string Name()
    {
      return name;
    }

    public override string Description()
    {
      return description;
    }

  }

  // Huai Xi 
  public class HuaiWest : Province
  {
    private string name;
    private string description;

    public HuaiWest(Region region): base(region)
    {
      name = textLib.get("r_huaiWest");
      description = textLib.get("r_huaiWest_d");
    }

    public override string Name()
    {
      return name;
    }

    public override string Description()
    {
      return description;
    }
  }

  // Huai Bei 
  public class HuaiNorth : Province
  {
    private string name;
    private string description;

    public HuaiNorth(Region region): base(region)
    {
      name = textLib.get("r_huaiNorth");
      description = textLib.get("r_huaiNorth_d");
    }

    public override string Name()
    {
      return name;
    }

    public override string Description()
    {
      return description;
    }

  }

  // Huai Nan
  public class HuaiSouth : Province
  {
    private string name;
    private string description;

    public HuaiSouth(Region region): base(region)
    {
      name = textLib.get("r_huaiSouth");
      description = textLib.get("r_huaiSouth_d");
    }

    public override string Name()
    {
      return name;
    }

    public override string Description()
    {
      return description;
    }
  }
}