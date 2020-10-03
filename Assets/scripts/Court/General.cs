using System.Collections.Generic;
using TextNS;
using FieldNS;
using MapTileNS;
using MonoNS;

namespace CourtNS {
  public enum FieldEvent {
    Undefined,
    CrashingVictory,
    GreatVictory,
    Victory,
    SmallDefeat,
    Defeat,
    GreatDefeat,
    SupplyIntercepted,
    Destroyed,
    Retreated,
    NoAction
  }

  public enum GeneralStat {
    OnField,
    Dead,
    OnCourtTask,
    Idle,
    Rest
  }

  public class General {
    public delegate void OnGeneralLeaveCampaign(General general);
    public delegate void OnFieldEvent(FieldEvent fieldEvent, General general);

    Faction _faction = null;
    public Faction faction {
      get {
        if (_faction == null) {
          Util.Throw("faction not set for " + Name());
        }
        return _faction;
      }
      set {
        _faction = value;
      }
    }
    public Party party;
    public Trait trait;
    public List<Ability> acquiredAbilities;
    public int age;
    public Province province;
    public Troop commandUnit;
    public event OnGeneralLeaveCampaign onGeneralLeaveCampaign;
    public event OnFieldEvent onFieldEvent;
    public FieldEvent[] fieldRecords;
    public LinkedList<General> nemesis = new LinkedList<General>();
    public GeneralStat stat = GeneralStat.Idle;
    public CommandSkill commandSkill;
    public int militatyPoints;

    HexMap hexMap;
    string name;
    string biography;
    TextLib txtLib = Cons.GetTextLib();

    public General(string name, string biography, Province province, CommandSkill commandSkill,
      int militaryPoints, List<Trait> traits = null) {
      this.name = name;
      this.biography = biography;
      this.province = province;
      this.trait = Trait.Random();
      this.militatyPoints = militaryPoints;
      this.commandSkill = commandSkill;
    }

    public bool Is(Trait trait) {
      return Util.eq<Trait>(this.trait, trait);
    }

    // General Stats
    public string Name() {
      return txtLib.get(name);
    }

    public string Bio() {
      return txtLib.get(biography);
    }

    public bool IsDead() {
      return stat == GeneralStat.Dead;
    }

    public bool IsIdle() {
      return stat == GeneralStat.Idle;
    }

    public bool IsRest() {
      return stat == GeneralStat.Rest;
    }

    public bool IsOnField() {
      return stat == GeneralStat.OnField;
    }

    // At Court Actions
    public void CreateTroop(HexMap hexMap, int num, Province province, UnitNS.Type type) {
      ResetFieldRecords();
      this.hexMap = hexMap;
      commandUnit = new Troop(num, province, type, this);
    }

    public void JoinParty(Party party) {
      party.generals.Add(this);
      this.party = party;
      Party counterParty = party.counterParty;
      if (counterParty != null) {
        counterParty.generals.Remove(this);
      }
    }

    public void JoinFaction(Faction faction, Party party) {
      faction.AddGeneral(this);
      this.faction = faction;
      JoinParty(party);
    }

    public void LeaveFaction() {
      this.faction.RemoveGeneral(this);
      this.faction = null;
      if (this.party != null) {
        this.party.generals.Remove(this);
      }
      stat = GeneralStat.Idle;
    }

    public bool EnterCampaign(HexMap hexMap, Tile deploymentTile) {
      this.hexMap = hexMap;
      if (commandUnit == null) return false;
      bool ready = commandUnit.EnterCampaign(deploymentTile);
      if (!ready) return ready;
      stat = GeneralStat.OnField;
      hexMap.GetWarParty(faction).JoinCampaign(this);
      return true;
    }

    public void TroopRetreat() {
      ReportFieldEvent(FieldEvent.Retreated);
      commandUnit.LeaveCampaign();
      LeaveCampaign();
    }

    public void TroopDestroyed(bool generalDead = false) {
      ReportFieldEvent(FieldEvent.Destroyed);
      commandUnit.Destroy();
      commandUnit.general = null;
      commandUnit = null;
      LeaveCampaign();
      if ((Is(Cons.brave) && Cons.FiftyFifty()) ||
          Cons.FairChance() || generalDead) {
        // Killed in battle
        Die();
      }
    }

    void Die() {
      LeaveFaction();
      stat = GeneralStat.Dead;
    }

    void LeaveCampaign() {
      if (onGeneralLeaveCampaign != null) onGeneralLeaveCampaign(this);
      stat = GeneralStat.Rest;
    }

    public List<FieldEvent> GetFieldRecords() {
      List<FieldEvent> records = new List<FieldEvent>();
      int currentIndex = ringIndex;
      TmpFun fun = (int index) => {
        FieldEvent record = fieldRecords[index];
        if (record != FieldEvent.Undefined) {
          records.Add(record);
          return true;
        }
        return false;
      };
      if (!fun(currentIndex--)) {
        return records;
      }
      while (currentIndex != ringIndex) {
        if (currentIndex < 0) currentIndex = MaxRecords - 1;
        if (!fun(currentIndex--)) {
          return records;
        }
      }
      return records;
    }

    // field records
    const int MaxRecords = 5;
    int ringIndex = 0;
    delegate bool TmpFun(int i);
    void ResetFieldRecords() {
      ringIndex = 0;
      fieldRecords = new FieldEvent[MaxRecords];
      for (int i = 0; i < MaxRecords; i++)
      {
        fieldRecords[i] = FieldEvent.Undefined;
      }
      ringIndex = 0;
    }

    public void ReportFieldEvent(FieldEvent record) {
      if (onFieldEvent != null) onFieldEvent(record, this);
      fieldRecords[ringIndex++] = record;
      if (ringIndex == MaxRecords) ringIndex = 0;
    }

  }
}