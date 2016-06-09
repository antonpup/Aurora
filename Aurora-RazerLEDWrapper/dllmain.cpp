// dllmain.cpp : Defines the entry point for the DLL application.
#include "RzChromaSDKDefines.h"
#include "RzChromaSDKTypes.h"
#include "RzErrors.h"
#include "stdafx.h"
#include <stdio.h>
#include <string>
#include <iomanip>
#include <sstream>
#include <windows.h>

#define PIPE_NAME L"\\\\.\\pipe\\Aurora\\server" 

#define LOGI_LED_BITMAP_WIDTH 21
#define LOGI_LED_BITMAP_HEIGHT 6
#define LOGI_LED_BITMAP_BYTES_PER_KEY 4

#define LOGI_LED_BITMAP_SIZE (LOGI_LED_BITMAP_WIDTH*LOGI_LED_BITMAP_HEIGHT*LOGI_LED_BITMAP_BYTES_PER_KEY)

typedef enum
{
	PERIPHERAL = -3,
	LOGO = -2,
	UNKNOWN = -1,
	BITLOC_ESC = 0,
	BITLOC_F1 = 4,
	BITLOC_F2 = 8,
	BITLOC_F3 = 12,
	BITLOC_F4 = 16,
	BITLOC_F5 = 20,
	BITLOC_F6 = 24,
	BITLOC_F7 = 28,
	BITLOC_F8 = 32,
	BITLOC_F9 = 36,
	BITLOC_F10 = 40,
	BITLOC_F11 = 44,
	BITLOC_F12 = 48,
	BITLOC_PRINT_SCREEN = 52,
	BITLOC_SCROLL_LOCK = 56,
	BITLOC_PAUSE_BREAK = 60,
	//64
	//68
	//72
	//76
	//80

	BITLOC_TILDE = 84,
	BITLOC_ONE = 88,
	BITLOC_TWO = 92,
	BITLOC_THREE = 96,
	BITLOC_FOUR = 100,
	BITLOC_FIVE = 104,
	BITLOC_SIX = 108,
	BITLOC_SEVEN = 112,
	BITLOC_EIGHT = 116,
	BITLOC_NINE = 120,
	BITLOC_ZERO = 124,
	BITLOC_MINUS = 128,
	BITLOC_EQUALS = 132,
	BITLOC_BACKSPACE = 136,
	BITLOC_INSERT = 140,
	BITLOC_HOME = 144,
	BITLOC_PAGE_UP = 148,
	BITLOC_NUM_LOCK = 152,
	BITLOC_NUM_SLASH = 156,
	BITLOC_NUM_ASTERISK = 160,
	BITLOC_NUM_MINUS = 164,

	BITLOC_TAB = 168,
	BITLOC_Q = 172,
	BITLOC_W = 176,
	BITLOC_E = 180,
	BITLOC_R = 184,
	BITLOC_T = 188,
	BITLOC_Y = 192,
	BITLOC_U = 196,
	BITLOC_I = 200,
	BITLOC_O = 204,
	BITLOC_P = 208,
	BITLOC_OPEN_BRACKET = 212,
	BITLOC_CLOSE_BRACKET = 216,
	BITLOC_BACKSLASH = 220,
	BITLOC_KEYBOARD_DELETE = 224,
	BITLOC_END = 228,
	BITLOC_PAGE_DOWN = 232,
	BITLOC_NUM_SEVEN = 236,
	BITLOC_NUM_EIGHT = 240,
	BITLOC_NUM_NINE = 244,
	BITLOC_NUM_PLUS = 248,

	BITLOC_CAPS_LOCK = 252,
	BITLOC_A = 256,
	BITLOC_S = 260,
	BITLOC_D = 264,
	BITLOC_F = 268,
	BITLOC_G = 272,
	BITLOC_H = 276,
	BITLOC_J = 280,
	BITLOC_K = 284,
	BITLOC_L = 288,
	BITLOC_SEMICOLON = 292,
	BITLOC_APOSTROPHE = 296,
	BITLOC_HASHTAG = 300,//300
	BITLOC_ENTER = 304,
	//308
	//312
	//316
	BITLOC_NUM_FOUR = 320,
	BITLOC_NUM_FIVE = 324,
	BITLOC_NUM_SIX = 328,
	//332

	BITLOC_LEFT_SHIFT = 336,
	BITLOC_BACKSLASH_UK = 340,
	BITLOC_Z = 344,
	BITLOC_X = 348,
	BITLOC_C = 352,
	BITLOC_V = 356,
	BITLOC_B = 360,
	BITLOC_N = 364,
	BITLOC_M = 368,
	BITLOC_COMMA = 372,
	BITLOC_PERIOD = 376,
	BITLOC_FORWARD_SLASH = 380,
	//384
	BITLOC_RIGHT_SHIFT = 388,
	//392
	BITLOC_ARROW_UP = 396,
	//400
	BITLOC_NUM_ONE = 404,
	BITLOC_NUM_TWO = 408,
	BITLOC_NUM_THREE = 412,
	BITLOC_NUM_ENTER = 416,

	BITLOC_LEFT_CONTROL = 420,
	BITLOC_LEFT_WINDOWS = 424,
	BITLOC_LEFT_ALT = 428,
	//432
	//436
	BITLOC_SPACE = 440,
	//444
	//448
	//452
	//456
	//460
	BITLOC_RIGHT_ALT = 464,
	BITLOC_RIGHT_WINDOWS = 468,
	BITLOC_APPLICATION_SELECT = 472,
	BITLOC_RIGHT_CONTROL = 476,
	BITLOC_ARROW_LEFT = 480,
	BITLOC_ARROW_DOWN = 484,
	BITLOC_ARROW_RIGHT = 488,
	BITLOC_NUM_ZERO = 492,
	BITLOC_NUM_PERIOD = 496,
	//500
}Logitech_keyboardBitmapKeys;

Logitech_keyboardBitmapKeys ToLogitechBitmap(int rzrow, int rzcolumn)
{
	//Row 1
	if (rzrow == 0 && rzcolumn == 1)
		return Logitech_keyboardBitmapKeys::BITLOC_ESC;
	else if (rzrow == 0 && rzcolumn == 3)
		return Logitech_keyboardBitmapKeys::BITLOC_F1;
	else if (rzrow == 0 && rzcolumn == 4)
		return Logitech_keyboardBitmapKeys::BITLOC_F2;
	else if (rzrow == 0 && rzcolumn == 5)
		return Logitech_keyboardBitmapKeys::BITLOC_F3;
	else if (rzrow == 0 && rzcolumn == 6)
		return Logitech_keyboardBitmapKeys::BITLOC_F4;
	else if (rzrow == 0 && rzcolumn == 7)
		return Logitech_keyboardBitmapKeys::BITLOC_F5;
	else if (rzrow == 0 && rzcolumn == 8)
		return Logitech_keyboardBitmapKeys::BITLOC_F6;
	else if (rzrow == 0 && rzcolumn == 9)
		return Logitech_keyboardBitmapKeys::BITLOC_F7;
	else if (rzrow == 0 && rzcolumn == 10)
		return Logitech_keyboardBitmapKeys::BITLOC_F8;
	else if (rzrow == 0 && rzcolumn == 11)
		return Logitech_keyboardBitmapKeys::BITLOC_F9;
	else if (rzrow == 0 && rzcolumn == 12)
		return Logitech_keyboardBitmapKeys::BITLOC_F10;
	else if (rzrow == 0 && rzcolumn == 13)
		return Logitech_keyboardBitmapKeys::BITLOC_F11;
	else if (rzrow == 0 && rzcolumn == 14)
		return Logitech_keyboardBitmapKeys::BITLOC_F12;
	else if (rzrow == 0 && rzcolumn == 15)
		return Logitech_keyboardBitmapKeys::BITLOC_PRINT_SCREEN;
	else if (rzrow == 0 && rzcolumn == 16)
		return Logitech_keyboardBitmapKeys::BITLOC_SCROLL_LOCK;
	else if (rzrow == 0 && rzcolumn == 17)
		return Logitech_keyboardBitmapKeys::BITLOC_PAUSE_BREAK;
	//Not implemented on Logitech
	else if (rzrow == 0 && rzcolumn == 20)
		return Logitech_keyboardBitmapKeys::LOGO;

	//Row 2
	else if (rzrow == 1 && rzcolumn == 1)
		return Logitech_keyboardBitmapKeys::BITLOC_TILDE;
	else if (rzrow == 1 && rzcolumn == 2)
		return Logitech_keyboardBitmapKeys::BITLOC_ONE;
	else if (rzrow == 1 && rzcolumn == 3)
		return Logitech_keyboardBitmapKeys::BITLOC_TWO;
	else if (rzrow == 1 && rzcolumn == 4)
		return Logitech_keyboardBitmapKeys::BITLOC_THREE;
	else if (rzrow == 1 && rzcolumn == 5)
		return Logitech_keyboardBitmapKeys::BITLOC_FOUR;
	else if (rzrow == 1 && rzcolumn == 6)
		return Logitech_keyboardBitmapKeys::BITLOC_FIVE;
	else if (rzrow == 1 && rzcolumn == 7)
		return Logitech_keyboardBitmapKeys::BITLOC_SIX;
	else if (rzrow == 1 && rzcolumn == 8)
		return Logitech_keyboardBitmapKeys::BITLOC_SEVEN;
	else if (rzrow == 1 && rzcolumn == 9)
		return Logitech_keyboardBitmapKeys::BITLOC_EIGHT;
	else if (rzrow == 1 && rzcolumn == 10)
		return Logitech_keyboardBitmapKeys::BITLOC_NINE;
	else if (rzrow == 1 && rzcolumn == 11)
		return Logitech_keyboardBitmapKeys::BITLOC_ZERO;
	else if (rzrow == 1 && rzcolumn == 12)
		return Logitech_keyboardBitmapKeys::BITLOC_MINUS;
	else if (rzrow == 1 && rzcolumn == 13)
		return Logitech_keyboardBitmapKeys::BITLOC_EQUALS;
	else if (rzrow == 1 && rzcolumn == 14)
		return Logitech_keyboardBitmapKeys::BITLOC_BACKSPACE;
	else if (rzrow == 1 && rzcolumn == 15)
		return Logitech_keyboardBitmapKeys::BITLOC_INSERT;
	else if (rzrow == 1 && rzcolumn == 16)
		return Logitech_keyboardBitmapKeys::BITLOC_HOME;
	else if (rzrow == 1 && rzcolumn == 17)
		return Logitech_keyboardBitmapKeys::BITLOC_PAGE_UP;
	else if (rzrow == 1 && rzcolumn == 18)
		return Logitech_keyboardBitmapKeys::BITLOC_NUM_LOCK;
	else if (rzrow == 1 && rzcolumn == 19)
		return Logitech_keyboardBitmapKeys::BITLOC_NUM_SLASH;
	else if (rzrow == 1 && rzcolumn == 20)
		return Logitech_keyboardBitmapKeys::BITLOC_NUM_ASTERISK;
	else if (rzrow == 1 && rzcolumn == 21)
		return Logitech_keyboardBitmapKeys::BITLOC_NUM_MINUS;

	//Row 3
	else if (rzrow == 2 && rzcolumn == 1)
		return Logitech_keyboardBitmapKeys::BITLOC_TAB;
	else if (rzrow == 2 && rzcolumn == 2)
		return Logitech_keyboardBitmapKeys::BITLOC_Q;
	else if (rzrow == 2 && rzcolumn == 3)
		return Logitech_keyboardBitmapKeys::BITLOC_W;
	else if (rzrow == 2 && rzcolumn == 4)
		return Logitech_keyboardBitmapKeys::BITLOC_E;
	else if (rzrow == 2 && rzcolumn == 5)
		return Logitech_keyboardBitmapKeys::BITLOC_R;
	else if (rzrow == 2 && rzcolumn == 6)
		return Logitech_keyboardBitmapKeys::BITLOC_T;
	else if (rzrow == 2 && rzcolumn == 7)
		return Logitech_keyboardBitmapKeys::BITLOC_Y;
	else if (rzrow == 2 && rzcolumn == 8)
		return Logitech_keyboardBitmapKeys::BITLOC_U;
	else if (rzrow == 2 && rzcolumn == 9)
		return Logitech_keyboardBitmapKeys::BITLOC_I;
	else if (rzrow == 2 && rzcolumn == 10)
		return Logitech_keyboardBitmapKeys::BITLOC_O;
	else if (rzrow == 2 && rzcolumn == 11)
		return Logitech_keyboardBitmapKeys::BITLOC_P;
	else if (rzrow == 2 && rzcolumn == 12)
		return Logitech_keyboardBitmapKeys::BITLOC_CLOSE_BRACKET;
	else if (rzrow == 2 && rzcolumn == 13)
		return Logitech_keyboardBitmapKeys::BITLOC_OPEN_BRACKET;
	else if (rzrow == 2 && rzcolumn == 14)
		return Logitech_keyboardBitmapKeys::BITLOC_BACKSLASH;
	else if (rzrow == 2 && rzcolumn == 15)
		return Logitech_keyboardBitmapKeys::BITLOC_KEYBOARD_DELETE;
	else if (rzrow == 2 && rzcolumn == 16)
		return Logitech_keyboardBitmapKeys::BITLOC_END;
	else if (rzrow == 2 && rzcolumn == 17)
		return Logitech_keyboardBitmapKeys::BITLOC_PAGE_DOWN;
	else if (rzrow == 2 && rzcolumn == 18)
		return Logitech_keyboardBitmapKeys::BITLOC_NUM_SEVEN;
	else if (rzrow == 2 && rzcolumn == 19)
		return Logitech_keyboardBitmapKeys::BITLOC_NUM_EIGHT;
	else if (rzrow == 2 && rzcolumn == 20)
		return Logitech_keyboardBitmapKeys::BITLOC_NUM_NINE;
	else if (rzrow == 2 && rzcolumn == 21)
		return Logitech_keyboardBitmapKeys::BITLOC_NUM_PLUS;

	//Row 4
	else if (rzrow == 3 && rzcolumn == 1)
		return Logitech_keyboardBitmapKeys::BITLOC_CAPS_LOCK;
	else if (rzrow == 3 && rzcolumn == 2)
		return Logitech_keyboardBitmapKeys::BITLOC_A;
	else if (rzrow == 3 && rzcolumn == 3)
		return Logitech_keyboardBitmapKeys::BITLOC_S;
	else if (rzrow == 3 && rzcolumn == 4)
		return Logitech_keyboardBitmapKeys::BITLOC_D;
	else if (rzrow == 3 && rzcolumn == 5)
		return Logitech_keyboardBitmapKeys::BITLOC_F;
	else if (rzrow == 3 && rzcolumn == 6)
		return Logitech_keyboardBitmapKeys::BITLOC_G;
	else if (rzrow == 3 && rzcolumn == 7)
		return Logitech_keyboardBitmapKeys::BITLOC_H;
	else if (rzrow == 3 && rzcolumn == 8)
		return Logitech_keyboardBitmapKeys::BITLOC_J;
	else if (rzrow == 3 && rzcolumn == 9)
		return Logitech_keyboardBitmapKeys::BITLOC_K;
	else if (rzrow == 3 && rzcolumn == 10)
		return Logitech_keyboardBitmapKeys::BITLOC_L;
	else if (rzrow == 3 && rzcolumn == 11)
		return Logitech_keyboardBitmapKeys::BITLOC_SEMICOLON;
	else if (rzrow == 3 && rzcolumn == 12)
		return Logitech_keyboardBitmapKeys::BITLOC_APOSTROPHE;
	else if (rzrow == 3 && rzcolumn == 13)
		return Logitech_keyboardBitmapKeys::BITLOC_HASHTAG;
	else if (rzrow == 3 && rzcolumn == 14)
		return Logitech_keyboardBitmapKeys::BITLOC_ENTER;
	else if (rzrow == 3 && rzcolumn == 18)
		return Logitech_keyboardBitmapKeys::BITLOC_NUM_FOUR;
	else if (rzrow == 3 && rzcolumn == 19)
		return Logitech_keyboardBitmapKeys::BITLOC_NUM_FIVE;
	else if (rzrow == 3 && rzcolumn == 20)
		return Logitech_keyboardBitmapKeys::BITLOC_NUM_SIX;

	//Row 5
	else if (rzrow == 4 && rzcolumn == 1)
		return Logitech_keyboardBitmapKeys::BITLOC_LEFT_SHIFT;
	else if (rzrow == 4 && rzcolumn == 2)
		return Logitech_keyboardBitmapKeys::BITLOC_BACKSLASH_UK;
	else if (rzrow == 4 && rzcolumn == 3)
		return Logitech_keyboardBitmapKeys::BITLOC_Z;
	else if (rzrow == 4 && rzcolumn == 4)
		return Logitech_keyboardBitmapKeys::BITLOC_X;
	else if (rzrow == 4 && rzcolumn == 5)
		return Logitech_keyboardBitmapKeys::BITLOC_C;
	else if (rzrow == 4 && rzcolumn == 6)
		return Logitech_keyboardBitmapKeys::BITLOC_V;
	else if (rzrow == 4 && rzcolumn == 7)
		return Logitech_keyboardBitmapKeys::BITLOC_B;
	else if (rzrow == 4 && rzcolumn == 8)
		return Logitech_keyboardBitmapKeys::BITLOC_N;
	else if (rzrow == 4 && rzcolumn == 9)
		return Logitech_keyboardBitmapKeys::BITLOC_M;
	else if (rzrow == 4 && rzcolumn == 10)
		return Logitech_keyboardBitmapKeys::BITLOC_COMMA;
	else if (rzrow == 4 && rzcolumn == 11)
		return Logitech_keyboardBitmapKeys::BITLOC_PERIOD;
	else if (rzrow == 4 && rzcolumn == 12)
		return Logitech_keyboardBitmapKeys::BITLOC_FORWARD_SLASH;
	else if (rzrow == 4 && rzcolumn == 14)
		return Logitech_keyboardBitmapKeys::BITLOC_RIGHT_SHIFT;
	else if (rzrow == 4 && rzcolumn == 16)
		return Logitech_keyboardBitmapKeys::BITLOC_ARROW_UP;
	else if (rzrow == 4 && rzcolumn == 18)
		return Logitech_keyboardBitmapKeys::BITLOC_NUM_ONE;
	else if (rzrow == 4 && rzcolumn == 19)
		return Logitech_keyboardBitmapKeys::BITLOC_NUM_TWO;
	else if (rzrow == 4 && rzcolumn == 20)
		return Logitech_keyboardBitmapKeys::BITLOC_NUM_THREE;
	else if (rzrow == 4 && rzcolumn == 21)
		return Logitech_keyboardBitmapKeys::BITLOC_NUM_ENTER;

	//Row 6
	else if (rzrow == 5 && rzcolumn == 1)
		return Logitech_keyboardBitmapKeys::BITLOC_LEFT_CONTROL;
	else if (rzrow == 5 && rzcolumn == 2)
		return Logitech_keyboardBitmapKeys::BITLOC_LEFT_WINDOWS;
	else if (rzrow == 5 && rzcolumn == 3)
		return Logitech_keyboardBitmapKeys::BITLOC_LEFT_ALT;
	else if (rzrow == 5 && rzcolumn == 7)
		return Logitech_keyboardBitmapKeys::BITLOC_SPACE;
	else if (rzrow == 5 && rzcolumn == 11)
		return Logitech_keyboardBitmapKeys::BITLOC_RIGHT_ALT;
	/* //Not included on Logitech
	else if (rzrow == 5 && rzcolumn == 12)
		return Logitech_keyboardBitmapKeys::BITLOC_FUNCTION_KEY;
	*/
	else if (rzrow == 5 && rzcolumn == 13)
		return Logitech_keyboardBitmapKeys::BITLOC_APPLICATION_SELECT;
	else if (rzrow == 5 && rzcolumn == 14)
		return Logitech_keyboardBitmapKeys::BITLOC_RIGHT_CONTROL;
	else if (rzrow == 5 && rzcolumn == 15)
		return Logitech_keyboardBitmapKeys::BITLOC_ARROW_LEFT;
	else if (rzrow == 5 && rzcolumn == 16)
		return Logitech_keyboardBitmapKeys::BITLOC_ARROW_DOWN;
	else if (rzrow == 5 && rzcolumn == 17)
		return Logitech_keyboardBitmapKeys::BITLOC_ARROW_RIGHT;
	else if (rzrow == 5 && rzcolumn == 19)
		return Logitech_keyboardBitmapKeys::BITLOC_NUM_ZERO;
	else if (rzrow == 5 && rzcolumn == 20)
		return Logitech_keyboardBitmapKeys::BITLOC_NUM_PERIOD;
	else
		return Logitech_keyboardBitmapKeys::UNKNOWN;
}


HANDLE hPipe;
static bool isInitialized = false;

static unsigned char current_bitmap[LOGI_LED_BITMAP_SIZE];
static unsigned char logo[4];


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

bool __fastcall WriteToPipe(unsigned char bitmap[], std::string command_cargo)
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
	ss << "],";

	ss << "\"extra_keys\": {";
	ss << "\"logo\": [";
	for (int c_pos = 0; c_pos < 4; c_pos++)
	{
		ss << (short)logo[c_pos];

		if (c_pos + 1 < 4)
			ss << ',';
	}
	ss << "]";
	ss << '}';


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

RZRESULT Init()
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
			return RZRESULT_INVALID;
		}
	}
	else
	{
		return RZRESULT_ALREADY_INITIALIZED;
	}

	isInitialized = true;
	return RZRESULT_SUCCESS;
}

RZRESULT UnInit()
{
	isInitialized = false;
	return RZRESULT_SUCCESS;
}

RZRESULT CreateEffect(RZDEVICEID DeviceId, ChromaSDK::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID *pEffectId)
{
	if (isInitialized)
	{
		switch (Effect)
		{
		case ChromaSDK::CHROMA_NONE:
			break;
		case ChromaSDK::CHROMA_WAVE:
			break;
		case ChromaSDK::CHROMA_SPECTRUMCYCLING:
			break;
		case ChromaSDK::CHROMA_BREATHING:
			break;
		case ChromaSDK::CHROMA_BLINKING:
			break;
		case ChromaSDK::CHROMA_REACTIVE:
			break;
		case ChromaSDK::CHROMA_STATIC:
			break;
		case ChromaSDK::CHROMA_CUSTOM:
			break;
		case ChromaSDK::CHROMA_STARLIGHT:
			break;
		case ChromaSDK::CHROMA_INVALID:
			break;
		default:
			break;
		}

		std::stringstream ss;
		ss << "\"command\": " << "\"CreateEffect\"" << ',';
		ss << "\"command_data\": {";

		ss << "\"custom_mode\": " << 0;

		ss << '}';

		WriteToPipe(current_bitmap, ss.str());

		return RZRESULT_SUCCESS;
	}
	else
	{
		return RZRESULT_INVALID;
	}
}

RZRESULT CreateKeyboardEffect(ChromaSDK::Keyboard::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID *pEffectId)
{
	if (isInitialized)
	{
		std::string effect_id = "";
		std::stringstream additional_effect_data;

		
		if (pEffectId == NULL)
		{
			if (Effect == ChromaSDK::Keyboard::CHROMA_STATIC)
			{
				struct ChromaSDK::Keyboard::STATIC_EFFECT_TYPE *static_effect = (struct ChromaSDK::Keyboard::STATIC_EFFECT_TYPE *)pParam;

				for (int colorset = 0; colorset < LOGI_LED_BITMAP_SIZE; colorset += 4)
				{
					current_bitmap[colorset] = GetBValue(static_effect->Color);
					current_bitmap[colorset + 1] = GetGValue(static_effect->Color);
					current_bitmap[colorset + 2] = GetRValue(static_effect->Color);
					current_bitmap[colorset + 3] = (char)255;
				}

				effect_id = "CHROMA_STATIC";
			}
			else if (Effect == ChromaSDK::Keyboard::CHROMA_NONE)
			{
				for (int colorset = 0; colorset < LOGI_LED_BITMAP_SIZE; colorset += 4)
				{
					current_bitmap[colorset] = (char)0;
					current_bitmap[colorset + 1] = (char)0;
					current_bitmap[colorset + 2] = (char)0;
					current_bitmap[colorset + 3] = (char)255;
				}

				effect_id = "CHROMA_NONE";
			}
			else if (Effect == ChromaSDK::Keyboard::CHROMA_CUSTOM)
			{
				struct ChromaSDK::Keyboard::CUSTOM_EFFECT_TYPE *custom_effect = (struct ChromaSDK::Keyboard::CUSTOM_EFFECT_TYPE *)pParam;
				
				for (int row = 0; row < ChromaSDK::Keyboard::MAX_ROW; row++)
				{
					for (int col = 0; col < ChromaSDK::Keyboard::MAX_COLUMN; col++)
					{
						Logitech_keyboardBitmapKeys bitmap_pos = ToLogitechBitmap(row, col);

						if (bitmap_pos != Logitech_keyboardBitmapKeys::UNKNOWN)
						{
							if (bitmap_pos == Logitech_keyboardBitmapKeys::LOGO)
							{
								logo[0] = GetBValue(custom_effect->Color[row][col]);
								logo[1] = GetGValue(custom_effect->Color[row][col]);
								logo[2] = GetRValue(custom_effect->Color[row][col]);
								logo[3] = (char)255;
							}
							else
							{
								current_bitmap[(int)bitmap_pos] = GetBValue(custom_effect->Color[row][col]);
								current_bitmap[(int)bitmap_pos + 1] = GetGValue(custom_effect->Color[row][col]);
								current_bitmap[(int)bitmap_pos + 2] = GetRValue(custom_effect->Color[row][col]);
								current_bitmap[(int)bitmap_pos + 3] = (char)255;
							}
						}
					}
				}

				effect_id = "CHROMA_CUSTOM";
			}
			else if (Effect == ChromaSDK::Keyboard::CHROMA_BREATHING)
			{
				struct ChromaSDK::Keyboard::BREATHING_EFFECT_TYPE *breathing_effect = (struct ChromaSDK::Keyboard::BREATHING_EFFECT_TYPE *)pParam;
				
				additional_effect_data << "\"red_start\": " << "\"" << GetRValue(breathing_effect->Color1) << "\"" << ',';
				additional_effect_data << "\"green_start\": " << "\"" << GetGValue(breathing_effect->Color1) << "\"" << ',';
				additional_effect_data << "\blue_start\": " << "\"" << GetBValue(breathing_effect->Color1) << "\"" << ',';
				additional_effect_data << "\"red_end\": " << "\"" << GetRValue(breathing_effect->Color2) << "\"" << ',';
				additional_effect_data << "\"green_end\": " << "\"" << GetGValue(breathing_effect->Color2) << "\"" << ',';
				additional_effect_data << "\blue_end\": " << "\"" << GetBValue(breathing_effect->Color2) << "\"" << ',';
				additional_effect_data << "\RZRandom\": " << "\"" << breathing_effect->Type << "\"" << ',';

				effect_id = "CHROMA_BREATHING";
			}
			else if (Effect == ChromaSDK::Keyboard::CHROMA_REACTIVE)
			{
				effect_id = "CHROMA_REACTIVE";
			}
			else if (Effect == ChromaSDK::Keyboard::CHROMA_SPECTRUMCYCLING)
			{
				effect_id = "CHROMA_SPECTRUMCYCLING";
			}
			else if (Effect == ChromaSDK::Keyboard::CHROMA_WAVE)
			{
				effect_id = "CHROMA_WAVE";
			}
			else if (Effect == ChromaSDK::Keyboard::CHROMA_STARLIGHT)
			{
				effect_id = "CHROMA_STARLIGHT";
			}
			else
			{
				effect_id = "CHROMA_INVALID";
			}
		}
		else
		{
			if (Effect == ChromaSDK::Keyboard::CHROMA_NONE)
			{
				for (int colorset = 0; colorset < LOGI_LED_BITMAP_SIZE; colorset += 4)
				{
					current_bitmap[colorset] = (char)0;
					current_bitmap[colorset + 1] = (char)0;
					current_bitmap[colorset + 2] = (char)0;
					current_bitmap[colorset + 3] = (char)255;
				}

				effect_id = "CHROMA_NONE";
			}
		}

		std::stringstream ss;
		ss << "\"command\": " << "\"CreateKeyboardEffect\"" << ',';
		ss << "\"command_data\": {";

		ss << "\"custom_mode\": " << 0;
		ss << ", \"RzEffect\": \"" << effect_id << "\"";
		if(pParam != NULL)
		ss << ", \"RzpParam\": \"" << pParam << "\"";
		if (pEffectId != NULL)
		ss << ", \"RzpEffectId\": \"" << pEffectId << "\"";
		ss << additional_effect_data.str();
		ss << '}';

		WriteToPipe(current_bitmap, ss.str());
		

		return RZRESULT_SUCCESS;
	}
	else
	{
		return RZRESULT_INVALID;
	}
}

RZRESULT CreateHeadsetEffect(ChromaSDK::Headset::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID *pEffectId)
{
	if (isInitialized)
	{
		switch (Effect)
		{
		case ChromaSDK::Headset::CHROMA_NONE:
			break;
		case ChromaSDK::Headset::CHROMA_STATIC:
			break;
		case ChromaSDK::Headset::CHROMA_BREATHING:
			break;
		case ChromaSDK::Headset::CHROMA_SPECTRUMCYCLING:
			break;
		case ChromaSDK::Headset::CHROMA_CUSTOM:
			break;
		case ChromaSDK::Headset::CHROMA_INVALID:
			break;
		default:
			break;
		}

		std::stringstream ss;
		ss << "\"command\": " << "\"CreateHeadsetEffect\"" << ',';
		ss << "\"command_data\": {";

		ss << "\"custom_mode\": " << 0;

		ss << '}';

		WriteToPipe(current_bitmap, ss.str());

		return RZRESULT_SUCCESS;
	}
	else
	{
		return RZRESULT_INVALID;
	}
}

RZRESULT CreateMousepadEffect(ChromaSDK::Mousepad::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID *pEffectId)
{
	if (isInitialized)
	{
		switch (Effect)
		{
		case ChromaSDK::Mousepad::CHROMA_NONE:
			break;
		case ChromaSDK::Mousepad::CHROMA_BREATHING:
			break;
		case ChromaSDK::Mousepad::CHROMA_CUSTOM:
			break;
		case ChromaSDK::Mousepad::CHROMA_SPECTRUMCYCLING:
			break;
		case ChromaSDK::Mousepad::CHROMA_STATIC:
			break;
		case ChromaSDK::Mousepad::CHROMA_WAVE:
			break;
		case ChromaSDK::Mousepad::CHROMA_INVALID:
			break;
		default:
			break;
		}

		return RZRESULT_SUCCESS;
	}
	else
	{
		return RZRESULT_INVALID;
	}
}

RZRESULT CreateMouseEffect(ChromaSDK::Mouse::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID *pEffectId)
{
	if (isInitialized)
	{
		switch (Effect)
		{
		case ChromaSDK::Mouse::CHROMA_NONE:
			break;
		case ChromaSDK::Mouse::CHROMA_BLINKING:
			break;
		case ChromaSDK::Mouse::CHROMA_BREATHING:
			break;
		case ChromaSDK::Mouse::CHROMA_CUSTOM:
			break;
		case ChromaSDK::Mouse::CHROMA_REACTIVE:
			break;
		case ChromaSDK::Mouse::CHROMA_SPECTRUMCYCLING:
			break;
		case ChromaSDK::Mouse::CHROMA_STATIC:
			break;
		case ChromaSDK::Mouse::CHROMA_WAVE:
			break;
		case ChromaSDK::Mouse::CHROMA_CUSTOM2:
			break;
		case ChromaSDK::Mouse::CHROMA_INVALID:
			break;
		default:
			break;
		}

		return RZRESULT_SUCCESS;
	}
	else
	{
		return RZRESULT_INVALID;
	}
}

RZRESULT CreateKeypadEffect(ChromaSDK::Keypad::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID *pEffectId)
{
	if (isInitialized)
	{
		switch (Effect)
		{
		case ChromaSDK::Keypad::CHROMA_NONE:
			break;
		case ChromaSDK::Keypad::CHROMA_BREATHING:
			break;
		case ChromaSDK::Keypad::CHROMA_CUSTOM:
			break;
		case ChromaSDK::Keypad::CHROMA_REACTIVE:
			break;
		case ChromaSDK::Keypad::CHROMA_SPECTRUMCYCLING:
			break;
		case ChromaSDK::Keypad::CHROMA_STATIC:
			break;
		case ChromaSDK::Keypad::CHROMA_WAVE:
			break;
		case ChromaSDK::Keypad::CHROMA_INVALID:
			break;
		default:
			break;
		}

		return RZRESULT_SUCCESS;
	}
	else
	{
		return RZRESULT_INVALID;
	}
}

RZRESULT SetEffect(RZEFFECTID EffectId)
{
	if (isInitialized)
	{
		std::stringstream ss;
		ss << "\"command\": " << "\"SetEffect\"" << ',';
		ss << "\"command_data\": {";

		ss << "\"custom_mode\": " << 0;

		ss << '}';

		WriteToPipe(current_bitmap, ss.str());
		
		return RZRESULT_SUCCESS;
	}
	else
	{
		return RZRESULT_INVALID;
	}
}

RZRESULT DeleteEffect(RZEFFECTID EffectId)
{
	if (isInitialized)
	{
		std::stringstream ss;
		ss << "\"command\": " << "\"DeleteEffect\"" << ',';
		ss << "\"command_data\": {";

		ss << "\"custom_mode\": " << 0;

		ss << '}';

		WriteToPipe(current_bitmap, ss.str());
		
		return RZRESULT_SUCCESS;
	}
	else
	{
		return RZRESULT_INVALID;
	}
}

RZRESULT RegisterEventNotification(HWND hWnd)
{
	if (isInitialized)
	{
		std::stringstream ss;
		ss << "\"command\": " << "\"RegisterEventNotification\"" << ',';
		ss << "\"command_data\": {";

		ss << "\"custom_mode\": " << 0;

		ss << '}';

		WriteToPipe(current_bitmap, ss.str());
		
		return RZRESULT_SUCCESS;
	}
	else
	{
		return RZRESULT_INVALID;
	}
}

RZRESULT UnregisterEventNotification(HWND hWnd)
{
	if (isInitialized)
	{
		std::stringstream ss;
		ss << "\"command\": " << "\"UnregisterEventNotification\"" << ',';
		ss << "\"command_data\": {";

		ss << "\"custom_mode\": " << 0;

		ss << '}';

		WriteToPipe(current_bitmap, ss.str());
		
		return RZRESULT_SUCCESS;
	}
	else
	{
		return RZRESULT_INVALID;
	}
}

RZRESULT QueryDevice(RZDEVICEID DeviceId, ChromaSDK::DEVICE_INFO_TYPE &DeviceInfo)
{
	DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DeviceType::DEVICE_KEYBOARD;
	DeviceInfo.Connected = 1;
	
	if (isInitialized)
	{
		std::stringstream ss;
		ss << "\"command\": " << "\"QueryDevice\"" << ',';
		ss << "\"command_data\": {";

		ss << "\"custom_mode\": " << 0;

		ss << '}';

		WriteToPipe(current_bitmap, ss.str());
		
		return RZRESULT_SUCCESS;
	}
	else
	{
		return RZRESULT_INVALID;
	}
}


/*
Init
UnInit
CreateEffect
SetEffect
DeleteEffect
RegisterEventNotification
UnregisterEventNotification
CreateKeyboardEffect
CreateMouseEffect
CreateHeadsetEffect
CreateMousepadEffect
CreateKeypadEffect
QueryDevice
EnumDevice
*/