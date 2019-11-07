using System.Collections;
using UnitNS;
using CourtNS;
using TextNS;
using UnityEngine;
using MapTileNS;
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
      StartCoroutine(CoDestroy(unit, type, killed));
    }

    IEnumerator CoDestroy(Unit unit, DestroyType type, int killed)
    {
      UnitView view = hexMap.GetUnitView(unit);
      view.DestroyAnimation(type);
      while (view.Animating) { yield return null; }
      view.Destroy();

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
      DestroyAnimating = false;
    }

    public bool MoveAnimating = false;
    public bool MoveUnit(Unit unit, List<Unit> ambusher, Tile tile = null) {
      MoveAnimating = true;
      Tile old = unit.tile;
      bool continuing = unit.DoMove(ambusher, tile);
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
      StartCoroutine(CoMoveUnit(unit, discontent));
      return continuing;
    }

    IEnumerator CoMoveUnit(Unit unit, int discontent) {
      UnitView view = hexMap.GetUnitView(unit);
      view.Move(unit.tile);
      while (view.Animating) { yield return null; }
      Riot(unit, discontent);
      while (riotAnimating) { yield return null; }
      MoveAnimating = false;
    }

    public bool riotAnimating = false;
    public void Riot(Unit unit, int discontent) {
      if (discontent == 0) {
        return;
      }
      riotAnimating = true;
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
      riotAnimating = false;
    }

    public bool PostAnimating = false;
    public void PostTurnAction(Unit unit)
    {
      PostAnimating = true;
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
        int[] effects = new int[8]{0,0,0,0,0,0,0,0};
        int morale = -1;
        effects[0] = morale;
        unit.rf.morale += morale;

        int miaNum = unit.GetWarWearyDissertNum();
        effects[5] = miaNum;
        unit.mia += miaNum;
        unit.rf.soldiers -= miaNum;
        int laborDisserter = (int) (miaNum / 4);
        unit.labor -= laborDisserter;
        effects[4] = laborDisserter;
        ShowEffects(unit, effects);
        while (ShowAnimating) { yield return null; }
      }

      if (unit.IsSicknessAffected()) {
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

      PostAnimating = false;
    }

    public bool RefreshAnimating = false;
    public void RefreshUnit(Unit unit) {
      RefreshAnimating = true;
      StartCoroutine(CoRefreshUnit(unit));
    }

    IEnumerator CoRefreshUnit(Unit unit) {
      ShowEffects(unit, unit.RefreshUnit());
      while (ShowAnimating) { yield return null; }
      if (Cons.IsHeat(hexMap.weatherGenerator.currentWeather)) {
        if(unit.armorRemEvent.Occur()) {
          int defReduce = unit.armorRemEvent.DefReduce();
          int discontent = unit.armorRemEvent.Discontent();
          eventDialog.Show(new MonoNS.Event(MonoNS.EventDialog.EventName.Disarmor, unit,
            null, defReduce, discontent));
          while (eventDialog.Animating) { yield return null; }
          if (eventDialog.accepted) {
            unit.disarmorDefDebuf = defReduce; 
            ShowEffects(unit, new int[8]{0,0,0,0,0,0,0,defReduce});
            while (ShowAnimating) { yield return null; }
          } else {
            Riot(unit, discontent);
            while (riotAnimating) { yield return null; }
          }
        }
      }
      RefreshAnimating = false;
    }

    public bool ShowAnimating = false;
    public void ShowEffects(Unit unit, int[] effects) {
      if (!unit.IsShowingAnimation()) { return; }
      ShowAnimating = true;
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

      ShowAnimating = false;
    }
  }

}