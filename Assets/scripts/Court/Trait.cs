
using System.Collections.Generic;

namespace CourtNS {

  public abstract class Trait {
    public abstract string Name();
    public abstract List<Ability> Abilities();
    public static List<Trait> RandomTraits() {
      List<Trait> traits = new List<Trait>();
      Trait trait = null;
      if (Cons.FairChance()) {
        trait = Cons.reckless;
      } else if (Cons.TinyChance()) {
        trait = Cons.brave;
      }
      
      if (trait != null) {
        traits.Add(trait);
        if (trait == Cons.brave) {
          if (Cons.FairChance()) {
            traits.Add(Cons.loyal);
          } else if (Cons.SlimChance()) {
            traits.Add(Cons.cunning);
          }
        } else {
          if (Cons.SlimChance()) {
            traits.Add(Cons.loyal);
          }
        }
      } else {
        if (Cons.FairChance()) {
          traits.Add(Cons.conservative);
        } else if (Cons.FairChance()) {
          traits.Add(Cons.loyal);
        } else if (Cons.SlimChance()) {
          traits.Add(Cons.cunning);
        }
      }

      return traits;
    }
  }

  public class Reckless: Trait {
    public override string Name() {
      return Cons.GetTextLib().get("trait_reckless");
    }

    public override List<Ability> Abilities() {
      return new List<Ability>(){Cons.forwarder, Cons.holdTheGround};
    }
  }

  public class Brave: Trait {
    public override string Name() {
      return Cons.GetTextLib().get("trait_brave");
    }

    public override List<Ability> Abilities() {
      return new List<Ability>(){Cons.breaker, Cons.unshaken,
        Cons.holdTheGround, Cons.easyTarget, Cons.counterAttack};
    }
  }

  public class Loyal: Trait {
    public override string Name() {
      return Cons.GetTextLib().get("trait_loyal");
    }

    public override List<Ability> Abilities() {
      return new List<Ability>(){Cons.attender, Cons.refuseToRetreat, Cons.unshaken};
    }
  }

  public class Conservative: Trait {
    public override string Name() {
      return Cons.GetTextLib().get("trait_conservative");
    }

    public override List<Ability> Abilities() {
      return new List<Ability>(){Cons.retreater, Cons.playSafe};
    }
  }

  public class Cunning: Trait {
    public override string Name() {
      return Cons.GetTextLib().get("trait_cunning");
    }

    public override List<Ability> Abilities() {
      return new List<Ability>(){Cons.feintDefeat, Cons.opportunist, Cons.playSafe};
    }
  }

}