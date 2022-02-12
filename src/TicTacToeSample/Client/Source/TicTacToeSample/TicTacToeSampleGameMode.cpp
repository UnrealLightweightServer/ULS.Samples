// Copyright Epic Games, Inc. All Rights Reserved.

#include "TicTacToeSampleGameMode.h"
#include "TicTacToeSamplePlayerController.h"
#include "TicTacToeSamplePawn.h"

ATicTacToeSampleGameMode::ATicTacToeSampleGameMode()
{
	// no pawn by default
	DefaultPawnClass = ATicTacToeSamplePawn::StaticClass();
	// use our own player controller class
	PlayerControllerClass = ATicTacToeSamplePlayerController::StaticClass();
}
