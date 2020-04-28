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
      {"u_concealing", "隐蔽中"},
      {"u_camping", "驻扎中"},
      {"u_disbanded", "被歼灭"},
      {"u_standing", ""},
      {"u_infantryName", @"{0}步兵"},
      {"u_cavalryName", @"{0}骑兵"},

      {"rank_hanInfantryRookie", "厢军"},
      {"rank_hanCavalryRookie", "游弋"},
      {"rank_hanInfantryVeteran", "禁军"},
      {"rank_hanCavalryVeteran", "大全装"},

      {"rank_qidanInfantryRookie", "部族军"},
      {"rank_qidanCavalryRookie", "打草谷"},
      {"rank_qidanInfantryVeteran", "皮室军"},
      {"rank_qidanCavalryVeteran", "斡鲁朵军"},

      {"rank_dangxiangInfantryRookie", "正军"},
      {"rank_dangxiangCavalryRookie", "擒生军"},
      {"rank_dangxiangInfantryVeteran", "侍卫军"},
      {"rank_dangxiangCavalryVeteran", "铁鹞子"},

      {"rank_nvzhenInfantryRookie", "部族军"},
      {"rank_nvzhenCavalryRookie", "部族马军"},
      {"rank_nvzhenInfantryVeteran", "合札"},
      {"rank_nvzhenCavalryVeteran", "铁浮图"},

      {"rank_tuboInfantryRookie", "奴从"},
      {"rank_tuboCavalryRookie", "索巴"},
      {"rank_tuboInfantryVeteran", "武士步军"},
      {"rank_tuboCavalryVeteran", "武士马军"},

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

      {"region_qidanRegion", "契丹族"},
      {"region_hanRegion", "汉族"},
      {"region_dangxiangRegion", "党项族"},
      {"region_tuboRegion", "吐蕃族"},
      {"region_nvZhenRegion", "女真族"},

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
      {"g_liubei", "李克用"},
      {"g_guanyu", "李存审"},
      {"g_zhaoyun", "李嗣昭"},
      {"g_zhangfei", "元行钦"},
      {"g_machao", "夏鲁奇"},
      {"g_y1", "周德威"},
      {"g_y2", "张承业"},
      {"g_y3", "李嗣源"},
      {"g_y4", "李存孝"},
      {"g_y5", "李存勖"},

      {"g_caocao", "朱温"},
      {"g_xuchu", "王彦章"},
      {"g_abc", "牛存节"},
      {"g_x1", "张全义"},
      {"g_x2", "杨师厚"},
      {"g_x3", "刘寻"},
      {"g_x4", "王景仁"},
      {"g_x5", "杨行密"},
      {"g_x6", "刘守光"},
      {"g_x7", "李茂贞"},
      {"g_x8", "柴再用"},
      {"g_x9", "朱瑾"},
      {"g_x10", "刘知俊"},

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
      {"event_generalKilled_title", "战死"},
      {"event_generalKilled", @"{0}将军战死于军中"},
      {"event_epidemic_title", "爆发痢疾"},
      {"event_epidemic", @"{0}所部{1}因久驻因暴雨过后而蚊虫滋生的密林，大量士卒染上痢疾"},
      {"event_emptySettlement_title", "无人守备"},
      {"event_emptySettlement", @"{0}并无敌军守备，占领还是焚毁?"},
      {"event_emptySettlement_occupy_title", "占领"},
      {"event_emptySettlement_burndown_title", "焚毁"},
      {"event_underSiege_title", "长围合拢"},
      {"event_underSiege", @"围城军队连日所筑长围现已合拢，{0}已经被完全包围，任何物资人员已无法出入"},
      {"event_unitConflict_title", "士卒斗殴"},
      {"event_unitConflict", @" {0}所部的{1}与临近驻扎的{2}发生冲突，有{3}人伤重不治，两部士兵士气下降{4}"},
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
      {"event_operationResult_weFeintDefeat", "佯败"},
      {"event_operationResult_weSmallWin", "小捷"},
      {"event_operationResult_weGreatWin", "大捷"},
      {"event_operationResult_weCrushingWin", "史诗大捷"},
      {"event_operationResult_theyCloseWin", "略占下风"},
      {"event_operationResult_theyFeintDefeat", "敌军佯败"},
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
      {"event_RefuseToRetreat", @"{0}: 食君之禄忠君之事，岂能遁逃!"},
      {"event_ChaseDialogue", @"{0}: 贼兵休走!"},
      {"event_FeintDefeat", @"{0}: 听我号令，全军后撤!"},
      {"event_SiegeComplain", @"{0}士兵: 我{0}勇士半日即刻拿下此城，何须筑此长围示弱于敌耶!"},
      {"event_InCampComplain", @"{0}士兵: 我{0}铁骑岂能龟缩于高墙之后任由贼人践踏我土地，掠杀我同胞，当以堂堂之阵临贼!"},
      {"event_OnFieldComplain", @"{0}士兵: 今贼势凶险，当凭城守坚以避贼锋，岂有野地浪战之理!"},
      {"event_RetreatStress", @"{0}士兵: 今我部兵士战死者半，将军为何还不撤兵，意欲置我辈于死地耶!"},

      // pop out msg
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
      {"pop_setFireFail", "放火失败"},
      {"pop_discovered", "被敌军发现!"},
      {"pop_noDamNearby", "需要靠近河堤"},
      {"pop_noCampNearby", "需要靠近城寨"},
      {"pop_notWaterbound", "需要靠近水源"},
      {"pop_noAllyNearby", "需要靠近友军"},
      {"pop_noSettlementNearby", "附近没有己方城寨"},
      {"pop_buildingStarted", "开始筑营!"},
      {"pop_buildingFailed", "无法筑营!"},
      {"pop_insufficientPoint", @"需要{0}体力"},
      {"pop_insufficientForce", @"需要{0}兵力"},
      {"pop_sieged", "围困工事已修筑"},
      {"pop_sieging", "修建长围"},
      {"pop_siegeBreak", "拆毁长围"},
      {"pop_buildFail", "无法建造"},
      {"pop_altitudeSickness", "高原反应"},
      {"pop_plainSickness", "平原反应"},
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
      {"pop_newCommander", "新主帅"},
      {"pop_failedToBreakthrough", "突围失败"},
      {"pop_breakthrough", "突围成功"},
      {"pop_toBreakThrough", "突围"},

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
      {"title_feintDefeat", "佯败"},
      {"title_noWayOut", "身陷重围"},
      {"title_deployment", "战前部署"},
      {"title_days", @"第{0}日 · {1}"},
      {"title_settlementTaken", "城破!"},

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

      {"ability_outOfOrder", "首尾失序"},
      {"ability_outOfOrder_description", "协同进攻时友军50%几率不参与行动"},
      {"ability_outOfControl", "不听调度"},
      {"ability_outOfControl_description", "所辖各部在战争开局会擅自出击"},
      {"ability_masterOfMist", "雾中指挥"},
      {"ability_masterOfMist_description", "即使是在大雾笼罩之下，仍能指挥部队组织协同防御"},
      {"ability_forecaster", "知天象"},
      {"ability_forecaster_description", "极大几率推测翌日的风向与气候"},
      {"ability_backStabber", "因粮于敌"},
      {"ability_backStabber_description", "深入敌军控制区域，大幅减少周围友军因补给不济影响"},
      {"ability_mustObey", "如山军令"},
      {"ability_mustObey_description", "指挥范围内所有友军参与作战几率100%"},
      {"ability_turningTheTide", "力挽狂澜"},
      {"ability_turningTheTide_description", "破阵时，50%几率阻止指挥范围内友军溃败"},
      {"ability_discipline", "治军有方"},
      {"ability_discipline_description", "一定几率减少部队各种不满发生"},
      {"ability_pursuer", "乘胜追击"},
      {"ability_pursuer_description", "统帅骑兵增加对敌方溃败单位追击杀伤率和士气削减"},
      {"ability_hammer", "气吞万里"},
      {"ability_hammer_description", "统帅骑兵部队时冲阵成功率加50%"},
      {"ability_builder", "鲁班再世"},
      {"ability_builder_description", "筑长围时间减半"},
      {"ability_breacher", "先登"},
      {"ability_breacher_description", "攻城时所部兵士战斗力增加50%"},
      {"ability_noPanic", "有序撤退"},
      {"ability_noPanic_description", "被击退时减少部队崩溃或者发生溃败几率50%"},
      {"ability_mechanician", "城防要义"},
      {"ability_mechanician_description", "守城时增加本部兵马战力50%"},
      {"ability_diminisher", "破城之法"},
      {"ability_diminisher_description", "当统帅步兵围城时城防能力每回合衰减速度加倍"},
      {"ability_staminaManager", "洪荒之力"},
      {"ability_staminaManager_description", "部队能发起两次进攻"},
      {"ability_holdTheGround", "纹丝不动"},
      {"ability_holdTheGround_description", "面对敌军冲阵时减少冲阵成功率50%"},
      {"ability_breaker", "气势如虹"},
      {"ability_breaker_description", "所部冲阵成功时50%几率对临近敌军产生连锁反应"},
      {"ability_unshaken", "寸步不让"},
      {"ability_unshaken_description", "部队战败时75%几率不会退败"},
      {"ability_formidable", "骁勇善战"},
      {"ability_formidable_description", "所部战力+25%,当所部被完全包围时战斗力提升3倍"},
      {"ability_attender", "永不缺席"},
      {"ability_attender_description", "100%几率参加协同作战"},
      {"ability_feintDefeat", "佯败"},
      {"ability_feintDefeat_description", "80%几率发动佯败, 撤退时增加敌军追击几率"},
      {"ability_playSafe", "步步为营"},
      {"ability_playSafe_description", "敌方战败时本部兵马不会擅自追击"},
      {"ability_punchThrough", "誓死一搏"},
      {"ability_punchThrough_description", "部队突围成功率提升50%"},
      {"ability_generous", "千金散尽"},
      {"ability_generous_description", "进入战场部队士气+20"},
      {"ability_runner", "神行太保"},
      {"ability_runner_description", "部队移动力提升50%"},
      {"ability_fireBug", "祝融之名"},
      {"ability_fireBug_description", "放火成功率100%"},

      {"trait_reckless", "鲁莽"},
      {"trait_reckless_description", "敌军退败容易主动追击"},
      {"trait_brave", "英勇"},
      {"trait_brave_description", "战败时容易战死,不易受突袭影响"},
      {"trait_loyal", "忠义"},
      {"trait_loyal_description", "容易拒绝撤退指令"},
      {"trait_conservative", "保守"},
      {"trait_conservative_description", "己方战败时容易主动撤退,不易受突袭影响"},
      {"trait_cunning", "精明"},
      {"trait_cunning_description", "己方处于劣势时容易按兵不动,不易战死"},
      {"trait_ego", "自负"},
      {"trait_ego_description", "容易受敌方激将"},
      };
    }
  }
}