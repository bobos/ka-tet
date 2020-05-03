
using System.Collections.Generic;

namespace CourtNS {

  public abstract class Trait {
    public abstract string Name();
    public abstract string Description();
    public static Trait Random() {
      Trait[] traits = new Trait[]{
        Cons.reckless,
        Cons.brave, Cons.brave,
        Cons.loyal, Cons.loyal,
        Cons.conservative, Cons.conservative,
        Cons.cunning, Cons.cunning,
        Cons.calm, Cons.calm, Cons.calm};
      return traits[Util.Rand(0, traits.Length-1)];
    }
  }

  public class Reckless: Trait {
    public override string Name() {
      return Cons.GetTextLib().get("trait_reckless");
    }

    public override string Description() {
      return Cons.GetTextLib().get("trait_reckless_description");
    }
  }

  public class Brave: Trait {
    public override string Name() {
      return Cons.GetTextLib().get("trait_brave");
    }

    public override string Description() {
      return Cons.GetTextLib().get("trait_brave_description");
    }
  }

  public class Loyal: Trait {
    public override string Name() {
      return Cons.GetTextLib().get("trait_loyal");
    }

    public override string Description() {
      return Cons.GetTextLib().get("trait_loyal_description");
    }
  }

  public class Conservative: Trait {
    public override string Name() {
      return Cons.GetTextLib().get("trait_conservative");
    }

    public override string Description() {
      return Cons.GetTextLib().get("trait_conservative_description");
    }
  }

  public class Cunning: Trait {
    public override string Name() {
      return Cons.GetTextLib().get("trait_cunning");
    }

    public override string Description() {
      return Cons.GetTextLib().get("trait_cunning_description");
    }
  }

  public class Calm: Trait {
    public override string Name() {
      return Cons.GetTextLib().get("trait_calm");
    }

    public override string Description() {
      return Cons.GetTextLib().get("trait_calm_description");
    }
  }

}