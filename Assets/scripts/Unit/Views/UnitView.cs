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
    public GameObject nameGO;
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
      actionController.actionDone += ActionDone;
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

    public void UpdateGeneralName() {
      nameGO.GetComponent<UnitNameView>().SetName(unit);
    }

    public void UpdateUnitInfo() {
      unitInfoGO.GetComponent<UnitInfoView>().SetName(unit.rf);
    }

    void ToggleUnitInfo(bool on) {
      foreach(MeshRenderer mr in unitInfoGO.GetComponentsInChildren<MeshRenderer>()) {
        mr.enabled = on;
      }
    }

    void ToggleText(bool on) {
      foreach(MeshRenderer mr in nameGO.GetComponentsInChildren<MeshRenderer>()) {
        mr.enabled = on;
      }
      ToggleUnitInfo(on);
    }

    public void Deactivate() {
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
      actionController.onBtnClick -= OnBtnClick;
      actionController.actionDone -= ActionDone;
      mouseController.onUnitSelect -= OnUnitSelect;
      mouseController.onModeQuit -= OnModeQuit;
      mouseController.onUnitDeselect -= OnUnitDeselect;
      Destroy(nameGO);
      Destroy(unitInfoGO);
    }

    public void ActionDone(Unit actionUnit, Unit[] units, ActionController.actionName actionName)
    {
      if (actionName == ActionController.actionName.ATTACK)
      {
        foreach (Unit attacker in units)
        {
          if (Util.eq<Unit>(attacker, unit))
          {
          }
        }
      }
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
        ToggleUnitInfo(false);
      }
      currentVelocity = Vector3.zero;
    }

    public void OnUnitSelect(Unit unit)
    {
      if (Util.eq<Unit>(unit, this.unit))
      {
        hexMap.HighlightArea(unit.GetAccessibleTiles(), HexMap.RangeType.movement, this.unit);
        hexMap.HighlightPath(unit.GetPath());
        if (!unit.IsAI() && !unit.TurnDone()) hexMap.HighlightUnit(unit);
      }
    }

    public void OnUnitDeselect(Unit unit)
    {
      if (Util.eq<Unit>(unit, this.unit))
      {
        hexMap.DehighlightPath();
        hexMap.DehighlightArea();
        hexMap.SetUnitSkin(unit);
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
      if (!Animating) { return; }
      Vector3 originPosition = this.transform.position;
      // NOTE: this point to the gameobject not the component
      this.transform.position = Vector3.SmoothDamp(originPosition, newPosition, ref currentVelocity, 0.1f);
      nameGO.transform.position = Vector3.SmoothDamp(originPosition, newPosition, ref currentVelocity, 0.1f);
      if (Vector3.Distance(this.transform.position, newPosition) < 0.1f)
      {
        if (viewActivated) {
          ToggleUnitInfo(true);
        }
        nameGO.transform.position = NamePosition(newPosition);
        unitInfoGO.transform.position = UnitInfoPosition(newPosition);
        Animating = false;
      }
    }

  }

}