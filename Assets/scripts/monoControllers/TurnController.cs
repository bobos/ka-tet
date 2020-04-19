using System.Collections;
using UnityEngine;
using UnitNS;
using FieldNS;
using TextNS;
using NatureNS;
using MapTileNS;
using System.Collections.Generic;

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
      if (hexMap.deployDone) {
        TurnChange();
      } else {
        DeploymentPhase();
      }
      if (onNewTurn != null) { onNewTurn(); }
    }

    public void DeploymentDone() {
      TurnChange();
    }

    public override void UpdateChild() {}

    public bool playerTurn = false;
    int turnNum = 1;

    void TurnChange()
    {
      showingTitle = true;
      playerTurn = !playerTurn;
      StartCoroutine(CoShowTurnTile());
    }

    void WarWeary() {
      showingTitle = true;
      title.Set(textLib.get("other_warWeary"), Color.red, "", Color.white);
      StartCoroutine(KeepShowingTitle());
    }

    void DeploymentPhase() {
      showingTitle = true;
      title.Set(textLib.get("title_deployment"), Color.yellow, "", Color.white);
      StartCoroutine(KeepShowingTitle());
    }

    public void ShowTitle(string txt, Color color) {
      showingTitle = true;
      title.Set(txt, color, "", Color.white);
      StartCoroutine(KeepShowingTitle());
    }

    IEnumerator KeepShowingTitle()
    {
      yield return new WaitForSeconds(1);
      showingTitle = false;
      title.Clear();
    }

    IEnumerator CoShowTurnTile() {
      if (playerTurn) {
        yield return new WaitForSeconds(1);
        title.Set(
          System.String.Format(textLib.get("title_days"), turnNum, weatherGenerator.GetWeatherName()),
          Color.white,
          weatherGenerator.GetWeatherExtraInfo(),
          Color.red
          );
        yield return new WaitForSeconds(1);
        title.Clear();
      }
      string faction = textLib.get(playerTurn ? "f_player": "f_AI");
      turnIndicator.Set(turnNum, faction);
      title.Set(textLib.get(playerTurn ? "other_playerTurn" : "other_AITurn"), Color.white, "", Color.white);
      yield return new WaitForSeconds(1);
      title.Clear();
      showingTitle = false;
    }

    public bool sleeping = false;
    public void Sleep(int sec) {
      sleeping = true;
      StartCoroutine(CoSleep(sec));
    }

    IEnumerator CoSleep(int sec) {
      yield return new WaitForSeconds(sec);
      sleeping = false;
    }

    //bool updateReady = false;
    public bool showingTitle { get; private set; }
    TurnPhaseTitle title;
    TurnIndicator turnIndicator;
    ActionController actionController;
    SettlementMgr settlementMgr;
    WeatherGenerator weatherGenerator {
      get {
        return hexMap.weatherGenerator;
      }
    }
    UnitAnimationController unitAniController {
      get {
        return hexMap.unitAniController;
      }
    }
    TileAnimationController tileAniController {
      get {
        return hexMap.tileAniController;
      }
    }

    EventStasher eventStasher {
      get {
        return hexMap.eventStasher;
      }
    }

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
      hexMap.cameraKeyboardController.DisableCamera();
      WarParty playerParty = hexMap.GetPlayerParty();
      WarParty aiParty = hexMap.GetAIParty();
      WarParty p = player ? playerParty : aiParty;
      WarParty otherP = !player ? playerParty : aiParty;

      foreach (Unit unit in p.GetUnits())
      {
        if (!unit.waitingForOrders())
        {
          if (!unit.IsCamping()) {
            cc.FixCameraAt(hexMap.GetUnitView(unit).transform.position);
            while (cc.fixingCamera) { yield return null; }
          }
          while (!actionController.move(unit))
          {
            yield return null;
          }
          while (actionController.ActionOngoing) { yield return null; };
        }
        unitAniController.PostTurnAction(unit);
        while (unitAniController.PostAnimating) { yield return null; };
      }

      player = !player;
      cnt++;
      if (cnt == 2) {
        cnt = 0;
        turnNum++;
        weatherGenerator.NextDay();
        hexMap.windGenerator.NextDay(weatherGenerator.IsTomorrowMist());
        if (onNewTurn != null) { 
          onNewTurn();
        }
      }
      TurnChange();
      if (cnt == 0) {
        if (turnNum > 20) {
          // starts to drop morale
          if (turnNum == 21) {
            WarWeary();
          }

          WarParty atkParty = playerParty.attackside ? playerParty : aiParty;
          WarParty defParty = playerParty.attackside ? aiParty : playerParty;
          foreach (Unit unit in atkParty.GetUnits()) {
            if (unit.rf.morale >= Rank.MoralePunishLine(unit.rf.IsSpecial())) {
              unit.rf.morale -= 2;
            }
          }

          if ((turnNum % 2) != 0) {
            foreach(Unit unit in defParty.GetUnits()) {
              if (unit.rf.morale >= Rank.MoralePunishLine(unit.rf.IsSpecial())) {
                unit.rf.morale -= 2;
              }
            }
          }
        }
      }

      // TODO
      p.ResetDiscoveredTiles();
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
      List<Tile> controlledTiles = settlementMgr.GetControlledTiles(otherP.isAI);
      List<Unit> units = new List<Unit>(); 
      Tile tile = settlementMgr.GetRoot(otherP.isAI).baseTile;
      foreach (Unit u in otherP.GetUnits()) {
        units.Add(u);
      }
      // order unit from near to far
      units.Sort(delegate (Unit a, Unit b)
      {
        return (int)(Tile.Distance(tile, a.tile) - Tile.Distance(tile, b.tile));
      });

      // Refresh unit at new turn
      foreach (Unit unit in units)
      {
        // consume supply
        int[] effects = new int[5]{0,0,0,0,0};
        unit.supply.Consume(effects, controlledTiles);
        if (!unit.supply.consumed) {
          View view;
          if (unit.IsCamping()) {
            view = settlementMgr.GetView(unit.tile.settlement);
          } else {
            view = hexMap.GetUnitView(unit);
          }
          hexMap.popAniController.Show(view,
            textLib.get("pop_starving"),
            Color.white);
          while (hexMap.popAniController.Animating)
          {
            yield return null;
          }
          unitAniController.ShowEffect(unit, effects);
          while (unitAniController.ShowAnimating) { yield return null; }
        }
        
        unitAniController.RefreshUnit(unit);
        while (unitAniController.RefreshAnimating) { yield return null; }
        hexMap.SetUnitSkin(unit);
      }
      Dictionary<HashSet<Unit>, HashSet<Tile>> spaces = otherP.GetFreeSpaces();
      HashSet<Unit> surroundedUnits = new HashSet<Unit>();
      foreach(KeyValuePair<HashSet<Unit>, HashSet<Tile>> kvp in spaces) {
        if (kvp.Value.Count == 0) {
          // unit group is surrounded
          unitAniController.UnitSurrounded(kvp.Key, surroundedUnits);
          while (unitAniController.SurroundAnimating) { yield return null; }
        }
      }
      foreach(Unit u in units) {
        if (!surroundedUnits.Contains(u)) {
          // reset surround count
          u.surroundCnt = 0;
        }
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
        foreach(Tile t in weatherGenerator.tileCB) {
          tileAniController.WeatherChange(t, weatherGenerator.currentWeather);
          while (tileAniController.WeatherAnimating) { yield return null; }
        }
      }

      hexMap.cameraKeyboardController.EnableCamera();
      // TurnChange();
      // while (showingTitle) { yield return null; }
    }
  }

}