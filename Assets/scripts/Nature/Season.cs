using System.Collections;
using System.Collections.Generic;
using TextNS;

namespace NatureNS
{
  public abstract class Season
  {
    protected string seasonName;
    protected string seasonDescription;
    protected TextLib textLib = Cons.GetTextLib();

    public string Name()
    {
      return seasonName;
    }

    public string Description()
    {
      return seasonDescription;
    }
  }

  public class Spring : Season
  {
    public Spring() : base()
    {
      seasonName = textLib.get("s_spring");
      seasonDescription = textLib.get("s_spring_d");
    }
  }

  public class Summer : Season
  {
    public Summer() : base()
    {
      seasonName = textLib.get("s_summer");
      seasonDescription = textLib.get("s_summer_d");
    }
  }

  public class Autumn : Season
  {
    public Autumn() : base()
    {
      seasonName = textLib.get("s_autumn");
      seasonDescription = textLib.get("s_autumn_d");
    }
  }

  public class Winter : Season
  {
    public Winter() : base()
    {
      seasonName = textLib.get("s_winter");
      seasonDescription = textLib.get("s_winter_d");
    }
  }
}