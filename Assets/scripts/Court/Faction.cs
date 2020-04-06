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
    Party[] GetParties();
    HashSet<Province> GetProvinces();
    void AddProvince(Province province);
    void RemoveProvince(Province province);
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
    public const int rgInfluence = 200;
    HashSet<Province> provinces = new HashSet<Province>();

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

    public HashSet<Province> GetProvinces() {
      return provinces;
    }

    public void AddProvince(Province province) {
      provinces.Add(province);
    }

    public void RemoveProvince(Province province) {
      provinces.Remove(province);
    }

	}

  public class Liao: _Faction {
    Party[] parties = new Party[]{Cons.NorthCourt, Cons.SouthCourt};
		public Liao(bool isAI, int population):base (isAI, population) {

    }
    public override string Name() {
      return txtLib.get("f_liao");
    }

    public override string Description() {
      return txtLib.get("f_liao_d");
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

	public class Song: _Faction {
    Party[] parties = new Party[]{Cons.NewParty, Cons.OldParty};
		public Song(bool isAI, int population):base (isAI, population) {}

    public override string Name() {
      return txtLib.get("f_song");
    }

    public override string Description() {
      return txtLib.get("f_song_d");
    }

    public override Party[] GetParties() {
      return parties;
    }
  }

  public class Xia: _Faction {
    Party[] parties = new Party[]{Cons.NoParty};
		public Xia(bool isAI, int population):base (isAI, population) {}

    public override string Name() {
      return txtLib.get("f_xia");
    }

    public override string Description() {
      return txtLib.get("f_xia_d");
    }

    public override Party[] GetParties() {
      return parties;
    }
  }


}