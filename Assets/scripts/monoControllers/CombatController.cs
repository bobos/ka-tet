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
    public int attackerOptimPoints = 1;
    public int defenderOptimPoints = 1;
    public OperationGeneralResult sugguestedResult;
    public CombatController.ResultType suggestedResultType;
  }

  public class CombatController : BaseController
  {
    public const float DefendModifier = 1.5f;

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

        if (u.IsAI() == attacker.IsAI() && u.GetStaminaLevel() != StaminaLvl.Exhausted
          && !Util.eq<Unit>(u, attacker) && u.CanAttack()) {
          supportAttackers.Add(u);
        }

        if (u.IsAI() != attacker.IsAI() && u.GetStaminaLevel() != StaminaLvl.Exhausted
          && !Util.eq<Unit>(u, attacker)) {
          supportDefenders.Add(u);
        }
      }

      UnitPredict unitPredict = new UnitPredict();
      unitPredict.unit = attacker;
      unitPredict.percentOfEffectiveForce = GetEffectiveForcePercentage(attacker, false);
      unitPredict.joinPossibility = 100;
      SetGaleVantage(attacker, defender, unitPredict);
      // TODO:tmp morale buff from commander
      unitPredict.operationPoint = attacker.IsCamping() ? attacker.unitCampingAttackCombatPoint : attacker.GetUnitAttackCombatPoint();
      predict.attackers.Add(unitPredict);
      predict.attackerOptimPoints += unitPredict.operationPoint;
      hexMap.ShowAttackArrow(attacker, targetUnit, unitPredict);

      foreach(Unit unit in supportAttackers) {
        unitPredict = new UnitPredict();
        unitPredict.unit = unit;
        unitPredict.percentOfEffectiveForce = GetEffectiveForcePercentage(unit, false);
        SetGaleVantage(unit, defender, unitPredict);
        unitPredict.joinPossibility = 100;
        unitPredict.operationPoint = unit.GetUnitAttackCombatPoint();
        predict.attackers.Add(unitPredict);
        predict.attackerOptimPoints += unitPredict.operationPoint;
      }

      // defenders
      unitPredict = new UnitPredict();
      unitPredict.unit = defender;
      unitPredict.percentOfEffectiveForce = GetEffectiveForcePercentage(defender, true);
      unitPredict.joinPossibility = 100;
      unitPredict.operationPoint = targetSettlement != null ? targetSettlement.GetDefendForce() : defender.GetUnitDefendCombatPoint(true);
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
        unitPredict.joinPossibility = 100;
        unitPredict.operationPoint = unit.GetUnitDefendCombatPoint(false);
        predict.defenders.Add(unitPredict);
        predict.defenderOptimPoints += unitPredict.operationPoint;
      }

      CalculateWinChance(predict);
      bool atkWin = predict.sugguestedResult.chance == 10;
      Unit initiator = atkWin ? defender : attacker; 
      List<UnitPredict> ups = atkWin ? predict.defenders : predict.attackers;
      foreach (UnitPredict up in ups) {
        up.joinPossibility = GetJoinPossibility(initiator, up.unit, predict.suggestedResultType);
      }

      foreach(UnitPredict up in predict.attackers) {
        if (Util.eq<Unit>(up.unit, attacker)) {
          continue;
        }
        hexMap.ShowAttackArrow(up.unit, targetUnit, up);
      }

      foreach(UnitPredict up in predict.defenders) {
        if (Util.eq<Unit>(up.unit, defender)) {
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
      if (odds <= 1.4f) {
        // 1 - 2x odds
        predict.suggestedResultType = ResultType.Close;
      } else if (odds <= 1.6f) {
        // 2x - 2.4x
        predict.suggestedResultType = ResultType.Small;
      } else if (odds <= 2.6f) {
        // 2.4x - 3.9xx
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

    int GetJoinPossibility(Unit unit1, Unit unit2, ResultType result) {
      Party mainParty = unit1.rf.general.party; 
      if (Util.eq<Unit>(unit1, defender) && defender.IsCamping()) {
        mainParty = hexMap.warProvince.ownerParty;
      }

      bool inRange = unit2.InCommanderRange();
      // TODO: apply general triats to adjust possibility 
      if (Util.eq<Party>(mainParty, unit2.rf.general.party)) {
        return 100;
      }

      //Party.Relation relation = mainParty.GetRelation();
      if (result == ResultType.Close) {
        return inRange ? 90 : 70;
      }

      if (result == ResultType.Small) {
        return inRange ? 80 : 60;
      }

      if (result == ResultType.Great) {
        return inRange ? 70 : 50;
      }

      return inRange ? 40 : 20;
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
          if (unit.rf.rank.Level() == 1) {
            if (!unit.IsCavalry()) {
              total -= 20;
              Helper(up, 20);
            } else {
              total -= 8;
              Helper(up, 8);
            }
          }

          // veteran
          if (unit.rf.rank.Level() == 2) {
            if (!unit.IsCavalry()) {
              total -= 10;
              Helper(up, 10);
            } else {
              total -= 5;
              Helper(up, 5);
            }
          }

          // elite
          if (unit.rf.rank.Level() == 3) {
            if (!unit.IsCavalry()) {
              total -= 5;
              Helper(up, 5);
            } else {
              total -= 3;
              Helper(up, 3);
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
        return new int[]{-1, -1, 0};
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
        return new int[]{-2, -2, 0};
      }
      if (type == ResultType.Small) {
        return new int[]{-4, -4, 1};
      }
      if (type == ResultType.Great) {
        return new int[]{-5, -5, 2};
      }
      return new int[]{-10, -10, 4};
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
            u.unit.attacked = true; 
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

          // TODO: apply general trait to increase the chance
          if (noRetreat && !defender.IsVulnerable()
            && !defender.IsWarWeary()
            && (predict.attackerOptimPoints > (int)(predict.defenderOptimPoints * 1.5f)) && Cons.FiftyFifty()) {
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
        bool attackerBigger = true;
        ResultType resultLevel = predict.suggestedResultType;
        predict.attackerOptimPoints = predict.attackerOptimPoints <= 0 ? 1 : predict.attackerOptimPoints;
        predict.defenderOptimPoints = predict.defenderOptimPoints <= 0 ? 1 : predict.defenderOptimPoints;

        bool atkWin = predict.sugguestedResult.chance == 10;
// 1.0 to 1.3 - 0.01(both)
// 1.4 to 1.8 - def: x - 1.2, atk: (x -1) * (0.5 - 0.6)   
// 1.9 to 2.2 - atk: (x - 1.2) * (0.25 - 0.4)
        if (resultLevel == ResultType.Close) {
          // 1 - 2x odds
          float m = 0.035f;
          float m1 = 0.03f;
          if (Cons.SlimChance()) {
            m = 0.04f;
            m1 = 0.01f;
          }
          defenderCasualty = (int)(defenderTotal * (attackerBigger ? m : m1));
          attackerCasualty = (int)(attackerTotal * (attackerBigger ? m1 : m));
        } else {
          if (attackerBigger) {
            defenderCasualty = (int)(defenderTotal * 0.02f);
          } else {
            attackerCasualty = (int)(attackerTotal * 0.02f);
          }

          if (resultLevel == ResultType.Small) {
            // 2x - 2.4x
            int modifier = Util.Rand(7, 8);
            if (attackerBigger) {
              attackerCasualty = (int)(defenderCasualty * modifier * 0.1f);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier * 0.1f);
            }
          }

          if (resultLevel == ResultType.Great) {
            // 2.4x - 3.9x
            int modifier = Util.Rand(5, 6);
            if (attackerBigger) {
              attackerCasualty = (int)(defenderCasualty * modifier* 0.1f);
            } else {
              defenderCasualty = (int)(attackerCasualty * modifier * 0.1f);
            }
          }

          if (resultLevel == ResultType.Crushing) {
            // 3.9x - 6x odds
            int modifier = Util.Rand(1, 3);
            if (attackerBigger) {
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
            // morale, movement, wounded, killed, laborKilled, disserter, attack, def, discontent
            int[] stats = new int[]{0,0,0,up.dead,0,0,0,0,0};
            hexMap.unitAniController.ShowEffects(unit, stats, view);
            while (hexMap.unitAniController.ShowAnimating) { yield return null; }
          }

          if (unit.rf.soldiers <= Unit.DisbandUnitUnder) {
            // unit disbanded
            hexMap.unitAniController.DestroyUnit(unit, DestroyType.ByDisband);
            while (hexMap.unitAniController.DestroyAnimating) { yield return null; }
          }
        }

        if (defender.IsCamping()) {
            int[] stats = new int[]{0,0,0,settlementDead,0,0,0,0,0};
            hexMap.unitAniController.ShowEffects(null, stats, hexMap.settlementMgr.GetView(defender.tile.settlement));
            while (hexMap.unitAniController.ShowAnimating) { yield return null; }
        }

        int capturedHorse = (int)((atkWin ? defenderCavDead : attackerCavDead) * 0.2f);
        hexMap.CaptureHorse(atkWin ? attacker : defender, capturedHorse);

        hexMap.eventDialogAlt.ShowOperationResult(resultLevel, atkWin, !attacker.IsAI(),
          attackerInfTotal, attackerCavTotal, defenderInfTotal, defenderCavTotal,
          attackerInfDead, attackerCavDead, defenderInfDead,  defenderCavDead, capturedHorse);
        while(hexMap.eventDialogAlt.Animating) { yield return null; }
        CancelOperation();

        // Aftermath
        int[] vicBuf = GetVicBuf(resultLevel);
        int[] dftBuf = GetDftBuf(resultLevel);
        if (atkWin) {
          // TODO
          // 1. accumulate operation result(kill+wounded vs enemy kill+wounded) for per commander for party influence update
          // 6. kill general
          int morale = vicBuf[0];
          int morale1 = vicBuf[1];
          int discontent = vicBuf[2];
          View view = null;
          foreach (UnitPredict up in predict.attackers) {
            Unit unit = up.unit;
            if (unit.IsGone()) {
              continue;
            }
            int[] stats = new int[]{0,0,0,0,0,0,0,0,0};
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

            hexMap.unitAniController.ShowEffects(unit, stats, view, true);
          }

          hexMap.turnController.Sleep(1);
          while(hexMap.turnController.sleeping) { yield return null; }
          attacker.riot.Discontent(discontent); 
          view = null;
          if (attacker.IsCamping()) {
            view = hexMap.settlementMgr.GetView(attacker.tile.settlement);
          }
          hexMap.unitAniController.ShowEffects(attacker, new int[]{0,0,0,0,0,0,0,0,discontent}, view);
          while(hexMap.unitAniController.ShowAnimating) { yield return null; }

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
            } else {
              unit.rf.morale += morale1;
              stats[0] = morale1;
            }

            view = null;
            if (unit.IsCamping()) {
              view = hexMap.settlementMgr.GetView(unit.tile.settlement);
            }
            hexMap.unitAniController.ShowEffects(unit, stats, view, true);
          }
          hexMap.turnController.Sleep(1);
          while(hexMap.turnController.sleeping) { yield return null; }
          if (!defender.IsCamping() && !defender.IsGone()) {
            hexMap.unitAniController.Riot(defender, discontent);
            while (hexMap.unitAniController.riotAnimating) { yield return null; }
          }
        } else {
          // defender win
          int morale = vicBuf[0];
          int morale1 = vicBuf[1];
          int discontent = vicBuf[2];
          View view = null;
          foreach (UnitPredict up in predict.defenders) {
            Unit unit = up.unit;
            if (unit.IsGone()) {
              continue;
            }
            int[] stats = new int[]{0,0,0,0,0,0,0,0,0};
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
            hexMap.unitAniController.ShowEffects(unit, stats, view, true);
          }
          hexMap.turnController.Sleep(1);
          while(hexMap.turnController.sleeping) { yield return null; }
          defender.riot.Discontent(discontent); 
          view = null;
          if (defender.IsCamping()) {
            view = hexMap.settlementMgr.GetView(defender.tile.settlement);
          }
          hexMap.unitAniController.ShowEffects(defender, new int[]{0,0,0,0,0,0,0,0,discontent}, view);
          while(hexMap.unitAniController.ShowAnimating) { yield return null; }

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
            } else {
              unit.rf.morale += morale1;
              stats[0] = morale1;
            }

            view = null;
            if (unit.IsCamping()) {
              view = hexMap.settlementMgr.GetView(unit.tile.settlement);
            }
            hexMap.unitAniController.ShowEffects(unit, stats, view, true);
          }
          hexMap.turnController.Sleep(1);
          while(hexMap.turnController.sleeping) { yield return null; }
          if (!attacker.IsCamping() && !attacker.IsGone()) {
            hexMap.unitAniController.Riot(attacker, discontent);
            while (hexMap.unitAniController.riotAnimating) { yield return null; }
          }
        }

        int deadToll = attackerInfDead + attackerCavDead + defenderInfDead + defenderCavDead;
        Tile tile = atkWin ? defender.tile : attacker.tile;
        if (tile.settlement == null) {
          tile.deadZone.Occur(deadToll);
        }

        Unit loser = atkWin ? defender : attacker;
        List<Unit> supporters = new List<Unit>();
        foreach(UnitPredict up in atkWin ? predict.defenders : predict.attackers) {
          if (!up.unit.IsGone()) {
            supporters.Add(up.unit);
          }
        }

        // affected all allies
        if (loser.IsCommander()) {
          int drop = -3;
          if (resultLevel == ResultType.Small) {
            drop = -5;
          }
          if (resultLevel == ResultType.Great) {
            drop = -8;
          }
          if (resultLevel == ResultType.Crushing) {
            drop = -10;
          }
          hexMap.unitAniController.ShakeNearbyAllies(loser, drop);
          while (hexMap.unitAniController.ShakeAnimating) { yield return null; }
        }

        // TODO: when defender is in city, capture the city on victory
        if (resultLevel != ResultType.Close) {
          Dictionary<Tile, bool> tiles = new Dictionary<Tile, bool>();
          Dictionary<Unit, List<Tile>> plan = new Dictionary<Unit, List<Tile>>();
          Tile baseTile = loser.tile;
          foreach(Tile t in baseTile.GetNeighboursWithinRange(10, (Tile _tile) => true)) {
            tiles[t] = !t.Deployable(loser);
          }

          // sort units from far to near
          supporters.Sort(delegate (Unit a, Unit b)
          {
            return (int)(Tile.Distance(baseTile, b.tile) - Tile.Distance(baseTile, a.tile));
          });

          foreach(Unit unit in supporters) {
            if (unit.IsCamping()) { 
              plan[unit] = null;
              continue;
            }
            // first step
            Tile step1 = null;
            foreach (Tile t in unit.tile.neighbours) {
              // already taken
              if (tiles[t]) { continue; }
              step1 = t;
              break;
            }

            if (step1 == null) {
              plan[unit] = null;
              continue;
            }

            List<Tile> path = new List<Tile>();
            path.Add(step1);
            // second step
            Tile step2 = null;
            foreach (Tile t in step1.neighbours) {
              // already taken
              if (tiles[t]) { continue; }
              step2 = t;
              break;
            }

            if (step2 == null) {
              tiles[step1] = true;
            } else {
              path.Add(step2);
              tiles[step2] = true;
            }
            tiles[unit.tile] = false;

            plan[unit] = path;
          }

          foreach (Unit unit in supporters) {
            if (resultLevel == ResultType.Crushing) {
              unit.chaos = true;
            } else if (resultLevel == ResultType.Great) {
              unit.defeating = true;
            }
          }

          if (resultLevel == ResultType.Small) {
            loser.defeating = true;
          }

          if (supporters.Count > 2) {
            if (resultLevel == ResultType.Crushing) {
              hexMap.turnController.ShowTitle(Cons.GetTextLib().get("title_formationCrushing"), Color.black);
            } else {
              hexMap.turnController.ShowTitle(Cons.GetTextLib().get("title_formationBreaking"), Color.black);
            }
            while(hexMap.turnController.showingTitle) { yield return null; }
            hexMap.dialogue.ShowFormationBreaking(atkWin ? attacker : defender);
            while(hexMap.dialogue.Animating) { yield return null; }
            hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetUnitView(loser).transform.position);
            while(hexMap.cameraKeyboardController.fixingCamera) { yield return null; }
          }

          // move all unit at once
          List<Unit> failedToMove = new List<Unit>();
          foreach(Unit unit in supporters) {
            List<Tile> path = plan[unit];
            if (path == null) {
              if (!unit.IsCamping()) {
                failedToMove.Add(unit);
              }
              continue;
            }
            foreach(Tile t in path) {
              hexMap.unitAniController.MoveUnit(unit, path[path.Count - 1], true);
            }
          }
          hexMap.turnController.Sleep(1);
          while(hexMap.turnController.sleeping) { yield return null; }

          // TODO: 力挽狂澜
          foreach(Unit unit in failedToMove) {
            foreach(Tile t in unit.tile.neighbours) {
              Unit u = t.GetUnit();
              if (u != null && u.IsAI() == unit.IsAI() && !supporters.Contains(u)) {
                hexMap.unitAniController.CrashByAlly(u, -10);
                while (hexMap.unitAniController.CrashAnimating) { yield return null; }
                continue;
              }
            }
          }
        } else {
          if (Cons.FiftyFifty()) {
            // TODO: general trait apply to stop defeating 
            loser.defeating = true;
          }
          if (!loser.IsCamping()) {
            int escapeDistance =  2;
            hexMap.popAniController.Show(hexMap.GetUnitView(loser), 
             Cons.GetTextLib().get("pop_retreat"),
             Color.white);
            while (hexMap.popAniController.Animating) { yield return null; }
            HashSet<Tile> from = new HashSet<Tile>{loser.tile};
            bool moved = false;
            while (escapeDistance > 0) {
              moved = false;
              foreach(Tile t in loser.tile.neighbours) {
                Unit u = t.GetUnit();
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
                List<Unit> ally = new List<Unit>();
                foreach(Tile t in loser.tile.neighbours) {
                  Unit u = t.GetUnit();
                  if (u != null && u.IsAI() == loser.IsAI()) {
                    ally.Add(u);
                  }
                }

                if (ally.Count > 0) {
                  Tile escapeTile = null;
                  foreach (Unit u in ally) {
                    foreach(Tile t1 in u.tile.neighbours) {
                      if (t1.Deployable(loser) && !from.Contains(t1)) {
                        escapeTile = t1;
                        break;
                      }
                      if (escapeTile != null) {
                        break;
                      }
                    }
                  }

                  if (escapeTile == null) {
                    hexMap.unitAniController.CrashByAlly(ally[Util.Rand(0, ally.Count-1)], -10);
                    while (hexMap.unitAniController.CrashAnimating) { yield return null; }
                  } else {
                    hexMap.unitAniController.MoveUnit(loser, escapeTile);
                    while (hexMap.unitAniController.MoveAnimating) { yield return null; }
                  }
                }
                break;
              }
            }
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