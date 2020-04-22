﻿using System.Collections.Generic;
using UnityEngine;
using UnitNS;
using FieldNS;
using MapTileNS;
using CourtNS;
using System.Collections;

namespace MonoNS
{
  public class OperationGeneralResult {
    public int chance;
    public OperationGeneralResult(int chance) {
      this.chance = chance > 10 ? 10 : (chance < 0 ? 0 : chance);
    }

    public string GetResult() {
      return System.String.Format(Cons.GetTextLib().get("operation_success_chance"), chance);
    }
  }

  public class UnitPredict {
    public Unit unit;
    public int percentOfEffectiveForce;
    public int joinPossibility;
    public int operationPoint;
    public bool windAdvantage = false;
    public bool windDisadvantage = false;
    public int dead = 0;
    public int leastNum = 0;
  }

  public class OperationPredict {
    public List<UnitPredict> attackers = new List<UnitPredict>();
    public List<UnitPredict> defenders = new List<UnitPredict>();
    public int attackerOptimPoints = 1;
    public int defenderOptimPoints = 1;
    public OperationGeneralResult sugguestedResult;
    public CombatController.ResultType suggestedResultType;
  }

  public class CombatController : BaseController
  {
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      mouseController = hexMap.mouseController;
      actionController = hexMap.actionController;
      msgBox = hexMap.msgBox;
    }

    public override void UpdateChild() {}
    MouseController mouseController;
    ActionController actionController;
    MsgBox msgBox;

    public bool start = false;
    Unit attacker;
    Unit defender;
    List<Unit> supportAttackers; 
    List<Unit> supportDefenders;

    int GetEffectiveForcePercentage(Unit unit, bool asMainDefender) {
      return 100;
    }

    public void SetGaleVantage(Unit unit, Unit target, UnitPredict predict) {
      if (Cons.IsGale(hexMap.windGenerator.current)) {
        if (hexMap.windGenerator.direction == Cons.Direction.dueNorth) {
          if(Util.eq<Tile>(target.tile.SouthTile<Tile>(), unit.tile)) {
            predict.windDisadvantage = true;
          } else if(Util.eq<Tile>(target.tile.NorthTile<Tile>(), unit.tile)) {
            predict.windAdvantage = true;
          }
        }

        if (hexMap.windGenerator.direction == Cons.Direction.northEast) {
          if(Util.eq<Tile>(target.tile.SouthWestTile<Tile>(), unit.tile)) {
            predict.windDisadvantage = true;
          } else if(Util.eq<Tile>(target.tile.NorthEastTile<Tile>(), unit.tile)) {
            predict.windAdvantage = true;
          }
        }

        if (hexMap.windGenerator.direction == Cons.Direction.northWest) {
          if(Util.eq<Tile>(target.tile.SouthEastTile<Tile>(), unit.tile)) {
            predict.windDisadvantage = true;
          } else if(Util.eq<Tile>(target.tile.NorthWestTile<Tile>(), unit.tile)) {
            predict.windAdvantage = true;
          }
        }

        if (hexMap.windGenerator.direction == Cons.Direction.dueSouth) {
          if(Util.eq<Tile>(target.tile.NorthTile<Tile>(), unit.tile)) {
            predict.windDisadvantage = true;
          } else if(Util.eq<Tile>(target.tile.SouthTile<Tile>(), unit.tile)) {
            predict.windAdvantage = true;
          }
        }

        if (hexMap.windGenerator.direction == Cons.Direction.southEast) {
          if(Util.eq<Tile>(target.tile.NorthWestTile<Tile>(), unit.tile)) {
            predict.windDisadvantage = true;
          } else if(Util.eq<Tile>(target.tile.SouthEastTile<Tile>(), unit.tile)) {
            predict.windAdvantage = true;
          }
        }

        if (hexMap.windGenerator.direction == Cons.Direction.southWest) {
          if(Util.eq<Tile>(target.tile.NorthEastTile<Tile>(), unit.tile)) {
            predict.windDisadvantage = true;
          } else if(Util.eq<Tile>(target.tile.SouthWestTile<Tile>(), unit.tile)) {
            predict.windAdvantage = true;
          }
        }
      }

      if (predict.windAdvantage) {
        predict.percentOfEffectiveForce += 30;
      } else if (predict.windDisadvantage) {
        predict.percentOfEffectiveForce -= 20;
      }
    }

    int AttackerPoint(Unit unit, Settlement targetSettlement) {
      if (targetSettlement != null && unit.rf.general.Has(Cons.breacher)) {
        return (int)(unit.unitCombatPoint * 1.5f);
      }

      return unit.unitCombatPoint;
    }

    OperationPredict predict;
    public OperationPredict StartOperation(Unit attacker, Unit targetUnit, Settlement targetSettlement) {
      if (!attacker.CanAttack()) {
        // no enough stamina, can not start operation
        return null;
      }
      OperationPredict predict = new OperationPredict();
      if (targetSettlement != null) {
        targetUnit = this.defender = targetSettlement.garrison[0];
      }

      start = true;
      hexMap.unitSelectionPanel.ToggleButtons(true, attacker);
      this.attacker = attacker;
      this.defender = targetUnit;
      supportAttackers = new List<Unit>();
      supportDefenders = new List<Unit>();

      foreach (Tile tile in targetUnit.tile.neighbours) {
        Unit u = tile.GetUnit();
        if (u == null) {
          continue;
        }

        if (u.IsAI() == attacker.IsAI() && !Util.eq<Unit>(u, attacker) && u.CanAttack()) {
          supportAttackers.Add(u);
        }

        if (u.IsAI() != attacker.IsAI() && !Util.eq<Unit>(u, attacker)) {
          supportDefenders.Add(u);
        }
      }

      UnitPredict unitPredict = new UnitPredict();
      unitPredict.unit = attacker;
      unitPredict.percentOfEffectiveForce = GetEffectiveForcePercentage(attacker, false);
      unitPredict.joinPossibility = 100;
      SetGaleVantage(attacker, defender, unitPredict);
      unitPredict.operationPoint = attacker.IsCamping() ?
        attacker.unitCampingAttackCombatPoint : AttackerPoint(attacker, targetSettlement);
      predict.attackers.Add(unitPredict);
      predict.attackerOptimPoints += unitPredict.operationPoint;
      hexMap.ShowAttackArrow(attacker, targetUnit, unitPredict);

      foreach(Unit unit in supportAttackers) {
        unitPredict = new UnitPredict();
        unitPredict.unit = unit;
        unitPredict.percentOfEffectiveForce = GetEffectiveForcePercentage(unit, false);
        SetGaleVantage(unit, defender, unitPredict);
        unitPredict.joinPossibility = JoinPossibility(unit, true);
        unitPredict.operationPoint = AttackerPoint(unit, targetSettlement);
        predict.attackers.Add(unitPredict);
        predict.attackerOptimPoints += unitPredict.operationPoint;
      }

      // defenders
      unitPredict = new UnitPredict();
      unitPredict.unit = defender;
      unitPredict.percentOfEffectiveForce = GetEffectiveForcePercentage(defender, true);
      unitPredict.joinPossibility = 100;
      unitPredict.operationPoint = targetSettlement != null ? targetSettlement.GetDefendForce() : defender.unitCombatPoint;
      predict.defenders.Add(unitPredict);
      predict.defenderOptimPoints += unitPredict.operationPoint;
      if (targetSettlement == null) {
        hexMap.ShowDefenderStat(defender, unitPredict);
      }
      if (targetSettlement != null) {
        foreach(Unit unit in targetSettlement.garrison) {
          if (Util.eq<Unit>(unit, defender)) {
            continue;
          }

          unitPredict = new UnitPredict();
          unitPredict.unit = unit;
          unitPredict.percentOfEffectiveForce = GetEffectiveForcePercentage(unit, true);
          unitPredict.joinPossibility = 100;
          unitPredict.operationPoint = 0;
          predict.defenders.Add(unitPredict);
        }
      }

      foreach(Unit unit in supportDefenders) {
        unitPredict = new UnitPredict();
        unitPredict.unit = unit;
        unitPredict.percentOfEffectiveForce = GetEffectiveForcePercentage(unit, false);
        unitPredict.joinPossibility = JoinPossibility(unit, false);
        unitPredict.operationPoint = unit.unitCombatPoint;
        predict.defenders.Add(unitPredict);
        predict.defenderOptimPoints += unitPredict.operationPoint;
      }

      CalculateWinChance(predict);
      bool atkWin = predict.sugguestedResult.chance == 10;
      Unit initiator = atkWin ? defender : attacker; 
      List<UnitPredict> ups = atkWin ? predict.defenders : predict.attackers;
      foreach (UnitPredict up in ups) {
        if (Util.eq<Unit>(up.unit, defender) || Util.eq<Unit>(up.unit, attacker)) {
          continue;
        }
        int pos = JoinPossibilityBaseOnOdds(up.unit, predict.suggestedResultType);
        up.joinPossibility = pos < up.joinPossibility ? pos : up.joinPossibility;
      }

      foreach(UnitPredict up in predict.attackers) {
        if (Util.eq<Unit>(up.unit, attacker)) {
          continue;
        }
        hexMap.ShowAttackArrow(up.unit, targetUnit, up);
      }

      foreach(UnitPredict up in predict.defenders) {
        if (Util.eq<Unit>(up.unit, defender) || up.unit.IsCamping()) {
          continue;
        }
        hexMap.ShowDefendArrow(up.unit, targetUnit, unitPredict);
      }

      this.predict = predict;
      return predict;
    }

    void CalculateWinChance(OperationPredict predict) {
      int bigger = predict.attackerOptimPoints > predict.defenderOptimPoints ? predict.attackerOptimPoints : predict.defenderOptimPoints;
      int smaller = predict.attackerOptimPoints > predict.defenderOptimPoints ? predict.defenderOptimPoints : predict.attackerOptimPoints;
      smaller = smaller < 1 ? 1 : smaller;
      float odds = bigger / smaller;
      if (odds <= 1.5f) {
        predict.suggestedResultType = ResultType.Close;
      } else if (odds <= 3f) {
        // 1.5x - 3x
        predict.suggestedResultType = ResultType.Small;
      } else if (odds <= 4.5f) {
        predict.suggestedResultType = ResultType.Great;
      } else {
        predict.suggestedResultType = ResultType.Crushing;
      }
      if (predict.attackerOptimPoints > predict.defenderOptimPoints) {
        predict.sugguestedResult = new OperationGeneralResult(10);
      } else {
        predict.sugguestedResult = new OperationGeneralResult(0);
      }
    }

    int JoinPossibility(Unit unit, bool attacker) {
      int ret = 100;
      bool inRange = unit.InCommanderRange();
      if (!attacker && Cons.IsMist(hexMap.weatherGenerator.currentWeather)) {
        if (inRange && unit.MyCommander().Has(Cons.masterOfMist)) {
          ret = 100;
        } else {
          ret = 60;
        }
      }

      if (attacker && unit.MyCommander().Has(Cons.outOfOrder)) {
        ret = ret < 50 ? ret : 50;
      }

      // if the target unit is the one hated
      Unit target = attacker ? this.attacker : this.defender;
      if (unit.rf.province.region.GetConflictRegions().Contains(target.rf.province.region)) {
        ret = ret < 80 ? ret : 80;
      }

      return ret;
    }

    int JoinPossibilityBaseOnOdds(Unit unit, ResultType result) {
      bool inRange = unit.InCommanderRange();
      if (unit.rf.general.Has(Cons.attender) || 
        (inRange && unit.MyCommander().Has(Cons.obey))) {
        return 100;
      }

      if (result == ResultType.Close) {
        return inRange ? 100 : 90;
      }

      // when odds is greater than close
      if (unit.rf.general.Has(Cons.opportunist)) {
        return 50;
      }

      if (result == ResultType.Small) {
        return inRange ? 90 : 80;
      }

      if (result == ResultType.Great) {
        return inRange ? 80 : 65;
      }

      return inRange ? 65 : 50;
    }

    public void CancelOperation() {
      if (start) {
        start = false;
        hexMap.CleanLines();
      }
    }

    void Helper(UnitPredict up, int dead) {
      Unit unit = up.unit;
      up.dead += dead;
      unit.kia += dead;
      unit.rf.soldiers -= dead;
    }

    int AllocateCasualty(int total, List<UnitPredict> units) {
      foreach(UnitPredict up in units) {
        up.leastNum = (int)(up.unit.rf.soldiers * (up.unit.IsCavalry() ? 0.2f : 0.12f)); 
      }

      bool dieMore = false; 
      while (total > 0) {
        int dryUnits = 0;
        foreach(UnitPredict up in units) {
          Unit unit = up.unit;
          dieMore = Util.eq<Unit>(unit, attacker) || Util.eq<Unit>(unit, defender);

          if (unit.rf.soldiers <= up.leastNum) {
            dryUnits++;
            continue;
          }

          if (total < 20) {
            if (unit.rf.soldiers < total) {
              total -= unit.rf.soldiers;
              up.dead += unit.rf.soldiers;
              unit.kia += unit.rf.soldiers;
              unit.rf.soldiers = 0;
            } else {
              up.dead += total;
              unit.kia += total;
              unit.rf.soldiers -= total;
              total = 0;
              break;
            }
            dryUnits++;
            continue;
          }

          if (unit.rf.soldiers < 20) {
            total -= unit.rf.soldiers;
            up.dead += unit.rf.soldiers;
            unit.kia += unit.rf.soldiers;
            unit.rf.soldiers = 0;
            dryUnits++;
            continue;
          }

          // rookie
          int toll = 0;
          if (unit.rf.rank.Level() == 1) {
            if (!unit.IsCavalry()) {
              toll = dieMore ? 40 : 20;
              total -= toll;
              Helper(up, toll);
            } else {
              toll = dieMore ? 16 : 8;
              total -= toll;
              Helper(up, toll);
            }
          }

          // veteran
          if (unit.rf.rank.Level() == 2) {
            if (!unit.IsCavalry()) {
              toll = dieMore ? 20 : 10;
              total -= toll;
              Helper(up, toll);
            } else {
              toll = dieMore ? 10 : 5;
              total -= toll;
              Helper(up, toll);
            }
          }

          // elite
          if (unit.rf.rank.Level() == 3) {
            if (!unit.IsCavalry()) {
              toll = dieMore ? 10 : 5;
              total -= toll;
              Helper(up, toll);
            } else {
              toll = dieMore ? 6 : 3;
              total -= toll;
              Helper(up, toll);
            }
          }
        }
        if (dryUnits == units.Count) {
          break;
        }
      }
      return total;
    }

    public enum ResultType {
      FeintDefeat,
      Close,
      Small,
      Great,
      Crushing
    }

    int[] GetVicBuf(ResultType type) {
      if (type == ResultType.FeintDefeat) {
        return new int[]{0, 0};
      }
      if (type == ResultType.Close) {
        return new int[]{0, 0};
      }
      if (type == ResultType.Small) {
        return new int[]{1, 0};
      }
      if (type == ResultType.Great) {
        return new int[]{2, 1};
      }
      return new int[]{3, 2};
    }

    // initiatorMorale, supporterMorale, initiatorDiscontent
    int[] GetDftBuf(ResultType type) {
      if (type == ResultType.FeintDefeat) {
        return new int[]{0, 0};
      }
      if (type == ResultType.Close) {
        return new int[]{-2, -1};
      }
      if (type == ResultType.Small) {
        return new int[]{-5, -3};
      }
      if (type == ResultType.Great) {
        return new int[]{-6, -4};
      }
      return new int[]{-10, -8};
    }

    public bool commenceOpAnimating = false;
    public void CommenceOperation() {
      if (!start) {
        return;
      }
      commenceOpAnimating = true;
      StartCoroutine(CoCommenceOperation());
    }

    IEnumerator CoCommenceOperation() {
      hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(attacker.tile).transform.position);
      while (hexMap.cameraKeyboardController.fixingCamera) { yield return null; }
      hexMap.eventDialogAlt.ShowOperationEvent(predict, !attacker.IsAI());
      while (hexMap.eventDialogAlt.Animating) { yield return null; };
      if (hexMap.eventDialogAlt.btn1Clicked) {
      // TODO: uncomment
      //if (!attacker.IsAI()) {
        hexMap.mouseController.EscapeOnOperationCommence();
      //}
        int defenderTotal = 0;
        int attackerTotal = 0;
        int attackerInfTotal = 0;
        int attackerCavTotal = 0;
        int defenderInfTotal = 0;
        int defenderCavTotal = 0;

        List<UnitPredict> newAttackers = new List<UnitPredict>();
        List<UnitPredict> newDefenders = new List<UnitPredict>();
        List<Unit> giveupAttackers = new List<Unit>();
        List<Unit> giveupDefenders = new List<Unit>();

        foreach (UnitPredict u in predict.attackers) {
          if (u.joinPossibility >= Util.Rand(0, 100)) {
            newAttackers.Add(u);
            u.unit.UseAtmpt(); 
          } else {
            giveupAttackers.Add(u.unit);
          }
        }
        predict.attackers = newAttackers;
        foreach (UnitPredict u in predict.defenders) {
          if (u.joinPossibility >= Util.Rand(0, 100)) {
            newDefenders.Add(u);
          } else {
            giveupDefenders.Add(u.unit);
          }
        }
        predict.defenders = newDefenders;
        predict.attackerOptimPoints = predict.defenderOptimPoints = 0;

        foreach (UnitPredict u in predict.attackers) {
          attackerTotal += u.unit.rf.soldiers;
          if (u.unit.IsCavalry()) {
            attackerCavTotal += u.unit.rf.soldiers;
          } else {
            attackerInfTotal += u.unit.rf.soldiers;
          }
          predict.attackerOptimPoints += u.operationPoint;
        }

        foreach (UnitPredict u in predict.defenders) {
          defenderTotal += u.unit.rf.soldiers;
          if (u.unit.IsCavalry()) {
            defenderCavTotal += u.unit.rf.soldiers;
          } else {
            defenderInfTotal += u.unit.rf.soldiers;
          }
          predict.defenderOptimPoints += u.operationPoint;
        }
        hexMap.CleanLines();
        foreach(UnitPredict up in newAttackers) {
          hexMap.ShowAttackArrow(up.unit, defender, up);
        }
        foreach(Unit unit in giveupAttackers) {
          hexMap.popAniController.Show(hexMap.GetUnitView(unit), 
            Cons.GetTextLib().get("pop_notJoinOperation"),
            Color.white);
          while (hexMap.popAniController.Animating) { yield return null; }
        }

        foreach(UnitPredict up in newDefenders) {
          if (!up.unit.IsCamping()) {
            if (Util.eq<Unit>(up.unit, defender)) {
              hexMap.ShowDefenderStat(defender, up);
            } else {
              hexMap.ShowDefendArrow(up.unit, defender, up);
            }
          }
        }
        foreach(Unit unit in giveupDefenders) {
          hexMap.popAniController.Show(hexMap.GetUnitView(unit), 
            Cons.GetTextLib().get("pop_notJoinOperation"),
            Color.white);
          while (hexMap.popAniController.Animating) { yield return null; }
        }

        if (!defender.IsCamping() && predict.defenders.Count == 1) {
          bool noRetreat = true;
          foreach (Tile t in defender.tile.neighbours) {
            if (t.Deployable(defender)) {
              noRetreat = false;
              break;
            }
          }

          if (noRetreat && !defender.IsVulnerable()
            && !defender.IsWarWeary()
            && (predict.attackerOptimPoints > (int)(predict.defenderOptimPoints * 1.5f))
            && (defender.rf.general.Has(Cons.formidable) || Cons.FairChance())) {
            predict.defenderOptimPoints = predict.defenderOptimPoints * 3;
            hexMap.dialogue.ShowNoRetreatEvent(defender);
            while (hexMap.dialogue.Animating) { yield return null; }
            hexMap.popAniController.Show(hexMap.GetUnitView(defender), 
              Cons.GetTextLib().get("pop_noRetreatBuf"),
              Color.green);
            while (hexMap.popAniController.Animating) { yield return null; }
          }
        }
        CalculateWinChance(predict);

        // combat starts
        int attackerCasualty = 0;
        int defenderCasualty = 0;
        predict.attackerOptimPoints = predict.attackerOptimPoints <= 0 ? 1 : predict.attackerOptimPoints;
        predict.defenderOptimPoints = predict.defenderOptimPoints <= 0 ? 1 : predict.defenderOptimPoints;
        bool atkWin = predict.sugguestedResult.chance == 10;
        if (!atkWin && attacker.rf.general.Has(Cons.feintDefeat) && Cons.FiftyFifty()) {
          // feint defeat triggered
          predict.suggestedResultType = ResultType.FeintDefeat;
          hexMap.turnController.ShowTitle(Cons.GetTextLib().get("title_feintDefeat"), Color.white);
          while(hexMap.turnController.showingTitle) { yield return null; }
        }

        ResultType resultLevel = predict.suggestedResultType;
        bool feint = resultLevel == ResultType.FeintDefeat;
        if (resultLevel == ResultType.FeintDefeat) {
          attackerCasualty = (int)(attackerTotal * 0.008f);
          defenderCasualty = (int)(attackerCasualty * 0.8f);
        }
        if (resultLevel == ResultType.Close) {
          // 1 - 1.5x odds
          float m = 0.025f;
          if (Cons.SlimChance()) {
            m = 0.03f;
          }
          if (atkWin) {
            defenderCasualty = (int)(defenderTotal * m);
          } else {
            attackerCasualty = (int)(attackerTotal * m);
          }

          if (atkWin) {
            attackerCasualty = (int)(defenderCasualty * 0.8f);
          } else {
            defenderCasualty = (int)(attackerCasualty * 0.8f);
          }
        } else {
          float factor = 0.05f;
          if (resultLevel == ResultType.Great) {
            factor = 0.08f;
          }
          if (resultLevel == ResultType.Crushing) {
            factor = 0.01f * Util.Rand(40, 50);
          }

          if (atkWin) {
            defenderCasualty = (int)(defenderTotal * factor);
          } else {
            attackerCasualty = (int)(attackerTotal * factor);
          }

          if (resultLevel == ResultType.Small) {
            // 2x - 3x
            int modifier = Util.Rand(5, 6);
            if (atkWin) {
              attackerCasualty = (int)(defenderCasualty * modifier * 0.1f);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier * 0.1f);
            }
          }

          if (resultLevel == ResultType.Great) {
            // 3x - 4x
            int modifier = Util.Rand(4, 5);
            if (atkWin) {
              attackerCasualty = (int)(defenderCasualty * modifier* 0.1f);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier * 0.1f);
            }
          }

          if (resultLevel == ResultType.Crushing) {
            // 3.9x - 6x odds
            int modifier = Util.Rand(1, 2);
            if (atkWin) {
              attackerCasualty = (int)(defenderCasualty * modifier * 0.1f);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier * 0.1f);
            }
          }
        }

        attackerCasualty = attackerCasualty > attackerTotal ? attackerTotal : attackerCasualty;
        defenderCasualty = defenderCasualty > defenderTotal ? defenderTotal : defenderCasualty;

        defenderCasualty = defenderCasualty - AllocateCasualty(defenderCasualty, predict.defenders);
        attackerCasualty = attackerCasualty - AllocateCasualty(attackerCasualty, predict.attackers);

        List<UnitPredict> all = new List<UnitPredict>();
        int attackerInfDead = 0;
        int attackerCavDead = 0;
        int defenderInfDead = 0;
        int defenderCavDead = 0;

        foreach(UnitPredict up in predict.attackers) {
          up.unit.movementRemaining -= Unit.ActionCost;
          if (up.unit.IsCavalry()) {
            attackerCavDead += up.dead;
          } else {
            attackerInfDead += up.dead;
          }
          all.Add(up);
        }

        foreach(UnitPredict up in predict.defenders) {
          up.unit.movementRemaining -= Unit.DefenceCost;
          if (up.unit.IsCavalry()) {
            defenderCavDead += up.dead;
          } else {
            defenderInfDead += up.dead;
          }
          all.Add(up);
        }

        int settlementDead = 0;
        foreach(UnitPredict up in all) {
          Unit unit = up.unit;
          if (up.operationPoint == 0 ||
            (defender.IsCamping() && Util.eq<Unit>(defender, up.unit))) {
            settlementDead += up.dead;
          } else {
            View view = null;
            if (unit.IsCamping()) {
              view = hexMap.settlementMgr.GetView(unit.tile.settlement);
            }
            // morale, movement, killed, attack, def
            int[] stats = new int[]{0,0,up.dead,0,0};
            hexMap.unitAniController.ShowEffect(unit, stats, view, true);
          }
        }
        hexMap.turnController.Sleep(1);
        while(hexMap.turnController.sleeping) { yield return null; }

        foreach(UnitPredict up in all) {
          Unit unit = up.unit;
          if (unit.rf.soldiers <= Unit.DisbandUnitUnder) {
            // unit disbanded
            hexMap.unitAniController.DestroyUnit(unit, DestroyType.ByDisband);
            while (hexMap.unitAniController.DestroyAnimating) { yield return null; }
          }
        }

        if (defender.IsCamping()) {
            int[] stats = new int[]{0,0,settlementDead,0,0};
            hexMap.unitAniController.ShowEffect(null, stats, hexMap.settlementMgr.GetView(defender.tile.settlement));
            while (hexMap.unitAniController.ShowAnimating) { yield return null; }
        }

        int capturedHorse = (int)((atkWin ? defenderCavDead : attackerCavDead) * 0.2f);
        hexMap.CaptureHorse(atkWin ? attacker : defender, capturedHorse);

        //hexMap.eventDialogAlt.ShowOperationResult(resultLevel, atkWin, !attacker.IsAI(),
        //  attackerInfTotal, attackerCavTotal, defenderInfTotal, defenderCavTotal,
        //  attackerInfDead, attackerCavDead, defenderInfDead,  defenderCavDead, capturedHorse);
        //while(hexMap.eventDialogAlt.Animating) { yield return null; }
        CancelOperation();

        // Aftermath
        int[] vicBuf = GetVicBuf(resultLevel);
        int[] dftBuf = GetDftBuf(resultLevel);
        if (atkWin) {
          if (!defender.IsGone() && defender.rf.general.Has(Cons.easyTarget) && Cons.SlimChance()) {
            hexMap.unitAniController.DestroyUnit(defender, DestroyType.ByDisband, true);
            while (hexMap.unitAniController.DestroyAnimating) { yield return null; }
          }
          int morale = vicBuf[0];
          int morale1 = vicBuf[1];
          View view = null;
          foreach (UnitPredict up in predict.attackers) {
            Unit unit = up.unit;
            if (unit.IsGone()) {
              continue;
            }
            int[] stats = new int[]{0,0,0,0,0};
            if (Util.eq<Unit>(unit, attacker)) {
              unit.rf.morale += morale;
              stats[0] = morale;
            } else {
              unit.rf.morale += morale1;
              stats[0] = morale1;
            }

            view = null;
            if (unit.IsCamping()) {
              view = hexMap.settlementMgr.GetView(unit.tile.settlement);
            }
            hexMap.unitAniController.ShowEffect(unit, stats, view, true);
          }

          morale = dftBuf[0];
          morale1 = dftBuf[1];
          foreach (UnitPredict up in predict.defenders) {
            Unit unit = up.unit;
            if (unit.IsGone()) {
              continue;
            }
            int[] stats = new int[]{0,0,0,0,0};
            if (Util.eq<Unit>(unit, defender)) {
              unit.rf.morale += morale;
              stats[0] = morale;
            } else {
              unit.rf.morale += morale1;
              stats[0] = morale1;
            }

            view = null;
            if (unit.IsCamping()) {
              view = hexMap.settlementMgr.GetView(unit.tile.settlement);
            }
            hexMap.unitAniController.ShowEffect(unit, stats, view, true);
          }
          hexMap.turnController.Sleep(1);
          while(hexMap.turnController.sleeping) { yield return null; }
        } else {
          if (!attacker.IsGone() && attacker.rf.general.Has(Cons.easyTarget) && Cons.SlimChance()) {
            hexMap.unitAniController.DestroyUnit(attacker, DestroyType.ByDisband, true);
            while (hexMap.unitAniController.DestroyAnimating) { yield return null; }
          }
          // defender win
          int morale = vicBuf[0];
          int morale1 = vicBuf[1];
          View view = null;
          foreach (UnitPredict up in predict.defenders) {
            Unit unit = up.unit;
            if (unit.IsGone()) {
              continue;
            }
            int[] stats = new int[]{0,0,0,0,0};
            if (Util.eq<Unit>(unit, defender)) {
              unit.rf.morale += morale;
              stats[0] = morale;
            } else {
              unit.rf.morale += morale1;
              stats[0] = morale1;
            }

            view = null;
            if (unit.IsCamping()) {
              view = hexMap.settlementMgr.GetView(unit.tile.settlement);
            }
            hexMap.unitAniController.ShowEffect(unit, stats, view, true);
          }

          morale = dftBuf[0];
          morale1 = dftBuf[1];
          if (!feint) {
            foreach (UnitPredict up in predict.attackers) {
              Unit unit = up.unit;
              if (unit.IsGone()) {
                continue;
              }
              int[] stats = new int[]{0,0,0,0,0};
              if (Util.eq<Unit>(unit, attacker)) {
                unit.rf.morale += morale;
                stats[0] = morale;
              } else {
                unit.rf.morale += morale1;
                stats[0] = morale1;
              }

              view = null;
              if (unit.IsCamping()) {
                view = hexMap.settlementMgr.GetView(unit.tile.settlement);
              }
              hexMap.unitAniController.ShowEffect(unit, stats, view, true);
            }
          }

          hexMap.turnController.Sleep(1);
          while(hexMap.turnController.sleeping) { yield return null; }
        }

        int deadToll = attackerInfDead + attackerCavDead + defenderInfDead + defenderCavDead;
        Tile tile = atkWin ? defender.tile : attacker.tile;
        if (tile.settlement == null) {
          tile.deadZone.Occur(deadToll);
        }

        Unit loser = atkWin ? defender : attacker;
        List<Unit> supporters = new List<Unit>();
        HashSet<Unit> chasers = new HashSet<Unit>();
        foreach(UnitPredict up in atkWin ? predict.defenders : predict.attackers) {
          if (!up.unit.IsGone()) {
            supporters.Add(up.unit);
          }
        }

        bool cunningLoser = loser.rf.general.Has(Cons.feintDefeat);
        foreach(UnitPredict up in atkWin ? predict.attackers : predict.defenders) {
          if (!up.unit.IsGone() && up.unit.rf.general.Has(Cons.forwarder) && Cons.MostLikely()) {
            chasers.Add(up.unit);
          }
        }

        Unit un = atkWin ? attacker : defender;
        if ((cunningLoser && Cons.EvenChance())
          || (!un.rf.general.Has(Cons.playSafe) && Cons.SlimChance())) {
          chasers.Add(un);
        }

        List<Unit> retreaters = new List<Unit>();
        foreach(Unit u in hexMap.GetWarParty(loser).GetUnits()) {
          if (u.RetreatOnDefeat() && !supporters.Contains(u)) {
            retreaters.Add(u);
          }
        }

        // affected all allies
        if (loser.IsCommander() && !feint) {
          int drop = 0;
          if (resultLevel == ResultType.Great) {
            drop = -8;
          }
          if (resultLevel == ResultType.Crushing) {
            drop = -10;
          }
          if (drop != 0) {
            hexMap.unitAniController.ShakeNearbyAllies(loser, drop);
            while (hexMap.unitAniController.ShakeAnimating) { yield return null; }
          }
        }

        HashSet<Unit> geese = new HashSet<Unit>();
        List<Unit> gonnaStick = new List<Unit>();
        List<Unit> gonnaMove = new List<Unit>();
        List<Unit> failedToMove = new List<Unit>();
        if (resultLevel != ResultType.Close) {
          foreach(Unit unit in supporters) {
            bool notMoving = unit.StickAsNailWhenDefeat();
            if (notMoving && !feint) {
              gonnaStick.Add(unit);
            } else {
              gonnaMove.Add(unit);
            }
          }
          if (gonnaMove.Count > 2 && !feint) {
            if (resultLevel == ResultType.Crushing) {
              hexMap.turnController.ShowTitle(Cons.GetTextLib().get("title_formationCrushing"), Color.red);
            } else {
              hexMap.turnController.ShowTitle(Cons.GetTextLib().get("title_formationBreaking"), Color.red);
            }
            while(hexMap.turnController.showingTitle) { yield return null; }
            hexMap.dialogue.ShowFormationBreaking(atkWin ? attacker : defender);
            while(hexMap.dialogue.Animating) { yield return null; }
            hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetUnitView(loser).transform.position);
            while(hexMap.cameraKeyboardController.fixingCamera) { yield return null; }
          }

          if (feint) {
            hexMap.dialogue.ShowFeintDefeat(loser);
            while(hexMap.dialogue.Animating) { yield return null; }
          }
          if (gonnaMove.Count > 0) {
            hexMap.unitAniController.Scatter(gonnaMove, failedToMove, feint ? 0 : -10);
            while(hexMap.unitAniController.ScatterAnimating) { yield return null; }
          }

          foreach (Unit unit in gonnaMove) {
            if (resultLevel == ResultType.Crushing) {
              unit.chaos = true;
            } else if (resultLevel == ResultType.Great) {
              unit.defeating = true;
            }

            if (!failedToMove.Contains(unit)) {
              geese.Add(unit);
            }
          }

          if (resultLevel == ResultType.Small && gonnaMove.Contains(loser)) {
            loser.defeating = true;
          }

          foreach(Unit unit in supporters) {
            if (!unit.Rout()) {
              unit.defeating = unit.chaos = false;
            }
          }

          if (!feint) {
            // conservative generals retreat
            foreach(Unit u in retreaters) {
              if (u.SetRetreatPath()) {
                hexMap.unitAniController.ForceRetreat(u);
                while(hexMap.unitAniController.ForceRetreatAnimating) { yield return null; }
              }
            }
          }

          if (atkWin && loser.IsCamping() &&
            (resultLevel == ResultType.Crushing || resultLevel == ResultType.Great)) {
            // settlement loss
            hexMap.unitAniController.TakeSettlement(attacker, loser.tile.settlement);
            while (hexMap.unitAniController.TakeAnimating) { yield return null; }
          }
        } else {
          bool notGonnaMove = loser.StickAsNailWhenDefeat();
          if (Cons.FiftyFifty() && !notGonnaMove) {
            loser.defeating = loser.Rout();
          }
          if (!loser.IsCamping() && !notGonnaMove) {
            hexMap.unitAniController.Scatter(new List<Unit>(){loser}, failedToMove, -10);
            while (hexMap.unitAniController.ScatterAnimating) { yield return null; }
            if (!failedToMove.Contains(loser)) {
              geese.Add(loser);
            }
          }
        }

        // goose chase
        HashSet<Unit> occupied = new HashSet<Unit>();
        foreach(Unit u in chasers) {
          Tile[] path = new Tile[]{};
          foreach(Unit goose in geese) {
            foreach(Tile t in goose.tile.neighbours) {
              if (!t.Deployable(u)) {
                continue;
              }

              path = u.FindPath(t);
              if (path.Length > 0) {
                break;
              }
            }
            if (path.Length > 0) {
              break;
            }
          }
          if (path.Length > 0) {
            hexMap.dialogue.ShowChaseDialogue(u);
            while (hexMap.dialogue.Animating) { yield return null; }
            hexMap.unitAniController.MoveUnit(u, path[path.Length - 1]);
            while (hexMap.unitAniController.MoveAnimating) { yield return null; }
          }
        }
      } else {
      // TODO: uncomment
      //if (!attacker.IsAI()) {
        hexMap.mouseController.EscapeOnOperationCancel();
      //}
        CancelOperation();
      }
      commenceOpAnimating = false;
    }

    public List<Unit> GetKnownEnemies() {
      List<Unit> known = new List<Unit>();
      bool isPlayer = hexMap.turnController.playerTurn;
      foreach(Tile tile in FoW.Get().GetVisibleArea()) {
        Unit unit = tile.GetUnit();
        if (unit != null && isPlayer == unit.IsAI() && unit.IsVisible()) {
          known.Add(unit);
        }
      }
      return known;
    }

  }

}