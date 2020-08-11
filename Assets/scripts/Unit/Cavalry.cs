using MapTileNS;
using CourtNS;

namespace  UnitNS
{
  public class Cavalry : Unit
  {
    public const float ModifierOnHill = 1f; // 4 blocks
    public const float ModifierOnPlainOrRoad = 0.8f; // 5 blocks
    public const int MaxTroopNum = 3000;

    public override float MovementCostModifierOnHill() {
      return ModifierOnHill;
    }

    public override float MovementCostModifierOnPlainOrRoad() {
      return ModifierOnPlainOrRoad;
    }

    public override bool IsCavalry() {
      return true;
    }

    public static Unit Create(bool clone, Troop troop, Tile tile, State state = State.Stand, 
                    int kia = 0, int movement = -1, float disarmorDefDebuf = 0f,
                    Supply supply = null, PlainSickness plainSickness = null) {
      Unit unit = new Cavalry(clone, troop, tile, state, kia, movement);
      unit.Init();
      if (clone) {
        unit.CloneInit(unit.disarmorDefDebuf, supply, plainSickness);
      }
      return unit;
    }

    private Cavalry(bool clone, Troop troop, Tile tile, State state = State.Stand,
                   int kia = 0, int movement = -1):
          base(clone, troop, tile, state, kia, movement)
    {
    }

    protected override Unit Clone()
    {
      return Create(true, rf, tile, state, kia, movementRemaining,
        disarmorDefDebuf, supply, plainSickness);
    }

  }
}