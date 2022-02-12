// Copyright Epic Games, Inc. All Rights Reserved.

#include "TicTacToeSampleBlockGrid.h"
#include "TicTacToeSampleBlock.h"
#include "Components/TextRenderComponent.h"
#include "Engine/World.h"

#define LOCTEXT_NAMESPACE "PuzzleBlockGrid"

ATicTacToeSampleBlockGrid::ATicTacToeSampleBlockGrid()
{
	// Create dummy root scene component
	DummyRoot = CreateDefaultSubobject<USceneComponent>(TEXT("Dummy0"));
	RootComponent = DummyRoot;

	// Set defaults
	BlockSpacing = 300.f;
}

void ATicTacToeSampleBlockGrid::BeginPlay()
{
	Super::BeginPlay();

	// Loop to spawn each block
	int size = 3;
	for (size_t y = 0; y < size; y++)
	{
		for (size_t x = 0; x < size; x++)
		{
			const float XOffset = x * BlockSpacing; // Divide by dimension
			const float YOffset = y * BlockSpacing; // Modulo gives remainder

			// Make position vector, offset from Grid location
			const FVector BlockLocation = FVector(XOffset, YOffset, 0.f) + GetActorLocation();

			// Spawn a block
			ATicTacToeSampleBlock* NewBlock = GetWorld()->SpawnActor<ATicTacToeSampleBlock>(BlockLocation, FRotator(0, 0, 0));

			// Tell the block about its owner
			if (NewBlock != nullptr)
			{
				NewBlock->GridX = x;
				NewBlock->GridY = y;
				NewBlock->OwningGrid = this;
			}
		}
	}
}

#undef LOCTEXT_NAMESPACE
