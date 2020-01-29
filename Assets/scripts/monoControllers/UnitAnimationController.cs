using System.Collections;
using UnitNS;
using CourtNS;
using TextNS;
using UnityEngine;
using MapTileNS;
using FieldNS;

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
      UnitView view = hexMap.GetUnitView(unit);
      view.DestroyAnimation(type);
      while (view.Animating) { yield return null; }
      hexMap.DestroyUnitView(unit);

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
    public bool MoveUnit(Unit unit, Tile tile = null) {
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
      }
      if (unit.IsShowingAnimation()) {
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
      Riot(unit, discontent);
      while (riotAnimating) { yield return null; }
      if (!unit.IsAI() && discovered) {
        popAniController.Show(view, textLib.get("pop_discovered"), Color.yellow);
        while(popAniController.Animating) { yield return null; }
      }
      int ret = unit.farmDestroy.Occur();
      if (ret != -1) {
        // occured
        if (ret == 0) {
          // stash event
          hexMap.eventStasher.Add(unit.rf.general, MonoNS.EventDialog.EventName.FarmDestroyed);
        } else {
          eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.FarmDestroyed, unit,
            null, ret));
          while (eventDialog.Animating) { yield return null; }
          Riot(unit, ret);
          while (riotAnimating) { yield return null; }
        }
      }
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

        General oldGeneral = unit.rf.general;
        unit.rf.general.UnitRiot();
        General newGeneral = unit.rf.general;

        if (oldGeneral.IsDead()) {
          eventDialog.Show(new MonoNS.Event(EventDialog.EventName.GeneralExecuted, null, null, 0, 0, 0, 0, 0, null, oldGeneral));
          while (eventDialog.Animating) { yield return null; }
        }

        if (oldGeneral.IsRest()) {
          eventDialog.Show(new MonoNS.Event(EventDialog.EventName.GeneralReturned, null, null, 0, 0, 0, 0, 0, null, oldGeneral));
          while (eventDialog.Animating) { yield return null; }
        }

        if (oldGeneral.IsIdle()) {
          eventDialog.Show(new MonoNS.Event(EventDialog.EventName.GeneralResigned, null, null, 0, 0, 0, 0, 0, null, oldGeneral));
          while (eventDialog.Animating) { yield return null; }
        }

        if (unit.rf.IsRest()) {
          eventDialog.Show(new MonoNS.Event(EventDialog.EventName.Retreat, unit, null));
          while (eventDialog.Animating) { yield return null; }
        }

        if (newGeneral != null) {
          eventDialog.Show(new MonoNS.Event(EventDialog.EventName.NewGeneral, unit, null));
          while (eventDialog.Animating) { yield return null; }
        }
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
            popAniController.Show(view, textLib.get("pop_routing"), Color.red);
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

      if (unit.IsPoisioned()) {
        if (unit.IsShowingAnimation()) {
          popAniController.Show(view, textLib.get("pop_poisioned"), Color.yellow);
          while (popAniController.Animating) { yield return null; }
        }
        ShowEffects(unit, unit.unitPoisioned.Apply());
        while (ShowAnimating) { yield return null; }
      }

      hexMap.cameraKeyboardController.EnableCamera();
      PostAnimating = false;
    }

    public bool AttackEmptyAnimating = false;
    public void AttackEmpty(Unit unit, Settlement settlement, bool occupy = true) {
      AttackEmptyAnimating = true;
      hexMap.cameraKeyboardController.FixCameraAt(hexMap.GetTileView(settlement.baseTile).transform.position);
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoAttackEmpty(unit, settlement, occupy));
    }

    IEnumerator CoAttackEmpty(Unit unit, Settlement settlement, bool occupy) {
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
        if(ret != 0) {
          if (ret < 0) {
            int discontent = -ret;
            // disarmor not allowed
            eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.Disarmor, unit,
              null, discontent));
            while (eventDialog.Animating) { yield return null; }
            Riot(unit, discontent);
            while (riotAnimating) { yield return null; }
          } else {
            // disarmor allowed
            int defReduce = ret;
            eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.Disarmor1, unit,
              null, defReduce));
            while (eventDialog.Animating) { yield return null; }
            unit.disarmorDefDebuf = defReduce; 
            ShowEffects(unit, new int[8]{0,0,0,0,0,0,0,-defReduce});
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

    public bool SiegeAnimating = false;
    public void Siege(Unit unit) {
      SiegeAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoSiege(unit));
    }

    public enum SiegeResult {
      NoCity,
      Ready,
      NoLabor,
      NoPoint
    }

    public class SiegePredict {
      public SiegeResult result = SiegeResult.NoCity;
      public Settlement target;
      public const int MinLabor = 800;
      public const int MinPoint = Unit.BasicMovementCost * 2;
    }

    public SiegePredict Ready4Siege(Unit unit) {
      SiegePredict predict = new SiegePredict();

      foreach (Tile tile in unit.tile.neighbours) {
        if (tile.settlement != null && tile.settlement.owner.isAI != unit.IsAI()) {
          predict.target = tile.settlement;
          predict.result = SiegeResult.Ready;
          break;
        }
      }

      if (predict.result == SiegeResult.NoCity) {
        return predict;
      }

      if (unit.labor < SiegePredict.MinLabor) {
        predict.result = SiegeResult.NoLabor;
      }

      if (unit.movementRemaining < SiegePredict.MinPoint) {
        predict.result = SiegeResult.NoPoint;
      }

      return predict;
    }

    IEnumerator CoSiege(Unit unit) {
      SiegePredict predict = Ready4Siege(unit); 

      if (predict.result == SiegeResult.Ready) {
        // siege ready
        unit.tile.sieged = true;
        unit.movementRemaining -= SiegePredict.MinPoint;
        if (unit.IsShowingAnimation()) {
          popAniController.Show(hexMap.GetUnitView(unit),
          textLib.get("pop_sieged"), Color.green);
          while (popAniController.Animating) { yield return null; }
        }

        if (predict.target.IsUnderSiege()) {
          eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.UnderSiege, unit, predict.target));
          while (eventDialog.Animating) { yield return null; }
        }
      }

      if (predict.result == SiegeResult.NoLabor) {
        if (unit.IsShowingAnimation()) {
          popAniController.Show(hexMap.GetUnitView(unit),
          System.String.Format(textLib.get("pop_insufficientLabor"), SiegePredict.MinLabor), Color.yellow);
          while (popAniController.Animating) { yield return null; }
        }
      }

      if (predict.result == SiegeResult.NoPoint) {
        if (unit.IsShowingAnimation()) {
          popAniController.Show(hexMap.GetUnitView(unit),
          System.String.Format(textLib.get("pop_insufficientPoint"), SiegePredict.MinPoint), Color.yellow);
          while (popAniController.Animating) { yield return null; }
        }
      }

      hexMap.cameraKeyboardController.EnableCamera();
      SiegeAnimating = false;
    }

    public bool ShowAnimating = false;
    public void ShowEffects(Unit unit, int[] effects) {
      if (!unit.IsShowingAnimation()) { return; }
      ShowAnimating = true;
      hexMap.cameraKeyboardController.DisableCamera();
      StartCoroutine(CoShowEffects(unit, effects));
    }

    IEnumerator CoShowEffects(Unit unit, int[] effects)
    {
      UnitView view = hexMap.GetUnitView(unit);
      int morale = effects[0];
      int movement = effects[1];
      int wounded = effects[2];
      int killed = effects[3];
      int laborKilled = effects[4];
      int desserter = effects[5];
      int atk = effects[6];
      int def = effects[7];
      if (morale != 0) {
        popAniController.Show(view,
          textLib.get("pop_morale") + (morale > 0 ? ("+" + morale) : ("" + morale)),
          morale > 0 ? Color.green : Color.red);
        while (popAniController.Animating) { yield return null; }
      }

      if (movement != 0) {
        popAniController.Show(view,
          textLib.get("pop_movement") + (movement > 0 ? ("+" + movement) : ("" + movement)),
          movement > 0 ? Color.green : Color.red);
        while (popAniController.Animating) { yield return null; }
      }

      if (wounded != 0) {
        popAniController.Show(view,
          textLib.get("pop_wounded") + wounded,
          Color.red);
        while (popAniController.Animating) { yield return null; }
      }

      if (killed != 0) {
        popAniController.Show(view,
          textLib.get("pop_killed") + killed,
          Color.red);
        while (popAniController.Animating) { yield return null; }
      }

      if (laborKilled != 0) {
        popAniController.Show(view,
          textLib.get("pop_laborKilled") + laborKilled,
          Color.red);
        while (popAniController.Animating) { yield return null; }
      }

      if (desserter != 0) {
        popAniController.Show(view,
          textLib.get("pop_desserter") + desserter,
          Color.red);
        while (popAniController.Animating) { yield return null; }
      }

      if (atk != 0) {
        popAniController.Show(view,
          textLib.get("pop_atk") + (atk > 0 ? ("+" + atk) : ("" + atk)),
          atk > 0 ? Color.green : Color.red);
        while (popAniController.Animating) { yield return null; }
      }

      if (def != 0) {
        popAniController.Show(view,
          textLib.get("pop_def") + (def > 0 ? ("+" + def) : ("" + def)),
          def > 0 ? Color.green : Color.red);
        while (popAniController.Animating) { yield return null; }
      }

      hexMap.cameraKeyboardController.EnableCamera();
      ShowAnimating = false;
    }
  }

}