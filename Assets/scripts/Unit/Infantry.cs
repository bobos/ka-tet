using System.Collections.Generic;
using MapTileNS;
using CourtNS;

namespace  UnitNS
{
  public class Infantry : Unit
  {
    public const int BaseSlots = 5;
    public const int MaxTroopNum = 10000;
    public const int MinTroopNum = 1000;
    public const float MinLabor = 0.1f;
    public const float MaxLabor = 0.5f;

    public static Unit Create(bool clone, Troop troop, Tile tile, int supply, int labor, State state = State.Stand, 
                    int kia = 0, int mia = 0, int movement = -1, float disarmorDefDebuf = 0f,
                    Supply theSupply = null, PlainSickness plainSickness = null, WarWeary warWeary = null) {
      Unit unit = new Infantry(clone, troop, tile, supply, labor, state, kia, mia, movement);
      unit.Init();
      if (clone) {
        unit.CloneInit(disarmorDefDebuf, theSupply, unit.plainSickness, warWeary);
      }
      return unit;
    }

    protected Infantry(bool clone, Troop troop, Tile tile, int supply, int labor, State state = State.Stand, 
                    int kia = 0, int mia = 0, int movement = -1):
          base(clone, troop, tile, state, supply, labor, kia, mia, movement)
    {
    }

    public override bool IsCavalry()
    {
      return false;
    }

    int MaxLabors() {
      return (int)((rf.soldiers + rf.wounded) * MaxLabor);
    }

    public override int LaborCanTakeIn() {
      int theLabor = labor;
      int force = MaxLabors();
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
      int num = rf.soldiers + rf.wounded;
      if (labor < (int)(num * MinLabor)) {
        return 0;
      }
      if (labor >= MaxLabors()) {
        return BaseSlots + 2;
      }
      return BaseSlots; 
    }

    protected override Unit Clone()
    {
      return Create(true, rf, tile, supply.supply, labor, state, kia, mia, movementRemaining,
        disarmorDefDebuf, supply, plainSickness, warWeary);
    }

    protected override bool Concealable() {
      return false;
    }


  }
}