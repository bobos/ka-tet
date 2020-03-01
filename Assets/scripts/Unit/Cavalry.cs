using MapTileNS;
using CourtNS;

namespace  UnitNS
{
  public class Cavalry : Unit
  {
    public const int BaseSlots = Infantry.BaseSlots + 2;
    public const float MovementCostModifierOnHill = 1.5f;
    public const float MovementCostModifierOnPlainOrRoad = 0.7f;
    public const int MinTroopNum = 300;
    public const int MaxTroopNum = 2500;

    public static Unit Create(bool clone, Troop troop, Tile tile, int supply, State state = State.Stand, 
                    int kia = 0, int mia = 0, int movement = -1, float disarmorDefDebuf = 0f,
                    Supply theSupply = null, PlainSickness plainSickness = null, WarWeary warWeary = null) {
      Unit unit = new Cavalry(clone, troop, tile, supply, state, kia, mia, movement);
      unit.Init();
      if (clone) {
        unit.CloneInit(unit.disarmorDefDebuf, theSupply, plainSickness, warWeary);
      }
      return unit;
    }

    private Cavalry(bool clone, Troop troop, Tile tile, int supply, State state = State.Stand,
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
      return BaseSlots;
    }

    protected override Unit Clone()
    {
      return Create(true, rf, tile, supply.supply, state, kia, mia, movementRemaining,
        disarmorDefDebuf, supply, plainSickness, warWeary);
    }

    protected override bool Concealable() {
      return false;
    }

  }
}