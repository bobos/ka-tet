using CourtNS;
using MapTileNS;
using UnityEngine;

namespace UnitNS
{
  public class UnitInfoView : MonoBehaviour {
  
    // Use this for initialization
    void Start () {
    }
  
    public static string Shorten(int num) {
      int numUnit = 1000;
      if (num < numUnit) {
        return (int)(num / 100) + Cons.GetTextLib().get("misc_hundred");
      }

      string ret = "";
      int remaining = num % numUnit;
      int h = (int)(remaining / 100);
      int t = (num - remaining) / numUnit;
      if (t >= 10) {
        int ht = t;
        t = t % 10;
        ht = (ht - t) / 10;
        ret = ht + Cons.GetTextLib().get("misc_hundredThousand");
      }

      if(t > 0) {
        ret += t + Cons.GetTextLib().get("misc_thousand");
      }

      if (h > 0) {
        ret += h + Cons.GetTextLib().get("misc_hundred");
      }
      return ret;
    }

    public static string NumIcons(int num, bool isCalvary) {
      string icon = isCalvary ? "♞" : "♟";
      int n = num % 2000;
      int u = (int)((num - n) / 2000);
      string ret = "";
      for (int i = 0; i < u; i++)
      {
        ret += icon;
      }
      if (n >= 1500) {
        ret += icon;
      } else if (n > Unit.DisbandUnitUnder) {
        ret += (isCalvary ? "♘" : "♙");
      }
      return ret;
    }

    public void SetName(Unit unit) {
      int totalDefendPoint = unit.unitCombatPoint;
      foreach(Tile tile in unit.tile.neighbours) {
        Unit u = tile.GetUnit();
        if (u != null && u.IsAI() == unit.IsAI()) {
          totalDefendPoint += u.unitCombatPoint;
        }
      }

      TextMesh textMesh = this.transform.GetComponent<TextMesh>();
      Color color = Color.white;
      textMesh.text =
        (unit.hexMap.wargameController.start ? "[演]" : "")
        + unit.GeneralName()
        + "(" + unit.rf.general.trait.Name() + ")"
        + (unit.IsAI() ? "" : (unit.IsCommander() ? "♛" : ""))
        + "[" + unit.rf.province.region.Name() + "]"
        + "("+unit.allowedAtmpt+")"
        + "\n"
        + unit.rf.province.Name() + NumIcons(unit.rf.soldiers, unit.type != Type.Infantry) + "\n"
        + "攻: " + Shorten(unit.unitCombatPoint) + "\n防: " + Shorten(totalDefendPoint) + "\n"
        + unit.rf.morale + "/" + unit.rf.province.region.MoralePunishLine() + "/" + unit.rf.province.region.RetreatThreshold();
      textMesh.fontSize = 45;
      textMesh.color = color;
    }

    public void SetStr(string str, Color color) {
      TextMesh textMesh = this.transform.GetComponent<TextMesh>();
      textMesh.text = str;
      textMesh.fontSize = 45;
      textMesh.color = color;
    }
    
    // Update is called once per frame
    void Update () {
      transform.rotation = Camera.main.transform.rotation;
    }
  }

}