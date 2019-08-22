using UnitNS;
using MapTileNS;

namespace CourtNS
{
  public enum TroopState {
    OnField,
    Idle,
    Rest,
    Disbanded
  }

  public class Troop
  {
    const int AbilityMin = 40;
    const int AbilityMax = 80;
    public const int MaxMorale = 100;

    public string name;
    public Faction faction;
    public Region region;
    public General general;
    public int wounded = 0;
    public Unit onFieldUnit;
    Type type;
    int _morale;
    int _soldiers;
    int atkCore;
    int defCore;
    int movCore;
    SkillTree skillTree = new SkillTree();
    TroopState state;

    public static int MaxNum(Type type) {
      return type == Type.Cavalry ? Cavalry.MaxTroopNum : Infantry.MaxTroopNum;
    }

    public Troop(int soldiers, Faction faction, Region region, Type type) {
      this.soldiers = soldiers;
      this.faction = faction;
      name = region.AssignLegionName(type);
      atkCore = Util.Rand(AbilityMin, AbilityMax);
      defCore = Util.Rand(AbilityMin, AbilityMax);
      movCore = Util.Rand(AbilityMin + 60, AbilityMax + 60);
      morale = 60;
      this.region = region;
      this.type = type;
      state = TroopState.Idle;
    }

    public bool IsCavalry() {
      return type == Type.Cavalry;
    }

    public int morale {
      get {
        return _morale;
      }
      set {
        _morale = value < 0 ? 0 : (value > MaxMorale ? MaxMorale : value);
      }
    }

    public int soldiers {
      get {
        return _soldiers;
      }
      set {
        _soldiers = value < 0 ? 0 : (value > Troop.MaxNum(type) ? Troop.MaxNum(type) : value);
      }
    }

    public int atk {
      get {
        return (int)(atkCore + (atkCore * (region.AtkBuf(type) + skillTree.GetAttackBuff(type))));
      }
    }

    public int def {
      get {
        return (int)(defCore + (defCore * (region.DefBuf(type) + skillTree.GetDefenseBuff(type))));
      }
    }

    public int mov {
      get {
        return (int)(movCore + (movCore * region.MovBuf(type)));
      }
    }

    public int Enlist(int rookies) {
      int gap = Troop.MaxNum(type) - (soldiers + wounded);
      if (rookies < gap) { gap = rookies; }
      int returned = rookies - gap;
      soldiers += gap;
      return returned;
    }

    public bool EnterCampaign(Tile deploymentTile, int supply, int labor) {
      if (state != TroopState.Idle) {
        return false;
      }
      state = TroopState.OnField;
      if(IsCavalry()) {
        onFieldUnit = new Cavalry(false, this, deploymentTile, supply);
      } else {
        onFieldUnit = new Infantry(false, this, deploymentTile, supply, labor);
      }
      return true;
    }

    public void LeaveCampaign() {
      // TODO: return labor to faction
      state = TroopState.Rest;
      onFieldUnit = null;
      general.LeaveCampaign();
    }

    public void Destroy() {
      // called by onFieldUnit on destroy
      state = TroopState.Disbanded;
      onFieldUnit = null;
      general.TroopDestroyed();
    }

  }
}