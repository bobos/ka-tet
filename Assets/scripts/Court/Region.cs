using System.Collections.Generic;
using UnitNS;
using TextNS;

namespace CourtNS
{
  public abstract class Region
  {
    protected TextLib textLib = Cons.GetTextLib();

    public abstract string Name();

    public abstract string Description();

    public abstract float AtkBuf(Type unitType);
    public abstract float DefBuf(Type unitType);
    public abstract float MovBuf(Type unitType);
    public abstract int MoraleBuf();
    public abstract int RetreatThreshold();
    public abstract int ExtraSupplySlot();

    protected Dictionary<string, int> CavNameSeq;
    protected Dictionary<string, int> InfNameSeq;

    public string AssignLegionName(Type unitType) {
      Dictionary<string, int> seq = unitType == Type.Cavalry ? CavNameSeq : InfNameSeq;
      Dictionary<string, int>.KeyCollection keys = seq.Keys;
      int index = Util.Rand(0, keys.Count - 1);
      string pickedName = "";
      int i = 0;
      foreach (string key in keys)
      {
        if (i++ == index) {
          pickedName = key;
          break;
        }
      }
      seq[pickedName] = seq[pickedName] + 1;
      return Name() + textLib.get(pickedName) + textLib.get("l_" + seq[pickedName]) + textLib.get("l_legion");
    }
  }

  public class RiverRun : Region
  {
    private string name;
    private string description;

    public RiverRun()
    {
      CavNameSeq = new Dictionary<string, int>() {
        {"l_longwei", 0},
        {"l_longshen", 0}
      };
      InfNameSeq = new Dictionary<string, int>() {
        {"l_huben", 0}
      };
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

    public override float AtkBuf(Type unitType)
    {
      if (unitType == Type.Infantry)
      {
        return 0.2f;
      }
      return 0;
    }

    public override float DefBuf(Type unitType)
    {
      if (unitType == Type.Infantry)
      {
        return 0.25f;
      }
      return 0.15f;
    }

    public override float MovBuf(Type unitType)
    {
      if (unitType == Type.Infantry)
      {
        return 0.2f;
      }
      return 0.1f;
    }

    public override int MoraleBuf()
    {
      return 10;
    }

    public override int RetreatThreshold()
    {
      return 30;
    }

    public override int ExtraSupplySlot()
    {
      return 0;
    }
  }

  public class RiverWest : Region
  {
    private string name;
    private string description;

    public RiverWest()
    {
      CavNameSeq = new Dictionary<string, int>() {
        {"l_longwei1", 0},
        {"l_longshen1", 0}
      };
      InfNameSeq = new Dictionary<string, int>() {
        {"l_huben1", 0},
        {"l_qingshen1", 0}
      };
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

    public override float AtkBuf(Type unitType)
    {
      if (unitType == Type.Infantry)
      {
        return 0.3f;
      }
      return 0.1f;
    }

    public override float DefBuf(Type _unitType)
    {
      return -0.2f;
    }

    public override float MovBuf(Type _unitType)
    {
      return 0;
    }

    public override int MoraleBuf()
    {
      return 0;
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

  public class RiverEast : Region
  {
    private string name;
    private string description;

    public RiverEast()
    {
      CavNameSeq = new Dictionary<string, int>() {
        {"l_longwei2", 0},
        {"l_longshen2", 0}
      };
      InfNameSeq = new Dictionary<string, int>() {
        {"l_huben2", 0},
        {"l_qingshen2", 0}
      };
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

    public override float AtkBuf(Type unitType)
    {
      return 0.1f;
    }

    public override float DefBuf(Type _unitType)
    {
      return 0.2f;
    }

    public override float MovBuf(Type _unitType)
    {
      return -0.2f;
    }

    public override int MoraleBuf()
    {
      return 0;
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

  public class RiverSouth : Region
  {
    private string name;
    private string description;

    public RiverSouth()
    {
      CavNameSeq = new Dictionary<string, int>() {
        {"l_longwei3", 0},
        {"l_longshen3", 0}
      };
      InfNameSeq = new Dictionary<string, int>() {
        {"l_huben3", 0},
        {"l_qingshen3", 0}
      };
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

    public override float AtkBuf(Type unitType)
    {
      return -0.2f;
    }

    public override float DefBuf(Type _unitType)
    {
      return 0.2f;
    }

    public override float MovBuf(Type _unitType)
    {
      return 0;
    }

    public override int MoraleBuf()
    {
      return 0;
    }

    public override int RetreatThreshold()
    {
      return 42;
    }

    public override int ExtraSupplySlot()
    {
      return 0;
    }

  }

  public class RiverBeyond : Region
  {
    private string name;
    private string description;

    public RiverBeyond()
    {
      CavNameSeq = new Dictionary<string, int>() {
        {"l_longwei4", 0},
        {"l_longshen4", 0}
      };
      InfNameSeq = new Dictionary<string, int>() {
        {"l_huben4", 0},
        {"l_qingshen4", 0}
      };
      name = textLib.get("r_riverBeyond");
      description = textLib.get("r_riverBeyond_d");
    }

    public override string Name()
    {
      return name;
    }

    public override string Description()
    {
      return description;
    }

    public override float AtkBuf(Type unitType)
    {
      if (unitType == Type.Cavalry)
      {
        return 0.3f;
      }
      return -0.2f;
    }

    public override float DefBuf(Type _unitType)
    {
      return -0.1f;
    }

    public override float MovBuf(Type unitType)
    {
      if (unitType == Type.Cavalry)
      {
        return 0.25f;
      }
      return -0.1f;
    }

    public override int MoraleBuf()
    {
      return 0;
    }

    public override int RetreatThreshold()
    {
      return 38;
    }

    public override int ExtraSupplySlot()
    {
      return -1;
    }

  }

  public class MiddleEarth : Region
  {
    private string name;
    private string description;

    public MiddleEarth()
    {
      CavNameSeq = new Dictionary<string, int>() {
        {"l_longwei5", 0},
        {"l_longshen5", 0}
      };
      InfNameSeq = new Dictionary<string, int>() {
        {"l_huben5", 0},
        {"l_qingshen5", 0}
      };
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

    public override float AtkBuf(Type _unitType)
    {
      return 0;
    }

    public override float DefBuf(Type _unitType)
    {
      return 0;
    }

    public override float MovBuf(Type _unitType)
    {
      return 0;
    }

    public override int MoraleBuf()
    {
      return 0;
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

  public class MountainBeyond : Region
  {
    private string name;
    private string description;

    public MountainBeyond()
    {
      CavNameSeq = new Dictionary<string, int>() {
        {"l_longwei6", 0},
        {"l_longshen6", 0}
      };
      InfNameSeq = new Dictionary<string, int>() {
        {"l_huben6", 0},
        {"l_qingshen6", 0}
      };
      name = textLib.get("r_mountainBeyond");
      description = textLib.get("r_mountainBeyond_d");
    }

    public override string Name()
    {
      return name;
    }

    public override string Description()
    {
      return description;
    }

    public override float AtkBuf(Type _unitType)
    {
      return -0.2f;
    }

    public override float DefBuf(Type _unitType)
    {
      return -0.1f;
    }

    public override float MovBuf(Type _unitType)
    {
      return -0.2f;
    }

    public override int MoraleBuf()
    {
      return 0;
    }

    public override int RetreatThreshold()
    {
      return 45;
    }

    public override int ExtraSupplySlot()
    {
      return 1;
    }

  }
}