using MapTileNS;
using CourtNS;

namespace  UnitNS
{
  public class Cavalry : Unit
  {
    public const float MovementCostModifierOnHill = 0.7f;
    public const float MovementCostModifierOnPlainOrRoad = 0.5f;
    public const int MinTroopNum = 500;

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

    public override bool IsCavalry()
    {
      return true;
    }

    protected override Unit Clone()
    {
      return Create(true, rf, tile, state, kia, movementRemaining,
        disarmorDefDebuf, supply, plainSickness);
    }

  }
}