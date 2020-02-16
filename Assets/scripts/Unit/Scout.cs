using MapTileNS;
using CourtNS;

namespace UnitNS
{
  public class Scout : Unit
  {
    public const int MaxTroopNum = 250;

    public static Unit Create(bool clone, Troop troop, Tile tile, int supply, State state = State.Stand, 
                    int kia = 0, int mia = 0, int movement = -1, float disarmorDefDebuf = 0f,
                    float newGenBuf = 0f, Supply theSupply = null, PlainSickness plainSickness = null, WarWeary warWeary = null) {
      Unit unit = new Scout(clone, troop, tile, supply, state, kia, mia, movement);
      unit.Init();
      if (clone) {
        unit.CloneInit(disarmorDefDebuf, newGenBuf, theSupply, plainSickness, warWeary);
      }
      return unit;
    }

    private Scout(bool clone, Troop troop, Tile tile, int supply, State state = State.Stand,
                 int kia = 0, int mia = 0, int movement = -1):
          base(clone, troop, tile, state, supply, 0, kia, mia, movement)
    {
    }

    public override bool IsCavalry()
    {
      return true;
    }

    protected override int GetBaseSupplySlots()
    {
      return 10;
    }

    protected override Unit Clone()
    {
      return Create(true, rf, tile, supply.supply, state, kia, mia, movementRemaining,
        disarmorDefDebuf, GetNewGeneralBuf(), supply, plainSickness, warWeary);
    }

    protected override bool Concealable() {
      return true;
    }

  }
}