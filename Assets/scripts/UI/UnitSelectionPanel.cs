using UnityEngine;
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
      GameObject[] btns = {MoveButton, AttackButton, DefendButton,
                           SabotageButton, FireButton, SiegeButton, EncampButton,
                           RetreatButton, DecampButton, ReposButton,
                           BuryButton, ChargeButton, TroopButton, GeneralButton,
                           BreakThroughButton, SurpriseAttackButton, FeintDefeatButton,
                           ForecastButton, FalseOrderButton, AlienateButton, SkirmishButton
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
    public GameObject ForecastButton;
    public GameObject FalseOrderButton;
    public GameObject AlienateButton;
    public GameObject SkirmishButton;
    GameObject[] buttons;

    public Text title;
    public Image generalPortrait; 

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
      TroopButton.SetActive(true);
      GeneralButton.SetActive(true);

      // TODO: uncomment
      //if (!state || hexMap.combatController.start || unit.IsAI()) return;
      if (!state || hexMap.combatController.start) return;

      if (!hexMap.wargameController.start && hexMap.deployDone) {
        BreakThroughButton.SetActive(unit.CanBreakThrough());
      }

      if (hexMap.deployDone) {
        AttackButton.SetActive(unit.CanAttack());
      }

      if (mouseController.nearFireTiles.Count > 0 && hexMap.deployDone && !hexMap.wargameController.start) {
        FireButton.SetActive(unit.CanFire());
      }

      if (mouseController.nearWater && hexMap.deployDone && !hexMap.wargameController.start) {
        DefendButton.SetActive(unit.CanPoision());
      }

      if (!hexMap.wargameController.start && hexMap.deployDone) {
        ChargeButton.SetActive(unit.CanCharge());
        SkirmishButton.SetActive(unit.CanHarras() && unit.GetHarrasTargets().Count > 0);
      }

      if (!hexMap.wargameController.start && hexMap.deployDone) {
        ForecastButton.SetActive(unit.CanForecast());
      }

      if (!hexMap.wargameController.start && unit.CanFalseOrder() && hexMap.deployDone) {
        FalseOrderButton.SetActive(mouseController.falseOrderTargets.Count > 0);
      }

      if (!hexMap.wargameController.start && unit.CanAlienate() && hexMap.deployDone) {
        AlienateButton.SetActive(mouseController.alienateTargets.Count > 0);
      }

      if (isGarrison) {
        deployableTiles = new List<Tile>();
        foreach(Tile tile in mouseController.selectedSettlement.baseTile.neighbours) {
          Unit u = tile.GetUnit();
          if (tile.Deployable(unit)) {
            deployableTiles.Add(tile);
          }
        }
        if (deployableTiles.Count > 0) {
          DecampButton.SetActive(true);
        }

        return;
      }

      if (hexMap.wargameController.start && hexMap.wargameController.IsWargameUnit(unit)
        || !hexMap.deployDone) {
      } else {
        MoveButton.SetActive(true);
      }

      if (!hexMap.wargameController.start && hexMap.deployDone && !unit.retreated) {
        NextTurnButton.SetActive(true);
        RetreatButton.SetActive(true);
      }
      
      if (!hexMap.combatController.start && hexMap.deployDone && unit.CanAttack()) {
        FeintDefeatButton.SetActive(unit.rf.general.Has(Cons.tactic) && mouseController.nearEnemy);
        SurpriseAttackButton.SetActive(mouseController.surpriseTargets.Length > 0 && unit.CanSurpriseAttack());
      }

      if (mouseController.nearAlly && hexMap.deployDone && !hexMap.wargameController.IsWargameUnit(unit)) {
        ReposButton.SetActive(true);
      }

      if (!hexMap.wargameController.start && !unit.IsCavalry() && unit.tile.deadZone.DecompositionCntDown > 0) {
        BuryButton.SetActive(true);
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

      if (unit.type == Type.Infantry && !hexMap.wargameController.start) {
        if (hexMap.deployDone &&
          mouseController.nearEnemySettlement != null && !mouseController.nearEnemySettlement.IsEmpty()
          && !hexMap.wargameController.start) {
          SiegeButton.SetActive(true);
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
      + "等级加成:" + unit.rf.lvlBuf * 100
      + "%\n地形加成:" + unit.vantage.Buf() * 100
      + "%\n重骑加成:" + unit.HeavyCavalryBuf() * 100
      + "%\n将领加成:" + unit.GetGeneralBuf() * 100
      + "%\n总计加成:" + (unit.GetBuff() *100) + "%\n\n";
      hexMap.hoverInfo.Show(details);

      title.text = unit.GeneralName();
      title.text += "\n统帅度:" + GetStarRate(unit.rf.general.commandSkill.commandSkill);
      title.text += "\n指挥度:" + GetStarRate(unit.rf.general.size.troopSize);
      title.text += "\n移动力:" + (isPreflight ? mouseController.selectedUnit.movementRemaining + " -> " : "")
        + unit.movementRemaining + "/" + unit.GetFullMovement();
      title.text += "\n" + unit.Name() + "[" + unit.GetUnitName() + " 兵:" + unit.rf.soldiers + "/亡:" + unit.kia + "]";
      title.text += "\n士气: " + unit.rf.morale;
      title.text += "\n单兵战力: " + GetCombatpointRate((int)(unit.cp * (1 + unit.rf.lvlBuf)));
      if (unit.IsCamping()) {
        title.text += "\n部队战力: " + UnitInfoView.Shorten(unit.unitCampingAttackCombatPoint);
      } else {
        title.text += "\n部队战力: " + UnitInfoView.Shorten(unit.unitCombatPoint);
      }
      string stateStr = unit.tile.siegeWall != null && unit.tile.siegeWall.IsFunctional() ? "围城中 " :
        (unit.tile.siegeWall != null && unit.tile.siegeWall.owner.isAI == unit.IsAI() ? ("建长围中:" + unit.tile.siegeWall.buildTurns + "回合完成 ") : "");
      stateStr += unit.IsWarWeary() ? "士气低落 " : "";
      stateStr += unit.IsStarving() ? "补给不济 " : "";
      stateStr += unit.GetStateName();
      title.text += "\n" + (unit.hasNoOpenning ? "[无懈可击]" : "")
        + (unit.defeatStreak > 0 ? ("[战败累计:" + unit.defeatStreak + "]") : "" ) + stateStr;
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
      foreach(Ability ability in unit.rf.general.acquiredAbilities) {
        title.text += "{" + ability.Name() + "}: " + ability.Description() + "\n";
      }
    }

    void ShowTraitInfo(Unit unit) {
      title.fontSize = 18;
      title.text = "";
      if (unit.IsCommander()) {
        title.text += "统帅能力:\n";
        if (unit.rf.general.commandSkill.ObeyMyOrder()) {
          title.text += "指挥范围内友军强制服从主帅指挥\n";
        }
        if (unit.rf.general.commandSkill.TurningTide()) {
          title.text += "指挥范围内友军战败溃败几率降低\n";
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
        DeploymentDoneButton.SetActive(false);
        hexMap.wargameController.WargameBtn.SetActive(true);
        NextTurnButton.SetActive(true);
        FoW.Get().Fog(hexMap.allTiles);
      }

      if (action == ActionController.actionName.SHOWMINE || action == ActionController.actionName.SHOWENEMY) {
        hexMap.CleanSupplyLines();
        if (toggled) {
          toggled = false;
          hexMap.DehighlightArea();
          return;
        }
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

      // disable other buttons than move
      ToggleButtons(false, mouseController.selectedUnit);
    }

    public override void UpdateChild() { }
  }

}