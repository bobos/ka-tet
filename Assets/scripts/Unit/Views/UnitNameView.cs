using CourtNS;
using FieldNS;
using MapTileNS;
using UnityEngine;

namespace UnitNS
{
  public class UnitNameView : MonoBehaviour {
  
    // Use this for initialization
    void Start () {
    }
  
    public Settlement settlement;
    public void UpdateName() {
      Color color = settlement.owner.attackside ? Color.yellow : Color.white;
      string factionName = settlement.owner.faction.Name();
      string name = settlement.name + "[" + factionName + "]";

      int defendForce = 0;
      if (!settlement.IsEmpty()) {
        bool isAI = settlement.garrison[0].IsAI();
        defendForce = settlement.GetDefendForce();
        foreach(Tile tile in settlement.baseTile.neighbours) {
          Unit unit = tile.GetUnit();
          if (unit != null && unit.IsAI() == isAI) {
            defendForce += unit.GetUnitDefendCombatPoint(false);
          }
        }
      }

      name += FoW.Get().IsFogged(settlement.baseTile) ? "??" : "\n♟" + UnitInfoView.Shorten(defendForce); 
      TextMesh textMesh = this.transform.GetComponent<TextMesh>();
      textMesh.text = name;
      textMesh.color = color;
      textMesh.fontSize = 60;
      transform.rotation = Camera.main.transform.rotation;
    }
    
    // Update is called once per frame
    void Update () {
      UpdateName();
      transform.rotation = Camera.main.transform.rotation;
    }
  }

}