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
      mouseController.onUnitAttack += OnUnitAttack;
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

    int GetEffectiveForcePercentage(Unit unit) {
      if (unit.GetStaminaLevel() == StaminaLvl.Tired) {
        return 70;
      } else if (unit.GetStaminaLevel() == StaminaLvl.Exhausted) {
        return 0;
      } else {
         return 100;
      }
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
        if (u == null || u.type == Type.Scout) {
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
      unitPredict.percentOfEffectiveForce = GetEffectiveForcePercentage(attacker);
      unitPredict.joinPossibility = 100;
      SetGaleVantage(attacker, defender, unitPredict);
      // TODO:tmp morale buff from commander
      unitPredict.operationPoint = (int)(attacker.unitAttack * unitPredict.percentOfEffectiveForce * 0.01f);
      predict.attackers.Add(unitPredict);
      predict.attackerOptimPoints += unitPredict.operationPoint;
      hexMap.ShowAttackArrow(attacker, targetUnit, unitPredict);

      foreach(Unit unit in supportAttackers) {
        unitPredict = new UnitPredict();
        unitPredict.unit = unit;
        unitPredict.percentOfEffectiveForce = GetEffectiveForcePercentage(unit);
        SetGaleVantage(unit, defender, unitPredict);
        unitPredict.joinPossibility = GetJoinPossibility(unit, attacker);
        unitPredict.operationPoint = (int)(unit.unitAttack * unitPredict.percentOfEffectiveForce * 0.01f);
        predict.attackers.Add(unitPredict);
        predict.attackerOptimPoints += unitPredict.operationPoint;
        hexMap.ShowAttackArrow(unit, targetUnit, unitPredict);
      }

      // defenders
      unitPredict = new UnitPredict();
      unitPredict.unit = defender;
      unitPredict.percentOfEffectiveForce = GetEffectiveForcePercentage(defender);
      // Exausted defense drop 50%
      unitPredict.percentOfEffectiveForce = unitPredict.percentOfEffectiveForce == 0 ? 50 : unitPredict.percentOfEffectiveForce;
      unitPredict.joinPossibility = 100;
      unitPredict.operationPoint = (int)(defender.unitDefence * unitPredict.percentOfEffectiveForce * 0.01f);
      predict.defenders.Add(unitPredict);
      predict.defenderOptimPoints += unitPredict.operationPoint;
      hexMap.ShowDefenderStat(defender, unitPredict);

      foreach(Unit unit in supportDefenders) {
        unitPredict = new UnitPredict();
        unitPredict.unit = unit;
        unitPredict.percentOfEffectiveForce = GetEffectiveForcePercentage(unit);
        unitPredict.joinPossibility = GetJoinPossibility(unit, targetUnit);
        unitPredict.operationPoint = (int)(unit.unitDefence * unitPredict.percentOfEffectiveForce * 0.01f);
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
      if (predict.defenderOptimPoints >= predict.attackerOptimPoints) {
        predict.sugguestedResult = new OperationGeneralResult(0);
      } else if (predict.attackerOptimPoints >= (int)(predict.defenderOptimPoints * 1.5f)) {
        predict.sugguestedResult = new OperationGeneralResult(10);
      } else if (predict.attackerOptimPoints >= (int)(predict.defenderOptimPoints * 1.4f)) {
        predict.sugguestedResult = new OperationGeneralResult(8);
      } else if (predict.attackerOptimPoints >= (int)(predict.defenderOptimPoints * 1.3f)) {
        predict.sugguestedResult = new OperationGeneralResult(5);
      } else if (predict.attackerOptimPoints >= (int)(predict.defenderOptimPoints * 1.2f)) {
        predict.sugguestedResult = new OperationGeneralResult(3);
      } else {
        predict.sugguestedResult = new OperationGeneralResult(1);
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

    void AllocateCasualty(int total, List<UnitPredict> units) {
      while (total > 0) {
        foreach(UnitPredict up in units) {
          Unit unit = up.unit;

          if (unit.rf.soldiers == 0) {
            continue;
          }

          if (total < 10) {
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
            continue;
          }

          if (unit.rf.soldiers < 10) {
            total -= unit.rf.soldiers;
            up.dead += unit.rf.soldiers;
            up.laborDead += unit.labor;
            unit.rf.soldiers = 0;
            unit.labor = 0;
            continue;
          }

          // rookie
          if (unit.rf.rank.Level() == 1) {
            if (!unit.IsCavalry()) {
              total -= 10;
              Helper(up, 10, 1);
            } else {
              total -= 3;
              Helper(up, 3, 1);
            }
          }

          // veteran
          if (unit.rf.rank.Level() == 2) {
            if (!unit.IsCavalry()) {
              total -= 5;
              Helper(up, 5, 1);
            } else {
              total -= 2;
              Helper(up, 2, 1);
            }
          }

          // elite
          if (unit.rf.rank.Level() == 3 || unit.rf.rank.Level() == -1) {
            if (!unit.IsCavalry()) {
              total -= 3;
              Helper(up, 3, 1);
            } else {
              total -= 1;
              Helper(up, 1, 1);
            }
          }
        }
      }
    }

    public enum ResultType {
      Close,
      Small,
      Great,
      Crushing
    }

    int[] GetVicBuf(ResultType type) {
      if (type == ResultType.Close) {
        return new int[]{1, 0, -1};
      }
      if (type == ResultType.Small) {
        return new int[]{2, 1, -2};
      }
      if (type == ResultType.Great) {
        return new int[]{3, 2, -3};
      }
      return new int[]{4, 2, -4};
    }

    // initiatorMorale, supporterMorale, initiatorDiscontent
    int[] GetDftBuf(ResultType type) {
      if (type == ResultType.Close) {
        return new int[]{-10, -3, 0};
      }
      if (type == ResultType.Small) {
        return new int[]{-15, -5, 1};
      }
      if (type == ResultType.Great) {
        return new int[]{-20, -10, 3};
      }
      return new int[]{-30, -20, 8};
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
          if (noRetreat && Cons.FiftyFifty()) {
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
        if (predict.attackerOptimPoints > predict.defenderOptimPoints) {
          factor = (int)((predict.attackerOptimPoints / predict.defenderOptimPoints) * 10) - 10;
        } else {
          attackerBigger = false;
          factor = (int)((predict.defenderOptimPoints / predict.attackerOptimPoints) * 10) - 10;
        }
        factor = factor > 95 ? 95 : factor;

// 1.0 to 1.1 - 0.01(both)
// 1.2 to 1.6 - def: x - 1, atk: (x -1) * (0.5 - 0.6)   
// 1.7 to 2.0 - atk: (x - 1) * (0.25 - 0.4)
// 2.1 to 3.0 - atk: (x - 1) * (0.125 - 0.2)
// 3.1 to 4.0 - atk: (x - 1) * (0.1 - 0.125);
// 4.0 above - atk: (x - 1) * 0.05
        if (factor <= 1) {
          float f = 0.04f;
          if (Cons.SlimChance()) {
            f = 0.1f;
          }
          defenderCasualty = (int)(defenderTotal * f);
          attackerCasualty = (int)(attackerTotal * f);
        } else {
          if (attackerBigger) {
            defenderCasualty = (int)(defenderTotal * factor * 0.025f);
          } else {
            attackerCasualty = (int)(attackerTotal * factor * 0.025f);
          }

          if (factor > 1 && factor <= 6) {
            resultLevel = ResultType.Small;
            int modifier = Util.Rand(5, 6);
            if (attackerBigger) {
              attackerCasualty = (int)(defenderCasualty * modifier * 0.1f);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier * 0.1f);
            }
          }

          if (factor > 6 && factor <= 20) {
            int modifier = Util.Rand(25, 40);
            if (attackerBigger) {
              attackerCasualty = (int)(defenderCasualty * modifier* 0.01f);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier * 0.01f);
            }
            resultLevel = modifier > 35 ? ResultType.Small : ResultType.Great;
          }

          if (factor > 20 && factor <= 30) {
            int modifier = Util.Rand(125, 200);
            resultLevel = Cons.EvenChance() ? ResultType.Crushing : ResultType.Great;
            if (attackerBigger) {
              attackerCasualty = (int)(defenderCasualty * modifier * 0.001f);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier * 0.001f);
            }
            resultLevel = modifier > 185 ? ResultType.Great : ResultType.Crushing;
          }

          if (factor > 30 && factor <= 40) {
            float modifier = Util.Rand(100, 125) * 0.001f;
            resultLevel = ResultType.Crushing;
            if (attackerBigger) {
              attackerCasualty = (int)(defenderCasualty * modifier);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier);
            }
          }

          if (factor > 40) {
            float modifier = 0.005f;
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

        AllocateCasualty(defenderCasualty, predict.defenders);
        AllocateCasualty(attackerCasualty, predict.attackers);

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
          up.unit.movementRemaining -= (Unit.ActionCost -10);
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

        int dice = Util.Rand(1, 10);
        bool atkWin = dice <= predict.sugguestedResult.chance;
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

          if (defender.chaos) {
            defender.movementRemaining = Unit.MovementcostOnHill * 100;
            hexMap.popAniController.Show(hexMap.GetUnitView(defender), 
              Cons.GetTextLib().get("pop_chaos"),
              Color.white);
            while (hexMap.popAniController.Animating) { yield return null; }
            hexMap.GetUnitView(defender).UpdateUnitInfo();
            // TODO routing
            HashSet<Tile> from = new HashSet<Tile>{defender.tile};
            int escapeDistance = 5;
            bool moved = false;
            while (escapeDistance > 0) {
              moved = false;
              foreach(Tile t in defender.tile.neighbours) {
                Unit u = t.GetUnit();
                if (u != null && u.IsConcealed()) {
                  u.DiscoveredByEnemy();
                }
                if (t.Deployable(defender) && !from.Contains(t)) {
                  escapeDistance--;
                  from.Add(t);
                  hexMap.unitAniController.MoveUnit(defender, t);
                  while (hexMap.unitAniController.MoveAnimating) { yield return null; }
                  moved = true;
                  break;
                }
              }

              if (!moved) {
                // Failed to find escape tile, pick a nearby ally and clash
                List<Unit> ally = new List<Unit>();
                foreach(Tile t in defender.tile.neighbours) {
                  Unit u = t.GetUnit();
                  if (u != null && u.IsAI() == defender.IsAI()) {
                    ally.Add(u);
                  }
                }

                if (ally.Count > 0) {
                  Unit clashed = ally[Util.Rand(0, ally.Count-1)];
                  int damage = (int)(defender.rf.soldiers * 0.1f);
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
                  hexMap.popAniController.Show(hexMap.GetUnitView(defender), 
                    Cons.GetTextLib().get("pop_escapeNoRout"),
                    Color.white);
                  while (hexMap.popAniController.Animating) { yield return null; }
                  hexMap.dialogue.ShowRoutingImpactIncident(defender, clashed);
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
                break;
              }
            }
            defender.movementRemaining = 0;
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

    public void OnUnitAttack(Unit[] punchers, Unit receiver)
    {
      int def = receiver.def;
      string msg = "";
      if (punchers.Length == 1)
      {
        int hit = punchers[0].atk - def;
        if (hit <= 0)
        {
          hit = 1;
        }
        int reduced = hit * 100;
        int moraleHit = hit * 2;
        receiver.rf.soldiers -= reduced;
        receiver.rf.morale -= moraleHit;

        msg += punchers[0].GeneralName() + " killed " + reduced + " soldiers of " + receiver.GeneralName() + "!\n";

        msg += receiver.GeneralName() + " killed " + (int)(reduced / 8) + "soldiers of " + punchers[0].GeneralName() + "\n";
        punchers[0].rf.soldiers -= (int)(reduced / 8);

        // TODO: punish the morale after severly reduced
        if (receiver.rf.soldiers <= 0)
        {
          msg += receiver.GeneralName() + " is destroyed!\n";
          punchers[0].rf.morale += 30;
          punchers[0].rf.morale = punchers[0].rf.morale > 100 ? 100 : punchers[0].rf.morale;
          msg += punchers[0].GeneralName() + "'s morale increased 30\n";
          receiver.attackReaction = Reaction.Disband;
        }
        else
        {
          msg += receiver.GeneralName() + "'s morale decreased to " + moraleHit + "\n";
          receiver.attackReaction = Reaction.Stand;
          if (receiver.rf.morale <= 0)
          {
            receiver.attackReaction = Reaction.Rout;
          }
        }

        if (punchers[0].rf.soldiers <= 0)
        {
          msg += punchers[0].Name() + " is destroyed!\n";
          punchers[0].attackReaction = Reaction.Disband;
        }
        else
        {
          if (punchers[0].IsCavalry())
          {
            punchers[0].movementRemaining -= 1;
            if (punchers[0].movementRemaining < 0) punchers[0].movementRemaining = 0;
          }
          else
          {
            punchers[0].movementRemaining = -1;
          }
        }
      }
      if (!actionController.DoAction(receiver, punchers, null, ActionController.actionName.ATTACK))
      {
        Debug.LogError("Failed to perform attack, try again!");
        return;
      }
      msgBox.Show(msg);
      // TODO: calculate hit points, and set reaction on receiver(retreat, set effect etc.)
    }

  }

}