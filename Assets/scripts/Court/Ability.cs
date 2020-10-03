
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
    public readonly int attempts = 0;
    public readonly string name;
    public readonly string description;
    int remaining = 0;

    protected Ability(string name, string description,
      AbilityType type, int attempts = -1) {
      // attempts indicates how many times this ability can be used on battle field, -1 means no limit
      this.type = type;
      this.remaining = this.attempts = attempts;
      this.name = name;
      this.description = description;
    }

    public bool Consume() {
      if (remaining == -1) {
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

    // find out if given general has the ability
    protected static bool Find(string name, General general) {
      foreach(Ability ability in general.acquiredAbilities) {
        if(ability.name == name) {
          return true;
        } 
      }
      return false;
    }

    // find out if the ability is available for given general, cavalry abililties are not availabe 
    // if general is commanding an infantry unit, vice-versa
    protected static bool Aval(AbilityType type, General general) {
      if (type == AbilityType.Cavalry && !general.commandUnit.onFieldUnit.IsCavalry() ||
      type == AbilityType.Infantry && general.commandUnit.onFieldUnit.IsCavalry()) {
        return false;
      }
      return true;
    }
  }

  // This class is used when a faction unlocks an ability or a general acquires an ability
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
    public const bool Passive = true;
    public const int requiredPoint = 2;
    static AbilityControl ac = new AbilityControl(-1, requiredPoint);
    const string N = "ability_drillMaster";
    const string D = "ability_drillMaster_description";
    const AbilityType T = AbilityType.Common;
    public DrillMaster(): base(N, D, T) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
  }

  public class FireBug: Ability {
    public const bool Passive = false;
    public const int requiredPoint = 5;
    static AbilityControl ac = new AbilityControl(-1, requiredPoint);
    const string N = "ability_fireBug";
    const string D = "ability_fireBug_description";
    const AbilityType T = AbilityType.Common;
    public FireBug(): base(N, D, T, 2) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
  }

  public class Ambusher: Ability {
    public const bool Passive = true;
    public const int requiredPoint = 6;
    static AbilityControl ac = new AbilityControl(4, requiredPoint);
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
    public const bool Passive = false;
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
    public const bool Passive = true;
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

  public class Rally: Ability {
    public const bool Passive = false;
    public const int requiredPoint = 8;
    static AbilityControl ac = new AbilityControl(2, requiredPoint);
    const string N = "ability_rally";
    const string D = "ability_rally_description";
    const AbilityType T = AbilityType.Common;
    public Rally(): base(N, D, T, 1) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
    public static int Range = 2;
    public static int MoraleBuf = 50;
    public static int MoveBuf = 40;
  }

  public class Holder: Ability {
    public const bool Passive = true;
    public const int requiredPoint = 3;
    static AbilityControl ac = new AbilityControl(-1, requiredPoint);
    const string N = "ability_holder";
    const string D = "ability_holder_description";
    const AbilityType T = AbilityType.Infantry;
    public Holder(): base(N, D, T, 3) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
    public static int ChanceToHold = 50;
  }

  public class Builder: Ability {
    public const bool Passive = true;
    public const int requiredPoint = 5;
    static AbilityControl ac = new AbilityControl(5, requiredPoint);
    const string N = "ability_builder";
    const string D = "ability_builder_description";
    const AbilityType T = AbilityType.Infantry;
    public Builder(): base(N, D, T) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
    public static float BuildEfficiencyIncr = 1f;
  }

  public class Fortifier: Ability {
    public const bool Passive = true;
    public const int requiredPoint = 4;
    static AbilityControl ac = new AbilityControl(-1, requiredPoint);
    const string N = "ability_fortifier";
    const string D = "ability_fortifier_description";
    const AbilityType T = AbilityType.Infantry;
    public Fortifier(): base(N, D, T) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
    public static float DefenceIncr = 0.5f;
  }

  public class Herbist: Ability {
    public const bool Passive = false;
    public const int requiredPoint = 5;
    static AbilityControl ac = new AbilityControl(-1, requiredPoint);
    const string N = "ability_herbist";
    const string D = "ability_herbist_description";
    const AbilityType T = AbilityType.Infantry;
    public Herbist(): base(N, D, T, 2) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
    public static int CureRange = 1;
  }

  public class Breacher: Ability {
    public const bool Passive = true;
    public const int requiredPoint = 6;
    static AbilityControl ac = new AbilityControl(4, requiredPoint);
    const string N = "ability_breacher";
    const string D = "ability_breacher_description";
    const AbilityType T = AbilityType.Infantry;
    public Breacher(): base(N, D, T, 2) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
    public static float AtkBuf = 0.8f;
  }

  public class Runner: Ability {
    public const bool Passive = false;
    public const int requiredPoint = 5;
    static AbilityControl ac = new AbilityControl(4, requiredPoint);
    const string N = "ability_runner";
    const string D = "ability_runner_description";
    const AbilityType T = AbilityType.Cavalry;
    public Runner(): base(N, D, T, 3) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
    public static float MoveBuf = 0.8f;
  }

  public class StaminaManager: Ability {
    public const bool Passive = true;
    public const int requiredPoint = 6;
    static AbilityControl ac = new AbilityControl(3, requiredPoint);
    const string N = "ability_staminaManager";
    const string D = "ability_staminaManager_description";
    const AbilityType T = AbilityType.Cavalry;
    public StaminaManager(): base(N, D, T) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
  }

  public class Hammer: Ability {
    public const bool Passive = true;
    public const int requiredPoint = 7;
    static AbilityControl ac = new AbilityControl(2, requiredPoint);
    const string N = "ability_hammer";
    const string D = "ability_hammer_description";
    const AbilityType T = AbilityType.Cavalry;
    public Hammer(): base(N, D, T) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
  }

  public class Finisher: Ability {
    public const bool Passive = true;
    public const int requiredPoint = 8;
    static AbilityControl ac = new AbilityControl(3, requiredPoint);
    const string N = "ability_finisher";
    const string D = "ability_finisher_description";
    const AbilityType T = AbilityType.Cavalry;
    public Finisher(): base(N, D, T) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
    public static float KillBuf = 1f;
  }

  public class Trapper: Ability {
    public const bool Passive = true;
    public const int requiredPoint = 6;
    static AbilityControl ac = new AbilityControl(4, requiredPoint);
    const string N = "ability_trapper";
    const string D = "ability_trapper_description";
    const AbilityType T = AbilityType.Advanced;
    public Trapper(): base(N, D, T) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
    public static int RedzoneRange = 2;
  }

  public class Consiprator: Ability {
    public const bool Passive = false;
    public const int requiredPoint = 6;
    static AbilityControl ac = new AbilityControl(3, requiredPoint);
    const string N = "ability_consiprator";
    const string D = "ability_consiprator_description";
    const AbilityType T = AbilityType.Advanced;
    public Consiprator(): base(N, D, T, 2) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
    public const int MoraleDropForDiffProvinceMin = 20;
    public const int MoraleDropForDiffProvinceMax = 30;
    public const int MoraleDropForDiffRaceMin = 30;
    public const int MoraleDropForDiffRaceMax = 40;
  }

  public class Deciever: Ability {
    public const bool Passive = false;
    public const int requiredPoint = 7;
    static AbilityControl ac = new AbilityControl(2, requiredPoint);
    const string N = "ability_deciever";
    const string D = "ability_deciever_description";
    const AbilityType T = AbilityType.Advanced;
    public Deciever(): base(N, D, T, 2) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
  }

  public class Tactician: Ability {
    public const bool Passive = false;
    public const int requiredPoint = 8;
    static AbilityControl ac = new AbilityControl(2, requiredPoint);
    const string N = "ability_tactician";
    const string D = "ability_tactician_description";
    const AbilityType T = AbilityType.Advanced;
    public Tactician(): base(N, D, T, 2) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
  }

  public class MindReader: Ability {
    public const bool Passive = false;
    public const int requiredPoint = 9;
    static AbilityControl ac = new AbilityControl(1, requiredPoint);
    const string N = "ability_mindReader";
    const string D = "ability_mindReader_description";
    const AbilityType T = AbilityType.Advanced;
    public MindReader(): base(N, D, T, 1) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
    public const int MoraleDropMin = 30;
    public const int MoraleDropMax = 60;
    public const int AffectRange = 2;
  }

  public class GameChanger: Ability {
    public const bool Passive = false;
    public const int requiredPoint = 10;
    static AbilityControl ac = new AbilityControl(1, requiredPoint);
    const string N = "ability_gameChanger";
    const string D = "ability_gameChanger_description";
    const AbilityType T = AbilityType.Advanced;
    public GameChanger(): base(N, D, T, 1) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
  }

}