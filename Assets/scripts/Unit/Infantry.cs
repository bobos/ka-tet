using System.Collections.Generic;
using MapTileNS;
using CourtNS;

namespace  UnitNS
{
  public class Infantry : Unit
  {
    public const int BaseSlots = 3;
    public const int MaxTroopNum = 15000;

    public static Unit Create(bool clone, Troop troop, Tile tile, int supply, int labor, State state = State.Stand, 
                    int kia = 0, int mia = 0, int movement = -1, float disarmorDefDebuf = 0f,
                    float newGenBuf = 0f, Supply theSupply = null, PlainSickness plainSickness = null) {
      Unit unit = new Infantry(clone, troop, tile, supply, labor, state, kia, mia, movement);
      unit.Init();
      if (clone) {
        unit.CloneInit(disarmorDefDebuf, newGenBuf, theSupply, unit.plainSickness);
      }
      return unit;
    }

    protected Infantry(bool clone, Troop troop, Tile tile, int supply, int labor, State state = State.Stand, 
                    int kia = 0, int mia = 0, int movement = -1):
          base(clone, troop, tile, state, supply, labor, kia, mia, movement)
    {
      this.type = Type.Infantry;
    }

    public override bool IsCavalry()
    {
      return false;
    }

    public override int LaborCanTakeIn() {
      int theLabor = labor;
      int force = rf.soldiers + rf.wounded;
      if (theLabor >= force) return 0; 
      return force - theLabor;
    }

    public override Dictionary<int, int> GetLaborSuggestion() {
      Dictionary<int, int> suggestions = new Dictionary<int, int>();
      int labors = labor;
      float[] threshold = new float[3];
      threshold[0] = 0.25f;
      threshold[1] = 0.5f;
      threshold[2] = 0.75f;
      int incr = 0;
      foreach (float item in threshold)
      {
        incr++;
        int tmp = (int)(rf.soldiers * item);
        if (labors < tmp) suggestions.Add(incr, tmp - labors);
      }
      return suggestions;
    }

    protected override int GetBaseSupplySlots()
    {
      int labors = labor;
      if (labors < (int)(rf.soldiers * 0.25f)) {
        return BaseSlots;
      }
      if (labors < (int)(rf.soldiers * 0.5f)) {
        return BaseSlots + 1;
      }
      else
      {
        return BaseSlots + 2; 
      }
    }

    protected override Unit Clone()
    {
      return Create(true, rf, tile, supply.supply, labor, state, kia, mia, movementRemaining,
        disarmorDefDebuf, GetNewGeneralBuf(), supply, plainSickness);
    }

    protected override bool Concealable() {
      return false;
    }


  }
}