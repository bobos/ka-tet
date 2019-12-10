using System;
using System.Collections.Generic;
using TextNS;

namespace CourtNS {
  public class Party {
    public enum Relation
    {
      normal,
      tense,
      xTense
    }
  
    public HashSet<General> generals;
    public City[] cities;
    public Party counterParty; // TODO: when no counterParty, then no field party
    public Faction faction;
    public int influence {
      get {
        return _influence;
      }

      set {
        _influence = value < 0 ? 0 : value;
      }
    }
  
    int _influence;
    string name;
    string description;
    TextLib txtLib = Cons.GetTextLib();
  
    public Party (Faction faction, string name, string description, int influence = 1000) {
      this.faction = faction;
      this.name = name;
      this.description = description;
      this.influence = influence;
      generals = new HashSet<General>();
    }
  
    public string Name() {
      return txtLib.get(this.name);
    }
  
    public string Description() {
      return txtLib.get(this.description);
    }
  
    public Relation GetRelation() {
      int gap = Math.Abs(influence - counterParty.influence);
      int minor = influence > counterParty.influence ? counterParty.influence : influence;
      if (gap < (int)(minor / 2)) {
        return Relation.normal;
      }
      if (gap < minor) {
        return Relation.tense;
      }
      return Relation.xTense;
    }
  }
  
  

}