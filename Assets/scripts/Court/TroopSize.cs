
namespace CourtNS {
  public class TroopSize {
    public int troopSize;

    public TroopSize(int size) {
      troopSize = size < 1 ? 1 : size > 4 ? 4 : size;
    }

    public int GetTroopSize(bool infantry) {
      if (troopSize == 1) {
        return infantry ? 5000 : 1000;
      }

      if (troopSize == 2) {
        return infantry ? 8000 : 2000;
      }

      if (troopSize == 3) {
        return infantry ? 10000 : 3000;
      }

      return infantry ? 15000 : 5000;
    }

  }

}