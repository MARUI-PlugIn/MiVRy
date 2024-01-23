/*
 * MiVRy - VR gesture recognition library plug-in for Unreal.
 * Version 2.10
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

#include "BTComposite_MiVRy.h"
#include "Misc/FileHelper.h"
#include "GestureRecognition.h"
#include "GestureCombinations.h"
#include "BehaviorTree/BlackboardComponent.h"
#include "BehaviorTree/Blackboard/BlackboardKeyType_Vector.h"
#include "BehaviorTree/Blackboard/BlackboardKeyType_Rotator.h"
#include "BehaviorTree/Blackboard/BlackboardKeyType_Bool.h"
#include "BehaviorTree/Blackboard/BlackboardKeyType_Float.h"
#include "BehaviorTree/Blackboard/BlackboardKeyType_Int.h"
#include "BehaviorTree/Blackboard/BlackboardKeyType_String.h"

UBTComposite_MiVRy::UBTComposite_MiVRy(const FObjectInitializer& ObjectInitializer) : Super(ObjectInitializer)
{
	this->NodeName = "MiVRy Gesture Recognition";
	INIT_COMPOSITE_NODE_NOTIFY_FLAGS();
	this->bUseNodeActivationNotify = 1;
	this->Delegate.BindUFunction(this, "OnGestureIdentified");
	if (!this->MiVRyActor.IsNull()) {
		this->MiVRyActor->OnGestureIdentifiedDelegate.Add(this->Delegate);
		this->BoundMiVRyActor = this->MiVRyActor;
	}
}

void UBTComposite_MiVRy::PostLoad()
{
	Super::PostLoad();
	if (this->BoundMiVRyActor.IsNull() && !this->MiVRyActor.IsNull()) {
		this->MiVRyActor->OnGestureIdentifiedDelegate.Add(this->Delegate);
		this->BoundMiVRyActor = this->MiVRyActor;
	}
}

void UBTComposite_MiVRy::PostEditChangeProperty(struct FPropertyChangedEvent& PropertyChangedEvent)
{
	if (PropertyChangedEvent.Property == nullptr) {
		return;
	}
	FName PropertyName = PropertyChangedEvent.Property->GetFName();
	if (PropertyName != GET_MEMBER_NAME_CHECKED(UBTComposite_MiVRy, MiVRyActor)) {
		Super::PostEditChangeProperty(PropertyChangedEvent);
		return;
	} // else:
	if (!this->BoundMiVRyActor.IsNull()) {
		if (this->BoundMiVRyActor.IsValid()) {
			this->BoundMiVRyActor->OnGestureIdentifiedDelegate.RemoveAll(this);
		}
		this->BoundMiVRyActor = nullptr;
	}
	Super::PostEditChangeProperty(PropertyChangedEvent);
	if (!this->MiVRyActor.IsNull()) {
		this->MiVRyActor->OnGestureIdentifiedDelegate.Add(this->Delegate);
		this->BoundMiVRyActor = this->MiVRyActor;
	}
}

void UBTComposite_MiVRy::NotifyNodeActivation(FBehaviorTreeSearchData& SearchData) const
{
	if (this->BoundMiVRyActor.IsNull() && !this->MiVRyActor.IsNull()) {
		UBTComposite_MiVRy* me = (UBTComposite_MiVRy*)this;
		me->MiVRyActor->OnGestureIdentifiedDelegate.Add(this->Delegate);
		me->BoundMiVRyActor = this->MiVRyActor;
	}
}

UBTComposite_MiVRy::~UBTComposite_MiVRy()
{
	if (!this->BoundMiVRyActor.IsNull()) {
		if (this->BoundMiVRyActor.IsValid()) {
			this->BoundMiVRyActor->OnGestureIdentifiedDelegate.RemoveAll(this);
		}
		this->BoundMiVRyActor = nullptr;
	}
	this->Delegate.Clear();
}

void UBTComposite_MiVRy::BeginDestroy()
{
	if (!this->BoundMiVRyActor.IsNull()) {
		if (this->BoundMiVRyActor.IsValid()) {
			this->BoundMiVRyActor->OnGestureIdentifiedDelegate.RemoveAll(this);
		}
		this->BoundMiVRyActor = nullptr;
	}
	this->Delegate.Clear();
	Super::BeginDestroy();
}

uint16 UBTComposite_MiVRy::GetInstanceMemorySize() const
{
	return sizeof(UBTComposite_MiVRyMemory);
}

void UBTComposite_MiVRy::InitializeMemory(UBehaviorTreeComponent& OwnerComp, uint8* NodeMemory, EBTMemoryInit::Type InitType) const
{
	Super::InitializeMemory(OwnerComp, NodeMemory, InitType);
	UBTComposite_MiVRyMemory* Memory = CastInstanceNodeMemory<UBTComposite_MiVRyMemory>(NodeMemory);
	if (InitType == EBTMemoryInit::Initialize)
	{
		Memory->LatestGestureCounter = 0;
	}
}

int32 UBTComposite_MiVRy::GetNextChildHandler(FBehaviorTreeSearchData& SearchData, int32 PrevChild, EBTNodeResult::Type LastResult) const
{
	UBTComposite_MiVRyMemory* Memory = GetNodeMemory<UBTComposite_MiVRyMemory>(SearchData);
	TEnumAsByte<UBTComposite_MiVRy_ChildFinishReaction::Type> reaction = UBTComposite_MiVRy_ChildFinishReaction::Continue;
	switch (LastResult) {
	case EBTNodeResult::Succeeded:
		reaction = this->OnChildSuccess;
		break;
	case EBTNodeResult::Failed:
		reaction = this->OnChildFailure;
		break;
	case EBTNodeResult::Aborted:
		reaction = this->OnChildAborted;
		break;
	}
	if (reaction == UBTComposite_MiVRy_ChildFinishReaction::Stop) {
		return BTSpecialChild::ReturnToParent;
	}
	// else:
	if (this->LatestGestureId >= 0) { // valid gesture ID
		this->SetLatestGestureBlackboardVars(SearchData.OwnerComp.GetBlackboardComponent());
		if (this->GestureChildMapping.Contains(this->LatestGestureId) == false) {
			return BTSpecialChild::ReturnToParent;
		}
		const int32 NextChild = this->GestureChildMapping[this->LatestGestureId];
		if (Memory->LatestGestureCounter != this->LatestGestureCounter) {
			Memory->LatestGestureCounter = this->LatestGestureCounter;
			return NextChild;
		}
		// else: already executed
		if (this->RepeatIdentifiedGestureChild) {
			return NextChild;
		}
		// else: already executed and not repeated
		if (this->GestureChildMapping.Contains(-1) && this->RepeatNoGestureChild) {
			return this->GestureChildMapping[-1]; // return to repeating the 'no-gesture' child (0)
		}
		return BTSpecialChild::ReturnToParent;
	}
	// else: (LatestGestureId < 0) -> "no-gesture"
	if (this->GestureChildMapping.Contains(-1) == false) {
		return BTSpecialChild::ReturnToParent;
	}
	if (Memory->LatestGestureCounter != this->LatestGestureCounter) {
		Memory->LatestGestureCounter = this->LatestGestureCounter;
		return this->GestureChildMapping[-1];
	}
	// else: already executed
	if (this->RepeatNoGestureChild) {
		return this->GestureChildMapping[-1];
	}
	// else:
	return BTSpecialChild::ReturnToParent;
}

void UBTComposite_MiVRy::OnGestureIdentified(
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
		this->LatestGestureName = GestureName;
		this->LatestGestureSimilarity = Similarity;
		this->LatestGestureParts = GestureParts;
	} else {
		this->LatestGestureId = -1;
	}
	this->LatestGestureCounter += 1;
}

void UBTComposite_MiVRy::SetLatestGestureBlackboardVars(UBlackboardComponent* BlackboardComponent) const
{
	if (this->LatestGestureBlackboardKeys.Num() <= 0) {
		return;
	}
	if (BlackboardComponent == nullptr) {
		UE_LOG(LogTemp, Warning, TEXT("UBTComposite_MiVRy::SetLatestGestureBlackboardVars: BlackboardComponent not set."));
	}
	if (this->LatestGestureBlackboardKeys.Contains(UBTComposite_MiVRy_LatestGestureData::GestureId)) {
		const FBlackboardKeySelector& Key = this->LatestGestureBlackboardKeys[UBTComposite_MiVRy_LatestGestureData::GestureId];
		BlackboardComponent->SetValue<UBlackboardKeyType_Int>(Key.SelectedKeyName, this->LatestGestureId);
	}
	if (this->LatestGestureBlackboardKeys.Contains(UBTComposite_MiVRy_LatestGestureData::GestureName)) {
		const FBlackboardKeySelector& Key = this->LatestGestureBlackboardKeys[UBTComposite_MiVRy_LatestGestureData::GestureName];
		BlackboardComponent->SetValue<UBlackboardKeyType_String>(Key.SelectedKeyName, this->LatestGestureName);
	}
	const FMiVRyGesturePart* LeftGesturePart = nullptr;
	const FMiVRyGesturePart* RightGesturePart = nullptr;
	for (int i = this->LatestGestureParts.Num() - 1; i >= 0; i--) {
		const FMiVRyGesturePart* GesturePart = &this->LatestGestureParts[i];
		if (GesturePart->Side == GestureRecognition_Side::Left) {
			LeftGesturePart = GesturePart;
		} else if (GesturePart->Side == GestureRecognition_Side::Right) {
			RightGesturePart = GesturePart;
		}
	}
	if (this->LatestGestureBlackboardKeys.Contains(UBTComposite_MiVRy_LatestGestureData::LeftPerformed)) {
		const FBlackboardKeySelector& Key = this->LatestGestureBlackboardKeys[UBTComposite_MiVRy_LatestGestureData::LeftPerformed];
		BlackboardComponent->SetValue<UBlackboardKeyType_Bool>(Key.SelectedKeyName, LeftGesturePart != nullptr);
	}
	if (LeftGesturePart != nullptr) {
		if (this->LatestGestureBlackboardKeys.Contains(UBTComposite_MiVRy_LatestGestureData::LeftPosition)) {
			const FBlackboardKeySelector& Key = this->LatestGestureBlackboardKeys[UBTComposite_MiVRy_LatestGestureData::LeftPosition];
			BlackboardComponent->SetValue<UBlackboardKeyType_Vector>(Key.SelectedKeyName, LeftGesturePart->Position);
		}
		if (this->LatestGestureBlackboardKeys.Contains(UBTComposite_MiVRy_LatestGestureData::LeftRotation)) {
			const FBlackboardKeySelector& Key = this->LatestGestureBlackboardKeys[UBTComposite_MiVRy_LatestGestureData::LeftRotation];
			BlackboardComponent->SetValue<UBlackboardKeyType_Rotator>(Key.SelectedKeyName, LeftGesturePart->Rotation);
		}
		if (this->LatestGestureBlackboardKeys.Contains(UBTComposite_MiVRy_LatestGestureData::LeftScale)) {
			const FBlackboardKeySelector& Key = this->LatestGestureBlackboardKeys[UBTComposite_MiVRy_LatestGestureData::LeftScale];
			BlackboardComponent->SetValue<UBlackboardKeyType_Float>(Key.SelectedKeyName, LeftGesturePart->Scale);
		}
		if (this->LatestGestureBlackboardKeys.Contains(UBTComposite_MiVRy_LatestGestureData::LeftPrimaryDirection)) {
			const FBlackboardKeySelector& Key = this->LatestGestureBlackboardKeys[UBTComposite_MiVRy_LatestGestureData::LeftPrimaryDirection];
			BlackboardComponent->SetValue<UBlackboardKeyType_Vector>(Key.SelectedKeyName, LeftGesturePart->PrimaryDirection);
		}
		if (this->LatestGestureBlackboardKeys.Contains(UBTComposite_MiVRy_LatestGestureData::LeftSecondaryDirection)) {
			const FBlackboardKeySelector& Key = this->LatestGestureBlackboardKeys[UBTComposite_MiVRy_LatestGestureData::LeftSecondaryDirection];
			BlackboardComponent->SetValue<UBlackboardKeyType_Vector>(Key.SelectedKeyName, LeftGesturePart->SecondaryDirection);
		}
	}
	if (this->LatestGestureBlackboardKeys.Contains(UBTComposite_MiVRy_LatestGestureData::RightPerformed)) {
		const FBlackboardKeySelector& Key = this->LatestGestureBlackboardKeys[UBTComposite_MiVRy_LatestGestureData::RightPerformed];
		BlackboardComponent->SetValue<UBlackboardKeyType_Bool>(Key.SelectedKeyName, RightGesturePart != nullptr);
	}
	if (RightGesturePart != nullptr) {
		if (this->LatestGestureBlackboardKeys.Contains(UBTComposite_MiVRy_LatestGestureData::RightPosition)) {
			const FBlackboardKeySelector& Key = this->LatestGestureBlackboardKeys[UBTComposite_MiVRy_LatestGestureData::RightPosition];
			BlackboardComponent->SetValue<UBlackboardKeyType_Vector>(Key.SelectedKeyName, RightGesturePart->Position);
		}
		if (this->LatestGestureBlackboardKeys.Contains(UBTComposite_MiVRy_LatestGestureData::RightRotation)) {
			const FBlackboardKeySelector& Key = this->LatestGestureBlackboardKeys[UBTComposite_MiVRy_LatestGestureData::RightRotation];
			BlackboardComponent->SetValue<UBlackboardKeyType_Rotator>(Key.SelectedKeyName, RightGesturePart->Rotation);
		}
		if (this->LatestGestureBlackboardKeys.Contains(UBTComposite_MiVRy_LatestGestureData::RightScale)) {
			const FBlackboardKeySelector& Key = this->LatestGestureBlackboardKeys[UBTComposite_MiVRy_LatestGestureData::RightScale];
			BlackboardComponent->SetValue<UBlackboardKeyType_Float>(Key.SelectedKeyName, RightGesturePart->Scale);
		}
		if (this->LatestGestureBlackboardKeys.Contains(UBTComposite_MiVRy_LatestGestureData::RightPrimaryDirection)) {
			const FBlackboardKeySelector& Key = this->LatestGestureBlackboardKeys[UBTComposite_MiVRy_LatestGestureData::RightPrimaryDirection];
			BlackboardComponent->SetValue<UBlackboardKeyType_Vector>(Key.SelectedKeyName, RightGesturePart->PrimaryDirection);
		}
		if (this->LatestGestureBlackboardKeys.Contains(UBTComposite_MiVRy_LatestGestureData::RightSecondaryDirection)) {
			const FBlackboardKeySelector& Key = this->LatestGestureBlackboardKeys[UBTComposite_MiVRy_LatestGestureData::RightSecondaryDirection];
			BlackboardComponent->SetValue<UBlackboardKeyType_Vector>(Key.SelectedKeyName, RightGesturePart->SecondaryDirection);
		}
	}
}

FString UBTComposite_MiVRy::GetStaticDescription() const
{
	if (this->DisplayGestureMappingInNode == false) {
		return Super::GetStaticDescription();
	}
	if (this->MiVRyActor == nullptr) {
		return TEXT("MiVRyActor not set");
	}
	UBTComposite_MiVRy* me = (UBTComposite_MiVRy*)this;
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
	FString MappingString;
	if (this->GestureChildMapping.Contains(-1)) {
		MappingString += FString::Printf(TEXT("-1: [no gesture] -> child %i\n"), this->GestureChildMapping[-1]);
	}
	for (int32 i = 0; i < GestureNames.Num(); i++) {
		FString GestureName = GestureNames[i];
		MappingString += FString::Printf(TEXT("%i: %s"), i, *GestureName);
		if (this->GestureChildMapping.Contains(i)) {
			MappingString += FString::Printf(TEXT(" -> child %i\n"), this->GestureChildMapping[i]);
		} else {
			MappingString += TEXT("\n");
		}
	}
	return MappingString;
}

void UBTComposite_MiVRy::DescribeRuntimeValues(const UBehaviorTreeComponent& OwnerComp, uint8* NodeMemory, EBTDescriptionVerbosity::Type Verbosity, TArray<FString>& Values) const
{
	Super::DescribeRuntimeValues(OwnerComp, NodeMemory, Verbosity, Values);
	if (Verbosity == EBTDescriptionVerbosity::Detailed) {
		Values.Add(FString::Printf(TEXT("MiVRyActor: %s"), this->MiVRyActor != nullptr ? TEXT("set") : TEXT("not set")));
		Values.Add(FString::Printf(TEXT("SimilarityThreshold: %f"), this->SimilarityThreshold));
		Values.Add(FString::Printf(TEXT("RepeatIdentifiedGestureChild: %i"), this->RepeatIdentifiedGestureChild ? 1 : 0));
		Values.Add(FString::Printf(TEXT("RepeatNoGestureChild: %i"), this->RepeatNoGestureChild ? 1 : 0));
	}
}
