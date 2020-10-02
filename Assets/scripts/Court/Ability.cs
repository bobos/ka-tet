
using System.Collections.Generic;

namespace CourtNS {
  public enum AbilityType {
    Common,
    Infantry,
    Cavalry,
    Advanced
  }

  public abstract class Ability {
    public string Name() {
      return Cons.GetTextLib().get(name);
    }

    public string Description() {
      return Cons.GetTextLib().get(description);
    }

    public readonly AbilityType type;
    public readonly bool isPassive;
    public readonly int attempts = 0;
    public readonly string name;
    public readonly string description;
    int remaining = 0;

    protected Ability(string name, string description,
      AbilityType type, int attempts = 0) {
      this.type = type;
      this.isPassive = attempts == 0;
      this.remaining = this.attempts = attempts;
      this.name = name;
      this.description = description;
    }

    public bool Consume() {
      if (isPassive) {
        return true;
      }

      if (remaining == 0) {
        return false;
      }

      remaining--;
      return true;
    }

    public void Init() {
      this.remaining = this.attempts;
    }

    protected static bool Find(string name, General general) {
      foreach(Ability ability in general.acquiredAbilities) {
        if(ability.name == name) {
          return true;
        } 
      }
      return false;
    }

    protected static bool Aval(AbilityType type, General general) {
      if (type == AbilityType.Cavalry && !general.commandUnit.onFieldUnit.IsCavalry() ||
      type == AbilityType.Infantry && general.commandUnit.onFieldUnit.IsCavalry()) {
        return false;
      }
      return true;
    }
  }

  class AbilityControl {
    Dictionary<Faction, int> quotaMap = new Dictionary<Faction, int>();
    readonly int quota;
    readonly int requiredPoints;
    public AbilityControl(int quota, int requiredPoints) {
      this.quota = quota;
      this.requiredPoints = requiredPoints;
    }

    public void Unlock(Faction faction) {
      quotaMap[faction] = quota;
    }

    public bool Acquire(General general) {
      Faction faction = general.faction;
      if (!Find(faction) || quotaMap[faction] == 0 || general.militatyPoints < requiredPoints) {
        return false;
      }

      if (quotaMap[faction] > 0) {
        quotaMap[faction]--;
      }
      general.militatyPoints -= requiredPoints;
      return true;
    }

    public bool Find(Faction faction) {
      return quotaMap.ContainsKey(faction);
    }

  }

  public class DrillMaster: Ability {
    public const int requiredPoint = 2;
    static AbilityControl ac = new AbilityControl(-1, requiredPoint);
    const string N = "ability_drillMaster";
    const string D = "ability_drillMaster_description";
    public DrillMaster(): base(N, D, AbilityType.Common) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
  }

  public class FireBug: Ability {
    public const int requiredPoint = 5;
    static AbilityControl ac = new AbilityControl(-1, requiredPoint);
    const string N = "ability_fireBug";
    const string D = "ability_fireBug_description";
    const AbilityType T = AbilityType.Common;
    public FireBug(): base(N, D, T, 3) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
  }

  public class Ambusher: Ability {
    public const int requiredPoint = 6;
    static AbilityControl ac = new AbilityControl(-1, requiredPoint);
    const string N = "ability_ambusher";
    const string D = "ability_ambusher_description";
    const AbilityType T = AbilityType.Common;
    public Ambusher(): base(N, D, T) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
    public static int AmbushRange = 4;
    public static int SupplyPunishment = 8;
    public static int ExtraChanceForMistAmbush = 20;
  }

  public class Striker: Ability {
    public const int requiredPoint = 7;
    static AbilityControl ac = new AbilityControl(2, requiredPoint);
    const string N = "ability_striker";
    const string D = "ability_striker_description";
    const AbilityType T = AbilityType.Common;
    public Striker(): base(N, D, T, 1) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
    public static float AtkBuf = 1f;
  }

  public class Outlooker: Ability {
    public const int requiredPoint = 7;
    static AbilityControl ac = new AbilityControl(-1, requiredPoint);
    const string N = "ability_outlooker";
    const string D = "ability_outlooker_description";
    const AbilityType T = AbilityType.Common;
    public Outlooker(): base(N, D, T) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
  }

  public class Generous: Ability {
    public const int requiredPoint = 9;
    static AbilityControl ac = new AbilityControl(2, requiredPoint);
    const string N = "ability_generous";
    const string D = "ability_generous_description";
    const AbilityType T = AbilityType.Common;
    public Generous(): base(N, D, T, 1) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
    public static int Range = 2;
    public static int MoraleBuf = 50;
    public static int MoveBuf = 40;
  }

}