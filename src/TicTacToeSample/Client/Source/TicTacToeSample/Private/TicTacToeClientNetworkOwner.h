// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "ULSClientNetworkOwner.h"
#include "TicTacToeClientNetworkOwner.generated.h"

/**
 * 
 */
UCLASS()
class UTicTacToeClientNetworkOwner : public UULSClientNetworkOwner
{
	GENERATED_BODY()
	
public:
	BEGIN_RPC_BP_EVENTS_FROM_SERVER
	END_RPC_BP_EVENTS_FROM_SERVER

	BEGIN_RPC_BP_EVENTS_TO_SERVER
	UFUNCTION(BlueprintCallable, Category = Rpc)
		void Server_Click(AActor* controller, int32 gridX, int32 gridY);
	END_RPC_BP_EVENTS_TO_SERVER

protected:
	virtual void ProcessHandleRpcPacket(const UULSWirePacket* packet, int packetReadPosition, AActor* existingActor, const FString& methodName,
		const FString& returnType, const int32 numberOfParameters) override;
};
