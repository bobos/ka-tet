using UnityEngine;
using MapTileNS;
using MonoNS;

namespace UnitNS
{
  public class UnitView : MonoBehaviour
  {
    Vector3 newPosition;
    Vector3 currentVelocity;
    HexMap hexMap;
    Unit _unit = null;
    public Unit unit
    {
      get
      {
        return _unit;
      }
      set
      {
        if (_unit != null)
        {
          _unit.onUnitMove -= OnUnitMove;
        }
        _unit = value;
        _unit.onUnitMove += OnUnitMove;
      }
    }
    public bool Animating { get; private set; }
    ActionController actionController;
    MouseController mouseController;
    bool viewActivated = true;

    // Call Start by next frame is too late, need to call this init on create
    public void OnCreate()
    {
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

    public void Deactivate() {
      viewActivated = false;
    }

    public void Activate() {
      transform.position = unit.tile.Position();
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

    public void OnUnitMove(Tile newTile)
    {
      if (!viewActivated) return;
      hexMap.HighlightPath(unit.GetPath());
      Animating = true;
      newPosition = newTile.Position();
      currentVelocity = Vector3.zero;
    }

    public void OnUnitSelect(Unit unit)
    {
      if (Util.eq<Unit>(unit, this.unit))
      {
        hexMap.HighlightArea(unit.GetAccessibleTiles(), HexMap.RangeType.movement);
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
          hexMap.HighlightArea(unit.GetAccessibleTiles(), HexMap.RangeType.movement);
        }
        if (name == ActionController.actionName.ATTACK)
        {
          hexMap.HighlightArea(unit.GetAttackRange(), HexMap.RangeType.attack);
        }
      }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
      // NOTE: this point to the gameobject not the component
      if (!Animating || !viewActivated)
      {
        return;
      }
      this.transform.position = Vector3.SmoothDamp(this.transform.position,
      newPosition, ref currentVelocity, 0.2f);
      if (Vector3.Distance(this.transform.position, newPosition) < 0.1f)
      {
        Animating = false;
      }
    }

  }

}