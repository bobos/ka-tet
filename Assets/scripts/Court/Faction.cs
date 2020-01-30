using System.Collections.Generic;
using UnitNS;
using TextNS;

namespace CourtNS {
  public interface Faction {
    string Name();
    string Description();
    bool IsAI();
    void SetAs(bool AI);

	  void AddGeneral(General general);
	  void RemoveGeneral(General general);
    General GetAvailableGeneral();

		Faction OverLord();
		HashSet<Faction> SubLords();
		void AddSubLord(Faction faction);
		void RemSubLord(Faction faction);
		void SetOverLord(Faction faction);
    int MaxUnitSize(Type unitType);
    Party[] GetParties();
  }

	public abstract class _Faction: Faction {
    public const float MaxInfanUnitSizeRatio = 0.011f; // 100 civillians: 1 soldier
    public const float MaxCavUnitSizeRatio = 0.0067f; // 150 civillians: 1 soldier
    public int population = 0;
	  protected bool isAI;
		HashSet<Faction> subLords = new HashSet<Faction>();  
		Faction overLord = null;
	  HashSet<General> generals = new HashSet<General>();
    protected TextLib txtLib = Cons.GetTextLib();

	  public _Faction(bool isAI, int population) {
	  	this.isAI = isAI;
      this.population = population;
	  }

    public abstract string Name();
    public abstract string Description();
    public abstract Party[] GetParties();

    public bool IsAI() {
      return isAI;
    }

    public virtual void SetAs(bool _AI) {}

    // TODO: population grow/decrease

    // ==============================================================
    // ================= General ====================================
    // ==============================================================
    public void AddGeneral(General general) {
      generals.Add(general);
    }
	  
    public void RemoveGeneral(General general) {
      generals.Remove(general);
    }

    public General GetAvailableGeneral() {
      foreach (General general in generals)
      {
        if (general.stat == GeneralStat.Idle) {
          return general;
        }
      }
      return null;
    }
	  
    public int MaxUnitSize(Type unitType) {
      int num = (int)(population * (unitType == Type.Cavalry ? MaxCavUnitSizeRatio : MaxInfanUnitSizeRatio));
      if (unitType == Type.Cavalry) {
        return num > Cavalry.MaxTroopNum ? Cavalry.MaxTroopNum : num;
      }
      return num > Infantry.MaxTroopNum ? Infantry.MaxTroopNum : num;
    }

    // ==============================================================
    // ================= Faction Relation ===========================
    // ==============================================================
		public Faction OverLord() {
			return overLord;
		}

		public HashSet<Faction> SubLords() {
			return subLords;
		}

		public void AddSubLord(Faction faction){
			subLords.Add(faction);
		}

		public void RemSubLord(Faction faction) {
			subLords.Remove(faction);
		}

		public void SetOverLord(Faction faction) {
			overLord = faction;
		}

	}

  public class Liang: _Faction {
    Party[] parties = new Party[]{Cons.Tiger};
		public Liang(bool isAI, int population):base (isAI, population) {

    }
    public override string Name() {
      return txtLib.get("f_liang");
    }

    public override string Description() {
      return txtLib.get("f_liang_d");
    }

    public override Party[] GetParties() {
      return parties;
    }
  }

  public class Ghost: _Faction {
		public Ghost():base (true, 1000000) {}

    public override string Name() {
      return txtLib.get("f_ghost");
    }

    public override string Description() {
      return txtLib.get("f_ghost_d");
    }

    public override void SetAs(bool AI) {
      isAI = AI;
    }

    public override Party[] GetParties() {
      return new Party[]{};
    }
  }

	public class HeJian: _Faction {
    Party[] parties = new Party[]{Cons.Pigeon, Cons.Eagle};
		public HeJian(bool isAI, int population):base (isAI, population) {}

    public override string Name() {
      return txtLib.get("f_hejian");
    }

    public override string Description() {
      return txtLib.get("f_hejian_d");
    }

    public override Party[] GetParties() {
      return parties;
    }
  }

}