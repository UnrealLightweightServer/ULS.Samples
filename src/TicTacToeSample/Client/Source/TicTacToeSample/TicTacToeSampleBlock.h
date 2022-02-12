// Copyright Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "TicTacToeSampleBlock.generated.h"

/** A block that can be clicked */
UCLASS()
class ATicTacToeSampleBlock : public AActor
{
	GENERATED_BODY()

	/** Dummy root component */
	UPROPERTY(Category = Block, VisibleDefaultsOnly, BlueprintReadOnly, meta = (AllowPrivateAccess = "true"))
	class USceneComponent* DummyRoot;

	/** StaticMesh component for the clickable block */
	UPROPERTY(Category = Block, VisibleDefaultsOnly, BlueprintReadOnly, meta = (AllowPrivateAccess = "true"))
	class UStaticMeshComponent* BlockMesh;

public:
	ATicTacToeSampleBlock();

	/** Are we currently active? */
	bool bIsActive;

	UPROPERTY(BlueprintReadOnly)
		int GridX;
	UPROPERTY(BlueprintReadOnly)
		int GridY;

	/** Pointer to white material used on unset blocks */
	UPROPERTY()
	class UMaterial* BaseMaterial;

	/** Pointer to circle (O) material used on blocks */
	UPROPERTY()
	class UMaterialInstance* CircleMaterial;

	/** Pointer to cross (X) material used on blocks */
	UPROPERTY()
	class UMaterialInstance* CrossMaterial;

	/** Grid that owns us */
	UPROPERTY()
	class ATicTacToeSampleBlockGrid* OwningGrid;

	/** Handle the block being clicked */
	UFUNCTION()
	void BlockClicked(UPrimitiveComponent* ClickedComp, FKey ButtonClicked);

	/** Handle the block being touched  */
	UFUNCTION()
	void OnFingerPressedBlock(ETouchIndex::Type FingerIndex, UPrimitiveComponent* TouchedComponent);

	void HandleClicked();

	UFUNCTION(BlueprintCallable)
		void SetBlockState(int32 state);

public:
	/** Returns DummyRoot subobject **/
	FORCEINLINE class USceneComponent* GetDummyRoot() const { return DummyRoot; }
	/** Returns BlockMesh subobject **/
	FORCEINLINE class UStaticMeshComponent* GetBlockMesh() const { return BlockMesh; }
};



