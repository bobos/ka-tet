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
    bool attackSettlement;
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

    int AttackerPoint(Unit unit) {
      int point = unit.unitCombatPoint;
      if (attackSettlement && Breacher.Aval(unit)) {
        point = (int)(point * (1f + Breacher.AtkBuf));
      }

      if (
        Cons.IsRain(hexMap.weatherGenerator.currentWeather) ||
        Cons.IsSnow(hexMap.weatherGenerator.currentWeather) ||
        Cons.IsHeat(hexMap.weatherGenerator.currentWeather)) {
        point = (int)(point * 0.8f); 
      }

      if (Cons.IsHeavyRain(hexMap.weatherGenerator.currentWeather)) {
        point = (int)(point * 0.6f); 
      }

      if (Cons.IsBlizard(hexMap.weatherGenerator.currentWeather)) {
        point = (int)(point * 0.4f); 
      }

      return point;
    }

    int DefenderPoint(Unit unit) {
      return (int)(unit.unitCombatPoint * 1.1f);
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
      attackSettlement = false;
      if (targetSettlement != null) {
        attackSettlement = true;
        targetUnit = this.defender = targetSettlement.garrison[Util.Rand(0, targetSettlement.garrison.Count-1)];
      }

      start = true;
      hexMap.unitSelectionPanel.ToggleButtons(true, attacker);
      this.attacker = attacker;
      this.defender = targetUnit;
      supportAttackers = new List<Unit>();
      supportDefenders = new List<Unit>();
      hiddenDefenders = new List<Unit>();

      List<Unit> candidateDefenders = targetUnit.OnFieldAllies();
      HashSet<Tile> attackerVision = hexMap.GetWarParty(attacker).discoveredTiles;
      if (!surprised && !Cons.IsMist(hexMap.weatherGenerator.currentWeather)) {
        foreach (Tile tile in targetUnit.tile.neighbours) {
          Unit u = tile.GetUnit();
          if (u == null) {
            continue;
          }

          if (u.IsAI() == attacker.IsAI() && !Util.eq<Unit>(u, attacker) && u.CanAttack() && !feintDefeat) {
            supportAttackers.Add(u);
          }
        }

        supportAttackers.Add(attacker);
        foreach (Unit u in supportAttackers) {
          foreach(Tile tile in u.tile.neighbours) {
            Unit u1 = tile.GetUnit();
            if (u1 == null || u1.IsAI() == attacker.IsAI() || supportDefenders.Contains(u1) || hiddenDefenders.Contains(u1) || Util.eq<Unit>(u1, defender)
            || !candidateDefenders.Contains(u1)) {
              continue;
            }
            if (!attackerVision.Contains(u1.tile)) {
              hiddenDefenders.Add(u1);
            } else {
              supportDefenders.Add(u1);
            }
          }
        }
        supportAttackers.Remove(attacker);
      }

      UnitPredict unitPredict = new UnitPredict();
      unitPredict.unit = attacker;
      unitPredict.percentOfEffectiveForce = 100;
      unitPredict.joinPossibility = 100;
      SetGaleVantage(attacker, defender, unitPredict);
      unitPredict.operationPoint = attacker.IsCamping() ?
        attacker.unitCampingAttackCombatPoint : AttackerPoint(attacker);
      predict.attackers.Add(unitPredict);
      predict.attackerOptimPoints += unitPredict.operationPoint;
      hexMap.ShowAttackArrow(attacker, targetUnit, unitPredict);

      foreach(Unit unit in supportAttackers) {
        unitPredict = new UnitPredict();
        unitPredict.unit = unit;
        unitPredict.percentOfEffectiveForce = 100;
        SetGaleVantage(unit, defender, unitPredict);
        unitPredict.joinPossibility = JoinPossibility(unit, true);
        unitPredict.operationPoint = AttackerPoint(unit);
        predict.attackers.Add(unitPredict);
        predict.attackerOptimPoints += unitPredict.operationPoint;
      }

      // defenders
      unitPredict = new UnitPredict();
      unitPredict.unit = defender;
      unitPredict.percentOfEffectiveForce = surprised ? 20 : 100;
      unitPredict.joinPossibility = 100;
      unitPredict.operationPoint = attackSettlement ? targetSettlement.GetDefendForce() : DefenderPoint(defender);
      unitPredict.operationPoint = (int)(unitPredict.percentOfEffectiveForce * 0.01f * unitPredict.operationPoint);
      predict.defenders.Add(unitPredict);
      predict.defenderOptimPoints += unitPredict.operationPoint;
      if (!attackSettlement) {
        hexMap.ShowDefenderStat(defender, unitPredict);
      } else {
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
        unitPredict.operationPoint = DefenderPoint(unit);
        predict.defenders.Add(unitPredict);
        predict.defenderOptimPoints += unitPredict.operationPoint;
      }

      foreach(Unit unit in hiddenDefenders) {
        unitPredict = new UnitPredict();
        unitPredict.unit = unit;
        unitPredict.percentOfEffectiveForce = 100;
        unitPredict.joinPossibility = JoinPossibility(unit, false);
        unitPredict.operationPoint = DefenderPoint(unit);
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
      if (odds < 2f) {
        predict.suggestedResultType = ResultType.Small;
      } else if (odds >= 2f && odds < 4f) {
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
      if (odds < 2f) {
        predict.trueSuggestedResultType = ResultType.Small;
      } else if (odds >= 2f && odds < 4f) {
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
      if (unit.fooled) {
        return 0;
      }
      // if the target unit is the one hated
      Unit target = attacker ? this.attacker : this.defender;
      if ((unit.rf.province.GetConflictProvinces().Contains(target.rf.province) ||
        !Util.eq<Region>(unit.rf.province.region, target.rf.province.region))
        && !unit.Obedient()) {
        ret = ret < 95 ? ret : 95;
      }

      if (!attacker && Cons.IsMist(hexMap.weatherGenerator.currentWeather)) {
        ret = ret < 50 ? ret : 50;
      }

      if (unit.IsWarWeary()) {
        ret -= 60;
      }

      return ret < 0 ? 0 : (ret > 100 ? 100 : ret);
    }

    public void CancelOperation() {
      if (start) {
        start = false;
        hexMap.CleanLines();
      }
    }

    int Helper(UnitPredict up, int dead) {
      Unit unit = up.unit;
      int actualDead = unit.Killed(dead);
      up.dead += actualDead;
      return dead - actualDead;
    }

    int AllocateCasualty(int total, List<UnitPredict> units) {
      int savedLives = 0;
      foreach(UnitPredict up in units) {
        up.leastNum = (int)(up.unit.rf.soldiers * (up.unit.type != Type.Infantry ? 0.2f : 0.12f)); 
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

          if (total < 30) {
            if (unit.rf.soldiers < total) {
              total -= unit.rf.soldiers;
              up.dead += unit.rf.soldiers;
              unit.Killed(unit.rf.soldiers, true);
            } else {
              int dead = unit.Killed(total);
              up.dead += dead;
              savedLives += total - dead;
              total = 0;
              break;
            }
            dryUnits++;
            continue;
          }

          if (unit.rf.soldiers < 30) {
            total -= unit.rf.soldiers;
            up.dead += unit.rf.soldiers;
            unit.Killed(unit.rf.soldiers, true);
            dryUnits++;
            continue;
          }

          // rookie
          int toll = 0;
          if (unit.type == Type.Infantry) {
            toll = dieMore ? 16 : 8;
            total -= toll;
            savedLives += Helper(up, toll);
          } else {
            toll = dieMore ? 6 : 3;
            total -= toll;
            savedLives += Helper(up, toll);
          }
        }
        if (dryUnits == units.Count) {
          break;
        }
      }
      return total + savedLives;
    }

    public enum ResultType {
      FeintDefeat,
      Small,
      Great,
      Crushing
    }

    int GetVicBuf(ResultType type) {
      if (type == ResultType.FeintDefeat) {
        return 0;
      }
      if (type == ResultType.Small) {
        return -5;
      }
      if (type == ResultType.Great) {
        return 0;
      }
      return 10;
    }

    // initiatorMorale, supporterMorale, initiatorDiscontent
    int GetDftBuf(ResultType type) {
      if (type == ResultType.FeintDefeat) {
        return 0;
      }
      if (type == ResultType.Small) {
        return -15;
      }
      if (type == ResultType.Great) {
        return -40;
      }
      return -90;
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
            if (attackSettlement && Breacher.Aval(u.unit)) {
              Breacher.Get(u.unit.rf.general).Consume();
            }
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
            if (Fortifier.Aval(u.unit)) {
              Fortifier.Get(u.unit.rf.general).Consume();
            }
          } else {
            giveupDefenders.Add(u.unit);
          }
        }
        predict.defenders = newDefenders;
        predict.attackerOptimPoints = predict.defenderOptimPoints = 0;

        foreach (UnitPredict u in predict.attackers) {
          attackerTotal += u.unit.rf.soldiers;
          if (u.unit.type == Type.Infantry) {
            attackerInfTotal += u.unit.rf.soldiers;
          } else {
            attackerCavTotal += u.unit.rf.soldiers;
          }
          predict.attackerOptimPoints += u.operationPoint;
        }

        foreach (UnitPredict u in predict.defenders) {
          defenderTotal += u.unit.rf.soldiers;
          if (u.unit.type == Type.Infantry) {
            defenderInfTotal += u.unit.rf.soldiers;
          } else {
            defenderCavTotal += u.unit.rf.soldiers;
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

          if (noRetreat && !defender.IsVulnerable() && !defender.IsWarWeary() && (defender.rf.general.Is(Cons.brave) || Cons.EvenChance())) {
            predict.defenderOptimPoints = (int)(predict.defenderOptimPoints * 3);
            hexMap.dialogue.ShowNoRetreatEvent(defender);
            while (hexMap.dialogue.Animating) { yield return null; }
            hexMap.popAniController.Show(hexMap.GetUnitView(defender), 
              Cons.GetTextLib().get("pop_noRetreatBuf"),
              Color.green);
            while (hexMap.popAniController.Animating) { yield return null; }
          }
        }
        CalculateWinChance(predict);
        if (!predict.feintDefeat) {
          string texts;
          if (predict.sugguestedResult.chance == 10) {
            // attacker win
            if (predict.suggestedResultType == ResultType.Small) {
              texts = "title_attackerSmallWin";
            } else if (predict.suggestedResultType == ResultType.Great) {
              texts = "title_attackerGreatWin";
            } else {
              texts = "title_attackerCrushingWin";
            }
          } else {
            if (predict.suggestedResultType == ResultType.Small) {
              texts = "title_attackerSmallLoss";
            } else if (predict.suggestedResultType == ResultType.Great) {
              texts = "title_attackerGreatLoss";
            } else {
              texts = "title_attackerCrushingLoss";
            }
          }
          hexMap.turnController.ShowTitle(Cons.GetTextLib().get(texts), Color.white);
          while(hexMap.turnController.showingTitle) { yield return null; }
        }

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
          attackerCasualty = (int)(attackerTotal * 0.006f);
          defenderCasualty = (int)(attackerCasualty * 0.4f);
        } else {
          float factor = 0.02f;
          if (resultLevel == ResultType.Great) {
            factor = 0.05f;
          }
          if (resultLevel == ResultType.Crushing) {
            factor = 0.15f;
          }

          if (atkWin) {
            defenderCasualty = (int)(defenderTotal * factor);
          } else {
            attackerCasualty = (int)(attackerTotal * factor);
          }

          if (resultLevel == ResultType.Small) {
            int modifier = 4;
            if (atkWin) {
              attackerCasualty = (int)(defenderCasualty * modifier * 0.15f);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier * 0.15f);
            }
          }

          if (resultLevel == ResultType.Great) {
            int modifier = 3;
            if (atkWin) {
              attackerCasualty = (int)(defenderCasualty * modifier* 0.15f);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier * 0.15f);
            }
          }

          if (resultLevel == ResultType.Crushing) {
            int modifier = Util.Rand(1, 2);
            if (atkWin) {
              attackerCasualty = (int)(defenderCasualty * modifier * 0.15f);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier * 0.15f);
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
          if (up.unit.type == Type.Infantry) {
            attackerInfDead += up.dead;
          } else {
            attackerCavDead += up.dead;
          }
          all.Add(up);
        }

        foreach(UnitPredict up in predict.defenders) {
          if (up.unit.type == Type.Infantry) {
            defenderInfDead += up.dead;
          } else {
            defenderCavDead += up.dead;
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
        }
        hexMap.turnController.Sleep(1);
        while(hexMap.turnController.sleeping) { yield return null; }

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
          if (!defender.IsGone() && defender.rf.general.Is(Cons.brave) && predict.suggestedResultType != ResultType.Small && Cons.SlimChance()) {
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
          if (!attacker.IsGone() && attacker.rf.general.Is(Cons.brave) && predict.suggestedResultType != ResultType.Small && Cons.SlimChance()) {
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
        List<Unit> supporters = new List<Unit>{loser};
        HashSet<Unit> chasers = new HashSet<Unit>();
        foreach(Unit u in loser.OnFieldAllies()) {
          supporters.Add(u);
        }

        foreach(UnitPredict up in atkWin ? predict.attackers : predict.defenders) {
          if (up.unit.IsCamping() && up.unit.tile.settlement.garrison.Count == 1) {
            continue;
          }
          if (!up.unit.IsGone() && up.unit.rf.general.Is(Cons.reckless) && Cons.MostLikely()) {
            chasers.Add(up.unit);
          }
        }

        Unit un = atkWin ? attacker : defender;
        if (feint && !un.Obedient()) {
          if (un.IsCamping() && un.tile.settlement.garrison.Count == 1) {}
          else {
            if (Deciever.Get(un.rf.general) != null) {
            } else if (Cons.MostLikely()) {
              chasers.Add(un);
            }
          }
        }

        HashSet<Unit> geese = new HashSet<Unit>();
        if (feint) {
          geese.Add(loser);
          hexMap.dialogue.ShowFeintDefeat(loser);
          while(hexMap.dialogue.Animating) { yield return null; }
        }
        if (atkWin && loser.IsCamping() &&
          (resultLevel == ResultType.Crushing || resultLevel == ResultType.Great)) {
          // settlement loss
          hexMap.unitAniController.TakeSettlement(attacker, loser.tile.settlement);
          while (hexMap.unitAniController.TakeAnimating) { yield return null; }
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