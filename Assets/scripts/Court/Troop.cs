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
    public const int MaxOrg = 65;

    public string name;
    public Faction faction;
    public Province province;
    public General general;
    public Unit onFieldUnit;
    public Type type;
    int _org;
    int _soldiers = 0;
    public int combatPoint;
    public int movementPoint;
    TroopState state;

    public Troop(int soldiers, Province province, Type type, General general) {
      this.type = type;
      Enlist(soldiers);
      this.general = general;
      faction = general.faction;
      name = province.region.Name();
      combatPoint = province.region.CombatPoint(type);
      movementPoint = 100;
      org = province.region.DefaultOrganizationPoint();
      this.province = province;
      state = TroopState.Idle;
    }

    public bool IsSpecial() {
      return Util.eq<Region>(province.region, Cons.nvzhen);
    }

    public bool IsRest() {
      return state == TroopState.Rest;
    }

    public int org {
      get {
        return _org;
      }
      set {
        _org = value < 0 ? 0 : (value > province.region.MaxOrganizationPoint()
         ? province.region.MaxOrganizationPoint() : value);
      }
    }

    public int soldiers {
      get {
        return _soldiers;
      }
      set {
        _soldiers = value < 0 ? 0 : value;
      }
    }

    public int mov {
      get {
        return movementPoint;
      }
    }

    public int Enlist(int rookies) {
      int maxNum = type == Type.Infantry ? Infantry.MaxTroopNum : Cavalry.MaxTroopNum;
      int gap = maxNum - soldiers;
      if (rookies < gap) { gap = rookies; }
      int returned = rookies - gap;
      soldiers += gap;
      return returned;
    }

    public bool EnterCampaign(Tile deploymentTile) {
      if (state != TroopState.Idle) {
        return false;
      }
      state = TroopState.OnField;
      if (general.Has(Cons.generous)) {
        org += 10;
      }
      movementPoint = general.Has(Cons.runner) ? 150 : 100;
      if (type == Type.Infantry) {
        onFieldUnit = Infantry.Create(false, this, deploymentTile);
      } else {
        onFieldUnit = Cavalry.Create(false, this, deploymentTile);
      }
      return true;
    }

    public void LeaveCampaign() {
      state = TroopState.Rest;
      onFieldUnit = null;
    }

    public void Destroy() {
      state = TroopState.Disbanded;
      onFieldUnit = null;
    }
  }
}