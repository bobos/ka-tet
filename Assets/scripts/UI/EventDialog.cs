using TextNS;
using UnitNS;
using UnityEngine;
using UnityEngine.UI;

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
    public Text description;
    public Text title;
    public Image image;
    public Sprite flood;
    public Sprite wildfire;
    public Sprite drown;
    public Sprite Gale;

    public delegate void DialogEvent();
    public event DialogEvent eventDialogOn;
    public event DialogEvent eventDialogOff;
    public enum EventName {
      LandSlide,
      Flood,
      WildFire,
      WildFireDestroyUnit,
      WildFireDestroyCamp,
      Drown,
      Gale,
      Poision,
      Dehydration,
      Null
    }
    public override void UpdateChild() {}

    public void OnBtnClick(ActionController.actionName actionName) {
      if (actionName == ActionController.actionName.EVENTDIALOGCONFIRM) {
        self.SetActive(false);
        if (eventDialogOff != null) eventDialogOff();
      }
    }

    public void Show(EventName name, Unit unit, Settlement settlement, int moraleReduce, int wounded, int killed, int killedLabor) {
      if (name == EventName.Null) return; 
      // TODO: AI Test
      // TODO: Queue Event
      if (eventDialogOn != null) eventDialogOn();
      self.SetActive(true);
      if (name == EventName.WildFire) {
        title.text = textLib.get("event_wildFire_title");
        description.text = System.String.Format(textLib.get("event_wildFire"), unit.GeneralName(),
          unit.Name(), killed, killedLabor, wounded, moraleReduce);
        image.sprite = wildfire;
      }
    }

  }
}
