
using System.Collections.Generic;

namespace CourtNS {

  public class CommandSkill {
    public int commandSkill;

    public CommandSkill(int skill) {
      commandSkill = skill < 1 ? 1 : skill > 4 ? 4 : skill;
    }

    public int GetCommandRange() {
      if (commandSkill == 1) {
        return 0;
      }

      if (commandSkill == 2) {
        return 2;
      }

      if (commandSkill == 3) {
        return 3;
      }

      return 6;
    }

    public bool ObeyMyOrder() {
      return commandSkill > 2;
    }

    public bool TurningTide() {
      return commandSkill > 3;
    }

  }

}