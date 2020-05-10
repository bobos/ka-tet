using System.Collections;
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
      general.TroopDestroyed(generalDead);

      if (general.IsDead()) {
        eventDialog.Show(new MonoNS.Event(EventDialog.EventName.GeneralKilledInBattle, null, null, 0, 0, 0, 0, 0, null, general));
        while (eventDialog.Animating) { yield return null; }
      }
      if (unit.IsCommander()) {
        Unit newCommander = hexMap.GetWarParty(unit).AssignNewCommander();
        if (newCommander.IsOnField()) {
          popAniController.Show(hexMap.GetUnitView(newCommander), textLib.get("pop_newCommander"), Color.white);
          while(popAniController.Animating) { yield return null; }
        }
      }

      ShakeNearbyAllies(unit, unit.IsCommander() ? -20 : -5);
      while (ShakeAnimating) { yield return null; }
      hexMap.cameraKeyboardController.EnableCamera();
      DestroyAnimating = false;
    }

    public bool ShakeAnimating = false;
    public void ShakeNearbyAllies(Unit unit, int moraleDrop) {
      ShakeAnimating = true;
      StartCoroutine(CoShakeNearbyAllies(unit, moraleDrop));
    }

    IEnumerator CoShakeNearbyAllies(Unit unit, int moraleDrop) {
      foreach(Tile t in unit.tile.GetNeighboursWithinRange<Tile>(4, (Tile tt) => true)) {
        Unit u = t.GetUnit();
        if (u != null && u.IsAI() == unit.IsAI() && !Util.eq<Unit>(unit, u)) {
          int[] stats = new int[]{moraleDrop,0,0,0,0};
          u.rf.morale += moraleDrop;
          if (u.IsShowingAnimation()) {
            hexMap.unitAniController.ShowEffect(u, stats, null, true);
          }
          if (!u.StickAsNailWhenDefeat()
            && u.RetreatOnDefeat()
            && u.SetRetreatPath()) {
            ForceRetreat(u, 60);
            while(ForceRetreatAnimating) { yield return null; }
          }
        }
      }
      ShakeAnimating = false;
      hexMap.cameraKeyboardController.EnableCamera();
    }

    public bool MoveAnimating = false;
    public bool MoveUnit(Unit unit, Tile tile = null, bool dontFixCamera = false) {
      MoveAnimating = true;
      Tile old = unit.tile;
      bool continuing = unit.DoMove(tile);
      int moraleDrop = 0;
      if (Util.eq<Tile>(old, unit.tile)) {
        MoveAnimating = false;
        return false;
      }

      if (tile == null) {
        moraleDrop = unit.marchOnHeat.Occur();
      }
      if (unit.IsShowingAnimation() && !dontFixCamera) {
        hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(unit.tile).transform.position);
      }
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoMoveUnit(unit, moraleDrop));
      return continuing;
    }

    IEnumerator CoMoveUnit(Unit unit, int moraleDrop) {
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
        unit.rf.morale += r;
        while(ShowAnimating) { yield return null; }
      }
      if (moraleDrop != 0) {
        unit.rf.morale += moraleDrop;
        ShowEffect(unit, new int[]{moraleDrop,0,0,0,0});
        while(ShowAnimating) { yield return null; }
      }
      if (!unit.IsAI()) {
        hexMap.GetAIParty().UpdateAlert();
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
      UnitView view = hexMap.GetUnitView(unit);
      if (unit.IsCamping()) {
        unit.rf.morale = unit.rf.morale > unit.GetRetreatThreshold() ? unit.rf.morale: unit.GetRetreatThreshold();
      }

      int moraleDrop = unit.retreatStress.Occur();
      if (moraleDrop != 0) {
        unit.rf.morale += moraleDrop;
        hexMap.dialogue.ShowRetreatStress(unit);
        while(hexMap.dialogue.Animating) { yield return null; }
        ShowEffect(unit, new int[]{moraleDrop, 0, 0, 0, 0});
        while(ShowAnimating) { yield return null; }
      }

      if (unit.rf.morale == 0 || unit.rf.soldiers <= Unit.DisbandUnitUnder)
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
        afterMath[0], afterMath[1], afterMath[2]));
      } else {
        eventDialog.Show(new MonoNS.Event(
          unit.IsAI() ? MonoNS.EventDialog.EventName.EnemyCaptureCamp : MonoNS.EventDialog.EventName.WeCaptureCamp,
        unit,
        settlement));
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
        } else if (g.rf.general.Is(Cons.cunning) && Cons.SlimChance()) {
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
            ForceRetreat(g, 200, true);
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
        afterMath[0], afterMath[1], afterMath[2]));
      } else {
        eventDialog.Show(new MonoNS.Event(
          unit.IsAI() ? MonoNS.EventDialog.EventName.EnemyCaptureCamp : MonoNS.EventDialog.EventName.WeCaptureCamp,
        unit,
        settlement));
      }
      while (eventDialog.Animating) { yield return null; }
      hexMap.cameraKeyboardController.EnableCamera();
      TakeAnimating = false;
    }

    public bool SurroundAnimating = false;
    public void UnitSurrounded (HashSet<Unit> units, HashSet<Unit> accu) {
      SurroundAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoUnitSurrounded(units, accu));
    }

    IEnumerator CoUnitSurrounded(HashSet<Unit> units, HashSet<Unit> accu) {
      const int moraleDrop = -5;
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
        unit.rf.morale += drop;
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

      if (unit.tile.deadZone.Apply(unit) && unit.epidemic.Occur()) {
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
            unit.rf.morale += ret;
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

      ConflictResult conflict = unit.unitConflict.Occur();
      if (conflict.moralDrop != 0) {
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
      }

      if (Cons.FiftyFifty()) {
        int ret = unit.plainSickness.Occur();
        if (ret != 0) {
          unit.rf.morale += ret;
          if (unit.IsShowingAnimation()) {
            popAniController.Show(hexMap.GetUnitView(unit), textLib.get("pop_plainSickness"), Color.yellow);
            while (popAniController.Animating) { yield return null; }
          }
        }
      }

      int moraleDrop = unit.inCampComplain.Occur();
      if (moraleDrop != 0) {
        unit.rf.morale += moraleDrop;
        hexMap.dialogue.ShowInCampComplain(unit.rf.province.region);
        while(hexMap.dialogue.Animating) { yield return null; }
        ShowEffect(unit, new int[]{moraleDrop, 0, 0, 0, 0}, settlementMgr.GetView(unit.tile.settlement));
        while(ShowAnimating) { yield return null; }
      }

      moraleDrop = unit.onFieldComplain.Occur();
      if (moraleDrop != 0) {
        unit.rf.morale += moraleDrop;
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
      if (!unit.CanPoision()) {
        return;
      }
      unit.poisionDone = true;
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
      if (unit.IsCavalry()) {
        return;
      }
      BuryAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoBury(unit));
    }

    IEnumerator CoBury(Unit unit) {
      if (!unit.ApplyDiscipline(Cons.FiftyFifty())) {
        int moraleDrop = -3;
        unit.rf.morale += moraleDrop;
        ShowEffect(unit, new int[]{moraleDrop,0,0,0,0});
        while(ShowAnimating) { yield return null; }
      }
      unit.tile.deadZone.Clean();

      hexMap.cameraKeyboardController.EnableCamera();
      BuryAnimating = false;
    }

    public bool ScatterAnimating = false;
    public void Scatter(List<Unit> units, List<Unit> failedToMove, int moraleDrop) {
      ScatterAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoScatter(units, failedToMove, moraleDrop));
    }

    IEnumerator CoScatter(List<Unit> units, List<Unit> failedToMove, int moraleDrop) {
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
        List<Unit> ally = new List<Unit>();
        foreach (Tile t in unit.tile.neighbours) {
          // already taken
          if (tiles[t]) { 
            Unit u = t.GetUnit();
            if (u != null && u.IsAI() == unit.IsAI()) {
              ally.Add(u);
            }
            continue;
          }
          step1 = t;
          break;
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
        foreach(Tile t in unit.tile.neighbours) {
          Unit u = t.GetUnit();
          if (u != null && u.IsAI() == unit.IsAI()&& !units.Contains(u) && moraleDrop != 0) {
            hexMap.unitAniController.CrashByAlly(u, -3);
            while (hexMap.unitAniController.CrashAnimating) { yield return null; }
            continue;
          }
        }
      }
      
      hexMap.cameraKeyboardController.EnableCamera();
      ScatterAnimating = false;
    }

    public bool ChargeAnimating = false;
    public void Charge(Unit from, Unit to) {
      if (!from.CanCharge() || to.IsVulnerable()) {
        return;
      }
      ChargeAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoCharge(from, to));
    }

    IEnumerator CoCharge(Unit from, Unit to) {
      from.charged = true;
      View view = from.IsCamping() ?
        settlementMgr.GetView(from.tile.settlement) : hexMap.GetUnitView(from);

      popAniController.Show(view, textLib.get("pop_charging"), Color.green);
      while (popAniController.Animating) { yield return null; }
      bool scared = to.CanBeShaked(from) >= Util.Rand(1, 100);
      if (scared && to.rf.general.Has(Cons.holdTheGround) && Cons.FiftyFifty()) {
        scared = false;
      }
      if (!scared) {
        scared = from.rf.general.Has(Cons.hammer) ? Cons.FiftyFifty() : scared;
      }
      if (!scared && from.rf.IsChargeBuffed()) {
        scared = Cons.HighlyLikely();
      }
      // morale, movement, killed, attack, def
      ShowEffect(from, new int[]{0,0,from.Killed(Util.Rand(2, 15)),0,0}, view);
      while (ShowAnimating) { yield return null; }
      if (scared) {
        if (!to.IsGone()) {
          List<Unit> affectedAllies = new List<Unit>();
          affectedAllies.Add(to);
          if (from.rf.general.Has(Cons.breaker)) {
            foreach(Tile t in to.tile.neighbours) {
              Unit u = t.GetUnit();
              if (u != null && u.IsAI() == to.IsAI() && !u.StickAsNailWhenDefeat() && Cons.FiftyFifty()) {
                if (u.rf.general.Has(Cons.holdTheGround) && Cons.FiftyFifty()) {
                  affectedAllies.Add(u);
                }
              }
            }
          }

          Tile toTile = to.tile;
          List<Unit> notMoved = new List<Unit>();
          Scatter(affectedAllies, notMoved, -4);
          while(ScatterAnimating) { yield return null; }
          if (toTile.Deployable(from)) {
            if (from.IsCamping()) {
              from.tile.settlement.Decamp(from, toTile);
            } else {
              MoveUnit(from, toTile);
              while (MoveAnimating) { yield return null; }
            }
          }
        }
      } else {
        popAniController.Show(hexMap.GetUnitView(to), textLib.get("pop_holding"), Color.green);
        while (popAniController.Animating) { yield return null; }
        int morale1 = -2;
        from.rf.morale += morale1;
        ShowEffect(from, new int[]{morale1,0,0,0,0}, view, true);
        while(ShowAnimating) { yield return null; }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      ChargeAnimating = false;
    }

    public bool PursueAnimating = false;
    public void Pursue(Unit from, Unit to) {
      if (!to.IsVulnerable()) {
        return;
      }
      PursueAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoPursue(from, to));
    }

    IEnumerator CoPursue(Unit from, Unit to) {
      from.UseAtmpt();
      View view = from.IsCamping() ?
        settlementMgr.GetView(from.tile.settlement) : hexMap.GetUnitView(from);

      popAniController.Show(view, textLib.get("pop_chasing"), Color.green);
      while (popAniController.Animating) { yield return null; }
      int dead = to.chaos ?
        (from.rf.soldiers / (from.IsHeavyCavalry() ? 3 : (from.IsCavalry() ? 5 : 10)))
      : (from.rf.soldiers / (from.IsHeavyCavalry() ? 5 : (from.IsCavalry() ? 8 : 12)));
      dead = from.rf.general.Has(Cons.hammer) ? (int)(dead * 1.5f) : dead;
      dead = dead > to.rf.soldiers ? to.rf.soldiers : dead;
      int morale = from.IsHeavyCavalry() ? -6 : (from.IsCavalry() ? -5 : -3); 
      morale = morale * (from.rf.general.Has(Cons.hammer) ? 2 : 1);
      to.rf.morale += morale;
      ShowEffect(to, new int[]{morale,0,to.Killed(dead),0,0});
      while (ShowAnimating) { yield return null; }
      if (to.rf.soldiers <= Unit.DisbandUnitUnder) {
        // unit disbanded
        hexMap.unitAniController.DestroyUnit(to, DestroyType.ByDisband);
        while (hexMap.unitAniController.DestroyAnimating) { yield return null; }
      }
      to.tile.deadZone.Occur(dead);

      if (!to.IsGone()) {
        List<Unit> affectedAllies = new List<Unit>();
        affectedAllies.Add(to);
        if (from.rf.general.Has(Cons.breaker)) {
          foreach(Tile t in to.tile.neighbours) {
            Unit u = t.GetUnit();
            if (u != null && u.IsAI() == to.IsAI() && !u.StickAsNailWhenDefeat() && Cons.FiftyFifty()) {
              if (u.rf.general.Has(Cons.holdTheGround) && Cons.FiftyFifty()) {
                affectedAllies.Add(u);
              }
            }
          }
        }

        Tile toTile = to.tile;
        List<Unit> notMoved = new List<Unit>();
        Scatter(affectedAllies, notMoved, -4);
        while(ScatterAnimating) { yield return null; }
        if (toTile.Deployable(from)) {
          if (from.IsCamping()) {
            from.tile.settlement.Decamp(from, toTile);
          } else {
            MoveUnit(from, toTile);
            while (MoveAnimating) { yield return null; }
          }
        }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      PursueAnimating = false;
    }

    public bool BreakthroughAnimating = false;
    public void BreakThrough(Unit from, Unit to) {
      if (!from.CanBreakThrough()) {
        return;
      }
      BreakthroughAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoBreakThrough(from, to));
    }

    IEnumerator CoBreakThrough(Unit from, Unit to) {
      from.UseAtmpt();
      View view = from.IsCamping() ?
        settlementMgr.GetView(from.tile.settlement) : hexMap.GetUnitView(from);

      bool defeatingUnit = to.IsVulnerable();
      popAniController.Show(view, textLib.get("pop_toBreakThrough"), Color.green);
      while (popAniController.Animating) { yield return null; }
      bool scared = to.CanBeShaked(from) >= Util.Rand(1, 100);
      if (scared && to.rf.general.Has(Cons.holdTheGround) && Cons.FiftyFifty()) {
        scared = false;
      }
      scared = defeatingUnit ? true : scared;
      if (!scared && from.rf.general.Has(Cons.formidable)) {
        scared = Cons.FiftyFifty();
      }
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
        int morale1 = -5;
        from.rf.morale += morale1;
        ShowEffect(from, new int[]{morale1,0,0,0,0}, view, true);
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
      unit.rf.morale += morale;
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
          !unit.ApplyDiscipline(Cons.FiftyFifty())) {
          hexMap.dialogue.ShowSiegeComplain(region);
          while(hexMap.dialogue.Animating) { yield return null; }
          int moraleDrop = -5;
          unit.rf.morale += moraleDrop;
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
        if (newCommander.IsOnField()) {
          popAniController.Show(hexMap.GetUnitView(newCommander), textLib.get("pop_newCommander"), Color.white);
          while(popAniController.Animating) { yield return null; }
        }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      RetreatAnimating = false;
    }

    public bool ForecastAnimating = false;
    public void Forecast(Unit unit) {
      if (!unit.CanForecast()) {
        return;
      }
      ForecastAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoForecast(unit));
    }

    IEnumerator CoForecast(Unit unit) {
      hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(unit.tile).transform.position);
      while(hexMap.cameraKeyboardController.fixingCamera) { yield return null; }
      if(unit.Forecast()) {
        hexMap.weatherIndicator.ShowInfo(true);
        popAniController.Show(hexMap.GetUnitView(unit), textLib.get("pop_wheatherForecasted"), Color.green);
        while(popAniController.Animating) { yield return null; }
      } else {
        popAniController.Show(hexMap.GetUnitView(unit), textLib.get("pop_wheatherForecastFailed"), Color.white);
        while(popAniController.Animating) { yield return null; }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      ForecastAnimating = false;
    }

    public bool FalseOrderAnimating = false;
    public void FalseOrder(Unit unit, Unit target) {
      if (!unit.CanFalseOrder()) {
        return;
      }
      FalseOrderAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoFalseOrder(unit, target));
    }

    IEnumerator CoFalseOrder(Unit unit, Unit target) {
      hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(target.tile).transform.position);
      while(hexMap.cameraKeyboardController.fixingCamera) { yield return null; }
      if(unit.FalseOrder(target)) {
        popAniController.Show(hexMap.GetUnitView(target), textLib.get("pop_falseOrderFollowed"), Color.green);
        while(popAniController.Animating) { yield return null; }
      } else {
        popAniController.Show(hexMap.GetUnitView(target), textLib.get("pop_falseOrderFailed"), Color.white);
        while(popAniController.Animating) { yield return null; }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      FalseOrderAnimating = false;
    }

    public bool ForceRetreatAnimating = false;
    public void ForceRetreat(Unit unit, int movement, bool breakThrough = false) {
      if (unit.retreated) {
        return;
      }
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
      } else {
        if (breakThrough && !unit.rf.IsSpecial()) {
          unit.chaos = true;
        } else {
          unit.defeating = true;
        }
        hexMap.dialogue.ShowRetreat(unit);
        while(hexMap.dialogue.Animating) { yield return null; }
        unit.movementRemaining = movement;
        while (unit.movementRemaining > 0 && unit.GetPath().Length > 0) {
          MoveUnit(unit);
          while(MoveAnimating) { yield return null; }
        }
        while(MoveAnimating) { yield return null; }
        if (unit.tile.UnitCount() > 1) {
          Tile tile = unit.tile.FindDeployableTile(unit);
          if (tile == null) {
            tile = lastTile;
          }
          MoveUnit(unit, tile);
          while(MoveAnimating) { yield return null; }
        }
        unit.movementRemaining = 0;
        int moraleDrop = breakThrough ? -10 : -5;
        unit.rf.morale += moraleDrop;
        ShowEffect(unit, new int[]{moraleDrop,0,0,0,0});
        while(ShowAnimating) { yield return null; }
      }
      unit.SetPath(new Tile[]{unit.tile});
      hexMap.cameraKeyboardController.EnableCamera();
      ForceRetreatAnimating = false;
    }

    public bool SurpriseAnimating = false;
    public void SurpriseAttack(Unit from, Unit to) {
      SurpriseAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoSurpriseAttack(from, to));
    }

    IEnumerator CoSurpriseAttack(Unit from, Unit to) {
      bool surprised = to.CanBeSurprised() >= Util.Rand(1, 100);
      from.SetPath(from.FindAttackPath(to.tile));
      while (from.GetPath().Length > 0) {
        MoveUnit(from);
        while(MoveAnimating) { yield return null; }
      }
      while(MoveAnimating) { yield return null; }
      popAniController.Show(hexMap.GetUnitView(from), textLib.get("pop_surpriseAttack"), Color.green);
      while(popAniController.Animating) { yield return null; }
      if (!surprised) {
        from.UseAtmpt();
        popAniController.Show(hexMap.GetUnitView(to), textLib.get("pop_surpriseAttackFailed"), Color.white);
        while(popAniController.Animating) { yield return null; }
        int killed = from.IsCavalry() ? Util.Rand(20, 50) : Util.Rand(40, 100);
        int moraleDrop = -3; 
        from.rf.morale += moraleDrop;
        ShowEffect(from, new int[]{moraleDrop,0,from.Killed(killed),0,0});
        while (ShowAnimating) { yield return null; }
      } else {
        hexMap.combatController.StartOperation(from, to, null, true);
        hexMap.combatController.CommenceOperation();
        while (hexMap.combatController.commenceOpAnimating) { yield return null; }
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