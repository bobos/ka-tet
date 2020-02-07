using System.Collections;
using UnityEngine;
using UnitNS;
using MapTileNS;
using TextNS;

namespace MonoNS
{
  public class ActionController : BaseController
  {

    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      ActionOngoing = false;
    }

    public override void UpdateChild() { }

    public delegate void BtnClicked(actionName btn);
    public event BtnClicked onBtnClick;
    public delegate void ActionDone(Unit unit, Unit[] units, actionName btn);
    public event ActionDone actionDone;
    TileAnimationController tileAniController {
      get {
        return hexMap.tileAniController;
      }
    }
    UnitAnimationController unitAniController {
      get {
        return hexMap.unitAniController;
      }
    }

    public bool ActionOngoing { get; private set; }

    public void OnMoveClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.MOVE);
      }
    }

    public void OnShowMyNetwork()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.SHOWMINE);
      }
    }

    public void OnShowTheirNetwork()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.SHOWENEMY);
      }
    }

    public void OnAttackClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.ATTACK);
      }
    }

    public void OnPoisionClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.POISION);
      }
    }

    public void OnCampClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.CAMP);
      }
    }

    public void OnSabotageClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.SABOTAGE);
      }
    }

    public void OnFireClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.FIRE);
      }
    }

    public void OnSiegeClick()
    {
      siege(hexMap.mouseController.selectedUnit);
    }

    public void OnEncampClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.ENCAMP);
      }
    }

    public void OnRetreatClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.RETREAT);
      }
    }

    public void OnWargameClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.WARGAME);
      }
    }

    public void OnCancelWargameClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.WGCANCEL);
      }
    }

    //TODO
    public void OnConfirmlWargameClick()
    {
      hexMap.wargameController.Commit();
    }

    //TODO
    public void OnConfirmAttackClick() {
      hexMap.combatController.CommenceOperation();
    }

    public void OnGarrisonMgtClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.DECAMP);
      }
    }

    public void OnSupplyMgtClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.DISTSUPPLY);
      }
    }

    public void OnAbandonClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.DISTLABOR);
      }
    }

    public void OnTransferSupplyClick() {
      if (onBtnClick != null) {
        onBtnClick(actionName.TRANSFERSUPPLY);
      }
    }

    public void OnTransferLaborClick() {
      if (onBtnClick != null) {
        onBtnClick(actionName.TRANSFERLABOR);
      }
    }

    public void OnTransferLabor2UnitClick() {
      if (onBtnClick != null) {
        onBtnClick(actionName.LABOR2Unit);
      }
    }

    public void OnInputConfirmClick() {
      if (onBtnClick != null) {
        onBtnClick(actionName.INPUTCONFIRM);
      }
    }

    public void OnEventDialogConfirm() {
      if (onBtnClick != null) {
        onBtnClick(actionName.EVENTDIALOGCONFIRM);
      }
    }

    public void PerformImmediateAction(Unit unit, actionName action)
    {
      unit.SetEndState(action);
      actionDone(unit, null, action);
    }

    public enum actionName
    {
      MOVE,
      STAND,
      ATTACK,
      POISION,
      SABOTAGE,
      ATTACKEmpty,
      FIRE,
      SIEGE,
      ENCAMP,
      RETREAT,
      CAMP,
      SHOWMINE,
      SHOWENEMY,
      WARGAME,
      WGCONFIRM,
      WGCANCEL,
      DECAMP,
      DISTSUPPLY,
      DISTLABOR,
      LABOR2Unit,
      TRANSFERSUPPLY,
      TRANSFERLABOR,
      INPUTCONFIRM,
      EVENTDIALOGCONFIRM,
    }

    // Make sure this is sequential
    public bool move(Unit unit)
    {
      return DoAction(unit, null, null, actionName.MOVE);
    }

    public bool attackEmptySettlement(Unit unit, Tile tile)
    {
      return DoAction(unit, null, tile, actionName.ATTACKEmpty);
    }

    public bool sabotage(Unit unit, Tile tile)
    {
      return DoAction(unit, null, tile, actionName.SABOTAGE);
    }

    public bool burn(Tile tile)
    {
      return DoAction(null, null, tile, actionName.FIRE);
    }

    public bool poision(Unit unit, Tile tile) {
      return DoAction(unit, null, tile, actionName.POISION);
    }

    public bool siege(Unit unit) {
      return DoAction(unit, null, null, actionName.SIEGE);
    }

    public bool DoAction(Unit unit, Unit[] units, Tile tile, actionName name)
    {
      if (ActionOngoing) return false;
      ActionOngoing = true;
      if (name == actionName.MOVE)
      {
        StartCoroutine(DoMove(unit));
      }
      if (name == actionName.ATTACK)
      {
        StartCoroutine(DoAttack(unit, units));
      }
      if (name == actionName.SABOTAGE)
      {
        StartCoroutine(Flood(unit, tile));
      }
      if (name == actionName.FIRE)
      {
        StartCoroutine(Burn(tile));
      }
      if (name == actionName.ATTACKEmpty) 
      {
        StartCoroutine(DoAttackEmptySettlement(unit, tile.settlement));
      }
      if (name == actionName.POISION) 
      {
        StartCoroutine(DoPoision(unit, tile));
      }
      if (name == actionName.SIEGE) 
      {
        StartCoroutine(DoSiege(unit));
      }
      return true;
    }

    IEnumerator DoMove(Unit unit)
    {
      UnitView view = hexMap.GetUnitView(unit);
      while (view && unitAniController.MoveUnit(unit))
      {
        while (unitAniController.MoveAnimating) { yield return null; }
      }
      while (unitAniController.MoveAnimating) { yield return null; }
      ActionOngoing = false;
      if (actionDone != null) actionDone(unit, null, actionName.MOVE);
    }

    TextLib textLib {
      get {
        return Cons.GetTextLib();
      }
    }

    IEnumerator Flood(Unit unit, Tile tile)
    {
      tileAniController.Flood(tile);
      while (tileAniController.FloodAnimating)
      {
        yield return null;
      }
      ActionOngoing = false;
    }

    IEnumerator Burn(Tile tile)
    {
      tileAniController.Burn(tile);
      while (tileAniController.BurnAnimating)
      {
        yield return null;
      }
      ActionOngoing = false;
    }

    IEnumerator DoAttack(Unit receiver, Unit[] punchers)
    {
      yield return new WaitForSeconds(3);
      ActionOngoing = false;
      if (actionDone != null) actionDone(receiver, punchers, actionName.ATTACK);
    }

    IEnumerator DoAttackEmptySettlement(Unit unit, Settlement settlement, bool occupy = true)
    {
      unitAniController.AttackEmpty(unit, settlement, occupy);
      while (unitAniController.AttackEmptyAnimating)
      {
        yield return null;
      }
      ActionOngoing = false;
    }

    IEnumerator DoPoision(Unit unit, Tile tile)
    {
      unitAniController.Poision(unit, tile);
      while (unitAniController.PoisionAnimating)
      {
        yield return null;
      }
      ActionOngoing = false;
    }

    IEnumerator DoSiege(Unit unit)
    {
      unitAniController.Siege(unit);
      while (unitAniController.SiegeAnimating)
      {
        yield return null;
      }
      ActionOngoing = false;
    }
  }

}