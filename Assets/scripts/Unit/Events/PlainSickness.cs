using CourtNS;

namespace UnitNS
{
  public class PlainSickness
  {
    Unit unit;
    public float atkDebuf = 0;
    public float moveDebuf = 0;
    public bool happened = false;
    public bool affected = false;
    public PlainSickness(Unit unit) {
      this.unit = unit;
    }

    public int Occur() {
      if (happened || 
      (!Util.eq<Region>(unit.hexMap.warProvince.region, Cons.plain) &&
       !Util.eq<Region>(unit.hexMap.warProvince.region, Cons.lowLand)) ||
       unit.type != Type.Cavalry ||
       !Util.eq<Region>(unit.rf.province.region, Cons.upLand)) {
        return 0;
      }

      happened = true;
      if (Cons.FiftyFifty()) {
        affected = true;
        atkDebuf = moveDebuf = 0.2f;
        return 2;
      }

      return 0;
    }
  }
}