using CourtNS;
using UnityEngine;

namespace UnitNS
{
  public class UnitInfoView : MonoBehaviour {
  
    // Use this for initialization
    void Start () {
    }
  
    public void SetName(Unit unit) {
      TextMesh textMesh = this.transform.GetComponent<TextMesh>();
      Color color = unit.hexMap.GetWarParty(unit).attackside ? Color.yellow : Color.white;
      string title = (unit.chaos ? "☠" : "")
        + "\n"
        + (unit.IsCavalry() ? (unit.type == Type.Scout ? "♘" : "♞") : "♜")
        + (unit.hexMap.wargameController.start ? "[推演]\n" : "")
        + unit.GeneralName()
        + "[" + unit.rf.general.party.Name() + "]"
        + "\n";
      textMesh.text = title + unit.rf.province.Name() + "-" + unit.rf.rank.Name() + "[" + unit.rf.soldiers + "]";
      textMesh.fontSize = 50;
      textMesh.color = color;
    }

    public void SetStr(string str, Color color) {
      TextMesh textMesh = this.transform.GetComponent<TextMesh>();
      textMesh.text = str;
      textMesh.fontSize = 50;
      textMesh.color = color;
    }
    
    // Update is called once per frame
    void Update () {
      transform.rotation = Camera.main.transform.rotation;
    }
  }

}