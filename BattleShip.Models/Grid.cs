namespace BattleShip.Models;

public class Grid
{
    private int[,] grid;
    private int rows;
    private int columns;

    public Grid(int rows, int columns)
    {
        this.rows = rows;
        this.columns = columns;
        grid = new int[rows, columns];

        InitializeGrid();
    }

    private void InitializeGrid()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                grid[i, j] = '\0';
            }
        }
    }

    public void DisplayGrid()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Console.Write(grid[i, j] + " ");
            }
            Console.WriteLine();
        }
    }

    public void UpdateCell(int row, int column, int value)
    {
        if (row >= 0 && row < rows && column >= 0 && column < columns)
        {
            grid[row, column] = value;
        }
        else
        {
            Console.WriteLine("Coordonnées hors limites.");
        }
    }

    public int GetCell(int row, int column)
    {
        if (row >= 0 && row < rows && column >= 0 && column < columns)
        {
            return grid[row, column];
        }
        else
        {
            Console.WriteLine("Coordonnées hors limites.");
            return -1;
        }
    }

    public int[,] GetGrid()
    {
        return grid;
    }
}
