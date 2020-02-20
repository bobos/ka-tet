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
    public int wounded = 0;
    public int laborDead = 0;
    public int leastNum = 0;
  }

  public class OperationPredict {
    public List<UnitPredict> attackers = new List<UnitPredict>();
    public List<UnitPredict> defenders = new List<UnitPredict>();
    public int attackerOptimPoints = 0;
    public int defenderOptimPoints = 0;
    public OperationGeneralResult sugguestedResult;
  }

  public class CombatController : BaseController
  {

    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      mouseController = hexMap.mouseController;
      actionController = hexMap.actionController;
      actionController.actionDone += ActionDone;
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
      return (int)((1 + unit.GetStaminaDebuf(asMainDefender)) * 100);
    }

    void SetGaleVantage(Unit unit, Unit target, UnitPredict predict) {
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

    OperationPredict predict;
    public OperationPredict StartOperation(Unit attacker, Unit targetUnit) {
      if (attacker.GetStaminaLevel() == StaminaLvl.Exhausted) {
        // no enough stamina, can not start operation
        return null;
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

        if (u.IsAI() == attacker.IsAI() && u.GetStaminaLevel() != StaminaLvl.Exhausted
          && !Util.eq<Unit>(u, attacker)) {
          supportAttackers.Add(u);
        }

        if (u.IsAI() != attacker.IsAI() && u.GetStaminaLevel() != StaminaLvl.Exhausted
          && !Util.eq<Unit>(u, attacker)) {
          supportDefenders.Add(u);
        }
      }

      OperationPredict predict = new OperationPredict();
      UnitPredict unitPredict = new UnitPredict();
      unitPredict.unit = attacker;
      unitPredict.percentOfEffectiveForce = GetEffectiveForcePercentage(attacker, false);
      unitPredict.joinPossibility = 100;
      SetGaleVantage(attacker, defender, unitPredict);
      // TODO:tmp morale buff from commander
      unitPredict.operationPoint = (int)(attacker.unitCombatPoint * unitPredict.percentOfEffectiveForce * 0.01f);
      predict.attackers.Add(unitPredict);
      predict.attackerOptimPoints += unitPredict.operationPoint;
      hexMap.ShowAttackArrow(attacker, targetUnit, unitPredict);

      foreach(Unit unit in supportAttackers) {
        unitPredict = new UnitPredict();
        unitPredict.unit = unit;
        unitPredict.percentOfEffectiveForce = GetEffectiveForcePercentage(unit, false);
        SetGaleVantage(unit, defender, unitPredict);
        unitPredict.joinPossibility = GetJoinPossibility(unit, attacker);
        unitPredict.operationPoint = (int)(unit.unitCombatPoint * unitPredict.percentOfEffectiveForce * 0.01f);
        predict.attackers.Add(unitPredict);
        predict.attackerOptimPoints += unitPredict.operationPoint;
        hexMap.ShowAttackArrow(unit, targetUnit, unitPredict);
      }

      // defenders
      unitPredict = new UnitPredict();
      unitPredict.unit = defender;
      unitPredict.percentOfEffectiveForce = GetEffectiveForcePercentage(defender, true);
      unitPredict.joinPossibility = 100;
      unitPredict.operationPoint = (int)(defender.unitCombatPoint * unitPredict.percentOfEffectiveForce * 0.01f * (defender.IsCavalry() ? 1 : 1.5f));
      predict.defenders.Add(unitPredict);
      predict.defenderOptimPoints += unitPredict.operationPoint;
      hexMap.ShowDefenderStat(defender, unitPredict);

      foreach(Unit unit in supportDefenders) {
        unitPredict = new UnitPredict();
        unitPredict.unit = unit;
        unitPredict.percentOfEffectiveForce = GetEffectiveForcePercentage(unit, false);
        unitPredict.joinPossibility = GetJoinPossibility(unit, targetUnit);
        unitPredict.operationPoint = (int)(unit.unitCombatPoint * unitPredict.percentOfEffectiveForce * 0.01f * (unit.IsCavalry() ? 1 : 1.5f));
        predict.defenders.Add(unitPredict);
        predict.defenderOptimPoints += unitPredict.operationPoint;
        if (!Util.eq<Unit>(unit, targetUnit)) {
          hexMap.ShowDefendArrow(unit, targetUnit, unitPredict);
        }
      }

      CalculateWinChance(predict);
      this.predict = predict;
      return predict;
    }

    void CalculateWinChance(OperationPredict predict) {
      if (predict.attackerOptimPoints > (int)(predict.defenderOptimPoints * 1.14)) {
        predict.sugguestedResult = new OperationGeneralResult(10);
      } else {
        predict.sugguestedResult = new OperationGeneralResult(0);
      }
    }

    int GetJoinPossibility(Unit unit1, Unit unit2) {
      // TODO: apply commander range and general triats to adjust possibility 
      if (Util.eq<Party>(unit1.rf.general.party, unit2.rf.general.party)) {
        return 100;
      }

      Party.Relation relation = unit1.rf.general.party.GetRelation();
      if (relation == Party.Relation.normal) {
        return 100;
      }

      if (relation == Party.Relation.tense) {
        return 70;
      }

      return 50;
    }

    public void CancelOperation() {
      if (start) {
        start = false;
        hexMap.CleanLines();
      }
    }

    void Helper(UnitPredict up, int step, int maxWd) {
      Unit unit = up.unit;
      int wounded = Util.Rand(0, maxWd);
      int dead = step - wounded;
      up.wounded += wounded;
      up.dead += dead;

      unit.rf.wounded += wounded;
      unit.kia += dead;
      unit.rf.soldiers -= step;

      if (unit.labor > 1) {
        int laborDead = Util.Rand(0, 1);
        up.laborDead += laborDead;
        unit.labor -= laborDead;
      }
    }

    int AllocateCasualty(int total, List<UnitPredict> units) {
      foreach(UnitPredict up in units) {
        up.leastNum = (int)(up.unit.rf.soldiers * (up.unit.IsCavalry() ? 0.2f : 0.12f)); 
      }

      while (total > 0) {
        int dryUnits = 0;
        foreach(UnitPredict up in units) {
          Unit unit = up.unit;

          if (unit.rf.soldiers <= up.leastNum) {
            dryUnits++;
            continue;
          }

          if (total < 20) {
            if (unit.rf.soldiers < total) {
              total -= unit.rf.soldiers;
              up.dead += unit.rf.soldiers;
              up.laborDead += unit.labor;
              unit.rf.soldiers = 0;
              unit.labor = 0;
            } else {
              up.dead += total;
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
            up.laborDead += unit.labor;
            unit.rf.soldiers = 0;
            unit.labor = 0;
            dryUnits++;
            continue;
          }

          // rookie
          if (unit.rf.rank.Level() == 1) {
            if (!unit.IsCavalry()) {
              total -= 20;
              Helper(up, 20, 2);
            } else {
              total -= 8;
              Helper(up, 8, 2);
            }
          }

          // veteran
          if (unit.rf.rank.Level() == 2) {
            if (!unit.IsCavalry()) {
              total -= 10;
              Helper(up, 10, 2);
            } else {
              total -= 5;
              Helper(up, 5, 2);
            }
          }

          // elite
          if (unit.rf.rank.Level() == 3) {
            if (!unit.IsCavalry()) {
              total -= 5;
              Helper(up, 5, 2);
            } else {
              total -= 3;
              Helper(up, 3, 2);
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
      Close,
      Small,
      Great,
      Crushing
    }

    int[] GetVicBuf(ResultType type) {
      if (type == ResultType.Close) {
        return new int[]{-1, 0, 0};
      }
      if (type == ResultType.Small) {
        return new int[]{0, 0, 0};
      }
      if (type == ResultType.Great) {
        return new int[]{1, 0, -2};
      }
      return new int[]{2, 1, -3};
    }

    // initiatorMorale, supporterMorale, initiatorDiscontent
    int[] GetDftBuf(ResultType type) {
      if (type == ResultType.Close) {
        return new int[]{-20, -15, 1};
      }
      if (type == ResultType.Small) {
        return new int[]{-25, -20, 2};
      }
      if (type == ResultType.Great) {
        return new int[]{-30, -25, 4};
      }
      return new int[]{-40, -30, 8};
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
        foreach(Unit unit in giveupAttackers) {
          hexMap.popAniController.Show(hexMap.GetUnitView(unit), 
            Cons.GetTextLib().get("pop_notJoinOperation"),
            Color.white);
          while (hexMap.popAniController.Animating) { yield return null; }
        }
        foreach(UnitPredict up in newAttackers) {
          hexMap.popAniController.Show(hexMap.GetUnitView(up.unit), 
            Cons.GetTextLib().get("pop_joinOperation"),
            Color.green);
          while (hexMap.popAniController.Animating) { yield return null; }
          hexMap.ShowAttackArrow(up.unit, defender, up);
        }

        foreach(Unit unit in giveupDefenders) {
          hexMap.popAniController.Show(hexMap.GetUnitView(unit), 
            Cons.GetTextLib().get("pop_notJoinOperation"),
            Color.white);
          while (hexMap.popAniController.Animating) { yield return null; }
        }
        foreach(UnitPredict up in newDefenders) {
          hexMap.popAniController.Show(hexMap.GetUnitView(up.unit), 
            Cons.GetTextLib().get("pop_joinOperation"),
            Color.green);
          while (hexMap.popAniController.Animating) { yield return null; }
          if (Util.eq<Unit>(up.unit, defender)) {
            hexMap.ShowDefenderStat(defender, up);
          } else {
            hexMap.ShowDefendArrow(up.unit, defender, up);
          }
        }

        if (predict.defenders.Count == 1) {
          bool noRetreat = true;
          foreach (Tile t in defender.tile.neighbours) {
            if (t.Deployable(defender)) {
              noRetreat = false;
              break;
            }
          }

          // TODO: apply general trait to increase the chance
          if (noRetreat && (predict.attackerOptimPoints > (int)(predict.defenderOptimPoints * 1.5f)) && Cons.FiftyFifty()) {
            predict.defenderOptimPoints = predict.defenderOptimPoints * 2;
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
        bool attackerBigger = true;
        int factor = 0;
        ResultType resultLevel = ResultType.Close;
        predict.attackerOptimPoints = predict.attackerOptimPoints <= 0 ? 1 : predict.attackerOptimPoints;
        predict.defenderOptimPoints = predict.defenderOptimPoints <= 0 ? 1 : predict.defenderOptimPoints;

        if (predict.attackerOptimPoints > predict.defenderOptimPoints) {
          factor = (int)((predict.attackerOptimPoints / predict.defenderOptimPoints) * 10) - 10;
        } else {
          attackerBigger = false;
          factor = (int)((predict.defenderOptimPoints / predict.attackerOptimPoints) * 10) - 10;
        }
        factor = factor > 95 ? 95 : factor;

        int dice = Util.Rand(1, 10);
        bool atkWin = dice <= predict.sugguestedResult.chance;
// 1.0 to 1.3 - 0.01(both)
// 1.4 to 1.8 - def: x - 1.2, atk: (x -1) * (0.5 - 0.6)   
// 1.9 to 2.2 - atk: (x - 1.2) * (0.25 - 0.4)
        if (factor <= 1) {
          float m = 0.04f;
          float m1 = 0.035f;
          if (Cons.SlimChance()) {
            m = 0.1f;
            m = 0.095f;
          }
          defenderCasualty = (int)(defenderTotal * (attackerBigger ? m : m1));
          attackerCasualty = (int)(attackerTotal * (attackerBigger ? m1 : m));
        } else {
          if (attackerBigger) {
            defenderCasualty = (int)(defenderTotal * factor * 0.025f);
          } else {
            attackerCasualty = (int)(attackerTotal * factor * 0.025f);
          }

          if (factor > 1 && factor <= 4) {
            // 2x odds
            resultLevel = ResultType.Small;
            int modifier = Util.Rand(4, 5);
            if (attackerBigger) {
              attackerCasualty = (int)(defenderCasualty * modifier * 0.1f);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier * 0.1f);
            }
          }

          if (factor > 4 && factor <= 10) {
            // 3x odds
            int modifier = Util.Rand(3, 4);
            if (attackerBigger) {
              attackerCasualty = (int)(defenderCasualty * modifier* 0.1f);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier * 0.1f);
            }
            resultLevel = modifier > 35 ? ResultType.Small : ResultType.Great;
          }

          if (factor > 10 && factor <= 16) {
            // 4x odds
            int modifier = Util.Rand(2, 3);
            resultLevel = Cons.EvenChance() ? ResultType.Crushing : ResultType.Great;
            if (attackerBigger) {
              attackerCasualty = (int)(defenderCasualty * modifier * 0.1f);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier * 0.1f);
            }
            resultLevel = modifier > 185 ? ResultType.Great : ResultType.Crushing;
          }

          if (factor > 16 && factor <= 24) {
            // 5x odds
            float modifier = Util.Rand(1, 2) * 0.1f;
            resultLevel = ResultType.Crushing;
            if (attackerBigger) {
              attackerCasualty = (int)(defenderCasualty * modifier);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier);
            }
          }

          if (factor > 24) {
            float modifier = 0.05f;
            resultLevel = ResultType.Crushing;
            if (attackerBigger) {
              attackerCasualty = (int)(defenderCasualty * modifier);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier);
            }
          }
        }

        attackerCasualty = attackerCasualty > attackerTotal ? attackerTotal : attackerCasualty;
        defenderCasualty = defenderCasualty > defenderTotal ? defenderTotal : defenderCasualty;

        defenderCasualty = defenderCasualty - AllocateCasualty(defenderCasualty, predict.defenders);
        attackerCasualty = attackerCasualty - AllocateCasualty(attackerCasualty, predict.attackers);

        List<UnitPredict> all = new List<UnitPredict>();
        int attackerInfDead = 0;
        int attackerInfWnd = 0;
        int attackerLaborDead = 0;
        int attackerCavDead = 0;
        int attackerCavWnd = 0;

        int defenderInfDead = 0;
        int defenderInfWnd = 0;
        int defenderLaborDead = 0;
        int defenderCavDead = 0;
        int defenderCavWnd = 0;

        foreach(UnitPredict up in predict.attackers) {
          up.unit.movementRemaining -= Unit.ActionCost;
          if (up.unit.IsCavalry()) {
            attackerCavDead += up.dead;
            attackerCavWnd += up.wounded;
          } else {
            attackerInfDead += up.dead;
            attackerInfWnd += up.wounded;
            attackerLaborDead += up.laborDead;
          }
          all.Add(up);
        }

        foreach(UnitPredict up in predict.defenders) {
          up.unit.movementRemaining -= (int)(Unit.ActionCost/2);
          if (up.unit.IsCavalry()) {
            defenderCavDead += up.dead;
            defenderCavWnd += up.wounded;
          } else {
            defenderInfDead += up.dead;
            defenderInfWnd += up.wounded;
            defenderLaborDead += up.laborDead;
          }
          all.Add(up);
        }

        foreach(UnitPredict up in all) {
          Unit unit = up.unit;
          hexMap.UpdateWound(unit, up.wounded);
          if(hexMap.IsAttackSide(unit.IsAI())) {
            hexMap.settlementMgr.attackerLaborDead += up.laborDead;
          } else {
            hexMap.settlementMgr.defenderLaborDead += up.laborDead;
          }

          // morale, movement, wounded, killed, laborKilled, disserter, attack, def, discontent
          int[] stats = new int[]{0,0,up.wounded,up.dead,up.laborDead,0,0,0,0};
          hexMap.unitAniController.ShowEffects(unit, stats);
          while (hexMap.unitAniController.ShowAnimating) { yield return null; }

          if (unit.rf.soldiers <= Unit.DisbandUnitUnder) {
            // unit disbanded
            hexMap.unitAniController.DestroyUnit(unit, DestroyType.ByDisband);
            while (hexMap.unitAniController.DestroyAnimating) { yield return null; }
          }
        }

        int capturedHorse = (int)((atkWin ? defenderCavDead : attackerCavDead) * 0.2f);
        hexMap.CaptureHorse(atkWin ? attacker : defender, capturedHorse);

        hexMap.eventDialogAlt.ShowOperationResult(resultLevel, atkWin, !attacker.IsAI(),
          attackerInfTotal, attackerCavTotal, defenderInfTotal, defenderCavTotal,
          attackerInfDead, attackerInfWnd, attackerLaborDead, attackerCavDead, attackerCavWnd,
          defenderInfDead, defenderInfWnd, defenderLaborDead, defenderCavDead, defenderCavWnd,
          capturedHorse);
        while(hexMap.eventDialogAlt.Animating) { yield return null; }
        CancelOperation();

        // Aftermath
        int[] vicBuf = GetVicBuf(resultLevel);
        int[] dftBuf = GetDftBuf(resultLevel);
        if (atkWin) {
          // TODO: general trait apply to stop chaos 
          defender.chaos = (resultLevel == ResultType.Great && Cons.HighlyLikely()) || resultLevel == ResultType.Crushing;
          // TODO
          // 1. accumulate operation result(kill+wounded vs enemy kill+wounded) for per commander for party influence update
          // 2. body cover
          // 6. kill general
          int morale = vicBuf[0];
          int morale1 = vicBuf[1];
          int discontent = vicBuf[2];
          foreach (UnitPredict up in predict.attackers) {
            Unit unit = up.unit;
            if (unit.IsGone()) {
              continue;
            }
            int[] stats = new int[]{0,0,0,0,0,0,0,0,0};
            if (Util.eq<Unit>(unit, attacker)) {
              unit.rf.morale += morale;
              unit.riot.Discontent(discontent); 
              stats[0] = morale;
              stats[8] = discontent;
            } else {
              unit.rf.morale += morale1;
              stats[0] = morale1;
            }

            hexMap.unitAniController.ShowEffects(unit, stats);
            while (hexMap.unitAniController.ShowAnimating) { yield return null; }
          }

          morale = dftBuf[0];
          morale1 = dftBuf[1];
          discontent = dftBuf[2];
          foreach (UnitPredict up in predict.defenders) {
            Unit unit = up.unit;
            if (unit.IsGone()) {
              continue;
            }
            int[] stats = new int[]{0,0,0,0,0,0,0,0,0};
            if (Util.eq<Unit>(unit, defender)) {
              unit.rf.morale += morale;
              stats[0] = morale;
              hexMap.unitAniController.Riot(unit, discontent);
              while (hexMap.unitAniController.riotAnimating) { yield return null; }
            } else {
              unit.rf.morale += morale1;
              stats[0] = morale1;
            }

            hexMap.unitAniController.ShowEffects(unit, stats);
            while (hexMap.unitAniController.ShowAnimating) { yield return null; }
          }
        } else {
          // defender win
          int morale = vicBuf[0];
          int morale1 = vicBuf[1];
          int discontent = vicBuf[2];
          foreach (UnitPredict up in predict.defenders) {
            Unit unit = up.unit;
            if (unit.IsGone()) {
              continue;
            }
            int[] stats = new int[]{0,0,0,0,0,0,0,0,0};
            if (Util.eq<Unit>(unit, defender)) {
              unit.rf.morale += morale;
              unit.riot.Discontent(discontent); 
              stats[0] = morale;
              stats[8] = discontent;
            } else {
              unit.rf.morale += morale1;
              stats[0] = morale1;
            }

            hexMap.unitAniController.ShowEffects(unit, stats);
            while (hexMap.unitAniController.ShowAnimating) { yield return null; }
          }

          morale = dftBuf[0];
          morale1 = dftBuf[1];
          discontent = dftBuf[2];
          foreach (UnitPredict up in predict.attackers) {
            Unit unit = up.unit;
            if (unit.IsGone()) {
              continue;
            }
            int[] stats = new int[]{0,0,0,0,0,0,0,0,0};
            if (Util.eq<Unit>(unit, attacker)) {
              unit.rf.morale += morale;
              stats[0] = morale;
              hexMap.unitAniController.Riot(unit, discontent);
              while (hexMap.unitAniController.riotAnimating) { yield return null; }
            } else {
              unit.rf.morale += morale1;
              stats[0] = morale1;
            }

            hexMap.unitAniController.ShowEffects(unit, stats);
            while (hexMap.unitAniController.ShowAnimating) { yield return null; }
          }
        }
        int deadToll = attackerInfDead + attackerCavDead + attackerLaborDead
          + defenderInfDead + defenderCavDead + defenderLaborDead;
        Tile tile = atkWin ? defender.tile : attacker.tile;
        tile.deadZone.Occur(deadToll);

        // affected all allies
        Unit loser = atkWin ? defender : attacker;
        HashSet<Unit> supporters = new HashSet<Unit>();
        foreach(UnitPredict up in atkWin ? predict.defenders : predict.attackers) {
          supporters.Add(up.unit);
        }
        foreach(Tile t in loser.tile.GetNeighboursWithinRange<Tile>(4, (Tile tt) => true)) {
          Unit unit = t.GetUnit();
          if (unit != null && unit.IsAI() == loser.IsAI() && !supporters.Contains(unit)) {
            int drop = -3;
            int[] stats = new int[]{drop,0,0,0,0,0,0,0,0};
            unit.rf.morale += drop;
            if (unit.IsShowingAnimation()) {
              hexMap.unitAniController.ShowEffects(unit, stats);
              while (hexMap.unitAniController.ShowAnimating) { yield return null; }
            }
          }
        }

        loser.movementRemaining = Unit.MovementcostOnHill * 100;
        // TODO: apply general trait to calmdown
        hexMap.popAniController.Show(hexMap.GetUnitView(loser), 
         loser.chaos ? Cons.GetTextLib().get("pop_chaos") : Cons.GetTextLib().get("pop_retreat"),
         Color.white);
        while (hexMap.popAniController.Animating) { yield return null; }
        HashSet<Tile> from = new HashSet<Tile>{loser.tile};
        int escapeDistance = 5;
        bool moved = false;
        while (escapeDistance > 0) {
          moved = false;
          foreach(Tile t in loser.tile.neighbours) {
            Unit u = t.GetUnit();
            if (u != null && u.IsConcealed()) {
              u.DiscoveredByEnemy();
            }
            if (t.Deployable(loser) && !from.Contains(t)) {
              escapeDistance--;
              from.Add(t);
              hexMap.unitAniController.MoveUnit(loser, t);
              while (hexMap.unitAniController.MoveAnimating) { yield return null; }
              moved = true;
              break;
            }
          }

          if (!moved) {
            // Failed to find escape tile
            if (loser.chaos) {
              List<Unit> ally = new List<Unit>();
              foreach(Tile t in loser.tile.neighbours) {
                Unit u = t.GetUnit();
                if (u != null && u.IsAI() == loser.IsAI()) {
                  ally.Add(u);
                }
              }

              if (ally.Count > 0) {
                Unit clashed = ally[Util.Rand(0, ally.Count-1)];
                int damage = (int)(loser.rf.soldiers * 0.1f);
                damage = damage > clashed.rf.soldiers ? clashed.rf.soldiers : damage;
                int killed = (int)(damage * Util.Rand(5, 7) * 0.1f);
                int wounded = damage - killed;
                clashed.rf.soldiers -= damage;
                clashed.kia += killed;
                clashed.rf.wounded += wounded;
                hexMap.UpdateWound(clashed, wounded);
                clashed.rf.morale -= 10;
                // morale, movement, wounded, killed, laborKilled, disserter, attack, def, discontent
                int[] stats = new int[]{-10,0,wounded,killed,0,0,0,0,0};
                hexMap.popAniController.Show(hexMap.GetUnitView(loser), 
                  Cons.GetTextLib().get("pop_escapeNoRout"),
                  Color.white);
                while (hexMap.popAniController.Animating) { yield return null; }
                hexMap.dialogue.ShowRoutingImpactIncident(loser, clashed);
                while (hexMap.dialogue.Animating) { yield return null; }
                hexMap.popAniController.Show(hexMap.GetUnitView(clashed), 
                  Cons.GetTextLib().get("pop_crashedByAlly"),
                  Color.white);
                while (hexMap.popAniController.Animating) { yield return null; }
                hexMap.unitAniController.ShowEffects(clashed, stats);
                while (hexMap.unitAniController.ShowAnimating) { yield return null; }
                hexMap.unitAniController.Riot(clashed, 2);
                while (hexMap.unitAniController.riotAnimating) { yield return null; }
                if (!clashed.IsGone() && clashed.rf.soldiers <= Unit.DisbandUnitUnder) {
                  // unit disbanded
                  hexMap.unitAniController.DestroyUnit(clashed, DestroyType.ByDisband);
                  while (hexMap.unitAniController.DestroyAnimating) { yield return null; }
                }
              }
            }
            break;
          }
        }
        loser.movementRemaining = 0;
      } else {
      // TODO: uncomment
      //if (!attacker.IsAI()) {
        hexMap.mouseController.EscapeOnOperationCancel();
      //}
        CancelOperation();
      }
      commenceOpAnimating = false;
    }

    public void ActionDone(Unit unitUnderAttack, Unit[] attackers, ActionController.actionName actionName)
    {
      if (actionName == ActionController.actionName.ATTACK)
      {
        if (unitUnderAttack.attackReaction == Reaction.Disband)
        {
          //unitUnderAttack.Destroy(DestroyType.ByDisband);
          return;
        }
        if (attackers[0].attackReaction == Reaction.Disband)
        {
          //attackers[0].Destroy(DestroyType.ByDisband);
          return;
        }
      }
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