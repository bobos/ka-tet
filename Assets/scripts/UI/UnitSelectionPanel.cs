using UnityEngine;
using UnityEngine.UI;
using UnitNS;
using FieldNS;
using System.Collections.Generic;
using MapTileNS;

namespace MonoNS
{

  public class UnitSelectionPanel : BaseController
  {

    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      mouseController = hexMap.mouseController;
      actionController = hexMap.actionController;
      turnController = hexMap.turnController;
      actionController.actionDone += ActionDone;
      actionController.onBtnClick += OnBtnClick;
      turnController.onEndTurnClicked += OnEndTurnClicked;
      turnController.onNewTurn += OnNewTurn;
      GameObject[] btns = {MoveButton, AttackButton, DefendButton, CampButton,
                           SabotageButton, FireButton, SiegeButton, EncampButton,
                           RetreatButton, TransferSupplyButton, TransferLaborButton,
                           DecampButton, ReposButton, BuryButton, ChargeButton
                           };
      buttons = btns;
      mouseController.onUnitSelect += OnUnitSelect;
      mouseController.onUnitPreflight += OnUnitSelect;
      mouseController.onUnitDeselect += OnUnitDeselect;
      mouseController.onModeQuit += OnModeQuit;
      hexMap.eventDialog.eventDialogOn += EventDialogOn;
      hexMap.eventDialog.eventDialogOff += EventDialogOff;
      self = this.transform.gameObject;
      self.SetActive(false);
      DeploymentDoneButton.SetActive(!hexMap.deployDone);
    }

    MouseController mouseController;
    ActionController actionController;
    TurnController turnController;
    GameObject self;
    public GameObject MoveButton;
    public GameObject AttackButton;
    public GameObject DefendButton; // Poison
    public GameObject CampButton;
    public GameObject SabotageButton;
    public GameObject NextTurnButton;
    public GameObject DeploymentDoneButton;
    public GameObject FireButton;
    public GameObject SiegeButton; // Siege
    public GameObject EncampButton;
    public GameObject RetreatButton;
    public GameObject TransferSupplyButton;
    public GameObject TransferLaborButton;
    public GameObject DecampButton;
    public GameObject ReposButton;
    public GameObject BuryButton;
    public GameObject ChargeButton;
    GameObject[] buttons;

    public Text title;
    public Text movement;
    public Text slots;
    public Text num;
    public Text morale;
    public Text offense;
    public Text defense;
    public Text stamina;
    public Text state;
    public Text illness;
    public Text poision;

    public void OnEndTurnClicked()
    {
      NextTurnButton.SetActive(false);
    }

    public void OnNewTurn()
    {
      if (hexMap.deployDone) {
        NextTurnButton.SetActive(true);
      }
    }

    public void EventDialogOn() {
      if (self.activeSelf) {
        ToggleButtons(false, mouseController.selectedUnit);
      }
    }

    public void EventDialogOff() {
      if (self.activeSelf) {
        ToggleButtons(true, mouseController.selectedUnit);
      }
    }

    public void ActionDone(Unit unit, Unit[] units, ActionController.actionName name)
    {
      if ((unit != null && !unit.IsAI() && name == ActionController.actionName.MOVE)
        || (unit != null && unit.IsAI() && name == ActionController.actionName.ATTACK))
      {
        RefreshButtons(unit, false);
      }
    }

    void RefreshButtons(Unit unit, bool isGarrison)
    {
      ToggleButtons(!unit.TurnDone(), unit, isGarrison);
    }

    public bool tileSelecting = false;
    public void Decamp() {
      tileSelecting = true;
      hexMap.msgBox.Show("选择部署地点");
      hexMap.HighlightArea(deployableTiles.ToArray(), HexMap.RangeType.camp);
    }

    public void CancelGarrison(Unit unit) {
      hexMap.msgBox.Show("");
      if (tileSelecting) {
        tileSelecting = false;
        hexMap.DehighlightArea();
        deployableTiles = null;
      }
      OnUnitDeselect(unit);
    }

    public bool isSelectableTile(Tile tile) {
      foreach(Tile t in deployableTiles) {
        if (Util.eq<Tile>(t, tile)) {
          return true;
        }
      }
      return false;
    }

    List<Tile> deployableTiles;
    public void ToggleButtons(bool state, Unit unit, bool isGarrison = false)
    {
      foreach (GameObject button in buttons)
      {
        button.SetActive(false);
      }

      // TODO: uncomment
      //if (!state || hexMap.combatController.start || unit.IsAI()) return;
      if (!state || hexMap.combatController.start) return;

      if (isGarrison) {
        bool hasEnemy = false;
        deployableTiles = new List<Tile>();
        foreach(Tile tile in mouseController.selectedSettlement.baseTile.neighbours) {
          Unit u = tile.GetUnit();
          if (u != null && u.IsAI() != unit.IsAI()) {
            hasEnemy = true;
          }
          if (tile.Deployable(unit)) {
            deployableTiles.Add(tile);
          }
        }
        if (hasEnemy && hexMap.deployDone) {
          AttackButton.SetActive(true);
        }
        if (deployableTiles.Count > 0) {
          DecampButton.SetActive(true);
        }
        if (unit.labor > 0) {
          TransferLaborButton.SetActive(true);
        }
        //SabotageSiegeButton.SetActive(true);
        return;
      }

      if (hexMap.wargameController.start && hexMap.wargameController.IsWargameUnit(unit)
        || !hexMap.deployDone) {
      } else {
        MoveButton.SetActive(true);
      }

      if (!hexMap.wargameController.start && hexMap.deployDone) {
        NextTurnButton.SetActive(true);
        RetreatButton.SetActive(unit.CanRetreat());
      }
      
      if (mouseController.nearEnemy || mouseController.nearEnemySettlement != null) {
        if (!hexMap.combatController.start && unit.GetStaminaLevel() != StaminaLvl.Exhausted
         && hexMap.deployDone && unit.CanAttack()) {
          AttackButton.SetActive(true);
        }
      }

      if (mouseController.nearAlly && hexMap.deployDone && !hexMap.wargameController.IsWargameUnit(unit)) {
        ReposButton.SetActive(true);
      }

      if (!hexMap.wargameController.start && !unit.IsCavalry() && unit.tile.deadZone.DecompositionCntDown > 0) {
        BuryButton.SetActive(true);
      }

      if (!hexMap.wargameController.start && unit.CanCharge() && hexMap.deployDone) {
        foreach(Unit enemy in mouseController.nearbyEnemey) {
          if (enemy.CanBeShaked(unit) > 0) {
            ChargeButton.SetActive(true);
            break;
          }
        }
      }

      if (mouseController.nearMySettlement != null && mouseController.nearMySettlement.HasRoom()
        && !mouseController.nearMySettlement.IsUnderSiege()) {
        if (!hexMap.wargameController.start) {
          EncampButton.SetActive(true);
        }
      }

      if (unit.type == Type.Infantry && hexMap.deployDone && !hexMap.wargameController.start) {
        if (mouseController.nearDam != null || (unit.tile.siegeWall != null && unit.tile.siegeWall.owner.isAI != unit.IsAI())) {
          SabotageButton.SetActive(true);
        }
      }

      if (mouseController.nearFire != null && hexMap.deployDone && !hexMap.wargameController.start) {
        FireButton.SetActive(true);
      }

      if (mouseController.nearWater && hexMap.deployDone && !hexMap.wargameController.start) {
        DefendButton.SetActive(true);
      }

      if (!hexMap.deployDone && mouseController.inCampField != null && !hexMap.wargameController.start) {
        CampButton.SetActive(true);
      }

      if (unit.type == Type.Infantry && !hexMap.wargameController.start) {
        if (mouseController.inCampField != null && !hexMap.wargameController.start) {
          CampButton.SetActive(true);
        }

        if (hexMap.deployDone &&
          mouseController.nearEnemySettlement != null && !mouseController.nearEnemySettlement.IsEmpty()
          && !hexMap.wargameController.start && mouseController.nearEnemySettlement.type == Settlement.Type.city) {
          SiegeButton.SetActive(true);
        }

        if ((mouseController.nearAlly || mouseController.nearMySettlement != null) && 
         !hexMap.wargameController.start) {
          if (unit.labor > 0) {
            TransferLaborButton.SetActive(true);
          }
          if (unit.supply.supply > 0) {
            TransferSupplyButton.SetActive(true);
          }
        }
      }

      if (unit.type == Type.Cavalry && !hexMap.wargameController.start) {
        if ((mouseController.nearAlly || mouseController.nearMySettlement != null) ||
         !hexMap.wargameController.start) {
          if (unit.supply.supply > 0) {
            TransferSupplyButton.SetActive(true);
          }
        }
      }
    }

    public void OnModeQuit(Unit unit)
    {
      RefreshButtons(unit, false);
    }

    string GetCombatpointRate(int cp) {
      int remaining = cp % 10;
      int stars = (int)((cp-remaining) / 10);
      string rate = "";
      while (stars-- > 0) {
        rate += "★";
      }
      if (remaining != 0) {
        rate += "☆";
      }
      return rate;
    }

    public void OnUnitSelect(Unit unit, bool isGarrison = false)
    {
      self.SetActive(true);
      bool isPreflight = !isGarrison && !Util.eq<Unit>(unit, mouseController.selectedUnit);

      // set attack, defense details
      string details = 
      "惩罚:\n"
      + "战败惩罚:" + unit.GetChaosBuf() * 100
      + "%\n厌战惩罚:" + unit.GetWarwearyBuf() * 100
      + "%\n无胄惩罚:" + unit.disarmorDefDebuf * 100
      + "%\n平原反应:" + unit.plainSickness.debuf * 100
      + "%\n疲惫惩罚:" + unit.GetStaminaDebuf(false) * 100
      + "%\n加成:\n"
      + "等级加成:" + unit.rf.lvlBuf * 100
      + "%\n地形加成:" + unit.vantage.Buf() * 100
      + "%\n总计加成:" + (unit.GetBuff() + unit.GetStaminaDebuf(false)) *100 + "%\n\n"
      + "有效作战人数:" + unit.vantage.GetEffective();
      hexMap.hoverInfo.Show(details);

      title.text = unit.GeneralName();
      movement.text = "移动力:" + (isPreflight ? mouseController.selectedUnit.movementRemaining + " -> " : "")
        + unit.movementRemaining + "/" + unit.GetFullMovement();
      stamina.text = "体力: " + unit.GetStaminaLvlName();
      slots.text = "粮草: " + unit.supply.supply + "石" + " 可维持" + unit.slots + "/" + unit.GetMaxSupplySlots() + "回合" + " 每回合消耗:" + unit.supply.SupplyNeededPerTurn() + "石";
      num.text = unit.Name() + "[兵:" + unit.rf.soldiers + "/伤:" + unit.rf.wounded + "/亡:" + unit.kia + "/逃:" + unit.mia + "/役:" + unit.labor + "]";
      morale.text = "士气: " + unit.rf.morale;
      offense.text = "单兵战力: " + GetCombatpointRate(unit.cp);
      if (unit.IsCamping()) {
        defense.text = "部队战力: " + UnitInfoView.Shorten(unit.unitCampingAttackCombatPoint)
        + " ♙" + UnitInfoView.Shorten(unit.GetUnitDefendCombatPoint(true));
      } else {
        defense.text = "部队战力: " + UnitInfoView.Shorten(unit.GetUnitAttackCombatPoint()) + "/" + UnitInfoView.Shorten(unit.unitPureCombatPoint)
        + " ♙" + UnitInfoView.Shorten(unit.GetUnitDefendCombatPoint(true));
      }
      string stateStr = unit.tile.siegeWall != null && unit.tile.siegeWall.IsFunctional() ? "围城中 " :
        (unit.tile.siegeWall != null && unit.tile.siegeWall.owner.isAI == unit.IsAI() ? ("建长围中:" + unit.tile.siegeWall.buildTurns + "回合完成 ") : "");
      stateStr += unit.IsWarWeary() ? "士气低落 " : "";
      stateStr += unit.GetDiscontent() + " ";
      stateStr += unit.IsHungry() ? (unit.IsStarving() ? "饥饿 " : "半饥饿 ") : "";
      stateStr += unit.GetStateName();
      int desserter = unit.IsStarving() ? unit.GetStarvingDessertNum() : 0;
      int killed = unit.IsStarving() ? unit.GetStarvingKillNum() : 0;
      desserter += unit.IsWarWeary() ? unit.warWeary.GetWarWearyDissertNum() : 0;
      state.text = "";
      if (desserter != 0) {
        stateStr += "[本轮" + desserter + "人逃亡" + (killed > 0 ? (killed + "人亡") : "") + "]";
      }
      state.text = stateStr;
      illness.text = unit.GetHeatSickTurns() > 0 ? "痢疾: 将持续" + unit.GetHeatSickTurns() + "回合 " : "";
      illness.text += unit.GetAltitudeSickTurns() > 0 ? "高原反应: 将持续" + unit.GetAltitudeSickTurns() + "回合" : "";
      illness.text += unit.plainSickness.affected  ? "平原反应" : "";
      poision.text = unit.GetPoisionTurns() > 0 ? "中毒: 将持续" + unit.GetPoisionTurns() + "回合" : "";
      if (unit.clone) {
        return;
      }

      ToggleButtons(unit.IsAI() == !turnController.player, unit, isGarrison);
    }

    public void OnUnitDeselect(Unit unit)
    {
      ToggleButtons(false, unit);
      self.SetActive(false);
      FoW.Init(hexMap);
    }

    public void OnBtnClick(ActionController.actionName action)
    {
      if (action == ActionController.actionName.DEPLOYMENTDONE) {
        hexMap.turnController.DeploymentDone();
        hexMap.deployDone = true;
        DeploymentDoneButton.SetActive(false);
        hexMap.wargameController.WargameBtn.SetActive(true);
        NextTurnButton.SetActive(true);
        FoW.Get().Fog();
      }

      if (action == ActionController.actionName.SHOWMINE || action == ActionController.actionName.SHOWENEMY) {
        WarParty wp = action == ActionController.actionName.SHOWENEMY ? hexMap.GetAIParty() : hexMap.GetPlayerParty();
        WarPartyStat stat = wp.GetStat();
        List<Settlement> roots = wp.attackside ? hexMap.settlementMgr.attackerRoots : hexMap.settlementMgr.defenderRoots;
        int labor = stat.numOfLabor;
        int count = 0;
        foreach(Settlement s in roots) {
          labor += s.labor;
          count++;
        }
        int attackerCP = 0;
        int defenderCP = 0;
        WarParty wp1 = hexMap.GetAIParty();
        foreach(Unit u in wp1.GetUnits()) {
          if (wp1.attackside) {
            attackerCP += u.unitPureCombatPoint;
          } else {
            defenderCP += u.unitPureDefendCombatPoint;
          }
        }
        wp1 = hexMap.GetPlayerParty();
        foreach(Unit u in wp1.GetUnits()) {
          if (wp1.attackside) {
            attackerCP += u.unitPureCombatPoint;
          } else {
            defenderCP += u.unitPureDefendCombatPoint;
          }
        }

        string info =
         "步兵: " + stat.numOfInfantryUnit + "都\n" +
         "  战兵: " + stat.numOfInfantry + " 伤: " + stat.numOfInfantryWound +
         " 亡: " + stat.numOfInfantryDead + "\n" +
         "骑兵: " + stat.numOfCavalryUnit + "都\n" +
         "  战兵: " + stat.numOfCavalry + " 伤: " + stat.numOfCavalryWound +
         " 亡: " + stat.numOfCavalryDead + "\n" +
         "兵役: " + labor + " 亡: " +
         (wp.attackside ? hexMap.settlementMgr.attackerLaborDead : hexMap.settlementMgr.defenderLaborDead) + "\n" + 
         "控制城市: " + count + "\n" +  
         "总计: 战兵 " + (stat.numOfInfantry + stat.numOfCavalry) +
         " 伤 " + (stat.numOfInfantryWound + stat.numOfCavalryWound) +
         " 亡 " + (stat.numOfInfantryDead + stat.numOfCavalryDead) + "\n";

        if (!wp.attackside) {
          info += "平民死亡: \n" + "  男: " + hexMap.settlementMgr.totalMaleDead +
           " 女: " + hexMap.settlementMgr.totalFemaleDead +
           " 幼: " + hexMap.settlementMgr.totalChildDead + "\n";
        }

        info += "党派关系： " + wp.faction.GetParties()[0].GetRelationDescription();
        info += "\n\n攻方: " + UnitInfoView.Shorten(attackerCP) + " VS 守方: " + UnitInfoView.Shorten(defenderCP);
        hexMap.hoverInfo.Show(info);
        return;
      }

      // disable other buttons than move
      ToggleButtons(false, mouseController.selectedUnit);
    }

    public override void UpdateChild() { }
  }

}