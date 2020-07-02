Game Tips:
1. 酷暑行军，一定几率增加部队不满
2. 菜鸟部队酷暑在野外50%几率发生脱帽事件
3. 暴雨(骑兵)，大雪，小雪，移动力至少降低50%
4. 夏季，下雨天于河东河北密林中，部队一定几率染上痢疾
5. 20回合后，战争泥潭，攻击部队每回合士气-4，防御部队每两回合士气-4
6. 士气低于惩罚阈值，等级加成消失，低于撤退阈值战斗力下降8成
7. 部队城内战斗力加成基于城防点数
8. 骑兵在山地战斗力下降20%, 步兵防御力上升20%
9. 步兵骑兵在高地战斗力上升100%
10. 疾病状态下下降6成
11. 敌方控制范围移动需要额外移动力
12. 非高原的骑步兵在高原行军到疲惫以及精疲力竭状态时一定几率出现高原反应, 只出现一次，发生时部队伤员增加移动力归0数回合
13. 高原地区骑兵在平原低地一定几率发生平原反应，只出现一次，发生时部队移动力和攻击力战斗期间永久下降20%
14. 攻击方部队，顺风方向战斗力+50%，逆风方向战斗力-50%, 顺风增加破阵几率, 顺风方向放火必定成功
15. 战斗爆发地，战斗结束如果死亡人数超过500，该地块标记为尸体区，停留尸体区，部队80%几率染上疾病，前去掩埋尸体，掩埋部队不满+2，如果超过3回合不掩埋，尸体消失，计入战场瘟疫爆发点数
16. 被完全包围，特殊技能武将城中战力*1.5 城外*3
17. 重骑兵战斗力在等级加成基础上再*0.2f
18. 如果主帅部队战败，周围部队士气下降8 - 30
19. 部队在山地上被冲阵几率略低
22. 女真人大概率不撤退
24. 女真人所有纪律事件免疫
25. 汉：步兵守方, 敌我战力比大于2倍时50%野战抱怨
26. 契丹:步兵围城50%抱怨, 骑兵防御时守城75%抱怨
29. 党项：步兵围城50%抱怨
30. 党项人：不受高原反应影响
31. 吐蕃人：长于后勤，战斗力偏弱，做偏师
32. 不同种族部队袖手旁观20%几率
33. 主动发起进攻和进攻目标方死的多
34. 减员超过1/2 75%几率抗议不撤退，士气-10
35. 同一回合内连续战败 士气累计-5, -10, -15, -20
36. 部队战败状态战力n * -10%, n为累计战败次数
37. 部队获胜移动力+40
38. 士气为0部队追击被亡
39. 两翼没有防护受到防御惩罚, 将领formidable和在高地上例外
40. 部队四面有守备防御力+200%
41. 小雨部队攻击力-20%， 暴雨-40%， 酷暑-20%, 小雪-20%, 暴雪-60%
42. 利用大雾攻击对方指挥范围外部队
43. 两次冲阵失败，对方冲阵免疫
44. 地域冲突换成挑拨技能，任何来自不同行省军队都能挑拨，来自不同民族军队挑拨几率极高，成功两军发生冲突后撤
45. 主帅至少两翼有友军，否则冲阵走一只，容易被四面夹击
46. overcharged metality incr 10% attack point
47. mental weak affects join possibility(-10), easy to surprise, easy to charge, easy to scatter, easy to retreat on defeat 
48. chaotic 60%几率 & defeating 40%几率变waving, discpline技能直接变normal
49. 移动范围，步兵4格 重骑兵5格 轻骑兵6格，轻骑兵行动后获得100%移动力, 其他兵种行动后获得50%移动力
abcdefghijklmnopqrstuvwxyz

morale punish line: 战斗力减半
奇袭多人埋伏只能一人触发
字体显示问题
敌军将领范围不显示，直接交战80%几率发现敌军主帅
全局ai和集团军ai两套
ai撤退条件（敌方主帅性格）：
鲁莽英勇死战不退
主帅阵亡，歼灭，丧失战斗力
核心部队2只以上丧失战斗力
超过1/3部队丧失战斗力
make attacker drag long to expose the openning
女真吐蕃限制招募数量为4
大坝对进攻方不可见
观天象只对玩家有效，ai不会利用气候
outlooker 发现敌军主帅
狭隘
破阵 -> 以少打多 -> 击败 -> 追击 -> 歼灭 -> 影响周边军队
兵团动态合并
map skeleton for AI(supply route, point, value tiles, dam, supply range etc.)
每月一回合，10个战斗回合

战场瘟疫爆发时，每只部队随机确定时候染病，染病几率由低到高在到低变化
战略回合，ai借助战斗回合战果施压，换取停战条约
ai和玩家战术，偏师一个方向吸引敌军主力，另一路伏兵从后方伏击后队敌军
AI ambush doesn't need to check players alert status, only check if ambusher is in player's visible field before attack
use as much configuration as possible for Mod development, and think about flexibility

in Attached script to direct access gameobject:
 public void Destroy()
  {
    GameObject.Destroy(gameObject);
  }

convert Array to Hashset:
HashSet<Tile> road = new HashSet<Tile>(baseTile.roads[tile]);

Find gameobject by name:
 GameObject.Find("LeftPart").gameObject;

use "System.Linq" to let Array has Contains method
delete meta and .DS_Store file to avoid project syntax error in IDE