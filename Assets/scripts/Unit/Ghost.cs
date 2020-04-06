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
      General gen = new General("x", "x", Cons.heBei, new CommandSkill(1), new TroopSize(1));
      gen.JoinFaction(Cons.GF, Cons.NoParty);

      return new GhostUnit(new Troop(3800, Cons.heBei, Type.Infantry, Cons.veteran, gen));
    }
  }
}