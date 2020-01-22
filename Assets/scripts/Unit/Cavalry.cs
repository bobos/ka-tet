using MapTileNS;
using CourtNS;

namespace  UnitNS
{
  public class Cavalry : Unit
  {
    public const int BaseSlots = Infantry.BaseSlots + 2;
    public const float MovementCostModifierOnHill = 2f;
    public const int MinTroopNum = 300;
    public const int MaxTroopNum = 3000;
    public const int ExhaustLine = 10;

    public static Unit Create(bool clone, Troop troop, Tile tile, int supply, State state = State.Stand, 
                    int kia = 0, int mia = 0, int movement = -1) {
      Unit unit = new Cavalry(clone, troop, tile, supply, state, kia, mia, movement);
      unit.Init();
      return unit;
    }

    private Cavalry(bool clone, Troop troop, Tile tile, int supply, State state = State.Stand,
                   int kia = 0, int mia = 0, int movement = -1):
          base(clone, troop, tile, state, supply, 0, kia, mia, movement)
    {
      this.type = Type.Cavalry;
    }

    public override bool IsCavalry()
    {
      return true;
    }

    protected override float GetMovementModifier()
    {
      return 1f;
    }

    protected override int GetBaseSupplySlots()
    {
      return BaseSlots;
    }

    protected override Unit Clone()
    {
      return Create(true, rf, tile, supply.supply,  state, kia, mia, movementRemaining);
    }

    protected override bool Concealable() {
      return false;
    }

  }
}