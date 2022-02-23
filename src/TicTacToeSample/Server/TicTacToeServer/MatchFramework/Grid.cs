using ULS.Core;

namespace TicTacToeServer.MatchFramework
{
    public class WinInfo
    {
        public long WinningPlayer;
        public int X1, X2, X3, Y1, Y2, Y3;

        public void SetCombination(int x1, int y1, int x2, int y2, int x3, int y3)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            X3 = x3;
            Y3 = y3;
        }

        public override string ToString()
        {
            return $"id: {WinningPlayer} -- Combination: {X1}x{Y1} | {X2}x{Y2} | {X3}x{Y3}";
        }
    }

    public class Grid
    {
        public long[,] Blocks = new long[3, 3];

        public event Action<int, int, Player?>? OnBlockOwnershipChanged;

        public Grid()
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Blocks[x, y] = -1;
                }
            }
        }

        public bool TrySet(Player? player, int x, int y)
        {
            try
            {
                long newValue = player != null ? player.UniqueId : -1;
                if (Blocks[x, y] == newValue ||
                    Blocks[x, y] != -1)
                {
                    return false;
                }

                Blocks[x, y] = newValue;
                OnBlockOwnershipChanged?.Invoke(x, y, player);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool CheckWinState(out WinInfo winInfo)
        {
            winInfo = new WinInfo();
            winInfo.WinningPlayer = -1;

            // Check horizontal
            for (int y = 0; y < Blocks.GetLength(1); y++)
            {
                if (Blocks[0, y] != -1 &&
                    Blocks[0, y] == Blocks[1, y] &&
                    Blocks[0, y] == Blocks[2, y])
                {
                    winInfo.WinningPlayer = Blocks[0, y];
                    winInfo.SetCombination(0, y, 1, y, 2, y);
                    return true;
                }
            }

            // Check vertical
            for (int x = 0; x < Blocks.GetLength(0); x++)
            {
                if (Blocks[x, 0] != -1 &&
                    Blocks[x, 0] == Blocks[x, 1] &&
                    Blocks[x, 0] == Blocks[x, 2])
                {
                    winInfo.WinningPlayer = Blocks[x, 0];
                    winInfo.SetCombination(x, 0, x, 1, x, 2);
                    return true;
                }
            }

            // Check diagonal
            if (Blocks[0, 0] != -1 &&
                Blocks[0, 0] == Blocks[1, 1] &&
                Blocks[0, 0] == Blocks[2, 2])
            {
                winInfo.WinningPlayer = Blocks[0, 0];
                winInfo.SetCombination(0, 0, 1, 1, 2, 2);
                return true;
            }
            if (Blocks[2, 0] != -1 &&
                Blocks[2, 0] == Blocks[1, 1] &&
                Blocks[2, 0] == Blocks[0, 2])
            {
                winInfo.WinningPlayer = Blocks[2, 0];
                winInfo.SetCombination(2, 0, 1, 1, 0, 2);
                return true;
            }

            return false;
        }
    }
}
