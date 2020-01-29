using UnityEngine;
using UnityEngine.UI;
using UnitNS;
using FieldNS;
using System.Collections.Generic;

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
                           SabotageButton, FireButton, AmbushButton, EncampButton,
                           RetreatButton, TransferSupplyButton, TransferLaborButton};
      buttons = btns;
      mouseController.onUnitSelect += OnUnitSelect;
      mouseController.onUnitPreflight += OnUnitSelect;
      mouseController.onUnitDeselect += OnUnitDeselect;
      mouseController.onModeQuit += OnModeQuit;
      hexMap.eventDialog.eventDialogOn += EventDialogOn;
      hexMap.eventDialog.eventDialogOff += EventDialogOff;
      self = this.transform.gameObject;
      self.SetActive(false);
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
    public GameObject FireButton;
    public GameObject AmbushButton; // Siege
    public GameObject EncampButton;
    public GameObject RetreatButton;
    public GameObject TransferSupplyButton;
    public GameObject TransferLaborButton;
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
      NextTurnButton.SetActive(true);
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
        RefreshButtons(unit);
      }
    }

    void RefreshButtons(Unit unit)
    {
      ToggleButtons(!unit.TurnDone(), unit);
    }

    void ToggleButtons(bool state, Unit unit)
    {
      foreach (GameObject button in buttons)
      {
        button.SetActive(false);
      }
      if (!state) return;

      MoveButton.SetActive(true);
      NextTurnButton.SetActive(true);
      RetreatButton.SetActive(true);
      
      if (mouseController.nearEnemy || mouseController.nearEnemySettlement != null) {
        AttackButton.SetActive(true);
      }

      if (mouseController.nearMySettlement != null && mouseController.nearMySettlement.HasRoom()
        && !mouseController.nearMySettlement.IsUnderSiege()) {
        EncampButton.SetActive(true);
      }

      if (unit.type == Type.Scout) {
        if (mouseController.nearDam != null) {
          SabotageButton.SetActive(true);
        }

        if (mouseController.nearFire != null) {
          FireButton.SetActive(true);
        }

        if (mouseController.nearWater) {
          DefendButton.SetActive(true);
        }
      }

      if (unit.type == Type.Infantry) {
        if (mouseController.inCampField != null) {
          CampButton.SetActive(true);
        }

        if (mouseController.nearEnemySettlement != null && !mouseController.nearEnemySettlement.IsEmpty()) {
          AmbushButton.SetActive(true);
        }

        if (mouseController.nearAlly || mouseController.nearMySettlement != null) {
          if (unit.labor > 0) {
            TransferLaborButton.SetActive(true);
          }
          if (unit.supply.supply > 0) {
            TransferSupplyButton.SetActive(true);
          }
        }
      }

      if (unit.type == Type.Cavalry) {
        if (mouseController.nearAlly || mouseController.nearMySettlement != null) {
          if (unit.supply.supply > 0) {
            TransferSupplyButton.SetActive(true);
          }
        }
      }
    }

    public void OnModeQuit(Unit unit)
    {
      RefreshButtons(unit);
    }

    public void OnUnitSelect(Unit unit)
    {
      self.SetActive(true);

      // set attack, defense details
      string details = 
      "基本加成\n"
      + "精力加成:" + unit.GetStaminaBuf() * 100
      + "% 饥饿惩罚:" + unit.GetStarvingBuf() * 100
      + "% 整合度惩罚:" + unit.GetNewGeneralBuf() * 100

      + "%\n单兵攻击:" + unit.atk + "\n[基本攻击:" + unit.rf.atkCore
      + " 等级加成:" + unit.rf.atkLvlBuf * 100
      + "% 地形加成:" + unit.vantage.AtkBuf() * 100
      + "% 气候加成:" + unit.weatherEffect.AtkBuf() * 100
      + "% 总计:" +
      (unit.GetStaminaBuf() + unit.GetStarvingBuf() + unit.GetNewGeneralBuf()
       + unit.rf.atkLvlBuf + unit.vantage.AtkBuf() + unit.weatherEffect.AtkBuf())*100
      + "%]\n"

      + "单兵防御:" + unit.def + "\n[基本防御:" + unit.rf.defCore
      + " 等级加成:" + unit.rf.defLvlBuf * 100
      + "% 地形加成:" + unit.vantage.DefBuf() * 100
      + "% 免胄惩罚:" + unit.disarmorDefDebuf
      + " 总计:" +
      (unit.GetStaminaBuf() + unit.GetStarvingBuf() + unit.GetNewGeneralBuf() + unit.rf.defLvlBuf + unit.vantage.DefBuf())*100
      + "%]\n"
      + "有效兵力:" + unit.vantage.GetEffective();
      hexMap.hoverInfo.Show(details);

      title.text = unit.GeneralName();
      movement.text = "移动力:" + unit.movementRemaining + "/" + unit.GetFullMovement();
      stamina.text = "体力: " + unit.GetStaminaLvlName();
      slots.text = "粮草: " + unit.supply.supply + "石" + " 可维持" + unit.slots + "/" + unit.GetMaxSupplySlots() + "回合" + " 每回合消耗:" + unit.supply.SupplyNeededPerTurn() + "石";
      num.text = unit.Name() + "[兵:" + unit.rf.soldiers + "/伤:" + unit.rf.wounded + "/亡:" + unit.kia + "/逃:" + unit.mia + "/役:" + unit.labor + "]";
      morale.text = "士气: " + unit.rf.morale;
      offense.text = "攻击: " + unit.unitAttack;
      defense.text = "防御: " + unit.unitDefence;
      string stateStr = unit.tile.sieged ? "围城中" : ""; 
      stateStr += unit.IsWarWeary() ? " 士气低落" : "";
      stateStr += " " + unit.GetDiscontent();
      stateStr += unit.IsStarving() ? " 饥饿" : "";
      stateStr += unit.GetStateName();
      int desserter = unit.IsStarving() ? unit.GetStarvingDessertNum() : 0;
      int killed = unit.IsStarving() ? unit.GetStarvingKillNum() : 0;
      desserter += unit.IsWarWeary() ? unit.warWeary.GetWarWearyDissertNum() : 0;
      state.text = "";
      if (desserter != 0) {
        stateStr += "[本轮" + desserter + "人逃亡" + (killed > 0 ? (killed + "人亡") : "") + "]";
      }
      state.text = stateStr;
      illness.text = unit.GetHeatSickTurns() > 0 ? "痢疾: 预计本轮至少伤亡"+ unit.GetHeatSickEffectNum()
      + "人,疫情还将持续" + unit.GetHeatSickTurns() + "回合" : "";
      poision.text = unit.GetPoisionTurns() > 0 ? "中毒: 预计本轮伤亡"+ unit.GetPoisionEffectNum()
      + "人,病情还将持续" + unit.GetPoisionTurns() + "回合" : "";
      if (unit.clone) {
        return;
      }

      ToggleButtons(false, unit);
      // TODO: AI test
      if (unit.IsAI() == !turnController.player) {
        RefreshButtons(unit);
      }
    }

    public void OnUnitDeselect(Unit unit)
    {
      ToggleButtons(false, unit);
      self.SetActive(false);
    }


    public void OnBtnClick(ActionController.actionName action)
    {
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

        string info =
         "步兵: " + stat.numOfInfantryUnit + "都\n" +
         "  战兵: " + stat.numOfInfantry + " 伤: " + stat.numOfInfantryWound +
         " 亡: " + stat.numOfInfantryDead + "\n" +
         "骑兵: " + stat.numOfCavalryUnit + "都\n" +
         "  战兵: " + stat.numOfCavalry + " 伤: " + stat.numOfCavalryWound +
         " 亡: " + stat.numOfCavalryDead + "\n" +
         "斥候: " + stat.numOfScoutUnit + "队\n" +
         "  战兵: " + stat.numOfScout + " 伤: " + stat.numOfScoutWound +
         " 亡: " + stat.numOfScoutDead + "\n" +
         "兵役: " + labor + " 亡: " +
         (wp.attackside ? hexMap.settlementMgr.attackerLaborDead : hexMap.settlementMgr.defenderLaborDead) + "\n" + 
         "控制城市: " + count + "\n" +  
         "总计: 战兵 " + (stat.numOfInfantry + stat.numOfCavalry + stat.numOfScout) +
         " 伤 " + (stat.numOfInfantryWound + stat.numOfCavalryWound + stat.numOfScoutWound) +
         " 亡 " + (stat.numOfInfantryDead + stat.numOfCavalryDead + stat.numOfScoutDead) + "\n";

        if (!wp.attackside) {
          info += "平民死亡: \n" + "  男: " + hexMap.settlementMgr.totalMaleDead +
           " 女: " + hexMap.settlementMgr.totalFemaleDead +
           " 幼: " + hexMap.settlementMgr.totalChildDead + "\n";
        }
        hexMap.hoverInfo.Show(info);
        return;
      }

      // disable other buttons than move
      ToggleButtons(false, mouseController.selectedUnit);
      if (action == ActionController.actionName.MOVE)
      {
        MoveButton.SetActive(true);
      }
      if (action == ActionController.actionName.ATTACK)
      {
        AttackButton.SetActive(true);
      }
    }

    public override void UpdateChild()
    {
      if (!turnController.endingTurn && !turnController.showingTitle)
      {
        NextTurnButton.SetActive(true);
      }
    }
  }

}