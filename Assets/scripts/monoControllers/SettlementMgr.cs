using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnitNS;
using MapTileNS;
using FieldNS;
using System.Collections;
using TextNS;

namespace MonoNS
{
  public class SettlementMgr : BaseController
  {
    public int attackerLaborDead = 0;
    public int defenderLaborDead = 0;
    public int totalMaleDead = 0;
    public int totalFemaleDead = 0;
    public int totalChildDead = 0;

    public enum QueueJobType {
      DistSupply,
      DistLabor
    }

    // Use this for initialization
    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      attackerRoots = new List<Settlement>();
      defenderRoots = new List<Settlement>();
      buildingQueue = new List<Building>();
      mouseController = hexMap.mouseController;
      popAniController = hexMap.popAniController;
      actionController = hexMap.actionController;
      cc = hexMap.cameraKeyboardController;
      ghostUnit = GhostUnit.createGhostUnit();
    }

    public override void UpdateChild() {}

    HashSet<Tile> campableTiles;

    public void SetCampableField(HashSet<Tile> tiles) {
      campableTiles = tiles;
    }

    // TODO: just for tmp test
    public void BuildRoad(Tile from, Tile to) {
      List<Tile> path = new List<Tile>(); 
      foreach(Tile tile in ghostUnit.FindPath(from, to, true)) {
        if (!Util.eq<Tile>(tile, from) && !Util.eq<Tile>(tile, to)) {
          tile.BuildRoad();
          path.Add(tile);
        }
      }
      Tile[] road = path.ToArray();
      from.roads[to] = road;
      to.roads[from] = road;
    }

    class DistJob {
      public Settlement from;
      public Settlement to;
      public int amount;
      public QueueJobType type;

      public DistJob(Settlement from, Settlement to, int amount, QueueJobType type) {
        this.from = from;
        this.to = to;
        this.amount = amount;
        this.type = type;
      }
    }

    List<DistJob> attackerDistJobs = new List<DistJob>();
    List<DistJob> defenderDistJobs = new List<DistJob>();
    public void AddDistJob(Settlement from, Settlement to, int amount, QueueJobType type) {
      DistJob job = new DistJob(from, to, amount, type);
      if (from.owner.attackside) {
        attackerDistJobs.Add(job);
      } else {
        defenderDistJobs.Add(job);
      }
    }

    public bool turnEndOngoing = false;
    public void TurnEnd(WarParty warParty) {
      turnEndOngoing = true;
      StartCoroutine(PerformTurnEnd(warParty));
    }

    public View GetView(Building settlement) {
      if (!settlement2GO.ContainsKey(settlement)) return null;
      return settlement2GO[settlement].GetComponent<View>();
    }

    TextLib textLib = Cons.GetTextLib();
    CameraKeyboardController cc;
    PopTextAnimationController popAniController;
    IEnumerator PerformTurnEnd(WarParty warParty)
    {
      foreach (Building settlement in buildingQueue.ToArray())
      {
        if (Util.eq<WarParty>(warParty, settlement.owner))
        {
          if (settlement.TurnEnd()) {
            popAniController.Show(GetView(settlement), textLib.get("pop_builded"), Color.green);
            while (popAniController.Animating) { yield return null; }
          }
        }
      }
      
      List<DistJob> jobs = warParty.attackside ? attackerDistJobs : defenderDistJobs;
      foreach(DistJob job in jobs) {
        if (!job.from.IsFunctional() || !job.to.IsFunctional()) {
          popAniController.Show(GetView(job.from),
            textLib.get(
              job.type == QueueJobType.DistSupply ?
              "pop_failedToDistSupply" : "pop_failedToDistLabor"),
            Color.white);
          while (popAniController.Animating) { yield return null; }
          continue;
        }
        // ambush supply caravans
        if(!job.from.GetReachableSettlements().Contains(job.to)) {
          hexMap.eventDialog.Show(new MonoNS.Event(EventDialog.EventName.SupplyRouteBlocked, null, job.from, 0, 0, 0, 0, 0, job.to));
          while (hexMap.eventDialog.Animating) { yield return null; }
          continue;
        }
        Tile[] route = job.from.baseTile.roads[job.to.baseTile];
        Unit ambusher = IsSupplyRouteAmbushed(route, warParty.isAI);
        if (ambusher != null) {
          // There is enemy unit ambushed on the supply route
          if (job.type == QueueJobType.DistSupply) {
            job.from.SupplyIntercepted(ambusher, job.to, job.amount);
          } else {
            job.from.LaborIntercepted(ambusher, job.to, job.amount);
          }
          while (hexMap.eventDialog.Animating) { yield return null; }
          continue;
        }

        if (job.type == QueueJobType.DistSupply) {
          job.from.DistSupply(job.amount, job.to);
        } else {
          job.from.DistLabor(job.amount, job.to);
        }
        while (hexMap.eventDialog.Animating) { yield return null; }
      }

      if (warParty.attackside) {
        attackerDistJobs = new List<DistJob>();
      } else {
        defenderDistJobs = new List<DistJob>();
      }

      List<Settlement> roots = warParty.attackside ? attackerRoots : defenderRoots;
      HashSet<Unit> units = warParty.GetUnits();
      foreach (Settlement root in roots)
      {
        if (root.state != Settlement.State.constructing) {
          View view = GetView(root);
          List<List<Unit>> failedUnits = root.ReduceSupply();
          foreach(Unit u in failedUnits[0]) {
            popAniController.Show(view, 
              System.String.Format(textLib.get("pop_failedToSupplyUnitInSettlement"), u.GeneralName()),
              Color.yellow);
            while (popAniController.Animating) { yield return null; }
          }

          foreach(Unit u in failedUnits[1]) {
            popAniController.Show(hexMap.GetUnitView(u), 
              textLib.get("pop_failedToSupplyUnitNearby"),
              Color.yellow);
            while (popAniController.Animating) { yield return null; }
          }
        }
        root.availableLabor = root.labor;
      }

      turnEndOngoing = false;
    }

    public Unit IsSupplyRouteAmbushed(Tile[] route, bool isAI) {
      foreach (Tile tile in route) {
        if (tile.IsThereConcealedEnemy(isAI)) {
          return tile.GetUnit();
        }
      }
      return null;
    }

    ActionController actionController;
    MouseController mouseController;
    GhostUnit ghostUnit;
    public List<Settlement> attackerRoots;
    public List<Settlement> defenderRoots;
    List<Building> buildingQueue;

    public bool BuildCamp(Tile location, WarParty warParty, Unit unit) {
      return BuildSettlement(location.name, location, Settlement.Type.camp, warParty,
        0, 0, 0, 0, 0,
        location.storageLevel, 1, unit);
    }

    public bool BuildStrategyBase(Tile location, WarParty warParty, int supply, int labor) {
      return BuildSettlement(
        System.String.Format(
          textLib.get("settlement_strategyBase"),
          warParty.faction.Name()
        ),
        location, Settlement.Type.strategyBase, warParty, supply, 0, 0, 0, labor, 3, 1, null);
    }

    public bool BuildCity(string name, Tile location,
      WarParty warParty,
      int wallLevel,
      int male, int female, int child, int labor, int supply) {
      return BuildSettlement(
        name,
        location,
        Settlement.Type.city,
        warParty,
        supply, male, female, child, labor, 3, wallLevel, null);
    }

    public bool BuildSiegeWall(Tile location, WarParty warParty) {
      if(location.Work2BuildSettlement() == -1) return false; // unbuildable tile
      SiegeWall siegeWall = new SiegeWall(location, warParty);
      siegeWall.onBuildingReady += this.SettlementReady;
      buildingQueue.Add(siegeWall);
      CreateSiegeWall(siegeWall);
      location.siegeWall = siegeWall;
      return true;
    }

    private bool BuildSettlement(
      string name, Tile location, Settlement.Type type, WarParty warParty,
      int supply, int male, int female, int child, int labor,
      int storageLevel,
      int wallLevel, Unit unit)
    {
      if(location.Work2BuildSettlement() == -1) return false; // unbuildable tile
      Settlement settlement = null;
      if (type == Settlement.Type.camp)
      {
        settlement = new Camp(name, location, warParty, storageLevel);
        settlement.onBuildingReady += this.SettlementReady;
        buildingQueue.Add(settlement);
      }
      else
      {
        if (type == Settlement.Type.city)
        {
          settlement = new City(name, location, warParty, supply, male, female,
            child, labor, wallLevel);
        }
        if (type == Settlement.Type.strategyBase)
        {
          settlement = new StrategyBase(name, location, warParty, supply, labor);
        }
      }

      if (warParty.attackside)
      {
        attackerRoots.Add(settlement);
      }
      else
      {
        defenderRoots.Add(settlement);
      }

      CreateSettlement(settlement);
      settlement.Encamp(unit);

      if (type != Settlement.Type.camp) SettlementReady(settlement);
      return true;
    }

    Dictionary<Building, GameObject> settlement2GO = new Dictionary<Building, GameObject>();
    void CreateSettlement(Settlement settlement)
    {
      Vector3 position = settlement.baseTile.GetSurfacePosition();
      GameObject GO = (GameObject)Instantiate(settlement.type == Settlement.Type.camp ? hexMap.CampPrefab : hexMap.TentPrefab,
        position,
        Quaternion.identity,
        hexMap.GetTileView(settlement.baseTile).transform);
      SettlementView view = GO.GetComponent<SettlementView>();
      view.OnCreate(settlement);
      view.SetNameGO((GameObject)Instantiate(hexMap.NameTextPrefab,
        SettlementView.NamePosition(position),
        Quaternion.identity, hexMap.GetTileView(settlement.baseTile).transform));
      settlement2GO[settlement] = GO;
    }

    void CreateSiegeWall(SiegeWall siegeWall)
    {
      Tile tile = siegeWall.baseTile;
      Vector3 position = tile.GetSurfacePosition();
      GameObject GO = (GameObject)Instantiate(hexMap.SiegeWallPrefab,
        position,
        Quaternion.identity,
        hexMap.GetTileView(tile).transform);
      SiegeWallView view = GO.GetComponent<SiegeWallView>();
      view.OnCreate(siegeWall);
      settlement2GO[siegeWall] = GO;
    }

    public void SettlementReady(Building settlement)
    {
      buildingQueue.Remove(settlement);
      SetBuildingSkinReady(GetView(settlement).gameObject);
    }

    void SetBuildingSkinReady(GameObject view) {
      MeshRenderer[] mrs = view.GetComponentsInChildren<MeshRenderer>();
      foreach (MeshRenderer mr in mrs) {
        mr.material = hexMap.MatSchorched;
      }
    }

    public int[] OccupySettlement(Unit unit, Settlement settlement) {
      int[] deathNum = new int[3]{0,0,0};
      settlement.owner = unit.IsAI() ? hexMap.GetAIParty() : hexMap.GetPlayerParty();
      settlement.Encamp(unit);
      if (settlement.owner.attackside) {
        attackerRoots.Add(settlement);
        defenderRoots.Remove(settlement);
        if (settlement.type == Settlement.Type.city) {
          // labor returns to male
          int returnedMale = (int)(settlement.labor * 0.9f);
          settlement.civillian_male += returnedMale;
          settlement.labor -= returnedMale;

          int maleDead = unit.rf.soldiers * 2;
          int femaleDead = unit.rf.soldiers;
          int childDead = (int)(unit.rf.soldiers * 0.05f);
          maleDead = maleDead > settlement.civillian_male ? settlement.civillian_male : maleDead;
          femaleDead = femaleDead > settlement.civillian_female ? settlement.civillian_female : femaleDead;
          childDead = childDead > settlement.civillian_child ? settlement.civillian_child : childDead;

          settlement.civillian_male -= maleDead;
          settlement.civillian_female -= femaleDead;
          settlement.civillian_child -= childDead;

          totalMaleDead += maleDead;
          totalFemaleDead += femaleDead;
          totalChildDead += childDead;

          deathNum[0] = maleDead;
          deathNum[1] = femaleDead;
          deathNum[2] = childDead;
        } else {
          int laborDead = (int)(settlement.labor * 0.7f);
          defenderLaborDead += laborDead;
          settlement.labor -= laborDead;

          deathNum[0] = laborDead;
        }
      } else {
        defenderRoots.Add(settlement);
        attackerRoots.Remove(settlement);
        if (settlement.type != Settlement.Type.city) {
          int laborDead = (int)(settlement.labor * 0.8f);
          attackerLaborDead += laborDead;
          settlement.labor -= laborDead;

          deathNum[0] = laborDead;
        }
      }

      return deathNum;
    }

    public HashSet<Tile> GetSupplyRangeTiles(Settlement settlement) {
      SetGhostOwner(settlement);
      HashSet<Tile> ret = new HashSet<Tile>();
      if (!settlement.IsNormal()) {
        ret.Add(settlement.baseTile);
        return ret;
      }
      foreach(Tile tile in ghostUnit.GetAccessibleTilesForSupply(settlement.baseTile,
                            GetGhostUnitRangeForSettlementSupply())) {
        ret.Add(tile);
      }
      return ret;
    }

    public Tile[] GetCampableFields() {
      List<Tile> tiles = new List<Tile>();
      foreach(Tile tile in campableTiles) {
        if (tile.Work2BuildSettlement() != -1) {
          tiles.Add(tile);
        }
      }
      return tiles.ToArray();
    }

    public void DestroySettlement(Settlement camp)
    {
      buildingQueue.Remove(camp);
      if (camp.owner.attackside)
      {
        attackerRoots.Remove(camp);
      }
      else
      {
        defenderRoots.Remove(camp);
      }
      if (camp.owner.attackside) {
        attackerLaborDead += camp.labor;
      } else {
        defenderLaborDead += camp.labor;
      }
    }

    public void DestroySiegeWall(SiegeWall sw)
    {
      buildingQueue.Remove(sw);
    }

    public void GetVisibleArea(bool attackSide, HashSet<Tile> tiles) {
      foreach(Settlement settlement in attackSide ? attackerRoots : defenderRoots) {
        foreach(Tile tile in settlement.GetVisibleArea()) {
          tiles.Add(tile);
        }
      }
    }

    int GetGhostUnitRangeForSettlementSupply()
    {
      return (int)(ghostUnit.GetFullMovement() * 1.3f);
    }

    void SetGhostOwner(Settlement settlement) {
      ghostUnit.SetPlayer(!settlement.owner.isAI);
    }

    public static bool Ready4Refresh = false;
  }

}