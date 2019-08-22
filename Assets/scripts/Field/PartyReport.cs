using System.Collections.Generic;
using CourtNS;
using TextNS;

namespace FieldNS {
  public abstract class PartyReport {
    public abstract string Name();
    public abstract string Description();

    private FieldParty myParty;
    protected TextLib txtLib = Cons.GetTextLib();
    HashSet<General> watchedGenerals = new HashSet<General>();
    public PartyReport(FieldParty myParty) {
      this.myParty = myParty;
    }

    public void WatchGeneral(General general) {
      if (watchedGenerals.Contains(general)) return;
      watchedGenerals.Add(general);
      general.onFieldEvent += OnFieldEvent;
    }

    public void DontWatchGeneral(General general) {
      watchedGenerals.Remove(general);
      general.onFieldEvent -= OnFieldEvent;
    }

    public abstract void OnFieldEvent(FieldEvent fieldEvent, General general);
  }

  public class SloppyOnDrill: PartyReport {
    public override string Name() {
      return txtLib.get("pr_sloppyOnDrill");
    }

    public override string Description() {
      return txtLib.get("pr_sloppyOnDrill_d");
    }

    public SloppyOnDrill(FieldParty party) : base(party) {}

    public override void OnFieldEvent(FieldEvent fieldEvent, General general) {
    }

  }
}