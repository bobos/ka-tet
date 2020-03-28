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
    public EventStasher eventStasher;
    public Province warProvince;

    // ==============================================================
    // ================= Interfaces required from hex map plugin ====
    // ==============================================================
    public Hex GetHex(int x, int y)
    {
      if (tiles == null)
      {
        Debug.LogError("not initiated");
        throw new UnityException("map not initiated yet");
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
    public int numRows = 20;
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
    [System.NonSerialized] public float HeightMountain = 0.33f, HeightHill = 0.28f, HeightFlat = 0f;
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

    public void SetWarParties(WarParty p1, WarParty p2) {
      warParties[0] = p1.isAI ? p2 : p1;
      warParties[1] = p1.isAI ? p1 : p2;
      deployDone = GetPlayerParty().attackside || turnNum != 1;
    }

    public WarParty GetPlayerParty() {
      return warParties[0];
    }

    public WarParty GetAIParty() {
      return warParties[1];
    }

    public int FoodPerTenMenPerTurn(bool isAI) {
      return IsAttackSide(isAI) ? 2 : 1;
    }

    public bool IsAttackSide(bool isAI) {
      WarParty wp = isAI ? GetAIParty() : GetPlayerParty();
      return wp.attackside;
    }

    public WarParty GetWarParty(Unit unit) {
      return unit.IsAI() ? GetAIParty() : GetPlayerParty();
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
      SetTroopSkin(unit2GO[unit], AttackRange);
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
          (predict.windAdvantage ? "↑↑↑" : (predict.windDisadvantage ? "↓↓": "")) +
         (fromUnit.GetStaminaLevel() == StaminaLvl.Tired ? "☹"
          : (fromUnit.GetStaminaLevel() == StaminaLvl.Exhausted ? "☹☹" : ""));
        UnitView view = GetUnitView(fromUnit);
        toggledUnitViews.Add(view);
        view.ToggleText(false);
      }
      CreateArrow(new Tile[]{fromUnit.tile, toUnit.tile}, MatBurning, txt,
        predict.joinPossibility >= 70 ? Color.cyan : Color.red);
    }

    public void ShowDefendArrow(Unit fromUnit, Unit toUnit, UnitPredict predict) {
      string txt = fromUnit.GeneralName() + "\n" + predict.joinPossibility + "%\n"
        + UnitInfoView.Shorten(predict.operationPoint) + "(" + predict.percentOfEffectiveForce + "%)" +
       (fromUnit.GetStaminaLevel() == StaminaLvl.Tired ? "☹"
        : (fromUnit.GetStaminaLevel() == StaminaLvl.Exhausted ? "☹☹" : ""));
      UnitView view = GetUnitView(fromUnit);
      toggledUnitViews.Add(view);
      view.ToggleText(false);
      CreateArrow(new Tile[]{fromUnit.tile, toUnit.tile}, MatOcean, txt,
        predict.joinPossibility >= 70 ? Color.cyan : Color.red);
    }

     public void ShowDefenderStat(Unit defender, UnitPredict predict) {
       string txt = defender.GeneralName() + "\n" + predict.joinPossibility + "%\n"
         + UnitInfoView.Shorten(predict.operationPoint) + "(" + predict.percentOfEffectiveForce + "%)" +
        (defender.GetStaminaLevel() == StaminaLvl.Tired ? "☹"
         : (defender.GetStaminaLevel() == StaminaLvl.Exhausted ? "☹☹" : ""));
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
          fieldType = Cons.MostLikely() ? FieldType.Wild : (Cons.TinyChance() ? FieldType.Village : FieldType.Wild);
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
        fieldType = Cons.FairChance() ? FieldType.Wild : (Cons.TinyChance() ? FieldType.Village : FieldType.Wild);
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

    public void OverlayDisable(Tile tile, HashSet<Tile> enemyZone = null) {
      HashSet<Tile> zone = enemyZone;
      if (zone == null) {
        zone = FoW.Get().GetVisibleArea(true);
      }
      Unit unit = tile.GetUnit();
      if (unit != null && !unit.IsConcealed()) {
        ActivateUnitView(unit);
      }
      if (FoW.Get() == null ||
        FoW.Get().GetVisibleArea().Contains(tile)) {
        Overlay(tile, TransMat);
        TileView view = GetTileView(tile);
        view.RefreshVisual();
        if (zone.Contains(tile)) {
          view.RefreshVisual(true);
        }
      } else {
        OverlayFoW(tile);
      }
    }

    public void DehighlightArea()
    {
      if (highlightedArea == null) return;
      foreach (Tile tile in highlightedArea)
      {
        OverlayDisable(tile);
      }
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