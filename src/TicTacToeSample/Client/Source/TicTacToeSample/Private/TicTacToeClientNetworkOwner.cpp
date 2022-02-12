// Fill out your copyright notice in the Description page of Project Settings.


#include "TicTacToeClientNetworkOwner.h"
#include "ULSWirePacket.h"
#include "ULSTransport.h"

void UTicTacToeClientNetworkOwner::HandleRpcPacket(const UULSWirePacket* packet)
{
	int position = 0;
	int32 flags = packet->ReadInt32(position, position);
	int64 uniqueId = packet->ReadInt64(position, position);
	auto existingActor = FindActor(uniqueId);
	if (IsValid(existingActor) == false)
	{
		UE_LOG(LogTemp, Warning, TEXT("HandleReplicationMessage failed: Actor with id %ld not found"), uniqueId);
		return;
	}
	FString methodName = packet->ReadString(position, position);
	FString returnType = packet->ReadString(position, position);
	int32 numberOfParameters = packet->ReadInt32(position, position);

	BEGIN_RPC_BP_EVENTS_FROM_SERVER_CALL
	// SetBlockOwnership
	if (methodName == TEXT("SetBlockOwnership"))
	{
		auto cls = existingActor->GetClass();
		UFunction* function = cls->FindFunctionByName(FName(TEXT("SetBlockOwnership")));
		if (IsValid(function) == false)
		{
			// TODO: Log properly
			UE_LOG(LogTemp, Error, TEXT("Failed to call function SetBlockOwnership on actor of type %s with uniqueId: %ld"), *cls->GetName(), FindUniqueId(existingActor));
			return;
		}
		struct
		{
			int32 param_SetBlockOwnership_0 = 0;
			int32 param_SetBlockOwnership_1 = 0;
			const AActor* param_SetBlockOwnership_2 = nullptr;
		} FuncParams;
		FuncParams.param_SetBlockOwnership_0 = DeserializeInt32Parameter(packet, position, position);
		FuncParams.param_SetBlockOwnership_1 = DeserializeInt32Parameter(packet, position, position);
		FuncParams.param_SetBlockOwnership_2 = DeserializeRefParameter(packet, position, position);
		existingActor->ProcessEvent(function, &FuncParams);
		return;
	}
	
	// SetBlockIsMarked
	if (methodName == TEXT("SetBlockIsMarked"))
	{
		auto cls = existingActor->GetClass();
		UFunction* function = cls->FindFunctionByName(FName(TEXT("SetBlockIsMarked")));
		if (IsValid(function) == false)
		{
			// TODO: Log properly
			UE_LOG(LogTemp, Error, TEXT("Failed to call function SetBlockIsMarked on actor of type %s with uniqueId: %ld"), *cls->GetName(), FindUniqueId(existingActor));
			return;
		}
		struct
		{
			int32 param_SetBlockIsMarked_0 = 0;
			int32 param_SetBlockIsMarked_1 = 0;
			bool param_SetBlockIsMarked_2 = false;
		} FuncParams;
		FuncParams.param_SetBlockIsMarked_0 = DeserializeInt32Parameter(packet, position, position);
		FuncParams.param_SetBlockIsMarked_1 = DeserializeInt32Parameter(packet, position, position);
		FuncParams.param_SetBlockIsMarked_2 = DeserializeBoolParameter(packet, position, position);
		existingActor->ProcessEvent(function, &FuncParams);
		return;
	}
	
	// StartMatch
	if (methodName == TEXT("StartMatch"))
	{
		auto cls = existingActor->GetClass();
		UFunction* function = cls->FindFunctionByName(FName(TEXT("StartMatch")));
		if (IsValid(function) == false)
		{
			// TODO: Log properly
			UE_LOG(LogTemp, Error, TEXT("Failed to call function StartMatch on actor of type %s with uniqueId: %ld"), *cls->GetName(), FindUniqueId(existingActor));
			return;
		}
		struct
		{
		} FuncParams;
		existingActor->ProcessEvent(function, &FuncParams);
		return;
	}
	
	// EndMatch
	if (methodName == TEXT("EndMatch"))
	{
		auto cls = existingActor->GetClass();
		UFunction* function = cls->FindFunctionByName(FName(TEXT("EndMatch")));
		if (IsValid(function) == false)
		{
			// TODO: Log properly
			UE_LOG(LogTemp, Error, TEXT("Failed to call function EndMatch on actor of type %s with uniqueId: %ld"), *cls->GetName(), FindUniqueId(existingActor));
			return;
		}
		struct
		{
			const AActor* param_EndMatch_0 = nullptr;
		} FuncParams;
		FuncParams.param_EndMatch_0 = DeserializeRefParameter(packet, position, position);
		existingActor->ProcessEvent(function, &FuncParams);
		return;
	}
	
	// SetActivePlayer
	if (methodName == TEXT("SetActivePlayer"))
	{
		auto cls = existingActor->GetClass();
		UFunction* function = cls->FindFunctionByName(FName(TEXT("SetActivePlayer")));
		if (IsValid(function) == false)
		{
			// TODO: Log properly
			UE_LOG(LogTemp, Error, TEXT("Failed to call function SetActivePlayer on actor of type %s with uniqueId: %ld"), *cls->GetName(), FindUniqueId(existingActor));
			return;
		}
		struct
		{
			const AActor* param_SetActivePlayer_0 = nullptr;
		} FuncParams;
		FuncParams.param_SetActivePlayer_0 = DeserializeRefParameter(packet, position, position);
		existingActor->ProcessEvent(function, &FuncParams);
		return;
	}
	
	END_RPC_BP_EVENTS_FROM_SERVER_CALL
}

BEGIN_RPC_BP_EVENTS_TO_SERVER_CALL
// OnHandleClick
void UTicTacToeClientNetworkOwner::Server_Click(AActor* controller, int32 gridX, int32 gridY)
{
   UULSWirePacket* packet = NewObject<UULSWirePacket>();
   packet->PacketType = (int32)EWirePacketType::RpcCall;
   
   FString methodName = TEXT("OnHandleClick");
   FString returnType = TEXT("void");
   
   int requiredPayloadSize = 4 + 8 + 4 + methodName.Len() + 4 + returnType.Len() + 4;
   FString fieldName_gridX = TEXT("gridX");
   requiredPayloadSize += GetSerializeInt32ParameterSize(fieldName_gridX);
   FString fieldName_gridY = TEXT("gridY");
   requiredPayloadSize += GetSerializeInt32ParameterSize(fieldName_gridY);
   TArray<uint8> bytes;
   bytes.AddUninitialized(requiredPayloadSize);
   packet->Payload = bytes;
   int position = 0;
   packet->PutInt32(0, position, position); // flags
   packet->PutInt64(FindUniqueId(controller), position, position);
   packet->PutString(methodName, position, position);
   packet->PutString(returnType, position, position);
   packet->PutInt32(2, position, position); // number of parameters
   SerializeInt32Parameter(packet, fieldName_gridX, gridX, position, position);
   SerializeInt32Parameter(packet, fieldName_gridY, gridY, position, position);
   this->Transport->SendWirePacket(packet);
}

	END_RPC_BP_EVENTS_TO_SERVER_CALL

