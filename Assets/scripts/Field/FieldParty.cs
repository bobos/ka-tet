using System.Collections.Generic;
using CourtNS;
using MonoNS;

namespace FieldNS
{
  public class FieldParty
  {
    public Party party;
    public PartyReport[] reports;
    public FieldParty counterFieldParty;
    public HashSet<General> generals = new HashSet<General>();
    public List<CombatController.ResultType> victories;
    public List<CombatController.ResultType> defeats;
    public FieldParty(Party party)
    {
      // TODO: report the point when phase ends
      victories = new List<CombatController.ResultType>();
      defeats = new List<CombatController.ResultType>();
      this.party = party;
      PartyReport[] reports = { new SloppyOnDrill(this) };
      this.reports = reports;
    }

    public void TheirGeneralEnterCampaign(General general)
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