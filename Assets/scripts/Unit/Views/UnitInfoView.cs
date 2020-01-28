using CourtNS;
using UnityEngine;

namespace UnitNS
{
  public class UnitInfoView : MonoBehaviour {
  
    // Use this for initialization
    void Start () {
    }
  
    public void SetName(Troop troop) {
      TextMesh textMesh = this.transform.GetComponent<TextMesh>();
      textMesh.text = troop.rank.Name() + "[" + troop.soldiers + "]";
      textMesh.fontSize = 60;
      textMesh.color = Color.white;
      if (troop.rank.Level() == 2) {
        textMesh.color = Color.green;
      }
      if (troop.rank.Level() == 3) {
        textMesh.color = Color.magenta;
      }
      if (troop.rank.Level() == -1) {
        textMesh.color = Color.yellow;
      }
    }
    
    // Update is called once per frame
    void Update () {
      transform.rotation = Camera.main.transform.rotation;
    }
  }

}