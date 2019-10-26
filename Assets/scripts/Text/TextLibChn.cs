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
      {"u_satisfied", @"士卒满意({0})"},
      {"u_unsatisfied", @"士卒不满({0})"},
      {"u_riot", @"士卒骚动{0}"},

      // region
      {"r_riverRun", "河中"},
      {"r_riverSouth", "河南"},
      {"r_riverNorth", "河北"},
      {"r_riverEast", "河东"},
      {"r_riverWest", "河西"},
      {"r_huaiWest", "淮西"},
      {"r_huaiNorth", "淮北"},
      {"r_huaiSouth", "淮南"},
      {"r_middleEarth", "关中"},
      {"r_farWest", "关外"},
      {"r_farNorth", "漠北"},

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
      {"g_liubei", "李存勖"},
      {"g_guanyu", "李存审"},
      {"g_zhaoyun", "李嗣昭"},
      {"g_zhangfei", "元行钦"},
      {"g_caocao", "朱温"},
      {"g_xuchu", "王彦章"},
      {"g_machao", "夏鲁奇"},

      // party report
      {"pr_sloppyOnDrill", "疏于操练"},
      {"pr_sloppyOnDrill_d", "General is sloppy on drill, which leads the shameful defeat"},

      // building
      {"b_ownedCamp", "所部所筑营寨"},

      // event
      {"event_fireDestroyCamp_title", "火烧连营"},
      {"event_fireDestroyCamp", @"我军{0}突发火情，扑之不灭，营寨终遭烈火焚毁，营中{1}民夫丧命于火海，{2}石粮草付诸一炬, 营中守军亦全数丧生于火海，损失极为惨重，将军扎营下寨尽量避开山高地陡之处，此等地形易被山火覆盖！"},
      {"event_burningCampDestroyUnit_title", "军营大火"},
      {"event_burningCampDestroyUnit", @"{0}将军所部{1}遭遇军营大火，军中{2}官兵民夫全数丧生火海！"},
      {"event_floodDestroyCamp_title", "水淹七军"},
      {"event_floodDestroyCamp", @"我军{0}突发水情，营寨被洪水淹没，营中{1}民夫丧命于洪水，{2}石粮草被冲走, 营中守军亦全数溺亡，损失极为惨重！"},
      {"event_floodDestroyUnit_title", "无人生还"},
      {"event_floodDestroyUnit", @"{0}将军所部{1}突遇洪水，军中{2}官兵民夫全数殒命！"},
      {"event_enemyCaptureCamp_title", "军营失守"},
      {"event_enemyCaptureCamp", @"我军{0}已落入敌军之手！"},
      {"event_insufficientLabor_title", "转移失败"},
      {"event_insufficientLabor", @"{0}仅有{2}民夫可用，无法满足{1}民夫的转移请求！"},
      {"event_insufficientSupply_title", "粮草转运失败"},
      {"event_insufficientSupply", @"{0}仅存{2}石粮草，无法满足{1}石粮草的转运请求！"},
      {"event_insufficientSupplyLabor_title", "粮草转运失败"},
      {"event_insufficientSupplyLabor", @"转运{0}石粮草需要{1}名民夫运输，目前{2}中仅有{3}名民夫可用差遣，其余民夫已有其他担负任务在身"},
      {"event_supplyDone_title", "粮草转运完成"},
      {"event_supplyDone", @"{1}石粮草已成功转运至{0}！"},
      {"event_laborDone_title", "转移完成"},
      {"event_laborDone", @"{1}民夫已成功转移至{0}！"},
      {"event_supplyIntercepted_title", "粮道遭劫"},
      {"event_supplyIntercepted", @"发往{0}的运粮队遭敌军伏击，{1}石粮草落入敌手，参与押运的{2}名民夫被杀，余者已逃回，请立即派兵肃清后方袭扰之敌！"},
      {"event_laborIntercepted_title", "转移失败"},
      {"event_laborIntercepted", @"往{0}方向转移的民夫队伍遭敌军伏击，{1}名民夫被杀，余者已逃回，请立即派兵肃清后方袭扰之敌！"},
      {"event_unitSupplyIntercepted_title", "粮道遭袭"},
      {"event_unitSupplyIntercepted", @"给{0}将军所部{1}提供日常补给的补给队遭敌军伏击，{2}石粮草落入敌手，{3}名押运民夫被杀，余者已逃回，请立即派兵肃清后方袭扰之敌！"},
      {"event_supplyRouteBlocked_title", "补给线阻断"},
      {"event_supplyRouteBlocked", @"我军{0}与{1}之间的所有补给线遭到敌军切断，请立即派兵驱逐补给线上敌军以恢复两地交通补给！"},
      {"event_disarmor_title", "士卒抱怨"},
      {"event_disarmor", @"{0}将军，酷暑连日，将士终日顶盔贯甲日夜巡逻操练以致多人中暑晕厥，军中颇多不满，{1}将士恳请将军允许士卒平素只用着毡帽上身可免披铁甲以解暑热"},
      {"event_disarmor_approve_title", @"许!(部队防御-{0})"},
      {"event_disarmor_disapprove_title", @"不许!(士卒不满+{0})"},
      {"event_riot_title", "部队哗变"},
      {"event_riot", @"{0}将军所部{1}因军中积怨深厚，发生哗变，部队士气下降{2}！"},
      {"event_generalExecuted_title", "死亡"},
      {"event_generalExecuted", @"{0}将军被哗变士卒所杀！"},
      {"event_newGeneral_title", "新帅上任"},
      {"event_newGeneral", @"{0}将军任命为{1}新统帅"},
      {"event_generalReturned_title", "回朝治罪"},
      {"event_generalReturned", @"{0}将军因治军不力，已回朝治罪"},
      {"event_generalResigned_title", "下野"},
      {"event_generalResigned", @"{0}将军因治军不力，已被夺去官职下野"},
      {"event_generalKilled_title", "战死"},
      {"event_generalKilled", @"{0}将军战死于军中"},
      {"event_unitRetreat_title", "撤兵"},
      {"event_unitRetreat", @"{0}撤出战场"},
      {"event_epidemic_title", "爆发痢疾"},
      {"event_epidemic", @"{0}所部{1}因久驻因暴雨过后而蚊虫滋生的密林，大量士卒染上痢疾"},
      {"event_poision_title", "遭人投毒"},
      {"event_poision", @"{0}所部{1}因饮用了上游遭歹人投毒的河水，大量士卒染病"},

      // pop out msg
      {"pop_discontent", @"不满+{0}"},
      };
    }
  }
}