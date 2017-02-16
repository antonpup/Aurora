/*********************************************************

LFX2.h - Defines the exports for the LightFX 2.0 DLL

Purpose: Provide library exports for communicating with
the LightFX 2.0 API

Copyright (c) 2007 Dell, Inc. All rights reserved.
Date: 8/7/2007

Dell, Inc. makes no warranty of any kind with regard
to this material, including, but not limited to, the
implied warranties of merchantability and fitness for
a particular purpose. Dell, Inc. shall not be liable
for any errors contained herein, or for incidental or
consequential damages in connection with the furnishing,
performance or use of this material.

This document contains proprietary information which
is protected by copyright. All rights reserved.
Reproduction without the written permission of Dell
is strictly forbidden.

**********************************************************/

#pragma once

#include "LFXDecl.h"
#define _EXPORTING

#ifdef _EXPORTING // To be used by SDK developer
#define FN_DECLSPEC __declspec(dllexport)

#elif _IMPORTING // To be used for dynamic linking to dll
#define FN_DECLSPEC __declspec(dllimport)

#else // To be used for linking using static library
#define FN_DECLSPEC    
#endif

#ifdef _STDCALL_SUPPORTED
#define STDCALL __stdcall // Declare our calling convention
#else
#define STDCALL
#endif // STDCALL_SUPPORTED

#ifdef __cplusplus
extern "C" {
#endif

	// LightFX 2.0 DLL export function declarations

	/*********************************************************
	Function: LFX_Initialize
	Description:
	Initializes the LightFX 2.0 system.
	This function must be called prior to any other library calls being made. If this
	function is not called, the system will not be initialized and the other functions
	will return LFX_ERROR_NOINIT or LFX_FAILURE.
	Inputs: None
	Outputs: None
	Returns:
	LFX_SUCCESS if the system is successfully initialized, or was already initialized.
	LFX_ERROR_NODEVS if the system is initialized, but no devices are available.
	LFX_FAILURE if initialization fails.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_Initialize();

	/*********************************************************
	Function: LFX_Release
	Description:
	Release the LightFX 2.0 system.
	This function may be called when the system is no longer needed. If this
	function is not called, release will still occur on process detach.
	PnP Note:
	An application may want to release the system and initialize it again in
	response to a device arrival notification, to account for new devices added
	while the application is running.
	Inputs: None
	Outputs: None
	Returns:
	LFX_SUCCESS
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_Release();

	/*********************************************************
	Function: LFX_Reset
	Description:
	Reset the state of the system to 'off' for any lights attached to any devices.
	Note that the change(s) to the physical light(s) do not occur immediately, rather
	only after an LFX_Update() call is made.
	To disable all lights, call LFX_Reset(), immediately followed by LFX_Update().
	Inputs:	None
	Outputs: None
	Returns:
	LFX_ERROR_NOINIT if the system is not yet initialized.
	LFX_ERROR_NODEVS if there are no devices available to reset.
	LFX_SUCCESS otherwise.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_Reset();

	/*********************************************************
	Function: LFX_Update
	Description:
	Update the entire system, submitting any state changes to hardware
	made since the last LFX_Reset()	call.
	Inputs:	None
	Outputs: None
	Returns:
	LFX_ERROR_NOINIT if the system is not yet initialized.
	LFX_ERROR_NODEVS if the system is initialized but no devices are available.
	LFX_SUCCESS otherwise.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_Update();

	/*********************************************************
	Function: LFX_UpdateDefault
	Description:
	Update the entire system, submitting any state changes made since the last LFX_Reset()
	call to the hardware, and set the appropriate flags to make the new state the
	power-on default.
	Note: Not all devices will support this functionality.
	Inputs:	None
	Outputs: None
	Returns:
	LFX_ERROR_NOINIT if the system is not yet initialized.
	LFX_ERROR_NODEVS if the system is initialized but no devices are available.
	LFX_FAILURE or LFX_SUCCESS otherwise.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_UpdateDefault();

	/*********************************************************
	Function: LFX_GetNumDevices
	Description:
	Get the number of devices attached to the LightFX system
	Inputs:	None
	Outputs: Populates a uint with the current number of attached devices.
	Returns:
	LFX_ERROR_NOINIT if the system is not yet initialized.
	LFX_ERROR_NODEVS if the system is initialized but no devices are available, leaving the param untouched.
	LFX_SUCCESS otherwise.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_GetNumDevices(unsigned int* const);

	/*********************************************************
	Function: LFX_GetDeviceDescription
	Description:
	Get the description of a device attached to the system
	Inputs:	Accepts an index to the device
	Outputs:
	Populates a character string with the device's description
	Populates a ushort with the device type (see LFXDecl.h for device types)
	Returns:
	LFX_ERROR_NOINIT if the system is not yet initialized.
	LFX_ERROR_NODEVS if the system is initialized but no devices are available.
	LFX_ERROR_BUFFSIZE if the buffer provided is too small.
	LFX_FAILURE or LFX_SUCCESS otherwise.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_GetDeviceDescription(const unsigned int, char* const, const unsigned int, unsigned char* const);

	/*********************************************************
	Function: LFX_GetNumLights
	Description:
	Get the number of lights attached to a device in the LightFX system
	Inputs:	Accepts an index to the device
	Outputs: Populates a uint with the current number of attached lights for the device index.
	Returns:
	LFX_ERROR_NOINIT if the system is not yet initialized.
	LFX_ERROR_NODEVS if the system is initialized but no devices are available at the index provided.
	LFX_ERROR_NOLIGHTS if no lights are available at the device index provided.
	LFX_FAILURE or LFX_SUCCESS otherwise.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_GetNumLights(const unsigned int, unsigned int* const);

	/*********************************************************
	Function: LFX_GetLightDescription
	Description:
	Get the description of a light attached to a device
	Inputs:	Accepts an index to the device and an index to the light
	Outputs: Populates a character string with the light's description
	Returns:
	LFX_ERROR_NOINIT if the system is not yet initialized.
	LFX_ERROR_NODEVS if the system is initialized but no devices are available at the index provided.
	LFX_ERROR_NOLIGHTS if no lights are available at the device and light index provided.
	LFX_ERROR_BUFFSIZE if the buffer provided is too small in size.
	LFX_FAILURE or LFX_SUCCESS otherwise.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_GetLightDescription(const unsigned int, const unsigned int, char* const, const unsigned int);

	/*********************************************************
	Function: LFX_GetLightLocation
	Description:
	Get the location of a light attached to a device
	Inputs:	Accepts an index to the device and an index to the light
	Outputs: Populates a LFX_POSITION structure with the light's position (see LFXDecl.h
	for more information).
	Returns:
	LFX_ERROR_NOINIT if the system is not yet initialized.
	LFX_ERROR_NODEVS if the system is initialized but no devices are available at the index provided.
	LFX_ERROR_NOLIGHTS if no lights are available at the device and light index provided.
	LFX_SUCCESS otherwise.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_GetLightLocation(const unsigned int, const unsigned int, PLFX_POSITION const);

	/*********************************************************
	Function: LFX_GetLightColor
	Description:
	Get the current color of a light attached to a device
	Important:
	This function provides the current color stored in the active state
	since the last LFX_Reset() call, it does not necessarily reflect the color of the
	physical light. To ensure that the returned value represents the state of the
	physical light, call LFX_GetLightColor immediately after a call to LFX_Update() and
	before the next call to LFX_Reset().
	Inputs:	Accepts an index to the device and an index to the light
	Outputs: Populates a LFX_COLOR structure with the light's description
	Returns:
	LFX_ERROR_NOINIT if the system is not yet initialized.
	LFX_ERROR_NODEVS if the system is initialized but no devices are available at the index provided.
	LFX_ERROR_NOLIGHTS if no lights are available at the device and light index provided.
	LFX_FAILURE or LFX_SUCCESS otherwise.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_GetLightColor(const unsigned int, const unsigned int, PLFX_COLOR const);

	/*********************************************************
	Function: LFX_SetLightColor
	Description:
	Sets the current color of a light attached to a device
	Important:
	This function changes the current color stored in the active state
	since the last LFX_Reset() call. It does NOT immediately update the physical light
	settings, until a call to LFX_Update() is made.
	Inputs:	Accepts an index to the device, an index to the light, and a new LFX_COLOR value
	Outputs: None
	Returns:
	LFX_ERROR_NOINIT if the system is not yet initialized.
	LFX_FAILURE or LFX_SUCCESS otherwise.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_SetLightColor(const unsigned int, const unsigned int, const PLFX_COLOR);

	/*********************************************************
	Function: LFX_Light
	Description:
	Sets the color of a location for any devices with lights in that
	corresponding location.
	Important:
	This function changes the current color stored in the active state
	since the last LFX_Reset() call. It does NOT immediately update the physical light
	settings, until a call to LFX_Update() is made.
	Location Mask Note:
	Location mask is a 32-bit field, where each of the first 27 bits represent
	a zone in the virtual cube representing the system (see LFXDecl.h)
	Color Packing Note:
	Color is packed into a 32-bit value, as follows:
	Bits 0-7: Blue
	Bits 8-15: Green
	Bits 16-23: Red
	Bits 24-32: Brightness
	Inputs:	Accepts a 32-bit location mask and a packed color value
	Outputs: None
	Returns:
	LFX_ERROR_NOINIT if the system is not yet initialized.
	LFX_ERROR_NOLIGHTS if no lights were found at the location mask specified.
	LFX_FAILURE if some other error occurred
	LFX_SUCCESS otherwise.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_Light(const unsigned int, const unsigned int);

	/*********************************************************
	Function: LFX_SetLightActionColor
	Description:
	Sets the primary color and an action type to a light
	Important:
	This function changes the current color and action type stored in the active state
	since the last LFX_Reset() call. It does NOT immediately update the physical light
	settings, until a call to LFX_Update() is made. If the action type is a morph, then
	the secondary color for the action is black.
	Inputs:	Accepts an index to the device, an index to the light, an action type, and a new primary LFX_COLOR value
	Outputs: None
	Returns:
	LFX_ERROR_NOINIT if the system is not yet initialized.
	LFX_FAILURE or LFX_SUCCESS otherwise.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_SetLightActionColor(const unsigned int, const unsigned int, const unsigned int, const PLFX_COLOR);

	/*********************************************************
	Function: LFX_SetLightActionColorEx
	Description:
	Sets the primary and secondary colors and an action type to a light
	Important:
	This function changes the current color and action type stored in the active state
	since the last LFX_Reset() call. It does NOT immediately update the physical light
	settings, until a call to LFX_Update() is made. If the action type is not a morph,
	then the secondary color is ignored.
	Inputs:	Accepts an index to the device, an index to the light, an action type, and two LFX_COLOR values
	Outputs: None
	Returns:
	LFX_ERROR_NOINIT if the system is not yet initialized.
	LFX_FAILURE or LFX_SUCCESS otherwise.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_SetLightActionColorEx(const unsigned int, const unsigned int, const unsigned int, const PLFX_COLOR, const PLFX_COLOR);

	/*********************************************************
	Function: LFX_ActionColor
	Description:
	Sets the primary color and an action type for any devices with lights in a location.
	Important:
	This function changes the current primary color and action type stored in the active state
	since the last LFX_Reset() call. It does NOT immediately update the physical light
	settings, until a call to LFX_Update() is made. If the action type is a morph, then
	the secondary color for the action is black.
	Location Mask Note:
	Location mask is a 32-bit field, where each of the first 27 bits represent
	a zone in the virtual cube representing the system (see LFXDecl.h)
	Color Packing Note:
	Color is packed into a 32-bit value, as follows:
	Bits 0-7: Blue
	Bits 8-15: Green
	Bits 16-23: Red
	Bits 24-32: Brightness
	Inputs:	Accepts a 32-bit location mask and a packed color value
	Outputs: None
	Returns:
	LFX_ERROR_NOINIT if the system is not yet initialized.
	LFX_ERROR_NOLIGHTS if no lights were found at the location mask specified.
	LFX_FAILURE if some other error occurred
	LFX_SUCCESS otherwise.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_ActionColor(const unsigned int, const unsigned int, const unsigned int);

	/*********************************************************
	Function: LFX_ActionColorEx
	Description:
	Sets the primary and secondary color and an action type for any devices with lights in a location.
	Important:
	This function changes the current primary and secondary color and action type stored in the active state
	since the last LFX_Reset() call. It does NOT immediately update the physical light
	settings, until a call to LFX_Update() is made. If the action type is not a morph,
	then the secondary color is ignored.
	Location Mask Note:
	Location mask is a 32-bit field, where each of the first 27 bits represent
	a zone in the virtual cube representing the system (see LFXDecl.h)
	Color Packing Note:
	Color is packed into a 32-bit value, as follows:
	Bits 0-7: Blue
	Bits 8-15: Green
	Bits 16-23: Red
	Bits 24-32: Brightness
	Inputs:	Accepts a 32-bit location mask and a packed color value
	Outputs: None
	Returns:
	LFX_ERROR_NOINIT if the system is not yet initialized.
	LFX_ERROR_NOLIGHTS if no lights were found at the location mask specified.
	LFX_FAILURE if some other error occurred
	LFX_SUCCESS otherwise.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_ActionColorEx(const unsigned int, const unsigned int, const unsigned int, const unsigned int);

	/*********************************************************
	Function: LFX_SetTiming
	Description:
	Sets the tempo for actions.
	Important:
	This function changes the current tempo or timing to be used for the
	next actions. It does NOT immediately update the physical light
	settings, until a call to LFX_Update() is made.
	Timing Note:
	Is a value between min and max tempo allowed for the main device.
	If a value lower than min or a value greather than max is entered,
	the value is readjusted to those extremes.
	Inputs:	Accepts a 32-bit timing value
	Outputs: None
	Returns:
	LFX_FAILURE if changing tempo is not supported or LFX_SUCCESS otherwise.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_SetTiming(const int);

	/*********************************************************
	Function: LFX_GetVersion
	Description:
	Get API Version
	Inputs:	Accepts the buffer and buffer size
	Outputs:
	Populates a character string with the API version
	Returns:
	LFX_ERROR_BUFFSIZE if the buffer provided is too small.
	LFX_FAILURE or LFX_SUCCESS otherwise.
	*********************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_GetVersion(char* const, const unsigned int);

#ifdef __cplusplus
} /* end extern "C" */
#endif

  // The remaining defines and typedefs are useful for explicitly linking to
  // the DLL, and can be ignored if using the static lib, or implicitely linking.
#define LFX_DLL_NAME "LightFX.dll"

  // LightFX 2.0 DLL export function names
#define LFX_DLL_INITIALIZE "LFX_Initialize"
#define LFX_DLL_RELEASE "LFX_Release"
#define LFX_DLL_RESET "LFX_Reset"
#define LFX_DLL_UPDATE "LFX_Update"
#define LFX_DLL_UPDATEDEFAULT "LFX_UpdateDefault"
#define LFX_DLL_GETNUMDEVICES "LFX_GetNumDevices"
#define LFX_DLL_GETDEVDESC "LFX_GetDeviceDescription"
#define LFX_DLL_GETNUMLIGHTS "LFX_GetNumLights"
#define LFX_DLL_GETLIGHTDESC "LFX_GetLightDescription"
#define LFX_DLL_GETLIGHTLOC "LFX_GetLightLocation"
#define LFX_DLL_GETLIGHTCOL "LFX_GetLightColor"
#define LFX_DLL_SETLIGHTCOL	"LFX_SetLightColor"
#define LFX_DLL_LIGHT "LFX_Light"
#define LFX_DLL_SETLIGHTACTIONCOLOR "LFX_SetLightActionColor"
#define LFX_DLL_SETLIGHTACTIONCOLOREX "LFX_SetLightActionColorEx"
#define LFX_DLL_ACTIONCOLOR "LFX_ActionColor"
#define LFX_DLL_ACTIONCOLOREX "LFX_ActionColorEx"
#define LFX_DLL_SETTIMING "LFX_SetTiming"
#define LFX_DLL_GETVERSION "LFX_GetVersion"

  // LightFX 2.0 function pointer declarations
typedef LFX_RESULT(*LFX2INITIALIZE)();
typedef LFX_RESULT(*LFX2RELEASE)();
typedef LFX_RESULT(*LFX2RESET)();
typedef LFX_RESULT(*LFX2UPDATE)();
typedef LFX_RESULT(*LFX2UPDATEDEFAULT)();
typedef LFX_RESULT(*LFX2GETNUMDEVICES)(unsigned int* const);
typedef LFX_RESULT(*LFX2GETDEVDESC)(const unsigned int, char* const, const unsigned int, unsigned char* const);
typedef LFX_RESULT(*LFX2GETNUMLIGHTS)(const unsigned int, unsigned int* const);
typedef LFX_RESULT(*LFX2GETLIGHTDESC)(const unsigned int, const unsigned int, char* const, const unsigned int);
typedef LFX_RESULT(*LFX2GETLIGHTLOC)(const unsigned int, const unsigned int, PLFX_POSITION const);
typedef LFX_RESULT(*LFX2GETLIGHTCOL)(const unsigned int, const unsigned int, PLFX_COLOR const);
typedef LFX_RESULT(*LFX2SETLIGHTCOL)(const unsigned int, const unsigned int, const PLFX_COLOR);
typedef LFX_RESULT(*LFX2LIGHT)(const unsigned int, const unsigned int);
typedef LFX_RESULT(*LFX2SETLIGHTACTIONCOLOR)(const unsigned int, const unsigned int, const unsigned int, const PLFX_COLOR);
typedef LFX_RESULT(*LFX2SETLIGHTACTIONCOLOREX)(const unsigned int, const unsigned int, const unsigned int, const PLFX_COLOR, const PLFX_COLOR);
typedef LFX_RESULT(*LFX2ACTIONCOLOR)(const unsigned int, const unsigned int, const unsigned int);
typedef LFX_RESULT(*LFX2ACTIONCOLOREX)(const unsigned int, const unsigned int, const unsigned int, const unsigned int);
typedef LFX_RESULT(*LFX2SETTIMING)(const int);
typedef LFX_RESULT(*LFX2GETVERSION)(char* const, const unsigned int);