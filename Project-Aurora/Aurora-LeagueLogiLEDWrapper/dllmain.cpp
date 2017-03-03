// dllmain.cpp : Defines the entry point for the DLL application.
#include "dllmain.h"
#include "LogitechLEDLib.h"
#include "stdafx.h"
#include <stdio.h>
#include <string>
#include <iomanip>
#include <sstream>
#include <windows.h>

#define PIPE_NAME L"\\\\.\\pipe\\Aurora\\server" 

HANDLE hPipe;
static bool isInitialized = false;

static unsigned char current_bitmap[LOGI_LED_BITMAP_SIZE];

static std::string program_name;

BOOL WINAPI DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
	)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}


bool WriteToPipe(unsigned char bitmap[], std::string command_cargo)
{
	if (!isInitialized)
		return false;

	//Create JSON
	std::stringstream ss;

	ss << '{';
	ss << "\"provider\": {\"name\": \"" << program_name << "\", \"appid\": 0},";
	ss << command_cargo << ',';
	ss << "\"bitmap\": [";
	for (int bitm_pos = 0; bitm_pos < LOGI_LED_BITMAP_SIZE; bitm_pos++)
	{
		ss << (short)bitmap[bitm_pos];

		if (bitm_pos + 1 < LOGI_LED_BITMAP_SIZE)
			ss << ',';
	}
	ss << "]";
	ss << '}';

	ss << "\r\n";

	if (INVALID_HANDLE_VALUE == hPipe)
	{
		//Try to gestore handle
		//Connect to the server pipe using CreateFile()
		hPipe = CreateFile(
			PIPE_NAME,   // pipe name 
			GENERIC_READ |  // read and write access 
			GENERIC_WRITE,
			0,              // no sharing 
			NULL,           // default security attributes
			OPEN_EXISTING,  // opens existing pipe 
			0,              // default attributes 
			NULL);          // no template file 

		if (INVALID_HANDLE_VALUE == hPipe)
		{
			return false;
		}
	}

	DWORD cbBytes;

	BOOL bResult = WriteFile(
		hPipe,                // handle to pipe 
		ss.str().c_str(),             // buffer to write from 
		strlen(ss.str().c_str()),   // number of bytes to write, include the NULL
		&cbBytes,             // number of bytes written 
		NULL);                // not overlapped I/O 

	if ((!bResult) || (strlen(ss.str().c_str()) != cbBytes))
	{
		CloseHandle(hPipe);
		return false;
	}
	else
	{
		return true;
	}

	return false;
}

bool LogiLedInit()
{
	if (!isInitialized)
	{
		//Get Application name
		CHAR pBuf[MAX_PATH];
		int bytes = GetModuleFileNameA(NULL, pBuf, MAX_PATH);
		std::string filepath = pBuf;

		int fn_beginning = 0;
		for (int chr_pos = strlen(pBuf) - 1; chr_pos > -1; chr_pos--)
		{
			if (pBuf[chr_pos] == '\\')
			{
				fn_beginning = chr_pos + 1;
				break;
			}
		}

		program_name = filepath.substr(fn_beginning);

		//Connect to the server pipe using CreateFile()
		hPipe = CreateFile(
			PIPE_NAME,   // pipe name 
			GENERIC_READ |  // read and write access 
			GENERIC_WRITE,
			0,              // no sharing 
			NULL,           // default security attributes
			OPEN_EXISTING,  // opens existing pipe 
			0,              // default attributes 
			NULL);          // no template file 

		if (INVALID_HANDLE_VALUE == hPipe)
		{
			isInitialized = false;
			return false;
		}
	}

	isInitialized = true;
	return true;
}

bool LogiLedSaveCurrentLighting(int deviceType)
{
	return isInitialized;
}

int LogiLedGetCurrentBrightnessPercentage(int deviceType)
{
	if (deviceType == LOGITECH_LED_KEYBOARD || deviceType == LOGITECH_LED_ALL)
		return ((int)current_bitmap[3] / (float)255) * 100;
	else
		return 100;
}

bool LogiLedSetLighting(int deviceType, int redPercentage, int greenPercentage, int bluePercentage, int brightnessPercentage)
{
	unsigned char redValue = (unsigned char)((redPercentage / 100.0f) * 255);
	unsigned char greenValue = (unsigned char)((greenPercentage / 100.0f) * 255);
	unsigned char blueValue = (unsigned char)((bluePercentage / 100.0f) * 255);
	unsigned char brightnessValue = (unsigned char)((brightnessPercentage / 100.0f) * 255);

	if (isInitialized && (deviceType == LOGITECH_LED_KEYBOARD || deviceType == LOGITECH_LED_ALL))
	{
		for (int colorset = 0; colorset < LOGI_LED_BITMAP_SIZE; colorset += 4)
		{
			current_bitmap[colorset] = blueValue;
			current_bitmap[colorset + 1] = greenValue;
			current_bitmap[colorset + 2] = redValue;
			current_bitmap[colorset + 3] = (char)255;
		}

		std::stringstream ss;
		ss << "\"command\": " << "\"SetLighting\"" << ',';
		ss << "\"command_data\": {";

		ss << '}';

		return WriteToPipe(current_bitmap, ss.str());
	}

	return isInitialized;
}

bool LogiLedRestoreLighting()
{
	return isInitialized;
}

bool LogiLedFlashLighting(int deviceType, int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval)
{
	if (deviceType == LOGITECH_LED_KEYBOARD || deviceType == LOGITECH_LED_ALL)
	{
		unsigned char redValue = (unsigned char)((redPercentage / 100.0f) * 255);
		unsigned char greenValue = (unsigned char)((greenPercentage / 100.0f) * 255);
		unsigned char blueValue = (unsigned char)((bluePercentage / 100.0f) * 255);

		std::stringstream ss;
		ss << "\"command\": " << "\"FlashLighting\"" << ',';
		ss << "\"command_data\": {";

		ss << "\"red_start\": " << (int)redValue << ',';
		ss << "\"green_start\": " << (int)greenValue << ',';
		ss << "\"blue_start\": " << (int)blueValue << ',';
		ss << "\"duration\": " << milliSecondsDuration << ',';
		ss << "\"interval\": " << milliSecondsInterval;

		ss << '}';

		return WriteToPipe(current_bitmap, ss.str());
	}

	return isInitialized;
}

bool LogiLedPulseLighting(int deviceType, int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval)
{
	if (deviceType == LOGITECH_LED_KEYBOARD || deviceType == LOGITECH_LED_ALL)
	{
		unsigned char redValue = (unsigned char)((redPercentage / 100.0f) * 255);
		unsigned char greenValue = (unsigned char)((greenPercentage / 100.0f) * 255);
		unsigned char blueValue = (unsigned char)((bluePercentage / 100.0f) * 255);

		std::stringstream ss;
		ss << "\"command\": " << "\"PulseLighting\"" << ',';
		ss << "\"command_data\": {";

		ss << "\"red_start\": " << (int)redValue << ',';
		ss << "\"green_start\": " << (int)greenValue << ',';
		ss << "\"blue_start\": " << (int)blueValue << ',';
		ss << "\"duration\": " << milliSecondsDuration << ',';
		ss << "\"interval\": " << milliSecondsInterval;

		ss << '}';

		return WriteToPipe(current_bitmap, ss.str());
	}

	return isInitialized;
}

void LogiLedShutdown()
{
	if (isInitialized)
	{
		if (hPipe != INVALID_HANDLE_VALUE)
			CloseHandle(hPipe);
		isInitialized = false;
	}
}