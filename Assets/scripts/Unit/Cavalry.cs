using MapTileNS;
using CourtNS;

namespace  UnitNS
{
  public class Cavalry : Unit
  {
    public const int BaseSlots = Infantry.BaseSlots + 2;
    public const float Under500MovementModifier = 1.8f;
    public const float DefaultMovementModifier = 1.4f;
    public const float MovementCostModifierOnHill = 1.2f;
    public const float MovementCostModifierOnPlain = 0.8f;
    public const int MinTroopNum = 200;
    public const int MaxTroopNum = 2500;
    public const int ExhaustLine = 10;
    public const float L1FireBuff = 0.25f;
    public const float L2FireBuff = 0.35f;
    public const float L3FireBuff = 0.5f;
    public const float L4FireBuff = 0.6f;
    public const float L1ForestBuff = 0.1f;
    public const float L2ForestBuff = 0.2f;
    public const float L3ForestBuff = 0.25f;
    public const float L4ForestBuff = 0.3f;

    public Cavalry(bool clone, Troop troop, Tile tile, int supply, State state = State.Stand,
                   int kia = 0, int mia = 0, int movement = -1):
          base(clone, troop, tile, state, supply, 0, kia, mia, movement)
    {}

    public override bool IsCavalry()
    {
      return true;
    }

    protected override float GetMovementModifier()
    {
      if (Concealable())
      {
        return Under500MovementModifier;
      } else
      {
        return DefaultMovementModifier;
      }
    }

    protected override int GetBaseSupplySlots()
    {
      return BaseSlots + (Concealable() ? 3 : 0);
    }

    protected override Unit Clone()
    {
      return new Cavalry(true, rf, tile, supply.supply,  state, kia, mia, movementRemaining);
    }

    protected override bool Concealable() {
      return rf.soldiers + rf.wounded <= 500;
    }

  }
}