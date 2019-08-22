using System.Collections;
using System.Collections.Generic;
using TextNS;

namespace NatureNS
{
  public abstract class Weather
  {
    protected string weatherName;
    protected string weatherDescription;
    protected TextLib textLib = Cons.GetTextLib();

    public string Name()
    {
      return weatherName;
    }

    public string Description()
    {
      return weatherDescription;
    }
  }

  public class Cloudy : Weather
  {
    public Cloudy() : base()
    {
      weatherName = textLib.get("w_cloudy");
      weatherDescription = textLib.get("w_cloudy_d");
    }
  }

  public class Rain : Weather
  {
    public Rain() : base()
    {
      weatherName = textLib.get("w_rain");
      weatherDescription = textLib.get("w_rain_d");
    }
  }

  public class HeavyRain : Weather
  {
    public HeavyRain() : base()
    {
      weatherName = textLib.get("w_heavyRain");
      weatherDescription = textLib.get("w_heavyRain_d");
    }
  }

  public class Heat : Weather
  {
    public Heat() : base()
    {
      weatherName = textLib.get("w_heat");
      weatherDescription = textLib.get("w_heat_d");
    }
  }

  public class Dry : Weather
  {
    public Dry() : base()
    {
      weatherName = textLib.get("w_dry");
      weatherDescription = textLib.get("w_dry_d");
    }
  }

  public class Snow : Weather
  {
    public Snow() : base()
    {
      weatherName = textLib.get("w_snow");
      weatherDescription = textLib.get("w_snow_d");
    }
  }

  public class Blizard : Weather
  {
    public Blizard() : base()
    {
      weatherName = textLib.get("w_blizard");
      weatherDescription = textLib.get("w_blizard_d");
    }
  }
}