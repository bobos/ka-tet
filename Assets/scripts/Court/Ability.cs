
using System.Collections.Generic;

namespace CourtNS {

  public abstract class Ability {
    public abstract string Name();
    public abstract string Description();
    public static Ability LuckyDraw() {
      Ability[] candidates = new Ability[]{Cons.forecaster, Cons.discipline,
        Cons.hammer, Cons.mechanician, Cons.diminisher, Cons.staminaManager,
        Cons.formidable, Cons.generous, Cons.runner, Cons.fireBug,
        Cons.holdTheGround, Cons.breaker, Cons.improvisor, Cons.tactic,
        Cons.outlooker, Cons.ambusher, Cons.doctor};
      int total = candidates.Length;
      int luckNum = Util.Rand(0, total + 15);
      if (luckNum < total) {
        return candidates[luckNum];
      }
      return null;
    }

    public static List<Ability> RandomAcquiredAbilities() {
      List<Ability> abilities = new List<Ability>();
      int num = Util.Rand(1, 4);
      for (int i = 0; i < num; i++)
      {
        Ability ability = LuckyDraw();
        if (ability != null && !abilities.Contains(ability)) {
          abilities.Add(ability);
        }
      }
      return abilities;
    }
  }

  public class Forecaster: Ability {
    public Forecaster() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_forecaster");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_forecaster_description");
    }
  } 

  public class Improvisor: Ability {
    public Improvisor() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_improvisor");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_improvisor_description");
    }
  }

  public class Discipline: Ability {
    public Discipline() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_discipline");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_discipline_description");
    }
  }

  public class Hammer: Ability {
    public Hammer() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_hammer");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_hammer_description");
    }
  }

  public class HoldTheGround: Ability {
    public HoldTheGround() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_holdTheGround");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_holdTheGround_description");
    }
  }

  public class Breaker: Ability {
    public Breaker() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_breaker");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_breaker_description");
    }
  }

  public class Formidable: Ability {
    public Formidable() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_formidable");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_formidable_description");
    }
  }

  public class Tactic: Ability {
    public Tactic() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_tactic");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_tactic_description");
    }
  }

  public class Mechanician: Ability {
    public Mechanician() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_mechanician");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_mechanician_description");
    }
  }

  public class Diminisher: Ability {
    public Diminisher() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_diminisher");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_diminisher_description");
    }
  }

  public class StaminaManager: Ability {
    public StaminaManager() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_staminaManager");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_staminaManager_description");
    }
  }

  public class Generous: Ability {
    public Generous() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_generous");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_generous_description");
    }
  }

  public class Runner: Ability {
    public Runner() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_runner");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_runner_description");
    }
  }

  public class FireBug: Ability {
    public FireBug() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_fireBug");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_fireBug_description");
    }
  }

  public class Outlooker: Ability {
    public Outlooker() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_outlooker");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_outlooker_description");
    }
  }

  public class Ambusher: Ability {
    public Ambusher() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_ambusher");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_ambusher_description");
    }
  }

  public class Doctor: Ability {
    public Doctor() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_doctor");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_doctor_description");
    }
  }

}