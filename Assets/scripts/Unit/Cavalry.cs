using MapTileNS;
using CourtNS;

namespace  UnitNS
{
  public class Cavalry : Unit
  {
    public const int BaseSlots = Infantry.BaseSlots + 2;
    public const float MovementCostModifierOnHill = 1.5f;
    public const float MovementCostModifierOnPlainOrRoad = 0.7f;
    public const int MinTroopNum = 250;
    public const int MaxTroopNum = 2000;

    public static Unit Create(bool clone, Troop troop, Tile tile, int supply, State state = State.Stand, 
                    int kia = 0, int mia = 0, int movement = -1, float disarmorDefDebuf = 0f,
                    float newGenBuf = 0f, Supply theSupply = null, PlainSickness plainSickness = null) {
      Unit unit = new Cavalry(clone, troop, tile, supply, state, kia, mia, movement);
      unit.Init();
      if (clone) {
        unit.CloneInit(unit.disarmorDefDebuf, unit.GetNewGeneralBuf(), theSupply, plainSickness);
      }
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

    protected override int GetBaseSupplySlots()
    {
      return BaseSlots;
    }

    protected override Unit Clone()
    {
      return Create(true, rf, tile, supply.supply, state, kia, mia, movementRemaining,
        disarmorDefDebuf, GetNewGeneralBuf(), supply, plainSickness);
    }

    protected override bool Concealable() {
      return false;
    }

  }
}