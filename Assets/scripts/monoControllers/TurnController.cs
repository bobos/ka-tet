﻿using System.Collections;
using UnityEngine;
using UnitNS;
using FieldNS;
using TextNS;

namespace MonoNS
{
  public class TurnController : BaseController
  {
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      actionController = hexMap.actionController;
      turnIndicator = hexMap.turnIndicator;
      title = hexMap.turnPhaseTitle;
      settlementMgr = hexMap.settlementMgr;
      cc = hexMap.cameraKeyboardController;
      endingTurn = false;
      if (onNewTurn != null) { onNewTurn(); }
      TurnChange();
    }

    public override void UpdateChild() {}

    public bool playerTurn = false;
    int turnNum = 1;

    void TurnChange()
    {
      playerTurn = !playerTurn;
      string faction = playerTurn ? "玩家" : "AI";
      turnIndicator.Set(turnNum, faction);
      showingTitle = true;
      title.Set(faction);
      StartCoroutine(KeepShowingTitle());
    }

    IEnumerator KeepShowingTitle()
    {
      yield return new WaitForSeconds(1);
      showingTitle = false;
      title.Clear();
    }

    //bool updateReady = false;
    public bool showingTitle { get; private set; }
    TurnPhaseTitle title;
    TurnIndicator turnIndicator;
    ActionController actionController;
    SettlementMgr settlementMgr;
    public bool endingTurn { get; private set; }
    public delegate void OnEndTurnClicked();
    public event OnEndTurnClicked onEndTurnClicked;
    public delegate void OnNewTurn();
    public event OnNewTurn onNewTurn;

    public void onEndTurnClick()
    {
      if (onEndTurnClicked != null) onEndTurnClicked();
      endingTurn = true;
      StartCoroutine(endTurn());
    }

    public bool player = true;
    int cnt = 0;
    TextLib textLib = Cons.GetTextLib();
    CameraKeyboardController cc;
    IEnumerator endTurn()
    {
      WarParty playerParty = hexMap.GetPlayerParty();
      WarParty aiParty = hexMap.GetAIParty();
      WarParty p = player ? playerParty : aiParty;
      WarParty otherP = !player ? playerParty : aiParty;
      player = !player;
      cnt++;
      if (cnt == 2) {
        cnt = 0;
        turnNum++;
      }

      foreach (Unit unit in p.GetUnits())
      {
        if (!unit.waitingForOrders())
        {
          if (!unit.IsCamping()) {
            cc.FixCameraAt(hexMap.GetUnitView(unit).transform.position);
          }
          while (cc.fixingCamera) { yield return null; }
          while (!actionController.move(unit))
          {
            yield return null;
          }
          while (actionController.ActionOngoing) { yield return null; };
        }
        unit.PostAction();
      }

      //TODO
      yield return new WaitForSeconds(1);

      // AI turn
      TurnChange();

      // TODO
      FoW.Get().Fog();
      foreach(Unit u in playerParty.GetUnits()) {
        if (!u.IsCamping()) {
          u.SetState(u.state);
        }
      }
      foreach(Unit u in aiParty.GetUnits()) {
        if (!u.IsCamping()) {
          u.SetState(u.state);
        }
      }

      // fix camera on next party
      Unit fixedAt = null;
      foreach(Unit u in otherP.GetUnits()) {
        fixedAt = u;
      }
      Vector3 cameraPosition;
      if (fixedAt.IsCamping()) {
        cameraPosition = settlementMgr.GetView(fixedAt.tile.settlement).transform.position;
      } else {
        cameraPosition = hexMap.GetUnitView(fixedAt).transform.position;
      }
      cc.FixCameraAt(cameraPosition);

      while (showingTitle || cc.fixingCamera) { yield return null; }
      // refresh AI
      settlementMgr.TurnEnd(otherP);
      while (settlementMgr.turnEndOngoing) { yield return null; }
      foreach (Unit unit in otherP.GetUnits())
      {
        if (!unit.consumed) {
          unit.ConsumeSupply();
        }
        if (unit.starving) {
          View view;
          if (unit.IsCamping()) {
            view = settlementMgr.GetView(unit.tile.settlement);
          } else {
            view = hexMap.GetUnitView(unit);
          }
          cc.FixCameraAt(view.transform.position);
          while (cc.fixingCamera) { yield return null; }

          view.Animating = true;
          view.textView = hexMap.ShowPopText(view, textLib.get("pop_starving"), Color.red);
          while (view.Animating)
          {
            yield return null;
          }
        }
        unit.consumed = false;
        unit.RefreshUnit();
        hexMap.SetUnitSkin(unit);
      }

      // AI Stuff
      /*
      foreach (Unit unit in aiParty.GetUnits())
      {
        if (unit.name == "Ghost") continue;
        Tile[] path = new Tile[2];
        path[0] = hexMap.GetTile(26, 10);
        path[1] = hexMap.GetTile(27, 10);
        unit.SetPath(path);
        while (!actionController.move(unit))
        {
          yield return null;
        }
        while (actionController.ActionOngoing) { yield return null; };
        actionController.PerformImmediateAction(unit, ActionController.actionName.STAND);
        unit.PostAction();
      }
      */

      // refresh player
      /*
      settlementMgrDone = false;
      while (showingTitle) { yield return null; }
      settlementMgr.TurnEnd(playerParty);
      while (!settlementMgrDone) { yield return null; }
      foreach (Unit unit in playerParty.GetUnits())
      {
        if (unit.name == "Ghost") continue;
        if (!unit.consumed) {
          unit.ConsumeSupply();
        }
        unit.consumed = false;
        unit.RefreshUnit();
        hexMap.SetUnitSkin(unit);
      }
      */
      endingTurn = false;
      if (cnt == 0) {
        if (onNewTurn != null) onNewTurn();
      }

      // TurnChange();
      // while (showingTitle) { yield return null; }
    }
  }

}