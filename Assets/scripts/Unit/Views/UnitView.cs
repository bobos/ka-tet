using UnityEngine;
using MapTileNS;
using MonoNS;
using System.Collections;

namespace UnitNS
{
  public class UnitView : View
  {
    Vector3 newPosition;
    Vector3 currentVelocity;
    HexMap hexMap;
    public Unit unit;
    public GameObject unitInfoGO;
    ActionController actionController;
    MouseController mouseController;

    // Call Start by next frame is too late, need to call this init on create
    public override void OnCreate(DataModel unit)
    {
      this.unit = (Unit)unit;
      Animating = false;
      newPosition = this.transform.position;
      hexMap = GameObject.FindObjectOfType<HexMap>();
      actionController = hexMap.actionController;
      mouseController = hexMap.mouseController;
      actionController.onBtnClick += OnBtnClick;
      mouseController.onUnitSelect += OnUnitSelect;
      mouseController.onModeQuit += OnModeQuit;
      mouseController.onUnitDeselect += OnUnitDeselect;
    }

    public static Vector3 UnitInfoPosition(Vector3 unitPosition) {
      return new Vector3(unitPosition.x - 0.5f, unitPosition.y - 0.6f, unitPosition.z);
    }

    public static Vector3 NamePosition(Vector3 unitPosition) {
      return new Vector3(unitPosition.x - 0.5f, unitPosition.y, unitPosition.z);
    }

    void UpdateUnitInfo() {
      unitInfoGO.transform.position = UnitInfoPosition(transform.position);
      unitInfoGO.GetComponent<UnitInfoView>().SetName(unit);
    }

    public void ToggleText(bool on) {
      foreach(MeshRenderer mr in unitInfoGO.GetComponentsInChildren<MeshRenderer>()) {
        mr.enabled = on;
      }
    }

    public void Deactivate() {
      OnUnitDeselect(mouseController.selectedUnit);
      ToggleText(false);
      viewActivated = false;
    }

    public void Activate() {
      ToggleText(true);
      transform.position = unit.tile.GetSurfacePosition();
      viewActivated = true;
    }

    public void OnModeQuit(Unit unit)
    {
      OnUnitSelect(unit);
    }

    public void Destroy()
    {
      OnUnitDeselect(mouseController.selectedUnit);
      actionController.onBtnClick -= OnBtnClick;
      mouseController.onUnitSelect -= OnUnitSelect;
      mouseController.onModeQuit -= OnModeQuit;
      mouseController.onUnitDeselect -= OnUnitDeselect;
      Destroy(unitInfoGO);
    }

    public void DestroyAnimation(DestroyType type)
    {
      Animating = true;
      StartCoroutine(CoDestroyAnimation(type));
    }

    IEnumerator CoDestroyAnimation(DestroyType type) {
      yield return new WaitForSeconds(1);
      Animating = false;
    }

    public void RoutAnimation()
    {
      Animating = true;
      StartCoroutine(CoRoutAnimation());
    }

    IEnumerator CoRoutAnimation() {
      yield return new WaitForSeconds(1);
      Animating = false;
    }

    public void Move(Tile newTile)
    {
      newPosition = newTile.GetSurfacePosition();
      if (viewActivated) {
        hexMap.HighlightPath(unit.GetPath());
        Animating = true;
      }
      currentVelocity = Vector3.zero;
    }

    public void OnUnitSelect(Unit unit, bool _isGarrison=false)
    {
      if (Util.eq<Unit>(unit, this.unit))
      {
        //if (!unit.IsAI() && !unit.TurnDone()) hexMap.HighlightUnit(unit);
        if (hexMap.wargameController.start && hexMap.wargameController.IsWargameUnit(unit)) {
        } else {
          Tile[] range = unit.GetAccessibleTiles(unit.IsAI() == hexMap.turnController.playerTurn);
          hexMap.HighlightArea(range, HexMap.RangeType.movement, this.unit);
          hexMap.HighlightPath(unit.GetPath());
        }
      }
    }

    public void OnUnitDeselect(Unit unit)
    {
      if (Util.eq<Unit>(unit, this.unit))
      {
        hexMap.DehighlightPath();
        hexMap.DehighlightArea();
        hexMap.UnhighlightUnit(unit);
      }
    }

    public void OnBtnClick(ActionController.actionName name)
    {
      if (Util.eq<Unit>(mouseController.selectedUnit, unit))
      {
        if (name == ActionController.actionName.MOVE)
        {
          hexMap.HighlightArea(unit.GetAccessibleTiles(), HexMap.RangeType.movement, this.unit);
        }
        if (name == ActionController.actionName.ATTACK)
        {
        }
      }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
      UpdateUnitInfo();
      if (!Animating) { return; }
      Vector3 originPosition = this.transform.position;
      // NOTE: this point to the gameobject not the component
      this.transform.position = Vector3.SmoothDamp(originPosition, newPosition, ref currentVelocity, 0.1f);
      //unitInfoGO.transform.position = Vector3.SmoothDamp(originPosition, newPosition, ref currentVelocity, 0.1f);
      if (Vector3.Distance(this.transform.position, newPosition) < 0.1f)
      {
        Animating = false;
      }
    }

  }

}