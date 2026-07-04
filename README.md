# kbWar2
An interactive monte-carlo simulation for the simple card game WAR! — now playable in any browser.

If you don't know what the card game WAR is, read this article:
https://en.wikipedia.org/wiki/War_(card_game)

## History

Back in 2014 I wrote [kbWar](https://github.com/kevinabrandon/kbWar) after playing the card game with my 7-year-old daughter.  I had a bunch of questions about the game — how long does it typically last, how often do you get a WAR, can it go on forever? — and I wanted to find out.  I wrote a Windows Forms app to run the simulation and got my answers.

The problem is that it only ran on Windows, and you had to download and compile it yourself.  So in 2026 I rebuilt it as a Blazor WebAssembly app.  The core game logic is the original C# code, unchanged.  The only new part is the UI layer that lets it run in a browser.

**Play it here:** https://kevinabrandon.github.io/kbWar2/

## Answers

Most of the original questions still have the same answers.  [See the full results from kbWar here.](https://github.com/kevinabrandon/kbWar/blob/master/Results.md)

Questions | Answers
------------- | -----------
How long does a game last? | The average game is 270 turns, the typical (median) game is about 204, and the most common game is in the 90–110 range (mode) — see the probability distribution below.
How often do you get a WAR? | About 6 every 100 turns.
How often do you get a double WAR? | About 3 every 1000 turns.
Can you have an unending game? | YES, but only if the players never mix up the cards they win.
How often are there unending games? | Somewhere between 10% and 40% of games — it depends on exactly how you pick up the pot.*

*This one got more interesting during the port.  If nobody ever shuffles the cards they win, the game is completely deterministic the moment the deal is done — whether it ends or goes on forever depends entirely on the order the cards land in the pot and get placed back under the winner's deck.  My original 2014 program dropped the two players' cards into the pot interleaved and handed them to the winner in reverse order, and about 10% of games never ended (that's the number in the old Results.md).  When I later rewrote the engine to support more than two players, it collected each player's thrown cards together in a group instead — and that one little change pushes unending games up to about 40%.  Both pickup orders are legal WAR; the physical game just never says what order the cards go back under your deck.  You can try both on the [Simulate](https://kevinabrandon.github.io/kbWar2/simulate) page: turn off "Shuffle recently won cards", then flip the "2015 pot pickup order" checkbox and watch the infinite-loop rate jump between the two.

![Probability Distribution](http://i.imgur.com/GJrZyB0.png)

## What's new in kbWar2

- Runs in any browser — no install, no Windows required
- Supports 2–52 players (same as the original)
- Interactive step-through mode: throw one turn at a time, or auto-play with a speed slider
- Monte Carlo simulation page with the same statistics the original reported, including war depth breakdown
- xUnit tests covering the core game logic

## Building

The project requires [.NET 8](https://dotnet.microsoft.com/download).

```
git clone https://github.com/kevinabrandon/kbWar2
cd kbWar2
dotnet test          # run the tests
dotnet run --project kbWar.Web   # run locally
```

The local dev server will print a URL (usually `http://localhost:5000`) you can open in a browser.

## Project structure

```
kbWar2/
├── kbWar.Core/     # The original game logic, ported to .NET 8 (kbPlayingCard, kbCardHand, kbCardGameWar)
├── kbWar.Web/      # Blazor WebAssembly UI
├── kbWar.Tests/    # xUnit tests
└── .github/
    └── workflows/
        └── deploy.yml   # builds and deploys to GitHub Pages on push to main
```
