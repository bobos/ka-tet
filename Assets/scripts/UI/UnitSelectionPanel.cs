using UnityEngine;
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
      movement.text = "movement:" + unit.movementRemaining + "/" + unit.GetFullMovement();
      slots.text = "slots: " + unit.slots + "/" + unit.GetMaxSupplySlots() + " supply: " + unit.supply + " consume " + unit.SupplyNeededPerTurn() + " per turn";
      num.text = "force:" + unit.GeneralName() + " wnd:" + unit.rf.wounded + " kia:" + unit.kia + " mia:" + unit.mia + " labor:" + unit.labor;
      morale.text = "morale: " + unit.rf.morale;
      offense.text = "offense: " + unit.atk;
      defense.text = "defense: " + unit.def;
      stamina.text = "stamina: " + unit.GetStaminaLvlName();
      state.text = "state: " + unit.GetStateName() + (unit.starving ? " Starving" : "");
      illness.text = "illness last:" + unit.GetIllTurns() + " illdeath:" + unit.GetIllDeathTurns();

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