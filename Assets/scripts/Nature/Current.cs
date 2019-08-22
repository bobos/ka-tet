using System.Collections;
using System.Collections.Generic;
using TextNS;

namespace NatureNS
{
  public abstract class Current
  {
    protected string currentName;
    protected string currentDescription;
    protected TextLib textLib = Cons.GetTextLib();

    public string Name()
    {
      return currentName;
    }

    public string Description()
    {
      return currentDescription;
    }
  }

  public class Nowind : Current
  {
    public Nowind() : base()
    {
      currentName = textLib.get("c_nowind");
      currentDescription = textLib.get("c_nowind_d");
    }
  }

  public class Wind : Current
  {
    public Wind() : base()
    {
      currentName = textLib.get("c_wind");
      currentDescription = textLib.get("c_wind_d");
    }
  }

  public class Gale : Current
  {
    public Gale() : base()
    {
      currentName = textLib.get("c_gale");
      currentDescription = textLib.get("c_gale_d");
    }
  }
}