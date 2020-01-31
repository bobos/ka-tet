using MapTileNS;
using CourtNS;

namespace UnitNS
{
  public class Scout : Unit
  {
    public const int MaxTroopNum = 200;

    public static Unit Create(bool clone, Troop troop, Tile tile, int supply, State state = State.Stand, 
                    int kia = 0, int mia = 0, int movement = -1) {
      Unit unit = new Scout(clone, troop, tile, supply, state, kia, mia, movement);
      unit.Init();
      if (clone) {
        unit.CloneInit(unit.disarmorDefDebuf, unit.GetNewGeneralBuf(), unit.epidemic, unit.unitPoisioned,
          unit.supply, unit.weatherEffect, unit.vantage, unit.plainSickness);
      }
      return unit;
    }

    private Scout(bool clone, Troop troop, Tile tile, int supply, State state = State.Stand,
                 int kia = 0, int mia = 0, int movement = -1):
          base(clone, troop, tile, state, supply, 0, kia, mia, movement)
    {
      this.type = Type.Scout;
    }

    public override bool IsCavalry()
    {
      return true;
    }

    protected override float GetMovementModifier()
    {
      return 1.4f;
    }

    protected override int GetBaseSupplySlots()
    {
      return 10;
    }

    protected override Unit Clone()
    {
      return Create(true, rf, tile, supply.supply, state, kia, mia, movementRemaining);
    }

    protected override bool Concealable() {
      return true;
    }

  }
}