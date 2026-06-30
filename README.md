# kbWar2
An interactive monte-carlo simulation for the simple card game WAR! — now playable in any browser.

If you don't know what the card game WAR is, read this article:
https://en.wikipedia.org/wiki/War_(card_game)

## History

Back in 2014 I wrote [kbWar](https://github.com/kevinabrandon/kbWar) after playing the card game with my 7-year-old daughter.  I had a bunch of questions about the game — how long does it typically last, how often do you get a WAR, can it go on forever? — and I wanted to find out.  I wrote a Windows Forms app to run the simulation and got my answers.

The problem is that it only ran on Windows, and you had to download and compile it yourself.  So in 2026 I rebuilt it as a Blazor WebAssembly app.  The core game logic is the original C# code, unchanged.  The only new part is the UI layer that lets it run in a browser.

**Play it here:** https://kevinabrandon.github.io/kbWar2/

## Answers

The original questions still have the same answers.  [See the full results from kbWar here.](https://github.com/kevinabrandon/kbWar/blob/master/Results.md)

Questions | Answers
------------- | -----------
How long does a game last? | About 270 turns on average — see the probability distribution below.
How often do you get a WAR? | About 6 every 100 turns.
How often do you get a double WAR? | About 3 every 1000 turns.
Can you have an unending game? | YES, but only if the players never mix up the cards they win.
How often are there unending games? | About a *tenth of all games are unending.

*Only if you are very careful never to shuffle the cards won after each turn.  In practice this is unlikely.

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
