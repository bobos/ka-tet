using CourtNS;

namespace UnitNS
{
  public class PlainSickness
  {
    Unit unit;
    public float debuf = 0;
    public float moveDebuf = 0;
    public bool happened = false;
    public bool affected = false;
    public PlainSickness(Unit unit) {
      this.unit = unit;
    }

    public int Occur() {
      // TODO: for DLC
      return 0;
      if (happened || 
      (!Util.eq<Province>(unit.hexMap.warProvince, Cons.heBei) &&
       !Util.eq<Province>(unit.hexMap.warProvince, Cons.heDong) &&
       !Util.eq<Province>(unit.hexMap.warProvince, Cons.heNan)
       ) ||
       unit.type != Type.Cavalry) {
        return 0;
      }

      happened = true;
      if (Cons.FairChance()) {
        affected = true;
        debuf = moveDebuf = 0.3f;
        return -5;
      }

      return 0;
    }
  }
}