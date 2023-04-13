// See https://aka.ms/new-console-template for more information
using SeaBattle2.Cli;

Console.WriteLine("Hello, World!");

var grid = Grid.Initialize(new List<Ship> {Ship.Horizontal(new Cell(0, 0), 4)});

while (grid.PlayableCells.Any())
{
    WriteGrid(grid);

    var point = ConsoleHelper.GetPointAnswer();

    grid = grid.Shoot(point.X, point.Y);
}
WriteGrid(grid);

Console.WriteLine("Game over.");

static void WriteGrid(Grid grid)
{
    var output = new char[Constants.GridDimension, Constants.GridDimension];

    foreach (var (row, column) in grid.PlayableCells)
    {
        output[row, column] = ' ';
    }

    foreach (var (row, column) in grid.Hits)
    {
        output[row, column] = 'X';
    }

    foreach (var (row, column) in grid.Misses)
    {
        output[row, column] = '·';
    }

    for (var i = 0; i < output.GetLength(0); i++)
    {
        for (int j = 0; j < output.GetLength(1); j++)
        {
            Console.Write(output[i, j]);
        }
        Console.WriteLine();
    }
}