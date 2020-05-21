
namespace CourtNS {
  public class TroopSize {
    public int troopSize;

    public TroopSize(int size) {
      troopSize = size < 1 ? 1 : size > 3 ? 3 : size;
    }

    public int GetTroopSize(bool infantry) {
      if (troopSize == 1) {
        return infantry ? 4000 : 800;
      }

      if (troopSize == 2) {
        return infantry ? 8000 : 1500;
      }

      return infantry ? 10000 : 2000;
    }

  }

}