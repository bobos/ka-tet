using System.Collections.Generic;
using System.Linq;
using PathFind;
using UnitNS;
using MonoNS;
using NatureNS;

namespace MapTileNS
{
  public class Tile : Hex, PFTile
  {
    public const float PlainHeightStart = 0f;
    public const float PlainHeightEnd = 0.75f;
    public const float HillHeightStart = 0.5f;
    public const float HillHeightEnd = 1.0f;
    public const float HighgroundWatermark = 0.65f;
    //public const int Work2BuildCamp = 34;
    public const int Work2BuildCamp = 10;
    public const int BurningLasts = 6; 
    public const int FloodingLasts = 15;

    public Tile(int q, int r, HexMap hexMap) : base(q, r, hexMap) { }
    public void PostCreation()
    {
      weatherGenerator = hexMap.weatherGenerator;
      turnController = hexMap.turnController;
      windGenerator = hexMap.windGenerator;
      settlementMgr = hexMap.settlementMgr;
      if (isDam)
      {
        ListenOnHeavyRain();
      }
      if (terrian == TerrianType.Hill)
      {
        ListenOnHeat();
      }
      if (terrian == TerrianType.Mountain)
      {
        ListenOnDry();
      }
    }
    
    // ==============================================================
    // ================= Information ================================
    // ==============================================================
    // TODO: textLib
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

    public string GetFieldType()
    {
      string prefix = "";
      if (field == FieldType.Wild) prefix = "Wild";
      if (field == FieldType.Farm) prefix = "Farmed";
      if (field == FieldType.Settlement) prefix = "Settelment On";
      if (field == FieldType.Schorched) prefix = "Schorched";
      if (field == FieldType.Burning) prefix = "Burning";
      if (field == FieldType.Flooded) prefix = "Flooded";
      if (field == FieldType.Flooding) prefix = "Flooding";

      string suffix = "";
      if (terrian == TerrianType.Hill) suffix = "Hills";
      if (terrian == TerrianType.Mountain) suffix = "Mountain";
      if (terrian == TerrianType.Plain) suffix = "Plain";
      if (terrian == TerrianType.Water) suffix = "Water";

      return prefix + " " + suffix;
    }

    // ==============================================================
    // ================= On Turn End ================================
    // ==============================================================
    WindGenerator windGenerator;
    WeatherGenerator weatherGenerator;

    bool onHeavyRainEventRegistered = false;
    void ListenOnHeavyRain()
    {
      if (!onHeavyRainEventRegistered)
      {
        weatherGenerator.onHeavyRain += OnHeavyRain;
        onHeavyRainEventRegistered = true;
      }
    }

    void RemoveOnHeavyRainListener()
    {
      if (onHeavyRainEventRegistered)
      {
        weatherGenerator.onHeavyRain -= OnHeavyRain;
        onHeavyRainEventRegistered = false;
      }
    }

    bool onRainEventRegistered = false;
    void ListenOnRain()
    {
      if (!onRainEventRegistered)
      {
        weatherGenerator.onRain += OnRain;
        onRainEventRegistered = true;
      }
    }

    void RemoveOnRainListener()
    {
      if (onRainEventRegistered)
      {
        weatherGenerator.onRain -= OnRain;
        onRainEventRegistered = false;
      }
    }

    bool onHeatEventRegistered = false;
    void ListenOnHeat()
    {
      if (!onHeatEventRegistered)
      {
        weatherGenerator.onHeat += OnHeat;
        onHeatEventRegistered = true;
      }
    }

    void RemoveOnHeatListener()
    {
      if (onHeatEventRegistered)
      {
        weatherGenerator.onHeat -= OnHeat;
        onHeatEventRegistered = false;
      }
    }

    bool onDryEventRegistered = false;
    void ListenOnDry()
    {
      if (!onDryEventRegistered)
      {
        weatherGenerator.onDry += OnDry;
        onDryEventRegistered = true;
      }
    }

    void RemoveOnDryListener()
    {
      if (onDryEventRegistered)
      {
        weatherGenerator.onDry -= OnDry;
        onDryEventRegistered = false;
      }
    }

    bool seasonEventRegistered = false;
    void ListenOnSeason()
    {
      if (!seasonEventRegistered)
      {
        weatherGenerator.onSeasonChange += OnSeasonChange;
        seasonEventRegistered = true;
      }
    }

    void RemoveSeasonListener()
    {
      if (!seasonEventRegistered)
      {
        weatherGenerator.onSeasonChange -= OnSeasonChange;
        seasonEventRegistered = false;
      }
    }

    bool turnEndRegistered = false;
    TurnController turnController;
    void ListenOnTurnEnd()
    {
      if (!turnEndRegistered)
      {
        turnController.onNewTurn += OnTurnEnd;
        turnEndRegistered = true;
      }
    }

    void RemoveTurnEndListener()
    {
      if (turnEndRegistered)
      {
        turnController.onNewTurn -= OnTurnEnd;
        turnEndRegistered = false;
      }
    }

    public void OnTurnEnd()
    {
      if (field == FieldType.Burning)
      {
        burningCntDown--;
        if (burningCntDown < 1)
        {
          PutOutFire();
        }
      }
      else if (field == FieldType.Flooding)
      {
        floodingCntDown--;
        if (floodingCntDown < 1)
        {
          FloodRecede();
        }
      }
      hexMap.UpdateTileTextAndSkin(this);
    }

    public void OnRain()
    {
      if (field == FieldType.Burning)
      {
        PutOutFire();
      }
    }

    public void OnHeavyRain()
    {
      if (isDam && Cons.FairChance())
      {
        SabotageDam();
      }

      if (field == FieldType.Burning)
      {
        PutOutFire();
      }
    }

    public void OnHeat()
    {
      // heat in hill causes illness
      if (terrian == TerrianType.Hill && Cons.HighlyLikely())
      {
        if (settlement != null)
        {
          foreach (Unit u in settlement.garrison)
          {
            u.SetSickness(Util.Rand(5, 10) * 0.01f, Util.Rand(1, 4) * 0.01f);
          }
        }
        else
        {
          Unit u = GetUnit();
          if (u != null)
          {
            u.SetSickness(Util.Rand(2, 5) * 0.01f, Util.Rand(1, 2) * 0.01f);
          }
        }
      }
    }

    public void OnDry()
    {
      if (terrian == TerrianType.Mountain && Cons.SlimChance() && CanSetFire() && burnable)
      {
        SpreadFire();
      }
    }

    public void OnSeasonChange(Season season)
    {
      if (Cons.IsWinter(season))
      {
        if (field == FieldType.Burning)
        {
          PutOutFire();
        }
      }
    }

    // ==============================================================
    // ================= Wild Fire ==================================
    // ==============================================================
    public int burningCntDown = 0;
    public bool burnable = false;
    public bool SetFire()
    {
      if (Cons.IsSpring(weatherGenerator.season) || Cons.IsWinter(weatherGenerator.season))
      {
        return false;
      }

      if (Cons.IsRain(weatherGenerator.currentWeather) || Cons.IsHeavyRain(weatherGenerator.currentWeather))
      {
        return false;
      }

      if (!CanSetFire() || burnable)
      {
        return false;
      }
      
      SpreadFire();
      return true;
    }

    void SpreadFire()
    {
      DisaterAffectUnit();
      SetFieldType(FieldType.Burning);
      ListenOnTurnEnd();
      ListenOnSeason();
      ListenOnRain();
      ListenOnHeavyRain();
      // spread to neighbours depends on season and wind and directions
      Cons.Direction windDirection = windGenerator.direction;
      List<Tile> affectedTiles = new List<Tile>();
      bool chance1 = Cons.FairChance();
      bool chance2 = Cons.SlimChance();
      bool chance3 = Cons.SlimChance();
      if (Cons.IsGale(windGenerator.current))
      {
        chance1 = Cons.MostLikely();
        chance2 = Cons.EvenChance();
        chance3 = Cons.EvenChance();
      }
      else if (Cons.IsWind(windGenerator.current))
      {
        chance1 = Cons.EvenChance();
        chance2 = Cons.FairChance();
        chance3 = Cons.FairChance();
      }

      if (windDirection == Cons.Direction.dueNorth)
      {
        PickTile2Burn(affectedTiles, NorthTile<Tile>(), NorthEastTile<Tile>(),
                      NorthWestTile<Tile>(), chance1, chance2, chance3);
      }

      if (windDirection == Cons.Direction.dueSouth)
      {
        PickTile2Burn(affectedTiles, SouthTile<Tile>(), SouthEastTile<Tile>(),
                      SouthWestTile<Tile>(), chance1, chance2, chance3);
      }

      if (windDirection == Cons.Direction.northEast)
      {
        PickTile2Burn(affectedTiles, NorthEastTile<Tile>(), NorthTile<Tile>(),
                      SouthEastTile<Tile>(), chance1, chance2, chance3);
      }

      if (windDirection == Cons.Direction.northWest)
      {
        PickTile2Burn(affectedTiles, NorthWestTile<Tile>(), NorthTile<Tile>(),
                      SouthWestTile<Tile>(), chance1, chance2, chance3);
      }

      if (windDirection == Cons.Direction.southWest)
      {
        PickTile2Burn(affectedTiles, SouthWestTile<Tile>(), SouthTile<Tile>(),
                      NorthWestTile<Tile>(), chance1, chance2, chance3);
      }

      if (windDirection == Cons.Direction.southEast)
      {
        PickTile2Burn(affectedTiles, SouthEastTile<Tile>(), SouthTile<Tile>(),
                      NorthEastTile<Tile>(), chance1, chance2, chance3);
      }

      foreach (Tile tile in affectedTiles)
      {
        if (tile.CanSetFire() || (tile.terrian == TerrianType.Plain && tile.CanPlainCatchFire()))
        {
          tile.SpreadFire();
        }
      }
    }
    
    void PickTile2Burn(List<Tile> tiles, Tile tile1, Tile tile2, Tile tile3, bool chance1, bool chance2, bool chance3)
    {
      if (tile1 != null && chance1) tiles.Add(tile1);
      if (tile2 != null && chance2) tiles.Add(tile2);
      if (tile3 != null && chance3) tiles.Add(tile3);
    }

    void PutOutFire()
    {
      SetFieldType(FieldType.Schorched);
      RemoveTurnEndListener();
      RemoveOnHeavyRainListener();
      RemoveOnRainListener();
    }

    public bool CanSetFire()
    {
      if (field == FieldType.Wild && terrian == TerrianType.Hill || terrian == TerrianType.Mountain)
      {
        return true;
      }
      return false;
    }

    // Can the plain tile caught fire by nearby burning tiles
    public bool CanPlainCatchFire()
    {
      if (field == FieldType.Wild && Cons.IsAutumn(weatherGenerator.season))
      {
        return true;
      }
      return false;
    }

    // ==============================================================
    // ================= Flood ======================================
    // ==============================================================
    public int floodingCntDown = 0;

    public bool SabotageDam()
    {
      if (isDam &&
          (Cons.IsSpring(weatherGenerator.season) || Cons.IsSummer(weatherGenerator.season))
          && (Cons.IsRain(weatherGenerator.currentWeather) || Cons.IsHeavyRain(weatherGenerator.currentWeather))) 
      {
        // TODO: set Dam from outside
        SpreadFlood();
        return true;
      }
      return false;
    }

    public void SpreadFlood()
    {
      isDam = false;
      DisaterAffectUnit();
      foreach (Tile tile in DownstreamTiles<Tile>())
      {
        if (tile.terrian == TerrianType.Water || tile.field == FieldType.Flooding)
        {
          tile.SpreadFlood();
        }
        else if (tile.CanBeFloodedByNearByTile() &&
         (Cons.IsHeavyRain(weatherGenerator.currentWeather) ? Cons.HighlyLikely() : Cons.MostLikely()))
        {
          if (tile.field == FieldType.Burning)
          {
            tile.PutOutFire();
          }
          tile.SetFieldType(FieldType.Flooding);
          tile.ListenOnTurnEnd();
          tile.SpreadFlood();
        }
      }
    }

    public bool CanBeFloodedByNearByTile()
    {
      if ((terrian == TerrianType.Hill || terrian == TerrianType.Plain)
        && !isHighGround)
      {
        if (field == FieldType.Settlement)
        {
          // camp can be flooded
          if (settlement.type == Settlement.Type.camp)
          {
            return true;
          }
          return false;
        }
        else
        {
          return true;
        }
      }
      return false;
    }

    void FloodRecede()
    {
      SetFieldType(FieldType.Flooded);
      RemoveTurnEndListener();
    }

    // ==============================================================
    // ================= Terrian ====================================
    // ==============================================================
    public TerrianType terrian;
    public FieldType field;
    public bool isHighGround = true;
    public bool isDam = false;

    int movementCost = Unit.BasicMovementCost;
    public void SetFieldType(FieldType type)
    {
      field = type;
      if (type == FieldType.Burning)
      {
        burningCntDown = Util.Rand(2, BurningLasts);
      }
      if (type == FieldType.Schorched)
      {
        burningCntDown = 0;
      }

      if (type == FieldType.Flooding)
      {
        floodingCntDown = Util.Rand(2, FloodingLasts);
      }
      if (type == FieldType.Flooded)
      {
        floodingCntDown = 0;
      }

      if (type == FieldType.Settlement || type == FieldType.Burning || type == FieldType.Flooding)
      {
        movementCost = Unit.MovementCostOnUnaccesible;
      }
      else
      {
        UpdateMovementcost();
      }

      hexMap.UpdateTileTextAndSkin(this);
    }

    public bool Ambushable()
    {
      return terrian == TerrianType.Hill && field == FieldType.Wild;
    }

     // TODO
    public int GetVantagePoint()
    {
      return 0;
    }

    public void SetTerrianType(TerrianType type)
    {
      terrian = type;
      UpdateMovementcost();
      if (type == TerrianType.Hill)
      {
        // Higher possibility for hill to be a high ground
        isHighGround = Util.Rand(HillHeightStart, HillHeightEnd) > HighgroundWatermark;
      }
      else if (type == TerrianType.Plain)
      {
        isHighGround = Util.Rand(PlainHeightStart, PlainHeightEnd) > HighgroundWatermark;
      }
      else
      {
        isHighGround = type == TerrianType.Mountain ? true : false;
      }
    }

    private void UpdateMovementcost()
    {
      if (terrian == TerrianType.Hill)
      {
        movementCost = Unit.MovementcostOnHill;
      }
      else if (terrian == TerrianType.Plain)
      {
        movementCost = Unit.MovementcostOnPlain;
      }
      else
      {
        movementCost = Unit.MovementCostOnUnaccesible;
      }
    }

    public override int GetCost(Unit unit)
    {
      //if (settlement != null && settlement.owner.isAI == unit.IsAI()
      //  && settlement.state == Settlement.State.normal)
      //{
      //  // settlement is passible
      //  return Unit.MovementcostOnPlain;
      //}
      Unit u = GetUnit();
      if (u != null)
      {
        if (u.IsAI() != unit.IsAI() && !u.IsConcealed())
        {
          return Unit.MovementCostOnUnaccesible;
        }
      }

      int ret;
      if (unit.IsCavalry())
      {
        // apply movement modifier for calvary unit
        ret = (int)(movementCost * (terrian == TerrianType.Plain ?
                    Cavalry.MovementCostModifierOnPlain : Cavalry.MovementCostModifierOnHill));
      }
      else
      {
        ret = movementCost;
      }

      return ret > 0 ? ret : Unit.MovementCostOnUnaccesible;
    }

    public bool Deployable(Unit unit)
    {
      Unit u = GetUnit();
      bool deployable = u == null && movementCost != Unit.MovementCostOnUnaccesible;
      if (!deployable && IsThereConcealedEnemy(unit.IsAI())) {
        u.DiscoveredByEnemy();
      }
      return deployable;
    }

    public bool DeployableForPathFind(Unit unit) {
      return (GetUnit() == null || IsThereConcealedEnemy(unit.IsAI())) && movementCost != Unit.MovementCostOnUnaccesible;
    }

    public bool IsThereConcealedEnemy(bool isAI) {
      Unit u = GetUnit();
      return u != null && u.IsAI() != isAI && u.IsConcealed();
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
        SetFieldType(FieldType.Settlement);
      }
    }
    Settlement _settlement;
    SettlementMgr settlementMgr;
    public int Work2BuildSettlement()
    {
      if ((terrian == TerrianType.Plain || terrian == TerrianType.Hill) &&
        (field == FieldType.Wild || field == FieldType.Farm || field == FieldType.Flooded
         || field == FieldType.Schorched))
      {
        return Work2BuildCamp;
      }
      return -1;
    }

    public void RemoveCamp(bool setFire)
    {
      _settlement = null;
      if (setFire) SpreadFire();
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
      return units.First();
    }

    void DisaterAffectUnit()
    {
      if (field == FieldType.Settlement && settlement != null)
      {
        settlementMgr.DestroyCamp(settlement, false);
      }
      Unit u = GetUnit();
      if (u != null)
      {
        Tile retreatTile = null;
        foreach (Tile tile in Neighbours<Tile>())
        {
          if (tile.Deployable(u))
          {
            retreatTile = tile;
            break;
          }
        }
        if (retreatTile == null)
        {
          u.Destroy();
        }
        else
        {
          u.EscapeFromWildFire();
          u.SetTile(retreatTile);
        }
      }
    }

    // * PathFind interfaces *
    public PFTile[] GetNeighbourTiles()
    {
      List<PFTile> ret = new List<PFTile>();
      foreach (Tile h in Neighbours<Tile>())
      {
        ret.Add(h);
      }
      return ret.ToArray();
    }

    public int AggregateCostToEnter(int costSoFar, PFTile sourceTile, PFUnit unit)
    {
      return ((Unit)unit).AggregateCostToEnterTile(this, costSoFar);
    }
  }

}