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
    public void DestroyUnit(Unit unit, DestroyType type)
    {
      DestroyAnimating = true;
      int killed = unit.Destroy();
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoDestroy(unit, type, killed));
    }

    IEnumerator CoDestroy(Unit unit, DestroyType type, int killed)
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
      general.TroopDestroyed();

      if (general.IsDead()) {
        eventDialog.Show(new MonoNS.Event(EventDialog.EventName.GeneralKilledInBattle, null, null, 0, 0, 0, 0, 0, null, general));
        while (eventDialog.Animating) { yield return null; }
      }

      ShakeNearbyAllies(unit, -10);
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
      foreach(Tile t in unit.tile.GetNeighboursWithinRange<Tile>(10, (Tile tt) => true)) {
        Unit u = t.GetUnit();
        if (u != null && u.IsAI() == unit.IsAI()) {
          int[] stats = new int[]{moraleDrop,0,0,0,0};
          u.rf.morale += moraleDrop;
          if (u.IsShowingAnimation()) {
            hexMap.unitAniController.ShowEffect(u, stats, null, true);
          }
          // TODO: apply general trait
          if (!u.IsCommander() && (u.rf.morale <= Util.Rand(1, 79))&& u.SetRetreatPath()) {
            ForceRetreat(u);
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
      bool hiddenB4 = unit.IsConcealed();
      bool continuing = unit.DoMove(tile);
      bool discovered = hiddenB4 && !unit.IsConcealed();
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
      StartCoroutine(CoMoveUnit(unit, moraleDrop, discovered));
      return continuing;
    }

    IEnumerator CoMoveUnit(Unit unit, int moraleDrop, bool discovered) {
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
      if (!unit.IsAI() && discovered) {
        popAniController.Show(view, textLib.get("pop_discovered"), Color.yellow);
        while(popAniController.Animating) { yield return null; }
      }
      // stash event
      // hexMap.eventStasher.Add(unit.rf.general, MonoNS.EventDialog.EventName.FarmDestroyed);

      hexMap.cameraKeyboardController.EnableCamera();
      // after each step, recalculate fog
      FoW.Get().Fog();
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

    public bool SurroundAnimating = false;
    public void UnitSurrounded (HashSet<Unit> units) {
      SurroundAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoUnitSurrounded(units));
    }

    IEnumerator CoUnitSurrounded(HashSet<Unit> units) {
      const int moraleDrop = -10;
      bool first = true;
      foreach(Unit unit in units) {
        if (first) {
          hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(unit.tile).transform.position);
          hexMap.turnController.ShowTitle(Cons.GetTextLib().get("title_noWayOut"), Color.white);
          while(hexMap.turnController.showingTitle) { yield return null; }
          first = false;
        }
        unit.rf.morale += moraleDrop;
        ShowEffect(unit, new int[]{moraleDrop,0,0,0,0});
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

      if (unit.tile.deadZone.Apply(unit)) {
        // epimedic caused by decomposing corpse
        eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.Epidemic, unit, null));
        while (eventDialog.Animating) { yield return null; }
        unit.epidemic.Occur();
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
        // unit conflict happens
        eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.UnitConflict, unit,
          null, conflict.moralDrop, conflict.unit1Dead, conflict.unit2Dead,
          0, 0, null, null, conflict.unit2));
        while (eventDialog.Animating) { yield return null; }
      }

      if (Cons.FiftyFifty()) {
        int ret = unit.plainSickness.Occur();
        if (ret != 0) {
          unit.rf.morale += ret;
          if (unit.IsShowingAnimation()) {
            popAniController.Show(hexMap.GetUnitView(unit), textLib.get("pop_plainSickness"), Color.yellow);
            while (popAniController.Animating) { yield return null; }
            ShowEffect(unit, new int[]{ret,0,0,0,0});
            while (ShowAnimating) { yield return null; }
          }
        }
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
      if (unit.IsShowingAnimation()) {
        popAniController.Show(hexMap.GetUnitView(unit), textLib.get("pop_poisionDone"), Color.green);
        while (popAniController.Animating) { yield return null; }
      }

      foreach(Unit u in tile.Poision(unit)) {
        if (u.IsAI()) {
          continue;
        }
        eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.Poision, u, null));
        while (eventDialog.Animating) { yield return null; }
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
      unit.movementRemaining -= Unit.ActionCost;
      // TODO: apply trait
      int moraleDrop = -5;
      unit.rf.morale += moraleDrop;
      ShowEffect(unit, new int[]{moraleDrop,0,0,0,0});
      while(ShowAnimating) { yield return null; }
      unit.tile.deadZone.Clean();

      hexMap.cameraKeyboardController.EnableCamera();
      BuryAnimating = false;
    }

    public bool ChargeAnimating = false;
    public const int chargePoint = Unit.ActionCost;
    public void Charge(Unit from, Unit to) {
      if (!from.IsCavalry()) {
        return;
      }
      if (!from.CanCharge() && !to.IsVulnerable()) {
        return;
      }
      ChargeAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoCharge(from, to));
    }

    IEnumerator CoCharge(Unit from, Unit to) {
      from.movementRemaining -= chargePoint;
      from.charged = true;
      from.attacked = true;
      bool defeatingUnit = to.IsVulnerable();
      popAniController.Show(hexMap.GetUnitView(from),
        defeatingUnit ? textLib.get("pop_chasing") : textLib.get("pop_charging"),
        Color.green);
      while (popAniController.Animating) { yield return null; }
      bool scared = to.CanBeShaked(from) >= Util.Rand(1, 100);
      scared = defeatingUnit ? true : scared;
      int killed = Util.Rand(0, 11);
      from.rf.soldiers -= killed;
      from.kia += killed;
      // morale, movement, killed, attack, def
      ShowEffect(from, new int[]{0,0,killed,0,0});
      while (ShowAnimating) { yield return null; }
      if (scared) {
        if (defeatingUnit) {
          int dead = to.chaos ? (from.rf.soldiers / (from.IsHeavyCavalry() ? 2 : 3))
            : (from.rf.soldiers / (from.IsHeavyCavalry() ? 4 : 6));
          dead = dead > to.rf.soldiers ? to.rf.soldiers : dead;
          int morale = -5;
          to.rf.soldiers -= dead;
          to.kia += dead;
          to.rf.morale += morale;
          ShowEffect(to, new int[]{morale,0,dead,0,0});
          while (ShowAnimating) { yield return null; }
          if (to.rf.soldiers <= Unit.DisbandUnitUnder) {
            // unit disbanded
            hexMap.unitAniController.DestroyUnit(to, DestroyType.ByDisband);
            while (hexMap.unitAniController.DestroyAnimating) { yield return null; }
          }
          to.tile.deadZone.Occur(dead);
        }

        if (!to.IsGone()) {
          Tile tile = null;
          List<Unit> ally = new List<Unit>();
          int allyNum = 0;
          foreach(Tile t in to.tile.neighbours) {
            if (t.Deployable(to)) {
              int count = 0;
              foreach(Tile t1 in t.neighbours) {
                Unit uu = t1.GetUnit();
                if (!Util.eq<Tile>(t1, to.tile) && uu != null && uu.IsAI() == to.IsAI()) {
                  count++;
                }
              }
              if (count >= allyNum) {
                allyNum = count;
                tile = t;
              }
            }
            Unit u = t.GetUnit();
            if (u != null && u.IsAI() == to.IsAI()) {
              ally.Add(u);
            }
          }

          if (tile == null && ally.Count == 0) {
            scared = false;
          } else {
            popAniController.Show(hexMap.GetUnitView(to),
              defeatingUnit ? textLib.get("pop_retreat") : textLib.get("pop_shaked"), Color.white);
            while (popAniController.Animating) { yield return null; }
            if (tile == null) {
              foreach(Unit u in ally) {
                foreach(Tile t in u.tile.neighbours) {
                  if (t.Deployable(to)) {
                    tile = t;
                    break;
                  }
                }
                if (tile != null) {
                  break;
                }
              }
            }

            if (tile == null) {
              Unit conflicted = ally[Util.Rand(0, ally.Count - 1)];
              killed = Util.Rand(20, 50);
              int morale = -2;
              // morale, movement, killed, attack, def
              ShowEffect(to, new int[]{morale,0,killed,0,0});
              while (ShowAnimating) { yield return null; }
              to.rf.soldiers -= killed;
              to.kia += killed;
              to.rf.morale += morale;

              CrashByAlly(conflicted, morale);
              while (CrashAnimating) { yield return null; }
            } else {
              Tile toTile = to.tile;
              MoveUnit(to, tile);
              while (MoveAnimating) { yield return null; }
              MoveUnit(from, toTile);
              while (MoveAnimating) { yield return null; }
            }
          }
        }
      }

      if (!scared) {
        popAniController.Show(hexMap.GetUnitView(to), textLib.get("pop_holding"), Color.green);
        while (popAniController.Animating) { yield return null; }
        int morale = 5;
        to.rf.morale += morale;
        // morale, movement, wounded, killed, laborKilled, disserter, attack, def, discontent
        ShowEffect(to, new int[]{morale,0,0,0,0}, null, true);
        int morale1 = -2;
        from.rf.morale += morale1;
        ShowEffect(to, new int[]{morale1,0,0,0,0}, null, true);
        hexMap.turnController.Sleep(1);
        while(hexMap.turnController.sleeping) { yield return null; }
      }
      hexMap.cameraKeyboardController.EnableCamera();
      ChargeAnimating = false;
    }

    public bool CrashAnimating = false;
    public void CrashByAlly(Unit unit, int morale) {
      CrashAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoCrashByAlly(unit, morale));
    }

    IEnumerator CoCrashByAlly(Unit unit, int morale) {
      popAniController.Show(hexMap.GetUnitView(unit), textLib.get("pop_crashedByAlly"), Color.white);
      while (popAniController.Animating) { yield return null; }
      int killed = Util.Rand(40, 100);
      // morale, movement, killed, attack, def
      ShowEffect(unit, new int[]{morale,0,killed,0,0});
      while (ShowAnimating) { yield return null; }
      unit.rf.soldiers -= killed;
      unit.kia += killed;
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
      if(!hexMap.settlementMgr.BuildSiegeWall(unit.tile, hexMap.GetWarParty(unit))) {
        if (unit.IsShowingAnimation()) {
          popAniController.Show(hexMap.GetUnitView(unit),
          textLib.get("pop_buildFail"), Color.yellow);
          while (popAniController.Animating) { yield return null; }
        }
      } else {
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
      hexMap.cameraKeyboardController.EnableCamera();
      RetreatAnimating = false;
    }

    public bool ForceRetreatAnimating = false;
    public void ForceRetreat(Unit unit) {
      if (unit.retreated) {
        return;
      }
      ForceRetreatAnimating = true;
      unit.retreated = true;
      unit.defeating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoForceRetreat(unit));
    }

    IEnumerator CoForceRetreat(Unit unit) {
      Tile lastTile = unit.GetPath()[unit.GetPath().Length - 1];
      hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(unit.tile).transform.position);
      while(hexMap.cameraKeyboardController.fixingCamera) { yield return null; }
      hexMap.dialogue.ShowRetreat(unit);
      while(hexMap.dialogue.Animating) { yield return null; }
      unit.__movementRemaining = 150;
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
      int moraleDrop = -5;
      unit.rf.morale += moraleDrop;
      ShowEffect(unit, new int[]{moraleDrop,0,0,0,0});
      while(ShowAnimating) { yield return null; }
      unit.SetPath(new Tile[]{unit.tile});
      hexMap.cameraKeyboardController.EnableCamera();
      ForceRetreatAnimating = false;
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