using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitNS;

namespace MonoNS
{
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