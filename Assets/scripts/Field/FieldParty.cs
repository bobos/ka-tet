using System.Collections.Generic;
using CourtNS;

namespace FieldNS
{
  public class FieldParty
  {
    public Party party;
    public PartyReport[] reports;
    public int fieldPressure = 0;
    public FieldParty counterFieldParty;
    public HashSet<General> generals = new HashSet<General>();
    public FieldParty(Party party)
    {
      this.party = party;
      PartyReport[] reports = { new SloppyOnDrill(this) };
      this.reports = reports;
    }

    public void GeneralEnterCampaign(General general)
    {
      generals.Add(general);
      general.onGeneralLeaveCampaign += OnGeneralLeaveCampaign;
      if (counterFieldParty != null)
      {
        counterFieldParty.CounterPartyGeneralEnterCampaign(general);
      }
    }

    public void CounterPartyGeneralEnterCampaign(General general)
    {
      foreach (PartyReport report in reports)
      {
        report.WatchGeneral(general);
      }
    }

    public void CounterPartyGeneralLeaveCampaign(General general)
    {
      foreach (PartyReport report in reports)
      {
        report.DontWatchGeneral(general);
      }
    }

    public void OnGeneralLeaveCampaign(General general)
    {
      generals.Remove(general);
      if (counterFieldParty != null)
      {
        counterFieldParty.CounterPartyGeneralLeaveCampaign(general);
      }
    }

  }
}