﻿using UnityEngine;
using UnityEngine.UI;
using UnitNS;

namespace MonoNS
{

  public class UnitSelectionPanel : BaseController
  {

    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      mouseController = hexMap.mouseController;
      actionController = hexMap.actionController;
      turnController = hexMap.turnController;
      actionController.actionDone += ActionDone;
      actionController.onBtnClick += OnBtnClick;
      turnController.onEndTurnClicked += OnEndTurnClicked;
      turnController.onNewTurn += OnNewTurn;
      GameObject[] btns = {MoveButton, AttackButton, DefendButton, CampButton,
                           SabotageButton, FireButton, AmbushButton, EncampButton,
                           RetreatButton, TransferSupplyButton, TransferLaborButton};
      buttons = btns;
      mouseController.onUnitSelect += OnUnitSelect;
      mouseController.onUnitPreflight += OnUnitSelect;
      mouseController.onUnitDeselect += OnUnitDeselect;
      mouseController.onModeQuit += OnModeQuit;
      self = this.transform.gameObject;
      self.SetActive(false);
    }

    MouseController mouseController;
    ActionController actionController;
    TurnController turnController;
    GameObject self;
    public GameObject MoveButton;
    public GameObject AttackButton;
    public GameObject DefendButton;
    public GameObject CampButton;
    public GameObject SabotageButton;
    public GameObject NextTurnButton;
    public GameObject FireButton;
    public GameObject AmbushButton;
    public GameObject EncampButton;
    public GameObject RetreatButton;
    public GameObject TransferSupplyButton;
    public GameObject TransferLaborButton;
    GameObject[] buttons;

    public Text title;
    public Text movement;
    public Text slots;
    public Text num;
    public Text morale;
    public Text offense;
    public Text defense;
    public Text stamina;
    public Text state;
    public Text illness;

    public void OnEndTurnClicked()
    {
      NextTurnButton.SetActive(false);
    }

    public void OnNewTurn()
    {
      NextTurnButton.SetActive(true);
    }

    public void ActionDone(Unit unit, Unit[] units, ActionController.actionName name)
    {
      if ((unit != null && !unit.IsAI() && name == ActionController.actionName.MOVE)
        || (unit != null && unit.IsAI() && name == ActionController.actionName.ATTACK))
      {
        RefreshButtons(unit);
      }
    }

    void RefreshButtons(Unit unit)
    {
      ToggleButtons(!unit.TurnDone());
    }

    void ToggleButtons(bool state)
    {
      foreach (GameObject button in buttons)
      {
        button.SetActive(state);
      }
    }

    public void OnModeQuit(Unit unit)
    {
      RefreshButtons(unit);
    }

    public void OnUnitSelect(Unit unit)
    {
      self.SetActive(true);
      title.text = unit.GeneralName();
      movement.text = "移动力:" + unit.movementRemaining + "/" + unit.GetFullMovement();
      slots.text = "粮草: " + unit.supply + "石" + " 可维持" + unit.slots + "/" + unit.GetMaxSupplySlots() + "回合" + " 每回合消耗:" + unit.SupplyNeededPerTurn() + "石";
      num.text = unit.Name() + "[兵:" + unit.rf.soldiers + "/伤:" + unit.rf.wounded + "/亡:" + unit.kia + "/逃:" + unit.mia + "/役:" + unit.labor + "]";
      morale.text = "士气: " + unit.rf.morale;
      offense.text = "攻击: " + unit.atk;
      defense.text = "防御: " + unit.def;
      stamina.text = "体力: " + unit.GetStaminaLvlName();
      state.text = "部队状态: " + unit.GetStateName() + (unit.starving ? " 饥饿[本轮" + unit.GetStarvingDessertNum() +"人逃"
        + unit.GetStarvingKillNum() + "人亡]" : "" + (unit.IsWarWeary() ? " 毫无战意[本轮" + unit.GetWarWearyDissertNum() + "人逃亡]" : ""));
      illness.text = unit.GetIllTurns() > 0 ? "疫情: 预计本轮致伤"+ unit.GetIllDisableNum() + "人,致死" + unit.GetIllKillNum() +"人,疫情还将持续" + unit.GetIllTurns() + "回合" : "";

      ToggleButtons(false);
      if (unit.IsAI() == !turnController.player) {
        RefreshButtons(unit);
      }
    }

    public void OnUnitDeselect(Unit unit)
    {
      ToggleButtons(false);
      self.SetActive(false);
    }

    public void OnBtnClick(ActionController.actionName action)
    {
      // disable other buttons than move
      ToggleButtons(false);
      if (action == ActionController.actionName.MOVE)
      {
        MoveButton.SetActive(true);
      }
      if (action == ActionController.actionName.ATTACK)
      {
        AttackButton.SetActive(true);
      }
    }

    public override void UpdateChild()
    {
      if (!turnController.endingTurn && !turnController.showingTitle)
      {
        NextTurnButton.SetActive(true);
      }
    }
  }

}