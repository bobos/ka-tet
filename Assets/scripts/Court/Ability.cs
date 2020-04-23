
using System.Collections.Generic;

namespace CourtNS {

  public abstract class Ability {
    public abstract string Name();
    public abstract string Description();
    public static Ability LuckyDraw() {
      Ability[] candidates = new Ability[]{Cons.forecaster, Cons.discipline,
        Cons.pursuer, Cons.hammer, Cons.builder, Cons.breacher, Cons.noPanic,
        Cons.mechanician, Cons.diminisher, Cons.staminaManager, Cons.punchThrough,
        Cons.generous, Cons.runner, Cons.fireBug};
      int total = candidates.Length;
      int luckNum = Util.Rand(0, total + 5);
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

  public class OutOfControl: Ability {
    public OutOfControl() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_outOfControl");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_outOfControl_description");
    }
  }

  public class OutOfOrder: Ability {
    public OutOfOrder() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_outOfOrder");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_outOfOrder_description");
    }
  }

  public class MasterOfMist: Ability {
    public MasterOfMist() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_masterOfMist");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_masterOfMist_description");
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

  public class BackStabber: Ability {
    public BackStabber() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_backStabber");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_backStabber_description");
    }
  }

  public class Obey: Ability {
    public Obey() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_mustObey");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_mustObey_description");
    }
  }

  public class TurningTheTide: Ability {
    public TurningTheTide() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_turningTheTide");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_turningTheTide_description");
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

  public class Pursuer: Ability {
    public Pursuer() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_pursuer");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_pursuer_description");
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

  public class Builder: Ability {
    public Builder() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_builder");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_builder_description");
    }
  }

  public class Breacher: Ability {
    public Breacher() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_breacher");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_breacher_description");
    }
  }

  public class NoPanic: Ability {
    public NoPanic() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_noPanic");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_noPanic_description");
    }
  }

  public class Forwarder: Ability {
    public Forwarder() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_forwarder");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_forwarder_description");
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

  public class Unshaken: Ability {
    public Unshaken() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_unshaken");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_unshaken_description");
    }
  }

  public class EasyTarget: Ability {
    public EasyTarget() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_easyTarget");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_easyTarget_description");
    }
  }

  public class CounterAttack: Ability {
    public CounterAttack() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_formidable");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_formidable_description");
    }
  }

  public class Attender: Ability {
    public Attender() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_attender");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_attender_description");
    }
  }

  public class RefuseToRetreat: Ability {
    public RefuseToRetreat() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_refuseToRetreat");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_refuseToRetreat_description");
    }
  }

  public class Retreater: Ability {
    public Retreater() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_retreater");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_retreater_description");
    }
  }

  public class FeintDefeat: Ability {
    public FeintDefeat() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_feintDefeat");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_feintDefeat_description");
    }
  }

  public class Opportunist: Ability {
    public Opportunist() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_opportunist");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_opportunist_description");
    }
  }

  public class PlaySafe: Ability {
    public PlaySafe() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_playSafe");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_playSafe_description");
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

  public class PunchThrough: Ability {
    public PunchThrough() {
    }

    public override string Name() {
      return Cons.GetTextLib().get("ability_punchThrough");
    }

    public override string Description() {
      return Cons.GetTextLib().get("ability_punchThrough_description");
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

}