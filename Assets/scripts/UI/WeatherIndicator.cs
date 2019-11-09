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
      indicator.text = "今日: " + weatherGenerator.currentWeather.Name() + "\n";
      indicator.text += "预计持续: " + weatherGenerator.weatherLastingTurns + " 日\n";
      indicator.text += "明日: " + weatherGenerator.Forecast().Name() + "\n";
      indicator.text += weatherGenerator.season.Name() + "\n";
      indicator.text += windGenerator.current.Name() + "\n";
      indicator.text += "风向: " + Cons.DirectionDisplay(windGenerator.direction) + "\n";
      indicator.text += "明日: " + windGenerator.ForecastWind().Name() + "\n";
      indicator.text += "明日风向: " + Cons.DirectionDisplay(windGenerator.ForecastDirection()) + "\n";
      indicator.text += hexMap.warProvince.Name();
    }

    public override void UpdateChild() {}

  }

}