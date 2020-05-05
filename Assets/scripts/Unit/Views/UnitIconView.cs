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
      textMesh.text = (unit.defeating ? "∇∇" : (unit.chaos ? "∇∇∇": (unit.defeatStreak > 0 ? "∇": "")))
        + (unit.InCommanderRange() ? "㊬" : "")
        + (unit.alerted ? "◎" : "")
        + (unit.rf.general.Has(Cons.ambusher) ? "☸": "")
        + (unit.rf.general.Has(Cons.tactic) ? "☯" : "")
        + (unit.rf.general.Has(Cons.fireBug) ? "♨" : "")
        + (unit.rf.general.Has(Cons.runner) ? "↹" : "")
        + (unit.rf.general.Has(Cons.staminaManager) ? "♋" : "")
        + (unit.rf.general.Has(Cons.outlooker) ? "⦿" : "")
        + (unit.rf.general.Has(Cons.formidable) ? "✴" : "")
        //+ (unit.rf.general.Has(Cons.formidable) ? "✪" : "")
        + (unit.rf.general.Has(Cons.hammer) ? "➲" : "")
        + (unit.rf.general.Has(Cons.breaker) && unit.IsCavalry() ? "♘" : "")
        + (unit.rf.general.Has(Cons.diminisher) ? "▣" : "")
        + (unit.rf.general.Has(Cons.mechanician) ? "♜" : "")
        + (unit.rf.general.Has(Cons.holdTheGround) ? "☍" : "")
        + (unit.rf.general.Has(Cons.forecaster) ? "➹" : "")
        + (unit.ImproviseOnSupply() ? "❥" : "");
      textMesh.fontSize = 65;
      textMesh.color = color;
    }

    // Update is called once per frame
    void Update () {
      transform.rotation = Camera.main.transform.rotation;
    }
  }

}