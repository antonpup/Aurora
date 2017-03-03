// dllmain.cpp : Defines the entry point for the DLL application.
#include "LFX2.h"
#include "stdafx.h"
#include <string>

//#define DEVICE_LIGHTS_NUM 5

#define PIPE_NAME L"\\\\.\\pipe\\Aurora\\server" 

HANDLE hPipe;
static bool isInitialized = false;

static LFX_COLOR current_bg = { (char)0, (char)0, (char)0, (char)0 };
static int action_timing = 200;

static std::string program_name;
static std::string device_name = "Aurora";
static int device_lights_num = 1; //DEVICE_LIGHTS_NUM;
static std::string device_lights_name = "Northern Light";
//static PLFX_COLOR device_lights[DEVICE_LIGHTS_NUM];
static bool isUpdated;


BOOL APIENTRY DllMain(HMODULE hModule,
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

bool WriteToPipe(const std::string command_cargo)
{
	if (!isInitialized)
		return false;

	//Create JSON
	std::string contents = "";

	contents += '{';
	contents += "\"provider\": {\"name\": \"" + program_name + "\", \"appid\": 0},";
	contents += command_cargo;
	contents += '}';
	contents += "\r\n";

	if (INVALID_HANDLE_VALUE == hPipe)
	{
		//Try to restore handle
		//Connect to the server pipe using CreateFile()
		hPipe = CreateFile(
			PIPE_NAME,   // pipe name 
			GENERIC_WRITE,  // write access 
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

	const char* c_contents = contents.c_str();
	int c_cotents_len = strlen(c_contents);

	BOOL bResult = WriteFile(
		hPipe,                // handle to pipe 
		c_contents,             // buffer to write from 
		c_cotents_len,   // number of bytes to write, include the NULL
		&cbBytes,             // number of bytes written 
		NULL);                // not overlapped I/O 

	if ((!bResult) || c_cotents_len != cbBytes)
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


#ifdef __cplusplus
extern "C" {
#endif

	FN_DECLSPEC LFX_RESULT STDCALL LFX_Initialize()
	{
		isUpdated = false;

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
				GENERIC_WRITE,  // write access 
				0,              // no sharing 
				NULL,           // default security attributes
				OPEN_EXISTING,  // opens existing pipe 
				0,              // default attributes 
				NULL);          // no template file 

			if (INVALID_HANDLE_VALUE == hPipe)
			{
				isInitialized = false;
				return LFX_FAILURE;
			}
		}

		isInitialized = true;
		return LFX_SUCCESS;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_Release()
	{
		if (isInitialized && hPipe != INVALID_HANDLE_VALUE)
		{
			CloseHandle(hPipe);
		}
		
		return isInitialized ? LFX_SUCCESS : LFX_FAILURE;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_Reset()
	{
		if (isInitialized)
		{
			current_bg = { (char)0, (char)0, (char)0, (char)0 };
			isUpdated = true;

			std::string contents = "";

			contents += "\"command\": \"LFX_Reset\",";
			contents += "\"command_data\": {";
			contents += '}';

			WriteToPipe(contents);
		}

		return isInitialized ? LFX_SUCCESS : LFX_FAILURE;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_Update()
	{
		if (isInitialized && isUpdated)
		{
			unsigned char redValue = (unsigned char)((int)(current_bg.red) * ((int)(current_bg.brightness) / 255.0f));
			unsigned char greenValue = (unsigned char)((int)(current_bg.green) * ((int)(current_bg.brightness) / 255.0f));
			unsigned char blueValue = (unsigned char)((int)(current_bg.blue) * ((int)(current_bg.brightness) / 255.0f));

			std::string contents = "";

			contents += "\"command\": \"LFX_Update\",";
			contents += "\"command_data\": {";
			contents += "\"red_start\": " + std::to_string((int)redValue) + ',';
			contents += "\"green_start\": " + std::to_string((int)greenValue) + ',';
			contents += "\"blue_start\": " + std::to_string((int)blueValue);
			contents += '}';

			WriteToPipe(contents);

			isUpdated = false;
		}

		return isInitialized ? LFX_SUCCESS : LFX_FAILURE;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_UpdateDefault()
	{
		// Not supported
		return LFX_FAILURE;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_GetNumDevices(unsigned int* const numDevices)
	{
		if (isInitialized)
		{
			std::string contents = "";

			contents += "\"command\": \"LFX_GetNumDevices\",";
			contents += "\"command_data\": {";
			contents += '}';

			WriteToPipe(contents);

			*numDevices = 1;
		}
		else
			*numDevices = 0;

		return isInitialized ? LFX_SUCCESS : LFX_FAILURE;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_GetDeviceDescription(const unsigned int devIndex, char* const devDesc, const unsigned int devDescSize, unsigned char* const devType)
	{
		if (isInitialized)
		{
			std::string contents = "";

			contents += "\"command\": \"LFX_GetDeviceDescription\",";
			contents += "\"command_data\": {";
			contents += '}';

			WriteToPipe(contents);

			if (devIndex >= 1)
				return LFX_ERROR_NODEVS;

			if (device_name.length() > devDescSize)
				return LFX_ERROR_BUFFSIZE;

			sprintf_s(devDesc, devDescSize, device_name.c_str());

			*devType = 6; //Keyboard
		}

		return isInitialized ? LFX_SUCCESS : LFX_FAILURE;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_GetNumLights(const unsigned int devIndex, unsigned int* const numLights)
	{
		if (isInitialized)
		{
			std::string contents = "";

			contents += "\"command\": \"LFX_GetNumLights\",";
			contents += "\"command_data\": {";
			contents += '}';

			WriteToPipe(contents);

			if (devIndex >= 1)
				return LFX_ERROR_NODEVS;

			*numLights = device_lights_num;
		}


		return isInitialized ? LFX_SUCCESS : LFX_FAILURE;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_GetLightDescription(const unsigned int devIndex, const unsigned int lightIndex, char* const lightDesc, const unsigned int lightDescSize)
	{
		if (isInitialized)
		{
			std::string contents = "";

			contents += "\"command\": \"LFX_GetLightDescription\",";
			contents += "\"command_data\": {";
			contents += '}';

			WriteToPipe(contents);

			if (devIndex >= 1)
				return LFX_ERROR_NODEVS;

			if (lightIndex >= 1)
				return LFX_ERROR_NOLIGHTS;

			if (device_lights_name.length() > lightDescSize)
				return LFX_ERROR_BUFFSIZE;

			sprintf_s(lightDesc, lightDescSize, device_lights_name.c_str());
		}

		return isInitialized ? LFX_SUCCESS : LFX_FAILURE;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_GetLightLocation(const unsigned int devIndex, const unsigned int lightIndex, PLFX_POSITION const lightLoc)
	{
		if (isInitialized)
		{
			std::string contents = "";

			contents += "\"command\": \"LFX_GetLightLocation\",";
			contents += "\"command_data\": {";
			contents += '}';

			WriteToPipe(contents);

			if (devIndex >= 1)
				return LFX_ERROR_NODEVS;

			if (lightIndex >= 1)
				return LFX_ERROR_NOLIGHTS;

			_LFX_POSITION newpos = { 0, (char)lightIndex , 0 };

			*lightLoc = newpos;
		}

		return isInitialized ? LFX_SUCCESS : LFX_FAILURE;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_GetLightColor(const unsigned int devIndex, const unsigned int lightIndex, PLFX_COLOR const lightCol)
	{
		if (isInitialized)
		{
			std::string contents = "";

			contents += "\"command\": \"LFX_GetLightColor\",";
			contents += "\"command_data\": {";
			contents += '}';

			WriteToPipe(contents);

			if (devIndex >= 1)
				return LFX_ERROR_NODEVS;

			if (lightIndex >= 1)
				return LFX_ERROR_NOLIGHTS;

			*lightCol = current_bg;
		}

		return isInitialized ? LFX_SUCCESS : LFX_FAILURE;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_SetLightColor(const unsigned int devIndex, const unsigned int lightIndex, const PLFX_COLOR lightCol)
	{
		if (isInitialized)
		{
			std::string contents = "";

			contents += "\"command\": \"LFX_SetLightColor\",";
			contents += "\"command_data\": {";
			contents += '}';

			WriteToPipe(contents);

			if (devIndex >= 1)
				return LFX_ERROR_NODEVS;

			if (lightIndex >= 1)
				return LFX_ERROR_NOLIGHTS;

			current_bg = *lightCol;

			isUpdated = true;
		}

		return isInitialized ? LFX_SUCCESS : LFX_FAILURE;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_Light(const unsigned int locationMask, const unsigned int lightCol)
	{
		//Not supported
		if (isInitialized)
		{
			LFX_COLOR lfx_color;
			lfx_color.brightness = (lightCol >> 24) & 0xFF;
			lfx_color.red = (lightCol >> 16) & 0xFF;
			lfx_color.green = (lightCol >> 8) & 0xFF;
			lfx_color.blue = lightCol & 0xFF;

			unsigned char redValue = (unsigned char)((int)(lfx_color.red) * ((int)(lfx_color.brightness) / 255.0f));
			unsigned char greenValue = (unsigned char)((int)(lfx_color.green) * ((int)(lfx_color.brightness) / 255.0f));
			unsigned char blueValue = (unsigned char)((int)(lfx_color.blue) * ((int)(lfx_color.brightness) / 255.0f));

			std::string contents = "";

			contents += "\"command\": \"LFX_Light\",";
			contents += "\"command_data\": {";

			contents += "\"locationMask\": \"" + std::to_string(locationMask) + "\",";

			contents += "\"red_start\": " + std::to_string((int)redValue) + ',';
			contents += "\"green_start\": " + std::to_string((int)greenValue) + ',';
			contents += "\"blue_start\": " + std::to_string((int)blueValue);
			contents += '}';

			WriteToPipe(contents);

			current_bg = lfx_color;
			isUpdated = true;
		}

		return isInitialized ? LFX_SUCCESS : LFX_FAILURE;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_SetLightActionColor(const unsigned int devIndex, const unsigned int lightIndex, const unsigned int actionType, const PLFX_COLOR primaryCol)
	{
		if (!isInitialized)
			return LFX_ERROR_NOINIT;

		if (devIndex >= 1)
			return LFX_ERROR_NODEVS;

		if (lightIndex >= device_lights_num)
			return LFX_ERROR_NOLIGHTS;

		if (isInitialized)
		{
			//Primary Color
			LFX_COLOR lfx_color_primary;
			lfx_color_primary.brightness = (*primaryCol).brightness;
			lfx_color_primary.red = (*primaryCol).red;
			lfx_color_primary.green = (*primaryCol).green;
			lfx_color_primary.blue = (*primaryCol).blue;

			unsigned char redValue = (unsigned char)((int)(lfx_color_primary.red) * ((int)(lfx_color_primary.brightness) / 255.0f));
			unsigned char greenValue = (unsigned char)((int)(lfx_color_primary.green) * ((int)(lfx_color_primary.brightness) / 255.0f));
			unsigned char blueValue = (unsigned char)((int)(lfx_color_primary.blue) * ((int)(lfx_color_primary.brightness) / 255.0f));

			std::string contents = "";

			contents += "\"command\": \"LFX_SetLightActionColor\",";
			contents += "\"command_data\": {";

			contents += "\"red_start\": " + std::to_string((int)redValue) + ',';
			contents += "\"green_start\": " + std::to_string((int)greenValue) + ',';
			contents += "\"blue_start\": " + std::to_string((int)blueValue) + ',';
			contents += "\"duration\": " + std::to_string((int)action_timing) + ',';

			switch (actionType) {
			case LFX_ACTION_MORPH:
				contents += "\"effect_type\": \"LFX_ACTION_MORPH\"";
				break;
			case LFX_ACTION_PULSE:
				contents += "\"effect_type\": \"LFX_ACTION_PULSE\"";
				break;
			case LFX_ACTION_COLOR:
				contents += "\"effect_type\": \"LFX_ACTION_COLOR\"";
				break;
			default:
				contents += "\"effect_type\": \"None\"";
				break;
			}

			contents += '}';

			WriteToPipe(contents);
		}

		return isInitialized ? LFX_SUCCESS : LFX_FAILURE;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_SetLightActionColorEx(const unsigned int devIndex, const unsigned int lightIndex, const unsigned int actionType, const PLFX_COLOR primaryCol, const PLFX_COLOR secondaryCol)
	{
		if (!isInitialized)
			return LFX_ERROR_NOINIT;

		if (devIndex >= 1)
			return LFX_ERROR_NODEVS;

		if (lightIndex >= device_lights_num)
			return LFX_ERROR_NOLIGHTS;

		if (isInitialized)
		{
			//Primary Color
			LFX_COLOR lfx_color_primary;
			lfx_color_primary.brightness = (*primaryCol).brightness;
			lfx_color_primary.red = (*primaryCol).red;
			lfx_color_primary.green = (*primaryCol).green;
			lfx_color_primary.blue = (*primaryCol).blue;

			unsigned char redValue = (unsigned char)((int)(lfx_color_primary.red) * ((int)(lfx_color_primary.brightness) / 255.0f));
			unsigned char greenValue = (unsigned char)((int)(lfx_color_primary.green) * ((int)(lfx_color_primary.brightness) / 255.0f));
			unsigned char blueValue = (unsigned char)((int)(lfx_color_primary.blue) * ((int)(lfx_color_primary.brightness) / 255.0f));

			//Secondary Color
			LFX_COLOR lfx_color_secondary;
			lfx_color_secondary.brightness = (*secondaryCol).brightness;
			lfx_color_secondary.red = (*secondaryCol).red;
			lfx_color_secondary.green = (*secondaryCol).green;
			lfx_color_secondary.blue = (*secondaryCol).blue;

			unsigned char redValue_end = (unsigned char)((int)(lfx_color_secondary.red) * ((int)(lfx_color_secondary.brightness) / 255.0f));
			unsigned char greenValue_end = (unsigned char)((int)(lfx_color_secondary.green) * ((int)(lfx_color_secondary.brightness) / 255.0f));
			unsigned char blueValue_end = (unsigned char)((int)(lfx_color_secondary.blue) * ((int)(lfx_color_secondary.brightness) / 255.0f));

			std::string contents = "";

			contents += "\"command\": \"LFX_SetLightActionColorEx\",";
			contents += "\"command_data\": {";

			contents += "\"red_start\": " + std::to_string((int)redValue) + ',';
			contents += "\"green_start\": " + std::to_string((int)greenValue) + ',';
			contents += "\"blue_start\": " + std::to_string((int)blueValue) + ',';
			contents += "\"red_end\": " + std::to_string((int)redValue_end) + ',';
			contents += "\"green_end\": " + std::to_string((int)greenValue_end) + ',';
			contents += "\"blue_end\": " + std::to_string((int)blueValue_end) + ',';
			contents += "\"duration\": " + std::to_string((int)action_timing) + ',';

			switch (actionType) {
			case LFX_ACTION_MORPH:
				contents += "\"effect_type\": \"LFX_ACTION_MORPH\"";
				break;
			case LFX_ACTION_PULSE:
				contents += "\"effect_type\": \"LFX_ACTION_PULSE\"";
				break;
			case LFX_ACTION_COLOR:
				contents += "\"effect_type\": \"LFX_ACTION_COLOR\"";
				break;
			default:
				contents += "\"effect_type\": \"None\"";
				break;
			}

			contents += '}';

			WriteToPipe(contents);
		}

		return isInitialized ? LFX_SUCCESS : LFX_FAILURE;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_ActionColor(const unsigned int locationMask, const unsigned int actionType, const unsigned int primaryCol)
	{
		if (!isInitialized)
			return LFX_ERROR_NOINIT;

		if (isInitialized)
		{
			//Primary Color
			LFX_COLOR lfx_color_primary;
			lfx_color_primary.brightness = (primaryCol >> 24) & 0xFF;
			lfx_color_primary.red = (primaryCol >> 16) & 0xFF;
			lfx_color_primary.green = (primaryCol >> 8) & 0xFF;
			lfx_color_primary.blue = primaryCol & 0xFF;

			unsigned char redValue = (unsigned char)((int)(lfx_color_primary.red) * ((int)(lfx_color_primary.brightness) / 255.0f));
			unsigned char greenValue = (unsigned char)((int)(lfx_color_primary.green) * ((int)(lfx_color_primary.brightness) / 255.0f));
			unsigned char blueValue = (unsigned char)((int)(lfx_color_primary.blue) * ((int)(lfx_color_primary.brightness) / 255.0f));

			std::string contents = "";

			contents += "\"command\": \"LFX_ActionColor\",";
			contents += "\"command_data\": {";

			contents += "\"red_start\": " + std::to_string((int)redValue) + ',';
			contents += "\"green_start\": " + std::to_string((int)greenValue) + ',';
			contents += "\"blue_start\": " + std::to_string((int)blueValue) + ',';
			contents += "\"duration\": " + std::to_string((int)action_timing) + ',';

			switch (actionType) {
			case LFX_ACTION_MORPH:
				contents += "\"effect_type\": \"LFX_ACTION_MORPH\"";
				break;
			case LFX_ACTION_PULSE:
				contents += "\"effect_type\": \"LFX_ACTION_PULSE\"";
				break;
			case LFX_ACTION_COLOR:
				contents += "\"effect_type\": \"LFX_ACTION_COLOR\"";
				break;
			default:
				contents += "\"effect_type\": \"None\"";
				break;
			}

			contents += '}';

			WriteToPipe(contents);
		}

		return isInitialized ? LFX_SUCCESS : LFX_FAILURE;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_ActionColorEx(const unsigned int locationMask, const unsigned int actionType, const unsigned int primaryCol, const unsigned int secondaryCol)
	{
		if (!isInitialized)
			return LFX_ERROR_NOINIT;

		if (isInitialized)
		{
			//Primary Color
			LFX_COLOR lfx_color_primary;
			lfx_color_primary.brightness = (primaryCol >> 24) & 0xFF;
			lfx_color_primary.red = (primaryCol >> 16) & 0xFF;
			lfx_color_primary.green = (primaryCol >> 8) & 0xFF;
			lfx_color_primary.blue = primaryCol & 0xFF;

			unsigned char redValue = (unsigned char)((int)(lfx_color_primary.red) * ((int)(lfx_color_primary.brightness) / 255.0f));
			unsigned char greenValue = (unsigned char)((int)(lfx_color_primary.green) * ((int)(lfx_color_primary.brightness) / 255.0f));
			unsigned char blueValue = (unsigned char)((int)(lfx_color_primary.blue) * ((int)(lfx_color_primary.brightness) / 255.0f));

			//Secondary Color
			LFX_COLOR lfx_color_secondary;
			lfx_color_secondary.brightness = (secondaryCol >> 24) & 0xFF;
			lfx_color_secondary.red = (secondaryCol >> 16) & 0xFF;
			lfx_color_secondary.green = (secondaryCol >> 8) & 0xFF;
			lfx_color_secondary.blue = secondaryCol & 0xFF;

			unsigned char redValue_end = (unsigned char)((int)(lfx_color_secondary.red) * ((int)(lfx_color_secondary.brightness) / 255.0f));
			unsigned char greenValue_end = (unsigned char)((int)(lfx_color_secondary.green) * ((int)(lfx_color_secondary.brightness) / 255.0f));
			unsigned char blueValue_end = (unsigned char)((int)(lfx_color_secondary.blue) * ((int)(lfx_color_secondary.brightness) / 255.0f));

			std::string contents = "";

			contents += "\"command\": \"LFX_ActionColorEx\",";
			contents += "\"command_data\": {";

			contents += "\"red_start\": " + std::to_string((int)redValue) + ',';
			contents += "\"green_start\": " + std::to_string((int)greenValue) + ',';
			contents += "\"blue_start\": " + std::to_string((int)blueValue) + ',';
			contents += "\"red_end\": " + std::to_string((int)redValue_end) + ',';
			contents += "\"green_end\": " + std::to_string((int)greenValue_end) + ',';
			contents += "\"blue_end\": " + std::to_string((int)blueValue_end) + ',';
			contents += "\"duration\": " + std::to_string((int)action_timing) + ',';

			switch (actionType) {
			case LFX_ACTION_MORPH:
				contents += "\"effect_type\": \"LFX_ACTION_MORPH\"";
				break;
			case LFX_ACTION_PULSE:
				contents += "\"effect_type\": \"LFX_ACTION_PULSE\"";
				break;
			case LFX_ACTION_COLOR:
				contents += "\"effect_type\": \"LFX_ACTION_COLOR\"";
				break;
			default:
				contents += "\"effect_type\": \"None\"";
				break;
			}

			contents += '}';

			WriteToPipe(contents);
		}

		return isInitialized ? LFX_SUCCESS : LFX_FAILURE;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_SetTiming(const int newTiming)
	{
		action_timing = newTiming;
		return isInitialized ? LFX_SUCCESS : LFX_FAILURE;
	}

	FN_DECLSPEC LFX_RESULT STDCALL LFX_GetVersion(char* const version, const unsigned int versionSize)
	{
		sprintf_s(version, versionSize, "2.2.0.0");
		return isInitialized ? LFX_SUCCESS : LFX_FAILURE;
	}

#ifdef __cplusplus
}
#endif

