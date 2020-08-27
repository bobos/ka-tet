using CourtNS;

namespace UnitNS
{
  public class PlainSickness
  {
    Unit unit;
    public float debuf = 0;
    public bool happened = false;
    public bool affected = false;
    public PlainSickness(Unit unit) {
      this.unit = unit;
    }

    public int Occur() {
      if (happened || 
      !Util.eq<Region>(unit.rf.province.region, Cons.tubo) &&
      (!Util.eq<Province>(unit.hexMap.warProvince, Cons.heBei) &&
       !Util.eq<Province>(unit.hexMap.warProvince, Cons.heDong) &&
       !Util.eq<Province>(unit.hexMap.warProvince, Cons.heNan)
      )) {
        return 0;
      }

      happened = true;
      if (Cons.SlimChance()) {
        affected = true;
        debuf = 0.25f;
        return -25;
      }

      return 0;
    }
  }
}