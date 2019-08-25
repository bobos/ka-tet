using System.Collections;
using System.Collections.Generic;

namespace TextNS
{
  public abstract class TextLib
  {
    private Dictionary<string, string> defaultTxtDict = new Dictionary<string, string>() {
    {"w_cloudy", "Cloudy"},
    {"w_cloudy_d", "Clear sky good for military activities"},
    {"w_rain", "Rain"},
    {"w_rain_d", "Rain can provide drinkable water if army is low on water, \n" +
               "rain will also stop wild fire from spreading"},
    {"w_heavyRain", "Heavy Rain"},
    {"w_heavyRain_d", "Heavy rain can provide drinkable water if army is low on water, \n" +
                   "rain will also stop wild fire from spreading and most likely cause flood, \n" +
                 "armies better stay away from rivers during heavy rain"},
    {"w_heat", "Heat"},
    {"w_heat_d", "Due to the unbearable heat, chance of illness is high"},
    {"w_dry", "Dry Air"},
    {"w_dry_d", "Forest tends to catch fire due to the dry air, good time to set a wild fire on forest, \n" +
             "also armies better stay away from forest to avoid the wild fire in this dry weather caused by nature or " +
             "enemies"},
    {"w_snow", "Snow"},
    {"w_snow_d", "Snow will slow down a marching army and cause some losses on an army due to frozen death, \n" +
              "but snow can also provide drinkable water if army is low on water"},
    {"w_blizard", "Blizard"},
    {"w_blizard_d", "Blizard will slow down a marching army tremendously and cause heavy losses on an army due to frozen death, " +
              "and punish the morale of an army\n" +
              "but blizard can also provide drinkable water if army is low on water"},

    // Season
    {"s_spring", "Spring"},
    {"s_spring_d", "Spring is a good choice of starting a warfare as weather throughout this season is rather comfortable \n" +
               "and also the field is replenished with fruits and wild livies which can be added into army's supply inventory"},
    {"s_summer", "Summer"},
    {"s_summer_d", "Summer is a hard season for lauching a warfare, as the flood caused by heavy rain is devastating to an army, " +
               "also the heat in summer will cause dehydration and cause the spreading sickness within the army which camp in forest"},
    {"s_autumn", "Autumn"},
    {"s_autumn_d", "Autumn is the perfect season for lauching a warfare, because the farms are now filled with grains which are " +
               "the countless supply deposits for armies which are low on food to fetch, the only thing a general needs to pay attention" +
               " to in Autumn is the wild fire caused by either enemy armies or by nature"},
    {"s_winter", "Winter"},
    {"s_winter_d", "Winter is as hard as Summer for lauching a warfare or even harder, " +
               "because all the lands are barren there is nothing can be sacavaged from field for supply, " +
               "and armies are significantly slowed down when march in snow " +
               "also the horrible blizard in Winter will cause unbearable soldier losses and break even the " +
               "toughest soldiers' wills for which dare to stay on field not in the Winter camp"},

    // Current
    {"c_nowind", "No Wind"},
    {"c_nowind_d", "No Wind"},
    {"c_wind", "Wind"},
    {"c_wind_d", "Wind helps wild fire to spread through the direction it blows"},
    {"c_gale", "Gale"},
    {"c_gale_d", "Gale helps wild fire to spread through the direction it blows, also Gale gives " +
              "combat disadvantage to the army face the direction of the blowing"},
          
    // direction
    {"d_dueNorth", "Due North"},
    {"d_dueSouth", "Due South"},
    {"d_dueWest", "Due West"},
    {"d_dueEast", "Due East"},
    {"d_northEast", "North East"},
    {"d_northWest", "North West"},
    {"d_southEast", "South East"},
    {"d_southWest", "South West"},

  // unit
  	{"u_exhausted", "Exhausted"},
  	{"u_tired", "Tired"},
  	{"u_fresh", "Fresh"},
  	{"u_vigorous", "Vigorous"},
    {"u_concealing", "Concealling"},
    {"u_camping", "Camping"},
    {"u_routing", "Routing"},
    {"u_disbanded", "Disbanded"},
    {"u_standing", "Standing"},

    // region
    {"r_riverRun", "河间地"},
    {"r_middleEarth", "河南"},
    {"r_mountainBeyond", "河外"},

		// legion names
    {"l_1", "第一"},
    {"l_2", "第二"},
    {"l_3", "第三"},
    {"l_4", "第四"},

		{"l_legion", "军"},

		{"l_longwei", "龙威"},
    {"l_longshen", "龙武"},
    {"l_huben", "虎贲"},
    {"l_longshen5", "龙神"},
    {"l_qingshen", "擒生"},
		{"l_longwei6", "虎威"},
    {"l_longshen6", "虎武"},
    {"l_huben6", "期门"},
    {"l_qingshen6", "捉生"},

    // faction
    {"f_liang", "梁"},
    {"f_hejian", "河间"},

    // generals
    {"g_liubei", "刘备"},
    {"g_guanyu", "关羽"},
    {"g_zhaoyun", "赵云"},
    {"g_caocao", "曹操"},
    {"g_xuchu", "许褚"},

    // party report
    {"pr_sloppyOnDrill", "General Sloppy On Drill"},
    {"pr_sloppyOnDrill_d", "General is sloppy on drill, which leads the shameful defeat"}
  };

  protected Dictionary<string, string> txtDict = null;

    public string get(string key)
    {
      string ret;
      if (!(txtDict != null && txtDict.TryGetValue(key, out ret)))
      {
        // use default txt lib
        if (!defaultTxtDict.TryGetValue(key, out ret))
        {
          return "TXT NOT FOUND";
        }
      }
      return ret;
    }
  }

  public class TextLibEng : TextLib { }
}