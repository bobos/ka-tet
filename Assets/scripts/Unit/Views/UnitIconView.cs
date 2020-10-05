using CourtNS;
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
      + FireBug.Icon(unit.rf.general)
      + Ambusher.Icon(unit.rf.general)
      + Outlooker.Icon(unit.rf.general)
      + Rally.Icon(unit.rf.general)
      + Holder.Icon(unit.rf.general)
      + Builder.Icon(unit.rf.general)
      + Fortifier.Icon(unit.rf.general)
      + Herbist.Icon(unit.rf.general)
      + Striker.Icon(unit.rf.general)
      + Runner.Icon(unit.rf.general)
      + StaminaManager.Icon(unit.rf.general)
      + Hammer.Icon(unit.rf.general)
      + Finisher.Icon(unit.rf.general)
      + Sentinel.Icon(unit.rf.general)
      + Disruptor.Icon(unit.rf.general)
      + Agitator.Icon(unit.rf.general)
      + Conspirator.Icon(unit.rf.general)
      + MindReader.Icon(unit.rf.general)
      + Deciever.Icon(unit.rf.general)
      + GameChanger.Icon(unit.rf.general)
      + Breacher.Icon(unit.rf.general);
      textMesh.fontSize = 65;
      textMesh.color = color;
    }

    // Update is called once per frame
    void Update () {
      transform.rotation = Camera.main.transform.rotation;
    }
  }

}