using System.Collections;
using UnityEngine;
using UnitNS;

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

    public void OnAmbushClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.AMBUSH);
      }
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

    public void OnBurnCampClick()
    {
      if (onBtnClick != null)
      {
        onBtnClick(actionName.BURNCAMP);
      }
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
      FIRE,
      AMBUSH,
      ENCAMP,
      RETREAT,
      CAMP,
      SHOWMINE,
      SHOWENEMY,
      BURNCAMP,
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
      return DoAction(unit, null, actionName.MOVE);
    }

    public bool DoAction(Unit unit, Unit[] units, actionName name)
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
      return true;
    }

    IEnumerator DoMove(Unit unit)
    {
      UnitView view = hexMap.GetUnitView(unit);
      while (view && unit.DoMove())
      {
        while (view.Animating)
        {
          yield return null;
        }
        // wait one frame 
      }
      ActionOngoing = false;
      if (actionDone != null) actionDone(unit, null, actionName.MOVE);
    }

    IEnumerator DoAttack(Unit receiver, Unit[] punchers)
    {
      yield return new WaitForSeconds(3);
      ActionOngoing = false;
      if (actionDone != null) actionDone(receiver, punchers, actionName.ATTACK);
    }
  }

}