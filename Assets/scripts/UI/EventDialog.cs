using TextNS;
using UnitNS;
using UnityEngine;
using UnityEngine.UI;
using CourtNS;

namespace MonoNS
{
  public class EventDialog : BaseController
  {

    // Use this for initialization
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      self = this.transform.gameObject;
      self.SetActive(false);
      hexMap.actionController.onBtnClick += OnBtnClick;
    }

    public bool Animating = false;
    TextLib textLib = Cons.GetTextLib();
    GameObject self;
    public GameObject configrmBtn;
    public GameObject approveBtn;
    public GameObject disapproveBtn;
    public Text description;
    public Text title;
    public Text approveText;
    public Text disapproveText;
    public Image image;
    public Sprite flood;
    public Sprite wildfire;
    public Sprite drown;
    public Sprite Gale;
    public Sprite UnitDestroyByWildFire;
    public Sprite UnitDestroyByBurningCamp;
    public Sprite UnitDestroyByFlood;
    public Sprite UnitDestroyByDisband;
    public Sprite CampDestoyedByFire;
    public Sprite Ambushed;
    public Sprite SupplyIntercepted;
    public Sprite LaborIntercepted;
    public Sprite SupplyReached;
    public Sprite InsufficientSupply;
    public Sprite InsufficientLabor;
    public Sprite CampLost;
    public Sprite CityLost;
    public Sprite Defeated;
    public Sprite Disarmor;
    public Sprite EmptySettlement;
    public Sprite Resigned;
    public Sprite GeneralExecuted;
    public Sprite GeneralSwapped;
    public Sprite GeneralKilled;
    public Sprite Riot;
    public Sprite Retreat;
    public Sprite Poisioned;
    public Sprite Decision;

    public delegate void DialogEvent();
    public event DialogEvent eventDialogOn;
    public event DialogEvent eventDialogOff;
    public enum EventName {
      FloodDestroyCamp,
      FloodDestroyUnit,
      WildFireDestroyUnit,
      WildFireDestroyCamp,
      BurningCampDestroyUnit,
      EnemyCaptureCamp,
      EnemyCaptureCity,
      WeCaptureCamp,
      WeCaptureCity,
      EnemyBurnCamp,
      WeBurnCamp,
      Poision,
      Epidemic,
      Disbanded,
      SupplyReached,
      SupplyRouteBlocked,
      LaborReached,
      SupplyIntercepted,
      unitSupplyIntercepted,
      LaborIntercepted,
      InsufficientSupply,
      InsufficientLabor,
      InsufficientSupplyLabor,
      Disarmor,
      Disarmor1,
      EmptySettlement,
      Riot,
      GeneralKilledInBattle,
      GeneralReturned,
      GeneralResigned,
      NewGeneral,
      GeneralExecuted,
      Retreat,
      FarmDestroyed,
      FarmDestroyedReported,
      Null
    }

    Unit currentUnit = null;
    Settlement currentSettlement = null;
    EventName currentEvent = EventName.Null;

    public override void UpdateChild() {}

    public bool accepted = false;
    public void OnAcceptClick() {
      accepted = true;
      Continue();
    }

    public void OnRejectClick() {
      accepted= false;
      Continue();
    }

    public delegate void OnDecisionMade(bool accept, Unit unit);
    public event OnDecisionMade onDisarmorDecisionClick;

    public void OnBtnClick(ActionController.actionName actionName) {
      if (actionName == ActionController.actionName.EVENTDIALOGCONFIRM) {
        Continue();
      }
    }

    void Continue() {
      Animating = false;
      self.SetActive(false);
      if (eventDialogOff != null) eventDialogOff();
      currentEvent = EventName.Null;
      currentSettlement = null;
      currentUnit = null;
    }

    // TODO: only for player
    public void Show(Event dialogEvent) {
      Animating = true;
      EventName name = currentEvent = dialogEvent.name;
      Unit unit = currentUnit = dialogEvent.unit;
      Settlement settlement = currentSettlement = dialogEvent.settlement;
      Settlement settlement1 = dialogEvent.settlement1;
      int argu1 = dialogEvent.moraleReduce;
      int argu2 = dialogEvent.wounded;
      int argu3 = dialogEvent.killed;
      int argu4 = dialogEvent.killedLabor;
      int argu5 = dialogEvent.supply;
      General general = dialogEvent.general;
      if (name == EventName.Null) {
        Animating = false;
        return; 
      }
      ToggleConfirm();

      // TODO: AI Test
      // TODO: Queue Event
      if (eventDialogOn != null) eventDialogOn();
      self.SetActive(true);
      if (name == EventName.WildFireDestroyCamp) {
        title.text = textLib.get("event_fireDestroyCamp_title");
        description.text = System.String.Format(textLib.get("event_fireDestroyCamp"), settlement.name,
          settlement.labor, settlement.supplyDeposit);
        image.sprite = CampDestoyedByFire;
      }

      if (name == EventName.BurningCampDestroyUnit) {
        title.text = textLib.get("event_burningCampDestroyUnit_title");
        description.text = System.String.Format(textLib.get("event_burningCampDestroyUnit"), unit.GeneralName(),
          unit.Name(), argu3);
        image.sprite = wildfire;
      }

      if (name == EventName.FloodDestroyCamp) {
        title.text = textLib.get("event_floodDestroyCamp_title");
        description.text = System.String.Format(textLib.get("event_floodDestroyCamp"), settlement.name,
          settlement.labor, settlement.supplyDeposit);
        image.sprite = flood;
      }

      if (name == EventName.FloodDestroyUnit) {
        title.text = textLib.get("event_floodDestroyUnit_title");
        description.text = System.String.Format(textLib.get("event_floodDestroyUnit"), unit.GeneralName(),
          unit.Name(), argu3);
        image.sprite = flood;
      }

      if (name == EventName.EnemyCaptureCamp) {
        title.text = textLib.get("event_enemyCaptureCamp_title");
        description.text = System.String.Format(textLib.get("event_enemyCaptureCamp"), settlement.name, argu1);
        image.sprite = CityLost;
      }

      if (name == EventName.EnemyCaptureCity) {
        title.text = textLib.get("event_enemyCaptureCity_title");
        description.text = System.String.Format(textLib.get("event_enemyCaptureCity"), settlement.name, argu1, argu2, argu3);
        image.sprite = CityLost;
      }

      if (name == EventName.WeCaptureCamp) {
        title.text = textLib.get("event_weCaptureCamp_title");
        description.text = System.String.Format(textLib.get("event_weCaptureCamp"), settlement.name, argu1);
        image.sprite = CityLost;
      }

      if (name == EventName.WeCaptureCity) {
        title.text = textLib.get("event_weCaptureCity_title");
        description.text = System.String.Format(textLib.get("event_weCaptureCity"), settlement.name, argu1, argu2, argu3);
        image.sprite = CityLost;
      }

      if (name == EventName.WeBurnCamp) {
        title.text = textLib.get("event_weBurnCamp_title");
        description.text = System.String.Format(textLib.get("event_weBurnCamp"), settlement.name);
        image.sprite = CampLost;
      }

      if (name == EventName.EnemyBurnCamp) {
        title.text = textLib.get("event_enemyBurnCamp_title");
        description.text = System.String.Format(textLib.get("event_enemyBurnCamp"), settlement.name);
        image.sprite = CampLost;
      }

      if (name == EventName.InsufficientLabor) {
        title.text = textLib.get("event_insufficientLabor_title");
        description.text = System.String.Format(textLib.get("event_insufficientLabor"), settlement.name, argu1, argu2);
        image.sprite = InsufficientLabor;
      }

      if (name == EventName.InsufficientSupply) {
        title.text = textLib.get("event_insufficientSupply_title");
        description.text = System.String.Format(textLib.get("event_insufficientSupply"), settlement.name, argu1, argu2);
        image.sprite = InsufficientSupply;
      }

      if (name == EventName.InsufficientSupplyLabor) {
        title.text = textLib.get("event_insufficientSupplyLabor_title");
        description.text = System.String.Format(textLib.get("event_insufficientSupplyLabor"), argu1, argu2, settlement.name, argu3);
        image.sprite = InsufficientLabor;
      }

      if (name == EventName.SupplyReached) {
        title.text = textLib.get("event_supplyDone_title");
        description.text = System.String.Format(textLib.get("event_supplyDone"), settlement.name, argu1);
        image.sprite = SupplyReached;
      }

      if (name == EventName.LaborReached) {
        title.text = textLib.get("event_laborDone_title");
        description.text = System.String.Format(textLib.get("event_laborDone"), settlement.name, argu1);
        image.sprite = SupplyReached;
      }

      if (name == EventName.SupplyIntercepted) {
        title.text = textLib.get("event_supplyIntercepted_title");
        description.text = System.String.Format(textLib.get("event_supplyIntercepted"), settlement.name, argu1, argu2);
        image.sprite = SupplyIntercepted;
      }

      if (name == EventName.LaborIntercepted) {
        title.text = textLib.get("event_laborIntercepted_title");
        description.text = System.String.Format(textLib.get("event_laborIntercepted"), settlement.name, argu1);
        image.sprite = Ambushed;
      }

      if (name == EventName.unitSupplyIntercepted) {
        title.text = textLib.get("event_unitSupplyIntercepted_title");
        description.text = System.String.Format(textLib.get("event_unitSupplyIntercepted"),
          unit.GeneralName(), unit.Name(), argu1, argu2);
        image.sprite = LaborIntercepted;
      }

      if (name == EventName.SupplyRouteBlocked) {
        title.text = textLib.get("event_supplyRouteBlocked_title");
        description.text = System.String.Format(textLib.get("event_supplyRouteBlocked"),
          settlement.name, settlement1.name);
        image.sprite = Defeated;
      }

      if (name == EventName.Riot) {
        title.text = textLib.get("event_riot_title");
        description.text = System.String.Format(textLib.get("event_riot"),
          unit.GeneralName(), unit.Name(), argu1);
        image.sprite = Riot;
      }

      if (name == EventName.GeneralExecuted) {
        title.text = textLib.get("event_generalExecuted_title");
        description.text = System.String.Format(textLib.get("event_generalExecuted"),
          general.Name());
        image.sprite = GeneralExecuted;
      }

      if (name == EventName.NewGeneral) {
        title.text = textLib.get("event_newGeneral_title");
        description.text = System.String.Format(textLib.get("event_newGeneral"),
          unit.GeneralName(), unit.Name());
        image.sprite = GeneralSwapped;
      }

      if (name == EventName.GeneralReturned) {
        title.text = textLib.get("event_generalReturned_title");
        description.text = System.String.Format(textLib.get("event_generalReturned"),
          general.Name());
        image.sprite = GeneralSwapped;
      }

      if (name == EventName.GeneralResigned) {
        title.text = textLib.get("event_generalResigned_title");
        description.text = System.String.Format(textLib.get("event_generalResigned"),
          general.Name());
        image.sprite = Resigned;
      }

      if (name == EventName.GeneralKilledInBattle) {
        title.text = textLib.get("event_generalKilled_title");
        description.text = System.String.Format(textLib.get("event_generalKilled"),
          general.Name());
        image.sprite = drown;
      }

      if (name == EventName.Retreat) {
        title.text = textLib.get("event_unitRetreat_title");
        description.text = System.String.Format(textLib.get("event_unitRetreat"),
          unit.Name());
        image.sprite = Retreat;
      }

      if (name == EventName.Epidemic) {
        title.text = textLib.get("event_epidemic_title");
        description.text = System.String.Format(textLib.get("event_epidemic"),
          unit.GeneralName(), unit.Name());
        image.sprite = Gale;
      }

      if (name == EventName.Poision) {
        title.text = textLib.get("event_poision_title");
        description.text = System.String.Format(textLib.get("event_poision"),
          unit.GeneralName(), unit.Name());
        image.sprite = Poisioned;
      }
      
      if (name == EventName.FarmDestroyedReported) {
        title.text = textLib.get("event_farmDestroyedReported_title");
        description.text = System.String.Format(textLib.get("event_farmDestroyedReported"),
          unit.GeneralName(), unit.rf.general.party.Name(), argu1);
        image.sprite = GeneralSwapped;
      }

      if (name == EventName.FarmDestroyed) {
        title.text = textLib.get("event_farmDestroyed_title");
        description.text = System.String.Format(textLib.get("event_farmDestroyed"),
          unit.GeneralName(), argu1);
        image.sprite = Decision;
      }

      if (name == EventName.Disarmor) {
        title.text = textLib.get("event_disarmor_title");
        description.text = System.String.Format(textLib.get("event_disarmor"),
          unit.GeneralName(), unit.Name(), argu1);
        image.sprite = Disarmor;
      }

      if (name == EventName.Disarmor1) {
        title.text = textLib.get("event_disarmor_title");
        description.text = System.String.Format(textLib.get("event_disarmor1"),
          unit.GeneralName(), unit.Name(), argu1);
        image.sprite = Disarmor;
      }

      if (name == EventName.EmptySettlement) {
        ToggleDecision();
        title.text = textLib.get("event_emptySettlement_title");
        description.text = System.String.Format(textLib.get("event_emptySettlement"),
          settlement.name);
        image.sprite = EmptySettlement;
        approveText.text = textLib.get("event_emptySettlement_occupy_title");
        disapproveText.text = textLib.get("event_emptySettlement_burndown_title");
      }

    }

    void ToggleConfirm() {
      configrmBtn.SetActive(true);
      approveBtn.SetActive(false);
      disapproveBtn.SetActive(false);
    }

    void ToggleDecision() {
      configrmBtn.SetActive(false);
      approveBtn.SetActive(true);
      disapproveBtn.SetActive(true);
    }

  }

  public class Event {
    public EventDialog.EventName name;
    public Unit unit = null;
    public Settlement settlement = null;
    public int moraleReduce = 0;
    public int wounded = 0;
    public int killed = 0;
    public int killedLabor = 0;
    public int supply = 0; 
    public Settlement settlement1 = null;
    public General general = null;
    public Event(EventDialog.EventName name, Unit unit, Settlement settlement,
      int moraleReduce = 0, int wounded = 0, int killed = 0, int killedLabor = 0, int supply = 0,
      Settlement settlement1 = null, General general= null) {
        this.name = name;
        this.unit = unit;
        this.settlement = settlement;
        this.moraleReduce = moraleReduce;
        this.wounded = wounded;
        this.killed = killed;
        this.killedLabor = killedLabor;
        this.supply = supply;
        this.settlement1 = settlement1;
        this.general = general;
    }
  }
}
