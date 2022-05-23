# Users' Guide

Contents

- [Users' Guide](#users-guide)
- [Definitions](#definitions)
- [Gameplay](#gameplay)
  - [Command-Line Interface](#command-line-interface)
  - [Graphical User Interface](#graphical-user-interface)

# Definitions

`User name` is any non-empty sequence of alphanumeric chars.

`Congo FEN` id a description of the immediate state of the `Congo` game in
textual form. `FEN` abbreviation is borrowed from the well-known
`Forsyth-Edwards Notation` for describing board position in chess.

# Gameplay

## Command-Line Interface

To start local game, use the following arguments:

```console
./Congo.exe --place=local --board=standard --white=hi/negamax --black=hi/negamax
```

To start new network game or continue already existing, use the following arguments:

```console
./Congo.exe --place=network --board=15087 --white=hi/negamax --host=127.0.0.1 --port=56789
```

The order of arguments does not make any difference. The meaning of each
argument is described below.

- `--place` decides locality of the game, `local` or `network`.
- `--board` restores the existing game or starts new. The command has different
    context in local and network modes. We may start a new `standard` game either
    locally or via network. Locally, we may construct a game from `FEN` word.
    To continue playing existing game via network, the player shall use `game id`
    provided by the server last time.
- `--white` or `--black` consists of two parts: kind of intelligence and an 
    algorithm for advising next move. Computer player uses advising algorithm
    in each move.
- `--host` is an IPv4 address of the game server.
- `--port` is an accepting port of the game server.

![open](./assets/images/console.png)

## Graphical User Interface

TBA
