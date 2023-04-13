// See https://aka.ms/new-console-template for more information
using SeaBattle2.Cli;
using System.Text;

Console.WriteLine("Hello, World!");

var grid = Grid.Initialize(new List<Ship> {Ship.Horizontal(new Cell(0, 0), 4)});
var grid2 = Grid.Initialize(new List<Ship> { Ship.Horizontal(new Cell(0, 0), 4) });

var board = Board.Initialize(grid, grid2);

while (!board.GameEnded)
{
    PrintBoard(board);

    Console.WriteLine("Current player: " + board.CurrentTurn);

    var point = ConsoleHelper.GetPointAnswer();

    board = board.Shoot(point.X, point.Y);
}
PrintBoard(board);

Console.WriteLine("Game over.");

static void PrintBoard(Board board)
{
    var sb = new StringBuilder();

    var firstPlayerName = "Player 1";

    var domainRow = Enumerable.Range(0, Constants.GridDimension).Aggregate("", (tmp, x) => $"{tmp} {x}");
    var difference = domainRow.Length - firstPlayerName.Length;

    var freeSpaceBetweenFields = difference < 0 ? new string(' ', -difference) : "";

    var firstView = Prepare(board.First);
    var secondView = Prepare(board.Second);

    sb.Append("\t  " + domainRow + freeSpaceBetweenFields + "\t\t  " + domainRow + "\n");

    for (int i = 0; i < Constants.GridDimension; i++)
    {
        sb.Append($"\t{i}|");

        sb.Append(DrawFieldRow(firstView, i));

        sb.Append($"{freeSpaceBetweenFields}\t\t{i}|");

        sb.Append(DrawFieldRow(secondView, i));

        sb.Append("\n");
    }

    Console.WriteLine(sb.ToString());
}

static char[,] Prepare(Grid grid)
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

    return output;
}


static string DrawFieldRow(char[,] field, int row)
{
    var builder = new StringBuilder();

    for (int column = 0; column < Constants.GridDimension; column++)
    {
        builder.Append(' ');

        builder.Append(field[row, column]);
    }

    return builder.ToString();
}