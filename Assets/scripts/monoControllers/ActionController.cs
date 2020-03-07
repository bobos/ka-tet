﻿using System.Collections;
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

    public void OnDeploymentDoneClick() {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.DEPLOYMENTDONE);
      }
    }

    public void OnDestroyClick() {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.DESTROY);
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

    public void OnChargeClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.CHARGE);
      }
    }

    public void OnReposition() {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.REPOS);
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

    public void OnBuryClick() {
      buryBody(hexMap.mouseController.selectedUnit);
    }

    public void OnEncampClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.ENCAMP);
      }
    }

    public void OnDecampClick() {
      hexMap.unitSelectionPanel.Decamp();
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

    public void OnConfirmlWargameClick()
    {
      hexMap.wargameController.Commit();
    }

    public void OnGarrisonMgtClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.MGTGARRISON);
      }
    }

    public void OnGarrisonCancelClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.GARRISONCANCEL);
      }
    }

    public void OnGarrison1Click()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.GARRISON1);
      }
    }

    public void OnGarrison2Click()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.GARRISON2);
      }
    }

    public void OnGarrison3Click()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.GARRISON3);
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

    public void OnMidBtn1Click() {
      if (onBtnClick != null) {
        onBtnClick(actionName.MIDBTN1);
      }
    }

    public void OnMidBtn2Click() {
      if (onBtnClick != null) {
        onBtnClick(actionName.MIDBTN2);
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
      DEPLOYMENTDONE,
      DESTROY,
      STAND,
      ATTACK,
      REPOS,
      POISION,
      SABOTAGE,
      ATTACKEmpty,
      FIRE,
      SIEGE,
      BURY,
      CHARGE,
      ENCAMP,
      RETREAT,
      CAMP,
      SHOWMINE,
      SHOWENEMY,
      WARGAME,
      WGCONFIRM,
      WGCANCEL,
      DECAMP,
      MGTGARRISON,
      GARRISONCANCEL,
      GARRISON1,
      GARRISON2,
      GARRISON3,
      DISTSUPPLY,
      DISTLABOR,
      LABOR2Unit,
      TRANSFERSUPPLY,
      TRANSFERLABOR,
      INPUTCONFIRM,
      EVENTDIALOGCONFIRM,
      COMMENCEOP,
      MIDBTN1,
      MIDBTN2
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

    public bool buryBody(Unit unit) {
      return DoAction(unit, null, null, actionName.BURY);
    }

    public bool commenceOperation() {
      return DoAction(null, null, null, actionName.COMMENCEOP);
    }

    public bool charge(Unit from, Unit to) {
      return DoAction(from, to, null, actionName.CHARGE);
    }

    public bool retreat(Unit unit) {
      return DoAction(unit, null, null, actionName.RETREAT);
    }

    public bool DoAction(Unit unit, Unit unit1, Tile tile, actionName name)
    {
      if (ActionOngoing) return false;
      ActionOngoing = true;
      if (name == actionName.MOVE)
      {
        StartCoroutine(DoMove(unit));
      }
      if (name == actionName.SABOTAGE)
      {
        if (tile == null) {
          StartCoroutine(DestroySiegeWall(unit));
        } else {
          StartCoroutine(Flood(unit, tile));
        }
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
      if (name == actionName.COMMENCEOP) 
      {
        StartCoroutine(DoCommenceOp());
      }
      if (name == actionName.BURY) 
      {
        StartCoroutine(DoBuryBody(unit));
      }
      if (name == actionName.CHARGE) 
      {
        StartCoroutine(DoCharge(unit, unit1));
      }
      if (name == actionName.RETREAT) 
      {
        StartCoroutine(DoRetreat(unit));
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

    IEnumerator DoCommenceOp()
    {
      hexMap.combatController.CommenceOperation();
      while (hexMap.combatController.commenceOpAnimating) { yield return null; }
      ActionOngoing = false;
    }

    TextLib textLib {
      get {
        return Cons.GetTextLib();
      }
    }

    IEnumerator Flood(Unit unit, Tile tile)
    {
      tileAniController.Flood(unit, tile);
      while (tileAniController.FloodAnimating)
      {
        yield return null;
      }
      ActionOngoing = false;
    }

    IEnumerator DestroySiegeWall(Unit unit)
    {
      tileAniController.DestroySiegeWall(unit);
      while (tileAniController.DestroySiegeAnimating)
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

    IEnumerator DoBuryBody(Unit unit)
    {
      unitAniController.Bury(unit);
      while (unitAniController.BuryAnimating)
      {
        yield return null;
      }
      ActionOngoing = false;
    }

    IEnumerator DoCharge(Unit from, Unit to) {
      unitAniController.Charge(from, to);
      while (unitAniController.ChargeAnimating)
      {
        yield return null;
      }
      ActionOngoing = false;
    }

    IEnumerator DoRetreat(Unit unit) {
      unitAniController.Retreat(unit);
      while (unitAniController.RetreatAnimating)
      {
        yield return null;
      }
      ActionOngoing = false;
    }

  }

}