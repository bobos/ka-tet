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

  }

  // 河湟 吐蕃
  public class HeHuang : Province
  {
    private string name;
    private string description;

    public HeHuang(): base(new TuboRegion())
    {
      name = textLib.get("r_heHuang");
      description = textLib.get("r_heHuang_d");
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

  // 河西 西夏
  public class HeXi : Province
  {
    private string name;
    private string description;

    public HeXi(): base(new DangxiangRegion())
    {
      name = textLib.get("r_heXi");
      description = textLib.get("r_heXi_d");
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

  // 河东
  public class HeDong : Province
  {
    private string name;
    private string description;

    public HeDong(): base(new HanRegion())
    {
      name = textLib.get("r_heDong");
      description = textLib.get("r_heDong_d");
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

  // 河北
  public class HeBei : Province
  {
    private string name;
    private string description;

    public HeBei(): base(new HanRegion())
    {
      name = textLib.get("r_heBei");
      description = textLib.get("r_heBei_d");
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

  // 河南
  public class HeNan : Province
  {
    private string name;
    private string description;

    public HeNan(): base(new HanRegion())
    {
      name = textLib.get("r_heNan");
      description = textLib.get("r_heNan_d");
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

  // 陕西
  public class ShanXi : Province
  {
    private string name;
    private string description;

    public ShanXi(): base(new HanRegion())
    {
      name = textLib.get("r_shanXi");
      description = textLib.get("r_shanXi_d");
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

  // 西京
  public class XiJing : Province
  {
    private string name;
    private string description;

    public XiJing(): base(new QidanRegion())
    {
      name = textLib.get("r_xiJing");
      description = textLib.get("r_xiJing_d");
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

  // 中京
  public class ZhongJing : Province
  {
    private string name;
    private string description;

    public ZhongJing(): base(new HanRegion())
    {
      name = textLib.get("r_zhongJing");
      description = textLib.get("r_zhongJing_d");
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

  // 上京
  public class ShangJing : Province
  {
    private string name;
    private string description;

    public ShangJing(): base(new QidanRegion())
    {
      name = textLib.get("r_shangJing");
      description = textLib.get("r_shangJing_d");
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