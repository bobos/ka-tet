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
    public Sprite EmptySettlement;
    public Sprite Resigned;
    public Sprite GeneralExecuted;
    public Sprite GeneralSwapped;
    public Sprite GeneralKilled;
    public Sprite Riot;
    public Sprite Retreat;
    public Sprite Poisioned;
    public Sprite Decision;
    public Sprite UnderSiege;

    public delegate void DialogEvent();
    public event DialogEvent eventDialogOn;
    public event DialogEvent eventDialogOff;
    public enum EventName {
      FloodDestroyUnit,
      WildFireDestroyUnit,
      EnemyCaptureCamp,
      EnemyCaptureCity,
      WeCaptureCamp,
      WeCaptureCity,
      Poision,
      Epidemic,
      Disbanded,
      UnitConflict,
      AltitudeSickness,
      PlainSickness,
      Riot,
      GeneralKilledInBattle,
      FarmDestroyed,
      FarmDestroyedReported,
      UnderSiege,
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

    //public delegate void OnDecisionMade(bool accept, Unit unit);

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
      Unit unit1 = dialogEvent.unit1;
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
      if (eventDialogOn != null) eventDialogOn();
      self.SetActive(true);

      if (name == EventName.FloodDestroyUnit) {
        title.text = textLib.get("event_floodDestroyUnit_title");
        description.text = System.String.Format(textLib.get("event_floodDestroyUnit"), unit.GeneralName(),
          unit.Name(), argu3);
        image.sprite = flood;
      }

      if (name == EventName.WildFireDestroyUnit) {
        title.text = textLib.get("event_wildfireDestroyUnit_title");
        description.text = System.String.Format(textLib.get("event_wildfireDestroyUnit"), unit.GeneralName(),
          unit.Name(), argu3);
        image.sprite = flood;
      }

      if (name == EventName.Disbanded) {
        title.text = textLib.get("event_disbandDestroyUnit_title");
        description.text = System.String.Format(textLib.get("event_disbandDestroyUnit"), unit.GeneralName(),
          unit.Name(), argu3);
        image.sprite = UnitDestroyByDisband;
      }

      if (name == EventName.EnemyCaptureCamp) {
        title.text = textLib.get("event_enemyCaptureCamp_title");
        description.text = System.String.Format(textLib.get("event_enemyCaptureCamp"), settlement.name);
        image.sprite = CityLost;
      }

      if (name == EventName.EnemyCaptureCity) {
        title.text = textLib.get("event_enemyCaptureCity_title");
        description.text = System.String.Format(textLib.get("event_enemyCaptureCity"), settlement.name, argu1, argu2, argu3);
        image.sprite = CityLost;
      }

      if (name == EventName.WeCaptureCamp) {
        title.text = textLib.get("event_weCaptureCamp_title");
        description.text = System.String.Format(textLib.get("event_weCaptureCamp"), settlement.name);
        image.sprite = CityLost;
      }

      if (name == EventName.WeCaptureCity) {
        title.text = textLib.get("event_weCaptureCity_title");
        description.text = System.String.Format(textLib.get("event_weCaptureCity"), settlement.name, argu1, argu2, argu3);
        image.sprite = CityLost;
      }

      if (name == EventName.Riot) {
        title.text = textLib.get("event_riot_title");
        description.text = System.String.Format(textLib.get("event_riot"),
          unit.GeneralName(), unit.Name(), argu1);
        image.sprite = Riot;
      }

      if (name == EventName.GeneralKilledInBattle) {
        title.text = textLib.get("event_generalKilled_title");
        description.text = System.String.Format(textLib.get("event_generalKilled"),
          general.Name());
        image.sprite = drown;
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
          unit.GeneralName(), unit.rf.general.party.Name(), argu1);
        image.sprite = GeneralSwapped;
      }

      if (name == EventName.UnderSiege) {
        title.text = textLib.get("event_underSiege_title");
        description.text = System.String.Format(textLib.get("event_underSiege"),
          settlement.name);
        image.sprite = UnderSiege;
      }

      if (name == EventName.UnitConflict) {
        title.text = textLib.get("event_unitConflict_title");
        description.text = System.String.Format(textLib.get("event_unitConflict"),
          unit.rf.general.Name(), unit1.rf.general.Name(), argu2 + argu3, argu1);
        image.sprite = drown;
      }

      if (name == EventName.AltitudeSickness) {
        title.text = textLib.get("event_altitudeSickness_title");
        description.text = System.String.Format(textLib.get("event_altitudeSickness"),
          unit.rf.general.Name());
        image.sprite = Retreat;
      }

      if (name == EventName.PlainSickness) {
        title.text = textLib.get("event_plainSickness_title");
        description.text = System.String.Format(textLib.get("event_plainSickness"),
          unit.rf.general.Name());
        image.sprite = Retreat;
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
    public Unit unit1 = null;
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
      Settlement settlement1 = null, General general= null, Unit unit1 = null) {
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
        this.unit1 = unit1;
    }
  }
}
