/*
 * MiVRy - VR gesture recognition library plug-in for Unreal.
 * Version 2.11
 * Copyright (c) 2024 MARUI-PlugIn (inc.)
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
 * OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#include "BTDecorator_MiVRy.h"
#include "Misc/Paths.h"
#include "Misc/FileHelper.h"
#include "GestureRecognition.h"
#include "GestureCombinations.h"
#include "Runtime/Launch/Resources/Version.h" // for ENGINE_MAJOR_VERSION / ENGINE_MINOR_VERSION

UBTDecorator_MiVRy::UBTDecorator_MiVRy(const FObjectInitializer& ObjectInitializer) : Super(ObjectInitializer)
{
	NodeName = "MiVRy Gesture Recognition";
	INIT_DECORATOR_NODE_NOTIFY_FLAGS();

	this->bNotifyTick = true;
	this->bTickIntervals = 1;

	this->bAllowAbortLowerPri = 0;
	this->bAllowAbortChildNodes = 1;
	this->bAllowAbortNone = 1;
	this->FlowAbortMode = EBTFlowAbortMode::Self;

	this->bShowInverseConditionDesc = 0;
	this->bNotifyActivation = true;
	this->bNotifyProcessed = true;
	this->Delegate.BindUFunction(this, "OnGestureIdentified");
	if (this->MiVRyActor.IsValid()) {
		this->MiVRyActor->OnGestureIdentifiedDelegate.Add(this->Delegate);
		this->BoundMiVRyActor = this->MiVRyActor;
	}
}

UBTDecorator_MiVRy::~UBTDecorator_MiVRy()
{
	if (this->BoundMiVRyActor.IsValid()) {
		if (this->BoundMiVRyActor.IsValid()) {
			this->BoundMiVRyActor->OnGestureIdentifiedDelegate.RemoveAll(this);
		}
		this->BoundMiVRyActor = nullptr;
	}
	this->Delegate.Clear();
}

void UBTDecorator_MiVRy::PostLoad()
{
	Super::PostLoad();
	if (!this->BoundMiVRyActor.IsValid() && this->MiVRyActor.IsValid()) {
		this->MiVRyActor->OnGestureIdentifiedDelegate.Add(this->Delegate);
		this->BoundMiVRyActor = this->MiVRyActor;
	}
}

void UBTDecorator_MiVRy::BeginDestroy()
{
	if (this->BoundMiVRyActor.IsValid()) {
		if (this->BoundMiVRyActor.IsValid()) {
			this->BoundMiVRyActor->OnGestureIdentifiedDelegate.RemoveAll(this);
		}
		this->BoundMiVRyActor = nullptr;
	}
	this->Delegate.Clear();
	Super::BeginDestroy();
}

void UBTDecorator_MiVRy::OnNodeActivation(FBehaviorTreeSearchData& SearchData)
{
	if (!this->BoundMiVRyActor.IsValid() && this->MiVRyActor.IsValid()) {
		this->MiVRyActor->OnGestureIdentifiedDelegate.Add(this->Delegate);
		this->BoundMiVRyActor = this->MiVRyActor;
	}
}

#if WITH_EDITOR
void UBTDecorator_MiVRy::PostEditChangeProperty(struct FPropertyChangedEvent& PropertyChangedEvent)
{
	if (PropertyChangedEvent.Property == nullptr) {
		return;
	}
	FName PropertyName = PropertyChangedEvent.Property->GetFName();
	if (PropertyName != GET_MEMBER_NAME_CHECKED(UBTDecorator_MiVRy, MiVRyActor)) {
		Super::PostEditChangeProperty(PropertyChangedEvent);
		return;
	} // else:
	if (this->BoundMiVRyActor.IsValid()) {
		if (this->BoundMiVRyActor.IsValid()) {
			this->BoundMiVRyActor->OnGestureIdentifiedDelegate.RemoveAll(this);
		}
		this->BoundMiVRyActor = nullptr;
	}
	Super::PostEditChangeProperty(PropertyChangedEvent);
	if (this->MiVRyActor.IsValid()) {
		this->MiVRyActor->OnGestureIdentifiedDelegate.Add(this->Delegate);
		this->BoundMiVRyActor = this->MiVRyActor;
	}
}
#endif

uint16 UBTDecorator_MiVRy::GetInstanceMemorySize() const
{
	return sizeof(UBTDecorator_MiVRyMemory);
}

void UBTDecorator_MiVRy::InitializeMemory(UBehaviorTreeComponent& OwnerComp, uint8* NodeMemory, EBTMemoryInit::Type InitType) const
{
#if ENGINE_MAJOR_VERSION >= 5 && ENGINE_MINOR_VERSION >= 4
	InitializeNodeMemory<UBTDecorator_MiVRyMemory>(NodeMemory, InitType);
#else
	Super::InitializeMemory(OwnerComp, NodeMemory, InitType);
#endif
	UBTDecorator_MiVRyMemory* Memory = CastInstanceNodeMemory<UBTDecorator_MiVRyMemory>(NodeMemory);
	if (InitType == EBTMemoryInit::Initialize)
	{
		Memory->LatestGestureCounter = 0;
	}
}

void UBTDecorator_MiVRy::CleanupMemory(UBehaviorTreeComponent& OwnerComp, uint8* NodeMemory, EBTMemoryClear::Type CleanupType) const
{
#if ENGINE_MAJOR_VERSION >= 5 && ENGINE_MINOR_VERSION >= 4
	CleanupNodeMemory<UBTDecorator_MiVRyMemory>(NodeMemory, CleanupType);
#else
	Super::CleanupMemory(OwnerComp, NodeMemory, CleanupType);
#endif
}

bool UBTDecorator_MiVRy::CalculateRawConditionValue(UBehaviorTreeComponent& OwnerComp, uint8* NodeMemory) const
{
	UBTDecorator_MiVRyMemory* Memory = CastInstanceNodeMemory<UBTDecorator_MiVRyMemory>(NodeMemory);
	if (this->LatestGestureCounter == Memory->LatestGestureCounter) { // already processed latest gesture
		if (this->LatestGestureId >= 0 && this->EvalEveryGestureOnlyOnce) {
			return false;
		} // else: no-gesture or latest gesture is allowed mutliple times
	} else {
		Memory->LatestGestureCounter = this->LatestGestureCounter;
	}

	switch (this->GestureIdListUse) {
		case UBTDecorator_MiVRy_GestureIdListUse::Whitelist:
			return this->GestureIDs.Contains((int32)this->LatestGestureId);
		case UBTDecorator_MiVRy_GestureIdListUse::Blacklist:
			return !this->GestureIDs.Contains((int32)this->LatestGestureId);
		case UBTDecorator_MiVRy_GestureIdListUse::Ignore:
		default:
			return true;
	}
}

void UBTDecorator_MiVRy::TickNode(UBehaviorTreeComponent& OwnerComp, uint8* NodeMemory, float DeltaSeconds)
{
	UBTDecorator_MiVRyMemory* Memory = CastInstanceNodeMemory<UBTDecorator_MiVRyMemory>(NodeMemory);
	if (this->LatestGestureCounter == Memory->LatestGestureCounter) { // already processed latest gesture
		return;
	} // else:
	if (FlowAbortMode == EBTFlowAbortMode::None) {
		return;
	} // else: FlowAbortMode == EBTFlowAbortMode::Self
	const bool IsOnActiveBranch = OwnerComp.IsExecutingBranch(this->GetMyNode(), this->GetChildIndex());
	if (!IsOnActiveBranch) {
		return;
	}
	const bool Pass = this->CalculateRawConditionValue(OwnerComp, NodeMemory);
	if (Pass) {
		OwnerComp.RequestBranchActivation(*this, true);
	} else {
		OwnerComp.RequestBranchDeactivation(*this);
	}
}

void UBTDecorator_MiVRy::OnGestureIdentified(
	AMiVRyActor* Source,
	GestureRecognition_Identification Result,
	int GestureID,
	FString GestureName,
	float Similarity,
	const TArray<FMiVRyGesturePart>& GestureParts
)
{
	if (GestureID >= 0 && Similarity >= this->SimilarityThreshold) {
		this->LatestGestureId = GestureID;
	} else {
		this->LatestGestureId = -1;
	}
	this->LatestGestureCounter += 1;
}

FString UBTDecorator_MiVRy::GetStaticDescription() const
{
	if (this->DisplayGesturesInNode == false) {
		return Super::GetStaticDescription();
	}
	if (this->MiVRyActor == nullptr) {
		return TEXT("MiVRyActor not set");
	}
	UBTDecorator_MiVRy* me = (UBTDecorator_MiVRy*)this;
	FString path = this->MiVRyActor->GestureDatabaseFile.FilePath;
	if (path != me->GestureDatabaseFile) {
		me->GestureDatabaseFile = path;
		if (path.Len() == 0) {
			return TEXT("MiVRyActor's GestureDatabaseFile not set");
		}
		TArray64<uint8> file_contents;
		if (FFileHelper::LoadFileToArray(file_contents, *path) == false) {
			// if (FPaths::IsRelative(path)) {
			FString relpath = FPaths::Combine(FPaths::ProjectDir(), path);
			if (FFileHelper::LoadFileToArray(file_contents, *relpath) == false) {
				FString abspath = FPaths::ConvertRelativePathToFull(relpath);
				if (FFileHelper::LoadFileToArray(file_contents, *abspath) == false) {
					return TEXT("MiVRyActor's GestureDatabaseFile not found");
				}
			}
		}
		me->GestureNames.Empty();
		bool GestureNamesLoaded = false;

		// Try one-part/one-hand gesture recognition object
		IGestureRecognition* gro = (IGestureRecognition*)GestureRecognition_create();
		if (gro != nullptr) {
			if (gro->loadFromBuffer((const char*)file_contents.GetData(), file_contents.Num(), nullptr) == 0) {
				const int NumGestures = gro->numberOfGestures();
				me->GestureNames.SetNum(NumGestures);
				for (int32 i = 0; i < NumGestures; i++) {
					me->GestureNames[i] = gro->getGestureName(i);
				}
				GestureNamesLoaded = true;
			}
			delete gro;
		}

		// Try multi-part gesture combinations object
		if (GestureNamesLoaded == false) {
			IGestureCombinations* gco = (IGestureCombinations*)GestureCombinations_create(0);
			if (gco != nullptr) {
				if (gco->loadFromBuffer((const char*)file_contents.GetData(), file_contents.Num(), nullptr) == 0) {
					const int NumGestures = gco->numberOfGestureCombinations();
					me->GestureNames.SetNum(NumGestures);
					for (int32 i = 0; i < NumGestures; i++) {
						me->GestureNames[i] = gco->getGestureCombinationName(i);
					}
					GestureNamesLoaded = true;
				}
				delete gco;
			}
		}
		if (GestureNamesLoaded == false) {
			return TEXT("Failed to load Gesture Database File");
		}
	}
	FString SetStr, NotStr;
	switch (this->GestureIdListUse) {
		case UBTDecorator_MiVRy_GestureIdListUse::Whitelist:
			SetStr = TEXT("[o] PASS");
			NotStr = TEXT("[x] MISS");
			break;
		case UBTDecorator_MiVRy_GestureIdListUse::Blacklist:
			SetStr = TEXT("[x] MISS");
			NotStr = TEXT("[o] PASS");
			break;
		case UBTDecorator_MiVRy_GestureIdListUse::Ignore:
		default:
			SetStr = TEXT("[o] PASS");
			NotStr = TEXT("[o] PASS");
			break;
	}
	FString GesturesString;
	GesturesString += FString::Printf(TEXT("-1: [no gesture] -> %s\n"), this->GestureIDs.Contains(-1) ? *SetStr : *NotStr);
	for (int32 i = 0; i < GestureNames.Num(); i++) {
		FString GestureName = GestureNames[i];
		GesturesString += FString::Printf(TEXT("%i: %s -> %s\n"), i, *GestureName, this->GestureIDs.Contains(i) ? *SetStr : *NotStr);
	}
	return GesturesString;
}

void UBTDecorator_MiVRy::DescribeRuntimeValues(const UBehaviorTreeComponent& OwnerComp, uint8* NodeMemory, EBTDescriptionVerbosity::Type Verbosity, TArray<FString>& Values) const
{
	Super::DescribeRuntimeValues(OwnerComp, NodeMemory, Verbosity, Values);
	if (Verbosity == EBTDescriptionVerbosity::Detailed) {
		Values.Add(FString::Printf(TEXT("MiVRyActor: %s"), this->MiVRyActor != nullptr ? TEXT("set") : TEXT("not set")));
		Values.Add(FString::Printf(TEXT("SimilarityThreshold: %f"), this->SimilarityThreshold));
		switch (this->GestureIdListUse) {
			case UBTDecorator_MiVRy_GestureIdListUse::Whitelist:
				Values.Add(TEXT("Use of Gesture ID list: Whitelist"));
				break;
			case UBTDecorator_MiVRy_GestureIdListUse::Blacklist:
				Values.Add(TEXT("Use of Gesture ID list: Blacklist"));
				break;
			case UBTDecorator_MiVRy_GestureIdListUse::Ignore:
			default:
				Values.Add(TEXT("Use of Gesture ID list: Ignore"));
				break;
		}
	}
}