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

    int phase = 0;
    public override void UpdateChild() {
      if (phase == 0) {
        return;
      }
      if (Input.GetMouseButtonUp(0)) {
        if (phase == 1) {
          DialogNext();
        } else if (phase == 2) {
          DialogEnd();
        }
      }
    }

    string leftCacheDialog = "";
    string rightCacheDialog = "";
    public void ShowRoutingImpactIncident(Unit routingUnit, Unit impactedUnit) {
      phase = 1;
      Animating = true;
      self.SetActive(true);
      right.SetActive(true);
      left.SetActive(true);

      leftImg.sprite = routingSoldier;
      leftText.text = "";
      leftCacheDialog = System.String.Format(textLib.get("event_routingRetreatSoldierDialog"), routingUnit.GeneralName());
      rightImg.sprite = scaredSoldier;
      rightText.text = System.String.Format(textLib.get("event_routingImpactSoldierDialog"), impactedUnit.GeneralName());
    }

    void DialogNext() {
      phase = 2;
      if (leftCacheDialog != "") {
        leftText.text = leftCacheDialog;
      } else {
        rightText.text = rightCacheDialog;
      }
    }

    void DialogEnd() {
      self.SetActive(false);
      Animating = false;
      phase = 0;
      leftCacheDialog = rightCacheDialog = "";
    }

    public void ShowNoRetreatEvent(Unit defender) {
      phase = 1;
      Animating = true;
      self.SetActive(true);
      right.SetActive(true);
      left.SetActive(true);

      leftImg.sprite = hexMap.imgLibrary.GetGeneralPortrait(defender.rf.general);
      leftText.text = System.String.Format(textLib.get("event_NoRetreatGeneralDialog"), defender.GeneralName());
      rightImg.sprite = normalSoldier;
      rightCacheDialog = System.String.Format(textLib.get("event_NoRetreatSoldierDialog"), defender.GeneralName());
      rightText.text = "";
    }

    public void ShowRemoveHelmet(Unit unit, bool allowed) {
      phase = 1;
      Animating = true;
      self.SetActive(true);
      right.SetActive(true);
      left.SetActive(true);

      leftImg.sprite = hexMap.imgLibrary.GetGeneralPortrait(unit.rf.general);
      leftCacheDialog = System.String.Format(textLib.get(allowed ? "event_RemoveAllowedGeneralDialog": "event_RemoveDisallowedGeneralDialog"),
        unit.GeneralName());
      leftText.text = "";
      rightImg.sprite = normalSoldier;
      rightText.text = System.String.Format(textLib.get("event_RemoveHelmetSoldierDialog"), unit.GeneralName());
    }

    public void ShowRemoveHelmetFollow(Unit unit, bool allowed) {
      phase = 1;
      Animating = true;
      self.SetActive(true);
      right.SetActive(true);
      left.SetActive(true);

      leftImg.sprite = hexMap.imgLibrary.GetGeneralPortrait(unit.rf.general);
      leftCacheDialog = System.String.Format(textLib.get(allowed ? "event_RemoveAllowedGeneralDialog": "event_RemoveDisallowedGeneralDialog"), unit.GeneralName());
      leftText.text = "";
      rightImg.sprite = normalSoldier;
      rightText.text = System.String.Format(textLib.get("event_RemoveHelmetSoldierDialogFollow"),
        unit.GeneralName(), hexMap.GetWarParty(unit).firstRemoveArmor.GeneralName());
    }

    public void ShowFormationBreaking(Unit unit) {
      phase = 2;
      Animating = true;
      self.SetActive(true);
      string text = System.String.Format(textLib.get("event_FormationBreaking_song2"), unit.GeneralName());
      if (hexMap.IsAttackSide(unit.IsAI())) {
        right.SetActive(false);
        left.SetActive(true);
        leftImg.sprite = hexMap.imgLibrary.GetGeneralPortrait(unit.rf.general);
        leftText.text = text;
      } else {
        left.SetActive(false);
        right.SetActive(true);
        rightImg.sprite = hexMap.imgLibrary.GetGeneralPortrait(unit.rf.general);
        rightText.text = text;
      }
    }

    public void ShowRetreat(Unit unit) {
      phase = 2;
      Animating = true;
      self.SetActive(true);
      string text = System.String.Format(textLib.get("event_Retreat"), unit.GeneralName());
      if (hexMap.IsAttackSide(unit.IsAI())) {
        right.SetActive(false);
        left.SetActive(true);
        leftImg.sprite = hexMap.imgLibrary.GetGeneralPortrait(unit.rf.general);
        leftText.text = text;
      } else {
        left.SetActive(false);
        right.SetActive(true);
        rightImg.sprite = hexMap.imgLibrary.GetGeneralPortrait(unit.rf.general);
        rightText.text = text;
      }
    }

    public void ShowRefuseToRetreat(Unit unit) {
      phase = 2;
      Animating = true;
      self.SetActive(true);
      string text = System.String.Format(textLib.get("event_RefuseToRetreat"), unit.GeneralName());
      if (hexMap.IsAttackSide(unit.IsAI())) {
        right.SetActive(false);
        left.SetActive(true);
        leftImg.sprite = hexMap.imgLibrary.GetGeneralPortrait(unit.rf.general);
        leftText.text = text;
      } else {
        left.SetActive(false);
        right.SetActive(true);
        rightImg.sprite = hexMap.imgLibrary.GetGeneralPortrait(unit.rf.general);
        rightText.text = text;
      }
    }

    public void ShowChaseDialogue(Unit unit) {
      phase = 2;
      Animating = true;
      self.SetActive(true);
      string text = System.String.Format(textLib.get("event_ChaseDialogue"), unit.GeneralName());
      if (hexMap.IsAttackSide(unit.IsAI())) {
        right.SetActive(false);
        left.SetActive(true);
        leftImg.sprite = hexMap.imgLibrary.GetGeneralPortrait(unit.rf.general);
        leftText.text = text;
      } else {
        left.SetActive(false);
        right.SetActive(true);
        rightImg.sprite = hexMap.imgLibrary.GetGeneralPortrait(unit.rf.general);
        rightText.text = text;
      }
    }

    public void ShowFeintDefeat(Unit unit) {
      phase = 2;
      Animating = true;
      self.SetActive(true);
      string text = System.String.Format(textLib.get("event_FeintDefeat"), unit.GeneralName());
      if (hexMap.IsAttackSide(unit.IsAI())) {
        right.SetActive(false);
        left.SetActive(true);
        leftImg.sprite = hexMap.imgLibrary.GetGeneralPortrait(unit.rf.general);
        leftText.text = text;
      } else {
        left.SetActive(false);
        right.SetActive(true);
        rightImg.sprite = hexMap.imgLibrary.GetGeneralPortrait(unit.rf.general);
        rightText.text = text;
      }
    }

  }


}
