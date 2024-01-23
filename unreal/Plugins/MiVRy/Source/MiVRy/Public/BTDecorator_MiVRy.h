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

#pragma once

#include "CoreMinimal.h"
#include "BehaviorTree/BTDecorator.h"
#include "MiVRyActor.h"
#include "BTDecorator_MiVRy.generated.h"

UENUM()
namespace UBTDecorator_MiVRy_GestureIdListUse
{
	enum Type : int
	{
		Blacklist UMETA(DisplayName = "Blacklist", ToolTip = "Fail when identified gesture is in list."),
		Whitelist UMETA(DisplayName = "Whitelist", ToolTip = "Pass only when identified gesture is in list."),
		Ignore UMETA(DisplayName = "Ignore", ToolTip = "Ignore the list, always pass."),
	};
}


struct UBTDecorator_MiVRyMemory
{
	/**
	* Counter variable to detect new gesture performances/identifications.
	*/
	int LatestGestureCounter = 0;
};

/**
 * MiVRy gesture recognition decorator.
 * MiVRy decorators allow execution when the latest performed gesture matches a list of pre-defined gestures.
 * It can also interrupt the current execution when a new gesture is performed.
 */
UCLASS(meta = (DisplayName = "MiVRy Decorator"))
class MIVRY_API UBTDecorator_MiVRy : public UBTDecorator
{
	GENERATED_BODY()

	UBTDecorator_MiVRy(const FObjectInitializer& ObjectInitializer);
	virtual ~UBTDecorator_MiVRy();
	virtual void BeginDestroy() override;
	virtual void PostLoad() override;
	virtual void OnNodeActivation(FBehaviorTreeSearchData& SearchData) override;
	virtual void PostEditChangeProperty(struct FPropertyChangedEvent& PropertyChangedEvent) override;
	virtual uint16 GetInstanceMemorySize() const override;
	virtual void InitializeMemory(UBehaviorTreeComponent& OwnerComp, uint8* NodeMemory, EBTMemoryInit::Type InitType) const override;
	virtual bool CalculateRawConditionValue(UBehaviorTreeComponent& OwnerComp, uint8* NodeMemory) const override;
	virtual void TickNode(UBehaviorTreeComponent& OwnerComp, uint8* NodeMemory, float DeltaSeconds) override;
	virtual FString GetStaticDescription() const override;
	virtual void DescribeRuntimeValues(const UBehaviorTreeComponent& OwnerComp, uint8* NodeMemory, EBTDescriptionVerbosity::Type Verbosity, TArray<FString>& Values) const override;

public:
	/**
	* MiVRy actor which performs the gesture identification.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy", meta = (DisplayName = "MiVRy Actor"))
	TSoftObjectPtr<AMiVRyActor> MiVRyActor;

	/**
	* Whether any performed gesture is accepted only one time, or wether the latest performed gesture
	* is used for evaluation of this decorator until another gesture is performed.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy", meta = (DisplayName = "Evaluate every gesture only once"))
	bool EvalEveryGestureOnlyOnce = false;

	/**
	* How to interpret the "Gesture IDs" list below.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy", meta = (DisplayName = "Use of Gesture ID list"))
	TEnumAsByte<UBTDecorator_MiVRy_GestureIdListUse::Type> GestureIdListUse = UBTDecorator_MiVRy_GestureIdListUse::Whitelist;

	/** Gesture IDs (or error IDs) accepted or rejected by this decorator. Use '-1' for 'no gesture' or 'identification failure'. */
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy", meta = (DisplayName = "Gesture IDs"))
	TSet<int32> GestureIDs;

	/**
	* Minimum similarity between the performed gesture and recorded gestures to accept the identification.
	* Similarities range from zero to one, where one means that the performed gesture was the exact average
	* off all previously recorded gesture samples.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy", meta = (DisplayName = "Similarity Threshold"))
	float SimilarityThreshold = 0.0f;

	/**
	* Whether to display which gestures of the MiVRy Actor are accepted in the Behavior Tree node.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy", meta = (DisplayName = "Display Gestures In Node"))
	bool DisplayGesturesInNode = false;

private:

	/**
	* File path to the Gesture Database (.DAT) file used by MiVRyActor.
	*/
	FString GestureDatabaseFile;

	/**
	* Names of Gestures found in the Gesture Database (.DAT) file.
	*/
	TArray<FString> GestureNames;

	/**
	* Delegate function to be called by MiVRy Actor.
	*/
	UFUNCTION()
	void OnGestureIdentified(
		AMiVRyActor* Source,
		GestureRecognition_Identification Result,
		int GestureID,
		FString GestureName,
		float Similarity,
		const TArray<FMiVRyGesturePart>& GestureParts
	);

	/**
	* Delegate to re-route callbacks from MiVRyActor.
	*/
	FScriptDelegate Delegate;

	/**
	* If the delegate is bound, the bound-to MiVRyActor is stored here.
	*/
	TSoftObjectPtr<AMiVRyActor> BoundMiVRyActor;

	/**
	* The ID (index) of the gesture which was last identified,
	* or a negative error code if identification failed.
	*/
	int LatestGestureId = -1;

	/**
	* Counter variable to detect new gesture performances/identifications.
	*/
	int LatestGestureCounter = 0;
};
