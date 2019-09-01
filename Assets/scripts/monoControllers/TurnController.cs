using System.Collections;
using UnityEngine;
using UnitNS;
using FieldNS;

namespace MonoNS
{
  public class TurnController : BaseController
  {
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      hexMap.settlementMgr.onJobDone += OnSettlementJobDone;
      actionController = hexMap.actionController;
      turnIndicator = hexMap.turnIndicator;
      title = hexMap.turnPhaseTitle;
      settlementMgr = hexMap.settlementMgr;
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
    private bool settlementMgrDone = true;

    public void onEndTurnClick()
    {
      if (onEndTurnClicked != null) onEndTurnClicked();
      endingTurn = true;
      StartCoroutine(endTurn());
    }

    public void OnSettlementJobDone()
    {
      settlementMgrDone = true;
    }

    public bool player = true;
    int cnt = 0;
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
      foreach(Unit u in playerParty.GetUnits()) {
        if (u.state != State.Camping) {
          u.SetState(u.state);
        }
      }
      foreach(Unit u in aiParty.GetUnits()) {
        if (u.state != State.Camping) {
          u.SetState(u.state);
        }
      }
      FoW.Get().Fog(!playerTurn);

      while (showingTitle) { yield return null; }
      // refresh AI
      settlementMgrDone = false;
      settlementMgr.TurnEnd(otherP);
      while (!settlementMgrDone) { yield return null; }
      foreach (Unit unit in otherP.GetUnits())
      {
        if (!unit.consumed) {
          unit.ConsumeSupply();
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