using FieldNS;

namespace UnitNS
{
  public class OnFieldComplain
  {
    Unit unit;
    bool fired = false;
    public OnFieldComplain(Unit unit) {
      this.unit = unit;
    }

    public int Occur() {
      WarParty myWp = unit.IsAI() ? unit.hexMap.GetAIParty() : unit.hexMap.GetPlayerParty();
      WarParty enWp = unit.IsAI() ? unit.hexMap.GetPlayerParty() : unit.hexMap.GetAIParty();
      bool odds = (int)(enWp.GetTotalPoint() / myWp.GetTotalPoint()) >= 2;
      if (!unit.hexMap.IsAttackSide(unit.IsAI()) && !fired && Cons.IsHan(unit.rf.province.region)
        && odds && unit.IsOnField() && !unit.IsCavalry() && !unit.ApplyDiscipline()) {
        fired = true;
        return MoraleDrop();
      }
      fired = true;
      return 0;
    }

    int MoraleDrop() {
      return -25;
    } 

    public void Destroy() {}

  }
}