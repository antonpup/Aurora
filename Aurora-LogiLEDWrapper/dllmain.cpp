// dllmain.cpp : Defines the entry point for the DLL application.
#include "dllmain.h"
#include "LogitechLEDLib.h"
#include "stdafx.h"
#include <stdio.h>
#include <string>
#include <iomanip>
#include <windows.h>

#define PIPE_NAME L"\\\\.\\pipe\\Aurora\\server" 

HANDLE hPipe;
static bool isInitialized = false;

static unsigned char current_bitmap[LOGI_LED_BITMAP_SIZE];
static unsigned char current_bg[3];

static int current_device = LOGI_DEVICETYPE_ALL;


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

LogiLed::Logitech_keyboardBitmapKeys ToLogitechBitmap(LogiLed::KeyName keyName)
{
	switch (keyName)
	{
	case(LogiLed::KeyName::ESC) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_ESC;
	case(LogiLed::KeyName::F1) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F1;
	case(LogiLed::KeyName::F2) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F2;
	case(LogiLed::KeyName::F3) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F3;
	case(LogiLed::KeyName::F4) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F4;
	case(LogiLed::KeyName::F5) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F5;
	case(LogiLed::KeyName::F6) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F6;
	case(LogiLed::KeyName::F7) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F7;
	case(LogiLed::KeyName::F8) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F8;
	case(LogiLed::KeyName::F9) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F9;
	case(LogiLed::KeyName::F10) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F10;
	case(LogiLed::KeyName::F11) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F11;
	case(LogiLed::KeyName::F12) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F12;
	case(LogiLed::KeyName::PRINT_SCREEN) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_PRINT_SCREEN;
	case(LogiLed::KeyName::SCROLL_LOCK) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_SCROLL_LOCK;
	case(LogiLed::KeyName::PAUSE_BREAK) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_PAUSE_BREAK;
	case(LogiLed::KeyName::TILDE) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_TILDE;
	case(LogiLed::KeyName::ONE) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_ONE;
	case(LogiLed::KeyName::TWO) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_TWO;
	case(LogiLed::KeyName::THREE) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_THREE;
	case(LogiLed::KeyName::FOUR) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_FOUR;
	case(LogiLed::KeyName::FIVE) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_FIVE;
	case(LogiLed::KeyName::SIX) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_SIX;
	case(LogiLed::KeyName::SEVEN) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_SEVEN;
	case(LogiLed::KeyName::EIGHT) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_EIGHT;
	case(LogiLed::KeyName::NINE) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NINE;
	case(LogiLed::KeyName::ZERO) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_ZERO;
	case(LogiLed::KeyName::MINUS) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_MINUS;
	case(LogiLed::KeyName::EQUALS) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_EQUALS;
	case(LogiLed::KeyName::BACKSPACE) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_BACKSPACE;
	case(LogiLed::KeyName::INSERT) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_INSERT;
	case(LogiLed::KeyName::HOME) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_HOME;
	case(LogiLed::KeyName::PAGE_UP) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_PAGE_UP;
	case(LogiLed::KeyName::NUM_LOCK) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NUM_LOCK;
	case(LogiLed::KeyName::NUM_SLASH) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NUM_SLASH;
	case(LogiLed::KeyName::NUM_ASTERISK) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NUM_ASTERISK;
	case(LogiLed::KeyName::NUM_MINUS) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NUM_MINUS;
	case(LogiLed::KeyName::TAB) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_TAB;
	case(LogiLed::KeyName::Q) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_Q;
	case(LogiLed::KeyName::W) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_W;
	case(LogiLed::KeyName::E) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_E;
	case(LogiLed::KeyName::R) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_R;
	case(LogiLed::KeyName::T) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_T;
	case(LogiLed::KeyName::Y) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_Y;
	case(LogiLed::KeyName::U) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_U;
	case(LogiLed::KeyName::I) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_I;
	case(LogiLed::KeyName::O) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_O;
	case(LogiLed::KeyName::P) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_P;
	case(LogiLed::KeyName::OPEN_BRACKET) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_OPEN_BRACKET;
	case(LogiLed::KeyName::CLOSE_BRACKET) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_CLOSE_BRACKET;
	case(LogiLed::KeyName::BACKSLASH) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_BACKSLASH;
	case(LogiLed::KeyName::KEYBOARD_DELETE) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_KEYBOARD_DELETE;
	case(LogiLed::KeyName::END) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_END;
	case(LogiLed::KeyName::PAGE_DOWN) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_PAGE_DOWN;
	case(LogiLed::KeyName::NUM_SEVEN) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NUM_SEVEN;
	case(LogiLed::KeyName::NUM_EIGHT) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NUM_EIGHT;
	case(LogiLed::KeyName::NUM_NINE) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NUM_NINE;
	case(LogiLed::KeyName::NUM_PLUS) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NUM_PLUS;
	case(LogiLed::KeyName::CAPS_LOCK) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_CAPS_LOCK;
	case(LogiLed::KeyName::A) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_A;
	case(LogiLed::KeyName::S) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_S;
	case(LogiLed::KeyName::D) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_D;
	case(LogiLed::KeyName::F) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F;
	case(LogiLed::KeyName::G) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_G;
	case(LogiLed::KeyName::H) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_H;
	case(LogiLed::KeyName::J) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_J;
	case(LogiLed::KeyName::K) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_K;
	case(LogiLed::KeyName::L) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_L;
	case(LogiLed::KeyName::SEMICOLON) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_SEMICOLON;
	case(LogiLed::KeyName::APOSTROPHE) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_APOSTROPHE;
		//case(LogiLed::KeyName::HASHTAG) :
		//	return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_HASHTAG;
	case(LogiLed::KeyName::ENTER) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_ENTER;
	case(LogiLed::KeyName::NUM_FOUR) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NUM_FOUR;
	case(LogiLed::KeyName::NUM_FIVE) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NUM_FIVE;
	case(LogiLed::KeyName::NUM_SIX) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NUM_SIX;
	case(LogiLed::KeyName::LEFT_SHIFT) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_LEFT_SHIFT;
		//case(LogiLed::KeyName::BACKSLASH_UK) :
		//	return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_BACKSLASH_UK;
	case(LogiLed::KeyName::Z) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_Z;
	case(LogiLed::KeyName::X) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_X;
	case(LogiLed::KeyName::C) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_C;
	case(LogiLed::KeyName::V) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_V;
	case(LogiLed::KeyName::B) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_B;
	case(LogiLed::KeyName::N) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_N;
	case(LogiLed::KeyName::M) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_M;
	case(LogiLed::KeyName::COMMA) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_COMMA;
	case(LogiLed::KeyName::PERIOD) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_PERIOD;
	case(LogiLed::KeyName::FORWARD_SLASH) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_FORWARD_SLASH;
	case(LogiLed::KeyName::RIGHT_SHIFT) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_RIGHT_SHIFT;
	case(LogiLed::KeyName::ARROW_UP) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_ARROW_UP;
	case(LogiLed::KeyName::NUM_ONE) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NUM_ONE;
	case(LogiLed::KeyName::NUM_TWO) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NUM_TWO;
	case(LogiLed::KeyName::NUM_THREE) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NUM_THREE;
	case(LogiLed::KeyName::NUM_ENTER) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NUM_ENTER;
	case(LogiLed::KeyName::LEFT_CONTROL) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_LEFT_CONTROL;
	case(LogiLed::KeyName::LEFT_WINDOWS) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_LEFT_WINDOWS;
	case(LogiLed::KeyName::LEFT_ALT) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_LEFT_ALT;
	case(LogiLed::KeyName::SPACE) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_SPACE;
	case(LogiLed::KeyName::RIGHT_ALT) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_RIGHT_ALT;
	case(LogiLed::KeyName::RIGHT_WINDOWS) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_RIGHT_WINDOWS;
	case(LogiLed::KeyName::APPLICATION_SELECT) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_APPLICATION_SELECT;
	case(LogiLed::KeyName::RIGHT_CONTROL) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_RIGHT_CONTROL;
	case(LogiLed::KeyName::ARROW_LEFT) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_ARROW_LEFT;
	case(LogiLed::KeyName::ARROW_DOWN) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_ARROW_DOWN;
	case(LogiLed::KeyName::ARROW_RIGHT) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_ARROW_RIGHT;
	case(LogiLed::KeyName::NUM_ZERO) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NUM_ZERO;
	case(LogiLed::KeyName::NUM_PERIOD) :
		return LogiLed::Logitech_keyboardBitmapKeys::BITLOC_NUM_PERIOD;
	default:
		return LogiLed::Logitech_keyboardBitmapKeys::UNKNOWN;
	}
}

/*

{
"provider": {
"name": "GTA5.exe",
"appid": 0
},
"command": "FlashLighting",
"command_data": {
"red_start": 255,
"green_start": 255,
"blue_start": 255,
"red_end": 255,
"green_end": 255,
"blue_end": 255,
"duration": 10,
"interval": 10,
"key": 13
},
"bitmap": [
39,
10
]
}

*/


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

void _LogiLedSetLighting(int redPercentage, int greenPercentage, int bluePercentage, int custom_mode = 0)
{
	unsigned char redValue = (unsigned char)((redPercentage / 100.0f) * 255);
	unsigned char greenValue = (unsigned char)((greenPercentage / 100.0f) * 255);
	unsigned char blueValue = (unsigned char)((bluePercentage / 100.0f) * 255);

	if (isInitialized && (current_device == LOGI_DEVICETYPE_ALL || current_device == LOGI_DEVICETYPE_PERKEY_RGB))
	{
		std::string contents = "";

		if (program_name.compare("GTA5.exe") == 0)
		{
			for (int colorset = 0; colorset < LOGI_LED_BITMAP_SIZE; colorset += 4)
			{
				current_bitmap[colorset] = blueValue;
				current_bitmap[colorset + 1] = greenValue;
				current_bitmap[colorset + 2] = redValue;
				current_bitmap[colorset + 3] = (char)255;
			}
			
			switch (custom_mode)
			{
			case 0xFFFB00:
			case 0xFFFB50:
			case 0xFFFBE4:
			case 0xFFFBFF:
				//F1
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F1 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F1 + 2] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F1 + 3] = (char)255;
				//F2
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F2] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F2 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F2 + 2] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F2 + 3] = (char)255;
				//F3
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F3] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F3 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F3 + 2] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F3 + 3] = (char)255;
				//F4
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F4] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F4 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F4 + 2] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F4 + 3] = (char)255;
				//F5
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F5] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F5 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F5 + 2] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F5 + 3] = (char)255;
				//F6
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F6] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F6 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F6 + 2] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F6 + 3] = (char)255;
				//F7
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F7] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F7 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F7 + 2] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F7 + 3] = (char)255;
				//F8
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F8] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F8 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F8 + 2] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F8 + 3] = (char)255;
				//F9
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F9] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F9 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F9 + 2] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F9 + 3] = (char)255;
				//F10
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F10] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F10 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F10 + 2] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F10 + 3] = (char)255;
				//F11
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F11] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F11 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F11 + 2] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F11 + 3] = (char)255;
				//F12
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F12] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F12 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F12 + 2] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F12 + 3] = (char)255;
				break;
			case 0xFF0100:
			case 0xFF0150:
			case 0xFF01E4:
			case 0xFF01FF:
				//F1
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F1] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F1 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F1 + 2] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F1 + 3] = (char)255;
				//F2
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F2] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F2 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F2 + 2] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F2 + 3] = (char)255;
				//F3
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F3] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F3 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F3 + 2] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F3 + 3] = (char)255;
				//F4
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F4] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F4 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F4 + 2] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F4 + 3] = (char)255;
				//F5
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F5] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F5 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F5 + 2] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F5 + 3] = (char)255;
				//F6
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F6] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F6 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F6 + 2] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F6 + 3] = (char)255;
				//F7
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F7] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F7 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F7 + 2] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F7 + 3] = (char)255;
				//F8
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F8] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F8 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F8 + 2] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F8 + 3] = (char)255;
				//F9
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F9] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F9 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F9 + 2] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F9 + 3] = (char)255;
				//F10
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F10] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F10 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F10 + 2] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F10 + 3] = (char)255;
				//F11
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F11] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F11 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F11 + 2] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F11 + 3] = (char)255;
				//F12
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F12] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F12 + 1] = (char)0;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F12 + 2] = (char)255;
				current_bitmap[(int)LogiLed::Logitech_keyboardBitmapKeys::BITLOC_F12 + 3] = (char)255;
			}


			contents += "\"bitmap\": [";
			for (int bitm_pos = 0; bitm_pos < LOGI_LED_BITMAP_SIZE; bitm_pos++)
			{
				contents += std::to_string((short)current_bitmap[bitm_pos]);

				if (bitm_pos + 1 < LOGI_LED_BITMAP_SIZE)
					contents += ',';
			}
			contents += "],";
		}
		else
		{

			if (current_bg[0] == blueValue &&
				current_bg[1] == greenValue &&
				current_bg[2] == redValue
				)
			{
				//No need to write on pipe, color did not change
				return;
			}

			current_bg[0] = blueValue;
			current_bg[1] = greenValue;
			current_bg[2] = redValue;

			for (int colorset = 0; colorset < LOGI_LED_BITMAP_SIZE; colorset += 4)
			{
				current_bitmap[colorset] = blueValue;
				current_bitmap[colorset + 1] = greenValue;
				current_bitmap[colorset + 2] = redValue;
				current_bitmap[colorset + 3] = (char)255;
			}
		}

		contents += "\"command\": \"SetLighting\",";
		contents += "\"command_data\": {";
		contents += "\"red_start\": " + std::to_string((int)redValue) + ',';
		contents += "\"green_start\": " + std::to_string((int)greenValue) + ',';
		contents += "\"blue_start\": " + std::to_string((int)blueValue) + ',';

		contents += "\"custom_mode\": " + std::to_string(custom_mode);

		contents += '}';

		WriteToPipe(contents);
	}
}

void _LogiLedFlashLighting(int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval)
{
	unsigned char redValue = (unsigned char)((redPercentage / 100.0f) * 255);
	unsigned char greenValue = (unsigned char)((greenPercentage / 100.0f) * 255);
	unsigned char blueValue = (unsigned char)((bluePercentage / 100.0f) * 255);

	std::string contents = "";
	contents += "\"command\": \"FlashLighting\",";
	contents += "\"command_data\": {";

	contents += "\"red_start\": " + std::to_string((int)redValue) + ',';
	contents += "\"green_start\": " + std::to_string((int)greenValue) + ',';
	contents += "\"blue_start\": " + std::to_string((int)blueValue) + ',';
	contents += "\"duration\": " + std::to_string(milliSecondsDuration) + ',';
	contents += "\"interval\": " + std::to_string(milliSecondsInterval);

	contents += '}';

	WriteToPipe(contents);
}

void _LogiLedPulseLighting(int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval)
{
	unsigned char redValue = (unsigned char)((redPercentage / 100.0f) * 255);
	unsigned char greenValue = (unsigned char)((greenPercentage / 100.0f) * 255);
	unsigned char blueValue = (unsigned char)((bluePercentage / 100.0f) * 255);

	std::string contents = "";
	contents += "\"command\": \"PulseLighting\",";
	contents += "\"command_data\": {";

	contents += "\"red_start\": " + std::to_string((int)redValue) + ',';
	contents += "\"green_start\": " + std::to_string((int)greenValue) + ',';
	contents += "\"blue_start\": " + std::to_string((int)blueValue) + ',';
	contents += "\"duration\": " + std::to_string(milliSecondsDuration) + ',';
	contents += "\"interval\": " + std::to_string(milliSecondsInterval);

	contents += '}';

	WriteToPipe(contents);
}

void _LogiLedStopEffects()
{
	std::string contents = "";
	contents += "\"command\": \"StopEffects\",";
	contents += "\"command_data\": {";
	contents += '}';

	WriteToPipe(contents);
}

void _LogiLedSetLightingFromBitmap(unsigned char bitmap[])
{
	if (isInitialized && (current_device == LOGI_DEVICETYPE_ALL || current_device == LOGI_DEVICETYPE_PERKEY_RGB))
	{
		for (int colorset = 0; colorset < LOGI_LED_BITMAP_SIZE; colorset += 4)
		{
			current_bitmap[colorset] = bitmap[colorset];
			current_bitmap[colorset + 1] = bitmap[colorset + 1];
			current_bitmap[colorset + 2] = bitmap[colorset + 2];
			current_bitmap[colorset + 3] = bitmap[colorset + 3];
		}

		std::string contents = "";
		contents += "\"command\": \"SetLightingFromBitmap\",";
		contents += "\"command_data\": {";
		contents += "},";
		contents += "\"bitmap\": [";
		for (int bitm_pos = 0; bitm_pos < LOGI_LED_BITMAP_SIZE; bitm_pos++)
		{
			contents += std::to_string((short)current_bitmap[bitm_pos]);

			if (bitm_pos + 1 < LOGI_LED_BITMAP_SIZE)
				contents += ',';
		}
		contents += "]";

		WriteToPipe(contents);
	}
}

void _LogiLedSetLightingForKeyWithScanCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage)
{
	unsigned char redValue = (unsigned char)((redPercentage / 100.0f) * 255);
	unsigned char greenValue = (unsigned char)((greenPercentage / 100.0f) * 255);
	unsigned char blueValue = (unsigned char)((bluePercentage / 100.0f) * 255);

	std::string contents = "";
	contents += "\"command\": \"SetLightingForKeyWithScanCode\",";
	contents += "\"command_data\": {";

	contents += "\"red_start\": " + std::to_string((int)redValue) + ',';
	contents += "\"green_start\": " + std::to_string((int)greenValue) + ',';
	contents += "\"blue_start\": " + std::to_string((int)blueValue) + ',';
	contents += "\"key\": " + std::to_string(keyCode);

	contents += '}';

	//NOT IMPLEMENTED
	/*
	LogiLed::Logitech_keyboardBitmapKeys bit_location = ToLogitechBitmap(keyName);

	if (bit_location != LogiLed::Logitech_keyboardBitmapKeys::UNKNOWN)
	{
	current_bitmap[(int)bit_location] = blueValue;
	current_bitmap[(int)bit_location + 1] = greenValue;
	current_bitmap[(int)bit_location + 2] = redValue;
	current_bitmap[(int)bit_location + 3] = (char)255;

	return WriteToPipe(current_bitmap, ss.str());
	}
	*/
	WriteToPipe(contents);
}

void _LogiLedSetLightingForKeyWithHidCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage)
{
	unsigned char redValue = (unsigned char)((redPercentage / 100.0f) * 255);
	unsigned char greenValue = (unsigned char)((greenPercentage / 100.0f) * 255);
	unsigned char blueValue = (unsigned char)((bluePercentage / 100.0f) * 255);

	std::string contents = "";
	contents += "\"command\": \"SetLightingForKeyWithHidCode\",";
	contents += "\"command_data\": {";

	contents += "\"red_start\": " + std::to_string((int)redValue) + ',';
	contents += "\"green_start\": " + std::to_string((int)greenValue) + ',';
	contents += "\"blue_start\": " + std::to_string((int)blueValue) + ',';
	contents += "\"key\": " + std::to_string(keyCode);

	contents += '}';

	//NOT IMPLEMENTED
	/*
	LogiLed::Logitech_keyboardBitmapKeys bit_location = ToLogitechBitmap(keyName);

	if (bit_location != LogiLed::Logitech_keyboardBitmapKeys::UNKNOWN)
	{
	current_bitmap[(int)bit_location] = blueValue;
	current_bitmap[(int)bit_location + 1] = greenValue;
	current_bitmap[(int)bit_location + 2] = redValue;
	current_bitmap[(int)bit_location + 3] = (char)255;

	return WriteToPipe(current_bitmap, ss.str());
	}
	*/
	WriteToPipe(contents);
}

void _LogiLedSetLightingForKeyWithQuartzCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage)
{
	unsigned char redValue = (unsigned char)((redPercentage / 100.0f) * 255);
	unsigned char greenValue = (unsigned char)((greenPercentage / 100.0f) * 255);
	unsigned char blueValue = (unsigned char)((bluePercentage / 100.0f) * 255);

	std::string contents = "";
	contents += "\"command\": \"SetLightingForKeyWithQuartzCode\",";
	contents += "\"command_data\": {";

	contents += "\"red_start\": " + std::to_string((int)redValue) + ',';
	contents += "\"green_start\": " + std::to_string((int)greenValue) + ',';
	contents += "\"blue_start\": " + std::to_string((int)blueValue) + ',';
	contents += "\"key\": " + std::to_string(keyCode);

	contents += '}';

	//NOT IMPLEMENTED
	/*
	LogiLed::Logitech_keyboardBitmapKeys bit_location = ToLogitechBitmap(keyName);

	if (bit_location != LogiLed::Logitech_keyboardBitmapKeys::UNKNOWN)
	{
	current_bitmap[(int)bit_location] = blueValue;
	current_bitmap[(int)bit_location + 1] = greenValue;
	current_bitmap[(int)bit_location + 2] = redValue;
	current_bitmap[(int)bit_location + 3] = (char)255;

	return WriteToPipe(current_bitmap, ss.str());
	}
	*/
	WriteToPipe(contents);
}

void _LogiLedSetLightingForKeyWithKeyName(LogiLed::KeyName keyName, int redPercentage, int greenPercentage, int bluePercentage)
{
	unsigned char redValue = (unsigned char)((redPercentage / 100.0f) * 255);
	unsigned char greenValue = (unsigned char)((greenPercentage / 100.0f) * 255);
	unsigned char blueValue = (unsigned char)((bluePercentage / 100.0f) * 255);

	LogiLed::Logitech_keyboardBitmapKeys bit_location = ToLogitechBitmap(keyName);

	if (isInitialized && (current_device == LOGI_DEVICETYPE_ALL || current_device == LOGI_DEVICETYPE_PERKEY_RGB))
	{
		if (bit_location == LogiLed::Logitech_keyboardBitmapKeys::UNKNOWN ||
			(
				current_bitmap[(int)bit_location] == blueValue &&
				current_bitmap[(int)bit_location + 1] == greenValue &&
				current_bitmap[(int)bit_location + 2] == redValue
				)
			)
		{
			//No need to write on pipe, color did not change
			return;
		}

		current_bitmap[(int)bit_location] = blueValue;
		current_bitmap[(int)bit_location + 1] = greenValue;
		current_bitmap[(int)bit_location + 2] = redValue;

		std::string contents = "";
		contents += "\"command\": \"SetLightingForKeyWithKeyName\",";
		contents += "\"command_data\": {";

		contents += "\"red_start\": " + std::to_string((int)redValue) + ',';
		contents += "\"green_start\": " + std::to_string((int)greenValue) + ',';
		contents += "\"blue_start\": " + std::to_string((int)blueValue) + ',';
		contents += "\"key\": " + std::to_string(keyName);

		contents += "}";

		WriteToPipe(contents);
	}
}

void _LogiLedFlashSingleKey(LogiLed::KeyName keyName, int redPercentage, int greenPercentage, int bluePercentage, int msDuration, int msInterval)
{
	unsigned char redValue = (unsigned char)((redPercentage / 100.0f) * 255);
	unsigned char greenValue = (unsigned char)((greenPercentage / 100.0f) * 255);
	unsigned char blueValue = (unsigned char)((bluePercentage / 100.0f) * 255);

	std::string contents = "";
	contents += "\"command\": \"FlashSingleKey\",";
	contents += "\"command_data\": {";

	contents += "\"red_start\": " + std::to_string((int)redValue) + ',';
	contents += "\"green_start\": " + std::to_string((int)greenValue) + ',';
	contents += "\"blue_start\": " + std::to_string((int)blueValue) + ',';
	contents += "\"duration\": " + std::to_string(msDuration) + ',';
	contents += "\"interval\": " + std::to_string(msInterval) + ',';
	contents += "\"key\": " + std::to_string(keyName);

	contents += '}';

	WriteToPipe(contents);
}

void _LogiLedPulseSingleKey(LogiLed::KeyName keyName, int startRedPercentage, int startGreenPercentage, int startBluePercentage, int finishRedPercentage, int finishGreenPercentage, int finishBluePercentage, int msDuration, bool isInfinite)
{
	unsigned char redValue = (unsigned char)((startRedPercentage / 100.0f) * 255);
	unsigned char greenValue = (unsigned char)((startGreenPercentage / 100.0f) * 255);
	unsigned char blueValue = (unsigned char)((startBluePercentage / 100.0f) * 255);
	unsigned char redValue_end = (unsigned char)((finishRedPercentage / 100.0f) * 255);
	unsigned char greenValue_end = (unsigned char)((finishGreenPercentage / 100.0f) * 255);
	unsigned char blueValue_end = (unsigned char)((finishBluePercentage / 100.0f) * 255);

	std::string contents = "";
	contents += "\"command\": \"PulseSingleKey\",";
	contents += "\"command_data\": {";

	contents += "\"red_start\": " + std::to_string((int)redValue) + ',';
	contents += "\"green_start\": " + std::to_string((int)greenValue) + ',';
	contents += "\"blue_start\": " + std::to_string((int)blueValue) + ',';
	contents += "\"red_end\": " + std::to_string((int)redValue_end) + ',';
	contents += "\"green_end\": " + std::to_string((int)greenValue_end) + ',';
	contents += "\"blue_end\": " + std::to_string((int)blueValue_end) + ',';
	contents += "\"duration\": " + std::to_string(msDuration) + ',';
	if (isInfinite)
		contents += "\"interval\": 0,";
	else
		contents += "\"interval\": -1,";
	contents += "\"key\": " + std::to_string(keyName);

	contents += '}';

	WriteToPipe(contents);
}

void _LogiLedStopEffectsOnKey(LogiLed::KeyName keyName)
{
	std::string contents = "";
	contents += "\"command\": \"StopEffectsOnKey\",";
	contents += "\"command_data\": {";
	contents += "\"key\": " + std::to_string(keyName);
	contents += '}';

	WriteToPipe(contents);
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

bool LogiLedGetSdkVersion(int *majorNum, int *minorNum, int *buildNum)
{
	*majorNum = 8;
	*minorNum = 75;
	*buildNum = 30;

	return true;
}

bool LogiLedSetTargetDevice(int targetDevice)
{
	current_device = targetDevice;
	
	return isInitialized;
}

bool LogiLedSaveCurrentLighting()
{
	return isInitialized;
}

bool LogiLedSetLighting(int redPercentage, int greenPercentage, int bluePercentage, int custom_mode = 0)
{
	_LogiLedSetLighting(redPercentage, greenPercentage, bluePercentage, custom_mode);

	return isInitialized;
}

bool LogiLedRestoreLighting()
{
	return isInitialized;
}

bool LogiLedFlashLighting(int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval)
{
	_LogiLedFlashLighting(redPercentage, greenPercentage, bluePercentage, milliSecondsDuration, milliSecondsInterval);

	return isInitialized;
}

bool LogiLedPulseLighting(int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval)
{
	_LogiLedPulseLighting(redPercentage, greenPercentage, bluePercentage, milliSecondsDuration, milliSecondsInterval);

	return isInitialized;
}

bool LogiLedStopEffects()
{
	_LogiLedStopEffects();

	return isInitialized;
}

bool LogiLedSetLightingFromBitmap(unsigned char bitmap[])
{
	_LogiLedSetLightingFromBitmap(bitmap);

	return isInitialized;
}

bool LogiLedSetLightingForKeyWithScanCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage)
{
	_LogiLedSetLightingForKeyWithScanCode(keyCode, redPercentage, greenPercentage, bluePercentage);

	return isInitialized;
}

bool LogiLedSetLightingForKeyWithHidCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage)
{
	_LogiLedSetLightingForKeyWithHidCode(keyCode, redPercentage, greenPercentage, bluePercentage);

	return isInitialized;
}

bool LogiLedSetLightingForKeyWithQuartzCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage)
{
	_LogiLedSetLightingForKeyWithQuartzCode(keyCode, redPercentage, greenPercentage, bluePercentage);

	return isInitialized;
}

bool LogiLedSetLightingForKeyWithKeyName(LogiLed::KeyName keyName, int redPercentage, int greenPercentage, int bluePercentage)
{
	_LogiLedSetLightingForKeyWithKeyName(keyName, redPercentage, greenPercentage, bluePercentage);

	return isInitialized;
}

bool LogiLedSaveLightingForKey(LogiLed::KeyName keyName)
{
	return isInitialized;
}

bool LogiLedRestoreLightingForKey(LogiLed::KeyName keyName)
{
	return isInitialized;
}

bool LogiLedFlashSingleKey(LogiLed::KeyName keyName, int redPercentage, int greenPercentage, int bluePercentage, int msDuration, int msInterval)
{
	_LogiLedFlashSingleKey(keyName, redPercentage, greenPercentage, bluePercentage, msDuration, msInterval);

	return isInitialized;
}

bool LogiLedPulseSingleKey(LogiLed::KeyName keyName, int startRedPercentage, int startGreenPercentage, int startBluePercentage, int finishRedPercentage, int finishGreenPercentage, int finishBluePercentage, int msDuration, bool isInfinite)
{
	_LogiLedPulseSingleKey(keyName, startRedPercentage, startGreenPercentage, startBluePercentage, finishRedPercentage, finishGreenPercentage, finishBluePercentage, msDuration, isInfinite);

	return isInitialized;
}

bool LogiLedStopEffectsOnKey(LogiLed::KeyName keyName)
{
	_LogiLedStopEffectsOnKey(keyName);

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