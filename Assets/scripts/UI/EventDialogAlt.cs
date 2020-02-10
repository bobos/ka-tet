using TextNS;
using UnitNS;
using UnityEngine;
using UnityEngine.UI;

namespace MonoNS
{
  public class EventDialogAlt : BaseController
  {

    // Use this for initialization
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      self = this.transform.gameObject;
      self.SetActive(false);
      hexMap.actionController.onBtnClick += OnBtnClick;
    }

    public void OnBtnClick(ActionController.actionName actionName) {
      if (actionName == ActionController.actionName.MIDBTN1) {
        btn1Clicked = true;
        Continue();
      }
      if (actionName == ActionController.actionName.MIDBTN2) {
        btn2Clicked = true;
        Continue();
      }
    }

    public bool Animating = false;
    TextLib textLib = Cons.GetTextLib();
    GameObject self;

    public GameObject btn1;
    public GameObject btn2;
    public Text leftTitle;
    public Text rightTitle;
    public Image leftImg;
    public Image rightImg;
    public Text leftDescription;
    public Text rightDescription;
    public Text middleDescription;

    public Sprite attackerSide;
    public Sprite defenderSide;

    public override void UpdateChild() {}
    public bool btn1Clicked = false;
    public bool btn2Clicked = false;

    void Continue() {
      Animating = false;
      self.SetActive(false);
    }

    public void ShowOperationResult(CombatController.ResultType result, bool attackerWin, bool playerAttack,
      int atkInfTotal, int atkCavTotal, int defInfTotal, int defCavTotal,
      int atkInfDead, int atkInfWnd, int atkLaborDead, int atkCavDead, int atkCavWnd,
      int defInfDead, int defInfWnd, int defLaborDead, int defCavDead, int defCavWnd,
      int capturedHorse) {
      btn1Clicked = btn2Clicked = false;
      Animating = true;
      btn1.SetActive(true);
      btn2.SetActive(false);
      self.SetActive(true);

      leftTitle.text = textLib.get("event_attackerSide_title") + "(" + 
        (playerAttack ? textLib.get("event_ourSide_title") : textLib.get("event_aiSide_title")) + ")";

      rightTitle.text = textLib.get("event_defenderSide_title") + "(" + 
        (playerAttack ? textLib.get("event_aiSide_title") : textLib.get("event_ourSide_title")) + ")";

      leftImg.sprite = attackerSide;
      rightImg.sprite = defenderSide;

      leftDescription.text = textLib.get("event_operationResult_infTotal") + ": " + atkInfTotal + "\n" +
        textLib.get("event_operationResult_infDead") + ": " + atkInfDead + "\n" +
        textLib.get("event_operationResult_infWnd") + ": " + atkInfWnd + "\n\n" +
        textLib.get("event_operationResult_cavTotal") + ": " + atkCavTotal + "\n" +
        textLib.get("event_operationResult_cavDead") + ": " + atkCavDead + "\n" +
        textLib.get("event_operationResult_cavWnd") + ": " + atkCavWnd + "\n\n" +
        textLib.get("event_operationResult_total") + ": " + (atkInfDead + atkCavDead) + "\n\n" +
        textLib.get("event_operationResult_laborDead") + ": " + atkLaborDead + "\n";

      rightDescription.text = textLib.get("event_operationResult_infTotal") + ": " + defInfTotal + "\n" +
        textLib.get("event_operationResult_infDead") + ": " + defInfDead + "\n" +
        textLib.get("event_operationResult_infWnd") + ": " + defInfWnd + "\n\n" +
        textLib.get("event_operationResult_cavTotal") + ": " + defCavTotal + "\n" +
        textLib.get("event_operationResult_cavDead") + ": " + defCavDead + "\n" +
        textLib.get("event_operationResult_cavWnd") + ": " + defCavWnd + "\n\n" +
        textLib.get("event_operationResult_total") + ": " + (defInfDead + defCavDead) + "\n\n" +
        textLib.get("event_operationResult_laborDead") + ": " + defLaborDead + "\n";

      string horseCapture = textLib.get("event_operationResult_horse") + ": " + capturedHorse + "\n";
      if (attackerWin) {
        leftDescription.text += horseCapture;
      } else {
        rightDescription.text += horseCapture;
      }

      string finalResult;
      if (attackerWin == playerAttack) {
        // player win
        if (result == CombatController.ResultType.Close) {
          finalResult = "event_operationResult_weCloseWin";
        } else if (result == CombatController.ResultType.Small) {
          finalResult = "event_operationResult_weSmallWin";
        } else if (result == CombatController.ResultType.Great) {
          finalResult = "event_operationResult_weGreatWin";
        } else {
          finalResult = "event_operationResult_weCrushingWin";
        }
      } else {
        if (result == CombatController.ResultType.Close) {
          finalResult = "event_operationResult_theyCloseWin";
        } else if (result == CombatController.ResultType.Small) {
          finalResult = "event_operationResult_theySmallWin";
        } else if (result == CombatController.ResultType.Great) {
          finalResult = "event_operationResult_theyGreatWin";
        } else {
          finalResult = "event_operationResult_theyCrushingWin";
        }
      }

      middleDescription.text = textLib.get(finalResult);
    }

    public void ShowOperationEvent(OperationPredict predict, bool playerAttack) {
      btn1Clicked = btn2Clicked = false;
      Animating = true;
      btn1.SetActive(true);
      if (playerAttack) {
        btn2.SetActive(true);
      } else {
        // TODO: remove AI test
        //btn2.SetActive(false);
        btn2.SetActive(true);
      }
      self.SetActive(true);

      int atkInf = 0;
      int atkCav = 0;
      int defInf = 0;
      int defCav = 0;
      string atkGenerals = "";
      string atkSerial = "";
      string defGenerals = "";
      string defSerial = "";

      foreach (UnitPredict u in predict.attackers) {
        atkGenerals += u.unit.GeneralName() + "\n";
        string unitInfo = u.unit.Name();
        unitInfo += u.unit.GetStaminaLevel() != StaminaLvl.Fresh ? ("[" + u.unit.GetStaminaLvlName() + "]") : "";
        unitInfo += u.windAdvantage ? ("[" + textLib.get("misc_windAdvantage") + "]") : "";
        unitInfo += u.windDisadvantage ? ("[" + textLib.get("misc_windDisadvantage") + "]") : "";
        atkSerial += unitInfo + "\n";

        if (u.unit.IsCavalry()) {
          atkCav += u.unit.rf.soldiers;
        } else {
          atkInf += u.unit.rf.soldiers;
        }
      }

      foreach (UnitPredict u in predict.defenders) {
        defGenerals += u.unit.GeneralName() + "\n";
        defSerial += u.unit.Name() +
          (u.unit.GetStaminaLevel() != StaminaLvl.Fresh ? ("(" + u.unit.GetStaminaLvlName() + ") ") : "") + "\n";
        if (u.unit.IsCavalry()) {
          defCav += u.unit.rf.soldiers;
        } else {
          defInf += u.unit.rf.soldiers;
        }
      }

      leftTitle.text = textLib.get("event_attackerSide_title") + "(" + 
        (playerAttack ? textLib.get("event_ourSide_title") : textLib.get("event_aiSide_title")) + ")";

      rightTitle.text = textLib.get("event_defenderSide_title") + "(" + 
        (playerAttack ? textLib.get("event_aiSide_title") : textLib.get("event_ourSide_title")) + ")";

      leftImg.sprite = attackerSide;
      rightImg.sprite = defenderSide;

      leftDescription.text = textLib.get("event_operationDetail_gen") + ":\n" + atkGenerals + "\n" +
        textLib.get("event_operationDetail_unit") + ":\n" + atkSerial + "\n" +
        textLib.get("event_operationDetail_inf") + ": " + atkInf + "\n" +
        textLib.get("event_operationDetail_cav") + ": " + atkCav + "\n" +
        textLib.get("event_operationDetail_total") + ": " + predict.attackerOptimPoints * 0.001f;

      rightDescription.text = textLib.get("event_operationDetail_gen") + ":\n" + defGenerals + "\n" +
        textLib.get("event_operationDetail_unit") + ":\n" + defSerial + "\n" +
        textLib.get("event_operationDetail_inf") + ": " + defInf + "\n" +
        textLib.get("event_operationDetail_cav") + ": " + defCav + "\n" +
        textLib.get("event_operationDetail_total") + ": " + predict.defenderOptimPoints * 0.001f;

      middleDescription.text = textLib.get("event_possibleVictRate_" + predict.sugguestedResult.chance);
      if (playerAttack) {
        middleDescription.text += "\n" + textLib.get("event_confirmAttack");
      }
    }

  }


}
