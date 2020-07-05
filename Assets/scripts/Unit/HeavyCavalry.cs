using MapTileNS;
using CourtNS;

namespace  UnitNS
{
  public class HeavyCavalry : Unit
  {
    const float modifier = 1.4f;
    public const float ModifierOnHill = LightCavalry.ModifierOnHill * modifier;
    public const float ModifierOnPlainOrRoad = LightCavalry.ModifierOnPlainOrRoad * modifier;
    public const int MinTroopNum = 500;
    public const int MinExpForCombat = 50;

    public override float MovementCostModifierOnHill() {
      return ModifierOnHill;
    }

    public override float MovementCostModifierOnPlainOrRoad() {
      return ModifierOnPlainOrRoad;
    }

    public static Unit Create(bool clone, Troop troop, Tile tile, State state = State.Stand, 
                    int kia = 0, int movement = -1, float disarmorDefDebuf = 0f,
                    Supply supply = null, PlainSickness plainSickness = null) {
      Unit unit = new HeavyCavalry(clone, troop, tile, state, kia, movement);
      unit.Init();
      if (clone) {
        unit.CloneInit(unit.disarmorDefDebuf, supply, plainSickness);
      }
      return unit;
    }

    private HeavyCavalry(bool clone, Troop troop, Tile tile, State state = State.Stand,
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