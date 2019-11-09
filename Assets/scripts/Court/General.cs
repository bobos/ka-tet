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
    public bool knowSoldiers = true;
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

    // At Court Actions
    public void Assign(HexMap hexMap, Troop troop) {
      ResetFieldRecords();
      this.hexMap = hexMap;
      commandUnit = troop;
      troop.general = this;
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

    public bool EnterCampaign(HexMap hexMap, WarParty wp, Tile deploymentTile, int supply, int labor = 0) {
      this.hexMap = hexMap;
      warParty = wp;
      if (commandUnit == null) return false;
      bool ready = commandUnit.EnterCampaign(deploymentTile, supply, labor);
      if (!ready) return ready;
      stat = GeneralStat.OnField;
      wp.JoinCampaign(this);
      return true;
    }

    public void TroopRetreat() {
      ReportFieldEvent(FieldEvent.Retreated);
      HandOutTroop().LeaveCampaign();
      LeaveCampaign();
    }

    public void TroopDestroyed() {
      ReportFieldEvent(FieldEvent.Destroyed);
      HandOutTroop().Destroy();
      LeaveCampaign();
      if (Cons.FairChance()) {
        // Killed in battle
        Die();
      }
    }

    public void GeneralKilledOnField() {
      hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.GeneralKilledInBattle, null, null, 0, 0, 0, 0, 0, null, this));
      Troop troop = HandOutTroop();
      LeaveCampaign();
      Die();
      AssignOnField(troop);
    }

    public void UnitRiot() {
      int rand = Util.Rand(1, 10);
      party.influence -= 100;
      AssignOnField(HandOutTroop());
      if (rand < 6) {
        // Returned
        LeaveCampaign();
      } else if (rand < 9) {
        // Resigned
        LeaveFaction();
      } else {
        // Executed
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

    void AssignOnField(Troop troop) {
      General newGen = faction.GetAvailableGeneral();
      if (newGen == null) {
        troop.onFieldUnit.Retreat();
        return;
      }
      newGen.Assign(hexMap, troop);
      newGen.stat = GeneralStat.OnField;
      warParty.Join(newGen);
    }

    Troop HandOutTroop() {
      Troop troop = commandUnit;
      if (troop != null) {
        troop.general = null;
      }
      commandUnit = null;
      return troop;
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