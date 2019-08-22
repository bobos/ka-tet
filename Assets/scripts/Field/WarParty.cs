using System.Collections.Generic;
using UnitNS;
using CourtNS;

namespace FieldNS
{
  public class WarPartyStat
  {
    public int numOfTroops;
    public int numOfSoldiers;
    public int numOfDead;
    public int numOfWound;
    public int numOfLabor;

    public WarPartyStat(int numOfTroops, int numOfSoldiers, int numOfDead,
              int numOfWound, int numOfLabor)
    {
      this.numOfDead = numOfDead;
      this.numOfLabor = numOfLabor;
      this.numOfSoldiers = numOfSoldiers;
      this.numOfTroops = numOfTroops;
      this.numOfWound = numOfWound;
    }
  }

  public class WarParty
  {
    public WarParty(bool isAI, bool attackside, Faction faction)
    {
      this.isAI = isAI;
      this.attackside = attackside;
      this.faction = faction;
      foreach (Party party in faction.GetParties())
      {
        fieldParties.Add(new FieldParty(party));   
      }
    }
    public List<FieldParty> fieldParties = new List<FieldParty>();
    public int force;
    public int wounded;
    public int kia;
    public int mia;
    public int captives;
    public Faction faction;

    public bool isAI { get; private set; }
    public bool attackside { get; private set; }
    HashSet<Unit> units = new HashSet<Unit>();

    public void JoinCampaign(General general) {
      Unit unit = general.commandUnit.onFieldUnit;
      units.Add(unit);
      unit.SpawnOnMap();
      foreach (FieldParty fieldParty in fieldParties)
      {
        if (Util.eq<Party>(general.party, fieldParty.party)) {
          FieldParty counterParty = fieldParty.counterFieldParty;
          if (counterParty != null) counterParty.GeneralEnterCampaign(general);
        }
      }
    }

    public HashSet<Unit> GetUnits()
    {
      HashSet<Unit> _units = new HashSet<Unit>();
      foreach (Unit unit in units)
      {
        if (!unit.IsGone()) _units.Add(unit);
      }
      return _units;
    }

    public WarPartyStat GetStat()
    {
      int numOfDead = 0;
      int numOfLabor = 0;
      int numOfSoldiers = 0;
      int numOfTroops = GetUnits().Count;
      int numOfWound = 0;

      foreach (Unit unit in units)
      {
        if (unit.IsGone()) {
          numOfDead += unit.kia;
        } else {
          numOfDead += unit.kia;
          numOfLabor += unit.labor;
          numOfSoldiers += unit.rf.soldiers;
          numOfWound += unit.rf.wounded;
        }
      }

      return new WarPartyStat(numOfTroops, numOfSoldiers,
                              numOfDead, numOfWound, numOfLabor);
    }

  }

}