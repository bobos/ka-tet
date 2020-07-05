using System.Collections.Generic;
using MapTileNS;
using CourtNS;

namespace  UnitNS
{
  public class Infantry : Unit
  {
    public const int MinTroopNum = 1000;
    public const int MinExpForCombat = 10;

    public static Unit Create(bool clone, Troop troop, Tile tile, State state = State.Stand, 
                    int kia = 0, int movement = -1, float disarmorDefDebuf = 0f,
                    Supply theSupply = null, PlainSickness plainSickness = null) {
      Unit unit = new Infantry(clone, troop, tile, state, kia, movement);
      unit.Init();
      if (clone) {
        unit.CloneInit(disarmorDefDebuf, theSupply, unit.plainSickness);
      }
      return unit;
    }

    protected Infantry(bool clone, Troop troop, Tile tile, State state = State.Stand, 
                    int kia = 0, int movement = -1):
          base(clone, troop, tile, state, kia, movement)
    {
    }

    protected override Unit Clone()
    {
      return Create(true, rf, tile, state, kia, movementRemaining, disarmorDefDebuf, supply, plainSickness);
    }

  }
}