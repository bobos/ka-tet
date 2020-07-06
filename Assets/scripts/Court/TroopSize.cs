
namespace CourtNS {
  public class TroopSize {
    public int troopSize;

    public TroopSize(int size) {
      troopSize = size < 1 ? 1 : size > 3 ? 3 : size;
    }

    public int GetTroopSize(bool infantry) {
      if (troopSize == 1) {
        return infantry ? 4000 : 1000;
      }

      if (troopSize == 2) {
        return infantry ? 8000 : 2000;
      }

      return infantry ? 10000 : 3000;
    }

  }

}