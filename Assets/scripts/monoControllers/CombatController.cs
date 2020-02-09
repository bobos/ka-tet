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

      this.predict = predict;
      return predict;
    }

    int GetJoinPossibility(Unit unit1, Unit unit2) {
      // TODO: consider other factors 
      if (Util.eq<Party>(unit1.rf.general.party, unit2.rf.general.party)) {
        return 100;
      }

      Party.Relation relation = unit1.rf.general.party.GetRelation();
      if (relation == Party.Relation.normal) {
        return 100;
      }

      if (relation == Party.Relation.tense) {
        return 80;
      }

      return 60;
    }

    public void CancelOperation() {
      start = false;
      hexMap.CleanLines();
    }

    void AllocateCasualty(int total, List<UnitPredict> units) {
      while (total > 0) {
        foreach(UnitPredict up in units) {
          Unit unit = up.unit;
          // rookie
          if (unit.rf.rank.Level() == 1) {
            if (unit.IsCavalry()) {
              // TODO: -3
              // update up.dead and up.wounded
            } else {
              if (total > 10) {
                total -= 10;
              } else {
                total = 0;
                // deduct
                break;
              }
            }
          }

          // veteran
          if (unit.rf.rank.Level() == 2) {
            if (unit.IsCavalry()) {
              // -2
            } else {
              if (total > 5) {
                total -= 5;
              } else {
                total = 0;
                // deduct
                break;
              }
            }
          }

          // elite
          if (unit.rf.rank.Level() == 3 || unit.rf.rank.Level() == -1) {
            if (unit.IsCavalry()) {
              // -1
            } else {
              if (total > 3) {
                total -= 3;
              } else {
                total = 0;
                // deduct
                break;
              }
            }
          }
        }
      }
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
        int defenderTotal = 0;
        int attackerTotal = 0;
        foreach (UnitPredict u in predict.attackers) {
          attackerTotal += u.unit.rf.soldiers;
        }

        foreach (UnitPredict u in predict.defenders) {
          defenderTotal += u.unit.rf.soldiers;
        }

        int attackerCasualty = 0;
        int defenderCasualty = 0;
        bool attackerBigger = true;
        int factor = 0;
        if (predict.attackerOptimPoints > predict.defenderOptimPoints) {
          factor = (int)((predict.attackerOptimPoints / predict.defenderOptimPoints) * 10) - 10;
        } else {
          attackerBigger = false;
          factor = (int)((predict.defenderOptimPoints / predict.attackerOptimPoints) * 10) - 10;
        }

// 1.0 to 1.1 - 0.01(both)
// 1.2 to 1.6 - def: x - 1, atk: (x -1) * (0.5 - 0.6)   
// 1.7 to 2.0 - atk: (x - 1) * (0.25 - 0.4)
// 2.1 to 3.0 - atk: (x - 1) * (0.125 - 0.2)
// 3.1 to 4.0 - atk: (x - 1) * (0.1 - 0.125);
// 4.0 above - atk: (x - 1) * 0.05
        if (factor <= 1) {
          defenderCasualty = attackerCasualty = (int)(defenderTotal * 0.01f);
        } else {
          if (attackerBigger) {
            defenderCasualty = (int)(defenderTotal * factor * 0.01f);
          } else {
            attackerCasualty = (int)(attackerTotal * factor * 0.01f);
          }

          if (factor > 1 && factor <= 6) {
            if (attackerBigger) {
              attackerCasualty = (int)(defenderCasualty * Util.Rand(5, 6) * 0.1f);
            } else {
              defenderCasualty = (int)(attackerCasualty * Util.Rand(5, 6) * 0.1f);
            }
          }

          if (factor > 6 && factor <= 20) {
            if (attackerBigger) {
              attackerCasualty = (int)(defenderCasualty * Util.Rand(25, 40) * 0.01f);
            } else {
              defenderCasualty = (int)(attackerCasualty * Util.Rand(25, 40) * 0.01f);
            }
          }

          if (factor > 20 && factor <= 30) {
            if (attackerBigger) {
              attackerCasualty = (int)(defenderCasualty * Util.Rand(125, 200) * 0.001f);
            } else {
              defenderCasualty = (int)(attackerCasualty * Util.Rand(125, 200) * 0.001f);
            }
          }

          if (factor > 30 && factor <= 40) {
            if (attackerBigger) {
              attackerCasualty = (int)(defenderCasualty * Util.Rand(100, 125) * 0.001f);
            } else {
              defenderCasualty = (int)(attackerCasualty * Util.Rand(100, 125) * 0.001f);
            }
          }

          if (factor > 40) {
            if (attackerBigger) {
              attackerCasualty = (int)(defenderCasualty * 0.05f);
            } else {
              defenderCasualty = (int)(attackerCasualty * 0.05f);
            }
          }
        }

        attackerCasualty = attackerCasualty > attackerTotal ? attackerTotal : attackerCasualty;
        defenderCasualty = defenderCasualty > defenderTotal ? defenderTotal : defenderCasualty;

        AllocateCasualty(defenderCasualty, predict.defenders);
        AllocateCasualty(attackerCasualty, predict.attackers);

        // commence attack
        int dice = Util.Rand(1, 10);
        if (dice > predict.sugguestedResult.chance) {
          // attacker lose
          // TODO: morale, horse capture from total cav dead

        } else {
          // defender lose
        }
      }
      CancelOperation();
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