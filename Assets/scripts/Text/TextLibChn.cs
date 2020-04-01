﻿using System.Collections.Generic;

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
      {"w_mist", "大雾"},

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
      {"d_dueNorth", "正北 ↼"},
      {"d_dueSouth", "正南 ⇀"},
      {"d_northEast", "东北 ↖"},
      {"d_northWest", "西北 ↙"},
      {"d_southEast", "东南 ↗"},
      {"d_southWest", "西南 ↘"},

  //   unit
  	  {"u_exhausted", "精疲力竭"},
  	  {"u_tired", "人困马乏"},
  	  {"u_fresh", "充沛"},
      {"u_concealing", "隐蔽中"},
      {"u_camping", "驻扎中"},
      {"u_disbanded", "被歼灭"},
      {"u_standing", ""},
      {"u_satisfied", @"士卒不满度: {0}/10"},
      {"u_unsatisfied", @"士卒不满度: {0}/10"},
      {"u_riot", @"不满度: {0}/10"},
      {"u_infantryName", @"{0}步兵"},
      {"u_cavalryName", @"{0}骑兵"},
      {"rank_rookie", "厢兵"},
      {"rank_veteran", "禁军"},
      {"rank_lightCav", "轻骑"},
      {"rank_heavyCav", "重骑"},

      // region
      {"r_heHuang", "河湟"},
      {"r_heXi", "河西"},
      {"r_heDong", "河东"},
      {"r_heBei", "河北"},
      {"r_heNan", "河南"},
      {"r_shanXi", "陕西"},
      {"r_xiJing", "西京"},
      {"r_zhongJing", "中京"},
      {"r_shangJing", "上京"},

      {"region_qidanRegion", "契丹人"},
      {"region_hanRegion", "汉人"},
      {"region_dangxiangRegion", "党项人"},
      {"region_tuboRegion", "吐蕃人"},

      // faction
      {"f_liao", "辽"},
      {"f_song", "宋"},
      {"f_xia", "夏"},

      // party
      {"p_newParty", "新党"},
      {"p_oldParty", "旧党"},
      {"p_northCourt", "北院"},
      {"p_southCourt", "南院"},
      {"p_noParty", ""},
      {"party_relationNormal", "缓和"},
      {"party_relationTense", "紧张"},
      {"party_relationXtense", "一触即发"},

      // generals
      {"g_liubei", "李存勖"},
      {"g_guanyu", "李存审"},
      {"g_zhaoyun", "李嗣昭"},
      {"g_zhangfei", "元行钦"},
      {"g_caocao", "朱温"},
      {"g_xuchu", "王彦章"},
      {"g_machao", "夏鲁奇"},
      {"g_abc", "牛存节"},

      // event
      {"event_disbandDestroyUnit_title", "全军覆没"},
      {"event_disbandDestroyUnit", @"{0}所部{1}全军覆没！"},
      {"event_wildfireDestroyUnit_title", "无人生还"},
      {"event_wildfireDestroyUnit", @"{0}将军所部{1}突遇山火，军中{2}官兵全数殒命！"},
      {"event_floodDestroyUnit_title", "无人生还"},
      {"event_floodDestroyUnit", @"{0}将军所部{1}突遇洪水，军中{2}官兵全数殒命！"},
      {"event_enemyCaptureCamp_title", "敌军占领营寨"},
      {"event_enemyCaptureCamp", @"{0}处营寨已落入敌军之手"},
      {"event_enemyCaptureCity_title", "敌军占领城市"},
      {"event_enemyCaptureCity", @"{0}已落入敌军之手, 城中{1}成年男子,{2}妇女和{3}孩童死于战乱之中"},
      {"event_weCaptureCamp_title", "我军占领营寨"},
      {"event_weCaptureCamp", @"{0}处营寨已落入我军之手"},
      {"event_weCaptureCity_title", "我军占领城市"},
      {"event_weCaptureCity", @"我军已占领{0}, 城中{1}成年男子,{2}妇女和{3}孩童死于战乱之中"},
      {"event_riot_title", "部队哗变"},
      {"event_riot", @"{0}将军所部{1}因军中积怨深厚，发生哗变，部队士气下降{2}！"},
      {"event_generalKilled_title", "战死"},
      {"event_generalKilled", @"{0}将军战死于军中"},
      {"event_epidemic_title", "爆发痢疾"},
      {"event_epidemic", @"{0}所部{1}因久驻因暴雨过后而蚊虫滋生的密林，大量士卒染上痢疾"},
      {"event_poision_title", "遭人投毒"},
      {"event_poision", @"{0}所部{1}因饮用了上游遭歹人投毒的河水，大量士卒染病"},
      {"event_farmDestroyed_title", "参本!"},
      {"event_farmDestroyed", @"御史接来自战区参本, 所述近日{0}将军所部士兵行军中毁坏了大量百姓稻田，朝中甚为不满，{1}朝中影响力下降{2}点，士卒亦颇多怨言"},
      {"event_emptySettlement_title", "无人守备"},
      {"event_emptySettlement", @"{0}并无敌军守备，占领还是焚毁?"},
      {"event_emptySettlement_occupy_title", "占领"},
      {"event_emptySettlement_burndown_title", "焚毁"},
      {"event_underSiege_title", "长围合拢"},
      {"event_underSiege", @"围城军队连日所筑长围现已合拢，{0}已经被完全包围，任何物资人员已无法出入"},
      {"event_unitConflict_title", "士卒斗殴"},
      {"event_unitConflict", @"由于朝中两派关系紧张, {0}所部与临近驻扎的{1}部发生冲突，有{2}人伤重不治，两部士兵不满+{3}"},
      {"event_altitudeSickness_title", "高原反应"},
      {"event_altitudeSickness", @"由于过度疲劳, {0}所部部分士卒出现高原反应，士卒颇多不满，部队不得不就地休整"},
      {"event_plainSickness_title", "战马出现平原反应"},
      {"event_plainSickness", @"由于{0}所部骑兵多来自于高原，部队大部分战马出现了平原气候不适，这将极大影响部队的机动力和冲击力"},
      {"event_attackerSide_title", "进攻方"},
      {"event_defenderSide_title", "防御方"},
      {"event_ourSide_title", "我方"},
      {"event_aiSide_title", "敌方"},
      {"event_operationDetail_gen", "参战将领"},
      {"event_operationDetail_inf", "步兵数量"},
      {"event_operationDetail_cav", "骑兵数量"},
      {"event_operationDetail_total", "总战力"},
      {"event_possibleVictRate_0", "我军必败无疑"},
      {"event_possibleVictRate_1", "我军有一成胜算"},
      {"event_possibleVictRate_2", "我军有两成胜算"},
      {"event_possibleVictRate_3", "我军有三成胜算"},
      {"event_possibleVictRate_4", "我军有四成胜算"},
      {"event_possibleVictRate_5", "我军有五成胜算"},
      {"event_possibleVictRate_6", "我军有六成胜算"},
      {"event_possibleVictRate_7", "我军有七成胜算"},
      {"event_possibleVictRate_8", "我军有八成胜算"},
      {"event_possibleVictRate_9", "我军有九成胜算"},
      {"event_possibleVictRate_10", "我军必胜无疑"},
      {"event_confirmAttack", "确定要进攻吗?"},
      {"event_operationResult_infDead", "步兵战死数"},
      {"event_operationResult_infTotal", "步兵参战数"},
      {"event_operationResult_cavDead", "骑兵战死数"},
      {"event_operationResult_cavTotal", "骑兵参战数"},
      {"event_operationResult_total", "总计战死数"},
      {"event_operationResult_horse", "战马俘获数"},
      {"event_operationResult_weCloseWin", "略占上风"},
      {"event_operationResult_weSmallWin", "小捷"},
      {"event_operationResult_weGreatWin", "大捷"},
      {"event_operationResult_weCrushingWin", "史诗大捷"},
      {"event_operationResult_theyCloseWin", "略占下风"},
      {"event_operationResult_theySmallWin", "小败"},
      {"event_operationResult_theyGreatWin", "大败"},
      {"event_operationResult_theyCrushingWin", "惨败"},
      {"event_routingRetreatSoldierDialog", @"{0}所部溃兵: 前方猪狗, 勿挡我道啊!!!"},
      {"event_routingImpactSoldierDialog", @"{0}所部士兵: 诸君镇定, 切莫再退了!!!"},
      {"event_NoRetreatGeneralDialog", @"{0}: 我军已无退路, 吾辈自当向死而生!!!"},
      {"event_NoRetreatSoldierDialog", @"{0}所部众兵士: 誓死不降!!!"},
      {"event_RemoveHelmetSoldierDialog", @"{0}所部兵士: 夏日炎炎，我军终日披挂铁盔铁甲行军备战，行伍之中已有多人中暑晕厥，军中抱怨者甚，恳请将军准许军中将士只着毡帽免戴铁胄以解暑热!"},
      {"event_RemoveHelmetSoldierDialogFollow", @"{0}所部兵士: 酷热难耐，军中兵士恳请将军效法{1}将军，准许军中将士只着毡帽免戴铁胄以解暑热!"},
      {"event_RemoveAllowedGeneralDialog", @"{0}: 此事关军心，告诸将士，我许卿等以便宜行事!"},
      {"event_RemoveDisallowedGeneralDialog", @"{0}: 盔者乃防具之首, 岂可轻易取走耶? 有再提此事者，我必斫掉其狗头使其再无顶盔之苦恼!!"},



      {"event_FormationBreaking", @"{0}: 尔等大势已去，何不速速来降! 解甲投矛者不杀!"},
      {"event_FormationBreaking1", @"{0}: 什么土鸡瓦狗，不过尔尔!"},
      {"event_FormationBreaking_songVliao1", @"{0}: 先取幽云十六州，再分子将打衙头!"},
      {"event_FormationBreaking_song1", @"{0}: 天威卷地过黄河，万里胡人尽汉歌!"},
      {"event_FormationBreaking_song2", @"{0}: 旗队浑如锦绣堆，银装背嵬打回回!"},
      {"event_FormationBreaking_song3", @"{0}: 看我金戈铁马，气吞万里如虎!"},
      {"event_FormationBreaking_songVxia", @"{0}: 汉家儿郎十万强，打下银州坐五凉!"},
      {"event_FormationBreaking_liaoVxia", @"{0}: 番汉精锐十万强，打下银州坐五凉!"},
      {"event_FormationBreaking_xia", @"{0}: 大宋何曾耸，大辽亦无奇。 满川龙虎辈，犹自说兵机!"},
      {"event_Retreat", @"{0}: 胜败乃兵家常事，他日再战!"},

      // pop out msg
      {"pop_discontent", @"不满+{0}"},
      {"pop_content", @"不满-{0}"},
      {"pop_builded", "建造完成!"},
      {"pop_starving", "后勤不济!"},
      {"pop_damBroken", "决堤!"},
      {"pop_morale", "士气"},
      {"pop_movement", "移动力"},
      {"pop_killed", "阵亡"},
      {"pop_atk", "战斗力"},
      {"pop_def", "防御力"},
      {"pop_routing", "溃败"},
      {"pop_warWeary", "厌战"},
      {"pop_sickness", "痢疾"},
      {"pop_poisioned", "中毒"},
      {"pop_setFire", "引燃"},
      {"pop_discovered", "被敌军发现!"},
      {"pop_noDamNearby", "需要靠近河堤"},
      {"pop_noCampNearby", "需要靠近城寨"},
      {"pop_notWaterbound", "需要靠近水源"},
      {"pop_noAllyNearby", "需要靠近友军"},
      {"pop_noSettlementNearby", "附近没有己方城寨"},
      {"pop_transferDone", "转运完成!"},
      {"pop_transferIssued", "转运命令已下发!"},
      {"pop_buildingStarted", "开始筑营!"},
      {"pop_buildingFailed", "无法筑营!"},
      {"pop_poisionDone", "投毒成功!"},
      {"pop_insufficientPoint", @"需要{0}体力"},
      {"pop_insufficientForce", @"需要{0}兵力"},
      {"pop_sieged", "围困工事已修筑"},
      {"pop_sieging", "修建长围"},
      {"pop_siegeBreak", "拆毁长围"},
      {"pop_buildFail", "无法建造"},
      {"pop_farmDestroyed", "农田被毁"},
      {"pop_altitudeSickness", "高原反应"},
      {"pop_plainSickness", "战马平原反应"},
      {"pop_notJoinOperation", "按兵不动"},
      {"pop_joinOperation", "投入战斗"},
      {"pop_chaos", "一溃千里"},
      {"pop_escapeNoRout", "无路可退"},
      {"pop_crashedByAlly", "受友军冲击"},
      {"pop_noRetreatBuf", "战力翻番！"},
      {"pop_retreat", "撤退!"},
      {"pop_charging", "冲阵!"},
      {"pop_chasing", "乘胜追击!"},
      {"pop_shaked", "阵脚松动!"},
      {"pop_holding", "稳住阵脚"},

      // settlement
      {"settlement_storageLvl1", "初级粮仓"},
      {"settlement_storageLvl2", "中级粮仓"},
      {"settlement_storageLvl3", "高级粮仓"},
      {"settlement_storageLvl4", "顶级粮仓"},
      {"settlement_wallLvl1", "初级城墙"},
      {"settlement_wallLvl2", "中级城墙"},
      {"settlement_wallLvl3", "高级城墙"},
      {"settlement_strategyBase", @"{0}军大本营"},
      
      {"title_wargame_commiting", "执行作战计划"},
      {"title_wargame_committed", "执行完毕"},
      {"title_formationBreaking", "破阵!"},
      {"title_formationCrushing", "一溃千里"},
      {"title_deployment", "战前部署"},
      {"title_days", @"第{0}日 · {1}"},

      {"operation_success_chance", @"胜算{0}成"},
      {"misc_windAdvantage", "背风面"},
      {"misc_windDisadvantage", "迎风面"},
      {"misc_hundredThousand", "万"},
      {"misc_thousand", "千"},
      {"misc_hundred", "百"},
      {"misc_defenceForce", @"{0}守军"},

      {"weather_galeReminder", "风劲角弓鸣"},
      {"weather_heatReminder", "禾木半枯焦"},
      {"weather_blizardReminder", "铁衣冷难着"},
      {"weather_heavyRainReminder", "电尾烧黑云"},
      {"weather_mistReminder", "厚雾结长空"},
      {"weather_citySieged", "黑云压城城欲摧"},

      };
    }
  }
}