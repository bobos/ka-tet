using TextNS;
using UnitNS;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
    public Sprite Defeated;
    public Sprite Disarmor;

    public delegate void DialogEvent();
    public event DialogEvent eventDialogOn;
    public event DialogEvent eventDialogOff;
    public enum EventName {
      LandSlide,
      Flood,
      WildFire,
      WildFireDestroyUnit,
      WildFireDestroyCamp,
      BurningCampDestroyUnit,
      EnemyCaptureCamp,
      EnemyCaptureCity,
      Drown,
      Gale,
      Poision,
      Dehydration,
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
      Null
    }

    Unit currentUnit = null;
    Settlement currentSettlement = null;
    EventName currentEvent = EventName.Null;

    public override void UpdateChild() {}

    public void OnAcceptClick() {
      if (currentEvent == EventName.Disarmor && onDisarmorDecisionClick != null) {
        onDisarmorDecisionClick(true, currentUnit);
      }
      Continue();
    }

    public void OnRejectClick() {
      if (currentEvent == EventName.Disarmor && onDisarmorDecisionClick != null) {
        onDisarmorDecisionClick(false, currentUnit);
      }
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
      self.SetActive(false);
      if (eventDialogOff != null) eventDialogOff();
      isShowing = false;
      currentEvent = EventName.Null;
      currentSettlement = null;
      currentUnit = null;
      if (events.Count > 0) {
        Show(events.Dequeue());
      }
    }

    bool isShowing = false;
    Queue<Event> events = new Queue<Event>();
    // TODO: only for player
    public void Show(Event dialogEvent) {
      if (isShowing) {
        events.Enqueue(dialogEvent);
        return;
      }
      EventName name = currentEvent = dialogEvent.name;
      Unit unit = currentUnit = dialogEvent.unit;
      Settlement settlement = currentSettlement = dialogEvent.settlement;
      Settlement settlement1 = dialogEvent.settlement1;
      int argu5 = dialogEvent.supply;
      int argu1 = dialogEvent.moraleReduce;
      int argu2 = dialogEvent.wounded;
      int argu3 = dialogEvent.killed;
      int argu4 = dialogEvent.killedLabor;
      if (name == EventName.Null) return; 
      isShowing = true;
      ToggleConfirm();

      // TODO: AI Test
      // TODO: Queue Event
      if (eventDialogOn != null) eventDialogOn();
      self.SetActive(true);
      if (name == EventName.WildFire) {
        title.text = textLib.get("event_wildFire_title");
        description.text = System.String.Format(textLib.get("event_wildFire"), unit.GeneralName(),
          unit.Name(), argu3, argu4, argu2, argu1);
        image.sprite = wildfire;
      }

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
        image.sprite = UnitDestroyByBurningCamp;
      }

      if (name == EventName.EnemyCaptureCamp) {
        title.text = textLib.get("event_enemyCaptureCamp_title");
        description.text = System.String.Format(textLib.get("event_enemyCaptureCamp"), settlement.name);
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

      if (name == EventName.Disarmor) {
        ToggleDecision();
        title.text = textLib.get("event_disarmor_title");
        description.text = System.String.Format(textLib.get("event_disarmor"),
          unit.GeneralName(), unit.Name());
        image.sprite = Disarmor;
        approveText.text = System.String.Format(textLib.get("event_disarmor_approve_title"), argu1);
        disapproveText.text = System.String.Format(textLib.get("event_disarmor_disapprove_title"), argu2);
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
    public Event(EventDialog.EventName name, Unit unit, Settlement settlement,
      int moraleReduce = 0, int wounded = 0, int killed = 0, int killedLabor = 0, int supply = 0,
      Settlement settlement1 = null) {
        this.name = name;
        this.unit = unit;
        this.settlement = settlement;
        this.moraleReduce = moraleReduce;
        this.wounded = wounded;
        this.killed = killed;
        this.killedLabor = killedLabor;
        this.supply = supply;
        this.settlement1 = settlement1;
    }
  }
}
