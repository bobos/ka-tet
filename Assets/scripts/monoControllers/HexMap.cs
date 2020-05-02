using System.Collections.Generic;
using UnityEngine;
using UnitNS;
using MapTileNS;
using FieldNS;
using CourtNS;

namespace MonoNS
{
  public class HexMap : MonoBehaviour
  {
    // all controllers
    public TurnController turnController;
    public WeatherGenerator weatherGenerator;
    public WindGenerator windGenerator;
    public SettlementMgr settlementMgr;
    public MsgBox msgBox;
    public ActionController actionController;
    public MouseController mouseController;
    public CombatController combatController;
    public CameraKeyboardController cameraKeyboardController;
    public UnitSelectionPanel unitSelectionPanel;
    public SettlementViewPanel settlementViewPanel;
    public WeatherIndicator weatherIndicator;
    public WargameController wargameController;
    public HoverInfo hoverInfo;
    public TurnIndicator turnIndicator;
    public TurnPhaseTitle turnPhaseTitle;
    public UnitActionBroker actionBroker;
    public InputField inputField;
    public EventDialog eventDialog;
    public EventDialogAlt eventDialogAlt;
    public Dialogue dialogue;
    public SettlementAnimationController settlementAniController;
    public UnitAnimationController unitAniController;
    public TileAnimationController tileAniController;
    public PopTextAnimationController popAniController;
    public ImgLibrary imgLibrary;
    public EventStasher eventStasher;
    public Province warProvince;
    public Tile[] frontier = new Tile[]{};
    public Tile[] middleField = new Tile[]{};
    public Tile[] keyPos = new Tile[]{};
    public Tile theBox;

    public Tile attackerReserveTile;

    // ==============================================================
    // ================= Interfaces required from hex map plugin ====
    // ==============================================================
    public Hex GetHex(int x, int y)
    {
      if (tiles == null)
      {
        Util.Throw("map not initiated yet");
      }
      if (x < 0 || y < 0 || x > numCols - 1 || y > numRows - 1) return null;
      return (Hex)(tiles[x, y]);
    }

    // ==============================================================
    // ==============================================================
    public enum RangeType
    {
      movement,
      attack,
      camp,
      supplyRange,
      PoisionRange
    }
    public int numRows = 40;
    public int numCols = 40;
    public Material MatOcean;
    public Material MatPlain;
    public Material MatLessPlain;
    public Material MatGrassland;
    public Material MatMountain;
    public Material MatBurning;
    public Material MatSchorched;
    public Material MatFlooded;
    public Material MovementRange;
    public Material AttackRange;
    public Material CampRange;
    public Material SupplyRange;
    public Material PoisionRange;
    public Material PlayerSkin;
    public Material GreySkin;
    public Material AISkin;
    public Material UnitHightlight;
    public Material OverLayMat;
    public Material TransMat;
    public Material WarningMat;
    public Material SupplyRouteMat;
    [System.NonSerialized] public float HeightMountain = 0.33f, HeightHill = 0.2f, HeightFlat = 0f;
    public GameObject InfantryPrefab;
    public GameObject CavalryPrefab;
    public GameObject HexPrefab;
    public GameObject HighGroundPrefab;
    public GameObject MountainRootPrefab;
    public GameObject MountainPeakPrefab;
    public GameObject MountainPrefab;
    public GameObject TentPrefab;
    public GameObject CampPrefab;
    public GameObject NameTextPrefab;
    public GameObject UnitInfoPrefab;
    public GameObject PopTextPrefab;
    public GameObject SiegeWallPrefab;
    public WarParty[] warParties = new WarParty[2];
    public List<GameObject> lineCache = new List<GameObject>();
    public List<GameObject> lineLabels = new List<GameObject>();

    public Tile[,] tiles;
    Tile[] _allTiles;
    public Tile[] allTiles {
      get {
        if (_allTiles == null) {
          List<Tile> all = new List<Tile>();
          foreach(Tile tile in tiles) {
            all.Add(tile);
          }
          _allTiles = all.ToArray();
        }
        return _allTiles;
      }
    }

    //bool updateReady = false;
    Tile[] highlightedArea;
    LineRenderer lineRenderer;
    Dictionary<Tile, GameObject> tile2GO;
    Dictionary<Unit, GameObject> unit2GO = new Dictionary<Unit, GameObject>();

    public Tile GetTile(int x, int y)
    {
      return (Tile)GetHex(x, y);
    }

    List<Tile> _DefenderZone;
    List<Tile> _AttackerZone;
    public List<Tile> DefenderZone {
      get {
        if (_DefenderZone == null) {
          _DefenderZone = new List<Tile>();
          foreach(Tile tile in
            GetHex(numCols - 1, numRows - 1).GetNeighboursWithinRange(2, (Tile _tile) => true)) {
            if (tile.Accessible()) { _DefenderZone.Add(tile); }
          }
        }
        return _DefenderZone;
      }
    }

    public List<Tile> AttackerZone {
      get {
        if (_AttackerZone == null) {
          _AttackerZone = new List<Tile>();
          foreach(Tile tile in
            GetHex(1, 1).GetNeighboursWithinRange(2, (Tile _tile) => true)) {
            if (tile.Accessible()) { _AttackerZone.Add(tile); }
          }
        }
        return _AttackerZone;
      }
    }

    List<Tile> _zone;
    public List<Tile> UndeploymentZone {
      get {
        if (_zone == null) {
          _zone = new List<Tile>();
          foreach(Tile tile in
            GetHex(1, 1).GetNeighboursWithinRange(11, (Tile _tile) => true)) {
              _zone.Add(tile);
          }
        }
        return _zone;
      }
    }

    Tile[] _initZone;
    public Tile[] InitPlayerDeploymentZone() {
      if (_initZone == null) {
        List<Tile> ts = new List<Tile>();
        for (int x = 0; x < numCols; x++)
        {
          for (int y = 0; y < numRows; y++)
          {
            Tile tile = tiles[x, y];
            if ((tile.Accessible() || tile.settlement != null) && !UndeploymentZone.Contains(tile)) {
              ts.Add(tile);
            }
          }
        }
        _initZone = ts.ToArray();
      }
      return _initZone;
    }

    // Use this for initialization
    public bool deployDone;
    public int turnNum;
    public void PreGameInit(int turnNum = 1)
    {
      turnController = GameObject.FindObjectOfType<TurnController>();
      weatherGenerator = GameObject.FindObjectOfType<WeatherGenerator>();
      windGenerator = GameObject.FindObjectOfType<WindGenerator>();
      settlementMgr = GameObject.FindObjectOfType<SettlementMgr>();
      msgBox = GameObject.FindObjectOfType<MsgBox>();
      actionController = GameObject.FindObjectOfType<ActionController>();
      mouseController = GameObject.FindObjectOfType<MouseController>();
      combatController = GameObject.FindObjectOfType<CombatController>();
      cameraKeyboardController = GameObject.FindObjectOfType<CameraKeyboardController>();
      unitSelectionPanel = GameObject.FindObjectOfType<UnitSelectionPanel>();
      settlementViewPanel = GameObject.FindObjectOfType<SettlementViewPanel>();
      weatherIndicator = GameObject.FindObjectOfType<WeatherIndicator>();
      hoverInfo = GameObject.FindObjectOfType<HoverInfo>();
      turnIndicator = GameObject.FindObjectOfType<TurnIndicator>();
      turnPhaseTitle = GameObject.FindObjectOfType<TurnPhaseTitle>();
      inputField = GameObject.FindObjectOfType<InputField>();
      eventDialog = GameObject.FindObjectOfType<EventDialog>();
      eventDialogAlt = GameObject.FindObjectOfType<EventDialogAlt>();
      dialogue = GameObject.FindObjectOfType<Dialogue>();
      settlementAniController = GameObject.FindObjectOfType<SettlementAnimationController>();
      unitAniController = GameObject.FindObjectOfType<UnitAnimationController>();
      tileAniController = GameObject.FindObjectOfType<TileAnimationController>();
      popAniController = GameObject.FindObjectOfType<PopTextAnimationController>();
      eventStasher = GameObject.FindObjectOfType<EventStasher>();
      wargameController = GameObject.FindObjectOfType<WargameController>();
      imgLibrary = GameObject.FindObjectOfType<ImgLibrary>();
      // init actionBroker
      actionBroker = UnitActionBroker.GetBroker();
      actionBroker.onUnitAction += OnUnitAction;

      GenerateMap();
      lineRenderer = transform.GetComponentInChildren<LineRenderer>();
      lineRenderer.startWidth = 0.1f;
      lineRenderer.endWidth = 0.1f;
      this.turnNum = turnNum;
      //updateReady = true;
    }

    public void PostGameInit()
    {
      foreach (Tile h in tiles)
      {
        h.PostCreation();
      }
    }

    bool CanDeploy(Tile tile) {
      return tile.Accessible() && tile.GetUnit() == null;
    }

    bool DeployAt(Tile tile, General general) {
      if (CanDeploy(tile)) {
        general.EnterCampaign(this, tile);
        return true;
      }

      foreach (Tile t in tile.GetNeighboursWithinRange(2, (Tile _tile) => true)) {
        if (CanDeploy(t)) {
          general.EnterCampaign(this, t);
          break;
        } 
      }

      return general.IsOnField();
    }

    void RandomDeployAttacker(General general, Tile baseTile) {
      foreach(Tile tile in baseTile.settlement.myTiles) {
        if (CanDeploy(tile)) {
          general.EnterCampaign(this, tile);
          break;
        }
      }
    }

    bool DeployAtSettlement(Tile tile, General general) {
      if (tile.settlement == null) {
        if (tile.GetUnit() != null) {
          return false;
        }
        general.EnterCampaign(this, tile);
        return true;
      }

      if (tile.settlement.HasRoom()) {
       general.EnterCampaign(this, tile);
       return true;
      }

      foreach (Tile t in tile.GetNeighboursWithinRange(2, (Tile _tile) => true)) {
        if (CanDeploy(t)) {
          general.EnterCampaign(this, t);
          break;
        } 
      }

      return general.IsOnField();
    }

    void RandomDeploy(General general) {
      List<Tile> tiles = new List<Tile>(frontier);
      foreach(Tile tile in middleField) {
        tiles.Add(tile);
      }
      foreach(Tile tile in keyPos) {
        tiles.Add(tile);
      }
      tiles.Add(theBox);
      Tile[] all = tiles.ToArray();

      if (DeployAtSettlement(all[Util.Rand(0, all.Length - 1)], general)) {
        return;
      }

      foreach(Tile tile in all) {
        if (DeployAtSettlement(tile, general)) {
          break;
        }
      }

      if (general.IsOnField()) {
        return;
      }

      foreach(Tile tile in settlementMgr.GetControlledTiles(general.faction.IsAI())) {
        if (CanDeploy(tile)) {
          general.EnterCampaign(this, tile);
          break;
        }
      }
    }

    public void InitDefendersOnMap(General[] defenders) {
      // init defender first
      General commander = defenders[0];
      foreach(General g in defenders) {
        if (g.Is(Cons.conservative)) {
          if (DeployAtSettlement(theBox, g)) { continue; }
          foreach(Tile t in middleField) {
            if (DeployAtSettlement(t, g)) { break; }
          }
          if (!g.IsOnField()) { RandomDeploy(g); };
        } else if (g.Is(Cons.loyal)) {
          int len = frontier.Length - 1;
          if (DeployAtSettlement(frontier[Util.Rand(0, len)], g)) {
            continue;
          }

          foreach(Tile t in frontier) {
            if (DeployAtSettlement(t, g)) { break; }
          }

          if (g.IsOnField()) { continue; }

          foreach(Tile t in keyPos) {
            if (DeployAtSettlement(t, g)) { break; }
          }

          if (!g.IsOnField()) { RandomDeploy(g); };
        } else if (g.Is(Cons.cunning)) {
          Tile[] ts = Cons.FairChance() ? keyPos : middleField;
          int len = ts.Length - 1;
          if (DeployAtSettlement(ts[Util.Rand(0, len)], g)) {
            continue;
          }

          foreach(Tile t in ts) {
            if (DeployAtSettlement(t, g)) { break; }
          }

          if (!g.IsOnField()) { RandomDeploy(g); };
        } else if (g.Is(Cons.brave)) {
          Tile[] ts = Cons.EvenChance() ? keyPos : frontier;
          int len = ts.Length - 1;
          if (DeployAtSettlement(ts[Util.Rand(0, len)], g)) {
            continue;
          }

          foreach(Tile t in ts) {
            if (DeployAtSettlement(t, g)) { break; }
          }

          if (!g.IsOnField()) { RandomDeploy(g); };
        } else {
          RandomDeploy(g);
        }
      }
      GetWarParty(commander.faction).AssignCommander(commander);
    }

    public class Army {
      public General leader;
      public List<General> lightCavalries = new List<General>();
      public List<General> heavyCavalries = new List<General>();
      public List<General> infantries = new List<General>();
      public List<General> all = new List<General>();
      public int point;
    }

    Army[] GetArmies(List<General> generals) {
      int armyNum;
      Army[] armies;
      if (generals.Count < 7) {
        armyNum = 1;
        armies = new Army[]{new Army()};
      } else if (generals.Count < 13) {
        armyNum = 2;
        armies = new Army[]{new Army(), new Army()};
      } else {
        armyNum = 3;
        armies = new Army[]{new Army(), new Army(), new Army()};
      }

      // Shuffle
      General[] all = generals.ToArray();
      for (int i = 0; i < all.Length; i++)
      {
        int pos = Util.Rand(0, all.Length-1);
        General tmp = all[pos];    
        all[pos] = all[i];
        all[i] = tmp; 
      }

      int index = 0;
      foreach(Army army in armies) {
        for (int i = 0; i < 5; i++)
        {
          if (index == all.Length) {
            break;
          }
          General general = all[index++];
          if (general.commandUnit.type == Type.Infantry) {
            army.infantries.Add(general);
          } else if (general.commandUnit.rank == Cons.veteran) {
            army.heavyCavalries.Add(general);
          } else {
            army.lightCavalries.Add(general);
          }
          army.all.Add(general);
        }
        if (index == all.Length) {
          break;
        }
      }

      int armyIndex = 0;
      for (int i = index; i < all.Length; i++) {
        armyIndex = armyIndex == armyNum ? 0 : armyIndex;
        General general = all[i];
        if (general.commandUnit.type == Type.Infantry) {
          armies[armyIndex].infantries.Add(general);
        } else if (general.commandUnit.rank == Cons.veteran) {
          armies[armyIndex].heavyCavalries.Add(general);
        } else {
          armies[armyIndex].lightCavalries.Add(general);
        }
        armies[armyIndex].all.Add(general);
        armyIndex++;
      }

      // find leader
      foreach(Army army in armies) {
        General leader = null;
        int score = 0;
        foreach(General general in army.all) {
          int point = (int)(general.commandUnit.soldiers * ( 1 + general.commandUnit.lvlBuf));
          army.point += point;
          if (Util.eq<General>(GetWarParty(general.faction).commanderGeneral, general)) {
            army.leader = general;
          }
          if (point > score) {
            score = point;
            leader = general;
          }
        }
        if (army.leader == null) {
          army.leader = leader;
        }
      }

      return armies;
    }

    public void InitAttackersOnMap(General[] attackers, Tile baseTile) {
      General commander = attackers[0];
      // 0: total random deployed at all frontier, strategybase and tiles between
      // 1: 1 cavary in one of frontier area, main army settled at middle sections, most 2 cavalries
      // 2: 2 or 3 groups, each consists one cavalry and 4 infantries, first one at frontier area, then middle sections
      // 3: weakest two at strategybase, rest at one of frontier area

      int aiType = 0;
      if (commander.commandSkill.commandSkill == 1) {
        aiType = 0;
      } else {
        aiType = Util.Rand(1, 3);
      }

      List<Tile> frontierArea = new List<Tile>();
      float score = 10000f;
      foreach(Tile tile in frontier) {
        Tile candidate = null;
        foreach (Tile t in tile.GetNeighboursWithinRange(6, (Tile _tile) => true)) {
          if (CanDeploy(t)) {
            float dist = Tile.Distance(t, baseTile);
            if (dist < score) {
              score = dist;
              candidate = t;
            }
          } 
        }
        frontierArea.Add(candidate);
      }

      score = 0f;
      List<Tile> all = new List<Tile>(frontierArea){};
      all.Add(attackerReserveTile);
      all.Add(baseTile);
      Tile[] array = all.ToArray();

      General weakest = commander;
      int num = System.Int32.MaxValue;
      foreach(General g in attackers) {
        if (g.commandUnit.type == Type.Cavalry) {
          continue;
        }
        int cp = (int)(g.commandUnit.soldiers * (1 + g.commandUnit.lvlBuf));
        if (cp < num) {
          num = cp;
          weakest = g;
        }
      }
      weakest.EnterCampaign(this, baseTile);
      List<General> rest = new List<General>();
      List<General> cavalries = new List<General>(); 
      List<General> infantries = new List<General>(); 
      foreach(General g in attackers) {
        if (Util.eq<General>(g, weakest)) {
          continue;
        }
        if (g.commandUnit.type == Type.Cavalry) {
          cavalries.Add(g);
        } else {
          infantries.Add(g);
        }
        rest.Add(g);
      }

      if (aiType == 0) {
        int len = array.Length - 1;
        foreach(General general in rest) {
          if(DeployAt(all[Util.Rand(0, len)], general)) { continue; }
          RandomDeployAttacker(general, baseTile);
        }
        return;
      }

      if (aiType == 1) {
        Tile target = frontierArea.FindLast((Tile tile) => true);
        int index = 0;
        General[] cavalryArray = cavalries.ToArray();
        for (int i = 0; i < 2; i++)
        {
          if (index == cavalryArray.Length) {
            break;
          }
          General general = cavalryArray[index++];
          if(DeployAt(target, general)) { continue; }
          RandomDeployAttacker(general, baseTile);
        }

        for (int i = index; i < cavalryArray.Length; i++) {
          General general = cavalryArray[i];
          if(DeployAt(attackerReserveTile, general)) { continue; }
          RandomDeployAttacker(general, baseTile);
        }

        foreach(General g in infantries) {
          if(DeployAt(attackerReserveTile, g)) { continue; }
          RandomDeployAttacker(g, baseTile);
        }
      }

      Tile[] fronts = frontierArea.ToArray();
      if (aiType == 2) {
        Army[] armies = GetArmies(rest);
        Army reserve = null;
        int point = System.Int32.MaxValue;
        foreach(Army army in armies) {
          if (army.point < point) {
            reserve = army;
            point = army.point;
          }
        }

        // deploy reserve at middle tile
        foreach(General general in reserve.all) {
          if(DeployAt(attackerReserveTile, general)) { continue; }
          RandomDeployAttacker(general, baseTile);
        }

        // deploy army at front
        int index = 0;
        foreach(Army army in armies) {
          if (Util.eq<Army>(army, reserve)) {
            continue;
          }
          if (index == fronts.Length) {
            index = 0;
          }
          Tile target = fronts[index++];
          foreach(General general in army.all) {
            if(DeployAt(target, general)) { continue; }
            RandomDeployAttacker(general, baseTile);
          }
        }
      }

      if (aiType == 3) {
        General weak = null;
        num = System.Int32.MaxValue;
        foreach(General g in rest) {
          int cp = (int)(g.commandUnit.soldiers * (1 + g.commandUnit.lvlBuf));
          if (cp < num) {
            num = cp;
            weak = g;
          }
        }
        weak.EnterCampaign(this, baseTile);

        foreach(General g in rest) {
          if (Util.eq<General>(g, weak)) {
            continue;
          }
          if(DeployAt(fronts[0], g)) { continue; }
          if(DeployAt(attackerReserveTile, g)) { continue; }
          RandomDeployAttacker(g, baseTile);
        }
      }
    }

    public void SetWarParties(WarParty p1, WarParty p2) {
      warParties[0] = p1.isAI ? p2 : p1;
      warParties[1] = p1.isAI ? p1 : p2;
    }

    public WarParty GetPlayerParty() {
      return warParties[0];
    }

    public WarParty GetAIParty() {
      return warParties[1];
    }

    public int FoodPerManPerTurn(bool isAI) {
      return IsAttackSide(isAI) ? 2 : 1;
    }

    public bool IsAttackSide(bool isAI) {
      WarParty wp = isAI ? GetAIParty() : GetPlayerParty();
      return wp.attackside;
    }

    public WarParty GetWarParty(Unit unit, bool counterParty = false) {
      return counterParty ?
      (unit.IsAI() ? GetPlayerParty() : GetAIParty()) :
      (unit.IsAI() ? GetAIParty() : GetPlayerParty());
    }

    public WarParty GetWarParty(Faction faction) {
      return faction.IsAI() ? GetAIParty() : GetPlayerParty();
    }

    public WarParty GetWarParty() {
      return turnController.playerTurn ? GetPlayerParty() : GetAIParty();
    }

    public void CaptureHorse(Unit unit, int num) {
      GetWarParty(unit).CaptureHorse(num);
    }

    public HashSet<Tile> GetRangeForDiscoveryWarning(bool isAI) {
      return GetEnemyRange(isAI, 0);
    }

    public HashSet<Tile> GetRangeForDiscoveryCheck(bool isAI) {
      return GetEnemyRange(isAI, 1);
    }

    public HashSet<Tile> GetRangeForGuardCheck(bool isAI) {
      return GetEnemyRange(isAI, 2);
    }

    HashSet<Tile> GetEnemyRange(bool isAI, int type) {
      // type: 0 for discovery warning, 1 for discovery check, 2 for guarded tile check
      HashSet<Tile> enemyScoutArea = new HashSet<Tile>();
      WarParty party = isAI ? GetPlayerParty() : GetAIParty();
      foreach (Unit u in party.GetUnits())
      {
        foreach(Tile t in (type == 0 || type == 1) ? u.GetScoutArea() : u.tile.neighbours) {
          if (type == 0 && u.IsConcealed()) {
            continue;
          }
          enemyScoutArea.Add(t);
        }
      }
      return enemyScoutArea;
    }

    List<GameObject> supplyLines = new List<GameObject>();
    public void DrawSupplyLine(Tile from, Tile to) {
      GameObject myLine = new GameObject();
      supplyLines.Add(myLine);
      myLine.transform.position = tile2GO[from].transform.position + (Vector3.up * 0.5f);
      myLine.AddComponent<LineRenderer>();
      LineRenderer lr = myLine.GetComponent<LineRenderer>();
      lr.material = MatFlooded;
      lr.startWidth = 0.1f;
      lr.endWidth = 0.1f;
      Vector3[] ps = new Vector3[2];
      ps[0] = tile2GO[from].transform.position + (Vector3.up * 0.5f);
      ps[1] = tile2GO[to].transform.position + (Vector3.up * 0.5f);
      lr.positionCount = 2;
      lr.SetPositions(ps);
    }

    public void CleanSupplyLines() {
      foreach(GameObject line in supplyLines) {
        GameObject.Destroy(line);
      }
      supplyLines = new List<GameObject>();
    }

    public void CreateArrow(Tile[] path, Material mat, string label, Color kolor)
    {
      if (path.Length == 0) return;
      CreateLine(path, mat);
      if (label != "") {
        GameObject unitInfoGO = (GameObject)Instantiate(UnitInfoPrefab,
            tile2GO[path[0]].transform.position,
            Quaternion.identity, tile2GO[path[0]].transform);
        unitInfoGO.GetComponent<UnitInfoView>().SetStr(label, kolor);
        lineLabels.Add(unitInfoGO);
      }
    }

    void CreateLine(Tile[] path, Material mat) {
      GameObject myLine = new GameObject();
      lineCache.Add(myLine);
      myLine.transform.position = tile2GO[path[0]].transform.position + (Vector3.up * 0.5f);
      myLine.AddComponent<LineRenderer>();
      LineRenderer lr = myLine.GetComponent<LineRenderer>();
      //lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
      lr.material = mat;
      lr.startWidth = 0.4f;
      lr.endWidth = 0.01f;
      Vector3[] ps = new Vector3[path.Length];
      for (int i = 0; i < path.Length; i++)
      {
        GameObject GO = tile2GO[path[i]];
        ps[i] = GO.transform.position + (Vector3.up * 0.5f);
      }
      lr.positionCount = path.Length;
      lr.SetPositions(ps);
    }

    public void CleanLines()
    {
      foreach (GameObject line in lineCache)
      {
        GameObject.Destroy(line);
      }
      foreach (GameObject label in lineLabels) {
        GameObject.Destroy(label);
      }
      foreach (UnitView view in toggledUnitViews) {
        if (view != null) {
          view.ToggleText(true);
        }
      }
      lineCache = new List<GameObject>();
      lineLabels = new List<GameObject>();
      toggledUnitViews = new List<UnitView>();
    }

    public void DehighlightPath()
    {
      lineRenderer.enabled = false;
    }

    public void HighlightPath(Tile[] tiles)
    {
      lineRenderer.enabled = true;
      Vector3[] ps = new Vector3[tiles.Length];
      for (int i = 0; i < tiles.Length; i++)
      {
        GameObject GO = tile2GO[tiles[i]];
        ps[i] = new Vector3(GO.transform.position.x, Tile.VantageGround, GO.transform.position.z);
      }
      lineRenderer.positionCount = tiles.Length;
      lineRenderer.SetPositions(ps);
    }

    public void UpdateTileVisuals()
    {
      for (int x = 0; x < numCols; x++)
      {
        for (int y = 0; y < numRows; y++)
        {
          Tile tile = tiles[x, y];
          CreateTileGO(tile);
        }
      }
      StaticBatchingUtility.Combine(this.gameObject);
    }

    public void AddTiles2Settlements()
    {
      for (int x = 0; x < numCols; x++)
      {
        for (int y = 0; y < numRows; y++)
        {
          Tile tile = tiles[x, y];
          if (tile.Accessible()) {
            settlementMgr.Add2Settlement(tile);
          }
        }
      }
    }

    private void SetTroopSkin(GameObject view, Material skin) {
      MeshRenderer[] mrs = view.GetComponentsInChildren<MeshRenderer>();
      foreach (MeshRenderer mr in mrs) {
        mr.material = skin;
      }
    }

    public void DarkenUnit(Unit unit) {
      if (unit == null) return;
      if (!unit2GO.ContainsKey(unit))
      {
        // unit has encamped
        return;
      }
      GameObject view = unit2GO[unit];
      SetTroopSkin(view, GreySkin);
    }

    public void SetUnitSkin(Unit unit)
    {
      if (unit == null) return;
      if (!unit2GO.ContainsKey(unit))
      {
        // unit has encamped
        return;
      }
      GameObject view = unit2GO[unit];
      if (unit.TurnDone())
      {
        DarkenUnit(unit);
      }
      else
      {
        SetTroopSkin(view, 
        unit.IsAI() ? AISkin : PlayerSkin);
      }
    }

    public void OnUnitAction(Unit unit, ActionType type, Tile tile) {
      if (type == ActionType.UnitLeft || type == ActionType.UnitDestroyed) {
        DestroyUnitView(unit);
        return;
      }

      if (type == ActionType.UnitHidden) {
        DeactivateUnitView(unit);
      }

      if (type == ActionType.UnitVisible) {
        if (!ActivateUnitView(unit)) {
          if (tile != null) CreateUnitViewAt(unit, tile);
        }
      }
    }

    public void OnWargameMove(Unit unit, Tile tile) {
      DestroyUnitView(unit);
      CreateUnitViewAt(unit, tile);
    }
    
    public void HighlightUnit(Unit unit)
    {
      if (unit == null && unit2GO.ContainsKey(unit)) return;
      SetTroopSkin(unit2GO[unit], UnitHightlight);
    }

    public void TargetUnit(Unit unit) {
      if (unit == null && unit2GO.ContainsKey(unit)) return;
      SetTroopSkin(unit2GO[unit], unit.IsVulnerable() ? CampRange : AttackRange);
    }

    public void UnhighlightUnit(Unit unit)
    {
      if (unit == null && unit2GO.ContainsKey(unit)) return;
      SetTroopSkin(unit2GO[unit], unit.IsAI() ? AISkin : PlayerSkin);
    }

    List<UnitView> toggledUnitViews = new List<UnitView>();
    public void ShowAttackArrow(Unit fromUnit, Unit toUnit, UnitPredict predict) {
      string txt = "";
      if (!fromUnit.IsCamping()) {
        txt = fromUnit.GeneralName() + "\n" + predict.joinPossibility + "%\n" + UnitInfoView.Shorten(predict.operationPoint)
          + "(" + predict.percentOfEffectiveForce + "%)" +
          (predict.windAdvantage ? "↑↑↑" : (predict.windDisadvantage ? "↓↓": ""));
        UnitView view = GetUnitView(fromUnit);
        toggledUnitViews.Add(view);
        view.ToggleText(false);
      }
      CreateArrow(new Tile[]{fromUnit.tile, toUnit.tile}, MatBurning, txt,
        predict.joinPossibility >= 70 ? Color.cyan : Color.red);
    }

    public void ShowDefendArrow(Unit fromUnit, Unit toUnit, UnitPredict predict) {
      string txt = fromUnit.GeneralName() + "\n" + predict.joinPossibility + "%\n"
        + UnitInfoView.Shorten(predict.operationPoint) + "(" + predict.percentOfEffectiveForce + "%)";
      UnitView view = GetUnitView(fromUnit);
      toggledUnitViews.Add(view);
      view.ToggleText(false);
      CreateArrow(new Tile[]{fromUnit.tile, toUnit.tile}, MatOcean, txt,
        predict.joinPossibility >= 70 ? Color.cyan : Color.red);
    }

     public void ShowDefenderStat(Unit defender, UnitPredict predict) {
       string txt = defender.GeneralName() + "\n" + predict.joinPossibility + "%\n"
         + UnitInfoView.Shorten(predict.operationPoint) + "(" + predict.percentOfEffectiveForce + "%)";
       UnitView view = GetUnitView(defender);
       toggledUnitViews.Add(view);
       view.ToggleText(false);
       GameObject tileGO = tile2GO[defender.tile];
       GameObject unitInfoGO = (GameObject)Instantiate(UnitInfoPrefab,
           tileGO.transform.position,
           Quaternion.identity, tileGO.transform);
       unitInfoGO.GetComponent<UnitInfoView>().SetStr(txt, Color.cyan);
       lineLabels.Add(unitInfoGO);
    }

    void CreateTileGO(Tile tile)
    {
      GameObject prefab = null;
      FieldType fieldType = FieldType.Wild;
      float elevation = tile.getElevation();
      if (elevation >= HeightMountain)
      {
        if (Cons.SlimChance()) {
          prefab = MountainPeakPrefab;
        } else {
          prefab = MountainPrefab;
        }
        fieldType = FieldType.Wild;
        tile.SetTerrianType(TerrianType.Mountain);
        if(Cons.SlimChance()) {
          tile.burnable = true;
        }
      }
      else if (elevation >= HeightHill)
      {
        if (!Cons.HighlyLikely()) {
          // vantage point
          prefab = MountainRootPrefab;
          tile.burnable = true;
          tile.SetTerrianType(TerrianType.Hill);
          fieldType = FieldType.Wild;
          tile.vantagePoint = true;
        } else {
          prefab = HighGroundPrefab;
          fieldType = Cons.MostLikely() ? FieldType.Wild : (Cons.MostLikely() ? FieldType.Forest : FieldType.Wild);
          tile.SetTerrianType(TerrianType.Hill);
          if(fieldType == FieldType.Wild && Cons.SlimChance()) {
            tile.burnable = true;
          }
        }
      }
      else if (elevation >= HeightFlat)
      {
        prefab = HexPrefab;
        tile.SetTerrianType(TerrianType.Plain);
        fieldType = Cons.FairChance() ? FieldType.Wild : (Cons.SlimChance() ? FieldType.Forest : FieldType.Wild);
      }
      else
      {
        prefab = HexPrefab;
        tile.SetTerrianType(TerrianType.Water);
        fieldType = FieldType.Wild;
      }
      GameObject tileGO = (GameObject)Instantiate(prefab, tile.Position(), Quaternion.identity, this.transform);
      View view = tileGO.GetComponentInChildren<TileView>();
      view.OnCreate(tile);
      tile2GO[tile] = tileGO;
      tileGO.name = tile.GetCoord().x + "," + tile.GetCoord().y;
      tile.SetFieldType(fieldType);
    }

    public TileView GetTileView(Tile tile) {
      if (tile == null || tile2GO == null || !tile2GO.ContainsKey(tile))
      {
        return null;
      }
      return tile2GO[tile].GetComponent<TileView>();
    }

    public void SpawnUnit(Unit unit)
    {
      if (unit == null)
      {
        return;
      }
      if(unit.IsVisible() || unit.IsConcealed()) {
        CreateUnitViewAt(unit, unit.tile);
        if (unit.IsConcealed() && unit.IsAI()) DeactivateUnitView(unit);
      }
    }

    public UnitView GetUnitView(Unit unit)
    {
      if (unit == null || unit2GO == null || !unit2GO.ContainsKey(unit))
      {
        return null;
      }
      return unit2GO[unit].GetComponent<UnitView>();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
    }

    void Overlay(Tile tile, Material mat) {
      GameObject tileGO = tile2GO[tile];
      MeshRenderer[] mrs = tileGO.GetComponentsInChildren<MeshRenderer>();
      foreach(MeshRenderer mr in mrs) {
        if (mr.name == "overlay") {
          mr.material = mat;
          break;
        }
      }
    }

    public void OverlayFoW(Tile tile) {
      Unit unit = tile.GetUnit();
      if (unit != null && !unit.IsConcealed()) {
        DeactivateUnitView(unit);
      }
      Overlay(tile, OverLayMat);
    }

    public void OverlaySupplyLine(Tile tile) {
      Overlay(tile, SupplyRouteMat);
    }

    public bool IsOverlayFoW(Tile tile) {
      GameObject tileGO = tile2GO[tile];
      MeshRenderer[] mrs = tileGO.GetComponentsInChildren<MeshRenderer>();
      foreach(MeshRenderer mr in mrs) {
        if (mr.name == "overlay") {
          if(mr.material.name == "OverlayMat (Instance)") {
            return true;
          }
          return false;
        }
      }
      return false;
    }

    public void OverlayDisable(Tile tile, bool markAsEnemyZone) {
      Unit unit = tile.GetUnit();
      if (unit != null) {
        ActivateUnitView(unit);
      }
      Overlay(tile, TransMat);
      TileView view = GetTileView(tile);
      view.RefreshVisual(markAsEnemyZone);
    }

    public void DehighlightArea()
    {
      if (highlightedArea == null) return;
      FoW.Get().Fog(highlightedArea);
      highlightedArea = null;
    }

    public void HighlightArea(Tile[] tiles, RangeType type, Unit unit = null)
    {
      DehighlightArea();
      HashSet<Tile> spotWarningRange = unit != null && unit.IsConcealed()
        ? GetRangeForDiscoveryWarning(unit.IsAI()) : null;
      HashSet<Tile> guardWarningRange = unit != null ? GetRangeForGuardCheck(unit.IsAI()) : null;
      Tile[] visible = unit != null ? unit.GetVisibleArea() : null;
      foreach (Tile tile in tiles)
      {
        Material mat = null;
        if (type == RangeType.attack) mat = AttackRange;
        if (type == RangeType.movement) {
          mat = MovementRange;
          if (unit != null && unit.IsConcealed() && spotWarningRange.Contains(tile)) {
            mat = WarningMat;
          }
          if (guardWarningRange.Contains(tile)) {
            bool found = false;
            foreach (Tile t in visible) {
              if (Util.eq<Tile>(tile, t)) {
                found = true;
                break;
              }
            }
            if (found) {
              mat = CampRange;
            }
          }
        }
        if (type == RangeType.camp) mat = CampRange;
        if (type == RangeType.supplyRange) mat = SupplyRange;
        if (type == RangeType.PoisionRange) mat = PoisionRange;
        Overlay(tile, mat);
      }
      highlightedArea = tiles;
    }

    public void DestroyUnitView(Unit unit)
    {
      if (unit2GO.ContainsKey(unit))
      {
        GameObject view = unit2GO[unit];
        view.GetComponent<UnitView>().Destroy();
        Destroy(view);
        unit2GO.Remove(unit);
      }
    }

    public void DeactivateUnitView(Unit unit)
    {
      if (unit2GO.ContainsKey(unit))
      {
        GameObject view = unit2GO[unit];
        foreach(MeshRenderer mr in view.GetComponentsInChildren<MeshRenderer>()) {
          mr.enabled = false;
        }
        view.GetComponent<UnitView>().Deactivate();
      } else {
        CreateUnitViewAt(unit, unit.tile);
        DeactivateUnitView(unit);
      }
    }

    public bool ActivateUnitView(Unit unit)
    {
      if (unit2GO.ContainsKey(unit))
      {
        GameObject view = unit2GO[unit];
        foreach(MeshRenderer mr in view.GetComponentsInChildren<MeshRenderer>()) {
          mr.enabled = true;
        }
        view.GetComponent<UnitView>().Activate();
        return true;
      }
      return false;
    }

    public void CreateUnitViewAt(Unit unit, Tile tile)
    {
      if (unit2GO.ContainsKey(unit)) {
        return;
      }
      GameObject prefab = unit.IsCavalry() ? CavalryPrefab : InfantryPrefab;
      GameObject tileGO = tile2GO[tile];
      //NOTE: spawn as child gameobject of hex
      Vector3 position = tile.GetSurfacePosition();
      Vector3 namePosition = UnitView.NamePosition(position);
      Vector3 unitInfoPosition = UnitView.UnitInfoPosition(position);
      GameObject unitGO = (GameObject)Instantiate(prefab,
          position,
          Quaternion.identity, tileGO.transform);
      UnitView view = unitGO.GetComponent<UnitView>();

      GameObject unitInfoGO = (GameObject)Instantiate(UnitInfoPrefab,
          unitInfoPosition,
          Quaternion.identity, tileGO.transform);
      view.unitInfoGO = unitInfoGO;
      view.OnCreate(unit);
      unit2GO[unit] = unitGO;
      SetUnitSkin(unit);
    }

    UnitView GetView(Unit unit) {
      if (!unit2GO.ContainsKey(unit)) {
        return null;
      }
      return unit2GO[unit].GetComponent<UnitView>();
    }

    public PopTextView ShowPopText(View view, string msg, Color color) {
      Vector3 p = view.transform.position;
      GameObject popGO = (GameObject)Instantiate(PopTextPrefab,
          new Vector3(p.x - 0.5f, p.y, p.z),
          Quaternion.identity, view.transform);
      PopTextView textView = popGO.GetComponent<PopTextView>();
      textView.Show(view, msg, color);
      return textView;
    }

    void GenerateMap()
    {
      tiles = new Tile[numCols, numRows];
      tile2GO = new Dictionary<Tile, GameObject>();

      for (int x = 0; x < numCols; x++)
      {
        for (int y = 0; y < numRows; y++)
        {
          Tile tile = new Tile(x, y, this);
          tile.setElevation(-0.3f);
          tiles[x, y] = tile;
          
          //hexGO.GetComponentInChildren<TextMesh>().text = x + "," + y;
        }
      }

      float noiseResolution = 0.3f;
      float noiseScale = 1.1f; //larger value, more islands
      Random.InitState(1);
      Vector2 noiseRandom = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
      for (float col = 0; col < numCols; col++)
      {
        for (float row = 0; row < numRows; row++)
        {
          Tile h = GetTile((int)col, (int)row);
          h.setElevation((Mathf.PerlinNoise(col / (float)numCols / noiseResolution + noiseRandom.x,
          row / (float)numRows / noiseResolution + noiseRandom.y) - 0.35f) * noiseScale);
        }
      }

      UpdateTileVisuals();
    }
  }
}