using CourtNS;

namespace UnitNS
{
  public class GhostUnit : Infantry
  {
    public GhostUnit(Troop troop) : base(false, troop, null, 0, 0) { }
    public void SetPlayer(bool isPlayer)
    {
      rf.faction.SetAs(!isPlayer);
    }

    public static GhostUnit createGhostUnit() {
      Troop troop = new Troop(3800, Cons.GF, Cons.middleEarth, Type.Infantry, Cons.elite);
      return new GhostUnit(troop);
    }
  }
}