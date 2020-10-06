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
      + FireBug.Icon(unit)
      + Ambusher.Icon(unit)
      + Outlooker.Icon(unit)
      + Rally.Icon(unit)
      + Holder.Icon(unit)
      + Builder.Icon(unit)
      + Fortifier.Icon(unit)
      + Herbist.Icon(unit)
      + Boxer.Icon(unit)
      + Runner.Icon(unit)
      + StaminaManager.Icon(unit)
      + Pusher.Icon(unit)
      + Finisher.Icon(unit)
      + Sentinel.Icon(unit)
      + Disruptor.Icon(unit)
      + Agitator.Icon(unit)
      + Freezer.Icon(unit)
      + FearMonger.Icon(unit)
      + Deciever.Icon(unit)
      + GameChanger.Icon(unit)
      + Breacher.Icon(unit)
      + Evader.Icon(unit);
      textMesh.fontSize = 65;
      textMesh.color = color;
    }

    // Update is called once per frame
    void Update () {
      transform.rotation = Camera.main.transform.rotation;
    }
  }

}