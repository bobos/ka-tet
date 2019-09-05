using System.Collections.Generic;

namespace TextNS
{
  public class TextLibChn : TextLib
  {
    public TextLibChn()
    {
      txtDict = new Dictionary<string, string>() {
      {"w_cloudy", "多云"},
      {"w_cloudy_d", "Clear sky good for military activities"},
      {"w_rain", "雨"},
      {"w_rain_d", "Rain can provide drinkable water if army is low on water, \n" +
                 "rain will also stop wild fire from spreading"},
      {"w_heavyRain", "暴雨"},
      {"w_heavyRain_d", "Heavy rain can provide drinkable water if army is low on water, \n" +
                     "rain will also stop wild fire from spreading and most likely cause flood, \n" +
                   "armies better stay away from rivers during heavy rain"},
      {"w_heat", "酷暑"},
      {"w_heat_d", "Due to the unbearable heat, chance of illness is high"},
      {"w_dry", "干燥"},
      {"w_dry_d", "Forest tends to catch fire due to the dry air, good time to set a wild fire on forest, \n" +
               "also armies better stay away from forest to avoid the wild fire in this dry weather caused by nature or " +
               "enemies"},
      {"w_snow", "小雪"},
      {"w_snow_d", "Snow will slow down a marching army and cause some losses on an army due to frozen death, \n" +
                "but snow can also provide drinkable water if army is low on water"},
      {"w_blizard", "暴雪"},
      {"w_blizard_d", "Blizard will slow down a marching army tremendously and cause heavy losses on an army due to frozen death, " +
                "and punish the morale of an army\n" +
                "but blizard can also provide drinkable water if army is low on water"},

      // Season
      {"s_spring", "春"},
      {"s_spring_d", "Spring is a good choice of starting a warfare as weather throughout this season is rather comfortable \n" +
                 "and also the field is replenished with fruits and wild livies which can be added into army's supply inventory"},
      {"s_summer", "夏"},
      {"s_summer_d", "Summer is a hard season for lauching a warfare, as the flood caused by heavy rain is devastating to an army, " +
                 "also the heat in summer will cause dehydration and cause the spreading sickness within the army which camp in forest"},
      {"s_autumn", "秋"},
      {"s_autumn_d", "Autumn is the perfect season for lauching a warfare, because the farms are now filled with grains which are " +
                 "the countless supply deposits for armies which are low on food to fetch, the only thing a general needs to pay attention" +
                 " to in Autumn is the wild fire caused by either enemy armies or by nature"},
      {"s_winter", "冬"},
      {"s_winter_d", "Winter is as hard as Summer for lauching a warfare or even harder, " +
                 "because all the lands are barren there is nothing can be sacavaged from field for supply, " +
                 "and armies are significantly slowed down when march in snow " +
                 "also the horrible blizard in Winter will cause unbearable soldier losses and break even the " +
                 "toughest soldiers' wills for which dare to stay on field not in the Winter camp"},

      // Current
      {"c_nowind", "无风"},
      {"c_nowind_d", "无风"},
      {"c_wind", "有风"},
      {"c_wind_d", "Wind helps wild fire to spread through the direction it blows"},
      {"c_gale", "狂风"},
      {"c_gale_d", "Gale helps wild fire to spread through the direction it blows, also Gale gives " +
                "combat disadvantage to the army face the direction of the blowing"},
            
      // direction
      {"d_dueNorth", "正北"},
      {"d_dueSouth", "正南"},
      {"d_dueWest", "正西"},
      {"d_dueEast", "正东"},
      {"d_northEast", "东北"},
      {"d_northWest", "西北"},
      {"d_southEast", "东南"},
      {"d_southWest", "西南"},

  //   unit
  	  {"u_exhausted", "精疲力竭"},
  	  {"u_tired", "人困马乏"},
  	  {"u_fresh", "充沛"},
  	  {"u_vigorous", "极佳"},
      {"u_concealing", "隐蔽中"},
      {"u_camping", "驻扎中"},
      {"u_routing", "溃败中"},
      {"u_disbanded", "被歼灭"},
      {"u_standing", ""},

      // region
      {"r_riverRun", "河间地"},
      {"r_middleEarth", "河南"},
      {"r_mountainBeyond", "河外"},

		//   legion names
      {"l_1", "第一"},
      {"l_2", "第二"},
      {"l_3", "第三"},
      {"l_4", "第四"},

		  {"l_legion", "军"},

		  {"l_longwei", "龙威"},
      {"l_longshen", "龙武"},
      {"l_huben", "虎贲"},
      {"l_qingshen", "擒生"},
		  {"l_longwei5", "神龙"},
      {"l_longshen5", "龙神"},
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
      {"g_zhangfei", "张飞"},
      {"g_caocao", "曹操"},
      {"g_xuchu", "许褚"},

      // party report
      {"pr_sloppyOnDrill", "疏于操练"},
      {"pr_sloppyOnDrill_d", "General is sloppy on drill, which leads the shameful defeat"},

      // building
      {"b_ownedCamp", "所部所筑营寨"},

      // event
      {"event_wildFire_title", "山火侵袭"},
      {"event_wildFire", @"{0}将军所部{1}突遭猛烈山火所袭，军中士卒疲于避险，已有{2}名士兵和{3}民夫丧命于火海，{4}人不同程度为烈火所伤，部队士气下降{5}点, 若天气干燥之时，将军务必带军避开山高林密之处，以免再为山火所伤！"}
      };
    }
  }
}