# Programmer's Guide

The purpose of this document is to describe application architecture,
algorithms and data structures used throughout the `Congo` project.

# Architecture

The solution is divided into several projects. Functional projects are
tested within supporting projects with `.MSTest` suffix in the name, such as
`Congo.Utils.MSTest`, `Congo.Core.MSTest` and `Congo.CLI.MSTest`.

## Project dependencies

| Project | Dependencies |
|---|---|
| Congo.Utils | - |
| Congo.Core | - |
| Congo.Server | Congo.Core |
| Congo.CLI | Congo.Utils, Congo.Core |
| Congo.GUI | Congo.Utils, Congo.Core |

Both `Congo.CLI` and `Congo.GUI` reference the same `congo.proto` service file
from `Congo.Server` project. Denendencies on third party `NuGet` packages
can be consulted [here](https://github.com/zhukovdm/Congo/network/dependencies).

## Congo.Assets

The project accomodates **only** artifacts, such as documentation and pictures.
It doesn't contain any source code.

## Congo.Utils

`Congo.Utils` implements simple supporting functions shared by user interface
implementations.

## Congo.Core

Most of the implemented data structures are immutable to provide better
programmer experience and completely avoid do/undo operations, that could
cause annoying bugs.

Possibly expensive operations on types, such as `typeof(..)`, are generally 
avoided within `Congo.Core`.

`Congo.Core` provides implementations of all essential parts for constructing
an instance of the game, such as `CongoPiece`, `CongoPlayer`, `CongoBoard`,
`CongoGame` and `CongoUser`.

### CongoPiece

This is an abstract class used for valid move generation for each kind
of piece in the game. `CongoPiece` descendants implement `Singleton` pattern.
The basic functionality is to calculate valid moves being provided with the
color, board and current position. Each descendant has its own specific
procedure to calculate moves.

### CongoPlayer

Instances of this class used for collecting moves valid for a particular
player color and current board. In the constructor, player iterate over all
pieces on the board and concatenate results into a larger list of moves. It
finds where the lion is located. The `Accept(move)` method tries to find
similar move in the list of all available moves and return it upon success,
otherwise `null` is returned. Move replacement enables retrieving proper
monkey jumps.

### CongoBoard

This class simulates a `Congo` board with pieces on it. Internally, the
instance of the board is represented by three pieces of information. First
two are `ulong` words `white-` and `blackOccupied` telling, whether particular
square is occupied and by which player. The third is an immutable array of $7$
`uint` words representing one rank of the board. There are $10$ kinds of pieces.
Each piece could be encoded in $4$-bit value, only $28$ bits of information are
necessary to encode the entire rank. Therefore, piece addition and removal
is implemented via simple bitwise operations.

The board class contains precalculated possible **leaps** for each type of
piece and each position. Lambda functions are heavily used in this calculation.
As a consequence, generation of the valid leaps is a simple iteration over all
possible for a given position and piece. Not all valid moves are leaps, some
of the pieces **slide** and such moves shall be generated each time.

### CongoGame

Class is a container for a game snapshot with references to the current
board and players. The most important instance method of this class
is `Transition(CongoMove move)`, given valid move it constructs new game with
new board and new players. All parts of the game are immutable, once the game
is created, nothing can be modified in it. Such property is a great helper
in concurrent calculations.

### CongoUser

`CongoUser` is a typed representation of the playing user. Three types are
distinguished, namely `Hi`, `Ai` and `Net`. Each user may enforce the board to
advise using an algorithm provided in the constructor. Artificial agent picks
this move to proceed. Implementation of a `Net` user behavior is a responsibility
of a user interface.

## Congo.CLI

`Congo.CLI` project implements simple command line interface. Commands
are parsed, validated and executed by a corresponding delegates retrieved from 
predefined hash tables. Lambda functions are heavily used in verification.

The project is platform independent and could be configured as local or
network application.

## Congo.GUI

The project implements graphical user interface based on `WPF` technology.
It provides functionality similar to the `Congo.CLI` project.

The project is inherently platform dependent and could be configured as local
or network application. Local game enforces players to alternate turns, network
version request input from the `Congo` server via `gRPC` channel.

## Congo.Server

TBA

# Algorithms and data structures

## Alpha-beta negamax

Minimax and negamax are recursive algorithms and we further deal with a notion
of the distance between two nodes in the decision tree. The distance is determined
by **the number of edges** between nodes.

### Minimax, introduction

In the original implementation of the [Minimax](https://en.wikipedia.org/wiki/Minimax)
algorithm, one of the players (white) maximizes, another (black) minimizes the
value given by the evaluation function. Score is calculated with respect to both
players. The value could be either `+high`, `0` or `-low`. For white player, the
higher score is the better. On the contrary, for black the lower score is the
better. Such implementation is rather verbose, therefore we implement concise
[Negamax](https://en.wikipedia.org/wiki/Negamax) variant.

### Negamax, leaf node

This paragraph describes how board score is determined and passed within
decision tree with respect to `Minimax` algorithm. `Negamax` deals with both
players equally, both maximize. To achieve such behavior we should properly
adjust score at the leaf node and pass obtained score in a specific way.

Lets proceed with simple example. Suppose evaluation function returns `+1` if
white player wins, `-1` if black player wins, otherwise `0`. White player makes
winning move and calls `Negamax` with depth `0`. Black player recognize that
the game is over and returns `+1`. White player goes through all returned values
and selects maximum value if it appears.

Consider different situation.The same evaluation function, but black player
checks winning move and neutral move (neither winning nor loosing) and calls

Negamax. For the
first move the board is evaluated to `-1`, and for the second to `0`. Maximum of
two values is `0`, so black would skip the best option if maximizes. We should
have inverted obtained score at the leaf node before the value is returned.
Inverted `0` becomes 0, but `-1` becomes `+1`. Smaller values at the leaf nodes
become larger values when returned.

Therefore, black active player (or white opponent) at a terminal node returns
`+score`, white active player (or black opponent) at a terminal node returns
`-score`.

### Negamax, internal node

Suppose, there is a white player from the previous paragraph. It found winning
move at the leaf node. Let black player be above white player within the
decision tree. If white player returns +1 (maximum possible value) and black
maximizes, then black would choose worst possible value and eventually move.
The best values returned from nodes below should be represented as the worst
values for the opponent. Therefore, before value is returned, it should be
inverted (* --1), so that the player above could maximize.

### Negamax, monkey jumps

Another complication is that monkey jump does change recursion depth, but
does not change the color of the player. Therefore, we should consider
returning +/-- score based on the color of active player in the predecessor
game.

### Alpha-beta pruning

This is a standard fail-hard beta cut-off implementation of the algorithm.
$\alpha$ is the best possible solution found so far, $\beta$ is the best
possible solution the opponent could ensure.

## Hash table

`Hash table` is used for identifying transposed boards. Such board could appear
in case the board returns to the original state after a cycle of moves. Hash
table enables an algorithm traversing game tree to recognize such boards and
use already precalculated best score and move. The hashed cell is recognized
as hash hit in case current board has equal or smaller sub-tree in terms of
recursion depth than the board found in the hash. Otherwise, the current board
traverses all valid moves leading to the recursion call. Internally, the hash
table has $2^{18}$ possible cells. Table could be accessed by several threads
concurrently, locks are implemented for any `R/W` operations on cells.

\vspace{0.5em}

Hash eviction policy is implemented based on the board equality. Once the best
move is found in Negamax, the method \textsf{.SetSolution(...)} is called on
the table instance. The board, move and score are stored in case boards are
not equal or better solution is found. Board equality is an efficient
operation, it compares 2 \textsf{ulong} values and 7 \textsf{uint} values.

The hash code of the cell is determined based on a board hash value. Hash is
calculated as proposed by [Albert Lindsey Zobrist](https://en.wikipedia.org/wiki/Albert_Lindsey_Zobrist).
First, pseudo-random values are generated for each `(color, piece, square)` triple.
Consider moving white pawn from `A1` to `A3`. We remove hash of triple
(white, pawn, A1) from the current board hash. Then we add no-piece to the
board hash (?, no-piece, A1). Then we remove no-piece from the board hash (?,
no-piece, A3). Finally, we add (white, pawn, A3) to the board hash. Initial
board hash is calculated via calling method \textsf{.InitHash(CongoBoard
board)} on the table instance. Initial hash will contain only hash addition
operations. All no-pieces (ground and river) are considered black. For each
triple (color, piece, square) retrieve random number and let new hash be
\textsf{hash := XOR(hash, number)}. Removing pieces is done via repeated
\textsf{XOR} application. New board hash is adjusted directly in Negamax
recursive procedure.

## Bit scan

The technique employs 

- [BitScan](https://www.chessprogramming.org/BitScan)

## Concurrent evaluation
