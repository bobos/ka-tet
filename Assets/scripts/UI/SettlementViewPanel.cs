using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnitNS;

namespace MonoNS
{
  public class SettlementViewPanel : BaseController
  {

    // Use this for initialization
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      mouseController = hexMap.mouseController;
      GameObject[] btns = {GarrisonButton, SupplyButton, AbandonButton};
      buttons = btns;
      mouseController.onSettlementSelect += OnSettlementSelect;
      mouseController.onSettlementDeselect += OnSettlementDeselect;
      self = this.transform.gameObject;
      self.SetActive(false);
    }

    MouseController mouseController;
    GameObject self;
    public GameObject GarrisonButton;
    public GameObject SupplyButton;
    public GameObject AbandonButton;
    GameObject[] buttons;

    public Text title;
    public Text population;
    public Text supply;
    public Text garrison1;
    public Text garrison2;
    public Text defense;
    public Text defenseWill;
    public Text state;
    public Text inNetwork;

    void DisableButtons() {
      foreach (GameObject button in buttons)
      {
        button.SetActive(false);
      }
    }

    void EnableButtons(bool settlementReady)
    {
      DisableButtons();
      if (settlementReady) {
        foreach (GameObject button in buttons)
        {
          button.SetActive(true);
        }
      } else {
        GarrisonButton.SetActive(true);
      }
    }

    public void OnSettlementSelect(Settlement s)
    {
      self.SetActive(true);
      string type = "camp";
      if (s.type == Settlement.Type.city)
      {
        type = "city";
      }
      if (s.type == Settlement.Type.strategyBase)
      {
        type = "Citedal";
      }
      title.text = s.name + " (" + type + ")";
      population.text = "civillians:" + s.civillian + " labor:" + s.labor;
      supply.text = "supply deposit: " + s.supplyDeposit + " Dan" + " Consume " + s.MinSupplyNeeded() + " per turn";
      Unit[] garrison = s.garrison.ToArray();
      garrison1.text = "";
      garrison2.text = "";
      if (garrison.Length > 0)
      {
        Unit u1 = garrison[0];
        garrison1.text = "unit1: " + u1.GeneralName() + " slots: " + u1.slots + "/" + u1.GetMaxSupplySlots() + " mv: " + u1.movementRemaining + "/" + u1.GetFullMovement()
        + " labor: " + u1.labor;
      }
      if (garrison.Length > 1)
      {
        Unit u2 = garrison[1];
        garrison2.text = "unit2: " + u2.GeneralName() + " slots: " + u2.slots + "/" + u2.GetMaxSupplySlots() + " mv: " + u2.movementRemaining + "/" + u2.GetFullMovement()
        + " labor: " + u2.labor;
      }
      defense.text = "wall: " + s.wall;
      defenseWill.text = "defense preparation: " + s.defensePrep;
      string state = "normal";
      if (s.state == Settlement.State.constructing)
      {
        state = "under construction";
      }
      this.state.text = "state: " + state;
      this.inNetwork.text = "Building turns: " + s.buildTurns;

      EnableButtons(s.IsFunctional());
      // TODO: for test
      //if (s.owner.isAI)
      //{
      //  ToggleButtons(false);
      //}
    }

    public void OnSettlementDeselect(Settlement s)
    {
      DisableButtons();
      self.SetActive(false);
    }

    public override void UpdateChild() {}

  }
}
