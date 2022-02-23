> Note:  
> ULS is still very much in early Alpha-stage. Expect bugs, missing features and various bumps and hurdles.

# ULS.Samples
UnrealLightweightServer samples.

Clone:  
`git clone --recursive https://github.com/UnrealLightweightServer/ULS.Samples.git`

## SimpleInProces

This is a short and simple sample showing the basic functionality of ULS in a C#-only project without the added complexity of an Unreal project.

It's non-interactive. Simply open `src\ULS.Samples.sln`, select `SimpleInProcess` as the launch project and run it.

You can also open a command prompt at `src\SimpleInProcess` and invoke `dotnet run` there.

## TicTacToe

This sample implements a more realistic scenario where two players play a game of TicTacToe against each other.

> The project currectly uses UE5 Preview 1. This will be replaced with the final UE5 version when it's released.

### Running

First launch the server application `TicTacToeServer` in `src\ULS.Samples.sln` by selecting it as the launch project in Visual Studio. Alternatively, you can also open a command prompt at `src\TicTacToeSample\Server\TicTacToeServer` and invoke `dotnet run` there.

Next, open the client at `src\TicTacToeSample\Client\TicTacToeSample.uproject`. In the Unreal Editor set the number of clients to 2 and start PIE.
The client automatically connects on localhost, so if you run the server on a different device, you must adapt the IP address in Blueprint accordingly.

