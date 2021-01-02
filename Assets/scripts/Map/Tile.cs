using System.Collections.Generic;
using System.Linq;
using PathFind;
using UnitNS;
using MonoNS;
using UnityEngine;
using CourtNS;

namespace MapTileNS
{
  public class Tile : Hex, PFTile, DataModel
  {
    public const int Work2BuildCamp = 20;
    public const float HighGround = 0.312f;
    public const float VantageGround = 0.8f;
    public Flood flood = null;
    public WildFire wildFire = null;
    public Epidemic epidemic = null;
    public Poision poision = null;
    public DeadZone deadZone = null;
    public bool waterBound = false;
    public bool burnable = false;
    public bool vantagePoint = false;
    public SiegeWall siegeWall = null;

    public Tile(int q, int r, HexMap hexMap) : base(q, r, hexMap) { }
    public void PostCreation()
    {
      weatherGenerator = hexMap.weatherGenerator;
      turnController = hexMap.turnController;
      windGenerator = hexMap.windGenerator;
      settlementMgr = hexMap.settlementMgr;
      eventDialog = hexMap.eventDialog;
      settlementAniController = hexMap.settlementAniController;
      if (terrian != TerrianType.Mountain) {
        flood = new Flood(this);
      }
      if (terrian != TerrianType.Water) {
        wildFire = new WildFire(this, burnable);
        deadZone = new DeadZone(this);
      }
      if (terrian == TerrianType.Water) {
        poision = new Poision(this);
      }
      if (field == FieldType.Forest) {
        if ((Util.eq<Province>(hexMap.warProvince, Cons.heBei)
             || Util.eq<Province>(hexMap.warProvince, Cons.heDong)) && Cons.FiftyFifty()) {
          epidemic = new Epidemic(this);
        }
      }

      if (flood != null || wildFire != null || epidemic != null || deadZone != null) {
        hexMap.weatherGenerator.tileCB.Add(this);
      }

      foreach (Tile tile in neighbours) {
        if (tile.terrian == TerrianType.Water) {
          waterBound = true;
          break;
        }
      }
    }

    public Vector3 GetSurfacePosition() {
      float y = 0f;
      if (terrian == TerrianType.Hill || terrian == TerrianType.Mountain) {
        y = HighGround;
      }
      if (vantagePoint) {
        y = VantageGround;
      }
      return new Vector3(Position().x, y, Position().z);
    }
    // ==============================================================
    // ================= Information ================================
    // ==============================================================
    Tile[] _neighbours;
    public Tile[] neighbours {
      get {
        if (_neighbours == null) {
          _neighbours = Neighbours<Tile>();
        }
        return _neighbours;
      }
      private set {}
    }

    // ==============================================================
    // ================= Listeners ==================================
    // ==============================================================
    public WindGenerator windGenerator;
    public WeatherGenerator weatherGenerator;
    public TurnController turnController;

    public void ListenOnTurnEnd(TurnController.OnNewTurn onNewTurn)
    {
      turnController.onNewTurn -= onNewTurn;
      turnController.onNewTurn += onNewTurn;
    }

    public void RemoveTurnEndListener(TurnController.OnNewTurn onNewTurn)
    {
      turnController.onNewTurn -= onNewTurn;
    }

    // ==============================================================
    // ================= Disasters ==================================
    // ==============================================================
    public HashSet<Tile> SetFire()
    {
      if (wildFire != null) {
        return wildFire.Start();
      }
      return new HashSet<Tile>();
    }

    public void Burn() {
      if (wildFire != null) {
        wildFire.BurnTile();
        if (deadZone != null) {
          deadZone.Clean();
        }
      }
    }

    public bool IsBurnable() {
      if (wildFire != null) {
        return wildFire.Burnable();
      }
      return false;
    }

    public HashSet<Tile> CreateFlood()
    {
      if (flood != null) {
        return flood.Start();
      }
      return new HashSet<Tile>();
    }

    public void Flood() {
      if (flood != null) {
        flood.FloodTile();
      }
    }

    public Unit[] Poision(Unit poisioner) {
      if (poision == null) {
        return new Unit[0];
      }
      return poision.SetPoision(poisioner);
    }

    // ==============================================================
    // ================= Terrian ====================================
    // ==============================================================
    public TerrianType terrian;
    public FieldType field;
    public bool isDam = false;

    public void SetFieldType(FieldType type)
    {
      field = type;
      if (type == FieldType.Burning) {
        burnable = false;
      }
      // TODO remove this
      hexMap.GetTileView(this).RefreshVisual();
    }

    public bool Accessible() {
      return terrian != TerrianType.Mountain && terrian != TerrianType.Water && 
      field != FieldType.Settlement && field != FieldType.Burning && field != FieldType.Flooding;
    }

    public bool Passable(bool isAI, bool forDeploy = false) {
      Unit u = GetUnit();
      bool deployable = u == null && Accessible();
      if (!deployable && u != null) {
        if (u.IsAI() == isAI) {
          if (!forDeploy) {
            deployable = true;
          }
        }
      }
      return deployable;
    }

    public void SetTerrianType(TerrianType type)
    {
      terrian = type;
    }

    public bool ignoreUnit = false;
    public override int GetCost(Unit unit, Mode mode)
    {
      //if (settlement != null && settlement.owner.isAI == unit.IsAI()
      //  && settlement.state == Settlement.State.normal)
      //{
      //  // settlement is passible
      //  return Unit.MovementcostOnPlain;
      //}
      int mov = Zone.Get(this, hexMap.GetWarParty(unit).cachedColorMap).Cost(unit);
      Unit u = GetUnit();
      if (u != null)
      {
        if (u.IsAI() != unit.IsAI() && !ignoreUnit) {
          return Unit.MovementCostOnUnaccesible;
        }
      }
      return mov;
    }

    public bool Deployable(Unit unit)
    {
      return Passable(unit.IsAI(), true);
    }

    public bool DeployableForPathFind(Unit unit) {
      return GetUnit() == null && Accessible();
    }

    public WindAdvantage GetGaleAdvantage(Tile target) {
      WindAdvantage advantage = WindAdvantage.NoAdvantage;
      if (Cons.IsGale(hexMap.windGenerator.current)) {
        if (hexMap.windGenerator.direction == Cons.Direction.dueNorth) {
          if(Util.eq<Tile>(target.SouthTile<Tile>(), this)) {
            advantage = WindAdvantage.Disadvantage;
          } else if(Util.eq<Tile>(target.NorthTile<Tile>(), this)) {
            advantage = WindAdvantage.Advantage;
          }
        }

        if (hexMap.windGenerator.direction == Cons.Direction.northEast) {
          if(Util.eq<Tile>(target.SouthWestTile<Tile>(), this)) {
            advantage = WindAdvantage.Disadvantage;
          } else if(Util.eq<Tile>(target.NorthEastTile<Tile>(), this)) {
            advantage = WindAdvantage.Advantage;
          }
        }

        if (hexMap.windGenerator.direction == Cons.Direction.northWest) {
          if(Util.eq<Tile>(target.SouthEastTile<Tile>(), this)) {
            advantage = WindAdvantage.Disadvantage;
          } else if(Util.eq<Tile>(target.NorthWestTile<Tile>(), this)) {
            advantage = WindAdvantage.Advantage;
          }
        }

        if (hexMap.windGenerator.direction == Cons.Direction.dueSouth) {
          if(Util.eq<Tile>(target.NorthTile<Tile>(), this)) {
            advantage = WindAdvantage.Disadvantage;
          } else if(Util.eq<Tile>(target.SouthTile<Tile>(), this)) {
            advantage = WindAdvantage.Advantage;
          }
        }

        if (hexMap.windGenerator.direction == Cons.Direction.southEast) {
          if(Util.eq<Tile>(target.NorthWestTile<Tile>(), this)) {
            advantage = WindAdvantage.Disadvantage;
          } else if(Util.eq<Tile>(target.SouthEastTile<Tile>(), this)) {
            advantage = WindAdvantage.Advantage;
          }
        }

        if (hexMap.windGenerator.direction == Cons.Direction.southWest) {
          if(Util.eq<Tile>(target.NorthEastTile<Tile>(), this)) {
            advantage = WindAdvantage.Disadvantage;
          } else if(Util.eq<Tile>(target.SouthWestTile<Tile>(), this)) {
            advantage = WindAdvantage.Advantage;
          }
        }
      }
      return advantage;
    }

    // ==============================================================
    // ================= Settlement =================================
    // ==============================================================
    public HashSet<Tile> linkedTilesForCamp = new HashSet<Tile>();

    public Settlement settlement
    {
      get
      {
        return _settlement;
      }
      set
      {
        _settlement = value;
        if (value != null) {
          SetFieldType(FieldType.Settlement);
        } else {
          SetFieldType(FieldType.Schorched);
        }
      }
    }
    Settlement _settlement;
    SettlementMgr settlementMgr;
    EventDialog eventDialog;
    SettlementAnimationController settlementAniController;
    public int Work2BuildSettlement()
    {
      if ((terrian == TerrianType.Plain || terrian == TerrianType.Hill) &&
        (field == FieldType.Wild || field == FieldType.Forest || field == FieldType.Flooded
         || field == FieldType.Schorched) && siegeWall == null)
      {
        return Work2BuildCamp;
      }
      return -1;
    }

    public void BuildDam()
    {
      SetTerrianType(TerrianType.Water);
      SetFieldType(FieldType.Wild);
      isDam = true;
    }

    // ==============================================================
    // ================= Units ======================================
    // ==============================================================
    HashSet<Unit> units;
    public void AddUnit(Unit u)
    {
      if (units == null)
      {
        units = new HashSet<Unit>();
      }
      units.Add(u);
    }

    public bool RemoveUnit(Unit u)
    {
      if (units == null || !units.Contains(u))
      {
        return false;
      }
      return units.Remove(u);
    }

    public Unit GetUnit()
    {
      if (units == null || units.Count == 0)
      {
        return null;
      }
      // just in case of wargame, there is hidden unit on the tile, or in case there is an unit moving through so war game unit will be last one
      return units.Last();
    }

    public int UnitCount() {
      return units == null ? 0 : units.Count;
    }

    public HashSet<Unit> GetUnits() {
      return units;
    }

    public Tile Escape() {
      Unit unit = GetUnit();
      List<Tile> tiles = new List<Tile>();
      foreach (Tile tile in neighbours)
      {
        if (tile.Deployable(unit))
        {
          tiles.Add(tile);
        }
      }
      if (tiles.Count == 0) {
        return null;
      }
      foreach(Tile tile in tiles) {
        if (!DownstreamTiles<Tile>().Contains(tile)) {
          return tile;
        }
      }

      return tiles[Util.Rand(0, tiles.Count-1)];
    }

    // * PathFind interfaces *
    public PFTile[] GetNeighbourTiles()
    {
      return neighbours;
    }

    public int AggregateCostToEnter(int costSoFar, PFTile sourceTile, PFUnit unit, Mode mode)
    {
      return ((Unit)unit).AggregateCostToEnterTile(this, costSoFar, mode);
    }

    public Tile FindDeployableTile(Unit unit, int cnt = 5) {
      Tile t = null;
      foreach(Tile tile in neighbours) {
        if (tile.Deployable(unit)) {
          t = tile;
          break;
        }
      }
      return t != null || cnt == 0 ? t : neighbours[0].FindDeployableTile(unit, cnt--); 
    }
  }

}