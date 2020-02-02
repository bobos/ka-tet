﻿using System.Collections.Generic;
using UnityEngine;
using UnitNS;
using FieldNS;
using MapTileNS;
using CourtNS;

namespace MonoNS
{
  public class OperationGeneralResult {
    public int result;
    public OperationGeneralResult(int result) {
      this.result = result;
    }

    public string GetResult() {
      if (result == 0) {
        //major defeat
        return Cons.GetTextLib().get("operation_major_defeat");
      }
      if (result == 1) {
        //minor defeat
        return Cons.GetTextLib().get("operation_minor_defeat");
      }
      if (result == 2) {
        //minor victory
        return Cons.GetTextLib().get("operation_minor_victory");
      }
      return Cons.GetTextLib().get("operation_major_victory");
    }
  }

  public class UnitPredict {
    public Unit unit;
    public int percentOfEffectiveForce;
    public int joinPossibility;
    public int operationPoint;
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

    public OperationPredict StartOperation(Unit attacker, Unit targetUnit, bool ImAttacker) {
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
      unitPredict.percentOfEffectiveForce = 100;
      unitPredict.joinPossibility = 100;
      // TODO: Wind buff, tmp morale buff from commander
      unitPredict.operationPoint = attacker.unitAttack;
      predict.attackers.Add(unitPredict);
      predict.attackerOptimPoints += unitPredict.operationPoint;
      hexMap.ShowAttackArrow(attacker, targetUnit,
          unitPredict.joinPossibility, unitPredict.percentOfEffectiveForce, unitPredict.operationPoint);

      foreach(Unit unit in supportAttackers) {
        unitPredict = new UnitPredict();
        unitPredict.unit = unit;
        if (unit.GetStaminaLevel() == StaminaLvl.Vigorous) {
          unitPredict.percentOfEffectiveForce = 100;
        }
        if (unit.GetStaminaLevel() == StaminaLvl.Fresh) {
          unitPredict.percentOfEffectiveForce = 80;
        }
        if (unit.GetStaminaLevel() == StaminaLvl.Tired) {
          unitPredict.percentOfEffectiveForce = 50;
        }
        if (unit.GetStaminaLevel() == StaminaLvl.Exhausted) {
          unitPredict.percentOfEffectiveForce = 0;
        }

        // TODO: join possibility
        unitPredict.joinPossibility = GetJoinPossibility(unit, attacker);
        unitPredict.operationPoint = (int)(unit.unitAttack * unitPredict.percentOfEffectiveForce * 0.001f);
        predict.attackers.Add(unitPredict);
        predict.attackerOptimPoints += unitPredict.operationPoint;
        hexMap.ShowAttackArrow(unit, targetUnit,
          unitPredict.joinPossibility, unitPredict.percentOfEffectiveForce, unitPredict.operationPoint);
      }

      // defenders
      unitPredict = new UnitPredict();
      unitPredict.unit = defender;
      unitPredict.percentOfEffectiveForce = 100;
      unitPredict.joinPossibility = 100;
      // TODO: Wind buff
      unitPredict.operationPoint = attacker.unitDefence;
      predict.defenders.Add(unitPredict);
      predict.defenderOptimPoints += unitPredict.operationPoint;

      foreach(Unit unit in supportDefenders) {
        unitPredict = new UnitPredict();
        unitPredict.unit = unit;
        if (unit.GetStaminaLevel() == StaminaLvl.Vigorous) {
          unitPredict.percentOfEffectiveForce = 100;
        }
        if (unit.GetStaminaLevel() == StaminaLvl.Fresh) {
          unitPredict.percentOfEffectiveForce = 80;
        }
        if (unit.GetStaminaLevel() == StaminaLvl.Tired) {
          unitPredict.percentOfEffectiveForce = 50;
        }
        if (unit.GetStaminaLevel() == StaminaLvl.Exhausted) {
          unitPredict.percentOfEffectiveForce = 0;
        }

        unitPredict.joinPossibility = GetJoinPossibility(unit, targetUnit);
        unitPredict.operationPoint = (int)(unit.unitDefence * unitPredict.percentOfEffectiveForce * 0.001f);
        predict.defenders.Add(unitPredict);
        predict.defenderOptimPoints += unitPredict.operationPoint;
        if (!Util.eq<Unit>(unit, targetUnit)) {
          hexMap.ShowDefendArrow(unit, targetUnit,
            unitPredict.joinPossibility, unitPredict.percentOfEffectiveForce, unitPredict.operationPoint);
        }
      }

      if (predict.defenderOptimPoints >= predict.attackerOptimPoints) {
        if (ImAttacker) {
          predict.sugguestedResult = new OperationGeneralResult(1);
        } else {
          predict.sugguestedResult = new OperationGeneralResult(2);
        }
      } else {
        if (ImAttacker) {
          predict.sugguestedResult = new OperationGeneralResult(2);
        } else {
          predict.sugguestedResult = new OperationGeneralResult(1);
        }
      }

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
      hexMap.unitSelectionPanel.ToggleButtons(true, attacker);
    }

    public void CommenceOperation() {
      // TODO
      // show pop msg
      // ...
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