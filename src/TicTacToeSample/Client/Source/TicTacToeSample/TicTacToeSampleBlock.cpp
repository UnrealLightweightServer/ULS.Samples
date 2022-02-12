// Copyright Epic Games, Inc. All Rights Reserved.

#include "TicTacToeSampleBlock.h"
#include "TicTacToeSampleBlockGrid.h"
#include "UObject/ConstructorHelpers.h"
#include "Components/StaticMeshComponent.h"
#include "Engine/StaticMesh.h"
#include "Materials/MaterialInstance.h"

ATicTacToeSampleBlock::ATicTacToeSampleBlock()
{
	// Structure to hold one-time initialization
	struct FConstructorStatics
	{
		ConstructorHelpers::FObjectFinderOptional<UStaticMesh> PlaneMesh;
		ConstructorHelpers::FObjectFinderOptional<UMaterial> BaseMaterial;
		ConstructorHelpers::FObjectFinderOptional<UMaterialInstance> CrossMaterial;
		ConstructorHelpers::FObjectFinderOptional<UMaterialInstance> CircleMaterial;
		FConstructorStatics()
			: PlaneMesh(TEXT("/Engine/BasicShapes/Cube.Cube"))
			, BaseMaterial(TEXT("/Game/Materials/BaseMaterial.BaseMaterial"))
			, CrossMaterial(TEXT("/Game/Materials/CrossMaterial.CrossMaterial"))
			, CircleMaterial(TEXT("/Game/Materials/CircleMaterial.CircleMaterial"))
		{
		}
	};
	static FConstructorStatics ConstructorStatics;

	// Create dummy root scene component
	DummyRoot = CreateDefaultSubobject<USceneComponent>(TEXT("Dummy0"));
	RootComponent = DummyRoot;

	// Create static mesh component
	BlockMesh = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("BlockMesh0"));
	BlockMesh->SetStaticMesh(ConstructorStatics.PlaneMesh.Get());
	BlockMesh->SetRelativeScale3D(FVector(2.f,2.f,0.25f));
	BlockMesh->SetRelativeLocation(FVector(0.f,0.f,25.f));
	BlockMesh->SetMaterial(0, ConstructorStatics.BaseMaterial.Get());
	BlockMesh->SetupAttachment(DummyRoot);
	BlockMesh->OnClicked.AddDynamic(this, &ATicTacToeSampleBlock::BlockClicked);
	BlockMesh->OnInputTouchBegin.AddDynamic(this, &ATicTacToeSampleBlock::OnFingerPressedBlock);

	// Save a pointer to the orange material
	BaseMaterial = ConstructorStatics.BaseMaterial.Get();
	CrossMaterial = ConstructorStatics.CrossMaterial.Get();
	CircleMaterial = ConstructorStatics.CircleMaterial.Get();
}

void ATicTacToeSampleBlock::BlockClicked(UPrimitiveComponent* ClickedComp, FKey ButtonClicked)
{
	HandleClicked();
}


void ATicTacToeSampleBlock::OnFingerPressedBlock(ETouchIndex::Type FingerIndex, UPrimitiveComponent* TouchedComponent)
{
	HandleClicked();
}

// 0 = not set, 1 = cross, 2 = circle
void ATicTacToeSampleBlock::SetBlockState(int32 state)
{
	switch (state)
	{
	case 0:
		BlockMesh->SetMaterial(0, BaseMaterial);
		break;

	case 1:
		BlockMesh->SetMaterial(0, CrossMaterial);
		break;

	case 2:
		BlockMesh->SetMaterial(0, CircleMaterial);
		break;
	}
}

void ATicTacToeSampleBlock::HandleClicked()
{
	if (IsValid(OwningGrid))
	{
		OwningGrid->BlockClicked(GridX, GridY);
	}
}
