using System.Collections.Immutable;

namespace SeaBattle2.Cli;

public class Board
{
    public Grid First { get; }
    public Grid Second { get; }

    public enum Turn
    {
        Player1,
        Player2
    }

    public Turn CurrentTurn { get; }

    private Grid CurrentPlayerGrid => CurrentTurn == Turn.Player1 ? First : Second;

    private Board(Grid first, Grid second, Turn nextTurn) => (First, Second, CurrentTurn) = (first, second, nextTurn);

    public bool GameEnded => !(First.PlayableCells.Any() && Second.PlayableCells.Any());

    public static Board Initialize(Grid first, Grid second) => new(first, second, Turn.Player1);

    public Board Shoot(int row, int column) =>
        GameEnded
            ? new Board(First, Second, CurrentTurn)
            : new Board(
                TryShoot(row, column, First),
                TryShoot(row, column, Second),
                NextTurn);

    private Grid TryShoot(int row, int column, Grid grid) => 
        grid == CurrentPlayerGrid
            ? grid.Shoot(row, column)
            : grid;

    private Turn NextTurn => CurrentTurn == Turn.Player1 ? Turn.Player2 : Turn.Player1;
}

public class Grid
{
    private Grid(ImmutableList<Ship> ships) => Ships = ships;
    private Grid(ImmutableList<Ship> ships, ImmutableList<Cell> shots) =>
        (Ships, Shots) = (ships, shots);

    public IEnumerable<Cell> Hits =>
        Shots.Intersect(
            Ships.SelectMany(s => s.Cells));

    public IEnumerable<Cell> Misses => Shots.Except(Hits);
    
    public IEnumerable<Cell> PlayableCells =>
        Ships.Except(DrownShips).Any()
            ? Cell.FullGrid().Except(Shots)
            : Enumerable.Empty<Cell>();

    private ImmutableList<Ship> Ships { get; }
    private ImmutableList<Cell> Shots { get; } = ImmutableList<Cell>.Empty;
    private IEnumerable<Ship> DrownShips => Ships.Where(IsDrown);

    public static Grid Initialize(IEnumerable<Ship> ships) => new(ships.ToImmutableList());

    public Grid Restart() => new(Ships);

    public Grid Shoot(int row, int column) =>
        Shoot(new Cell(row, column));

    private Grid Shoot(Cell at) =>
        PlayableCells
            .Where(at.Equals)
            .Select(empty => new Grid(
                Ships, 
                Shots
                    .Add(empty)
                    .AddRange(DrownShipArea(empty))))
            .DefaultIfEmpty(this)
            .First();

    private IEnumerable<Cell> DrownShipArea(Cell at) =>
        Ships
            .FirstOrDefault(s => s.Cells.Contains(at) && s.Cells.Except(at.Yield()).SubsetOf(Shots))?
                .NeighborCells
                .Except(Shots)
            ?? Enumerable.Empty<Cell>();

    private bool IsDrown(Ship ship) => ship.Cells.SubsetOf(Shots);
}

public record Cell(int Row, int Column)
{
    private IEnumerable<Cell> AllNeighbors
    {
        get
        {
            yield return new Cell(Row - 1, Column - 1);
            yield return new Cell(Row - 1, Column);
            yield return new Cell(Row - 1, Column + 1);
            yield return new Cell(Row, Column - 1);
            yield return new Cell(Row, Column + 1);
            yield return new Cell(Row + 1, Column - 1);
            yield return new Cell(Row + 1, Column);
            yield return new Cell(Row + 1, Column + 1);
        }
    }

    private bool InRange => Row >= 0 && Column >= 0 && Row <= Constants.LastCellOffset && Column <= Constants.LastCellOffset;

    public IEnumerable<Cell> Neighbors => AllNeighbors.Where(c => c.InRange);

    public static IEnumerable<Cell> FullGrid() =>
        Enumerable.Range(0, Constants.GridSize)
            .Select(index => new Cell(
                index / Constants.GridDimension,
                index % Constants.GridDimension));

    public static implicit operator Cell((int row, int column) tuple) => new(tuple.row, tuple.column);
}

public record Ship
{
    private Ship(Cell from, Cell to) => (From, To) = (from, to);

    private int Size => Math.Max(To.Column - From.Column, To.Row - From.Row);
    
    public Cell From { get; }
    public Cell To { get; }

    public static Ship Vertical(Cell at, int size) => new(at, at with {Row = at.Row + size});

    public static Ship Horizontal(Cell at, int size) => new(at, at with {Column = at.Column + size});

    public IEnumerable<Cell> Cells =>
        From.Row == To.Row
            ? Enumerable.Range(From.Column, Size).Select(column => From with {Column = column})
            : Enumerable.Range(From.Row, Size).Select(row => From with {Row = row});

    public IEnumerable<Cell> NeighborCells =>
        Cells
            .SelectMany(c => c.Neighbors)
            .Distinct()
            .Except(Cells);
}

public static class EnumerableExt
{
    /// <summary>
    /// Wraps this object instance into an IEnumerable&lt;T&gt;
    /// consisting of a single item.
    /// </summary>
    /// <typeparam name="T"> Type of the object. </typeparam>
    /// <param name="item"> The instance that will be wrapped. </param>
    /// <returns> An IEnumerable&lt;T&gt; consisting of a single item. </returns>
    public static IEnumerable<T> Yield<T>(this T item)
    {
        yield return item;
    }

    /// <summary>
    /// Checks whether the first sequence is a subset of the second
    /// </summary>
    /// <typeparam name="T"> Type of the object. </typeparam>
    /// <param name="first"> The first sequence checked to be a subset. </param>
    /// <param name="second"> The second sequence checked to be a superset. </param>
    /// <returns> true if the first sequence is a subset of the second; otherwise, false. </returns>
    public static bool SubsetOf<T>(this IEnumerable<T> first, IEnumerable<T> second) =>
        !first.Except(second).Any();
}