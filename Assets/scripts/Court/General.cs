
using System.Collections.Generic;
using TextNS;
using FieldNS;
using MapTileNS;

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

  public class General {
    public delegate void OnGeneralLeaveCampaign(General general);
    public delegate void OnFieldEvent(FieldEvent fieldEvent, General general);

    public Faction faction;
    public Party party;
    public Traits[] bornTraits;
    public List<Traits> developedTraits;
    public int age;
    public Region region;
    public Troop commandUnit;
    public event OnGeneralLeaveCampaign onGeneralLeaveCampaign;
    public event OnFieldEvent onFieldEvent;
    public FieldEvent[] fieldRecords;
    public LinkedList<General> nemesis = new LinkedList<General>();
    public bool knowSoldiers = true;

    string name;
    string biography;
    TextLib txtLib = Cons.GetTextLib();

    public General(string name, string biography, Region region, Traits[] traits) {
      this.name = name;
      this.biography = biography;
      this.region = region;
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

    // At Court Actions
    public void Assign(Troop troop) {
      ResetFieldRecords();
      commandUnit = troop;
      if (troop.general != null) troop.general.commandUnit = null;
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
      this.faction = faction;
      if (this.party != null) {
        this.party.generals.Remove(this);
        if (commandUnit != null) {
          commandUnit.general = null;
        }
        commandUnit = null;
      }
      JoinParty(party);
    }

    public bool EnterCampaign(WarParty wp, Tile deploymentTile, int supply, int labor = 0) {
      if (commandUnit == null) return false;
      bool ready = commandUnit.EnterCampaign(deploymentTile, supply, labor);
      if (!ready) return ready;
      wp.JoinCampaign(this);
      return true;
    }

    public void LeaveCampaign() {
      if (onGeneralLeaveCampaign != null) onGeneralLeaveCampaign(this);
    }

    public void TroopDestroyed() {
      // TODO: chance to get killed
      LeaveCampaign();
      commandUnit = null;
    }

    // On Field Actions and Events
    public static void ReplaceOnField(General oldGen, General newGen, FieldParty targetParty) {
      newGen.AssignOnField(oldGen.commandUnit);
      oldGen.RemoveOnField();
      targetParty.GeneralEnterCampaign(newGen);
    }

    public void AssignOnField(Troop troop) {
      ResetFieldRecords();
      commandUnit = troop;
      commandUnit.general = this;
    }

    public void RemoveOnField() {
      ResetFieldRecords();
      commandUnit = null;
      if (onGeneralLeaveCampaign != null) onGeneralLeaveCampaign(this);
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