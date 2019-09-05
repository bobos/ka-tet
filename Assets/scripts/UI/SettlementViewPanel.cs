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
      hexMap.eventDialog.eventDialogOn += EventDialogOn;
      hexMap.eventDialog.eventDialogOff += EventDialogOff;
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

    public void OnSettlementSelect(Settlement s)
    {
      self.SetActive(true);
      string type = "营寨";
      if (s.type == Settlement.Type.city)
      {
        type = "城市";
      }
      if (s.type == Settlement.Type.strategyBase)
      {
        type = "大本营";
      }
      title.text = s.name + " (" + type + ")";
      population.text = "人口: 平民" + s.civillian + " 兵役" + s.labor;
      supply.text = "粮草: " + s.supplyDeposit + "石" + " 每回合消耗" + s.MinSupplyNeeded() + "石";
      Unit[] garrison = s.garrison.ToArray();
      garrison1.text = "";
      garrison2.text = "";
      if (garrison.Length > 0)
      {
        Unit u1 = garrison[0];
        garrison1.text = "部队: " + u1.GeneralName() + " 补给: " + u1.slots + "/" + u1.GetMaxSupplySlots() + " 战兵:" + u1.rf.soldiers + " 兵役:" + u1.labor;
      }
      if (garrison.Length > 1)
      {
        Unit u2 = garrison[1];
        garrison2.text = "部队: " + u2.GeneralName() + " 补给: " + u2.slots + "/" + u2.GetMaxSupplySlots() + " 战兵:" + u2.rf.soldiers + " 兵役:" + u2.labor;
      }
      defense.text = "城防: " + s.wall;
      defenseWill.text = "守备: " + s.defensePrep;
      string state = "正常";
      if (s.state == Settlement.State.constructing)
      {
        state = "筑城中 预计" + s.buildTurns + "回合完成";
      }
      this.state.text = state;
      this.inNetwork.text = "";

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
