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
      {"u_routing", "溃败中"},
      {"u_disbanded", "被歼灭"},
      {"u_standing", ""},
      {"u_satisfied", @"士卒不满度: {0}/10"},
      {"u_unsatisfied", @"士卒不满度: {0}/10"},
      {"u_riot", @"不满度: {0}/10"},
      {"rank_rookie", "厢兵"},
      {"rank_veteran", "牙兵"},
      {"rank_elite", "禁军"},
      {"rank_scout", "斥候"},

      // region
      {"r_riverRun", "河间"},
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
		  {"l_longwei1", "神武"},
      {"l_longshen1", "殿前"},
      {"l_huben", "虎贲"},
      {"l_qingshen", "擒生"},
      {"l_huben1", "铁林"},
      {"l_qingshen1", "浮屠"},
		  {"l_longwei5", "神龙"},
      {"l_longshen5", "龙神"},
		  {"l_longwei6", "虎威"},
      {"l_longshen6", "虎武"},
      {"l_huben6", "期门"},
      {"l_qingshen6", "捉生"},

      // faction
      {"f_liang", "梁"},
      {"f_hejian", "河间"},

      // party
      {"p_eagleParty", "东林党"},
      {"p_pigeonParty", "内戚党"},
      {"p_yanParty", "阉党"},
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

      // party report
      {"pr_sloppyOnDrill", "疏于操练"},
      {"pr_sloppyOnDrill_d", "General is sloppy on drill, which leads the shameful defeat"},

      // building
      {"b_ownedCamp", "所部所筑营寨"},

      // event
      {"event_fireDestroyCamp_title", "营寨焚毁"},
      {"event_fireDestroyCamp", @"{0}处营寨被烈火焚毁，营中{1}民夫被烧死，{2}石粮草也被悉数焚毁！"},
      {"event_burningCampDestroyUnit_title", "军营大火"},
      {"event_burningCampDestroyUnit", @"{0}将军所部{1}遭遇军营大火，军中{2}官兵民夫全数丧生火海！"},
      {"event_floodDestroyCamp_title", "水淹七军"},
      {"event_floodDestroyCamp", @"我军{0}突发水情，营寨被洪水淹没，营中{1}民夫丧命于洪水，{2}石粮草被冲走, 营中守军亦全数溺亡，损失极为惨重！"},
      {"event_floodDestroyUnit_title", "无人生还"},
      {"event_floodDestroyUnit", @"{0}将军所部{1}突遇洪水，军中{2}官兵民夫全数殒命！"},
      {"event_enemyCaptureCamp_title", "敌军占领营寨"},
      {"event_enemyCaptureCamp", @"{0}处营寨已落入敌军之手, 营寨中{1}民夫为敌军所杀"},
      {"event_enemyCaptureCity_title", "敌军占领城市"},
      {"event_enemyCaptureCity", @"{0}已落入敌军之手, 城中{1}成年男子,{2}妇女和{3}孩童死于战乱之中"},
      {"event_weCaptureCamp_title", "我军占领营寨"},
      {"event_weCaptureCamp", @"{0}处营寨已落入我军之手, 击杀营寨中{1}敌军担负"},
      {"event_weCaptureCity_title", "我军占领城市"},
      {"event_weCaptureCity", @"我军已占领{0}, 城中{1}成年男子,{2}妇女和{3}孩童死于战乱之中"},
      {"event_weBurnCamp_title", "我军摧毁营寨"},
      {"event_weBurnCamp", @"{0}处营寨已被我军摧毁"},
      {"event_enemyBurnCamp_title", "敌军摧毁营寨"},
      {"event_enemyBurnCamp", @"{0}处营寨已被敌军摧毁"},
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
      {"event_supplyIntercepted_title", "伏击成功"},
      {"event_supplyIntercepted", @"{1}所部斥候伏击了往{0}方向的运粮队，{2}石粮草被毁, {3}名押运民夫被杀，余者已逃回始发地，{1}所部战死{6}人，伤{5}人，士气上升{4}点"},
      {"event_laborIntercepted_title", "伏击成功"},
      {"event_laborIntercepted", @"{1}所部斥候伏击了往{0}方向增援的民夫队伍，{2}名民夫被杀，余者已逃回始发地，{1}所部战死{5}人，伤{4}人，士气上升{3}点"},
      {"event_interceptFailed_title", "伏击失败"},
      {"event_interceptFailed", @"{0}所部斥候伏击了路过的运粮队，被护卫士兵击退，{0}所部战死{3}人，伤{2}人，士气下降{1}点"},
      {"event_supplyRouteBlocked_title", "补给线阻断"},
      {"event_supplyRouteBlocked", @"我军{0}与{1}之间的所有补给线遭到敌军切断，请立即派兵驱逐补给线上敌军以恢复两地交通补给！"},
      {"event_disarmor_title", "酷热难当"},
      {"event_disarmor", @"酷暑连日，{1}士兵终日顶盔贯甲行军备战已致多人中暑晕厥，军中抱怨者甚，有士卒恳求只着毡帽上身免披铁甲以解暑热，遭{0}厉斥，部队不满+{2}"},
      {"event_disarmor1", @"酷暑连日，{1}士兵终日顶盔贯甲行军备战已致多人中暑晕厥，军中抱怨者甚，有士卒恳求只着毡帽上身免披铁甲以解暑热，{0}许之，部队防御下降{2}%"},
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
      {"event_farmDestroyed_title", "参本!"},
      {"event_farmDestroyed", @"御史接来自战区参本, 所述近日{0}将军所部士兵行军中毁坏了大量百姓稻田，朝中甚为不满，{1}朝中影响力下降{2}点，士卒亦颇多怨言"},
      {"event_emptySettlement_title", "无人守备"},
      {"event_emptySettlement", @"{0}并无敌军守备，占领还是焚毁?"},
      {"event_emptySettlement_occupy_title", "占领"},
      {"event_emptySettlement_burndown_title", "焚毁"},
      {"event_underSiege_title", "长围合拢"},
      {"event_underSiege", @"围城军队连日所筑长围现已合拢，{0}已经被完全包围，任何物资人员已无法出入"},
      {"event_unitConflict_title", "士卒斗殴"},
      {"event_unitConflict", @"由于朝中两派关系紧张, {0}所部与临近驻扎的{1}部发生冲突，{0}所部有{3}人受伤{4}人伤重不治，{1}所部则是{5}人受伤{6}人死亡，两部士兵不满+{2}"},
      {"event_altitudeSickness_title", "高原反应"},
      {"event_altitudeSickness", @"由于过度疲劳, {0}所部部分士卒出现高原反应，士卒颇多不满，部队不得不就地休整"},
      {"event_plainSickness_title", "战马出现平原反应"},
      {"event_plainSickness", @"由于{0}所部骑兵多来自于高原，部队大部分战马出现了平原气候不适，这将极大影响部队的机动力和冲击力"},
      {"event_attackerSide_title", "进攻方"},
      {"event_defenderSide_title", "防御方"},
      {"event_ourSide_title", "我方"},
      {"event_aiSide_title", "敌方"},
      {"event_operationDetail_gen", "参战将领"},
      {"event_operationDetail_unit", "作战序列"},
      {"event_operationDetail_inf", "步兵数量"},
      {"event_operationDetail_cav", "骑兵数量"},
      {"event_operationDetail_total", "总战力"},
      {"event_possibleVictRate_0", "本方必败无疑"},
      {"event_possibleVictRate_1", "本方有一成胜算"},
      {"event_possibleVictRate_2", "本方有两成胜算"},
      {"event_possibleVictRate_3", "本方有三成胜算"},
      {"event_possibleVictRate_4", "本方有四成胜算"},
      {"event_possibleVictRate_5", "本方有五成胜算"},
      {"event_possibleVictRate_6", "本方有六成胜算"},
      {"event_possibleVictRate_7", "本方有七成胜算"},
      {"event_possibleVictRate_8", "本方有八成胜算"},
      {"event_possibleVictRate_9", "本方有九成胜算"},
      {"event_possibleVictRate_10", "本方必胜无疑"},
      {"event_confirmAttack", "确定要进攻吗?"},

      // pop out msg
      {"pop_discontent", @"不满+{0}"},
      {"pop_builded", "建造完成!"},
      {"pop_failedToSupplyUnitInSettlement", @"无法补给{0}所部"},
      {"pop_failedToSupplyUnitNearby", "据点补给失败"},
      {"pop_failedToDistSupply", "无法转运粮草"},
      {"pop_failedToDistLabor", "无法转移民夫"},
      {"pop_starving", "部队饥饿!"},
      {"pop_halfStarving", "部队半饥饿!"},
      {"pop_damBroken", "决堤!"},
      {"pop_morale", "士气"},
      {"pop_movement", "移动力"},
      {"pop_wounded", "负伤"},
      {"pop_killed", "阵亡"},
      {"pop_laborKilled", "民夫死亡"},
      {"pop_desserter", "逃亡"},
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
      {"pop_insufficientLabor", @"需要{0}民夫"},
      {"pop_insufficientPoint", @"需要{0}体力"},
      {"pop_sieged", "围困工事已修筑"},
      {"pop_farmDestroyed", "农田被毁"},
      {"pop_altitudeSickness", "高原反应"},
      {"pop_plainSickness", "战马平原反应"},

      // settlement
      {"settlement_storageLvl1", "初级粮仓"},
      {"settlement_storageLvl2", "中级粮仓"},
      {"settlement_storageLvl3", "高级粮仓"},
      {"settlement_wallLvl1", "初级城墙"},
      {"settlement_wallLvl2", "中级城墙"},
      {"settlement_wallLvl3", "高级城墙"},
      {"settlement_strategyBase", @"{0}军大本营"},
      
      {"title_wargame_commiting", "执行作战计划"},
      {"title_wargame_committed", "执行完毕"},

      {"operation_success_chance", @"胜算{0}成"},
      {"misc_windAdvantage", "背风面"},
      {"misc_windDisadvantage", "迎风面"},

      };
    }
  }
}