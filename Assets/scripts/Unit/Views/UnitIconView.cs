using CourtNS;
using MapTileNS;
using UnityEngine;

namespace UnitNS
{
  public class UnitIconView : MonoBehaviour {
  
    // Use this for initialization
    void Start () {
    }
  
    public void SetName(Unit unit) {
      TextMesh textMesh = this.transform.GetComponent<TextMesh>();
      Color color = Color.white;
      textMesh.text =
      unit.wavingPoint +
      (unit.mentality == Mental.Supercharged ? "☻" :
      (unit.mentality == Mental.Waving ? "∇" :
      (unit.mentality == Mental.Defeating ? "∇∇" :
      (unit.mentality == Mental.Chaotic ? "∇∇∇": ""))))
      + (unit.defeatStreak > 0 ? "☹": "")
      + (unit.hasNoOpenning ? "▦" : "")
      + (unit.fooled ? "⊜" : "")
      + (unit.inCommanderRange ? "㊬" : "")
      + (unit.rf.general.Has(Cons.ambusher) ? "☸": "")
      + (unit.rf.general.Has(Cons.tactic) ? "≜" : "")
      + (unit.rf.general.Has(Cons.fireBug) ? "♨" : "")
      + (unit.rf.general.Has(Cons.runner) ? "↹" : "")
      + (unit.rf.general.Has(Cons.staminaManager) ? "♋" : "")
      + (unit.rf.general.Has(Cons.outlooker) ? "⦿" : "")
      + (unit.rf.general.Has(Cons.formidable) ? "✴" : "")
      + (unit.rf.general.Has(Cons.discipline) ? "✪" : "")
      + (unit.rf.general.Has(Cons.generous) ? "㊎" : "")
      + (unit.rf.general.Has(Cons.hammer) ? "➲" : "")
      + (unit.rf.general.Has(Cons.breaker) && unit.IsHeavyCavalry() ? "♗" : "")
      + (unit.rf.general.Has(Cons.diminisher) ? "▣" : "")
      + (unit.rf.general.Has(Cons.mechanician) ? "♜" : "")
      + (unit.rf.general.Has(Cons.holdTheGround) ? "☍" : "")
      + (unit.rf.general.Has(Cons.forecaster) ? "➹" : "")
      + (unit.rf.general.Has(Cons.doctor) ? "✚" : "")
      + (unit.rf.general.Has(Cons.conspirator) ? "☯" : "")
      + (unit.rf.general.Has(Cons.vanguard) && unit.IsCavalry() && !unit.IsHeavyCavalry()? "♦" : "")
      + (unit.rf.general.Has(Cons.improvisor) ? "❥" : "");
      textMesh.fontSize = 65;
      textMesh.color = color;
    }

    // Update is called once per frame
    void Update () {
      transform.rotation = Camera.main.transform.rotation;
    }
  }

}