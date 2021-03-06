﻿using UnityEngine;
using UnityEngine.UI;
using UnitNS;
using FieldNS;
using System.Collections.Generic;
using MapTileNS;
using static MonoNS.HexMap;
using CourtNS;

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
      actionController.onBtnClick += OnBtnClick;
      turnController.onEndTurnClicked += OnEndTurnClicked;
      turnController.onNewTurn += OnNewTurn;
      GameObject[] btns = {MoveButton, AttackButton, PoisonButton,
                           SabotageButton, FireButton, SiegeButton, EncampButton,
                           RetreatButton, DecampButton, ReposButton,
                           BuryButton, ChargeButton, TroopButton, GeneralButton,
                           BreakThroughButton, SurpriseAttackButton, FeintDefeatButton,
                           FreezeButton, AgitateButton, RallyButton
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
    public GameObject PoisonButton; // Poison
    public GameObject SabotageButton;
    public GameObject NextTurnButton;
    public GameObject DeploymentDoneButton;
    public GameObject FireButton;
    public GameObject SiegeButton; // Siege
    public GameObject EncampButton;
    public GameObject RetreatButton;
    public GameObject DecampButton;
    public GameObject ReposButton;
    public GameObject BuryButton;
    public GameObject ChargeButton;
    public GameObject BreakThroughButton;
    public GameObject TroopButton;
    public GameObject GeneralButton;
    public GameObject SurpriseAttackButton;
    public GameObject FeintDefeatButton;
    public GameObject FreezeButton;
    public GameObject AgitateButton;
    public GameObject RallyButton;
    GameObject[] buttons;

    void Disable(GameObject btn) {
      btn.GetComponent<Button>().interactable = false;
    }
    void Enable(GameObject btn) {
      btn.GetComponent<Button>().interactable = true;
    }

    public Text title;
    public Image generalPortrait; 

    public void OnEndTurnClicked()
    {
      Disable(NextTurnButton);
    }

    public void OnNewTurn()
    {
      if (hexMap.deployDone) {
        Enable(NextTurnButton);
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

    //public void ActionDone(Unit unit, Unit[] units, ActionController.actionName name)
    //{
    //  if ((unit != null && !unit.IsAI() && name == ActionController.actionName.MOVE)
    //    || (unit != null && unit.IsAI() && name == ActionController.actionName.ATTACK))
    //  {
    //    RefreshButtons(unit, false);
    //  }
    //}

    void RefreshButtons(Unit unit, bool isGarrison)
    {
      ToggleButtons(!unit.TurnDone(), unit, isGarrison);
    }

    public bool tileSelecting = false;
    public void Decamp() {
      tileSelecting = true;
      hexMap.msgBox.Show("选择部署地点");
      hexMap.HighlightArea(decampTiles.ToArray(), HexMap.RangeType.camp);
    }

    List<Tile> decampTiles {
      get {
        return hexMap.mouseController.UnitSelected().CanDecamp(hexMap.mouseController.selectedSettlement);
      }
    }

    public void CancelGarrison(Unit unit) {
      hexMap.msgBox.Show("");
      if (tileSelecting) {
        tileSelecting = false;
        hexMap.DehighlightArea();
      }
      OnUnitDeselect(unit);
    }

    public bool isSelectableTile(Tile tile) {
      foreach(Tile t in decampTiles) {
        if (Util.eq<Tile>(t, tile)) {
          return true;
        }
      }
      return false;
    }

    public void ToggleButtons(bool state, Unit unit, bool isGarrison = false)
    {
      if (unit == null) return;
      FreezeButton.SetActive(Freezer.Aval(unit));
      FeintDefeatButton.SetActive(Deciever.Aval(unit));
      AgitateButton.SetActive(Agitator.Aval(unit));
      RallyButton.SetActive(Rally.Aval(unit));
      SurpriseAttackButton.SetActive(Ambusher.Aval(unit));

      foreach (GameObject button in buttons)
      {
        Disable(button);
      }
      Enable(TroopButton);
      Enable(GeneralButton);

      // TODO: uncomment
      //if (!state || hexMap.combatController.start || unit.IsAI()) return;
      if (!state || hexMap.combatController.start) return;

      if (!hexMap.wargameController.start && hexMap.deployDone && unit.CanBreakThrough().Count > 0) {
        Enable(BreakThroughButton);
      }

      if (hexMap.deployDone && unit.CanAttack().Count > 0) {
        Enable(AttackButton);
      }

      if (hexMap.deployDone && !hexMap.wargameController.start && unit.CanFire().Count > 0) {
        Enable(FireButton);
      }

      if (hexMap.deployDone && !hexMap.wargameController.start && unit.CanPoison().Count > 0) {
        Enable(PoisonButton);
      }

      if (!hexMap.wargameController.start && hexMap.deployDone && unit.CanCharge().Count > 0) {
        Enable(ChargeButton);
      }

      if (!hexMap.wargameController.start && unit.CanFreeze().Count > 0 && hexMap.deployDone) {
        Enable(FreezeButton);
      }

      if (!hexMap.wargameController.start && unit.CanPlot().Count > 0 && hexMap.deployDone) {
        Enable(AgitateButton);
      }

      if (!hexMap.wargameController.start && unit.CanRally().Count > 0 && hexMap.deployDone) {
        Enable(RallyButton);
      }

      if (isGarrison) {
        if (unit.CanDecamp(mouseController.selectedSettlement).Count > 0) {
          Enable(DecampButton);
        }
        return;
      }

      if (hexMap.wargameController.start && hexMap.wargameController.IsWargameUnit(unit)
        || !hexMap.deployDone) {
      } else {
        Enable(MoveButton);
      }

      if (!hexMap.wargameController.start && hexMap.deployDone) {
        Enable(NextTurnButton);
        if (unit.CanTriggerRetreat()) {
          Enable(RetreatButton);
        }
      }
      
      if (!hexMap.combatController.start && hexMap.deployDone) {
        if (unit.CanDecieve().Count > 0) {
          Enable(FeintDefeatButton);
        }
        if (unit.CanSurpriseAttack().Count > 0) {
          Enable(SurpriseAttackButton);
        }
      }

      if (unit.OnFieldAllies().Count > 0 && hexMap.deployDone && !hexMap.wargameController.IsWargameUnit(unit)) {
        Enable(ReposButton);
      }

      if (!hexMap.wargameController.start && unit.CanBury()) {
        Enable(BuryButton);
      }

      if (unit.CanEncamp() != null) {
        if (!hexMap.wargameController.start) {
          Enable(EncampButton);
        }
      }

      if (hexMap.deployDone && !hexMap.wargameController.start && unit.CanSabotage() != null) {
        Enable(SabotageButton);
      }

      if (hexMap.deployDone && !hexMap.wargameController.start && unit.CanSiege()) {
        Enable(SiegeButton);
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

    string GetStarRate(int stars) {
      string rate = "";
      while(stars-- > 0) {
        rate += "★";
      }
      return rate;
    }

    Unit currentUnit;
    bool isCurrentGarrison;
    public void OnUnitSelect(Unit unit, bool isGarrison = false)
    {
      title.fontSize = 20;
      currentUnit = unit;
      isCurrentGarrison = isGarrison;
      self.SetActive(true);
      generalPortrait.sprite = hexMap.imgLibrary.GetGeneralPortrait(unit.rf.general);
      bool isPreflight = !isGarrison && !Util.eq<Unit>(unit, mouseController.selectedUnit);

      // set attack, defense details
      string details = 
      "惩罚:\n"
      + "心理影响:" + unit.GetMentalBuf() * 100
      + "%\n无胄惩罚:" + unit.disarmorDefDebuf * 100
      + "%\n平原反应:" + unit.plainSickness.debuf * 100
      + "%\n加成:\n"
      + "%\n地形加成:" + unit.vantage.Buf() * 100
      + "%\n总计加成:" + (unit.GetBuff() *100) + "%\n\n";
      hexMap.hoverInfo.Show(details);

      title.text = unit.GeneralName();
      title.text += "\n统帅度:" + GetStarRate(unit.rf.general.commandSkill.commandSkill);
      title.text += "\n移动力:" + (isPreflight ? mouseController.selectedUnit.movementRemaining + " -> " : "")
        + unit.movementRemaining + "/" + unit.GetFullMovement();
      title.text += "\n" + unit.Name() + "[兵:" + unit.rf.soldiers + "/亡:" + unit.kia + "]";
      title.text += "\n伤亡承受率: " + unit.rf.org + "%";
      title.text += "\n单兵战力: " + GetCombatpointRate(unit.cp);
      if (unit.IsCamping()) {
        title.text += "\n部队战力: " + UnitInfoView.Shorten(unit.unitCampingAttackCombatPoint);
      } else {
        title.text += "\n部队战力: " + UnitInfoView.Shorten(unit.unitCombatPoint);
      }
      string stateStr = unit.tile.siegeWall != null && unit.tile.siegeWall.IsFunctional() ? "围城中 " :
        (unit.tile.siegeWall != null && unit.tile.siegeWall.owner.isAI == unit.IsAI() ? ("建长围中:" + unit.tile.siegeWall.buildTurns + "回合完成 ") : "");
      stateStr += unit.IsWarWeary() ? "无心恋战 " : "";
      stateStr += unit.IsStarving() ? "补给不济 " : "";
      stateStr += unit.GetStateName();
      title.text += "\n" + stateStr;
      title.text += unit.GetHeatSickTurns() > 0 ? "\n痢疾: 将持续" + unit.GetHeatSickTurns() + "回合 " : "";
      title.text += unit.GetAltitudeSickTurns() > 0 ? "\n高原反应: 将持续" + unit.GetAltitudeSickTurns() + "回合" : "";
      title.text += unit.plainSickness.affected  ? "\n平原反应" : "";
      title.text += unit.GetPoisionTurns() > 0 ? "\n中毒: 将持续" + unit.GetPoisionTurns() + "回合" : "";
      if (unit.clone) {
        return;
      }

      ToggleButtons(unit.IsAI() == !turnController.player, unit, isGarrison);
    }

    void ShowAbilityInfo(Unit unit) {
      title.fontSize = 18;
      title.text = "";
      foreach(Ability ability in unit.rf.general.acquiredAbilities.Values) {
        title.text += "{" + ability.Name() + ability.Icon(unit) + "}: " + ability.Description() + "\n";
      }
    }

    void ShowTraitInfo(Unit unit) {
      title.fontSize = 18;
      title.text = "";
      if (unit.IsCommander()) {
        title.text += "统帅能力:\n";
        if (unit.rf.general.commandSkill.Obey()) {
          title.text += "指挥范围内友军强制服从主帅指挥\n";
        }
      }
      title.text += "\n**性格**: " + unit.rf.general.trait.Name() + "\n  " + unit.rf.general.trait.Description();
    }

    public void OnUnitDeselect(Unit unit)
    {
      ToggleButtons(false, unit);
      self.SetActive(false);
      FoW.Init(hexMap);
    }

    bool toggled = false;
    public void OnBtnClick(ActionController.actionName action)
    {
      if (toggled) {
        toggled = false;
        hexMap.DehighlightArea();
        return;
      }

      if (action == ActionController.actionName.TroopInfo) {
        OnUnitSelect(currentUnit, isCurrentGarrison);
      } else if (action == ActionController.actionName.AbilityInfo) {
        ShowAbilityInfo(currentUnit);
      } else if (action == ActionController.actionName.TraitInfo) {
        ShowTraitInfo(currentUnit);
      }

      if (action == ActionController.actionName.DEPLOYMENTDONE) {
        hexMap.turnController.DeploymentDone();
        hexMap.deployDone = true;
        Disable(DeploymentDoneButton);
        Enable(hexMap.wargameController.WargameBtn);
        Enable(NextTurnButton);
        FoW.Get().Fog(hexMap.allTiles);
      }

      if (action == ActionController.actionName.SHOWMINE || action == ActionController.actionName.SHOWENEMY) {
        hexMap.CleanSupplyLines();
        toggled = true;
        Settlement root;
        if (action == ActionController.actionName.SHOWENEMY) {
          root = hexMap.IsAttackSide(true) ? hexMap.settlementMgr.attackerRoot : hexMap.settlementMgr.defenderRoot;
        } else {
          root = hexMap.IsAttackSide(false) ? hexMap.settlementMgr.attackerRoot : hexMap.settlementMgr.defenderRoot;
        }
        List<Settlement> linked = new List<Settlement>();
        root.GetLinked(linked);
        foreach(Settlement s in linked) {
          foreach(Tile tile in s.baseTile.linkedTilesForCamp) {
            if (linked.Contains(tile.settlement)) {
              hexMap.DrawSupplyLine(s.baseTile, tile);
            }
          }
        }

        WarParty wp = action == ActionController.actionName.SHOWENEMY ? hexMap.GetAIParty() : hexMap.GetPlayerParty();
        WarPartyStat stat = wp.GetStat();
        WarParty wp1 = hexMap.GetAIParty();
        WarParty wp2 = hexMap.GetPlayerParty();
        int attackerCP = wp1.attackside ? wp1.GetTotalPoint() : wp2.GetTotalPoint();
        int defenderCP = wp2.attackside ? wp1.GetTotalPoint() : wp2.GetTotalPoint();

        string info =
         "步兵: " + stat.numOfInfantryUnit + "都\n" +
         "  战兵: " + UnitInfoView.Shorten(stat.numOfInfantry) + "/" + UnitInfoView.Shorten(stat.numOfInfantryDead) + "亡\n" +
         "骑兵: " + stat.numOfCavalryUnit + "都\n" +
         "  战兵: " + UnitInfoView.Shorten(stat.numOfCavalry) + "/" + UnitInfoView.Shorten(stat.numOfCavalryDead) + "亡\n" +
         "总计: 战兵 " + UnitInfoView.Shorten(stat.numOfInfantry + stat.numOfCavalry) +
         "/" + UnitInfoView.Shorten(stat.numOfInfantryDead + stat.numOfCavalryDead) + "亡\n";

        if (!wp.attackside) {
          info += "平民死亡: \n" + "  男: " + hexMap.settlementMgr.totalMaleDead +
           " 女: " + hexMap.settlementMgr.totalFemaleDead +
           " 幼: " + hexMap.settlementMgr.totalChildDead + "\n";
        }

        info += "粮草: " + UnitInfoView.Shorten(wp.supply) + "石\n";
        info += "党派关系： " + wp.faction.GetParties()[0].GetRelationDescription();
        info += "\n\n攻方: " + UnitInfoView.Shorten(attackerCP) + " VS 守方: " + UnitInfoView.Shorten(defenderCP);
        hexMap.hoverInfo.Show(info);

        hexMap.HighlightArea(hexMap.settlementMgr.GetControlledTiles(wp.isAI).ToArray(), RangeType.supplyRange);
        return;
      }

      if (action == ActionController.actionName.ShowZone) {
        toggled = true;
        hexMap.HighlightArea(turnController.GetWarParty().discoveredTiles, RangeType.zoneIndication);
      }
      if (mouseController.selectedUnit != null) {
        // disable other buttons than move
        ToggleButtons(false, mouseController.selectedUnit);
      }
    }

    public override void UpdateChild() { }
  }

}