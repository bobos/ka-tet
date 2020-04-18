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
    public List<Trait> traits;
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
    public HashSet<Ability> onFieldAbilities;

    HexMap hexMap;
    WarParty warParty;
    string name;
    string biography;
    TextLib txtLib = Cons.GetTextLib();
    public TroopSize size;

    public General(string name, string biography, Province province, CommandSkill commandSkill, TroopSize size,
      List<Trait> traits = null, List<Ability> acquired = null) {
      this.name = name;
      this.biography = biography;
      this.province = province;
      this.traits = traits == null ? Trait.RandomTraits() : traits;
      acquiredAbilities = acquired == null ? Ability.RandomAcquiredAbilities() : acquired;
      this.commandSkill = commandSkill;
      this.size = size;
    }

    public bool Has(Ability ability) {
      return onFieldAbilities.Contains(ability);
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
    public void CreateTroop(HexMap hexMap, int num, Province province, UnitNS.Type type, UnitNS.Rank rank) {
      ResetFieldRecords();
      this.hexMap = hexMap;
      int maxNum = MaxNum(type);
      commandUnit = new Troop(num > maxNum ? maxNum : num, province, type, rank, this);
    }

    public int MaxNum(UnitNS.Type type) {
      return size.GetTroopSize(type == UnitNS.Type.Infantry);
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
      InitOnFieldAbilities();
      bool ready = commandUnit.EnterCampaign(deploymentTile);
      if (!ready) return ready;
      stat = GeneralStat.OnField;
      hexMap.GetWarParty(faction).JoinCampaign(this);
      return true;
    }

    public void InitOnFieldAbilities(bool isCommander = false) {
      onFieldAbilities = new HashSet<Ability>();
      if (isCommander) {
        foreach(Ability ability in commandSkill.abilities) {
          onFieldAbilities.Add(ability);
        }
      }
      foreach(Trait trait in traits) {
        foreach(Ability ability in trait.Abilities()) {
          onFieldAbilities.Add(ability);
        }
      }
      foreach(Ability ability in acquiredAbilities) {
        onFieldAbilities.Add(ability);
      }
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
      if ((Has(Cons.easyTarget) && Cons.FiftyFifty()) ||
          (Has(Cons.playSafe) && Cons.TinyChance()) ||
          Cons.FairChance() || generalDead
          ) {
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