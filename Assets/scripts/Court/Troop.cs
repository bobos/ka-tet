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
    public Province province;
    public General general;
    public int wounded = 0;
    public Unit onFieldUnit;
    Type type;
    int _morale;
    int _soldiers;
    int atkCore;
    int defCore;
    int movCore;
    public Rank rank;
    public Level level = new Level();
    TroopState state;

    public static int MaxNum(Type type) {
      return type == Type.Cavalry ? Cavalry.MaxTroopNum : Infantry.MaxTroopNum;
    }

    public Troop(int soldiers, Faction faction, Province province, Type type, Rank rank) {
      this.rank = rank;
      this.type = type;
      this.soldiers = soldiers;
      this.faction = faction;
      name = province.AssignLegionName(type);
      atkCore = province.region.Atk(type);
      defCore = province.region.Def(type);
      movCore = province.region.Mov(type);
      morale = province.region.Will();
      this.province = province;
      state = TroopState.Idle;
    }

    public bool IsCavalry() {
      return type == Type.Cavalry;
    }

    public bool IsRest() {
      return state == TroopState.Rest;
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
        if (onFieldUnit != null) {
          onFieldUnit.UpdateUnitInfo();
        }
      }
    }

    public int atk {
      get {
        return (int)(atkCore + (atkCore * rank.AtkBuf()));
      }
    }

    public int def {
      get {
        return (int)(defCore + (defCore * rank.DefBuf()));
      }
    }

    public int mov {
      get {
        return movCore;
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
    }

    public void Destroy() {
      state = TroopState.Disbanded;
      onFieldUnit = null;
    }

    public void AssignGeneral(General general) {
      this.general = general;
      if (onFieldUnit != null) {
        onFieldUnit.UpdateGeneralName();
      }
    }

  }
}