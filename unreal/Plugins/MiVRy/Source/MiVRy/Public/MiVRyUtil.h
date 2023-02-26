/*
 * MiVRy - VR gesture recognition library plug-in for Unreal.
 * Version 2.7
 * Copyright (c) 2023 MARUI-PlugIn (inc.)
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
#include "Kismet/BlueprintFunctionLibrary.h"
#include "MiVRyUtil.generated.h"


/**
* Possible results for calling MiVRy functions.
*/
UENUM(BlueprintType)
enum class GestureRecognition_Result : uint8
{
    Then, Error
};

/**
* Possible results for attempts to identify a gesture.
*/
UENUM(BlueprintType)
enum class GestureRecognition_Identification : uint8
{
    WaitingForOtherHand, FailedToIdentify, GestureIdentified
};

/**
* Index/ID of the (hand) side (0=left, 1=right).
*/
UENUM(BlueprintType)
enum class GestureRecognition_Side : uint8
{
    Left  = 0 UMETA(DisplayName = "0/Left"),
    Right = 1 UMETA(DisplayName = "1/Right"),
};

/**
* Whether a gesture is interpreted as seen from the users POV (head)
* or an objective/3rd person POV (world).
*/
UENUM(BlueprintType)
enum class GestureRecognition_FrameOfReference : uint8
{
    Headset = 0 UMETA(DisplayName = "Head"),
    World = 1 UMETA(DisplayName = "World"),
};

/**
* Whether to update the hmd (frame of reference) position/rotation during the gesturing motion.
*/
UENUM(BlueprintType)
enum class GestureRecognition_UpdateHeadPositionPolicy : uint8 
{
    UpdateHeadPositionPolicy_UseLatest = 0 UMETA(DisplayName = "Use Latest")//!< Use the hmd position most recently submitted as current head position.
    ,
    UpdateHeadPositionPolicy_UseInitial = 1 UMETA(DisplayName = "Use Initial") //!< Use the initial head position, don't use later head positional updates.
};

/**
* Different orderings of rotation for (Euler) rotation angles.
*/
UENUM(BlueprintType)
enum class GestureRecognition_RotationOrder : uint8
{
    XYZ = 0 UMETA(DisplayName = "x->y->z"),
    XZY = 1 UMETA(DisplayName = "x->z->y"),
    YXZ = 2 UMETA(DisplayName = "y->x->z"),
    YZX = 3 UMETA(DisplayName = "y->z->x"),
    ZXY = 4 UMETA(DisplayName = "z->x->y"),
    ZYX = 5 UMETA(DisplayName = "z->y->x"),
};


/**
* Type of target VR device.
*/
UENUM(BlueprintType)
enum class GestureRecognition_DeviceType : uint8
{
    None = 0 UMETA(DisplayName = "None"),
    Headset = 1 UMETA(DisplayName = "Headset"),
    Controller = 2 UMETA(DisplayName = "Controller"),
};

/**
* Type of target VR device.
*/
UENUM(BlueprintType)
enum class GestureRecognition_CoordinateSystem : uint8
{
    Unreal = 0 UMETA(DisplayName = "Unreal"),
    UnityOpenXR = 1 UMETA(DisplayName = "Unity OpenXR"),
    UnityOculusVR = 2 UMETA(DisplayName = "Unity OculusVR"),
    UnitySteamVR = 3 UMETA(DisplayName = "Unity SteamVR"),
};


/**
 * Utility function class for the MiVRy Gesture Recognition plug-in.
 */
UCLASS()
class MIVRY_API UMiVRyUtil : public UBlueprintFunctionLibrary
{
	GENERATED_BODY()
	
public:

    /**
    * Turn error code into human-readable error message.
    * @return A human-readable string describing this version of MiVRy.
    */
    UFUNCTION(BlueprintPure, Category = "MiVRy Util", Meta = (DisplayName = "Version String"))
        static FString versionString();

    /**
    * Turn error code into human-readable error message.
    * @param errorCode The error code.
    * @return The error message associated with the integer error code.
    */
    UFUNCTION(BlueprintPure, Category = "MiVRy Util", Meta = (DisplayName = "Error Code to String"))
        static FString errorCodeToString(int errorCode);

    /**
    * Find gesture database file in the various UE directories.
    * This is just a helper function to simplify cross-platform use of relative file paths
    * in the ProjectDir, ProjectContentDir, and other UE directories.
    * The path can be used with the "loadFromFile" and "importFromFile" functions
    * of the GestureRecognitionActor and GestureCombinationsActor.
    * @param Path The file name or relative path of the gesture database file.
    * @param Result Result of the search process.
    * @param FoundPath The absolute filesystem path to the gesture database file.
    */
    UFUNCTION(BlueprintCallable, Category = "MiVRy Util", Meta = (DisplayName = "Find Gesture Database File", ExpandEnumAsExecs = "Result"))
        static void findFile(const FString& Path, GestureRecognition_Result& Result, FString& FoundPath);

    /**
    * Load gesture database file into an array buffer.
    * The buffer can be used with the "loadFromBuffer" and "importFromBuffer" functions
    * of the GestureRecognitionActor and GestureCombinationsActor.
    * @param Path The file path from where to load the gesture database.
    * @param Result Result of the loading process.
    * @param Data The contents of the gesture database file.
    */
    UFUNCTION(BlueprintCallable, Category = "MiVRy Util", Meta = (DisplayName = "Read Gesture Database File to Buffer", ExpandEnumAsExecs = "Result"))
        static void readFileToBuffer(const FString& Path, GestureRecognition_Result& Result, TArray<uint8>& Data);

    /**
    * Helper function to convert UnrealEngine coordinates to internal MiVRy coordinates (if they differ from Unreal coordinates).
    * @param location           The position in Unreal coordinates.
    * @param rotation           The rotation in Unreal coordinates.
    * @param device_type        Whether the device is a VR handheld controller, a headset, or neither.
    * @param coord_sys          The internal coordinate system used by MiVRy.
    * @param p                  [OUT] The translation in MiVRy's internal coordinates.
    * @param q                  [OUT] The rotation quaternion in MiVRy's internal coordinates.
    */
    static void convertInput(const FVector& location, const FQuat& rotation, GestureRecognition_DeviceType device_type, GestureRecognition_CoordinateSystem coord_sys, double p[3], double q[4]);

    /**
    * Helper function to convert internal MiVRy coordinates to UnrealEngine coordinates (if they differ from Unreal coordinates).
    * @param coord_sys          The internal coordinate system used by MiVRy.
    * @param p                  The location in MiVRy's internal coordinate system.
    * @param q                  The orientation in MiVRy's internal coordinate system.
    * @param device_type        Whether the device is a VR handheld controller, a headset, or neither.
    * @param location           [OUT] UnrealEngine coordinate location.
    * @param rotation           [OUT] UnrealEngine coordinate orientation.
    */
    static void convertOutput(GestureRecognition_CoordinateSystem coord_sys, const double p[3], const double q[4], GestureRecognition_DeviceType device_type, FVector& location, FQuat& rotation);
};
