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
#include "BehaviorTree/BTCompositeNode.h"
#include "MiVRyActor.h"
#include "BTComposite_MiVRy.generated.h"

UENUM()
namespace UBTComposite_MiVRy_ChildFinishReaction
{
	enum Type : int
	{
		Stop UMETA(DisplayName = "Stop Execution", ToolTip = "Stop executing child nodes.."),
		Continue UMETA(DisplayName = "Continue Execution", ToolTip = "Continue executing child nodes."),
	};
}

/**
* Index/ID of the (hand) side (0=left, 1=right).
*/
UENUM(BlueprintType)
enum class UBTComposite_MiVRy_LatestGestureData : uint8
{
	GestureId               = 0  UMETA(DisplayName = "Gesture ID (Int)"),
	GestureName             = 1  UMETA(DisplayName = "Gesture Name (String)"),
	LeftPerformed           = 2  UMETA(DisplayName = "[Left] Performed (Bool)"),
	LeftPosition            = 3  UMETA(DisplayName = "[Left] Position (Vector)"),
	LeftRotation            = 4  UMETA(DisplayName = "[Left] Rotation (Rotator)"),
	LeftScale               = 5  UMETA(DisplayName = "[Left] Scale (Float)"),
	LeftPrimaryDirection    = 6  UMETA(DisplayName = "[Left] Primary Direction (Vector)"),
	LeftSecondaryDirection  = 7  UMETA(DisplayName = "[Left] Secondary Direction (Vector)"),
	RightPerformed          = 8  UMETA(DisplayName = "[Right] Performed (Bool)"),
	RightPosition           = 9 UMETA(DisplayName = "[Right] Position (Vector)"),
	RightRotation           = 10 UMETA(DisplayName = "[Right] Rotation (Rotator)"),
	RightScale              = 11 UMETA(DisplayName = "[Right] Scale (Float)"),
	RightPrimaryDirection   = 12 UMETA(DisplayName = "[Right] Primary Direction (Vector)"),
	RightSecondaryDirection = 13 UMETA(DisplayName = "[Right] Secondary Direction (Vector)"),
};


struct UBTComposite_MiVRyMemory : public FBTCompositeMemory
{
	/**
	* Counter variable to detect new gesture performances/identifications.
	*/
	int LatestGestureCounter = 0;
};


/**
 * MiVRy gesture recognition composite node.
 * MiVRy Nodes execute the child which matches the identified gesture ID (from left to right,
 * where the left-most child is executed when  from left to right, and will stop executing its children when one of their children succeeds.
 * If a Selector's child succeeds, the Selector succeeds. If all the Selector's children fail, the Selector fails.
 */
UCLASS(meta = (DisplayName = "MiVRy Composite"))
class MIVRY_API UBTComposite_MiVRy : public UBTCompositeNode
{
	GENERATED_BODY()
	
	UBTComposite_MiVRy(const FObjectInitializer& ObjectInitializer);
	virtual ~UBTComposite_MiVRy();
	virtual void PostLoad() override;
	virtual void BeginDestroy() override;
	virtual void PostEditChangeProperty(struct FPropertyChangedEvent& PropertyChangedEvent) override;
	virtual void NotifyNodeActivation(FBehaviorTreeSearchData& SearchData) const override;
	virtual int32 GetNextChildHandler(struct FBehaviorTreeSearchData& SearchData, int32 PrevChild, EBTNodeResult::Type LastResult) const override;
	virtual uint16 GetInstanceMemorySize() const override;
	virtual void InitializeMemory(UBehaviorTreeComponent& OwnerComp, uint8* NodeMemory, EBTMemoryInit::Type InitType) const override;
	virtual FString GetStaticDescription() const override;
	virtual void DescribeRuntimeValues(const UBehaviorTreeComponent& OwnerComp, uint8* NodeMemory, EBTDescriptionVerbosity::Type Verbosity, TArray<FString>& Values) const override;

public:

	/**
	* MiVRy actor which performs the gesture identification.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy", meta = (DisplayName = "MiVRy Actor"))
	TSoftObjectPtr<AMiVRyActor> MiVRyActor;

	/**
	* Minimum similarity between the performed gesture and recorded gestures to accept the identification.
	* Similarities range from zero to one, where one means that the performed gesture was the exact average
	* off all previously recorded gesture samples.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy", meta = (DisplayName = "Similarity Threshold"))
	float SimilarityThreshold = 0.0f;

	/**
	* Blackboard variable to edit when a gesture was identified.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy", meta = (DisplayName = "Blackboard Keys for latest Gesture"))
	TMap<UBTComposite_MiVRy_LatestGestureData, FBlackboardKeySelector> LatestGestureBlackboardKeys;

	/**
	* What to do when the last executed child finishes with "Success".
	*/
	UPROPERTY(EditInstanceOnly, Category = "MiVRy", meta = (DisplayName = "When child finishes with 'Success'"))
	TEnumAsByte<UBTComposite_MiVRy_ChildFinishReaction::Type> OnChildSuccess = UBTComposite_MiVRy_ChildFinishReaction::Continue;

	/**
	* What to do when the last executed child finishes with "Failure".
	*/
	UPROPERTY(EditInstanceOnly, Category = "MiVRy", meta = (DisplayName = "When child finishes with 'Failure'"))
	TEnumAsByte<UBTComposite_MiVRy_ChildFinishReaction::Type> OnChildFailure = UBTComposite_MiVRy_ChildFinishReaction::Continue;

	/**
	* What to do when the last executed child finishes with "Abort".
	*/
	UPROPERTY(EditInstanceOnly, Category = "MiVRy", meta = (DisplayName = "When child finishes with 'Abort'"))
	TEnumAsByte<UBTComposite_MiVRy_ChildFinishReaction::Type> OnChildAborted = UBTComposite_MiVRy_ChildFinishReaction::Stop;

	/** Which Gesture ID (first number) should lead to the execution of which child
	* (second number, where "0" is the leftmost child). A Gesture ID of "-1" means "no gesture was identified".
	* For example: "[2,0]" means "on Gesture ID 2, execute the left-most child (index = 0)",
	* "[-1,6]" means "when no gesture was identified, execute the 7th child (index = 6, counting from left)".
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy", meta = (DisplayName = "Mapping: GestureID -> Child"))
	TMap<int32, int32> GestureChildMapping;

	/**
	* If false, the child associated with the identified gesture will be executed only once.
	* If true, the child associated with the identified gesture will be executed continuously,
	* until StopExecutingIdentifiedGestureChild is set.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy", meta = (DisplayName = "Repeat Identified Gesture Child"))
	bool RepeatIdentifiedGestureChild = false;

	/**
	* If "First Child is 'No-Gesture'" is set, this property controls whether the 'No-Gesture' child will be executed repeatedly.
	* If false, the 'no-gesture' child will be executed only once.
	* If true, the 'no-gesture' will be executed continuously, until StopExecutingNoGestureChild is set.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy", meta = (DisplayName = "Repeat 'No-Gesture' Child"))
	bool RepeatNoGestureChild = true;

	/**
	* Whether to display which gestures of the MiVRy Actor are mapped to which children of this Behavior Tree node.
	*/
	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "MiVRy", meta = (DisplayName = "Display Gesture Mapping In Node"))
	bool DisplayGestureMappingInNode = false;

private:
	/**
	* The ID (index) of the gesture which was last identified,
	* or a negative error code if identification failed.
	*/
	int LatestGestureId = -1;

	/**
	* The name of the gesture which was last identified.
	*/
	FString LatestGestureName;

	/**
	* The similarity value of the gesture which was last identified.
	*/
	float LatestGestureSimilarity = -1;

	/**
	* The gesture motion parts of the gesture which was last identified.
	*/
	TArray<FMiVRyGesturePart> LatestGestureParts;

	/**
	* Counter variable to detect new gesture performances/identifications.
	*/
	int LatestGestureCounter = 0;

	/**
	* File path to the Gesture Database (.DAT) file used by MiVRyActor.
	*/
	FString GestureDatabaseFile;

	/**
	* Names of Gestures found in the Gesture Database (.DAT) file.
	*/
	TArray<FString> GestureNames;

	/**
	* Delegate to re-route callbacks from MiVRyActor.
	*/
	FScriptDelegate Delegate;

	/**
	* If the delegate is bound, the bound-to MiVRyActor is stored here.
	*/
	TSoftObjectPtr<AMiVRyActor> BoundMiVRyActor;

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
	* Helper function to update the Blackboard variables
	* set in LatestGestureBlackboardKeys with the
	* gesture data of the last gesture.
	*/
	void SetLatestGestureBlackboardVars(UBlackboardComponent* BlackboardComponent) const;
};
