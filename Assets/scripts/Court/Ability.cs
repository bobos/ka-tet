
using System.Collections.Generic;

namespace CourtNS {
  public enum AbilityType {
    Common,
    Infantry,
    Cavalry,
    Advanced
  }

  public abstract class Ability {
    public static void Acquire4Test(General general) {
      List<Ability> all = new List<Ability>(){
        new DrillMaster(),
        new FireBug(),
        new Ambusher(),
        new Striker(),
        new Outlooker(),
        new Rally(),
        new Holder(),
        new Builder(),
        new Fortifier(),
        new Herbist(),
        new Breacher(),
        new Runner(),
        new StaminaManager(),
        new Hammer(),
        new Finisher(),
        new Sentinel(),
        new Conspirator(),
        new Deciever(),
        new Agitator(),
        new MindReader(),
        new GameChanger(),
        new ShadowWarrior(),
        new Disruptor()
      };

      List<Ability> remaining = new List<Ability>();
      foreach(Ability ability in all) {
        if (ability.RequiredPoints() <= general.militatyPoints) {
          remaining.Add(ability);
        }
      }
      while (remaining.Count > 0) {
        Ability ability = remaining[Util.Rand(0, remaining.Count-1)];
        general.acquiredAbilities.Add(ability.Clone());
        general.militatyPoints -= ability.RequiredPoints();
        remaining.Remove(ability);
        all = new List<Ability>();
        foreach(Ability a in remaining) {
          if (a.RequiredPoints() <= general.militatyPoints) {
            all.Add(ability);
          }
        }
        remaining = all;
      }
    }

    public abstract int RequiredPoints();
    public string Name() {
      return Cons.GetTextLib().get(name);
    }

    public string Description() {
      return Cons.GetTextLib().get(description);
    }

    public abstract Ability Clone();
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
    public override Ability Clone() { return new DrillMaster(); }
    public override int RequiredPoints() { return requiredPoint; }
    
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
    public override Ability Clone() { return new FireBug(); }
    public override int RequiredPoints() { return requiredPoint; }
    
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
    public override Ability Clone() { return new Ambusher(); }
    public override int RequiredPoints() { return requiredPoint; }
    
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
    public override Ability Clone() { return new Striker(); }
    public override int RequiredPoints() { return requiredPoint; }
    
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
    public override Ability Clone() { return new Outlooker(); }
    public override int RequiredPoints() { return requiredPoint; }
    
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
    public override Ability Clone() { return new Rally(); }
    public override int RequiredPoints() { return requiredPoint; }
    
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

  public class ShadowWarrior: Ability {
    public override Ability Clone() { return new ShadowWarrior(); }
    public override int RequiredPoints() { return requiredPoint; }
    
    public const bool Passive = true;
    public const int requiredPoint = 3;
    static AbilityControl ac = new AbilityControl(-1, requiredPoint);
    const string N = "ability_shadowWarrior";
    const string D = "ability_shadowWarrior_description";
    const AbilityType T = AbilityType.Common;
    public ShadowWarrior(): base(N, D, T) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
  }

  public class Holder: Ability {
    public override Ability Clone() { return new Holder(); }
    public override int RequiredPoints() { return requiredPoint; }
    
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
    public override Ability Clone() { return new Builder();
    }
    public override int RequiredPoints() { return requiredPoint; }
    
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
    public override Ability Clone() { return new Fortifier(); }
    public override int RequiredPoints() { return requiredPoint; }
    
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
    public override Ability Clone() { return new Herbist(); }
    public override int RequiredPoints() { return requiredPoint; }
    
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
    public override Ability Clone() { return new Breacher(); }
    public override int RequiredPoints() { return requiredPoint; }
    
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
    public override Ability Clone() { return new Runner(); }
    public override int RequiredPoints() { return requiredPoint; }
    
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
    public override Ability Clone() { return new StaminaManager(); }
    public override int RequiredPoints() { return requiredPoint; }
    
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
    public override Ability Clone() { return new Hammer(); }
    public override int RequiredPoints() { return requiredPoint; }
    
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
    public override Ability Clone() { return new Finisher(); }
    public override int RequiredPoints() { return requiredPoint; }
    
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

  public class Sentinel: Ability {
    public override Ability Clone() { return new Sentinel(); }
    public override int RequiredPoints() { return requiredPoint; }
    
    public const bool Passive = true;
    public const int requiredPoint = 6;
    static AbilityControl ac = new AbilityControl(4, requiredPoint);
    const string N = "ability_sentinel";
    const string D = "ability_sentinel_description";
    const AbilityType T = AbilityType.Advanced;
    public Sentinel(): base(N, D, T) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
    public static int RedzoneRange = 2;
  }

  public class Agitator: Ability {
    public override Ability Clone() { return new Agitator(); }
    public override int RequiredPoints() { return requiredPoint; }
    
    public const bool Passive = false;
    public const int requiredPoint = 6;
    static AbilityControl ac = new AbilityControl(3, requiredPoint);
    const string N = "ability_agitator";
    const string D = "ability_agitator_description";
    const AbilityType T = AbilityType.Advanced;
    public Agitator(): base(N, D, T, 2) {}
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

  public class Conspirator: Ability {
    public override Ability Clone() { return new Conspirator(); }
    public override int RequiredPoints() { return requiredPoint; }
    
    public const bool Passive = false;
    public const int requiredPoint = 7;
    static AbilityControl ac = new AbilityControl(2, requiredPoint);
    const string N = "ability_conspirator";
    const string D = "ability_conspirator_description";
    const AbilityType T = AbilityType.Advanced;
    public Conspirator(): base(N, D, T, 2) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
  }

  public class Deciever: Ability {
    public override Ability Clone() { return new Deciever(); }
    public override int RequiredPoints() { return requiredPoint; }
    
    public const bool Passive = false;
    public const int requiredPoint = 8;
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

  public class MindReader: Ability {
    public override Ability Clone() { return new MindReader(); }
    public override int RequiredPoints() { return requiredPoint; }
    
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

  public class Disruptor: Ability {
    public override Ability Clone() { return new Disruptor(); }
    public override int RequiredPoints() { return requiredPoint; }
    
    public const bool Passive = false;
    public const int requiredPoint = 7;
    static AbilityControl ac = new AbilityControl(3, requiredPoint);
    const string N = "ability_disruptor";
    const string D = "ability_disruptor_description";
    const AbilityType T = AbilityType.Advanced;
    public Disruptor(): base(N, D, T, 1) {}
    public static void Unlock(Faction faction) { ac.Unlock(faction); }
    public static bool Acquire(General general) { return ac.Acquire(general); }
    public static bool Find(General general) { return Ability.Find(N, general); }
    public static bool Find(Faction faction) { return ac.Find(faction); }
    public static bool Aval(General general) { return Find(general) && Ability.Aval(T, general); }
  }

  public class GameChanger: Ability {
    public override Ability Clone() { return new GameChanger(); }
    public override int RequiredPoints() { return requiredPoint; }
    
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