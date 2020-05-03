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

    public void SetName(Unit unit) {
      int totalDefendPoint = unit.unitCombatPoint;
      foreach(Tile tile in unit.tile.neighbours) {
        Unit u = tile.GetUnit();
        if (u != null && u.IsAI() == unit.IsAI()) {
          totalDefendPoint += u.unitCombatPoint;
        }
      }

      TextMesh textMesh = this.transform.GetComponent<TextMesh>();
      Color color = unit.hexMap.GetWarParty(unit).attackside ? Color.yellow : Color.white;
      string title = (unit.defeating ? "⤋⤋" : (unit.chaos ? "⤋⤋⤋": (unit.defeatStreak > 0 ? "⤋": "")))
        + "\n"
        + (unit.InCommanderRange() ? "[*]" : "")
        + (unit.alerted ? "◉" : "")
        + (unit.rf.general.Has(Cons.ambusher) ? "♘" : "")
        + (unit.rf.general.Has(Cons.tactic) ? "☯" : "")
        + (unit.rf.general.Has(Cons.fireBug) ? "♨" : "")
        + (unit.rf.general.Has(Cons.runner) ? "⇶" : "")
        + (unit.rf.general.Has(Cons.staminaManager) ? "☸" : "")
        + (unit.hexMap.wargameController.start ? "[推演]\n" : "")
        + unit.GeneralName()
        + (unit.IsAI() ? "" : (unit.IsCommander() ? "♛" : ""))
        + "[" + unit.rf.province.region.Name() + "]"
        + "\n";
      textMesh.text = title + unit.rf.province.Name() + "-"
        + unit.GetUnitName()
        + "[" + Shorten(unit.rf.soldiers) + "]\n"
        + "攻: " + Shorten(unit.unitCombatPoint) + "\n防: " + Shorten(totalDefendPoint);
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