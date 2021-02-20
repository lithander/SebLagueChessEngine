# Sebastian Lague's Chess AI with added UCI support

For his video series [Coding Adventures](https://www.youtube.com/watch?v=U4ogK0MIzqk&list=PLFt_AvWsXl0ehjAfLFsp1PGaatzAwo0uK) Sebastian Lague created a Chess-AI in Unity. 
I gutted [my own C# chess engine](https://github.com/lithander/MinimalChessEngine), then copied the chess playing code from Sebastians Unity project over, changed what needed changing to make it compile again.
The result is a chess engine based on Sebastian Lagues code that supports enough of the UCI interface to be used in your favorite chess GUIs.

* Sebastion Lague original source code can be found [here](https://github.com/SebLague/Chess-AI).
* His Unity based executable that features a custom GUI [here](https://sebastian.itch.io/chess-ai).
* His video that describes how he developed it is [here](https://www.youtube.com/watch?v=U4ogK0MIzqk).

## How to play?

To play with *this* version, which implements the UCI protocol, you need a compatible chess GUI. For example...
* [Arena Chess GUI](http://www.playwitharena.de/) (free)
* [BanksiaGUI](https://banksiagui.com/) (free, has Lc0 specific features)
* [Cutechess](https://cutechess.com/) (free)
* [Nibbler](https://github.com/fohristiwhirl/nibbler/releases) (free, has Lc0 specific features)
* [Chessbase](https://chessbase.com/) (paid).

Then you either compile an executable from source or download from the [most recent release](https://github.com/lithander/SebLagueChessEngine/releases/tag/0.1), register the executable (e.g. SebLagueChessEngine.exe) with the GUI and you're ready to go. 
