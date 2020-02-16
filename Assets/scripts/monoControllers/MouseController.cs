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
    Unit __transferedUnit = null;
    public mode mouseMode;
    public Unit selectedUnit
    {
      get { return __selectedUnit; }
      set
      {
        __selectedUnit = value;
      }
    }

    public Unit transferedUnit
    {
      get { return __transferedUnit; }
      set
      {
        __transferedUnit = value;
      }
    }
    public Settlement transferedSettlement = null;
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
    public Tile nearFire = null;
    public Tile inCampField = null;
    public bool nearAlly = false;
    public bool nearEnemy = false;
    public bool nearWater = false;
    public List<Unit> nearbyEnemey = null;

    void ResetUnitSelection() {
      nearEnemySettlement = null;
      nearMySettlement = null;
      nearDam = null;
      nearFire = null;
      nearAlly = false;
      nearEnemy = false;
      nearWater = false;
      inCampField = null;
      nearbyEnemey = new List<Unit>();
    }

    void PrepareUnitSelection() {
      ResetUnitSelection();
      Settlement s = null;
      Tile t = selectedUnit.tile;
      if (t.IsCampable()) {
        inCampField = t;
      }

      foreach(Tile tile in t.neighbours) {
        if (tile.settlement != null) {
          s = tile.settlement;
        }

        if (tile.burnable) {
          nearFire = tile;
        }

        if (tile.isDam) {
          nearDam = tile;
        }

        if (tile.terrian == TerrianType.Water) {
          nearWater = true;
        }

        Unit u = tile.GetUnit();
        if (u != null && u.IsAI() == selectedUnit.IsAI()) {
          nearAlly = true;
        }

        if (u != null && u.IsAI() != selectedUnit.IsAI() && !u.IsConcealed()) {
          nearEnemy = true;
          nearbyEnemey.Add(u);
        }

        //TODO: check assault for Scout with enemy in 2 rings and self is hidden

      }

      if (s != null && selectedUnit.IsAI() == s.owner.isAI) {
        nearMySettlement = s;
      }
      if (s != null && selectedUnit.IsAI() != s.owner.isAI) {
        nearEnemySettlement = s;
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

      if (action == ActionController.actionName.CAMP)
      {
        if (settlementMgr.BuildCamp(inCampField,
            selectedUnit.IsAI() ? hexMap.warParties[1] : hexMap.warParties[0],
            selectedUnit))
        {
          popAniController.Show(hexMap.GetUnitView(selectedUnit), textLib.get("pop_buildingStarted"), Color.green);
          Escape();
        }
        else
        {
          popAniController.Show(hexMap.GetUnitView(selectedUnit), textLib.get("pop_buildingFailed"), Color.white);
        }
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

      if (action == ActionController.actionName.SABOTAGE)
      {
        if(!actionController.sabotage(selectedUnit, nearDam)){
          // TODO
          Debug.LogError("Failed to sabotage, try again!");
        }
        return;
      }

      if (action == ActionController.actionName.FIRE)
      {
        actionController.burn(nearFire);
        return;
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
        msgBox.Show("选择进攻目标!");
        foreach(Unit u in nearbyEnemey) {
          hexMap.TargetUnit(u);
        }
      }

      if (action == ActionController.actionName.TRANSFERSUPPLY ||
          action == ActionController.actionName.TRANSFERLABOR)
      {
        mouseMode = action == ActionController.actionName.TRANSFERLABOR ? mode.transferLabor : mode.transfer;
        transferedUnit = null;
        transferedSettlement = null;
        if (action == ActionController.actionName.TRANSFERLABOR) {
          Update_CurrentFunc = UpdateUnitTransferLabor;
        } else {
          Update_CurrentFunc = UpdateUnitTransferSupply;
        }
      }

      if (action == ActionController.actionName.DISTSUPPLY || action == ActionController.actionName.DISTLABOR)
      {
        if (selectedSettlement.GetReachableSettlements().Length <= 0) {
          popAniController.Show(settlementMgr.GetView(selectedSettlement), textLib.get("pop_noSettlementNearby"), Color.yellow);
          return;
        }
        transferedSettlement = null;
        mouseMode = action == ActionController.actionName.DISTSUPPLY ? mode.dist : mode.distLabor;
        Update_CurrentFunc = UpdateSettlementDistSupply;
      }

      if (action == ActionController.actionName.LABOR2Unit)
      {
        bool allyNearBy = false;
        foreach (Tile tile in selectedSettlement.baseTile.neighbours)
        {
          Unit u = tile.GetUnit();
          if (selectedSettlement.IsFunctional() && u != null &&
              u.IsAI() == selectedSettlement.owner.isAI) allyNearBy = true;
        }
        if (!allyNearBy) {
          popAniController.Show(settlementMgr.GetView(selectedSettlement), textLib.get("pop_noAllyNearby"), Color.yellow);
          return;
        }
        transferedUnit = null;
        mouseMode = mode.labor2unit;
        Update_CurrentFunc = UpdateDistLabor2Unit;
      }

      if (action == ActionController.actionName.INPUTCONFIRM)
      {
        if (mouseMode == mode.transfer) {
          try {
            int supply = hexMap.inputField.GetInput();
            if (supply == 0 || supply > selectedUnit.supply.supply) {
              msgBox.Show("invalid input");
              return;
            }
            selectedUnit.supply.supply -= supply;
            if (transferedUnit != null) {
              selectedUnit.supply.supply += transferedUnit.supply.TakeTransferSupply(supply);
              popAniController.Show(hexMap.GetUnitView(transferedUnit), textLib.get("pop_transferDone"), Color.green);
            } else {
              transferedSettlement.TakeInSupply(supply);
              popAniController.Show(settlementMgr.GetView(transferedSettlement), textLib.get("pop_transferDone"), Color.green);
            }
            Escape();
          } catch (System.Exception exception) {
            msgBox.Show(exception.Message);
            return;
          }
        }

        if (mouseMode == mode.transferLabor) {
          try {
            int labor = hexMap.inputField.GetInput();
            if (labor == 0 || labor > selectedUnit.labor ||
                (transferedUnit != null && labor > transferedUnit.LaborCanTakeIn())) {
              msgBox.Show("invalid input");
              return;
            }
            selectedUnit.labor -= labor;
            if (transferedUnit != null) {
              int remain = transferedUnit.TakeInLabor(labor);
              selectedUnit.labor += remain;
              popAniController.Show(hexMap.GetUnitView(transferedUnit), textLib.get("pop_transferDone"), Color.green);
            } else {
              transferedSettlement.TakeInLabor(labor);
              popAniController.Show(settlementMgr.GetView(transferedSettlement), textLib.get("pop_transferDone"), Color.green);
            }
            Escape();
          } catch (System.Exception exception) {
            msgBox.Show(exception.Message);
            return;
          }
        }

        if (mouseMode == mode.dist) {
          try {
            int supply = hexMap.inputField.GetInput();
            if (supply == 0 || supply > selectedSettlement.MaxDistSupply() ||
                supply > transferedSettlement.SupplyCanTakeIn()) {
              msgBox.Show("invalid input, max can dist supply " + selectedSettlement.MaxDistSupply() +
                          " can take in " + transferedSettlement.SupplyCanTakeIn());
              return;
            }
            settlementMgr.AddDistJob(selectedSettlement, transferedSettlement, supply, SettlementMgr.QueueJobType.DistSupply);
            popAniController.Show(settlementMgr.GetView(selectedSettlement), textLib.get("pop_transferIssued"), Color.green);
            Escape();
          } catch (System.Exception exception) {
            msgBox.Show(exception.Message);
            return;
          }
        }

        if (mouseMode == mode.distLabor) {
          try {
            int supply = hexMap.inputField.GetInput();
            int canDist = supply > selectedSettlement.labor ? selectedSettlement.labor : supply;
            if (supply == 0 || supply != canDist) {
              msgBox.Show("invalid input, max labor can distribute " + canDist);
              return;
            }
            settlementMgr.AddDistJob(selectedSettlement, transferedSettlement, supply, SettlementMgr.QueueJobType.DistLabor);
            popAniController.Show(settlementMgr.GetView(selectedSettlement), textLib.get("pop_transferIssued"), Color.green);
            Escape();
          } catch (System.Exception exception) {
            msgBox.Show(exception.Message);
            return;
          }
        }

        if (mouseMode == mode.labor2unit) {
          try {
            int supply = hexMap.inputField.GetInput();
            if (supply == 0 || supply > selectedSettlement.labor || supply > transferedUnit.LaborCanTakeIn()) {
              msgBox.Show("invalid input");
              return;
            }
            int remain = transferedUnit.TakeInLabor(supply);
            selectedSettlement.labor -= supply - remain;
            popAniController.Show(hexMap.GetUnitView(transferedUnit), textLib.get("pop_transferDone"), Color.green);
            Escape();
          } catch (System.Exception exception) {
            msgBox.Show(exception.Message);
            return;
          }
        }
      }
    }

    public void ActionDone(Unit unit, Unit[] units, ActionController.actionName actionName)
    {
      if (selectedUnit == null) return;
      if ((actionName == ActionController.actionName.ATTACK && unit.IsAI())
        || (actionName == ActionController.actionName.MOVE
          && Util.eq<Unit>(unit, selectedUnit) && !unit.IsAI()))
      {
        Escape();
      }
    }

    public enum mode
    {
      detect,
      camera,
      move,
      attack,
      sabotage,
      transfer,
      transferLabor,
      dist,
      distLabor,
      labor2unit
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
          if (mouseMode == mode.transfer || mouseMode == mode.transferLabor) {
            transferedUnit = null;
            transferedSettlement = null;
            hexMap.inputField.DeactivateInput();
          }
          onModeQuit(selectedUnit);
        }
      }
      if (selectedSettlement != null)
      {
        if (mouseMode == mode.detect || mouseMode == mode.camera)
        {
          onSettlementDeselect(selectedSettlement);
          selectedSettlement = null;
        }
        if (mouseMode == mode.dist || mouseMode == mode.distLabor || mouseMode == mode.labor2unit) {
          transferedSettlement = null;
          transferedUnit = null;
          hexMap.inputField.DeactivateInput();
        }
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

    public delegate void OnUnitSelect(Unit selectedUnit);
    public event OnUnitSelect onUnitSelect;
    public event OnUnitSelect onUnitPreflight;
    public delegate void OnUnitDeselect(Unit deselectedUnit);
    public event OnUnitDeselect onUnitDeselect;
    public delegate void OnUnitAttack(Unit[] punchers, Unit receiver);
    public event OnUnitAttack onUnitAttack;
    public delegate void OnSettlementSelect(Settlement selectedSettlement);
    public event OnSettlementSelect onSettlementSelect;
    public delegate void OnSettlementDeselect(Settlement deselectedSettlement);
    public event OnSettlementDeselect onSettlementDeselect;

    public Unit targetUnit;
    public Settlement targetSettlement;
    void ClickOnTile()
    {
      targetUnit = null;
      targetSettlement = null;
      if (mouseMode == mode.detect) {
        msgBox.Show(tileUnderMouse.Q + ", " + tileUnderMouse.R);
      }
      Unit u = tileUnderMouse.GetUnit();
      Settlement s = tileUnderMouse.settlement;
      // TODO: search playerTurn when disable AI turn debug
      if (tileUnderMouse.IsThereConcealedEnemy(!hexMap.turnController.playerTurn)) {
        u = null;
      }
      if (u != null && hexMap.wargameController.start && u.type == Type.Scout)
      {
        u = null;
      }

      if (mouseMode == mode.attack) {
        if(u != null && u.IsAI() != selectedUnit.IsAI()) {
          targetUnit = u;
          return;
        }

        if (s != null && s.owner.isAI != selectedUnit.IsAI()) {
          targetSettlement = s;
          return;
        }
      }

      if (mouseMode == mode.detect)
      {
        if (selectedUnit != null && onUnitDeselect != null) onUnitDeselect(selectedUnit);
        if (selectedSettlement != null && onSettlementDeselect != null) onSettlementDeselect(selectedSettlement);
        if (s != null)
        {
          selectedSettlement = s;
          if (onSettlementSelect != null) onSettlementSelect(s);
        }
        else if (u != null)
        {
          selectedUnit = u;
          PrepareUnitSelection();
          if (onUnitSelect != null) onUnitSelect(selectedUnit);
        }
        else
        {
          Escape();
        }
      }

      if (mouseMode == mode.transfer || mouseMode == mode.transferLabor)
      {
        Tile[] tiles = selectedUnit.tile.neighbours;
        if ((u == null || u.IsAI() != selectedUnit.IsAI() || !tiles.Contains(u.tile)) &&
            (s == null || s.owner.isAI != selectedUnit.IsAI() || !s.IsFunctional() ||
             !tiles.Contains(s.baseTile))) return;
        if (u != null) {
          transferedSettlement = null;
          transferedUnit = u;
        } else {
          transferedUnit = null;
          transferedSettlement = s;
        }
      }

      if (mouseMode == mode.labor2unit)
      {
        Tile[] tiles = selectedSettlement.baseTile.neighbours;
        if (u == null || u.IsAI() != selectedSettlement.owner.isAI || !tiles.Contains(u.tile)) return;
        transferedUnit = u;
      }

      if (mouseMode == mode.dist || mouseMode == mode.distLabor)
      {
        Settlement[] settlements = selectedSettlement.GetReachableSettlements();
        if (settlements.Contains(s)) {
          transferedSettlement = s;
        } else {
          return;
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

        if (targetUnit != null) {
          msgBox.Show("");
          hexMap.combatController.StartOperation(selectedUnit, targetUnit);
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

    void UpdateUnitTransferSupply()
    {
      if (Input.GetMouseButtonUp(0) && tileUnderMouse != null)
      {
        ClickOnTile();
        if (transferedUnit == null && transferedSettlement == null) {
          return;
        }
        string needed = "" + (transferedUnit != null ? transferedUnit.supply.SupplyNeededPerTurn()
          : transferedSettlement.SupplyNeeded());
        int minNeeded = transferedUnit != null ? transferedUnit.supply.MinSupplyNeeded() : transferedSettlement.SupplyNeeded(); 
        string suggestions = "needed " + needed + " supply for last one turn, at least " + minNeeded + " supply to support one turn\n";
        if (transferedSettlement != null) {
          foreach(Settlement.SupplySuggestion sug in transferedSettlement.GetSuggestion()) {
            suggestions += "to support " + sug.supportTroop + " infantry per turn, need labor "
              + sug.laborNeeded + " supply " + sug.supplyNeeded + "\n"; 
          }
        }
        hover.Show(suggestions);
        hexMap.inputField.ActivateInput();
      }
    }

    void UpdateSettlementDistSupply()
    {
      if (Input.GetMouseButtonUp(0) && tileUnderMouse != null)
      {
        ClickOnTile();
        if (transferedSettlement == null) {
          return;
        }
        string suggestions = "";
        if (transferedSettlement.SupplyNeeded() > 0) {
          suggestions += "need " + transferedSettlement.SupplyNeeded() + " to support garrison\n";
        }
        foreach(Settlement.SupplySuggestion sug in transferedSettlement.GetSuggestion()) {
          suggestions += "to support " + sug.supportTroop + " infantry per turn, need labor "
            + sug.laborNeeded + " supply " + sug.supplyNeeded + "\n"; 
        }
        hover.Show(suggestions);
        hexMap.inputField.ActivateInput();
      }
    }

    void UpdateDistLabor2Unit()
    {
      if (Input.GetMouseButtonUp(0) && tileUnderMouse != null)
      {
        ClickOnTile();
        if (transferedUnit == null) {
          return;
        }
        string suggestion = "max taken " + transferedUnit.LaborCanTakeIn() + "\n";
        foreach(KeyValuePair<int, int> kv in transferedUnit.GetLaborSuggestion()) {
          suggestion += kv.Key + " turns of supply needs " + kv.Value + "\n";
        }
        hover.Show(suggestion);
        hexMap.inputField.ActivateInput();
      }
    }

    void UpdateUnitTransferLabor()
    {
      if (Input.GetMouseButtonUp(0) && tileUnderMouse != null)
      {
        ClickOnTile();
        if (transferedUnit == null && transferedSettlement == null) {
          return;
        }
        string suggestion = "max taken " + (transferedUnit != null ?
                            transferedUnit.LaborCanTakeIn() + "\n" :
                            "No Limit\n");
        if (transferedUnit != null) {
          foreach(KeyValuePair<int, int> kv in transferedUnit.GetLaborSuggestion()) {
            suggestion += kv.Key + " turns of supply needs " + kv.Value + "\n";
          }
        }
        else {
          foreach(Settlement.SupplySuggestion sug in transferedSettlement.GetSuggestion()) {
            suggestion += "to support " + sug.supportTroop + " infantry per turn, need labor "
              + sug.laborNeeded + " supply " + sug.supplyNeeded + "\n"; 
          }
        }
        hover.Show(suggestion);
        hexMap.inputField.ActivateInput();
      }
    }

    void UpdateUnitMovement()
    {
      if (Input.GetMouseButtonUp(0))
      {
        // copy the path to movement queue
        if (selectedPath != null && selectedPath.Length > 0)
        {
          if (hexMap.wargameController.start) {
            hexMap.wargameController.Add(tileUnderMouse, selectedUnit, selectedPath);
            Escape();
          } else {
            selectedUnit.SetPath(selectedPath);
            if (!actionController.move(selectedUnit))
            {
              // TODO
              Debug.LogError("Failed to move unit, try again!");
            }
          }
        }
        Update_CurrentFunc = Escape;
        return;
      }

      if (tileUnderMouse != null && !Util.eq<Tile>(tileUnderMouse, selectedUnit.tile))
      {
        if (!tileUnderMouse.DeployableForPathFind(selectedUnit) ||
         !selectedUnit.GetAccessibleTiles().Contains(tileUnderMouse))
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
        if (onUnitPreflight != null && u != null) onUnitPreflight(u);
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
        || !selectedUnit.tile.neighbours.Contains(tileUnderMouse)) {
          return;
      }
      hexMap.HighlightArea(tileUnderMouse.poision.downStreams.ToArray(), HexMap.RangeType.PoisionRange);
      if (Input.GetMouseButtonUp(0))
      {
        if (!actionController.poision(selectedUnit, tileUnderMouse)) {
          // TODO
          Debug.LogError("Failed to poision river, try again!");
        }
        Update_CurrentFunc = Escape;
        return;
      }
    }

    void UpdateUnitSetFire()
    {
      if (Input.GetMouseButtonUp(0) && tileUnderMouse != null
        && !Util.eq<Tile>(tileUnderMouse, selectedUnit.tile))
      {

        Tile burnTile = null;
        foreach (Tile h in selectedUnit.tile.neighbours)
        {
          if (Util.eq<Tile>(tileUnderMouse, h)
            && h.IsBurnable())
          {
            burnTile = h;
          }
        }
        if (burnTile != null)
        {
          actionController.burn(burnTile);
          Escape();
        }
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