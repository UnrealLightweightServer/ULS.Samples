// Fill out your copyright notice in the Description page of Project Settings.


#include "TicTacToeClientNetworkOwner.h"
#include "ULSWirePacket.h"
#include "ULSTransport.h"

void UTicTacToeClientNetworkOwner::ProcessHandleRpcPacket(const UULSWirePacket* packet, int packetReadPosition, UObject* existingObject, const FString& methodName,
	const FString& returnType, const int32 numberOfParameters)
{
	int position = packetReadPosition;

	BEGIN_RPC_BP_EVENTS_FROM_SERVER_CALL
	// SetBlockOwnership
	if (methodName == TEXT("SetBlockOwnership"))
	{
		auto cls = existingObject->GetClass();
		UFunction* function = cls->FindFunctionByName(FName(TEXT("SetBlockOwnership")));
		if (IsValid(function) == false)
		{
			// TODO: Log properly
			UE_LOG(LogTemp, Error, TEXT("Failed to call function SetBlockOwnership on object of type %s with uniqueId: %ld"), *cls->GetName(), FindUniqueId(existingObject));
			return;
		}
		struct
		{
			int32 param_SetBlockOwnership_0 = 0;
			int32 param_SetBlockOwnership_1 = 0;
			const UObject* param_SetBlockOwnership_2 = nullptr;
		} FuncParams;
		FuncParams.param_SetBlockOwnership_0 = DeserializeInt32Parameter(packet, position, position);
		FuncParams.param_SetBlockOwnership_1 = DeserializeInt32Parameter(packet, position, position);
		FuncParams.param_SetBlockOwnership_2 = DeserializeRefParameter(packet, position, position);
		existingObject->ProcessEvent(function, &FuncParams);
		return;
	}
	
	// SetBlockIsMarked
	if (methodName == TEXT("SetBlockIsMarked"))
	{
		auto cls = existingObject->GetClass();
		UFunction* function = cls->FindFunctionByName(FName(TEXT("SetBlockIsMarked")));
		if (IsValid(function) == false)
		{
			// TODO: Log properly
			UE_LOG(LogTemp, Error, TEXT("Failed to call function SetBlockIsMarked on object of type %s with uniqueId: %ld"), *cls->GetName(), FindUniqueId(existingObject));
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
		existingObject->ProcessEvent(function, &FuncParams);
		return;
	}
	
	// StartMatch
	if (methodName == TEXT("StartMatch"))
	{
		auto cls = existingObject->GetClass();
		UFunction* function = cls->FindFunctionByName(FName(TEXT("StartMatch")));
		if (IsValid(function) == false)
		{
			// TODO: Log properly
			UE_LOG(LogTemp, Error, TEXT("Failed to call function StartMatch on object of type %s with uniqueId: %ld"), *cls->GetName(), FindUniqueId(existingObject));
			return;
		}
		struct
		{
		} FuncParams;
		existingObject->ProcessEvent(function, &FuncParams);
		return;
	}
	
	// EndMatch
	if (methodName == TEXT("EndMatch"))
	{
		auto cls = existingObject->GetClass();
		UFunction* function = cls->FindFunctionByName(FName(TEXT("EndMatch")));
		if (IsValid(function) == false)
		{
			// TODO: Log properly
			UE_LOG(LogTemp, Error, TEXT("Failed to call function EndMatch on object of type %s with uniqueId: %ld"), *cls->GetName(), FindUniqueId(existingObject));
			return;
		}
		struct
		{
			const UObject* param_EndMatch_0 = nullptr;
		} FuncParams;
		FuncParams.param_EndMatch_0 = DeserializeRefParameter(packet, position, position);
		existingObject->ProcessEvent(function, &FuncParams);
		return;
	}
	
	// SetActivePlayer
	if (methodName == TEXT("SetActivePlayer"))
	{
		auto cls = existingObject->GetClass();
		UFunction* function = cls->FindFunctionByName(FName(TEXT("SetActivePlayer")));
		if (IsValid(function) == false)
		{
			// TODO: Log properly
			UE_LOG(LogTemp, Error, TEXT("Failed to call function SetActivePlayer on object of type %s with uniqueId: %ld"), *cls->GetName(), FindUniqueId(existingObject));
			return;
		}
		struct
		{
			const UObject* param_SetActivePlayer_0 = nullptr;
		} FuncParams;
		FuncParams.param_SetActivePlayer_0 = DeserializeRefParameter(packet, position, position);
		existingObject->ProcessEvent(function, &FuncParams);
		return;
	}
	
	END_RPC_BP_EVENTS_FROM_SERVER_CALL
}

BEGIN_RPC_BP_EVENTS_TO_SERVER_CALL
// OnHandleClick
void UTicTacToeClientNetworkOwner::Server_Click(UObject* controller, int32 gridX, int32 gridY)
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

