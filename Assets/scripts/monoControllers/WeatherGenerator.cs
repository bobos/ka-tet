using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NatureNS;
using UnitNS;
using MapTileNS;

namespace MonoNS
{
  public class WeatherGenerator : BaseController
  {

    // TODO: instance per province
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      turnController = hexMap.turnController;
      currentWeather = GenerateWeather();
      weatherLastingTurns = DecideLastingDays(currentWeather);
      NextDay();
    }

    public override void UpdateChild() {}

    public HashSet<Tile> tileCB = new HashSet<Tile>();
    public Weather currentWeather;
    Weather tomorrowWeather;
    public int weatherLastingTurns = 0;
    // TODO: OnSeasonChange Event
    Season _season;
    public Season season
    {
      get { return _season; }
      set
      {
        _season = value;
      }
    }

    TurnController turnController;

    public Weather NextDay()
    {
      if (weatherLastingTurns == 0)
      {
        currentWeather = tomorrowWeather;
        weatherLastingTurns = DecideLastingDays(currentWeather);
      }
      weatherLastingTurns--;
      if (weatherLastingTurns > 0)
      {
        tomorrowWeather = currentWeather;
      }
      else
      {
        // generate future weather
        tomorrowWeather = GenerateWeather();
      }
      return currentWeather;
    }

    public Weather Forecast()
    {
      return tomorrowWeather;
    }

    Weather GenerateWeather()
    {
      int luckNum = Util.Rand(1, 10);
      if (Cons.IsSpring(season))
      {
        // clear sky for most of Spring time
        if (luckNum < 6)
        {
          return Cons.cloudy;
        } else if (luckNum < 9) {
          return Cons.rain;
        } else {
          return Cons.heavyRain;
        }
      }
      else if (Cons.IsSummer(season))
      {
        if (luckNum < 11)
        {
          return Cons.heavyRain;
        }
        else if (luckNum < 5)
        {
          return Cons.rain;
        }
        else if (luckNum < 7)
        {
          return Cons.heavyRain;
        }
        else
        {
          return Cons.heat;
        }
      }
      else if (Cons.IsAutumn(season))
      {
        if (luckNum < 4)
        {
          return Cons.cloudy;
        }
        else if (luckNum < 6)
        {
          return Cons.rain;
        } else if (luckNum < 7) {
          return Cons.heat;
        }
        else
        {
          return Cons.dry;
        }
      }
      else
      {
        if (luckNum < 7)
        {
          return Cons.snow;
        }
        else
        {
          return Cons.blizard;
        }
      }
    }

    int DecideLastingDays(Weather weather)
    {
      // now decides how long the weather lasts
      if (Cons.IsCloudy(weather))
      {
        return Util.Rand(3, 8);
      }
      else if (Cons.IsRain(weather))
      {
        return Util.Rand(1, 6);
      }
      else if (Cons.IsHeavyRain(weather))
      {
        return Util.Rand(1, 3);
      }
      else if (Cons.IsHeat(weather))
      {
        return Util.Rand(1, 4);
      }
      else if (Cons.IsDry(weather))
      {
        return Util.Rand(2, 6);
      }
      else if (Cons.IsSnow(weather))
      {
        return Util.Rand(3, 8);
      }
      else
      {
        // blizard
        return Util.Rand(2, 6);
      }
    }

  }

}