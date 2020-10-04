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
      unit.morale
      + (unit.IsVulnerable() ? "∇": "")
      + (unit.IsWarWeary() ? "☹": "")
      + (unit.fooled ? "⊜" : "")
      + (FireBug.Aval(unit.rf.general) ? "♨" : "")
      + (Ambusher.Aval(unit.rf.general) ? "☸": "")
      + (Outlooker.Aval(unit.rf.general) ? "⦿" : "")
      + (Rally.Aval(unit.rf.general) ? "❥" : "")
      + (Holder.Aval(unit.rf.general) ? "☍" : "")
      + (Builder.Aval(unit.rf.general) ? "♜" : "")
      + (Fortifier.Aval(unit.rf.general) ? "㊎" : "")
      + (Herbist.Aval(unit.rf.general) ? "✚" : "")
      + (Striker.Aval(unit.rf.general) ? "➹" : "")
      + (Runner.Aval(unit.rf.general) ? "↹" : "")
      + (StaminaManager.Aval(unit.rf.general) ? "♋" : "")
      + (Hammer.Aval(unit.rf.general) ? "✪" : "")
      + (Finisher.Aval(unit.rf.general) ? "➲" : "")
      + (Sentinel.Aval(unit.rf.general) ? "▣" : "")
      + (Disruptor.Aval(unit.rf.general) || Agitator.Aval(unit.rf.general) ? "✴" : "")
      + (Conspirator.Aval(unit.rf.general) || MindReader.Aval(unit.rf.general)
        || Deciever.Aval(unit.rf.general) || GameChanger.Aval(unit.rf.general) ? "☯" : "")
      + (Breacher.Aval(unit.rf.general) ? "≜" : "");
      textMesh.fontSize = 65;
      textMesh.color = color;
    }

    // Update is called once per frame
    void Update () {
      transform.rotation = Camera.main.transform.rotation;
    }
  }

}