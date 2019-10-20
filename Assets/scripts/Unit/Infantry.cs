using System.Collections.Generic;
using MapTileNS;
using CourtNS;

namespace  UnitNS
{
  public class Infantry : Unit
  {
    public const int BaseSlots = 3;
    public const float Under1K5MovementModifier = 1.2f;
    public const float Under4KMovementModifier = 1f;
    public const float MoreThan4KMovementModifier = 0.8f;
    public const int MinTroopNum = 500;
    public const int MaxTroopNum = 10000;
    public const int ExhaustLine = 15;
    public const float L1FireBuff = 0.2f;
    public const float L2FireBuff = 0.25f;
    public const float L3FireBuff = 0.3f;
    public const float L4FireBuff = 0.35f;
    public const float L1ForestBuff = 0.2f;
    public const float L2ForestBuff = 0.3f;
    public const float L3ForestBuff = 0.4f;
    public const float L4ForestBuff = 0.5f;

    public Infantry(bool clone, Troop troop, Tile tile, int supply, int labor, State state = State.Stand, 
                    int kia = 0, int mia = 0, int movement = -1):
          base(clone, troop, tile, state, supply, labor, kia, mia, movement)
    {}

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

    protected override float GetMovementModifier()
    {
      int num = rf.soldiers + rf.wounded + labor;
      if (num <= 1500)
      {
        return Under1K5MovementModifier;
      }
      else if (num <= 4000)
      {
        return Under4KMovementModifier;
      }
      else
      {
        return MoreThan4KMovementModifier;
      }
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
      return new Infantry(true, rf, tile, supply, labor, state, kia, mia, movementRemaining);
    }

    protected override bool Concealable() {
      return labor == 0 && rf.soldiers + rf.wounded <= 800;
    }


  }
}