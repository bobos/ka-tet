using MapTileNS;

namespace UnitNS
{
  public class UnitActionBroker {
    public delegate void OnUnitAction(Unit unit, ActionType type, Tile tile);
    public event OnUnitAction onUnitAction;
    private static UnitActionBroker actionBroker;
    public static UnitActionBroker GetBroker() {
      if (actionBroker == null) {
        actionBroker = new UnitActionBroker();
      }
      return actionBroker;
    }

    private UnitActionBroker() {}

    public void BrokeChange(Unit unit, ActionType type, Tile targetTile) {
      if (onUnitAction != null) {
        onUnitAction(unit, type, targetTile);
      }
    }
  }

}