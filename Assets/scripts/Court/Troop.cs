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
    public Type type;
    int _morale;
    int _soldiers;
    public int combatPoint;
    public int movementPoint;
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
      combatPoint = province.region.CombatPoint(type);
      movementPoint = province.region.Mov(type);
      morale = province.region.Will();
      this.province = province;
      state = TroopState.Idle;
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
      }
    }

    public float lvlBuf {
      get {
        return rank.Buf(morale);
      }
    }

    public int mov {
      get {
        return movementPoint;
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
      if(type == Type.Cavalry) {
        onFieldUnit = Cavalry.Create(false, this, deploymentTile, supply);
      } else {
        onFieldUnit = Infantry.Create(false, this, deploymentTile, supply, labor);
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
      //if (onFieldUnit != null) {
      //  onFieldUnit.UpdateGeneralName();
      //}
    }

  }
}