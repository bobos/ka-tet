namespace CourtNS {
  public class CommandSkill {
    public int commandSkill;
    public int remainingOrder;

    public CommandSkill(int skill) {
      commandSkill = skill < 1 ? 1 : skill > 4 ? 4 : skill;
    }

    public void Reset(int num = 0) {
      remainingOrder = num == 0 ? NumOfOrder() : num;
    }

    public int NumOfOrder() {
      switch (commandSkill)
      {
        case 1: return 2;
        case 2: return 3;    
        case 3: return 4;    
        default: return 8;
      }
    }

    public bool GiveOrder() {
      if (remainingOrder == 0) {
        return false;
      }
      remainingOrder--;
      return true;
    }

    public bool Obey() {
      return commandSkill > 3;
    }
  }

}