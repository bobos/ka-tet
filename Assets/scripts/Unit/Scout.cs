using MapTileNS;
using CourtNS;

namespace UnitNS
{
  public class Scout : Unit
  {
    public const int MinTroopNum = 50;
    public const int MaxTroopNum = 250;

    public Scout(bool clone, Troop troop, Tile tile, int supply, State state = State.Stand,
                 int kia = 0, int mia = 0, int movement = -1):
          base(clone, troop, tile, state, supply, 0, kia, mia, movement)
    {
      this.type = Type.Scout;
    }

    public override bool IsCavalry()
    {
      return true;
    }

    protected override float GetMovementModifier()
    {
      return 2.0f;
    }

    protected override int GetBaseSupplySlots()
    {
      return 10;
    }

    protected override Unit Clone()
    {
      return new Scout(true, rf, tile, supply.supply, state, kia, mia, movementRemaining);
    }

    protected override bool Concealable() {
      return true;
    }

  }
}