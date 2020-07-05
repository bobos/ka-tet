using MapTileNS;
using CourtNS;

namespace  UnitNS
{
  public class LightCavalry : Unit
  {
    public const float ModifierOnHill = 0.8f; // 20 movement points, 5 blocks
    public const float ModifierOnPlainOrRoad = 0.64f; // 16 movement points, 6 blocks
    public const int MinTroopNum = 500;
    public const int MinExpForCombat = 20;

    public override float MovementCostModifierOnHill() {
      return ModifierOnHill;
    }

    public override float MovementCostModifierOnPlainOrRoad() {
      return ModifierOnPlainOrRoad;
    }

    public static Unit Create(bool clone, Troop troop, Tile tile, State state = State.Stand, 
                    int kia = 0, int movement = -1, float disarmorDefDebuf = 0f,
                    Supply supply = null, PlainSickness plainSickness = null) {
      Unit unit = new LightCavalry(clone, troop, tile, state, kia, movement);
      unit.Init();
      if (clone) {
        unit.CloneInit(unit.disarmorDefDebuf, supply, plainSickness);
      }
      return unit;
    }

    private LightCavalry(bool clone, Troop troop, Tile tile, State state = State.Stand,
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