# Congo

This project is a .NET implementation of the board game `Congo`.

## System requirements

Before installation, ensure that the following technologies are available
on the target system:
- `Windows 10`,
- `.NET Framework 4.8`,
- `Visual Studio Community 2022`.

## Installation

- Open `Visual Studio Community 2019`.
- Left click on `Clone a repository` in the upper right corner.
- Insert `https://github.com/zhukovdm/Congo` into `Repository location` field.
- Right click on `Clone` in the bottom right corner.
- `Open` the solution.

![open](./assets/images/install.png)

- Restore NuGet packages.

![open](./assets/images/nugets.png)

- Press `Ctrl+Shift+B` to build entire solution.
- Press `Ctrl+F5` to start the game.

## Gameplay

To play local game, use the following parameters.

```console
./Congo.exe --play=local --game=standard --player1=p1/hi/negamax --player2=p2/hi/negamax
```

To play a game via network, use the following parameters.

```console
./Congo.exe --play=network --game=15087 --player=p1/hi/negamax --host=127.0.0.1 --port=56789
```

![open](./assets/images/console.png)

## Credits

- [BitScan](https://www.chessprogramming.org/BitScan)
