/***************************************************************

LFXConfigurator.h - Defines the exports for the LightFX
Config 1.0 DLL

Purpose: Provide library exports for communicating with
the LightFX Config 1.0 API

Copyright (c) 2007 Dell, Inc. All rights reserved.

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

***************************************************************/

#pragma once

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

// Return values
#define LFX_RESULT unsigned int
#define LFX_SUCCESS				0		// Success
#define LFX_FAILURE				1		// Generic failure
#define LFX_ERROR_NOINIT		2		// System not initialized yet
#define LFX_ERROR_NODEVS		3		// No devices available
#define LFX_ERROR_NOLIGHTS		4		// No lights available
#define LFX_ERROR_BUFFSIZE		5		// Buffer size too small

//event position
#define LFX_EVENTPOSITION unsigned int
#define LFX_FIRSTEVENT			0		// First event
#define LFX_NEXTEVENT			1		// Next event

//event position
#define LFX_APPTYPE unsigned int
#define LFX_GAME				0		// The application is a game
#define LFX_GENERALUSEAPP		1		// It is a general use application

#ifdef __cplusplus
extern "C" {
#endif

	// LightFX Config 1.0 DLL export function declarations

	/***************************************************************
	Function: LFX_CONFIGURATOR_Initialize
	Description:
	Initializes the LightFX Config 1.0 system.
	This function must be called prior to any other library calls being made. If this
	function is not called, the system will not be initialized and the other functions
	will return LFX_ERROR_NOINIT.
	Inputs: None
	Outputs: None
	Returns:
	LFX_SUCCESS if the system is successfully initialized, or was already initialized.
	LFX_FAILURE if initialization fails.
	***************************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_CONFIGURATOR_Initialize();

	/***************************************************************
	Function: LFX_CONFIGURATOR_Release
	Description:
	Release the LightFX Config 1.0 system.
	This function may be called when the system is no longer needed. If this
	function is not called, release will still occur on process detach.
	Inputs: None
	Outputs: None
	Returns:
	LFX_SUCCESS
	***************************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_CONFIGURATOR_Release();

	/***************************************************************
	Function: LFX_CONFIGURATOR_RegisterConfigurationFile
	Description:
	Register an app in order to be read by the AlienFX Editor.
	Inputs: Application name and full path to the config file
	Outputs: None
	Returns:
	LFX_SUCCESS if the application is successfully registered.
	LFX_FAILURE if the registration fails.
	LFX_ERROR_NOINIT if the LFX_CONFIGURATOR_Initialize
	function was not called before
	***************************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_CONFIGURATOR_RegisterConfigurationFile(char* const appFXName, char* const configurationFileFullPath, const LFX_APPTYPE appType);

	/***************************************************************
	Function: LFX_CONFIGURATOR_UnregisterConfigurationFile
	Description:
	Unregister an app. Once this function is called, the app
	will not appear in AlienFX Editor and cannot be configured
	Inputs: Application name
	Outputs: None
	Returns:
	LFX_SUCCESS if the application is successfully unregistered.
	LFX_FAILURE if the registration fails.
	LFX_ERROR_NOINIT if the LFX_CONFIGURATOR_Initialize
	function was not called before
	***************************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_CONFIGURATOR_UnregisterConfigurationFile(char* const appFXName);

	/***************************************************************
	Function: LFX_CONFIGURATOR_GetUserConfigurationFilePath
	Description:
	Get the user's configuration file for an application
	Inputs: Application name and the size of the buffer where the
	user's configuration filename is going to be returned
	Outputs:
	Populates a character string with the user's configuration file
	Returns:
	LFX_SUCCESS if the user configuration file is returned.
	LFX_FAILURE if the user's configuration file cannot be returned.
	LFX_ERROR_BUFFSIZE if the buffer provided is too small.
	LFX_ERROR_NOINIT if the LFX_CONFIGURATOR_Initialize
	function was not called before
	***************************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_CONFIGURATOR_GetUserConfigurationFilePath(char* const appFXName, char* const userConfigurationFilename, const unsigned int userConfigurationFilenameSize);

	/***************************************************************
	Function: LFX_CONFIGURATOR_GetConfigurationEvent
	Description:
	Get the color configuration for an event
	Inputs: Application name and event id
	Outputs:
	Populates a color array and the amount of colors for that event
	Returns:
	LFX_SUCCESS if the colors for the event is returned.
	LFX_FAILURE if the colors for the event cannot be returned.
	LFX_ERROR_NOINIT if the LFX_CONFIGURATOR_Initialize
	function was not called before
	***************************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_CONFIGURATOR_GetConfigurationEvent(char* const appFXName, const unsigned int eventID, unsigned int* &colors, unsigned int &colorCount);

	/***************************************************************
	Function: LFX_CONFIGURATOR_GetConfigurationEventAt
	Description:
	Get the color configuration for an event
	Inputs: Application name and event position
	Outputs:
	Returns the event in that position and data related
	Populates a character string with the event name
	Populates a color array and the amount of colors for that event
	Returns:
	LFX_SUCCESS if there is an event to be returned.
	LFX_FAILURE if the event cannot be returned.
	LFX_ERROR_BUFFSIZE if the buffer provided is too small.
	LFX_ERROR_NOINIT if the LFX_CONFIGURATOR_Initialize
	function was not called before
	***************************************************************/
	FN_DECLSPEC LFX_RESULT STDCALL LFX_CONFIGURATOR_GetConfigurationEventAt(char* const appFXName, unsigned int position,
		int &eventID, char* const eventName, const unsigned int eventNameSize, unsigned int* &colors, unsigned int &colorCount);

#ifdef __cplusplus
} /* end extern "C" */
#endif

  // The remaining defines and typedefs are useful for explicitly linking to
  // the DLL, and can be ignored if using the static lib, or implicitely linking.
#define LFX_CONFIGURATOR_DLL_NAME "LightFXConfigurator64.dll"

  // LightFX Config 1.0 DLL export function names
#define LFX_CONFIGURATOR_DLL_INITIALIZE							"LFX_CONFIGURATOR_Initialize"
#define LFX_CONFIGURATOR_DLL_RELEASE							"LFX_CONFIGURATOR_Release"
#define LFX_CONFIGURATOR_DLL_REGISTERCONFIGURATIONFILE			"LFX_CONFIGURATOR_RegisterConfigurationFile"
#define LFX_CONFIGURATOR_DLL_UNREGISTERCONFIGURATIONFILE		"LFX_CONFIGURATOR_UnregisterConfigurationFile"
#define LFX_CONFIGURATOR_DLL_GETUSERCONFIGURATIONFILEPATH		"LFX_CONFIGURATOR_GetUserConfigurationFilePath"
#define LFX_CONFIGURATOR_DLL_GETCONFIGURATIONEVENT				"LFX_CONFIGURATOR_GetConfigurationEvent"
#define LFX_CONFIGURATOR_DLL_GETCONFIGURATIONEVENTFROMPOSITION	"LFX_CONFIGURATOR_GetConfigurationEventAt"


  // LightFX Config 1.0 function pointer declarations
typedef LFX_RESULT(*LFXCONFIGURATORINITIALIZE)();
typedef LFX_RESULT(*LFXCONFIGURATORRELEASE)();
typedef LFX_RESULT(*LFXCONFIGURATORREGISTERCONFIGURATIONFILE)(char* const, char* const, const LFX_APPTYPE);
typedef LFX_RESULT(*LFXCONFIGURATORUNREGISTERCONFIGURATIONFILE)(char* const);
typedef LFX_RESULT(*LFXCONFIGURATORGETUSERCONFIGURATIONFILEPATH)(char* const, char* const, const unsigned int);
typedef LFX_RESULT(*LFXCONFIGURATORGETCONFIGURATIONEVENT)(char* const, const unsigned int, unsigned int*&, unsigned int&);
typedef LFX_RESULT(*LFXCONFIGURATORGETCONFIGURATIONEVENTFROMPOSITION)(char* const, unsigned int, int&, char* const, const unsigned int, unsigned int*&, unsigned int&);
