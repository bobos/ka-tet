﻿using System.Collections;
using UnitNS;
using CourtNS;
using TextNS;
using UnityEngine;
using MapTileNS;
using FieldNS;
using System.Collections.Generic;

namespace MonoNS
{
  public class UnitAnimationController : BaseController
  {
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      settlementMgr = hexMap.settlementMgr;
      eventDialog = hexMap.eventDialog;
      popAniController = hexMap.popAniController;
    }

    SettlementMgr settlementMgr;
    EventDialog eventDialog;
    PopTextAnimationController popAniController;
    TextLib textLib = Cons.GetTextLib();

    public override void UpdateChild() {}

    public bool DestroyAnimating = false;
    public void DestroyUnit(Unit unit, DestroyType type, bool generalDead = false)
    {
      // TODO: check if this is the last unit, if so, campaign ends
      DestroyAnimating = true;
      int killed = unit.Destroy();
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoDestroy(unit, type, killed, generalDead));
    }

    IEnumerator CoDestroy(Unit unit, DestroyType type, int killed, bool generalDead)
    {
      if (!unit.IsCamping()) {
        UnitView view = hexMap.GetUnitView(unit);
        view.DestroyAnimation(type);
        while (view.Animating) { yield return null; }
        hexMap.DestroyUnitView(unit);
      }

      if (type == DestroyType.ByWildFire) {
        eventDialog.Show(new MonoNS.Event(EventDialog.EventName.WildFireDestroyUnit, unit, null, 0, 0, killed));
      } else if (type == DestroyType.ByFlood) {
        eventDialog.Show(new MonoNS.Event(EventDialog.EventName.FloodDestroyUnit, unit, null, 0, 0, killed));
      } else {
        eventDialog.Show(new MonoNS.Event(EventDialog.EventName.Disbanded, unit, null, 0, 0, killed));
      }
      while (eventDialog.Animating) { yield return null; }

      General general = unit.rf.general;
      bool isCommander = unit.IsCommander();
      general.TroopDestroyed(generalDead);

      if (general.IsDead()) {
        eventDialog.Show(new MonoNS.Event(EventDialog.EventName.GeneralKilledInBattle, null, null, 0, 0, 0, 0, 0, null, general));
        while (eventDialog.Animating) { yield return null; }
      } else {
        eventDialog.Show(new MonoNS.Event(EventDialog.EventName.GeneralRetreated, null, null, 0, 0, 0, 0, 0, null, general));
        while (eventDialog.Animating) { yield return null; }
      }
      if (isCommander) {
        Unit newCommander = hexMap.GetWarParty(unit).AssignNewCommander();
        popAniController.Show(
          newCommander.IsOnField() ? hexMap.GetUnitView(newCommander):
          hexMap.settlementMgr.GetView(newCommander.tile.settlement),
          textLib.get("pop_newCommander"), Color.white);
        while(popAniController.Animating) { yield return null; }
      }

      ShakeNearbyAllies(unit);
      while (ShakeAnimating) { yield return null; }
      int drop = unit.IsCommander() ? -90 : -60;
      foreach(Unit u in hexMap.GetWarParty(unit).GetUnits()) {
        u.morale += drop;
        ShowEffect(u, new int[]{drop, 0, 0, 0, 0}, null, true);
      }
      hexMap.turnController.Sleep(1);
      while(hexMap.turnController.sleeping) { yield return null; }
      hexMap.cameraKeyboardController.EnableCamera();
      DestroyAnimating = false;
    }

    public bool ShakeAnimating = false;
    public void ShakeNearbyAllies(Unit unit) {
      ShakeAnimating = true;
      StartCoroutine(CoShakeNearbyAllies(unit));
    }

    IEnumerator CoShakeNearbyAllies(Unit unit) {
      foreach(Tile t in unit.tile.GetNeighboursWithinRange<Tile>(4, (Tile tt) => true)) {
        Unit u = t.GetUnit();
        if (u != null && u.IsAI() == unit.IsAI() && !Util.eq<Unit>(unit, u)) {
          if (u.RetreatOnDefeat() && u.SetRetreatPath()) {
            ForceRetreat(u, 60);
            while(ForceRetreatAnimating) { yield return null; }
          }
        }
      }
      ShakeAnimating = false;
      hexMap.cameraKeyboardController.EnableCamera();
    }

    public bool MoveAnimating = false;
    public bool MoveUnit(Unit unit, Tile tile = null, bool dontFixCamera = false, bool normalMove = true) {
      if(!unit.DoMove(tile)) {
        return false;
      }
      MoveAnimating = true;
      int moraleDrop = 0;
      if (tile == null) {
        moraleDrop = unit.marchOnHeat.Occur();
      }
      if (unit.IsShowingAnimation() && !dontFixCamera) {
        hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(unit.tile).transform.position);
      }
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoMoveUnit(unit, moraleDrop, normalMove));
      return true;
    }

    IEnumerator CoMoveUnit(Unit unit, int moraleDrop, bool normalMove) {
      UnitView view = hexMap.GetUnitView(unit);
      view.Move(unit.tile);
      while (view.Animating) { yield return null; }
      int r = unit.altitudeSickness.Occur();
      if (r != 0) {
        // altitude sickness
        if (unit.IsShowingAnimation()) {
          popAniController.Show(view, textLib.get("pop_altitudeSickness"), Color.white);
          while(popAniController.Animating) { yield return null; }
        }
        ShowEffect(unit, new int[]{r,0,0,0,0});
        unit.morale += r;
        while(ShowAnimating) { yield return null; }
      }
      if (moraleDrop != 0) {
        unit.morale += moraleDrop;
        ShowEffect(unit, new int[]{moraleDrop,0,0,0,0});
        while(ShowAnimating) { yield return null; }
      }
      if (normalMove) {
        Unit ambusher = hexMap.GetWarParty(unit, true).GetAmbusher(unit);
        if (ambusher != null) {
          hexMap.UnsetPath(unit);
          if (!unit.IsAI()) {
            hexMap.mouseController.Escape();
          }
        }
      }
      // stash event
      // hexMap.eventStasher.Add(unit.rf.general, MonoNS.EventDialog.EventName.FarmDestroyed);

      hexMap.cameraKeyboardController.EnableCamera();
      //TODO: optimize, after each step, recalculate fog
      FoW.Get().Fog(hexMap.allTiles);
      hexMap.mouseController.RefreshUnitPanel(unit);
      MoveAnimating = false;
    }

    public bool PostAnimating = false;
    public void PostTurnAction(Unit unit)
    {
      PostAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoPostTurnAction(unit));
    }

    IEnumerator CoPostTurnAction(Unit unit) {
      if (unit.rf.soldiers <= Unit.DisbandUnitUnder)
      {
        hexMap.unitAniController.DestroyUnit(unit, DestroyType.ByDisband);
        while (hexMap.unitAniController.DestroyAnimating) { yield return null; }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      PostAnimating = false;
    }

    public bool AttackEmptyAnimating = false;
    public void AttackEmpty(Unit unit, Settlement settlement) {
      AttackEmptyAnimating = true;
      StartCoroutine(CoAttackEmpty(unit, settlement));
    }

    IEnumerator CoAttackEmpty(Unit unit, Settlement settlement) {
      hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(settlement.baseTile).transform.position);
      while (hexMap.cameraKeyboardController.fixingCamera) { yield return null; }
      hexMap.cameraKeyboardController.DisableCamera();
      int[] afterMath = settlementMgr.OccupySettlement(unit, settlement);
      if (settlement.type == Settlement.Type.city) {
        eventDialog.Show(new MonoNS.Event(
          unit.IsAI() ? MonoNS.EventDialog.EventName.EnemyCaptureCity : MonoNS.EventDialog.EventName.WeCaptureCity,
        unit,
        settlement,
        afterMath[0], afterMath[1], afterMath[2], afterMath[3]));
      } else {
        eventDialog.Show(new MonoNS.Event(
          unit.IsAI() ? MonoNS.EventDialog.EventName.EnemyCaptureCamp : MonoNS.EventDialog.EventName.WeCaptureCamp,
        unit,
        settlement, afterMath[3]));
      }
      while (eventDialog.Animating) { yield return null; }
      hexMap.cameraKeyboardController.EnableCamera();
      AttackEmptyAnimating = false;
    }

    public bool TakeAnimating = false;
    public void TakeSettlement(Unit unit, Settlement settlement) {
      TakeAnimating = true;
      StartCoroutine(CoTakeSettlement(unit, settlement));
    }

    IEnumerator CoTakeSettlement(Unit unit, Settlement settlement) {
      hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(settlement.baseTile).transform.position);
      while (hexMap.cameraKeyboardController.fixingCamera) { yield return null; }
      hexMap.cameraKeyboardController.DisableCamera();
      hexMap.turnController.ShowTitle(textLib.get("title_settlementTaken"), Color.red);
      while (hexMap.turnController.showingTitle) { yield return null; }

      List<Unit> tmp = new List<Unit>();
      foreach (Unit g in settlement.garrison) {
        tmp.Add(g);
      }

      foreach (Unit g in tmp) {
        bool killed = false;
        if ((g.rf.general.Is(Cons.brave) || g.rf.general.Is(Cons.loyal))
          && Cons.MostLikely()) {
          killed = true;
        } else {
          killed = Cons.EvenChance();
        }

        Tile tile = g.FindBreakThroughPoint();
        if (killed || tile == null) {
          DestroyUnit(g, DestroyType.ByDisband, true);
          while (DestroyAnimating) { yield return null; }
        } else {
          settlement.Decamp(g, tile);
          if(g.SetRetreatPath()) {
            ForceRetreat(g, 150, true);
            while (ForceRetreatAnimating) { yield return null; }
          }
        }
      }

      int[] afterMath = settlementMgr.OccupySettlement(unit, settlement);
      if (settlement.type == Settlement.Type.city) {
        eventDialog.Show(new MonoNS.Event(
          unit.IsAI() ? MonoNS.EventDialog.EventName.EnemyCaptureCity : MonoNS.EventDialog.EventName.WeCaptureCity,
        unit,
        settlement,
        afterMath[0], afterMath[1], afterMath[2], afterMath[3]));
      } else {
        eventDialog.Show(new MonoNS.Event(
          unit.IsAI() ? MonoNS.EventDialog.EventName.EnemyCaptureCamp : MonoNS.EventDialog.EventName.WeCaptureCamp,
        unit,
        settlement, afterMath[3]));
      }
      while (eventDialog.Animating) { yield return null; }
      hexMap.cameraKeyboardController.EnableCamera();
      TakeAnimating = false;
    }

    public bool SurroundAnimating = false;
    public void UnitSurrounded(HashSet<Unit> units, HashSet<Unit> accu) {
      SurroundAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoUnitSurrounded(units, accu));
    }

    IEnumerator CoUnitSurrounded(HashSet<Unit> units, HashSet<Unit> accu) {
      const int moraleDrop = -10;
      bool first = true;
      foreach(Unit unit in units) {
        accu.Add(unit);
        if (first) {
          hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(unit.tile).transform.position);
          hexMap.turnController.ShowTitle(Cons.GetTextLib().get("title_noWayOut"), Color.white);
          while(hexMap.turnController.showingTitle) { yield return null; }
          first = false;
        }
        unit.surroundCnt++;
        int drop = moraleDrop * unit.surroundCnt;
        unit.morale += drop;
        ShowEffect(unit, new int[]{drop,0,0,0,0});
      }
      hexMap.turnController.Sleep(1);
      while(hexMap.turnController.sleeping) { yield return null; }
      hexMap.cameraKeyboardController.EnableCamera();
      SurroundAnimating = false;
    }

    public bool RefreshAnimating = false;
    public void RefreshUnit(Unit unit) {
      RefreshAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoRefreshUnit(unit));
    }

    IEnumerator CoRefreshUnit(Unit unit) {
      ShowEffect(unit, unit.RefreshUnit());
      while (ShowAnimating) { yield return null; }

      UnitView view = hexMap.GetUnitView(unit);
      if (unit.IsWarWeary())
      {
        if (unit.IsShowingAnimation()) {
          popAniController.Show(view, textLib.get("pop_warWeary"), Color.yellow);
          while (popAniController.Animating) { yield return null; }
        }
        ShowEffect(unit, unit.warWeary.Apply());
        while (ShowAnimating) { yield return null; }
      }

      if (unit.IsHeatSicknessAffected()) {
        if (unit.IsShowingAnimation()) {
          popAniController.Show(view, textLib.get("pop_sickness"), Color.yellow);
          while (popAniController.Animating) { yield return null; }
        }
        ShowEffect(unit, unit.epidemic.Apply());
        while (ShowAnimating) { yield return null; }
      }

      if (unit.altitudeSickness.lastTurns > 0) {
        if (unit.IsShowingAnimation()) {
          popAniController.Show(view, textLib.get("pop_altitudeSickness"), Color.yellow);
          while (popAniController.Animating) { yield return null; }
        }
        ShowEffect(unit, new int[5]{0,0,unit.altitudeSickness.Apply(),0,0});
        while (ShowAnimating) { yield return null; }
      }

      if (unit.IsPoisioned()) {
        if (unit.IsShowingAnimation()) {
          popAniController.Show(view, textLib.get("pop_poisioned"), Color.yellow);
          while (popAniController.Animating) { yield return null; }
        }
        ShowEffect(unit, unit.unitPoisioned.Apply());
        while (ShowAnimating) { yield return null; }
      }

      if (unit.tile.deadZone.Apply(unit) && Cons.EvenChance() && unit.epidemic.Occur()) {
        // epimedic caused by decomposing corpse
        popAniController.Show(hexMap.GetUnitView(unit), textLib.get("pop_epidemic"), Color.white);
        while (popAniController.Animating) { yield return null; }
      }

      if (Cons.IsHeat(hexMap.weatherGenerator.currentWeather)) {
        int ret = unit.armorRemEvent.Occur();
        WarParty wp = hexMap.GetWarParty(unit);
        if(ret != 0) {
          if (ret < 0) {
            // disarmor not allowed
            if (wp.firstRemoveArmor == null) {
              hexMap.dialogue.ShowRemoveHelmet(unit, false);
            } else {
              hexMap.dialogue.ShowRemoveHelmetFollow(unit, false);
            }
            while (hexMap.dialogue.Animating) { yield return null; }
            unit.morale += ret;
            ShowEffect(unit, new int[]{ret,0,0,0,0});
            while(ShowAnimating) { yield return null; }
          } else {
            // disarmor allowed
            int defReduce = ret;
            if (wp.firstRemoveArmor == null) {
              wp.firstRemoveArmor = unit;
              hexMap.dialogue.ShowRemoveHelmet(unit, true);
            } else {
              hexMap.dialogue.ShowRemoveHelmetFollow(unit, true);
            }
            while (hexMap.dialogue.Animating) { yield return null; }
            unit.disarmorDefDebuf = defReduce * 0.01f; 
          }
        }
      }

      if (Cons.FiftyFifty()) {
        int ret = unit.plainSickness.Occur();
        if (ret != 0) {
          unit.morale += ret;
          if (unit.IsShowingAnimation()) {
            popAniController.Show(hexMap.GetUnitView(unit), textLib.get("pop_plainSickness"), Color.yellow);
            while (popAniController.Animating) { yield return null; }
          }
        }
      }

      int moraleDrop = unit.inCampComplain.Occur();
      if (moraleDrop != 0) {
        unit.morale += moraleDrop;
        hexMap.dialogue.ShowInCampComplain(unit.rf.province.region);
        while(hexMap.dialogue.Animating) { yield return null; }
        ShowEffect(unit, new int[]{moraleDrop, 0, 0, 0, 0}, settlementMgr.GetView(unit.tile.settlement));
        while(ShowAnimating) { yield return null; }
      }

      moraleDrop = unit.onFieldComplain.Occur();
      if (moraleDrop != 0) {
        unit.morale += moraleDrop;
        hexMap.dialogue.ShowOnFieldComplain(unit.rf.province.region);
        while(hexMap.dialogue.Animating) { yield return null; }
        ShowEffect(unit, new int[]{moraleDrop, 0, 0, 0, 0});
        while(ShowAnimating) { yield return null; }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      RefreshAnimating = false;
    }

    public bool PoisionAnimating = false;
    public void Poision(Unit unit, Tile tile) {
      PoisionAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoPoision(unit, tile));
    }

    IEnumerator CoPoision(Unit unit, Tile tile) {
      foreach(Unit u in tile.Poision(unit)) {
        popAniController.Show(hexMap.GetUnitView(u), textLib.get("pop_poisioned"), Color.white);
        while (popAniController.Animating) { yield return null; }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      PoisionAnimating = false;
    }

    public bool BuryAnimating = false;
    public void Bury(Unit unit) {
      if (unit.type != Type.Infantry) {
        return;
      }
      BuryAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoBury(unit));
    }

    IEnumerator CoBury(Unit unit) {
      if (!unit.ApplyDiscipline()) {
        int moraleDrop = -10;
        unit.morale += moraleDrop;
        ShowEffect(unit, new int[]{moraleDrop,0,0,0,0});
        while(ShowAnimating) { yield return null; }
      }
      unit.tile.deadZone.Clean();

      hexMap.cameraKeyboardController.EnableCamera();
      BuryAnimating = false;
    }

    public bool ScatterAnimating = false;
    public void Scatter(List<Unit> units, bool oneStep = false) {
      ScatterAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoScatter(units, oneStep));
    }

    IEnumerator CoScatter(List<Unit> units, bool oneStep) {
      Dictionary<Tile, bool> tiles = new Dictionary<Tile, bool>();
      Dictionary<Unit, List<Tile>> plan = new Dictionary<Unit, List<Tile>>();
      Unit loser = units[0];
      Tile baseTile = loser.tile;
      foreach(Tile t in baseTile.GetNeighboursWithinRange(10, (Tile _tile) => true)) {
        tiles[t] = !t.Deployable(loser);
      }

      // sort units from far to near
      units.Sort(delegate (Unit a, Unit b)
      {
        return (int)(Tile.Distance(baseTile, b.tile) - Tile.Distance(baseTile, a.tile));
      });

      foreach(Unit unit in units) {
        // first step
        Tile step1 = null;
        List<Unit> ally = unit.OnFieldAllies();
        foreach (Tile t in unit.tile.neighbours) {
          if (!tiles[t]) {
            // the tile is not taken
            step1 = t;
            break;
          }
        }

        if (oneStep) {
          plan[unit] = step1 == null ? null : new List<Tile>{step1};
          continue;
        }

        if (step1 == null) {
          // look for ally's nearby tile
          foreach(Unit u in ally) {
            foreach(Tile t in u.tile.neighbours) {
              if (!Util.eq<Tile>(t, unit.tile) && t.Deployable(unit)) {
                step1 = t;
                break;
              }
            }
          }
          if (step1 == null) {
            plan[unit] = null;
            continue;
          }
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

      List<Unit> failedToMove = new List<Unit>();
      // move all unit at once
      foreach(Unit unit in units) {
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

      foreach(Unit unit in failedToMove) {
        foreach(Unit u in unit.OnFieldAllies()) {
          if (!units.Contains(u)) {
            hexMap.unitAniController.CrashByAlly(u, -25);
            while (hexMap.unitAniController.CrashAnimating) { yield return null; }
            continue;
          }
        }
      }

      hexMap.cameraKeyboardController.EnableCamera();
      ScatterAnimating = false;
    }

    public bool RallyAnimating = false;
    public void Rally(Unit from) {
      RallyAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoRally(from));
    }

    IEnumerator CoRally(Unit from) {
      List<Unit> units = from.CanRally();
      int incr = from.RallyAlly();
      View view = hexMap.GetUnitView(from);
      popAniController.Show(view, textLib.get("pop_rally"), Color.green);
      while (popAniController.Animating) { yield return null; }
      foreach(Unit u in units) {
        u.morale += incr;
        ShowEffect(u, new int[]{incr,0,0,0,0}, null, true);
      }
      hexMap.turnController.Sleep(1);
      while(hexMap.turnController.sleeping) { yield return null; }

      hexMap.cameraKeyboardController.EnableCamera();
      RallyAnimating = false;
    }

    public bool ChargeAnimating = false;
    public void Charge(Unit from, Unit to) {
      ChargeAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoCharge(from, to));
    }

    IEnumerator CoCharge(Unit from, Unit to) {
      from.Charge();
      View view = from.IsCamping() ?
        settlementMgr.GetView(from.tile.settlement) : hexMap.GetUnitView(from);

      popAniController.Show(view, textLib.get("pop_charging"), Color.green);
      while (popAniController.Animating) { yield return null; }
      int chance = from.morale - to.morale;
      if (Cons.IsGale(hexMap.windGenerator.current)) {
        WindAdvantage advantage = from.tile.GetGaleAdvantage(to.tile);
        if (advantage == WindAdvantage.Advantage) {
          chance += 50;
        }
        if (advantage == WindAdvantage.Disadvantage) {
          chance -= 50;
        }
      }
      if (Holder.Aval(to)) {
        chance -= Holder.ChanceToHold;
        Holder.Get(to.rf.general).Consume();
      }
      if (to.rf.general.Is(Cons.cunning)) {
        chance += 30;
      }
      bool scared = chance >= Util.Rand(1, 100);
      // morale, movement, killed, attack, def
      ShowEffect(from, new int[]{0,0,from.Killed(Util.Rand(2, 15)),0,0}, view);
      while (ShowAnimating) { yield return null; }
      if (scared) {
        Scatter(new List<Unit>{to}, !Pusher.Aval(from));
        while(ScatterAnimating) { yield return null; }
      } else {
        popAniController.Show(hexMap.GetUnitView(to), textLib.get("pop_holding"), Color.green);
        while (popAniController.Animating) { yield return null; }
        int morale = -5;
        from.morale += morale;
        ShowEffect(from, new int[]{morale,0,0,0,0}, view, true);
      }
      int toDrop = -5;
      toDrop = Pusher.Aval(from) ? (toDrop - Pusher.ExtMoraleDrop) : toDrop;
      to.morale += toDrop;
      ShowEffect(to, new int[]{toDrop,0,0,0,0}, null, true);
      hexMap.cameraKeyboardController.EnableCamera();
      ChargeAnimating = false;
    }

    public bool BreakAnimating = false;
    public void Break(Unit from, Unit to, bool doubleTheKill = false) {
      BreakAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoBreak(from, to, doubleTheKill));
    }

    IEnumerator CoBreak(Unit from, Unit to, bool doubleTheKill) {
      from.UseAtmpt();
      View view = from.IsCamping() ?
        settlementMgr.GetView(from.tile.settlement) : hexMap.GetUnitView(from);
      popAniController.Show(view, textLib.get("pop_chasing"), Color.green);
      while (popAniController.Animating) { yield return null; }
      List<Unit> allies = to.OnFieldAllies(true);
      int orgBuf = from.rf.org > Region.RookieOrg ? (from.rf.org - Region.RookieOrg) : 0;
      int dead = (int)(from.rf.soldiers * (from.IsCavalry() ? 0.75f : 0.2f) * (1f + orgBuf / 100));
      dead = to.IsCavalry() ? (int)(dead / 4) : dead;
      if (doubleTheKill && Finisher.Aval(from) && Finisher.Get(from.rf.general).Consume()) {
        dead = (int)(dead * (1f + Finisher.KillBuf));
      }
      dead = dead > to.rf.soldiers ? to.rf.soldiers : dead;
      ShowEffect(to, new int[]{0,0,to.Killed(dead),0,0});
      while (ShowAnimating) { yield return null; }
      if (to.rf.soldiers <= Unit.DisbandUnitUnder) {
        // unit disbanded
        hexMap.unitAniController.DestroyUnit(to, DestroyType.ByDisband);
        while (hexMap.unitAniController.DestroyAnimating) { yield return null; }
        allies.Remove(to);
      }
      to.tile.deadZone.Occur(dead);
      hexMap.turnController.ShowTitle(Cons.GetTextLib().get("title_formationBreaking"), Color.red);
      while(hexMap.turnController.showingTitle) { yield return null; }
      hexMap.dialogue.ShowFormationBreaking(from);
      while(hexMap.dialogue.Animating) { yield return null; }
      hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetUnitView(from).transform.position);
      while(hexMap.cameraKeyboardController.fixingCamera) { yield return null; }
      hexMap.unitAniController.Scatter(allies);
      while(hexMap.unitAniController.ScatterAnimating) { yield return null; }
      ShowEffect(from, new int[]{from.Victory(5), 0, 0, 0, 0});
      while( ShowAnimating ) { yield return null; }
      hexMap.cameraKeyboardController.EnableCamera();
      BreakAnimating = false;
    }

    public bool BreakthroughAnimating = false;
    public void BreakThrough(Unit from, Unit to) {
      BreakthroughAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoBreakThrough(from, to));
    }

    IEnumerator CoBreakThrough(Unit from, Unit to) {
      from.UseAtmpt();
      View view = from.IsCamping() ?
        settlementMgr.GetView(from.tile.settlement) : hexMap.GetUnitView(from);

      int chance = from.morale - to.morale;
      if (Cons.IsGale(hexMap.windGenerator.current)) {
        WindAdvantage advantage = from.tile.GetGaleAdvantage(to.tile);
        if (advantage == WindAdvantage.Advantage) {
          chance += 55;
        }
        if (advantage == WindAdvantage.Disadvantage) {
          chance -= 55;
        }
      }
      if (Holder.Aval(to)) {
        chance += Holder.ChanceToHold;
        Holder.Get(to.rf.general).Consume();
      }
      bool scared = to.IsVulnerable() || chance >= Util.Rand(1, 100);
      popAniController.Show(view, textLib.get("pop_toBreakThrough"), Color.green);
      while (popAniController.Animating) { yield return null; }
      // morale, movement, killed, attack, def
      ShowEffect(from, new int[]{0,0,from.Killed(Util.Rand(20, 50)),0,0}, view);
      while (ShowAnimating) { yield return null; }
      if (scared) {
        Tile escapeTile = null;
        foreach(Tile t in to.tile.neighbours) {
          if (t.Deployable(from)) {
            escapeTile = t;
            break;
          }
        }

        if (escapeTile == null) {
          popAniController.Show(view,
            textLib.get("pop_failedToBreakthrough"),
            Color.white);
          while (popAniController.Animating) { yield return null; }
        } else {
          if (from.IsCamping()) {
            from.tile.settlement.Decamp(from, escapeTile);
          } else {
            MoveUnit(from, escapeTile);
            while (MoveAnimating) { yield return null; }
          }
          popAniController.Show(view,
            textLib.get("pop_breakthrough"),
            Color.green);
          while (popAniController.Animating) { yield return null; }
        }
      } else {
        popAniController.Show(view,
          textLib.get("pop_failedToBreakthrough"),
          Color.white);
        while (popAniController.Animating) { yield return null; }
        int morale = -10;
        from.morale += morale;
        ShowEffect(from, new int[]{morale,0,0,0,0}, view, true);
        while(ShowAnimating) { yield return null; }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      BreakthroughAnimating = false;
    }

    public bool CrashAnimating = false;
    public void CrashByAlly(Unit unit, int morale) {
      if (!unit.CanBeCrashed()) {
        return;
      }
      unit.crashed = true;
      CrashAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoCrashByAlly(unit, morale));
    }

    IEnumerator CoCrashByAlly(Unit unit, int morale) {
      popAniController.Show(hexMap.GetUnitView(unit), textLib.get("pop_crashedByAlly"), Color.white);
      while (popAniController.Animating) { yield return null; }
      // morale, movement, killed, attack, def
      ShowEffect(unit, new int[]{morale,0,unit.Killed(Util.Rand(40, 100)),0,0});
      while (ShowAnimating) { yield return null; }
      unit.morale += morale;
      if (unit.rf.soldiers <= Unit.DisbandUnitUnder) {
        // unit disbanded
        hexMap.unitAniController.DestroyUnit(unit, DestroyType.ByDisband);
        while (hexMap.unitAniController.DestroyAnimating) { yield return null; }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      CrashAnimating = false;
    }

    public bool SiegeAnimating = false;
    public void Siege(Unit unit) {
      Settlement target = GetSettlement(unit);
      if (target == null || target.IsUnderSiege() && unit.tile.siegeWall != null) {
        return;
      }
      SiegeAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoSiege(unit));
    }

    Settlement GetSettlement(Unit unit) {
      Settlement settlement = null;
      foreach (Tile tile in unit.tile.neighbours) {
        if (tile.settlement != null && tile.settlement.owner.isAI != unit.IsAI()) {
          settlement = tile.settlement;
          break;
        }
      }

      return settlement;
    }

    IEnumerator CoSiege(Unit unit) {
      if(!hexMap.settlementMgr.BuildSiegeWall(unit, hexMap.GetWarParty(unit))) {
        if (unit.IsShowingAnimation()) {
          popAniController.Show(hexMap.GetUnitView(unit),
          textLib.get("pop_buildFail"), Color.yellow);
          while (popAniController.Animating) { yield return null; }
        }
      } else {
        Region region = unit.rf.province.region;
        if ((Cons.IsQidan(region) || Cons.IsDangxiang(region)) && 
          !unit.ApplyDiscipline()) {
          hexMap.dialogue.ShowSiegeComplain(region);
          while(hexMap.dialogue.Animating) { yield return null; }
          int moraleDrop = -20;
          unit.morale += moraleDrop;
          ShowEffect(unit, new int[]{moraleDrop,0,0,0,0});
          while(ShowAnimating) { yield return null; }
        }
        if (unit.IsShowingAnimation()) {
          popAniController.Show(hexMap.GetUnitView(unit),
          textLib.get("pop_sieging"), Color.green);
          while (popAniController.Animating) { yield return null; }
        }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      SiegeAnimating = false;
    }

    public bool RetreatAnimating = false;
    public void Retreat(Unit unit) {
      RetreatAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoRetreat(unit));
    }

    IEnumerator CoRetreat(Unit unit) {
      hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(unit.tile).transform.position);
      while(hexMap.cameraKeyboardController.fixingCamera) { yield return null; }
      hexMap.dialogue.ShowRetreat(unit);
      while(hexMap.dialogue.Animating) { yield return null; }
      unit.Retreat();
      if (unit.IsCommander()) {
        Unit newCommander = hexMap.GetWarParty(unit).AssignNewCommander();
        popAniController.Show(
          newCommander.IsOnField() ? hexMap.GetUnitView(newCommander):
          hexMap.settlementMgr.GetView(newCommander.tile.settlement),
          textLib.get("pop_newCommander"), Color.white);
        while(popAniController.Animating) { yield return null; }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      RetreatAnimating = false;
    }

    public bool DecieveAnimating = false;
    public void Decieve(Unit unit, Unit target) {
      DecieveAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoDecieve(unit, target));
    }

    IEnumerator CoDecieve(Unit unit, Unit target) {
      hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(target.tile).transform.position);
      while(hexMap.cameraKeyboardController.fixingCamera) { yield return null; }
      if (unit.Decieve(target)) {
        popAniController.Show(hexMap.GetUnitView(target), textLib.get("pop_decieved"), Color.green);
        while(popAniController.Animating) { yield return null; }
        hexMap.combatController.StartOperation(unit, target, null, false, true);
        hexMap.combatController.CommenceOperation();
        while (hexMap.combatController.commenceOpAnimating) { yield return null; }
      } else {
        popAniController.Show(hexMap.GetUnitView(target), textLib.get("pop_failed"), Color.red);
        while(popAniController.Animating) { yield return null; }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      DecieveAnimating = false;
    }

    public bool FreezeAnimating = false;
    public void Freeze(Unit unit, Unit target) {
      FreezeAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoFreeze(unit, target));
    }

    IEnumerator CoFreeze(Unit unit, Unit target) {
      hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(target.tile).transform.position);
      while(hexMap.cameraKeyboardController.fixingCamera) { yield return null; }
      if (unit.Freeze(target)) {
        popAniController.Show(hexMap.GetUnitView(target), textLib.get("pop_decieved"), Color.green);
      } else {
        popAniController.Show(hexMap.GetUnitView(target), textLib.get("pop_failed"), Color.red);
      }
      while(popAniController.Animating) { yield return null; }
      hexMap.cameraKeyboardController.EnableCamera();
      FreezeAnimating = false;
    }

    public bool PlotAnimating = false;
    public void Plot(Unit unit, Unit target) {
      PlotAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoPlot(unit, target));
    }

    IEnumerator CoPlot(Unit from, Unit target) {
      hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(target.tile).transform.position);
      while(hexMap.cameraKeyboardController.fixingCamera) { yield return null; }

      ConflictResult conflict = from.Plot(target);
      if (conflict.moralDrop != 0) {
        popAniController.Show(hexMap.GetUnitView(target), textLib.get("pop_plot"), Color.green);
        while(popAniController.Animating) { yield return null; }
        Unit unit = conflict.unit1;

        hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetUnitView(unit).transform.position);
        while (hexMap.cameraKeyboardController.fixingCamera) { yield return null; }
        hexMap.dialogue.ShowUnitConflict(unit, conflict.unit2);
        while (hexMap.dialogue.Animating) { yield return null; }
        ShowEffect(unit, new int[]{conflict.moralDrop, 0, 0, 0, 0}, null, true);
        ShowEffect(conflict.unit2, new int[]{conflict.moralDrop, 0, 0, 0, 0}, null, true);
        hexMap.turnController.Sleep(1);
        while(hexMap.turnController.sleeping) { yield return null; }
        ShowEffect(unit, new int[]{0, 0, conflict.unit1Dead, 0, 0}, null, true);
        ShowEffect(conflict.unit2, new int[]{0, 0, conflict.unit2Dead, 0, 0}, null, true);
        hexMap.turnController.Sleep(1);
        while(hexMap.turnController.sleeping) { yield return null; }
        Tile moveTile = null;
        foreach(Tile tile in conflict.unit2.tile.neighbours) {
          if (tile.Deployable(conflict.unit2)) {
            moveTile = tile;
            break;
          }
        }
        if (moveTile != null) {
          MoveUnit(conflict.unit2, moveTile);
          while(MoveAnimating) { yield return null; };
        } else {
          foreach(Tile tile in unit.tile.neighbours) {
            if (tile.Deployable(unit)) {
              moveTile = tile;
              break;
            }
          }
          if (moveTile != null) {
            MoveUnit(unit, moveTile);
            while(MoveAnimating) { yield return null; };
          }
        }
      } else {
        popAniController.Show(hexMap.GetUnitView(target), textLib.get("pop_failed"), Color.white);
        while(popAniController.Animating) { yield return null; }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      PlotAnimating = false;
    }

    public bool ForceRetreatAnimating = false;
    public void ForceRetreat(Unit unit, int movement, bool breakThrough = false) {
      ForceRetreatAnimating = true;
      unit.retreated = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoForceRetreat(unit, movement, breakThrough));
    }

    IEnumerator CoForceRetreat(Unit unit, int movement, bool breakThrough) {
      Tile lastTile = unit.GetPath()[unit.GetPath().Length - 1];
      hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(unit.tile).transform.position);
      while(hexMap.cameraKeyboardController.fixingCamera) { yield return null; }
      if (!breakThrough && unit.rf.general.Is(Cons.loyal) && Cons.MostLikely()) {
        hexMap.dialogue.ShowRefuseToRetreat(unit);
        while(hexMap.dialogue.Animating) { yield return null; }
        hexMap.UnsetPath(unit);
      } else {
        hexMap.dialogue.ShowRetreat(unit);
        while(hexMap.dialogue.Animating) { yield return null; }
        unit.movementRemaining = movement;
        while (unit.GetPath().Length > 0) {
          if (MoveUnit(unit)) {
            while(MoveAnimating) { yield return null; }
          } else {
            break;
          }
        }
        if (unit.tile.UnitCount() > 1) {
          Tile tile = unit.tile.FindDeployableTile(unit);
          if (tile == null) {
            tile = lastTile;
          }
          MoveUnit(unit, tile);
          while(MoveAnimating) { yield return null; }
        }
        unit.movementRemaining = 0;
        int moraleDrop = breakThrough ? -60 : -40;
        unit.morale += moraleDrop;
        ShowEffect(unit, new int[]{moraleDrop,0,0,0,0});
        hexMap.UnsetPath(unit);
        while(ShowAnimating) { yield return null; }
        if (unit.IsAtRetreatZone()) {
          Retreat(unit);
          while(RetreatAnimating) { yield return null; }
        }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      ForceRetreatAnimating = false;
    }

    public bool SurpriseAnimating = false;
    public void SurpriseAttack(Unit from, Unit to, bool hitNrun = false) {
      SurpriseAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      from.Surprise();
      StartCoroutine(CoSurpriseAttack(from, to, hitNrun));
    }

    IEnumerator CoSurpriseAttack(Unit from, Unit to, bool hitNrun) {
      bool surprised = to.CanBeSurprised() + (Ambusher.Aval(from) ? Ambusher.ExtraChanceForMistAmbush : 0) >= Util.Rand(1, 100);
      Tile originPos = from.tile;
      from.SetPath(from.FindAttackPath(to.tile));
      while (from.GetPath().Length > 0) {
        if (MoveUnit(from, null, false, false)) {
          while(MoveAnimating) { yield return null; }
        } else {
          break;
        }
      }
      popAniController.Show(hexMap.GetUnitView(from), textLib.get("pop_surpriseAttack"), Color.green);
      while(popAniController.Animating) { yield return null; }
      if (!surprised) {
        popAniController.Show(hexMap.GetUnitView(to), textLib.get("pop_surpriseAttackFailed"), Color.white);
        while(popAniController.Animating) { yield return null; }
        int killed = from.type == Type.Infantry ? Util.Rand(40, 100) : Util.Rand(20, 50);
        int moraleDrop = -15;
        from.morale += moraleDrop;
        ShowEffect(from, new int[]{moraleDrop,0,from.Killed(killed),0,0});
        while (ShowAnimating) { yield return null; }
      } else {
        hexMap.combatController.StartOperation(from, to, null, true);
        hexMap.combatController.CommenceOperation();
        while (hexMap.combatController.commenceOpAnimating) { yield return null; }
      }
      if (hitNrun) {
        MoveUnit(from, originPos);
      }
      
      hexMap.cameraKeyboardController.EnableCamera();
      SurpriseAnimating = false;
    }

    public bool ShowAnimating = false;
    public void ShowEffect(Unit unit, int[] effects, View view = null, bool noFix = false) {
      if (view == null && !unit.IsShowingAnimation()) { return; }
      ShowAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoShowEffects(unit, effects, view, noFix));
    }

    IEnumerator CoShowEffects(Unit unit, int[] effects, View theView, bool noFix)
    {
      View view = theView != null ? theView : hexMap.GetUnitView(unit);
      int morale = effects[0];
      int movement = effects[1];
      int killed = effects[2];
      int atk = effects[3];
      int def = effects[4];
      if (morale != 0) {
        popAniController.Show(view,
          textLib.get("pop_morale") + (morale > 0 ? ("+" + morale) : ("" + morale)),
          morale > 0 ? Color.green : Color.white, noFix);
        while (popAniController.Animating) { yield return null; }
      }

      if (movement != 0) {
        popAniController.Show(view,
          textLib.get("pop_movement") + (movement > 0 ? ("+" + movement) : ("" + movement)),
          movement > 0 ? Color.green : Color.white, noFix);
        while (popAniController.Animating) { yield return null; }
      }

      if (killed != 0) {
        popAniController.Show(view,
          textLib.get("pop_killed") + killed,
          Color.white, noFix);
        while (popAniController.Animating) { yield return null; }
      }

      if (atk != 0) {
        popAniController.Show(view,
          textLib.get("pop_atk") + (atk > 0 ? ("+" + atk) : ("" + atk)),
          atk > 0 ? Color.green : Color.white, noFix);
        while (popAniController.Animating) { yield return null; }
      }

      if (def != 0) {
        popAniController.Show(view,
          textLib.get("pop_def") + (def > 0 ? ("+" + def) : ("" + def)),
          def > 0 ? Color.green : Color.white, noFix);
        while (popAniController.Animating) { yield return null; }
      }

      hexMap.cameraKeyboardController.EnableCamera();
      ShowAnimating = false;
    }
  }

}