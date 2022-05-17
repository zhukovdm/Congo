# Congo API

- [Congo API](#congo-api)
- [Congo.Utils](#congoutils)
  - [UserInput.cs](#userinputcs)
- [Congo.Core](#congocore)
  - [IParametrizedEnum.cs](#iparametrizedenumcs)
  - [IUserInterface.cs](#iuserinterfacecs)
  - [BitScan.cs](#bitscancs)
  - [Square.cs](#squarecs)
  - [Color.cs](#colorcs)
  - [Move.cs](#movecs)
  - [Piece.cs](#piececs)
  - [Board.cs](#boardcs)
  - [Player.cs](#playercs)
  - [Fen.cs](#fencs)
  - [Game.cs](#gamecs)
  - [HashTable.cs](#hashtablecs)
  - [Evaluator.cs](#evaluatorcs)
  - [Algorithm.cs](#algorithmcs)
- [Congo.CLI](#congocli)
  - [ArgumentParser.cs](#argumentparsercs)
  - [CommandLine.cs](#commandlinecs)
  - [Program.cs](#programcs)
- [Congo.GUI](#congogui)
  - [MenuLocalPopup.xaml](#menulocalpopupxaml)
  - [MenuNetworkPopup.xaml](#menunetworkpopupxaml)
  - [MainWindow.xaml](#mainwindowxaml)

# Congo.Utils

## UserInput.cs

```txt
public static class UserInput
    public static bool IsUserNameValid(string name)
    public static bool IsIpAddressHolderValid(string holder)
    public static bool IsPortValid(string port)
```

# Congo.Core

## IParametrizedEnum.cs

```txt
public interface IParametrizedEnumerator<out T>
    T Current;
    bool MoveNext()

public interface IParametrizedEnumerable<in T, out U>
    IParametrizedEnumerator<U> GetEnumerator(T param)
```

## IUserInterface.cs

```txt
public interface ICongoUserInterface
    CongoMove GetHiMove(CongoGame game)
    void ReportWrongHiMove()
```

## BitScan.cs

```txt
public static class BitScan
    private static readonly ulong magicNumber;
    private static readonly ImmutableArray<int> magicHash
    public static int DeBruijnLsb(ulong word)

public class BitScanEnumerator : IParametrizedEnumerator<int>
    private ulong occupancy
    public BitScanEnumerator(ulong occupancy)
    public int Current
    public bool MoveNext()
```

## Square.cs

```txt
public enum Square : int
```

## Color.cs

```txt
public abstract class CongoColor
    public static bool operator ==(CongoColor c1, CongoColor c2)
    public static bool operator !=(CongoColor c1, CongoColor c2)
    private protected enum ColorId : int
    private protected abstract ColorId Id
    internal int Code
    public bool IsWhite()
    public bool IsBlack()
    public CongoColor Invert()
    public override bool Equals(object o)
    public override int GetHashCode()

public sealed class White : CongoColor
    public static CongoColor Color
    private White()
    private protected override ColorId Id

public sealed class Black : CongoColor
    public static CongoColor Color
    private Black()
    private protected override ColorId Id
```

## Move.cs

```txt
public class CongoMove
    public static int Compare(CongoMove x, CongoMove y)
    public static bool operator ==(CongoMove m1, CongoMove m2)
    public static bool operator !=(CongoMove m1, CongoMove m2)
    public readonly int Fr
    public readonly int To
    public CongoMove(int fr, int to)
    public override bool Equals(object o)
    public override int GetHashCode()
    public override string ToString()

public class MonkeyJump : CongoMove
    public readonly int Bt
    public MonkeyJump(int fr, int bt, int to)
    public override string ToString()

public class CongoMoveObjComparer : IComparer
    public int Compare(object x, object y)

public class CongoMoveGenComparer : IComparer<CongoMove>
    public int Compare(CongoMove x, CongoMove y)
```

## Piece.cs

```txt
public abstract class CongoPiece
    private protected enum PieceId : uint
    private protected abstract PieceId Id
    internal uint Code
    protected List<CongoMove> GetValidCapturingLeaps(List<CongoMove> moves, ImmutableArray<int> capturingLeaps, CongoColor color, CongoBoard board, int square)
    protected List<CongoMove> GetValidNonCapturingLeaps(List<CongoMove> moves, ImmutableArray<int> nonCapturingLeaps, CongoBoard board, int square)
    public bool IsAnimal()
    public bool IsCrocodile()
    public bool IsPawn()
    public bool IsSuperpawn()
    public bool IsLion()
    public bool IsMonkey()
    public abstract List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)

public sealed class Ground : CongoPiece
    public static CongoPiece Piece
    private Ground()
    private protected override PieceId Id
    public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)

public sealed class River : CongoPiece
    public static CongoPiece Piece
    private River()
    private protected override PieceId Id
    public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)

public sealed class Elephant : CongoPiece
    public static CongoPiece Piece
    private Elephant()
    private protected override PieceId Id
    public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)

public sealed class Zebra : CongoPiece
    public static CongoPiece Piece
    private Zebra()
    private protected override PieceId Id
    public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)

public sealed class Giraffe : CongoPiece
    public static CongoPiece Piece
    private Giraffe()
    private protected override PieceId Id
    public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)

public sealed class Crocodile : CongoPiece
    public static CongoPiece Piece
    private Crocodile()
    private protected override PieceId Id
    private List<CongoMove> CapturingRiverSlide(List<CongoMove> moves, CongoColor color, CongoBoard board, int square, int direction)
    private List<CongoMove> CapturingGroundSlide(List<CongoMove> moves, CongoColor color, CongoBoard board, int square)
    public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)

public sealed class Pawn : CongoPiece
    public static CongoPiece Piece
    private Pawn()
    private protected override PieceId Id
    protected List<CongoMove> NonCapturingVerticalSlide(List<CongoMove> moves, CongoColor color, CongoBoard board, int square, bool s)
    public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)

public sealed class Superpawn : Pawn
    public static CongoPiece Piece
    private Superpawn()
    private protected override PieceId Id
    private List<CongoMove> NonCapturingDiagonalSlide(List<CongoMove> moves, CongoBoard board, int square, int rdir, int fdir)
    public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)

public sealed class Lion : CongoPiece
    public static CongoPiece Piece
    private Lion()
    private protected override PieceId Id
    private List<CongoMove> VerticalJump(List<CongoMove> moves, CongoColor color, CongoBoard board, int square)
    private List<CongoMove> DiagonalJump(List<CongoMove> moves, CongoColor color, CongoBoard board, int square)
    public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)

public sealed class Monkey : CongoPiece
    public static CongoPiece Piece
    private Monkey()
    private protected override PieceId Id
    private List<CongoMove> AddMonkeyJump(List<CongoMove> moves, CongoBoard board, int square, int leap)
    public List<CongoMove> ContinueJump(CongoColor color, CongoBoard board, int square)
    public override List<CongoMove> GetMoves(CongoColor color, CongoBoard board, int square)
```

## Board.cs

```txt
public sealed class CongoBoard : IParametrizedEnumerable<CongoColor, int>
    private static readonly int size
    private static readonly CongoBoard empty
    private static readonly ImmutableArray<CongoPiece> pieceSample
    private static bool GetBit(ulong word, int position)
    private static ulong SetBitToValue(ulong current, int position, bool value)
    private static bool IsJungleImpl(int rank, int file)
    private static bool IsJungleImpl(int square)
    private static bool IsAboveRiverImpl(int square)
    private static bool IsRiverImpl(int square)
    private static bool IsBelowRiverImpl(int square)
    private static uint SetPieceCode(uint rank, CongoPiece piece, int file)
    public static CongoBoard Empty
    public static ImmutableArray<CongoPiece> PieceSample
    public static bool operator ==(CongoBoard b1, CongoBoard b2)
    public static bool operator !=(CongoBoard b1, CongoBoard b2)
    private delegate ImmutableArray<int> LeapGenerator(int rank, int file)
    private delegate ImmutableArray<int> ColoredLeapGenerator(CongoColor color, int rank, int file)
    private static bool AddLeap(List<int> leaps, int rank, int file)
    private static ImmutableArray<int> CircleLeapGenerator(Func<int, int, bool> predicate, int rank, int file, int radius)
    private static ImmutableArray<int> KingLeapGenerator(int rank, int file)
    private static ImmutableArray<int> KnightLeapGenerator(int rank, int file)
    private static ImmutableArray<int> ElephantLeapGenerator(int rank, int file)
    private static ImmutableArray<int> CapturingGiraffeLeapGenerator(int rank, int file)
    private static ImmutableArray<int> CrocodileLeapGenerator(int rank, int file)
    private static readonly ImmutableHashSet<int> whiteLionCastle
    private static readonly ImmutableHashSet<int> blackLionCastle
    private static ImmutableArray<int> LionLeapGenerator(CongoColor color, int rank, int file)
    private static ImmutableArray<int> PawnLeapGenerator(CongoColor color, int rank, int file)
    private static ImmutableArray<int> SuperpawnLeapGenerator(CongoColor color, int rank, int file)
    private static ImmutableArray<ImmutableArray<int>> PrecalculateLeaps(LeapGenerator gen)
    private static ImmutableArray<ImmutableArray<int>> PrecalculateLeaps(ColoredLeapGenerator gen, CongoColor color)
    private static readonly ImmutableArray<ImmutableArray<int>> kingLeaps
    private static readonly ImmutableArray<ImmutableArray<int>> knightLeaps
    private static readonly ImmutableArray<ImmutableArray<int>> elephantLeaps
    private static readonly ImmutableArray<ImmutableArray<int>> capturingGiraffeLeaps
    private static readonly ImmutableArray<ImmutableArray<int>> crocodileLeaps
    private static readonly ImmutableArray<ImmutableArray<int>> whiteLionLeaps
    private static readonly ImmutableArray<ImmutableArray<int>> blackLionLeaps
    private static readonly ImmutableArray<ImmutableArray<int>> whitePawnLeaps
    private static readonly ImmutableArray<ImmutableArray<int>> blackPawnLeaps
    private static readonly ImmutableArray<ImmutableArray<int>> whiteSuperpawnLeaps
    private static readonly ImmutableArray<ImmutableArray<int>> blackSuperpawnLeaps
    static CongoBoard()
    private readonly ulong whiteOccupied
    private readonly ulong blackOccupied
    private readonly ImmutableArray<int> pieces
    private CongoBoard(ulong whiteOccupied, ulong blackOccupied, ImmutableArray<uint> pieces)
    private uint GetPieceCode(int square)
    public int Size
    public bool IsJungle(int rank, int file)
    public bool IsJungle(int square)
    public bool IsAboveRiver(int square)
    public bool IsRiver(int square)
    public bool IsBelowRiver(int square)
    public bool IsUpDownBorder(CongoColor color, int square)
    public bool IsOccupied(int square)
    public bool IsWhitePiece(int square)
    public bool IsBlackPiece(int square)
    public bool IsFirstMovePiece(int square)
    public bool IsFriendlyPiece(CongoColor color, int square)
    public bool IsOpponentPiece(CongoColor color, int square)
    public CongoPiece GetPiece(int square)
    public CongoBoard With(CongoColor color, CongoPiece piece, int square)
    public CongoBoard Without(int square)
    public ImmutableArray<int> LeapsAsKing(int square)
    public ImmutableArray<int> LeapsAsKnight(int square)
    public ImmutableArray<int> LeapsAsElephant(int square)
    public ImmutableArray<int> LeapsAsCapturingGiraffe(int square)
    public ImmutableArray<int> LeapsAsCrocodile(int square)
    public ImmutableArray<int> LeapsAsLion(CongoColor color, int square)
    public ImmutableArray<int> LeapsAsPawn(CongoColor color, int square)
    public ImmutableArray<int> LeapsAsSuperpawn(CongoColor color, int square)
    public bool TryDiagonalJump(CongoColor color, int square, out int target)
    public bool IsLionCastle(CongoColor color, int square)
    public IParametrizedEnumerator<int> GetEnumerator(CongoColor color)
    public override bool Equals(object obj)
    public override int GetHashCode()
```

## Player.cs

```txt
public abstract class CongoPlayer
    protected readonly int lionSquare
    protected readonly CongoColor color
    protected readonly ImmutableArray<CongoMove> moves
    public CongoPlayer(CongoColor color, CongoBoard board, MonkeyJump firstMonkeyJump)
    public ImmutableArray<CongoMove> Moves
    public bool HasLion
    public CongoColor Color
    public abstract CongoPlayer With(CongoBoard board, MonkeyJump firstMonkeyJump)
    public bool LionInDanger(ImmutableArray<CongoMove> opponentMoves)
    public CongoMove Accept(CongoMove candidateMove)

public class Ai : CongoPlayer
    public Ai(CongoColor color, CongoBoard board, MonkeyJump firstMonkeyJump)
    public override CongoPlayer With(CongoBoard board, MonkeyJump firstMonkeyJump)

public class Hi : CongoPlayer
    public Hi(CongoColor color, CongoBoard board, MonkeyJump firstMonkeyJump)
    public override CongoPlayer With(CongoBoard board, MonkeyJump firstMonkeyJump)
```

## Fen.cs

```txt
public static class CongoFen
    private static readonly string pieceSignatures
    private static readonly ImmutableDictionary<char, CongoPiece> view2piece
    private static CongoColor GetActivePlayerColor(string color)
    private static MonkeyJump GetFirstMonkeyJump(string input)
    private static CongoBoard AddPiece(CongoBoard board, CongoColor color, char pieceView, int square, ref int file)
    private static CongoPlayer GetPlayer(CongoBoard board, string type, CongoColor color, MonkeyJump firstMonkeyJump)
    public static CongoGame FromFen(string fen)
    public static string ToFen(CongoGame game)
```

## Game.cs

```txt
public class Game
    public static void Initialize()
    public static CongoGame Unattached(CongoBoard board, CongoPlayer whitePlayer, CongoPlayer blackPlayer, CongoPlayer activePlayer, MonkeyJump firstMonkeyJump)
    private static CongoBoard SetMixedRank(CongoBoard board, CongoColor color, int rank)
    private static CongoBoard SetPawnRank(CongoBoard board, CongoColor color, int rank)
    public static CongoGame Standard(Type whitePlayerType, Type blackPlayerType)
    private readonly int distance
    private readonly CongoGame predecessor
    private readonly CongoMove transitionMove
    private readonly CongoBoard board
    private readonly CongoPlayer whitePlayer
    private readonly CongoPlayer blackPlayer
    private readonly CongoPlayer activePlayer
    private readonly MonkeyJump firstMonkeyJump
    private bool IsInterruptedMonkeyJump(CongoMove move)
    private bool IsPawnPromotion(CongoMove move)
    private bool IsFriendlyAnimal(CongoPiece piece, CongoColor color)
    private CongoGame(CongoGame predecessor, CongoMove transitionMove, CongoBoard board, CongoPlayer whitePlayer, CongoPlayer blackPlayer, CongoPlayer activePlayer, MonkeyJump firstMonkeyJump)
    public CongoGame Predecessor
    public CongoMove TransitionMove
    public CongoBoard Board
    public CongoPlayer WhitePlayer
    public CongoPlayer BlackPlayer
    public CongoPlayer ActivePlayer
    public CongoPlayer Opponent
    public CongoMove FirstMonkeyJump
    public CongoGame Transition(CongoMove move)
    public bool IsNew()
    public bool IsInvalid()
    public bool IsWin()
    public bool HasEnded()
```

## HashTable.cs

```txt
class CongoHashCell
    public ulong Hash
    public CongoBoard Board
    public CongoMove Move
    public int Depth
    public int Score

class CongoHashTable
    private static readonly int colorFactor
    private static readonly int pieceFactor
    private static readonly int boardFactor
    private static readonly int colorSpan
    private static readonly ImmutableArray<ulong> hashValues
    private static readonly int tableSize
    private static readonly ulong mask
    static CongoHashTable()
    private static ulong AddPiece(ulong hash, CongoPiece piece, CongoColor color, int square)
    private static ulong RemovePiece(ulong hash, CongoPiece piece, CongoColor color, int square)
    public static ulong ApplyMove(ulong hash, CongoBoard board, CongoMove move)
    public static ulong ApplyBetween(ulong hash, CongoBoard board, MonkeyJump jump)
    public static ulong InitHash(CongoBoard board)
    private CongoHashCell[] table
    public CongoHashTable()
    public bool TryGetSolution(ulong hash, CongoBoard board, int depth, out CongoMove move, out int score)
    public void SetSolution(ulong hash, CongoBoard board, int depth, CongoMove move, int score)
```

## Evaluator.cs

```txt
public static class Evaluator
    public static int INF
    public static int WinLose(CongoGame game)
    private static readonly ImmutableArray<int> materialValues
    private static int ScoreByColor(CongoColor color, CongoBoard board)
    public static int Material(CongoGame game)
    public static int Default(CongoGame game)
```

## Algorithm.cs

```txt
public static class Algorithm
    private static readonly Random rnd
    public static CongoMove Rnd(CongoGame game)
    private static readonly int negamaxDepth
    private static CongoHashTable hT
    private static (CongoMove, int) Max((CongoMove, int score) p1, (CongoMove, int score) p2)
    private static (CongoMove, int) NegamaxSingleThread(ulong hash, CongoGame game, ImmutableArray<CongoMove> moves, int alpha, int beta, int depth)
    private static (CongoMove, int) NegamaxMultiThread(ulong hash, CongoGame game, int depth)
    public static CongoMove NegaMax(CongoGame game)
```

# Congo.CLI

## ArgumentParser.cs

```txt
public static class ArgumentParser
    private static string[] AcceptLocalPlace(string arg)
    private static string[] AcceptLocalBoard(string arg)
    private static string[] AcceptNetworkPlace(string arg)
    private static string[] AcceptNetworkBoard(string arg)
    private static string[] AcceptNetworkHost(string arg)
    private static string[] AcceptNetworkPort(string arg)
    private static string[] AcceptPlayer(string arg)
    private static ImmutableDictionary<string, string[]> TryParse(string[] args, Dictionary<string, Func<string, string[]>> acceptors, int cnt)
    public static ImmutableDictionary<string, string[]> Parse(string[] args)
```

## CommandLine.cs

```txt
public abstract class CongoCommandLine : IDisposable
    private static readonly string resourcesFolder
    private static readonly string textFileExt
    private static readonly TextReader reader
    private static readonly TextWriter writer
    private delegate string[] VerifyCommandDelegate(string[] input)
    private static readonly ImmutableDictionary<string, VerifyCommandDelegate> supportedCommands
    private delegate CongoMove AlgorithmDelegate(CongoGame game)
    private static readonly ImmutableDictionary<string, AlgorithmDelegate> supportedAlgorithms
    private static readonly ImmutableList<string> squareViews
    private static readonly ImmutableList<Type> pieceTypes
    private static readonly ImmutableDictionary<Type, string> pieceViews
    private delegate void ShowDelegate(CongoGame game)
    private static readonly ImmutableDictionary<string, ShowDelegate> supportedShows
    private static string GetMoveView(CongoMove move)
    private static string ReadTextFile(string filename)
    protected static void ReportAllowedCommands(List<string> allowedCommands)
    private static void ReportHelpFile(string helpFile)
    private static void ReportEmptyCommand()
    private static void ReportNotSupportedCommand(string command)
    private static void ReportWrongCommandFormat(string command)
    private static void ReportNotAllowedCommand(string command)
    private static void ReportAdvisedMove(CongoMove move, string algorithm)
    private static int[] CountPieces(CongoBoard board, CongoColor color)
    private static void ShowTransition(CongoMove game)
    private static void ShowBoardImpl(CongoGame game)
    private static void ShowPlayer(CongoBoard board, CongoColor color, CongoColor activeColor)
    private static void ShowPlayers(CongoGame game)
    private static void ShowMoves(CongoGame game)
    private static void ShowGame(CongoGame game)
    private static string[] VerifyCommand(Func<string[], bool> predicate, string[] input)
    private static string[] VerifyAdviseCommand(string[] input)
    private static string[] VerifyAllowCommand(string[] input)
    private static string[] VerifyConnectCommand(string[] input)
    private static string[] VerifyExitCommand(string[] input)
    private static string[] VerifyGameCommand(string[] input)
    private static string[] VerifyHelpCommand(string[] input)
    private static string[] VerifyMoveCommand(string[] input)
    private static string[] VerifyPlayCommand(string[] input)
    private static string[] VerifyShowCommand(string[] input)
    private static void Greet()
    protected static string[] GetUserCommand(List<string> allowedCommands)
    protected static Type GetPlayerType(CongoColor color)
    public static CongoCommandLine SetCommandLine()
    public CongoMove GetHiMove(CongoGame game)
    public void ReportWrongHiMove()
    public void ShowBoard(CongoGame game)
    public abstract CongoGame SetGame()
    public abstract CongoGame WaitResponse(CongoGame game)
    public void ReportResult(CongoGame game)
    public abstract void Dispose()

public class LocalCommandLine : CongoCommandLine
    public override CongoGame SetGame()
    public override CongoGame WaitResponse(CongoGame game)
    public override void Dispose()

public class NetworkCommandLine : CongoCommandLine
    public override CongoGame SetGame()
    public override CongoGame WaitResponse(CongoGame game)
    public override void Dispose()
```

## Program.cs

```txt
class Program
    static void Main(string[] args)
```

# Congo.GUI

## MenuLocalPopup.xaml

```txt

```

## MenuNetworkPopup.xaml

```txt

```

## MainWindow.xaml

```txt
public static class TileExtensions
    private static readonly double tileSize
    private static readonly ImmutableDictionary<Type, string> type2suffix
    public static Canvas WithMoveFrBorder(this Canvas tile)
    public static Canvas WithMoveToBorder(this Canvas tile)
    public static Canvas WithStandardBorder(this Canvas tile)
    public static Canvas WithPiece(this Canvas tile, CongoColor color, Type type)

enum State : int { INIT, FR, TO, END }

public partial class MainWindow : Window
    private IEnumerable<CongoMove> getMovesFr(int fr)
    private void replaceTile(int idx, Canvas tile)
    private Canvas getEmptyTile(int idx)
    private void tile_Click(object sender, RoutedEventArgs e)
```
