
using System.Collections.Generic;

namespace CourtNS {

  public class CommandSkill {
    public int commandSkill;

    public CommandSkill(int skill) {
      commandSkill = skill < 1 ? 1 : skill > 4 ? 4 : skill;
    }

    public bool ObeyMyOrder() {
      return commandSkill > 3;
    }

    public bool TurningTide() {
      return commandSkill > 2;
    }

  }

}