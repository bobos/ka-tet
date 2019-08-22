using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MonoNS
{
  public class WeatherIndicator : BaseController
  {

    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      hexMap.turnController.onNewTurn += OnNewTurn;
      weatherGenerator = hexMap.weatherGenerator;
      windGenerator = hexMap.windGenerator;
    }
    public Text indicator;
    WeatherGenerator weatherGenerator;
    WindGenerator windGenerator;

    public void OnNewTurn()
    {
      indicator.text = "Today Weather: " + weatherGenerator.currentWeather.Name() + "\n";
      indicator.text += "Lasts: " + weatherGenerator.weatherLastingTurns + " days\n";
      indicator.text += "Tomorrow Weather: " + weatherGenerator.Forecast().Name() + "\n";
      indicator.text += weatherGenerator.season.Name() + "\n";
      indicator.text += "Current Wind: " + windGenerator.current.Name() + "\n";
      indicator.text += "Wind direction: " + Cons.DirectionDisplay(windGenerator.direction) + "\n";
      indicator.text += "Tomorrow Wind: " + windGenerator.ForecastWind().Name() + "\n";
      indicator.text += "Tomorrow Wind direction: " + Cons.DirectionDisplay(windGenerator.ForecastDirection()) + "\n";
    }

    public override void UpdateChild() {}

  }

}