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
      GameObject[] btns = {GarrisonButton, SupplyButton, AbandonButton, GarrisonCancel,
        Garrison1Button, Garrison2Button, Garrison3Button, Labor2UnitButton};
      buttons = btns;
      mouseController.onSettlementSelect += OnSettlementSelect;
      mouseController.onSettlementDeselect += OnSettlementDeselect;
      hexMap.eventDialog.eventDialogOn += EventDialogOn;
      hexMap.eventDialog.eventDialogOff += EventDialogOff;
      hexMap.actionController.onBtnClick += OnBtnClick;
      self = this.transform.gameObject;
      self.SetActive(false);
    }

    MouseController mouseController;
    GameObject self;
    public GameObject GarrisonButton;
    public GameObject SupplyButton;
    public GameObject AbandonButton;
    public GameObject Garrison1Button;
    public GameObject Garrison2Button;
    public GameObject Garrison3Button;
    public GameObject GarrisonCancel;
    public GameObject Labor2UnitButton;
    GameObject[] buttons;

    public Text title;
    public Text population;
    public Text supply;
    public Text defense;
    public Text defenseWill;
    public Text state;
    public Text inNetwork;

    public Unit selectedUnit = null;
    Unit G1;
    Unit G2;
    Unit G3;
    public void OnBtnClick(ActionController.actionName actionName) {
      Settlement settlement = mouseController.selectedSettlement;
      if (actionName == ActionController.actionName.MGTGARRISON) {
        G1 = G2 = G3 = selectedUnit = null;
        int len = settlement.garrison.Count;
        if (len > 0) {
          ToggleRest(false);
          GarrisonCancel.SetActive(true);
          G1 = settlement.garrison[0]; 
          Garrison1Button.SetActive(true);
          Garrison1Button.GetComponentInChildren<Text>().text = G1.GeneralName();
        }
        if (len > 1) {
          G2 = settlement.garrison[1]; 
          Garrison2Button.SetActive(true);
          Garrison2Button.GetComponentInChildren<Text>().text = G2.GeneralName();
        }
        if (len > 2) {
          G3 = settlement.garrison[2]; 
          Garrison3Button.SetActive(true);
          Garrison3Button.GetComponentInChildren<Text>().text = G3.GeneralName();
        }
      }

      if (actionName == ActionController.actionName.GARRISONCANCEL) {
        CancelGarrison();
      }

      if (actionName == ActionController.actionName.GARRISON1) {
        selectedUnit = G1;
        hexMap.unitSelectionPanel.OnUnitSelect(selectedUnit, true);
      }

      if (actionName == ActionController.actionName.GARRISON2) {
        selectedUnit = G2;
        hexMap.unitSelectionPanel.OnUnitSelect(selectedUnit, true);
      }

      if (actionName == ActionController.actionName.GARRISON3) {
        selectedUnit = G3;
        hexMap.unitSelectionPanel.OnUnitSelect(selectedUnit, true);
      }

    }

    public void CancelGarrison() {
      if (selectedUnit != null) {
        hexMap.unitSelectionPanel.CancelGarrison(selectedUnit);
      }
      G1 = G2 = G3 = selectedUnit = null;
      OnSettlementSelect(hexMap.mouseController.selectedSettlement);
    }

    public void EventDialogOn() {
      if (self.activeSelf) {
        DisableButtons();
      }
    }

    public void EventDialogOff() {
      if (self.activeSelf) {
        EnableButtons(true);
      }
    }

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

    void ToggleGarrison(bool onOff) { 
      Garrison1Button.SetActive(onOff);
      Garrison2Button.SetActive(onOff);
      Garrison3Button.SetActive(onOff);
      GarrisonCancel.SetActive(onOff);
    }

    void ToggleRest(bool onOff) {
      GarrisonButton.SetActive(onOff);
      SupplyButton.SetActive(onOff);
      AbandonButton.SetActive(onOff);
      Labor2UnitButton.SetActive(onOff);
    }

    public void OnSettlementSelect(Settlement s)
    {
      self.SetActive(true);
      Unit[] garrison = s.garrison.ToArray();
      title.text = s.name + "[驻扎部队: " + garrison.Length + "/" + s.room + "]";
      population.text = "人口: 平民" + (s.civillian_male + s.civillian_child + s.civillian_female) + " 兵役" + s.labor;
      supply.text = "粮草每回合消耗" + s.SupplyNeeded() + "石";
      defense.text = "城防: " + s.wall.GetLevelTxt() + "[" + s.wall.defensePoint + "/" + s.wall.MaxDefensePoint() + "]";
      defenseWill.text = "粮仓: " + s.storageLvl.GetLevelTxt()
        + " [" + s.supplyDeposit + "/" + s.storageLvl.MaxStorage() + "石]";
      string state = s.IsUnderSiege() ? "被围困" : "正常";
      if (s.state == Settlement.State.constructing)
      {
        state = "筑城中 预计" + s.buildTurns + "回合完成";
      }
      this.state.text = state;
      this.inNetwork.text = "";

      EnableButtons(s.IsFunctional());
      if (hexMap.wargameController.start) {
        DisableButtons();
      }
      ToggleGarrison(false);
      if (mouseController.selectedSettlement.IsEmpty()) {
        GarrisonButton.SetActive(false);
      }
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
