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

#include "MiVRyUtil.h"
#include "Core.h"


FString UMiVRyUtil::errorCodeToString(int errorCode)
{
    switch (errorCode)
    {
        case 0:
            return "Function executed successfully.";
        case -1:
            return "Invalid parameter(s) provided to function.";
        case -2:
            return "Invalid index provided to function.";
        case -3:
            return "Invalid file path provided to function.";
        case -4:
            return "Path to an invalid file provided to function.";
        case -5:
            return "Calculations failed due to numeric instability(too small or too large numbers).";
        case -6:
            return "The internal state of the AI was corrupted.";
        case -7:
            return "Available data(number of samples etc) is insufficient for this operation.";
        case -8:
            return "The operation could not be performed because the AI is currently training.";
        case -9:
            return "No gestures registered.";
        case -10:
            return "The neural network is inconsistent - re-training might solve the issue.";
        case -11:
            return "File or object exists and can't be overwritten.";
    }
    return "Unknown error.";
}

