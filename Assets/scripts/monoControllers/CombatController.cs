using System.Collections.Generic;
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
    public List<UnitPredict> hiddenDefenders = new List<UnitPredict>();
    public int attackerOptimPoints = 1;
    public int defenderOptimPoints = 1;
    public int hiddenDefenderOptimPoints = 1;
    public OperationGeneralResult sugguestedResult;
    public OperationGeneralResult trueSugguestedResult;
    public CombatController.ResultType suggestedResultType;
    public CombatController.ResultType trueSuggestedResultType;
    public bool feintDefeat = false;
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
    List<Unit> hiddenDefenders;

    public void SetGaleVantage(Unit unit, Unit target, UnitPredict predict) {
      WindAdvantage advantage = unit.tile.GetGaleAdvantage(target.tile);
      if (advantage == WindAdvantage.Advantage) {
        predict.windAdvantage = true;
        predict.percentOfEffectiveForce += 50;
      } else if (advantage == WindAdvantage.Disadvantage) {
        predict.windDisadvantage = true;
        predict.percentOfEffectiveForce -= 50;
      }

      predict.percentOfEffectiveForce = predict.percentOfEffectiveForce < 0 ? 1 : predict.percentOfEffectiveForce;
    }

    int AttackerPoint(Unit unit, Settlement targetSettlement) {
      if (targetSettlement != null && unit.rf.general.Has(Cons.diminisher)) {
        return (int)(unit.unitCombatPoint * 1.5f);
      }

      return unit.unitCombatPoint;
    }

    OperationPredict predict;
    public OperationPredict StartOperation(Unit attacker, Unit targetUnit,
      Settlement targetSettlement, bool surprised = false, bool feintDefeat = false) {
      if (!attacker.CanAttack()) {
        // no enough attempt, can not start operation
        return null;
      }
      OperationPredict predict = new OperationPredict();
      predict.feintDefeat = feintDefeat;
      if (targetSettlement != null) {
        targetUnit = this.defender = targetSettlement.garrison[Util.Rand(0, targetSettlement.garrison.Count-1)];
      }

      start = true;
      hexMap.unitSelectionPanel.ToggleButtons(true, attacker);
      this.attacker = attacker;
      this.defender = targetUnit;
      supportAttackers = new List<Unit>();
      supportDefenders = new List<Unit>();
      hiddenDefenders = new List<Unit>();

      HashSet<Tile> attackerVision = hexMap.GetWarParty(attacker).GetVisibleArea();
      if (!surprised) {
        foreach (Tile tile in targetUnit.tile.neighbours) {
          Unit u = tile.GetUnit();
          if (u == null) {
            continue;
          }

          if (u.IsAI() == attacker.IsAI() && !Util.eq<Unit>(u, attacker) && u.CanAttack() && !feintDefeat) {
            supportAttackers.Add(u);
          }

          if (u.IsAI() != attacker.IsAI() && !Util.eq<Unit>(u, attacker)) {
            if (!attackerVision.Contains(u.tile)) {
              hiddenDefenders.Add(u);
            } else {
              supportDefenders.Add(u);
            }
          }
        }
      }

      UnitPredict unitPredict = new UnitPredict();
      unitPredict.unit = attacker;
      unitPredict.percentOfEffectiveForce = 100;
      unitPredict.joinPossibility = 100;
      SetGaleVantage(attacker, defender, unitPredict);
      unitPredict.operationPoint = attacker.IsCamping() ?
        attacker.unitCampingAttackCombatPoint : AttackerPoint(attacker, targetSettlement);
      unitPredict.operationPoint = (int)(unitPredict.percentOfEffectiveForce * 0.01f * unitPredict.operationPoint);
      predict.attackers.Add(unitPredict);
      predict.attackerOptimPoints += unitPredict.operationPoint;
      hexMap.ShowAttackArrow(attacker, targetUnit, unitPredict);

      foreach(Unit unit in supportAttackers) {
        unitPredict = new UnitPredict();
        unitPredict.unit = unit;
        unitPredict.percentOfEffectiveForce = 100;
        SetGaleVantage(unit, defender, unitPredict);
        unitPredict.joinPossibility = JoinPossibility(unit, true);
        unitPredict.operationPoint = AttackerPoint(unit, targetSettlement);
        unitPredict.operationPoint = (int)(unitPredict.percentOfEffectiveForce * 0.01f * unitPredict.operationPoint);
        predict.attackers.Add(unitPredict);
        predict.attackerOptimPoints += unitPredict.operationPoint;
      }

      // defenders
      unitPredict = new UnitPredict();
      unitPredict.unit = defender;
      unitPredict.percentOfEffectiveForce = surprised ? 20 : 100;
      unitPredict.joinPossibility = 100;
      unitPredict.operationPoint = targetSettlement != null ? targetSettlement.GetDefendForce() : defender.unitCombatPoint;
      unitPredict.operationPoint = (int)(unitPredict.percentOfEffectiveForce * 0.01f * unitPredict.operationPoint);
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
          unitPredict.percentOfEffectiveForce = 100;
          unitPredict.joinPossibility = 100;
          unitPredict.operationPoint = 0;
          predict.defenders.Add(unitPredict);
        }
      }

      foreach(Unit unit in supportDefenders) {
        unitPredict = new UnitPredict();
        unitPredict.unit = unit;
        unitPredict.percentOfEffectiveForce = 100;
        unitPredict.joinPossibility = JoinPossibility(unit, false);
        unitPredict.operationPoint = unit.unitCombatPoint;
        unitPredict.operationPoint = (int)(unitPredict.percentOfEffectiveForce * 0.01f * unitPredict.operationPoint);
        predict.defenders.Add(unitPredict);
        predict.defenderOptimPoints += unitPredict.operationPoint;
      }

      foreach(Unit unit in hiddenDefenders) {
        unitPredict = new UnitPredict();
        unitPredict.unit = unit;
        unitPredict.percentOfEffectiveForce = 100;
        unitPredict.joinPossibility = JoinPossibility(unit, false);
        unitPredict.operationPoint = unit.unitCombatPoint;
        unitPredict.operationPoint = (int)(unitPredict.percentOfEffectiveForce * 0.01f * unitPredict.operationPoint);
        predict.hiddenDefenders.Add(unitPredict);
        predict.hiddenDefenderOptimPoints += unitPredict.operationPoint;
      }

      CalculateWinChance(predict);
      CalculateTrueWinChance(predict);
      bool atkWin = predict.trueSugguestedResult.chance == 10;
      List<UnitPredict> allDefenders = new List<UnitPredict>(predict.defenders);
      foreach(UnitPredict up in predict.hiddenDefenders) {
        allDefenders.Add(up);
      }
      List<UnitPredict> ups = atkWin ? allDefenders : predict.attackers;
      foreach (UnitPredict up in ups) {
        if (Util.eq<Unit>(up.unit, defender) || Util.eq<Unit>(up.unit, attacker)) {
          continue;
        }
        int pos = JoinPossibilityBaseOnOdds(up.unit, predict.trueSuggestedResultType);
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

    void CalculateTrueWinChance(OperationPredict predict) {
      int defenderPoints = predict.hiddenDefenderOptimPoints + predict.defenderOptimPoints;
      int bigger = predict.attackerOptimPoints > defenderPoints ? predict.attackerOptimPoints : defenderPoints;
      int smaller = predict.attackerOptimPoints > defenderPoints ? defenderPoints : predict.attackerOptimPoints;
      smaller = smaller < 1 ? 1 : smaller;
      float odds = bigger / smaller;
      if (odds <= 1.5f) {
        predict.trueSuggestedResultType = ResultType.Close;
      } else if (odds <= 3f) {
        // 1.5x - 3x
        predict.trueSuggestedResultType = ResultType.Small;
      } else if (odds <= 4.5f) {
        predict.trueSuggestedResultType = ResultType.Great;
      } else {
        predict.trueSuggestedResultType = ResultType.Crushing;
      }
      if (predict.attackerOptimPoints > predict.defenderOptimPoints) {
        predict.trueSugguestedResult = new OperationGeneralResult(10);
      } else {
        predict.trueSugguestedResult = new OperationGeneralResult(0);
      }
    }

    int JoinPossibility(Unit unit, bool attacker) {
      int ret = 100;
      bool inRange = unit.InCommanderRange();
      // if the target unit is the one hated
      Unit target = attacker ? this.attacker : this.defender;
      if (unit.rf.province.region.GetConflictRegions().Contains(target.rf.province.region)
        && !unit.FollowOrder()) {
        ret = ret < 75 ? ret : 75;
      }

      if (attacker && !inRange) {
        ret = ret < 85 ? ret : 85;
      }

      return ret;
    }

    int JoinPossibilityBaseOnOdds(Unit unit, ResultType result) {
      bool inRange = unit.InCommanderRange();
      if (inRange && unit.MyCommander().commandSkill.ObeyMyOrder()) {
        return 100;
      }

      if (result == ResultType.Close) {
        return inRange ? 100 : 90;
      }

      // when odds is greater than close
      if (unit.rf.general.Is(Cons.cunning)) {
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
      unit.Killed(dead);
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
              unit.Killed(unit.rf.soldiers);
            } else {
              up.dead += total;
              unit.Killed(total);
              total = 0;
              break;
            }
            dryUnits++;
            continue;
          }

          if (unit.rf.soldiers < 20) {
            total -= unit.rf.soldiers;
            up.dead += unit.rf.soldiers;
            unit.Killed(unit.rf.soldiers);
            dryUnits++;
            continue;
          }

          // rookie
          int toll = 0;
          if (unit.rf.rank.Level() == 1) {
            if (!unit.IsCavalry()) {
              toll = dieMore ? 30 : 15;
              total -= toll;
              Helper(up, toll);
            } else {
              toll = dieMore ? 10 : 5;
              total -= toll;
              Helper(up, toll);
            }
          }

          // veteran
          if (unit.rf.rank.Level() == 2) {
            if (!unit.IsCavalry()) {
              toll = dieMore ? 16 : 8;
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

    int GetVicBuf(ResultType type) {
      if (type == ResultType.FeintDefeat) {
        return 0;
      }
      if (type == ResultType.Close) {
        return 0;
      }
      if (type == ResultType.Small) {
        return 0;
      }
      if (type == ResultType.Great) {
        return 1;
      }
      return 2;
    }

    // initiatorMorale, supporterMorale, initiatorDiscontent
    int GetDftBuf(ResultType type) {
      if (type == ResultType.FeintDefeat) {
        return 0;
      }
      if (type == ResultType.Close) {
        return -3;
      }
      if (type == ResultType.Small) {
        return -6;
      }
      if (type == ResultType.Great) {
        return -10;
      }
      return -15;
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

        foreach(UnitPredict up in predict.hiddenDefenders) {
          predict.defenders.Add(up);
        }
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
        bool atkWin = predict.sugguestedResult.chance == 10 && !predict.feintDefeat;
        if (predict.feintDefeat) {
          predict.suggestedResultType = ResultType.FeintDefeat;
          hexMap.turnController.ShowTitle(Cons.GetTextLib().get("title_feintDefeat"), Color.white);
          while(hexMap.turnController.showingTitle) { yield return null; }
        }

        ResultType resultLevel = predict.suggestedResultType;
        bool feint = resultLevel == ResultType.FeintDefeat;
        if (resultLevel == ResultType.FeintDefeat) {
          attackerCasualty = (int)(attackerTotal * 0.008f);
          defenderCasualty = (int)(attackerCasualty * 0.4f);
        } else if (resultLevel == ResultType.Close) {
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
            attackerCasualty = (int)(defenderCasualty * 0.45f);
          } else {
            defenderCasualty = (int)(attackerCasualty * 0.45f);
          }
        } else {
          float factor = 0.05f;
          if (resultLevel == ResultType.Great) {
            factor = 0.08f;
          }
          if (resultLevel == ResultType.Crushing) {
            factor = 0.01f * Util.Rand(35, 45);
          }

          if (atkWin) {
            defenderCasualty = (int)(defenderTotal * factor);
          } else {
            attackerCasualty = (int)(attackerTotal * factor);
          }

          if (resultLevel == ResultType.Small) {
            // 2x - 3x
            int modifier = Util.Rand(3, 4);
            if (atkWin) {
              attackerCasualty = (int)(defenderCasualty * modifier * 0.1f);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier * 0.1f);
            }
          }

          if (resultLevel == ResultType.Great) {
            // 3x - 4x
            int modifier = Util.Rand(2, 3);
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
          if (up.unit.IsCavalry()) {
            attackerCavDead += up.dead;
          } else {
            attackerInfDead += up.dead;
          }
          all.Add(up);
        }

        foreach(UnitPredict up in predict.defenders) {
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
        Unit winner = atkWin ? attacker : defender;
        hexMap.CaptureHorse(winner, capturedHorse);
        if (capturedHorse > 0) {
          View view = winner.IsCamping() ? hexMap.settlementMgr.GetView(winner.tile.settlement) : hexMap.GetUnitView(winner);
          hexMap.popAniController.Show(view, System.String.Format(Cons.textLib.get("pop_capturedHorse"), capturedHorse), Color.green);
          while (hexMap.popAniController.Animating) { yield return null; }
        }

        //hexMap.eventDialogAlt.ShowOperationResult(resultLevel, atkWin, !attacker.IsAI(),
        //  attackerInfTotal, attackerCavTotal, defenderInfTotal, defenderCavTotal,
        //  attackerInfDead, attackerCavDead, defenderInfDead,  defenderCavDead, capturedHorse);
        //while(hexMap.eventDialogAlt.Animating) { yield return null; }
        CancelOperation();

        // Aftermath
        int vicBuf = GetVicBuf(resultLevel);
        int dftBuf = GetDftBuf(resultLevel);
        if (atkWin) {
          if (!defender.IsGone() && defender.rf.general.Is(Cons.brave) && Cons.SlimChance()) {
            hexMap.unitAniController.DestroyUnit(defender, DestroyType.ByDisband, true);
            while (hexMap.unitAniController.DestroyAnimating) { yield return null; }
          }
          int morale = vicBuf;
          View view = null;
          foreach (UnitPredict up in predict.attackers) {
            Unit unit = up.unit;
            if (unit.IsGone()) {
              continue;
            }
            int[] stats = new int[]{0,0,0,0,0};
            stats[0] = unit.Victory(morale);

            view = null;
            if (unit.IsCamping()) {
              view = hexMap.settlementMgr.GetView(unit.tile.settlement);
            }
            hexMap.unitAniController.ShowEffect(unit, stats, view, true);
          }

          morale = dftBuf;
          foreach (UnitPredict up in predict.defenders) {
            Unit unit = up.unit;
            if (unit.IsGone()) {
              continue;
            }
            int[] stats = new int[]{0,0,0,0,0};
            stats[0] = unit.Defeat(morale);

            view = null;
            if (unit.IsCamping()) {
              view = hexMap.settlementMgr.GetView(unit.tile.settlement);
            }
            hexMap.unitAniController.ShowEffect(unit, stats, view, true);
          }
          hexMap.turnController.Sleep(1);
          while(hexMap.turnController.sleeping) { yield return null; }
        } else {
          if (!attacker.IsGone() && attacker.rf.general.Is(Cons.brave) && Cons.SlimChance()) {
            hexMap.unitAniController.DestroyUnit(attacker, DestroyType.ByDisband, true);
            while (hexMap.unitAniController.DestroyAnimating) { yield return null; }
          }
          // defender win
          int morale = vicBuf;
          View view = null;
          foreach (UnitPredict up in predict.defenders) {
            Unit unit = up.unit;
            if (unit.IsGone()) {
              continue;
            }
            int[] stats = new int[]{0,0,0,0,0};
            stats[0] = unit.Victory(morale);

            view = null;
            if (unit.IsCamping()) {
              view = hexMap.settlementMgr.GetView(unit.tile.settlement);
            }
            hexMap.unitAniController.ShowEffect(unit, stats, view, true);
          }

          morale = dftBuf;
          if (!feint) {
            foreach (UnitPredict up in predict.attackers) {
              Unit unit = up.unit;
              if (unit.IsGone()) {
                continue;
              }
              int[] stats = new int[]{0,0,0,0,0};
              stats[0] = unit.Defeat(morale);

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

        foreach(UnitPredict up in atkWin ? predict.attackers : predict.defenders) {
          if (!up.unit.IsGone() && up.unit.rf.general.Is(Cons.reckless) && Cons.MostLikely()) {
            chasers.Add(up.unit);
          }
        }

        Unit un = atkWin ? attacker : defender;
        if (feint && !un.FollowOrder()) {
          if (un.rf.general.Is(Cons.conservative) || un.rf.general.Has(Cons.tactic)) {
            if (Cons.SlimChance()) {
              chasers.Add(un);
            }
          } else if (Cons.MostLikely()) {
            chasers.Add(un);
          }
        }

        // affected all allies
        if (loser.IsCommander() && !feint) {
          int drop = 0;
          if (resultLevel == ResultType.Small) {
            drop = -3;
          }
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
        } else if (!feint && resultLevel != ResultType.Close) {
          hexMap.unitAniController.ShakeNearbyAllies(loser, 0);
          while (hexMap.unitAniController.ShakeAnimating) { yield return null; }
        }

        HashSet<Unit> geese = new HashSet<Unit>();
        List<Unit> gonnaMove = new List<Unit>();
        List<Unit> failedToMove = new List<Unit>();
        if (resultLevel != ResultType.Close) {
          foreach(Unit unit in supporters) {
            bool notMoving = unit.StickAsNailWhenDefeat();
            if (!notMoving || feint) {
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

          if (atkWin && loser.IsCamping() &&
            (resultLevel == ResultType.Crushing || resultLevel == ResultType.Great)) {
            // settlement loss
            hexMap.unitAniController.TakeSettlement(attacker, loser.tile.settlement);
            while (hexMap.unitAniController.TakeAnimating) { yield return null; }
          }
        } else {
          bool notGonnaMove = loser.StickAsNailWhenDefeat();
          if (Cons.FiftyFifty() && !notGonnaMove) {
            loser.defeating = true;
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
          if (u.IsCamping()) {
            Tile targetTile = null;
            foreach(Tile t in u.tile.neighbours) {
              if (t.Deployable(u)) {
                targetTile = t;
                break;
              }
            }
            if (targetTile == null) {
              continue;
            }
            u.tile.settlement.Decamp(u, targetTile);
          }
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