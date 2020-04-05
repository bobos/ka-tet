
using System.Collections.Generic;

namespace CourtNS {

  public class CommandSkill {
    public int commandSkill;
    public List<Ability> abilities = new List<Ability>();

    public CommandSkill(int skill) {
      commandSkill = skill < 1 ? 1 : skill > 4 ? 4 : skill;
      if (commandSkill < 3) {
        abilities.Add(Cons.outOfControl);
        if (commandSkill == 1) {
          abilities.Add(Cons.outOfOrder);
        }
      } else {
        Ability[] candidates = new Ability[]{Cons.masterOfMist, Cons.backStabber, Cons.obey, Cons.turningTide};
        int num = commandSkill > 3 ? Util.Rand(3, 4) : Util.Rand(1, 2);
        for (int i = 0; i < num; i++)
        {
          int luckDraw = Util.Rand(0, candidates.Length - 1);
          if (!abilities.Contains(candidates[luckDraw])) {
            abilities.Add(candidates[luckDraw]);
          }
        }
      }
    }

    public int GetCommandRange() {
      if (commandSkill == 1) {
        return 1;
      }

      if (commandSkill == 2) {
        return 2;
      }

      if (commandSkill == 3) {
        return 4;
      }

      return 6;
    }

  }

}