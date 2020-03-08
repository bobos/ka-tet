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

    public Faction faction;
    public Party party;
    public Traits[] bornTraits;
    public List<Traits> developedTraits;
    public int age;
    public Province province;
    public Troop commandUnit;
    public event OnGeneralLeaveCampaign onGeneralLeaveCampaign;
    public event OnFieldEvent onFieldEvent;
    public FieldEvent[] fieldRecords;
    public LinkedList<General> nemesis = new LinkedList<General>();
    public GeneralStat stat = GeneralStat.Idle;

    HexMap hexMap;
    WarParty warParty;
    string name;
    string biography;
    TextLib txtLib = Cons.GetTextLib();

    public General(string name, string biography, Province province, Traits[] traits) {
      this.name = name;
      this.biography = biography;
      this.province = province;
      this.bornTraits = traits;
      developedTraits = new List<Traits>();
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
      commandUnit = new Troop(num, province, type, rank, this);
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

    public bool EnterCampaign(HexMap hexMap, Tile deploymentTile, int supply, int labor = 0) {
      this.hexMap = hexMap;
      if (commandUnit == null) return false;
      bool ready = commandUnit.EnterCampaign(deploymentTile, supply, labor);
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

    public void TroopDestroyed() {
      ReportFieldEvent(FieldEvent.Destroyed);
      commandUnit.Destroy();
      commandUnit.general = null;
      commandUnit = null;
      LeaveCampaign();
      // TODO: apply traits
      if (Cons.FairChance()) {
        // Killed in battle
        Die();
      }
    }

    public void UnitRiot() {
      int rand = Util.Rand(1, 10);
      party.influence -= 400;
    }

    void Die() {
      // TODO: assign new commanderGeneral in warparty
      hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.GeneralKilledInBattle, null, null, 0, 0, 0, 0, 0, null, this));
      LeaveFaction();
      stat = GeneralStat.Dead;
    }

    void LeaveCampaign() {
      // TODO: assign new commanderGeneral in warparty
      if (onGeneralLeaveCampaign != null) onGeneralLeaveCampaign(this);
      stat = GeneralStat.Rest;
    }

    void Treason() {
      // TODO: assign new commanderGeneral in warparty
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