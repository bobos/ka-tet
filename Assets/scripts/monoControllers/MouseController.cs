using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using UnitNS;
using MapTileNS;
using TextNS;

namespace MonoNS
{
  public class MouseController : BaseController
  {

    public override void PreGameInit(HexMap hexMap, BaseController me)
    {
      base.PreGameInit(hexMap, me);
      actionController = hexMap.actionController;
      actionController.onBtnClick += OnBtnClick;
      actionController.actionDone += ActionDone;
      tc = hexMap.turnController;
      hover = hexMap.hoverInfo;
      tc.onEndTurnClicked += OnEndTurnClicked;
      msgBox = hexMap.msgBox;
      settlementMgr = hexMap.settlementMgr;
      hexMap.eventDialog.eventDialogOn += EventDialogOn;
      hexMap.eventDialog.eventDialogOff += EventDialogOff;
      ResetUpdateFunc();
    }

    HoverInfo hover;
    MsgBox msgBox;
    TurnController tc;
    ActionController actionController;
    SettlementMgr settlementMgr;
    Tile tileUnderMouse;
    Tile[] selectedPath;
    Dictionary<Tile, Tile[]> paths = new Dictionary<Tile, Tile[]>();
    delegate void UpdateFunc();
    UpdateFunc Update_CurrentFunc;
    Vector3 lastMousePixelPosition;
    Vector3 lastMousePosition;
    int mouseDragThreshold = 4;
    Unit __selectedUnit = null;
    public mode mouseMode;
    public Unit selectedUnit
    {
      get { return __selectedUnit; }
      set
      {
        __selectedUnit = value;
      }
    }

    Settlement __selectedSettlement = null;
    public Settlement selectedSettlement
    {
      get { return __selectedSettlement; }
      set
      {
        __selectedSettlement = value;
      }
    }
    public LayerMask hexTileLayerMask;

    public void OnEndTurnClicked()
    {
      // we wish to clean up all highlights for next turn start
      mouseMode = mode.detect;
      Escape();
    }

    public void EventDialogOn() {
      updateReady = false;
    }

    public void EventDialogOff() {
      updateReady = true;
    }

    PopTextAnimationController popAniController {
      get {
        return hexMap.popAniController;
      }
    }
    TextLib textLib {
      get {
        return Cons.GetTextLib();
      }
    }

    public Settlement nearEnemySettlement = null;
    public Settlement nearMySettlement = null;
    public Tile nearDam = null;
    public List<Tile> nearFireTiles = null;
    public bool nearAlly = false;
    public bool nearEnemy = false;
    public bool nearWater = false;
    public List<Unit> nearbyEnemey = null;
    public Unit[] surpriseTargets = null;
    public List<Unit> falseOrderTargets = null;
    public List<Unit> alienateTargets = null;
    public Tile[] accessibleTiles = null;
    public HashSet<Unit> nearbyAlly = null;

    void ResetUnitSelection() {
      nearEnemySettlement = null;
      nearMySettlement = null;
      nearDam = null;
      nearAlly = false;
      nearEnemy = false;
      nearWater = false;
      nearbyEnemey = new List<Unit>();
      nearbyAlly = new HashSet<Unit>();
      nearFireTiles = new List<Tile>();
      surpriseTargets = new Unit[]{};
      accessibleTiles = new Tile[]{};
      falseOrderTargets = new List<Unit>();
      alienateTargets = new List<Unit>();
    }

    public void PrepareUnitSelection() {
      ResetUnitSelection();
      Settlement s = null;
      Tile t = selectedUnit != null ? selectedUnit.tile : selectedSettlement.baseTile;
      bool isAI = selectedUnit != null ? selectedUnit.IsAI() : selectedSettlement.owner.isAI;

      foreach(Tile tile in t.neighbours) {
        if (tile.settlement != null) {
          s = tile.settlement;
        }

        if (tile.burnable) {
          nearFireTiles.Add(tile);
        }

        if (tile.isDam) {
          nearDam = tile;
        }

        if (tile.terrian == TerrianType.Water) {
          nearWater = true;
        }

        Unit u = tile.GetUnit();
        if (u != null && u.IsAI() == isAI) {
          nearAlly = true;
          nearbyAlly.Add(u);
        }

        if (u != null && u.IsAI() != isAI) {
          nearEnemy = true;
          nearbyEnemey.Add(u);
        }

        if (tile.settlement != null && tile.settlement.owner.isAI != selectedUnit.IsAI()) {
          nearEnemy = true;
        }
      }

      if (s != null && isAI == s.owner.isAI) {
        nearMySettlement = s;
      }
      if (s != null && isAI != s.owner.isAI) {
        nearEnemySettlement = s;
      }
      
      if (selectedUnit != null) {
        surpriseTargets = selectedUnit.GetSurpriseTargets();
        accessibleTiles = selectedUnit.GetAccessibleTiles();
        Unit u = selectedUnit != null ? selectedUnit : hexMap.settlementViewPanel.selectedUnit;
        falseOrderTargets = u.GetFalseOrderTargets();
        alienateTargets = u.GetAlienateTargets();
      }
    }

    public void RefreshUnitPanel(Unit unit) {
      if (Util.eq<Unit>(unit, selectedUnit)) {
        PrepareUnitSelection();
        hexMap.unitSelectionPanel.ToggleButtons(true, unit);
      }
    }

    List<Unit> highlightEnemyUnits;
    public void OnBtnClick(ActionController.actionName action)
    {
      if (action == ActionController.actionName.MOVE)
      {
        mouseMode = mode.move;
        Update_CurrentFunc = UpdateUnitMovement;
      }

      if (action == ActionController.actionName.POISION)
      {
        mouseMode = mode.sabotage;
        Update_CurrentFunc = UpdateUnitPoision;
      }

      if (action == ActionController.actionName.ENCAMP)
      {
        nearMySettlement.Encamp(selectedUnit);
        Escape();
        return;
      }

      if (action == ActionController.actionName.RETREAT)
      {
        if (selectedUnit.CanRetreat()) {
          hexMap.actionController.retreat(selectedUnit);
        } else if (selectedUnit.SetRetreatPath()) {
          hexMap.actionController.ForceRetreat(selectedUnit);
        }
        Escape();
        return;
      }

      if (action == ActionController.actionName.SABOTAGE)
      {
        Tile tile = nearDam;
        if (selectedUnit.tile.siegeWall != null && selectedUnit.tile.siegeWall.owner.isAI != selectedUnit.IsAI()) {
          tile = null;
        }
        if(!actionController.sabotage(selectedUnit, tile)){
          // TODO
          Debug.LogError("Failed to sabotage, try again!");
        }
        return;
      }

      if (action == ActionController.actionName.FIRE)
      {
        mouseMode = mode.fire;
        Update_CurrentFunc = UpdateUnitSetFire;
        msgBox.Show("选择放火地点");
        hexMap.HighlightArea(nearFireTiles.ToArray(), HexMap.RangeType.camp);
      }

      if (action == ActionController.actionName.WARGAME)
      {
        hexMap.wargameController.StartWargame();
      }

      if (action == ActionController.actionName.WGCANCEL)
      {
        hexMap.wargameController.Cancel();
        Escape();
      }

      if (action == ActionController.actionName.ATTACK)
      {
        mouseMode = mode.attack;
        Update_CurrentFunc = UpdateUnitAttack;
        msgBox.Show("选择目标!");
        foreach(Unit u in nearbyEnemey) {
          hexMap.TargetUnit(u);
        }
      }

      if (action == ActionController.actionName.FeintDefeat)
      {
        mouseMode = mode.feint;
        Update_CurrentFunc = UpdateUnitFeintDefeat;
        msgBox.Show("选择目标!");
        foreach(Unit u in nearbyEnemey) {
          hexMap.TargetUnit(u);
        }
      }

      if (action == ActionController.actionName.SurpriseAttack)
      {
        if (surpriseTargets.Length == 0) {
          msgBox.Show("无可奇袭目标!");
          Escape();
        } else {
          mouseMode = mode.surpriseAttack;
          Update_CurrentFunc = UpdateUnitSurpriseAttack;
          msgBox.Show("选择目标!");
          foreach(Unit u in surpriseTargets) {
            hexMap.TargetUnit(u);
          }
        }
      }

      if (action == ActionController.actionName.FalseOrder)
      {
        if (falseOrderTargets.Count == 0) {
          msgBox.Show("无可迷惑目标!");
          Escape();
        } else {
          mouseMode = mode.falseOrder;
          Update_CurrentFunc = UpdateUnitFalseOrder;
          msgBox.Show("选择目标!");
          foreach(Unit u in falseOrderTargets) {
            hexMap.TargetUnit(u);
          }
        }
      }

      if (action == ActionController.actionName.Alienate)
      {
        if (alienateTargets.Count == 0) {
          msgBox.Show("无可离间目标!");
          Escape();
        } else {
          mouseMode = mode.alienate;
          Update_CurrentFunc = UpdateUnitAlienate;
          msgBox.Show("选择目标!");
          foreach(Unit u in alienateTargets) {
            hexMap.TargetUnit(u);
          }
        }
      }

      if (action == ActionController.actionName.CHARGE)
      {
        mouseMode = mode.attack;
        Update_CurrentFunc = UpdateUnitCharge;
        msgBox.Show("选择目标!");
        foreach(Unit u in nearbyEnemey) {
          if (u.CanBeShaked(selectedUnit) > 0) {
            hexMap.TargetUnit(u);
          }
        }
      }

      if (action == ActionController.actionName.Skirmish)
      {
        mouseMode = mode.attack;
        Update_CurrentFunc = UpdateUnitSkirmish;
        msgBox.Show("选择目标!");
        foreach(Unit u in nearbyEnemey) {
          if (u.CanBeWaved()) {
            hexMap.TargetUnit(u);
          }
        }
      }

      if (action == ActionController.actionName.Breakthrough)
      {
        mouseMode = mode.attack;
        Update_CurrentFunc = UpdateUnitBreakThrough;
        msgBox.Show("选择突破目标!");
        foreach(Unit u in nearbyEnemey) {
          if (u.CanBeShaked(selectedUnit) > 0) {
            hexMap.TargetUnit(u);
          }
        }
      }

      if (action == ActionController.actionName.REPOS)
      {
        mouseMode = mode.repos;
        Update_CurrentFunc = UpdateUnitRepos;
        msgBox.Show("选择目标!");
        foreach(Unit u in nearbyAlly) {
          hexMap.TargetUnit(u);
        }
      }

      if (action == ActionController.actionName.Forecast)
      {
        hexMap.actionController.Forecast(selectedUnit);
        Escape();
      }

      if (action == ActionController.actionName.INPUTCONFIRM)
      {
        int _supply = hexMap.inputField.GetInput();
        hexMap.inputField.DeactivateInput();
      }
    }

    public void ActionDone(Unit unit, ActionController.actionName actionName)
    {
      if (selectedUnit == null) return;
      if (actionName == ActionController.actionName.MOVE
          && Util.eq<Unit>(unit, selectedUnit) && !unit.IsAI())
      {
        if (onUnitSelect != null) onUnitSelect(selectedUnit, false);
      }
    }

    public enum mode
    {
      detect,
      camera,
      move,
      attack,
      feint,
      surpriseAttack,
      falseOrder,
      alienate,
      repos,
      sabotage,
      fire
    }

    public override void UpdateChild()
    {
      if (actionController.ActionOngoing || EventSystem.current.IsPointerOverGameObject() || tc.endingTurn || tc.showingTitle)
      {
        return;
      }
      tileUnderMouse = MouseToTile();
      if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonUp(1))
      {
        Escape();
      }
      Update_CurrentFunc();
      lastMousePixelPosition = Input.mousePosition;
      // to avoid the camera shake
      lastMousePosition = GetMousePos();
      UpdateCameraScroll();
    }

    public delegate void OnModeQuit(Unit unit);
    public event OnModeQuit onModeQuit;

    public void Escape()
    {
      if (mouseMode == mode.detect && selectedSettlement != null && hexMap.settlementViewPanel.selectedUnit != null) {
        hexMap.settlementViewPanel.CancelGarrison();
        return;
      }

      if (mouseMode == mode.repos) {
        foreach(Unit u in nearbyAlly) {
          hexMap.SetUnitSkin(u);
          targetUnit = null;
        }
      }

      if (mouseMode == mode.attack || mouseMode == mode.feint) {
        foreach(Unit u in nearbyEnemey) {
          hexMap.SetUnitSkin(u);
        }
      }

      if (mouseMode == mode.surpriseAttack) {
        foreach(Unit u in surpriseTargets) {
          hexMap.SetUnitSkin(u);
        }
      }

      if (mouseMode == mode.falseOrder) {
        foreach(Unit u in falseOrderTargets) {
          hexMap.SetUnitSkin(u);
        }
      }

      if (mouseMode == mode.alienate) {
        foreach(Unit u in alienateTargets) {
          hexMap.SetUnitSkin(u);
        }
      }

      if (mouseMode == mode.fire) {
        hexMap.DehighlightArea();
      }

      selectedPath = null;
      if (selectedUnit != null)
      {
        if (mouseMode == mode.detect || mouseMode == mode.camera)
        {
          onUnitDeselect(selectedUnit);
          selectedUnit = null;
        }
        else
        {
          onModeQuit(selectedUnit);
        }
      }
      if (selectedSettlement != null && (mouseMode == mode.detect || mouseMode == mode.camera))
      {
        onSettlementDeselect(selectedSettlement);
        selectedSettlement = null;
      }
      ResetUpdateFunc();
    }

    public void EscapeOnOperationCommence()
    {
      msgBox.Show("");
      foreach(Unit u in nearbyEnemey) {
        if (u != null) {
          hexMap.SetUnitSkin(u);
        }
      }
      onUnitDeselect(selectedUnit);
      selectedUnit = null;
      ResetUnitSelection();
      ResetUpdateFunc();
    }

    public void EscapeOnOperationCancel()
    {
      msgBox.Show("");
      foreach(Unit u in nearbyEnemey) {
        hexMap.SetUnitSkin(u);
      }
      hexMap.combatController.CancelOperation();
      onModeQuit(selectedUnit);
      ResetUpdateFunc();
    }

    void ResetUpdateFunc()
    {
      mouseMode = mode.detect;
      Update_CurrentFunc = Update_DetectModeStart;
      // also clean up ui stuff associated with previous mode
      paths = new Dictionary<Tile, Tile[]>();
    }

    void Update_DetectModeStart()
    {
      if (Input.GetMouseButtonDown(0))
      {
        // left mouse button clicked, do nothing
        // only happens once for each click
      }
      else if (Input.GetMouseButtonUp(0) && tileUnderMouse != null)
      {
        ClickOnTile();
      }
      else if (Input.GetMouseButton(0) &&
      Vector3.Distance(Input.mousePosition, lastMousePixelPosition) > mouseDragThreshold)
      {
        // left button's been held down and mouse moved, this is a camera drag
        mouseMode = mode.camera;
        Update_CurrentFunc = UpdateCameraDrag;
      }
      else if (selectedUnit != null && Input.GetMouseButton(1))
      {
        // unit selected, show the path

      }
    }

    public delegate void OnUnitSelect(Unit selectedUnit, bool isGarrison);
    public event OnUnitSelect onUnitSelect;
    public event OnUnitSelect onUnitPreflight;
    public delegate void OnUnitDeselect(Unit deselectedUnit);
    public event OnUnitDeselect onUnitDeselect;
    public delegate void OnSettlementSelect(Settlement selectedSettlement);
    public event OnSettlementSelect onSettlementSelect;
    public delegate void OnSettlementDeselect(Settlement deselectedSettlement);
    public event OnSettlementDeselect onSettlementDeselect;

    public Unit targetUnit;
    public Settlement targetSettlement;
    void ClickOnTile()
    {
      if (hexMap.unitSelectionPanel.tileSelecting) {
        if (hexMap.unitSelectionPanel.isSelectableTile(tileUnderMouse)) {
          selectedSettlement.Decamp(hexMap.settlementViewPanel.selectedUnit, tileUnderMouse);
          hexMap.settlementViewPanel.CancelGarrison();
        }
        return;
      }

      targetUnit = null;
      targetSettlement = null;
      if (mouseMode == mode.detect) {
        msgBox.Show(tileUnderMouse.Q + ", " + tileUnderMouse.R);
      }
      Unit u = tileUnderMouse.GetUnit();
      Settlement s = tileUnderMouse.settlement;

      if (!hexMap.deployDone && s == null && u == null
        && hexMap.InitPlayerDeploymentZone().Contains(tileUnderMouse)) {
        if (selectedUnit != null && s == null) {
          selectedUnit.SetWargameTile(tileUnderMouse);
          Escape();
        }
      }

      if (mouseMode == mode.attack || mouseMode == mode.feint) {
        if(u != null && u.IsAI() != selectedUnit.IsAI() && nearbyEnemey.Contains(u)) {
          targetUnit = u;
          return;
        }

        if (hexMap.settlementViewPanel.selectedUnit == null && s != null && s.owner.isAI != selectedUnit.IsAI()
          && Util.eq<Settlement>(nearEnemySettlement, s)) {
          targetSettlement = s;
          return;
        }
      }

      if (mouseMode == mode.surpriseAttack) {
        if(u != null && u.IsAI() != selectedUnit.IsAI() && surpriseTargets.Contains(u)) {
          targetUnit = u;
          return;
        }
      }

      if (mouseMode == mode.falseOrder) {
        if(u != null && u.IsAI() != selectedUnit.IsAI() && falseOrderTargets.Contains(u)) {
          targetUnit = u;
          return;
        }
      }

      if (mouseMode == mode.alienate) {
        if(u != null && u.IsAI() != selectedUnit.IsAI() && alienateTargets.Contains(u)) {
          targetUnit = u;
          return;
        }
      }

      if (mouseMode == mode.repos && nearbyAlly.Contains(u)) {
        targetUnit = u;
        return;
      }

      if (mouseMode == mode.detect)
      {
        if (selectedUnit != null && onUnitDeselect != null) onUnitDeselect(selectedUnit);
        if (selectedSettlement != null && onSettlementDeselect != null) onSettlementDeselect(selectedSettlement);
        if (s != null)
        {
          selectedSettlement = s;
          PrepareUnitSelection();
          if (onSettlementSelect != null) onSettlementSelect(s);
        }
        else if (u != null)
        {
          selectedUnit = u;
          PrepareUnitSelection();
          if (onUnitSelect != null) onUnitSelect(selectedUnit, false);
        }
        else
        {
          Escape();
        }
      }
    }

    void UpdateUnitAttack()
    {
      if (tileUnderMouse == null) {
        return;
      }
      if (Input.GetMouseButtonUp(0))
      {
        ClickOnTile();
        if (targetSettlement != null)
        {
          msgBox.Show("");
          if (targetSettlement.IsEmpty()) {
            if (!actionController.attackEmptySettlement(selectedUnit, tileUnderMouse)) {
              // TODO
              Debug.LogError("Failed to attack empty settlement, try again!");
            }
            Update_CurrentFunc = Escape;
            return;
          }
        }

        if (targetUnit != null && targetUnit.IsVulnerable()) {
          actionController.Pursue(selectedUnit, targetUnit);
          Escape();
        } else if (targetUnit != null || targetSettlement != null) {
          msgBox.Show("");
          hexMap.combatController.StartOperation(selectedUnit, targetUnit, targetSettlement);
          hexMap.actionController.commenceOperation();
        }
      } else if (!Util.eq<Tile>(tileUnderMouse, selectedUnit.tile))
      {
        Unit u = tileUnderMouse.GetUnit();
        if (u != null)
        {
          hover.Show(u.Name());
        }
      }
    }

    void UpdateUnitFeintDefeat()
    {
      if (tileUnderMouse == null) {
        return;
      }
      if (Input.GetMouseButtonUp(0))
      {
        ClickOnTile();
        if (targetUnit != null || targetSettlement != null) {
          msgBox.Show("");
          hexMap.combatController.StartOperation(selectedUnit, targetUnit, targetSettlement, false, true);
          hexMap.actionController.commenceOperation();
        }
      } else if (!Util.eq<Tile>(tileUnderMouse, selectedUnit.tile))
      {
        Unit u = tileUnderMouse.GetUnit();
        if (u != null)
        {
          hover.Show(u.Name());
        }
      }
    }

    void UpdateUnitSurpriseAttack()
    {
      if (tileUnderMouse == null) {
        return;
      }
      if (Input.GetMouseButtonUp(0))
      {
        ClickOnTile();
        if (targetUnit != null) {
          msgBox.Show("");
          hexMap.actionController.SurpriseAttack(selectedUnit, targetUnit);
          Escape();
        }
      } else if (!Util.eq<Tile>(tileUnderMouse, selectedUnit.tile))
      {
        Unit u = tileUnderMouse.GetUnit();
        if (u != null)
        {
          hover.Show(u.Name());
        }
      }
    }

    void UpdateUnitFalseOrder()
    {
      if (tileUnderMouse == null) {
        return;
      }
      if (Input.GetMouseButtonUp(0))
      {
        ClickOnTile();
        if (targetUnit != null) {
          msgBox.Show("");
          hexMap.actionController.FalseOrder(selectedUnit, targetUnit);
          Escape();
        }
      } else if (!Util.eq<Tile>(tileUnderMouse, selectedUnit.tile))
      {
        Unit u = tileUnderMouse.GetUnit();
        if (u != null)
        {
          hover.Show(u.Name());
        }
      }
    }

    void UpdateUnitAlienate()
    {
      if (tileUnderMouse == null) {
        return;
      }
      if (Input.GetMouseButtonUp(0))
      {
        ClickOnTile();
        if (targetUnit != null) {
          msgBox.Show("");
          hexMap.actionController.Alienate(selectedUnit, targetUnit);
          Escape();
        }
      } else if (!Util.eq<Tile>(tileUnderMouse, selectedUnit.tile))
      {
        Unit u = tileUnderMouse.GetUnit();
        if (u != null)
        {
          hover.Show(u.Name());
        }
      }
    }

    void UpdateUnitRepos()
    {
      if (tileUnderMouse == null) {
        return;
      }
      if (Input.GetMouseButtonUp(0))
      {
        ClickOnTile();
        if (targetUnit != null) {
          msgBox.Show("");
          Tile tile1 = selectedUnit.tile;
          if (hexMap.wargameController.start) {
            hexMap.wargameController.AddRepo(targetUnit.tile, selectedUnit, new Tile[]{targetUnit.tile});
            hexMap.wargameController.AddRepo(tile1, targetUnit, new Tile[]{tile1});
          } else {
            hexMap.unitAniController.MoveUnit(selectedUnit, targetUnit.tile);
            hexMap.unitAniController.MoveUnit(targetUnit, tile1);
          }
          Escape();
        }
      }
    }

    void UpdateUnitCharge()
    {
      if (tileUnderMouse == null) {
        return;
      }
      if (Input.GetMouseButtonUp(0))
      {
        ClickOnTile();
        if (targetUnit != null && targetUnit.CanBeShaked(selectedUnit) > 0) {
          msgBox.Show("");
          actionController.charge(selectedUnit, targetUnit);
          Escape();
        }
      }
    }

    void UpdateUnitSkirmish()
    {
      if (tileUnderMouse == null) {
        return;
      }
      if (Input.GetMouseButtonUp(0))
      {
        ClickOnTile();
        if (targetUnit != null && targetUnit.CanBeWaved()) {
          msgBox.Show("");
          actionController.Skirmish(selectedUnit, targetUnit);
          Escape();
        }
      }
    }

    void UpdateUnitBreakThrough()
    {
      if (tileUnderMouse == null) {
        return;
      }
      if (Input.GetMouseButtonUp(0))
      {
        ClickOnTile();
        if (targetUnit != null && targetUnit.CanBeShaked(selectedUnit) > 0) {
          msgBox.Show("");
          actionController.breakThrough(selectedUnit, targetUnit);
          Escape();
        }
      }
    }

    void UpdateUnitMovement()
    {
      if (Input.GetMouseButtonUp(0))
      {
        if (tileUnderMouse != null && tileUnderMouse.GetUnit() != null) {
          Escape();
        } else if (selectedPath != null && selectedPath.Length > 0)
        {
          if (hexMap.wargameController.start) {
            hexMap.wargameController.Add(tileUnderMouse, selectedUnit, selectedPath);
            Escape();
          } else {
            selectedUnit.SetPath(selectedPath);
            accessibleTiles = selectedUnit.GetAccessibleTiles();
            if (!actionController.move(selectedUnit))
            {
              // TODO
              Debug.LogError("Failed to move unit, try again!");
            }
          }
        }
        return;
      }

      if (tileUnderMouse != null && !Util.eq<Tile>(tileUnderMouse, selectedUnit.tile))
      {
        if (!tileUnderMouse.DeployableForPathFind(selectedUnit) ||
         !accessibleTiles.Contains(tileUnderMouse))
        {
          selectedPath = new Tile[0];
        }
        else if (!paths.ContainsKey(tileUnderMouse))
        {
          paths[tileUnderMouse] = selectedUnit.FindPath(tileUnderMouse);
          selectedPath = paths[tileUnderMouse];
        }
        else
        {
          selectedPath = paths[tileUnderMouse];
        }
        Unit u = selectedUnit.Preflight(tileUnderMouse, selectedPath);
        if (onUnitPreflight != null && u != null) onUnitPreflight(u, false);
        hexMap.HighlightPath(selectedPath);
      }
      else
      {
        selectedPath = null;
      }
    }

    void UpdateUnitPoision()
    {
      if (tileUnderMouse == null || tileUnderMouse.terrian != TerrianType.Water
        || (selectedUnit != null && !selectedUnit.tile.neighbours.Contains(tileUnderMouse))
        || (selectedSettlement != null && !selectedSettlement.baseTile.neighbours.Contains(tileUnderMouse))) {
          return;
      }
      hexMap.HighlightArea(tileUnderMouse.poision.downStreams.ToArray(), HexMap.RangeType.PoisionRange);
      if (Input.GetMouseButtonUp(0))
      {
        Unit u = selectedUnit == null ? hexMap.settlementViewPanel.selectedUnit : selectedUnit;
        if (!actionController.poision(u, tileUnderMouse)) {
          // TODO
          Debug.LogError("Failed to poision river, try again!");
        }
        Update_CurrentFunc = Escape;
        return;
      }
    }

    void UpdateUnitSetFire()
    {
      if (tileUnderMouse != null && Input.GetMouseButtonUp(0) && nearFireTiles.Contains(tileUnderMouse))
      {
        if(!actionController.burn(selectedUnit, tileUnderMouse)) {
          // TODO
          Debug.LogError("Failed to set fire!");
        }
        Escape();
      }
    }

    Tile MouseToTile()
    {
      Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hitInfo;
      if (Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, hexTileLayerMask.value)) // only hit 9th layer which is 2^8
      {
        return hitInfo.transform.parent.gameObject.GetComponentInChildren<TileView>().tile;
      }
      return null;
    }

    Vector3 GetMousePos()
    {
      Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
      // what the point is at which the mouse ray intersects Y=0
      if (mouseRay.direction.y >= 0)
      {
        return Vector3.zero;
      }
      float rayLen = (mouseRay.origin.y / mouseRay.direction.y); // rayLen = magnitude
      return mouseRay.origin - (mouseRay.direction * rayLen);
    }

    void UpdateCameraDrag()
    {
      if (Input.GetMouseButtonUp(0))
      {
        ResetUpdateFunc();
        return;
      }

      Vector3 diff = lastMousePosition - GetMousePos();
      Camera.main.transform.Translate(diff, Space.World);
    }

    void UpdateCameraScroll()
    {
      float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
      float minHeight = 2f;
      float maxHeight = 20f;
      if (Mathf.Abs(scrollAmount) > 0.01f)
      {
        Vector3 dir = Camera.main.transform.position - lastMousePosition;
        Vector3 p = Camera.main.transform.position;
        if (p.y < (maxHeight - 0.1f) || scrollAmount < 0)
        {
          Camera.main.transform.Translate(dir * scrollAmount, Space.World);
        }
        p = Camera.main.transform.position;
        if (p.y < minHeight)
        {
          p.y = minHeight;
        }
        else if (p.y > maxHeight)
        {
          p.y = maxHeight;
        }
        Camera.main.transform.position = p;
      }

      // change the camera angle
      // float lowZoom = minHeight + 3;
      // float highZoom = maxHeight - 10;

      Camera.main.transform.rotation = Quaternion.Euler(
        Mathf.Lerp(35, 90, Camera.main.transform.position.y / (maxHeight / 1.5f)),
        Camera.main.transform.rotation.eulerAngles.y,
        Camera.main.transform.rotation.eulerAngles.z
      );
    }
  }

}