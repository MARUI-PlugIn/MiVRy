/*
 * MiVRy - VR gesture recognition library plug-in for Unreal.
 * Version 1.20
 * Copyright (c) 2021 MARUI-PlugIn (inc.)
 *
 * MiVRy is licensed under a Creative Commons Attribution-NonCommercial 4.0 International License
 * ( http://creativecommons.org/licenses/by-nc/4.0/ )
 *
 * This software is free to use for non-commercial purposes.
 * You may use this software in part or in full for any project
 * that does not pursue financial gain, including free software
 * and projects completed for evaluation or educational purposes only.
 * Any use for commercial purposes is prohibited.
 * You may not sell or rent any software that includes
 * this software in part or in full, either in it's original form
 * or in altered form.
 * If you wish to use this software in a commercial application,
 * please contact us at support@marui-plugin.com to obtain
 * a commercial license.
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
 * Utility function class for the MiVRy Gesture Recognition plug-in.
 */
UCLASS()
class MIVRY_API UMiVRyUtil : public UBlueprintFunctionLibrary
{
	GENERATED_BODY()
	
public:

    /**
    * Turn error code into human-readable error message.
    * @param errorCode The error code.
    * @return The error message associated with the integer error code.
    */
    UFUNCTION(BlueprintPure, Category = "MiVRy Util", Meta = (DisplayName = "Error Code to String"))
        static FString errorCodeToString(int errorCode);

};

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
