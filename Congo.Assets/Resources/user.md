# User Guide

The purpose of this document is to describe rules, gameplay and user interface
of the board game `Congo`.

# Definitions

`Congo` is an abstract game (chess variant) popular in the Netherlands.

`Board` is a flat surface with specific pattern of squares, on which pieces
are placed. Congo board contains $7 \times 7$ squares.

`Rank` is a horizontal row of squares on the board. `Congo` board has $7$
ranks, marked from $1$ to $7$.

`File` is a vertical row of squares on the board. `Congo` board has $7$
files, marked from $A$ to $G$.

`Piece` is any figure on the board used by the players to play the game.

`Capture` is an action when active player's move removes its opponent's piece
from the board.

`River` is the middle $4$<sup>th</sup> row of the board. Such squares have
special behavior towards animal pieces, which is described in the next chapter.

`Castle` is a $3 \times 3$ square at each side of the board, namely cartesian
product $\lbrace C, D, E \rbrace \times \lbrace 1, 2, 3, 5, 6, 7 \rbrace$.

`User name` is any non-empty sequence of **alphanumeric** chars.

`Congo FEN` is a description of the immediate state of the `Congo` game in
textual form. `FEN` abbreviation is borrowed from the well-known
`Forsyth-Edwards Notation` for describing board position in chess. More on
the subjects at [Congo FEN](#congo-fen).

# Rules

There are two competing players in the game, black and white. Players alternate
turns, passing is not possible. Further, we describe the behavior of each piece
and the aim of a game.

`[L]ion` is the King of the Jungle. It may not leave $3 \times 3$ castle around
it. Inside castle, it moves and captures as the King in Chess. If there is
a vertical or diagonal line with no pieces between the two lions, the lion may
jump to the other lion and capture it.

`[Z]ebra` moves as the Knight in classic chess.

`[E]lephant` moves one or two squares in the horizontal or vertical direction.
It may jump over the nearest square (also river) and capture the piece on the
next square.

`[G]iraffe` perform non-capturing moves in any direction (as the chess
King). It can move or capture two steps away in any straight direction.

`[C]rocodile` moves as the King in Chess when on land. Outside the river it
can move straight towards the river (including the river square) as a rook.
Inside the river it move to another river square as a rook. A crocodile
**cannot** drown.

`[P]awn` moves and captures both straight and diagonally forward. Being on the
other side of the river, a pawn may also move one or two squares straight back,
without the right to capture or jump. If a pawn moves to the last row, it is
promoted to a superpawn.

`[S]uperpawn` has the additional powers of moving and capturing one square
straight sideways and going one or two square straight backwards or diagonally
backward. When going backwards, it may neither capture nor jump. A superpawns
right to go backwards does not depend on its position: they may go backwards
at both sides and on the river.

`[M]onkey` moves as the King in Chess while not capturing. It captures a piece
by jumping over it in any direction to the square immediately beyond, which
must be vacant. A monkey may capture multiple pieces in the same turn, but is
not obliged to do so. Monkey jump can be interrupted at any time. Once a
monkey jumps over a piece, the piece immediately disappears. If a monkey
starts multiple capture being at a river square and ends at any river square,
it immediately drowns. If a monkey starts its jump on the ground and ends in
the river or opposite, it is not drown. Captures before drowning are legal.
The monkey captured opponent's Lion terminates the move and the game.

The aim of the game is to win by capturing opponent's `Lion` as there is only
one King of the Jungle. The game immediately ends once opponent's lion is
captured. There is no chess-like check in `Congo`, so a lion may move to an
already attacked square. Consequently, `Congo` has no draw by stalemate.

A non-crocodile piece that ends its move standing on the river square must reach
a ground square in the next turn, otherwise the piece disappears as being drown.
Crocodiles cannot drown.

# Congo FEN

Any board can be encoded into a string with $9$ sections `rank/rank/rank/rank/rank/rank/rank/color/jump`.

- 7x ranks. Squares are described from left to right. Numbers indicate empty
  squares, letters indicate pieces.
- 1x active player color. `w` stands for `white` and `b` stands for `black`.
- 1x active monkey jump started at a position. If monkey jump doesn't happen, 
  then the last field is $-1$. Otherwise, there should be a square, from which
  monkey has started its jump.

`Congo FEN` examples follow.

Empty board.
```txt
7/7/7/7/7/7/7/w/-1
```

Standard board.
```txt
gmelecz/ppppppp/7/7/7/PPPPPPP/GMELECZ/w/-1
```

$3$-long white monkey jump and white wins.
```txt
7/4l2/7/4c2/7/2p4/1M1L3/w/-1
```

Board without pawns.
```txt
gmelecz/2ppp2/7/7/7/2PPP2/GMELECZ/w/-1
```

Board with plenty of moves.
```txt
1melec1/ppppp1p/2g2zp/5C1/2G2ZP/PPPPP1P/1MELE2/b/-1
```

# Commond-line interface

## Setting up

Options could have different sets of allowed values depending on the locality
of the game. The order of arguments doesn't make any difference.

To start **local** game in the terminal, use the following arguments:

```txt
$ congo-cli --place=local --board=standard --white=hi/negamax --black=ai/negamax
```

- `--place` decides locality of the game, `local` or `network`.
- `--board` starts `standard` game or restores game from valid `Congo FEN`.
- `--white` **and** `--black` value consists of two parts: kind of intelligence
  (`ai` or `hi`) and an algorithm for advising next move (currently `random`
  and `negamax` are supported). Computer player uses advising algorithm in each
  move.

To start **network** game in the terminal, use the following arguments:

```txt
$ congo-cli --place=network --host=localhost --port=7153 --board=standard --white=hi/negamax
```

- `--place` has similar to the previous paragraph meaning.
- `--host` is an IPv4 address of the game server.
- `--port` is an accepting port of the game server.
- `--board` creates `standard` game or new game from valid `Congo FEN` or 
  connects already existing game by `id`. The unique game `id` is generated by
  the server for all `standard` and `Congo FEN` games.
- `--white` **or** `--black` defines local player.

Server instance should be active on (`--host`, `--port`) prior to creating
network game.

## Gameplay

The game is started with a text greeting the user showing current position
on the board and players with summed up piece occupancies.

```txt
   ____
  / ___|___  _ __   __ _  ___
 | |   / _ \| '_ \ / _` |/ _ \
 | |__| (_) | | | | (_| | (_) |
  \____\___/|_| |_|\__, |\___/
                   |___/

 7   g m e l e c z
 6   p p p p p p p
 5   - - - - - - -
 4   + + + + + + +
 3   - - - - - - -
 2   P P P P P P P
 1   G M E L E C Z

 /   a b c d e f g

 * white 2E 1Z 1G 1C 7P 0S 1L 1M
   black 2e 1z 1g 1c 7p 0s 1l 1m

 > _
```

If the user attempts to enter unsupported command, the program properly informs
the user.

```txt
 > unknown
 Command unknown is not supported. Consult "help help".
```

The user can obtain a list of allowed commands via `allow`.

```txt
 > allow
 Allowed commands are allow, advise, help, show, exit, move.
```

All supported/allowed commands have `man` page, which is retrieved by `help`
command.

```txt
 > help move
 NAME
    move
 DESCRIPTION
    Moves a certain piece from a square to a square.
 USAGE
    move [a-g][1-7] [a-g][1-7]
```

During the game, a player could print out current board, players or available
moves via `show` command.

```txt
 > show board

 7   g m e l e c z
 6   p p p p p p p
 5   - - - - - - -
 4   + + + + + + +
 3   - - - - - - -
 2   P P P P P P P
 1   G M E L E C Z

 /   a b c d e f g

 > show players

 * white 2E 1Z 1G 1C 7P 0S 1L 1M
   black 2e 1z 1g 1c 7p 0s 1l 1m

 > show moves

 (a2,a3) (a2,b3) (b2,a3) (b2,b3) (b2,c3)
 (c2,b3) (c2,c3) (c2,d3) (d2,c3) (d2,d3)
 (d2,e3) (e2,d3) (e2,e3) (e2,f3) (f2,e3)
 (f2,f3) (f2,g3) (g2,f3) (g2,g3) (a1,a3)
 (a1,c3) (c1,c3) (e1,e3) (g1,f3)
```

Current player could move piece using `move` command. Transition is useful
for declaring moves done by `ai` players as moves are automatically generated.
New board position is shown in the terminal after the move is done. Wrong
moves are reported.

```txt
 > move a2 a3

 transition (a2,a3)

 7   g m e l e c z
 6   p p p p p p p
 5   - - - - - - -
 4   + + + + + + +
 3   P - - - - - -
 2   - P P P P P P
 1   G M E L E C Z

 /   a b c d e f g

 > move a1 b3
 Entered move is wrong. Consult "show moves".
```

Next move can be advised by the algorithm provided in the arguments. Currently
`random` (random choice) and `negamax` (recursion with evaluation function) are
supported.

```txt
 > advise
 Advised move is (b1,a2).
```

The game is exited upon entering `exit` command.

```txt
 > exit
The program is terminated...
```

# Play with GUI

## Setting up

TBA

## Gameplay

TBA

# Play via network

TBA

## Start server instance

TBA
