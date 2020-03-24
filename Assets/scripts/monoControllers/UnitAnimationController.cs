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
    public const int MinLaborBuryBody = 800;

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

      if (type == DestroyType.ByBurningCamp) {
        eventDialog.Show(new MonoNS.Event(EventDialog.EventName.BurningCampDestroyUnit, unit, null, 0, 0, killed));
      } else if (type == DestroyType.ByWildFire) {
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
      hexMap.cameraKeyboardController.EnableCamera();
      DestroyAnimating = false;
    }

    public bool MoveAnimating = false;
    public bool MoveUnit(Unit unit, Tile tile = null, bool dontFixCamera = false) {
      MoveAnimating = true;
      Tile old = unit.tile;
      bool hiddenB4 = unit.IsConcealed();
      bool continuing = unit.DoMove(tile);
      bool discovered = hiddenB4 && !unit.IsConcealed();
      int discontent = 0;
      if (Util.eq<Tile>(old, unit.tile)) {
        MoveAnimating = false;
        return false;
      }

      if (tile == null) {
        discontent = unit.marchOnHeat.Occur();
        discontent += unit.marchOnExhaustion.Occur();
      }
      if (unit.IsShowingAnimation() && !dontFixCamera) {
        hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(unit.tile).transform.position);
      }
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoMoveUnit(unit, discontent, discovered));
      return continuing;
    }

    IEnumerator CoMoveUnit(Unit unit, int discontent, bool discovered) {
      UnitView view = hexMap.GetUnitView(unit);
      view.Move(unit.tile);
      while (view.Animating) { yield return null; }
      int r = unit.altitudeSickness.Occur();
      if (r != 0) {
        // altitude sickness
        discontent = r;
        if (!unit.IsAI()) {
          eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.AltitudeSickness, unit,
            null));
          while (eventDialog.Animating) { yield return null; }
        } else if (unit.IsShowingAnimation()) {
          popAniController.Show(view, textLib.get("pop_altitudeSickness"), Color.white);
          while(popAniController.Animating) { yield return null; }
        }
      }
      Riot(unit, discontent);
      while (riotAnimating) { yield return null; }
      if (!unit.IsAI() && discovered) {
        popAniController.Show(view, textLib.get("pop_discovered"), Color.yellow);
        while(popAniController.Animating) { yield return null; }
      }
      FarmDestryResult ret = unit.farmDestroy.Occur();
      if (ret.destroyed) {
        if (unit.IsShowingAnimation()) {
          popAniController.Show(view, textLib.get("pop_farmDestroyed"), Color.yellow);
          while(popAniController.Animating) { yield return null; }
        }
        unit.tile.SetFieldType(FieldType.Wild);
      }
      if (ret.discontent != 0) {
        eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.FarmDestroyed, unit,
          null, ret.influence));
        while (eventDialog.Animating) { yield return null; }
        Riot(unit, ret.discontent);
        while (riotAnimating) { yield return null; }
      }
      // stash event
      // hexMap.eventStasher.Add(unit.rf.general, MonoNS.EventDialog.EventName.FarmDestroyed);

      hexMap.cameraKeyboardController.EnableCamera();
      // after each step, recalculate fog
      FoW.Get().Fog();
      hexMap.mouseController.RefreshUnitPanel(unit);
      MoveAnimating = false;
    }

    public bool riotAnimating = false;
    public void Riot(Unit unit, int discontent) {
      if (discontent == 0) {
        return;
      }
      riotAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoRiot(unit, discontent));
    }

    IEnumerator CoRiot(Unit unit, int discontent) {
      int moraleReduce = unit.riot.Discontent(discontent);
      UnitView view = hexMap.GetUnitView(unit);
      if (unit.IsShowingAnimation()) {
        popAniController.Show(view, 
          System.String.Format(textLib.get("pop_discontent"), discontent),
          Color.yellow);
        while (popAniController.Animating) { yield return null; }
      }
      
      if (moraleReduce != 0) {
        // Riot
        eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.Riot, unit, null, moraleReduce));
        while (eventDialog.Animating) { yield return null; }
        unit.rf.general.UnitRiot();
      }
      hexMap.cameraKeyboardController.EnableCamera();
      riotAnimating = false;
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
      if (unit.rf.morale == 0)
      {
        if (!unit.IsCamping()) {
          // TODO: routing state should not go to any settlement, leave the campaign instead
          unit.SetState(State.Routing);
          unit.labor = 0;
          if (unit.IsShowingAnimation()) {
            popAniController.Show(view, textLib.get("pop_routing"), Color.white);
            while (popAniController.Animating) { yield return null; }
            view.RoutAnimation();
            while (view.Animating) { yield return null; }
          }
        }
      }
      else if (unit.IsWarWeary())
      {
        if (unit.IsShowingAnimation()) {
          popAniController.Show(view, textLib.get("pop_warWeary"), Color.yellow);
          while (popAniController.Animating) { yield return null; }
        }
        ShowEffects(unit, unit.warWeary.Apply());
        while (ShowAnimating) { yield return null; }
      }

      if (unit.IsHeatSicknessAffected()) {
        if (unit.IsShowingAnimation()) {
          popAniController.Show(view, textLib.get("pop_sickness"), Color.yellow);
          while (popAniController.Animating) { yield return null; }
        }
        ShowEffects(unit, unit.epidemic.Apply());
        while (ShowAnimating) { yield return null; }
      }

      if (unit.altitudeSickness.lastTurns > 0) {
        if (unit.IsShowingAnimation()) {
          popAniController.Show(view, textLib.get("pop_altitudeSickness"), Color.yellow);
          while (popAniController.Animating) { yield return null; }
        }
        ShowEffects(unit, new int[9]{0,0,unit.altitudeSickness.Apply(),0,0,0,0,0,0});
        while (ShowAnimating) { yield return null; }
      }

      if (unit.IsPoisioned()) {
        if (unit.IsShowingAnimation()) {
          popAniController.Show(view, textLib.get("pop_poisioned"), Color.yellow);
          while (popAniController.Animating) { yield return null; }
        }
        ShowEffects(unit, unit.unitPoisioned.Apply());
        while (ShowAnimating) { yield return null; }
      }

      if (unit.tile.deadZone.Apply(unit)) {
        // epimedic caused by decomposing corpse
        eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.Epidemic, unit, null));
        while (eventDialog.Animating) { yield return null; }
        Riot(unit, unit.epidemic.Occur());
        while (riotAnimating) { yield return null; }
      }

      hexMap.cameraKeyboardController.EnableCamera();
      PostAnimating = false;
    }

    public bool AttackEmptyAnimating = false;
    public void AttackEmpty(Unit unit, Settlement settlement, bool occupy = true) {
      AttackEmptyAnimating = true;
      StartCoroutine(CoAttackEmpty(unit, settlement, occupy));
    }

    IEnumerator CoAttackEmpty(Unit unit, Settlement settlement, bool occupy) {
      hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(settlement.baseTile).transform.position);
      while (hexMap.cameraKeyboardController.fixingCamera) { yield return null; }
      hexMap.cameraKeyboardController.DisableCamera();
      if (settlement.type != Settlement.Type.camp) {
        occupy = true;
      } else if (!unit.IsAI()) {
        eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.EmptySettlement,
        unit,
        settlement));
        while (eventDialog.Animating) { yield return null; }
        occupy = eventDialog.accepted;
      }

      if (occupy) {
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
          settlement,
          afterMath[0]));
        }
      } else {
        unit.supply.TakeInSupply(settlement.supplyDeposit);
        eventDialog.Show(new MonoNS.Event(
            unit.IsAI() ? MonoNS.EventDialog.EventName.EnemyBurnCamp : MonoNS.EventDialog.EventName.WeBurnCamp,
          unit,
          settlement));
      }
      while (eventDialog.Animating) { yield return null; }

      if (!occupy) {
        hexMap.settlementAniController.DestroySettlement(settlement, BuildingNS.DestroyType.ByFire);
        while (hexMap.settlementAniController.Animating) { yield return null; }
      }

      hexMap.cameraKeyboardController.EnableCamera();
      AttackEmptyAnimating = false;
    }

    public bool RefreshAnimating = false;
    public void RefreshUnit(Unit unit) {
      RefreshAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoRefreshUnit(unit));
    }

    IEnumerator CoRefreshUnit(Unit unit) {
      ShowEffects(unit, unit.RefreshUnit());
      while (ShowAnimating) { yield return null; }
      if (Cons.IsHeat(hexMap.weatherGenerator.currentWeather)) {
        int ret = unit.armorRemEvent.Occur();
        WarParty wp = hexMap.GetWarParty(unit);
        if(ret != 0) {
          if (ret < 0) {
            // disarmor not allowed
            int discontent = -ret;
            if (wp.firstRemoveArmor == null) {
              hexMap.dialogue.ShowRemoveHelmet(unit, false);
            } else {
              hexMap.dialogue.ShowRemoveHelmetFollow(unit, false);
            }
            while (hexMap.dialogue.Animating) { yield return null; }
            Riot(unit, discontent);
            while (riotAnimating) { yield return null; }
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
      if (conflict.discontent != 0) {
        // unit conflict happens
        eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.UnitConflict, unit,
          null, conflict.discontent, conflict.unit1Wound, conflict.unit1Dead,
          conflict.unit2Wound, conflict.unit2Dead, null, null, conflict.unit2));
        while (eventDialog.Animating) { yield return null; }

        Riot(unit, conflict.discontent);
        while (riotAnimating) { yield return null; }
        Riot(conflict.unit2, conflict.discontent);
        while (riotAnimating) { yield return null; }
      }

      if (Cons.FiftyFifty()) {
        int ret = unit.plainSickness.Occur();
        if (ret > 0) {
          if (!unit.IsAI()) {
            eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.PlainSickness, unit, null));
            while (eventDialog.Animating) { yield return null; }
          } else if (unit.IsShowingAnimation()) {
            popAniController.Show(hexMap.GetUnitView(unit), textLib.get("pop_plainSickness"), Color.yellow);
            while (popAniController.Animating) { yield return null; }
          }
          Riot(unit, ret);
          while (riotAnimating) { yield return null; }
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
      BuryAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoBury(unit));
    }

    IEnumerator CoBury(Unit unit) {
      if (unit.labor < MinLaborBuryBody) {
        if (unit.IsShowingAnimation()) {
          popAniController.Show(hexMap.GetUnitView(unit),
          System.String.Format(
            textLib.get("pop_insufficientLabor"),
            MinLaborBuryBody
          ),
          Color.yellow);
          while (popAniController.Animating) { yield return null; }
        }
      } else {
        unit.movementRemaining -= Unit.ActionCost;
        // TODO: apply trait
        int discontent = 2;
        Riot(unit, discontent);
        while (riotAnimating) { yield return null; }
        unit.tile.deadZone.Clean();
      }

      hexMap.cameraKeyboardController.EnableCamera();
      BuryAnimating = false;
    }

    public bool ChargeAnimating = false;
    public const int chargePoint = Unit.ActionCost;
    public void Charge(Unit from, Unit to) {
      if (!from.CanCharge()) {
        return;
      }
      ChargeAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoCharge(from, to));
    }

    IEnumerator CoCharge(Unit from, Unit to) {
      from.movementRemaining -= chargePoint;
      from.charged = true;
      popAniController.Show(hexMap.GetUnitView(from), textLib.get("pop_charging"), Color.green);
      while (popAniController.Animating) { yield return null; }
      bool defeatingUnit = to.defeating || to.chaos;
      bool scared = to.CanBeShaked(from) >= Util.Rand(1, 100);
      if (scared && to.tile.terrian != TerrianType.Plain && !defeatingUnit) {
        scared = Cons.FiftyFifty();
      }
      int killed = Util.Rand(0, 11);
      from.rf.soldiers -= killed;
      from.kia += killed;
      // morale, movement, wounded, killed, laborKilled, disserter, attack, def, discontent
      ShowEffects(from, new int[]{0,0,0,killed,0,0,0,0,0});
      while (ShowAnimating) { yield return null; }
      if (scared) {
        if (defeatingUnit) {
          int dead = to.chaos ? (from.rf.soldiers / (from.rf.royalGuard ? 2 : 3))
            : (from.rf.soldiers / (from.rf.royalGuard ? 4 : 6));
          dead = dead > to.rf.soldiers ? to.rf.soldiers : dead;
          int morale = -5;
          to.rf.soldiers -= dead;
          to.kia += dead;
          to.rf.morale += morale;
          ShowEffects(to, new int[]{morale,0,0,dead,0,0,0,0,0});
          while (ShowAnimating) { yield return null; }
          if (to.rf.soldiers <= Unit.DisbandUnitUnder) {
            // unit disbanded
            hexMap.unitAniController.DestroyUnit(to, DestroyType.ByDisband);
            while (hexMap.unitAniController.DestroyAnimating) { yield return null; }
          }
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
            popAniController.Show(hexMap.GetUnitView(to), textLib.get("pop_shaked"), Color.white);
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
              // morale, movement, wounded, killed, laborKilled, disserter, attack, def, discontent
              ShowEffects(to, new int[]{morale,0,0,killed,0,0,0,0,0});
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

      if (!scared && !defeatingUnit) {
        popAniController.Show(hexMap.GetUnitView(to), textLib.get("pop_holding"), Color.green);
        while (popAniController.Animating) { yield return null; }
        int morale = 5;
        to.rf.morale += morale;
        // morale, movement, wounded, killed, laborKilled, disserter, attack, def, discontent
        ShowEffects(to, new int[]{morale,0,0,0,0,0,0,0,0});
        while (ShowAnimating) { yield return null; }
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
      int wounded = 0;
      int killed = Util.Rand(40, 100);
      // morale, movement, wounded, killed, laborKilled, disserter, attack, def, discontent
      ShowEffects(unit, new int[]{morale,0,wounded,killed,0,0,0,0,0});
      while (ShowAnimating) { yield return null; }
      unit.rf.soldiers -= (wounded + killed);
      unit.rf.wounded += wounded;
      unit.kia += killed;
      unit.rf.morale += morale;
      hexMap.UpdateWound(unit, wounded);
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
      ForceRetreatAnimating = true;
      unit.retreated = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoForceRetreat(unit));
    }

    IEnumerator CoForceRetreat(Unit unit) {
      hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(unit.tile).transform.position);
      while(hexMap.cameraKeyboardController.fixingCamera) { yield return null; }
      hexMap.dialogue.ShowRetreat(unit);
      while(hexMap.dialogue.Animating) { yield return null; }
      unit.movementRemaining = (int)(unit.GetFullMovement() * 1.5);
      while (unit.movementRemaining > 0 && unit.GetPath().Length > 0) {
        MoveUnit(unit);
        while(MoveAnimating) { yield return null; }
      }
      while(MoveAnimating) { yield return null; }
      unit.movementRemaining = 0;
      Riot(unit, 2);
      while(riotAnimating) { yield return null; }
      unit.SetPath(new Tile[]{unit.tile});
      hexMap.cameraKeyboardController.EnableCamera();
      ForceRetreatAnimating = false;
    }

    public bool ShowAnimating = false;
    public void ShowEffects(Unit unit, int[] effects, View view = null, bool noFix = false) {
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
      int wounded = effects[2];
      int killed = effects[3];
      int laborKilled = effects[4];
      int desserter = effects[5];
      int atk = effects[6];
      int def = effects[7];
      int discontent = effects[8];
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

      if (wounded != 0) {
        popAniController.Show(view,
          textLib.get("pop_wounded") + wounded,
          Color.white, noFix);
        while (popAniController.Animating) { yield return null; }
      }

      if (laborKilled != 0) {
        popAniController.Show(view,
          textLib.get("pop_laborKilled") + laborKilled,
          Color.white);
        while (popAniController.Animating) { yield return null; }
      }

      if (desserter != 0) {
        popAniController.Show(view,
          textLib.get("pop_desserter") + desserter,
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

      if (discontent != 0) {
        if (discontent < 0) {
          popAniController.Show(view, 
            System.String.Format(textLib.get("pop_content"), -discontent),
            Color.green, noFix);
        } else {
          popAniController.Show(view, 
            System.String.Format(textLib.get("pop_discontent"), discontent),
            Color.yellow, noFix);
        }
        while (popAniController.Animating) { yield return null; }
      }

      hexMap.cameraKeyboardController.EnableCamera();
      ShowAnimating = false;
    }
  }

}