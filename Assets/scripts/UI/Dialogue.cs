using TextNS;
using UnitNS;
using UnityEngine;
using UnityEngine.UI;

namespace MonoNS
{
  public class Dialogue : BaseController
  {

    // Use this for initialization
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      self = this.transform.gameObject;
      left = GameObject.Find("LeftPart").gameObject;
      right = GameObject.Find("RightPart").gameObject;
      self.SetActive(false);
    }

    public bool Animating = false;
    TextLib textLib = Cons.GetTextLib();
    GameObject self;
    GameObject left;
    GameObject right;

    public Text leftText;
    public Text rightText;
    public Image leftImg;
    public Image rightImg;

    public Sprite routingSoldier;
    public Sprite scaredSoldier;
    public Sprite normalSoldier;
    public Sprite AttackGeneral;
    public Sprite DefenderGeneral;

    int phase = 0;
    public override void UpdateChild() {
      if (phase == 0) {
        return;
      }
      if (Input.GetMouseButtonUp(0)) {
        if (phase == 1) {
          ShowRoutingImpactIncidentNext();
        } else if (phase == 2) {
          ShowRoutingImpactIncidentFinal();
        } else if (phase == 3) {
          ShowNoRetreatEventNext();
        } else if (phase == 4) {
          ShowNoRetreatEventFinal();
        }
      }
    }

    public void ShowRoutingImpactIncident(Unit routingUnit, Unit impactedUnit) {
      phase = 1;
      Animating = true;
      self.SetActive(true);
      leftImg.sprite = routingSoldier;
      leftText.text = System.String.Format(textLib.get("event_routingRetreatSoldierDialog"), routingUnit.GeneralName());
      rightImg.sprite = scaredSoldier;
      rightText.text = System.String.Format(textLib.get("event_routingImpactSoldierDialog"), impactedUnit.GeneralName());
      right.SetActive(true);
      left.SetActive(false);
    }

    void ShowRoutingImpactIncidentNext() {
      phase = 2;
      left.SetActive(true);
      right.SetActive(false);
    }

    void ShowRoutingImpactIncidentFinal() {
      self.SetActive(false);
      Animating = false;
      phase = 0;
    }

    public void ShowNoRetreatEvent(Unit defender) {
      phase = 3;
      Animating = true;
      self.SetActive(true);
      leftImg.sprite = defender.IsAI() ? AttackGeneral : DefenderGeneral;
      leftText.text = System.String.Format(textLib.get("event_NoRetreatGeneralDialog"), defender.GeneralName());
      rightImg.sprite = normalSoldier;
      rightText.text = System.String.Format(textLib.get("event_NoRetreatSoldierDialog"), defender.GeneralName());
      right.SetActive(false);
      left.SetActive(true);
    }

    void ShowNoRetreatEventNext() {
      phase = 4;
      left.SetActive(false);
      right.SetActive(true);
    }

    void ShowNoRetreatEventFinal() {
      self.SetActive(false);
      Animating = false;
      phase = 0;
    }

  }


}
