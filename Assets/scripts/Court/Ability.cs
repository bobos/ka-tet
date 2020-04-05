
namespace CourtNS {

  public abstract class Ability {
    public abstract string Name();
    public abstract string Description();
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

}