// dllmain.cpp : Defines the entry point for the DLL application.
#include "RzChromaSDKDefines.h"
#include "RzChromaSDKTypes.h"
#include "RzErrors.h"
#include "stdafx.h"
#include <fstream>
#include <objbase.h>
#include <stdio.h>
#include <string>
#include <iomanip>
#include <map>
#include <sstream>
#include <windows.h>

#define PIPE_NAME L"\\\\.\\pipe\\Aurora\\server" 

#define LOGI_LED_BITMAP_WIDTH 21
#define LOGI_LED_BITMAP_HEIGHT 6
#define LOGI_LED_BITMAP_BYTES_PER_KEY 4

#define LOGI_LED_BITMAP_SIZE (LOGI_LED_BITMAP_WIDTH*LOGI_LED_BITMAP_HEIGHT*LOGI_LED_BITMAP_BYTES_PER_KEY)

const GUID GUID_NULL = { 0, 0, 0,{ 0, 0, 0, 0, 0, 0, 0, 0 } };

typedef enum
{
	G1 = -7,
	G2 = -6,
	G3 = -5,
	G4 = -4,
	G5 = -3,
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
	else if (rzrow == 1 && rzcolumn == 0)
		return Logitech_keyboardBitmapKeys::G1;
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
	else if (rzrow == 2 && rzcolumn == 0)
		return Logitech_keyboardBitmapKeys::G2;
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
	else if (rzrow == 3 && rzcolumn == 0)
		return Logitech_keyboardBitmapKeys::G3;
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
	else if (rzrow == 4 && rzcolumn == 0)
		return Logitech_keyboardBitmapKeys::G4;
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
	else if (rzrow == 5 && rzcolumn == 0)
		return Logitech_keyboardBitmapKeys::G5;
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
	else if (rzrow == 5 && rzcolumn == 12)
		return Logitech_keyboardBitmapKeys::BITLOC_RIGHT_WINDOWS;
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

typedef struct WRAPPER_EFFECT
{
	unsigned char bitmap[LOGI_LED_BITMAP_SIZE] = { NULL };
	unsigned char mpad[56] = { NULL };
	unsigned char logo[4] = { NULL };
	unsigned char g1[4] = { NULL };
	unsigned char g2[4] = { NULL };
	unsigned char g3[4] = { NULL };
	unsigned char g4[4] = { NULL };
	unsigned char g5[4] = { NULL };
	unsigned char peripheral[4] = { NULL };
	std::string command_cargo = "";
} WRAPPER_EFFECT;


HANDLE hPipe = INVALID_HANDLE_VALUE;
static bool isInitialized = false;
static bool requiresUpdate = true;

static unsigned char current_bitmap[LOGI_LED_BITMAP_SIZE];
static unsigned char current_mpad[56];
static unsigned char current_logo[4];
static unsigned char current_g1[4];
static unsigned char current_g2[4];
static unsigned char current_g3[4];
static unsigned char current_g4[4];
static unsigned char current_g5[4];
static unsigned char current_peripheral[4];
struct GUIDComparer
{
	bool operator()(const GUID & Left, const GUID & Right) const
	{
		// comparison logic goes here
		return memcmp(&Left, &Right, sizeof(Right)) < 0;
	}
};

static std::map<GUID, WRAPPER_EFFECT, GUIDComparer> effects;

static std::string program_name;

void write_text_to_log_file(const std::string &text)
{
	/* std::ofstream out("output.txt", std::ios_base::app);
	out << text;
	out.close(); */

}

BOOL WINAPI DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		write_text_to_log_file("DLL_PROCESS_ATTACH");
	case DLL_THREAD_ATTACH:
		write_text_to_log_file("DLL_THREAD_ATTACH");
	case DLL_THREAD_DETACH:
		write_text_to_log_file("DLL_THREAD_DETACH");
	case DLL_PROCESS_DETACH:
		write_text_to_log_file("DLL_PROCESS_DETACH");
		break;
	}
	return TRUE;
}

bool __fastcall WriteToPipe(WRAPPER_EFFECT effect)
{
	if (!isInitialized)
		return false;

	if (!requiresUpdate)
		return true;
	else
		requiresUpdate = false;

	//Create JSON
	std::stringstream ss;

	ss << '{';
	ss << "\"provider\": {\"name\": \"" << program_name << "\", \"appid\": 0},";
	ss << effect.command_cargo << ',';
	ss << "\"bitmap\": [";
	for (int bitm_pos = 0; bitm_pos < LOGI_LED_BITMAP_SIZE; bitm_pos += 4)
	{
		if (effect.bitmap[bitm_pos + 3] != NULL)
		{
			current_bitmap[bitm_pos] = effect.bitmap[bitm_pos];
			current_bitmap[bitm_pos + 1] = effect.bitmap[bitm_pos + 1];
			current_bitmap[bitm_pos + 2] = effect.bitmap[bitm_pos + 2];
			current_bitmap[bitm_pos + 3] = effect.bitmap[bitm_pos + 3];
		}

		ss << (int)(((int)current_bitmap[bitm_pos + 2] << 16) | ((int)current_bitmap[bitm_pos + 1] << 8) | ((int)current_bitmap[bitm_pos]));

		if (bitm_pos + 4 < LOGI_LED_BITMAP_SIZE)
			ss << ',';
	}
	ss << "],";

	ss << "\"extra_keys\": {";
	ss << "\"logo\": ";
	if (effect.logo[3] != NULL)
	{
		current_logo[0] = effect.logo[0];
		current_logo[1] = effect.logo[1];
		current_logo[2] = effect.logo[2];
		current_logo[3] = effect.logo[3];
	}

	ss << (int)(((int)current_logo[2] << 16) | ((int)current_logo[1] << 8) | ((int)current_logo[0]));

	ss << ",";
	ss << "\"G1\": ";
	if (effect.g1[3] != NULL)
	{
		current_g1[0] = effect.g1[0];
		current_g1[1] = effect.g1[1];
		current_g1[2] = effect.g1[2];
		current_g1[3] = effect.g1[3];
	}

	ss << (int)(((int)current_g1[2] << 16) | ((int)current_g1[1] << 8) | ((int)current_g1[0]));

	ss << ",";
	ss << "\"G2\": ";
	if (effect.g2[3] != NULL)
	{
		current_g2[0] = effect.g2[0];
		current_g2[1] = effect.g2[1];
		current_g2[2] = effect.g2[2];
		current_g2[3] = effect.g2[3];
	}

	ss << (int)(((int)current_g2[2] << 16) | ((int)current_g2[1] << 8) | ((int)current_g2[0]));

	ss << ",";
	ss << "\"G3\": ";
	if (effect.g3[3] != NULL)
	{
		current_g3[0] = effect.g3[0];
		current_g3[1] = effect.g3[1];
		current_g3[2] = effect.g3[2];
		current_g3[3] = effect.g3[3];
	}

	ss << (int)(((int)current_g3[2] << 16) | ((int)current_g3[1] << 8) | ((int)current_g3[0]));

	ss << ",";
	ss << "\"G4\": ";
	if (effect.g4[3] != NULL)
	{
		current_g4[0] = effect.g4[0];
		current_g4[1] = effect.g4[1];
		current_g4[2] = effect.g4[2];
		current_g4[3] = effect.g4[3];
	}

	ss << (int)(((int)current_g4[2] << 16) | ((int)current_g4[1] << 8) | ((int)current_g4[0]));

	ss << ",";
	ss << "\"G5\": ";
	if (effect.g5[3] != NULL)
	{
		current_g5[0] = effect.g5[0];
		current_g5[1] = effect.g5[1];
		current_g5[2] = effect.g5[2];
		current_g5[3] = effect.g5[3];
	}

	ss << (int)(((int)current_g5[2] << 16) | ((int)current_g5[1] << 8) | ((int)current_g5[0]));

	ss << ",";
	ss << "\"peripheral\": ";
	if (effect.peripheral[3] != NULL)
	{
		current_peripheral[0] = effect.peripheral[0];
		current_peripheral[1] = effect.peripheral[1];
		current_peripheral[2] = effect.peripheral[2];
		current_peripheral[3] = effect.peripheral[3];
	}

	ss << (int)(((int)current_peripheral[2] << 16) | ((int)current_peripheral[1] << 8) | ((int)current_peripheral[0]));
	ss << ",";
	int index = 0;
	for (int mpadLED = 0; mpadLED < 15; mpadLED++)
	{
		ss << "\"mousepad";
		ss << mpadLED << "\": ";

		if (effect.mpad[index + 3] != NULL)
		{
			current_mpad[index] = effect.mpad[index];
			current_mpad[index + 1] = effect.mpad[index + 1];
			current_mpad[index + 2] = effect.mpad[index + 2];
			current_mpad[index + 3] = effect.mpad[index + 3];
		}

		ss << (int)(((int)current_mpad[index + 2] << 16) | ((int)current_mpad[index + 1] << 8) | ((int)current_mpad[index]));
		index = index + 4;

		if (mpadLED < 14)
			ss << ',';
	}
	ss << "";
	ss << '}';


	ss << '}';

	ss << "\r\n";

	if (hPipe == NULL || hPipe == INVALID_HANDLE_VALUE)
	{
		//Try to gestore handle
		//Connect to the server pipe using CreateFile()
		hPipe = CreateFile(
			PIPE_NAME,   // pipe name 
			GENERIC_WRITE,  // write access 
			0,              // no sharing 
			NULL,           // default security attributes
			OPEN_EXISTING,  // opens existing pipe 
			0,              // default attributes 
			NULL);          // no template file 

		if (hPipe == NULL || hPipe == INVALID_HANDLE_VALUE)
		{
			DWORD last_error = GetLastError();

			switch (last_error)
			{
			case ERROR_PIPE_BUSY:
				write_text_to_log_file("Pipe error, ERROR_PIPE_BUSY");
				break;
			case ERROR_PIPE_CONNECTED:
				write_text_to_log_file("Pipe error, ERROR_PIPE_CONNECTED");
				break;
			case ERROR_PIPE_LISTENING:
				write_text_to_log_file("Pipe error, ERROR_PIPE_LISTENING");
				break;
			case ERROR_PIPE_LOCAL:
				write_text_to_log_file("Pipe error, ERROR_PIPE_LOCAL");
				break;
			case ERROR_PIPE_NOT_CONNECTED:
				write_text_to_log_file("Pipe error, ERROR_PIPE_NOT_CONNECTED");
				break;
			default:
				write_text_to_log_file("Non-pipe related error");
				break;
			}

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

WRAPPER_EFFECT HandleKeyboardEffect(ChromaSDK::Keyboard::EFFECT_TYPE Effect, PRZPARAM pParam)
{
	WRAPPER_EFFECT return_effect;
	std::stringstream additional_effect_data;

	additional_effect_data << ',';

	if (Effect == ChromaSDK::Keyboard::CHROMA_STATIC)
	{
		struct ChromaSDK::Keyboard::STATIC_EFFECT_TYPE *static_effect = (struct ChromaSDK::Keyboard::STATIC_EFFECT_TYPE *)pParam;
		if (static_effect != NULL)
		{
			unsigned char blue = GetBValue(static_effect->Color);
			unsigned char green = GetGValue(static_effect->Color);
			unsigned char red = GetRValue(static_effect->Color);

			for (int colorset = 0; colorset < LOGI_LED_BITMAP_SIZE; colorset += 4)
			{
				if (return_effect.bitmap[colorset] != blue ||
					return_effect.bitmap[colorset + 1] != green ||
					return_effect.bitmap[colorset + 2] != red
					)
					requiresUpdate = true;

				return_effect.bitmap[colorset] = blue;
				return_effect.bitmap[colorset + 1] = green;
				return_effect.bitmap[colorset + 2] = red;
				return_effect.bitmap[colorset + 3] = (char)255;
			}

			additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_STATIC" << "\"";
		}
	}
	else if (Effect == ChromaSDK::Keyboard::CHROMA_NONE)
	{
		for (int colorset = 0; colorset < LOGI_LED_BITMAP_SIZE; colorset += 4)
		{
			if (return_effect.bitmap[colorset] != 0 ||
				return_effect.bitmap[colorset + 1] != 0 ||
				return_effect.bitmap[colorset + 2] != 0
				)
				requiresUpdate = true;

			return_effect.bitmap[colorset] = (char)0;
			return_effect.bitmap[colorset + 1] = (char)0;
			return_effect.bitmap[colorset + 2] = (char)0;
			return_effect.bitmap[colorset + 3] = (char)255;
		}

		//Logo
		if (current_logo[0] != 0 ||
			current_logo[1] != 0 ||
			current_logo[2] != 0
			)
			requiresUpdate = true;

		return_effect.logo[0] = 0;
		return_effect.logo[1] = 0;
		return_effect.logo[2] = 0;
		return_effect.logo[3] = (char)255;

		//G Keys
		if (current_g1[0] != 0 ||
			current_g1[1] != 0 ||
			current_g1[2] != 0
			)
			requiresUpdate = true;

		return_effect.g1[0] = 0;
		return_effect.g1[1] = 0;
		return_effect.g1[2] = 0;
		return_effect.g1[3] = (char)255;

		if (current_g2[0] != 0 ||
			current_g2[1] != 0 ||
			current_g2[2] != 0
			)
			requiresUpdate = true;

		return_effect.g2[0] = 0;
		return_effect.g2[1] = 0;
		return_effect.g2[2] = 0;
		return_effect.g2[3] = (char)255;

		if (current_g3[0] != 0 ||
			current_g3[1] != 0 ||
			current_g3[2] != 0
			)
			requiresUpdate = true;

		return_effect.g3[0] = 0;
		return_effect.g3[1] = 0;
		return_effect.g3[2] = 0;
		return_effect.g3[3] = (char)255;

		if (current_g4[0] != 0 ||
			current_g4[1] != 0 ||
			current_g4[2] != 0
			)
			requiresUpdate = true;

		return_effect.g4[0] = 0;
		return_effect.g4[1] = 0;
		return_effect.g4[2] = 0;
		return_effect.g4[3] = (char)255;

		if (current_g5[0] != 0 ||
			current_g5[1] != 0 ||
			current_g5[2] != 0
			)
			requiresUpdate = true;

		return_effect.g5[0] = 0;
		return_effect.g5[1] = 0;
		return_effect.g5[2] = 0;
		return_effect.g5[3] = (char)255;

		additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_NONE" << "\"";
	}
	else if (Effect == ChromaSDK::Keyboard::CHROMA_CUSTOM)
	{
		struct ChromaSDK::Keyboard::CUSTOM_EFFECT_TYPE *custom_effect = (struct ChromaSDK::Keyboard::CUSTOM_EFFECT_TYPE *)pParam;
		if (custom_effect != NULL)
		{
			for (int row = 0; row < ChromaSDK::Keyboard::MAX_ROW; row++)
			{
				for (int col = 0; col < ChromaSDK::Keyboard::MAX_COLUMN; col++)
				{
					Logitech_keyboardBitmapKeys bitmap_pos = ToLogitechBitmap(row, col);

					if (bitmap_pos != Logitech_keyboardBitmapKeys::UNKNOWN)
					{
						unsigned char blue = GetBValue(custom_effect->Color[row][col]);
						unsigned char green = GetGValue(custom_effect->Color[row][col]);
						unsigned char red = GetRValue(custom_effect->Color[row][col]);


						if (bitmap_pos == Logitech_keyboardBitmapKeys::LOGO)
						{
							if (current_logo[0] != blue ||
								current_logo[1] != green ||
								current_logo[2] != red
								)
								requiresUpdate = true;

							return_effect.logo[0] = blue;
							return_effect.logo[1] = green;
							return_effect.logo[2] = red;
							return_effect.logo[3] = (char)255;
						}
						else if (bitmap_pos == Logitech_keyboardBitmapKeys::G1)
						{
							if (current_g1[0] != blue ||
								current_g1[1] != green ||
								current_g1[2] != red
								)
								requiresUpdate = true;

							return_effect.g1[0] = blue;
							return_effect.g1[1] = green;
							return_effect.g1[2] = red;
							return_effect.g1[3] = (char)255;
						}
						else if (bitmap_pos == Logitech_keyboardBitmapKeys::G2)
						{
							if (current_g2[0] != blue ||
								current_g2[1] != green ||
								current_g2[2] != red
								)
								requiresUpdate = true;

							return_effect.g2[0] = blue;
							return_effect.g2[1] = green;
							return_effect.g2[2] = red;
							return_effect.g2[3] = (char)255;
						}
						else if (bitmap_pos == Logitech_keyboardBitmapKeys::G3)
						{
							if (current_g3[0] != blue ||
								current_g3[1] != green ||
								current_g3[2] != red
								)
								requiresUpdate = true;

							return_effect.g3[0] = blue;
							return_effect.g3[1] = green;
							return_effect.g3[2] = red;
							return_effect.g3[3] = (char)255;
						}
						else if (bitmap_pos == Logitech_keyboardBitmapKeys::G4)
						{
							if (current_g4[0] != blue ||
								current_g4[1] != green ||
								current_g4[2] != red
								)
								requiresUpdate = true;

							return_effect.g4[0] = blue;
							return_effect.g4[1] = green;
							return_effect.g4[2] = red;
							return_effect.g4[3] = (char)255;
						}
						else if (bitmap_pos == Logitech_keyboardBitmapKeys::G5)
						{
							if (current_g5[0] != blue ||
								current_g5[1] != green ||
								current_g5[2] != red
								)
								requiresUpdate = true;

							return_effect.g5[0] = blue;
							return_effect.g5[1] = green;
							return_effect.g5[2] = red;
							return_effect.g5[3] = (char)255;
						}
						else
						{
							if (current_bitmap[(int)bitmap_pos] != blue ||
								current_bitmap[(int)bitmap_pos + 1] != green ||
								current_bitmap[(int)bitmap_pos + 2] != red
								)
								requiresUpdate = true;

							return_effect.bitmap[(int)bitmap_pos] = blue;
							return_effect.bitmap[(int)bitmap_pos + 1] = green;
							return_effect.bitmap[(int)bitmap_pos + 2] = red;
							return_effect.bitmap[(int)bitmap_pos + 3] = (char)255;
						}
					}
				}
			}

			additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_CUSTOM" << "\"";
		}
	}
	else if (Effect == ChromaSDK::Keyboard::CHROMA_CUSTOM_KEY)
	{
		write_text_to_log_file("\nChroma Custom Key ");
		struct ChromaSDK::Keyboard::CUSTOM_KEY_EFFECT_TYPE *custom_effect = (struct ChromaSDK::Keyboard::CUSTOM_KEY_EFFECT_TYPE *)pParam;
		if (custom_effect != NULL)
		{
			for (int row = 0; row < ChromaSDK::Keyboard::MAX_ROW; row++)
			{
				for (int col = 0; col < ChromaSDK::Keyboard::MAX_COLUMN; col++)
				{
					Logitech_keyboardBitmapKeys bitmap_pos = ToLogitechBitmap(row, col);

					if (bitmap_pos != Logitech_keyboardBitmapKeys::UNKNOWN)
					{
						unsigned char blue = GetBValue(custom_effect->Key[row][col]);
						if (blue == 0)
						{
							blue = GetBValue(custom_effect->Color[row][col]);
						}
						unsigned char green = GetGValue(custom_effect->Key[row][col]);
						if (green == 0)
						{
							green = GetGValue(custom_effect->Color[row][col]);
						}
						unsigned char red = GetRValue(custom_effect->Key[row][col]);
						if (red == 0)
						{
							red = GetRValue(custom_effect->Color[row][col]);
						}


						if (bitmap_pos == Logitech_keyboardBitmapKeys::LOGO)
						{
							if (current_logo[0] != blue ||
								current_logo[1] != green ||
								current_logo[2] != red
								)
								requiresUpdate = true;

							return_effect.logo[0] = blue;
							return_effect.logo[1] = green;
							return_effect.logo[2] = red;
							return_effect.logo[3] = (char)255;
						}
						else if (bitmap_pos == Logitech_keyboardBitmapKeys::G1)
						{
							if (current_g1[0] != blue ||
								current_g1[1] != green ||
								current_g1[2] != red
								)
								requiresUpdate = true;

							return_effect.g1[0] = blue;
							return_effect.g1[1] = green;
							return_effect.g1[2] = red;
							return_effect.g1[3] = (char)255;
						}
						else if (bitmap_pos == Logitech_keyboardBitmapKeys::G2)
						{
							if (current_g2[0] != blue ||
								current_g2[1] != green ||
								current_g2[2] != red
								)
								requiresUpdate = true;

							return_effect.g2[0] = blue;
							return_effect.g2[1] = green;
							return_effect.g2[2] = red;
							return_effect.g2[3] = (char)255;
						}
						else if (bitmap_pos == Logitech_keyboardBitmapKeys::G3)
						{
							if (current_g3[0] != blue ||
								current_g3[1] != green ||
								current_g3[2] != red
								)
								requiresUpdate = true;

							return_effect.g3[0] = blue;
							return_effect.g3[1] = green;
							return_effect.g3[2] = red;
							return_effect.g3[3] = (char)255;
						}
						else if (bitmap_pos == Logitech_keyboardBitmapKeys::G4)
						{
							if (current_g4[0] != blue ||
								current_g4[1] != green ||
								current_g4[2] != red
								)
								requiresUpdate = true;

							return_effect.g4[0] = blue;
							return_effect.g4[1] = green;
							return_effect.g4[2] = red;
							return_effect.g4[3] = (char)255;
						}
						else if (bitmap_pos == Logitech_keyboardBitmapKeys::G5)
						{
							if (current_g5[0] != blue ||
								current_g5[1] != green ||
								current_g5[2] != red
								)
								requiresUpdate = true;

							return_effect.g5[0] = blue;
							return_effect.g5[1] = green;
							return_effect.g5[2] = red;
							return_effect.g5[3] = (char)255;
						}
						else
						{
							if (current_bitmap[(int)bitmap_pos] != blue ||
								current_bitmap[(int)bitmap_pos + 1] != green ||
								current_bitmap[(int)bitmap_pos + 2] != red
								)
								requiresUpdate = true;

							return_effect.bitmap[(int)bitmap_pos] = blue;
							return_effect.bitmap[(int)bitmap_pos + 1] = green;
							return_effect.bitmap[(int)bitmap_pos + 2] = red;
							return_effect.bitmap[(int)bitmap_pos + 3] = (char)255;
						}
					}
				}
			}

			additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_CUSTOM" << "\"";
		}
	}
	else if (Effect == ChromaSDK::Keyboard::CHROMA_BREATHING)
	{
		struct ChromaSDK::Keyboard::BREATHING_EFFECT_TYPE *breathing_effect = (struct ChromaSDK::Keyboard::BREATHING_EFFECT_TYPE *)pParam;
		if (breathing_effect != NULL)
		{
			additional_effect_data << "\"red_start\": " << "\"" << GetRValue(breathing_effect->Color1) << "\"" << ',';
			additional_effect_data << "\"green_start\": " << "\"" << GetGValue(breathing_effect->Color1) << "\"" << ',';
			additional_effect_data << "\"blue_start\": " << "\"" << GetBValue(breathing_effect->Color1) << "\"" << ',';
			additional_effect_data << "\"red_end\": " << "\"" << GetRValue(breathing_effect->Color2) << "\"" << ',';
			additional_effect_data << "\"green_end\": " << "\"" << GetGValue(breathing_effect->Color2) << "\"" << ',';
			additional_effect_data << "\"blue_end\": " << "\"" << GetBValue(breathing_effect->Color2) << "\"" << ',';
			additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_BREATHING" << "\"" << ',';

			switch (breathing_effect->Type)
			{
			case ChromaSDK::Keyboard::BREATHING_EFFECT_TYPE::Type::TWO_COLORS:
				additional_effect_data << "\"effect_config\": " << "\"" << "TWO_COLORS" << "\"";
				break;
			case ChromaSDK::Keyboard::BREATHING_EFFECT_TYPE::Type::RANDOM_COLORS:
				additional_effect_data << "\"effect_config\": " << "\"" << "RANDOM_COLORS" << "\"";
				break;
			default:
				additional_effect_data << "\"effect_config\": " << "\"" << "INVALID" << "\"";
				break;
			}
		}
	}
	else if (Effect == ChromaSDK::Keyboard::CHROMA_REACTIVE)
	{
		struct ChromaSDK::Keyboard::REACTIVE_EFFECT_TYPE *reactive_effect = (struct ChromaSDK::Keyboard::REACTIVE_EFFECT_TYPE *)pParam;
		if (reactive_effect != NULL)
		{
			additional_effect_data << "\"red_start\": " << "\"" << GetRValue(reactive_effect->Color) << "\"" << ',';
			additional_effect_data << "\"green_start\": " << "\"" << GetGValue(reactive_effect->Color) << "\"" << ',';
			additional_effect_data << "\"blue_start\": " << "\"" << GetBValue(reactive_effect->Color) << "\"" << ',';
			additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_REACTIVE" << "\"" << ',';

			switch (reactive_effect->Duration)
			{
			case ChromaSDK::Keyboard::REACTIVE_EFFECT_TYPE::Duration::DURATION_NONE:
				additional_effect_data << "\"effect_config\": " << "\"" << "NONE" << "\"";
				break;
			case ChromaSDK::Keyboard::REACTIVE_EFFECT_TYPE::Duration::DURATION_SHORT:
				additional_effect_data << "\"effect_config\": " << "\"" << "SHORT" << "\"";
				break;
			case ChromaSDK::Keyboard::REACTIVE_EFFECT_TYPE::Duration::DURATION_MEDIUM:
				additional_effect_data << "\"effect_config\": " << "\"" << "MEDIUM" << "\"";
				break;
			case ChromaSDK::Keyboard::REACTIVE_EFFECT_TYPE::Duration::DURATION_LONG:
				additional_effect_data << "\"effect_config\": " << "\"" << "LONG" << "\"";
				break;
			default:
				additional_effect_data << "\"effect_config\": " << "\"" << "INVALID" << "\"";
				break;
			}
		}
	}
	else if (Effect == ChromaSDK::Keyboard::CHROMA_SPECTRUMCYCLING)
	{
		additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_SPECTRUMCYCLING" << "\"";
	}
	else if (Effect == ChromaSDK::Keyboard::CHROMA_WAVE)
	{
		struct ChromaSDK::Keyboard::WAVE_EFFECT_TYPE *wave_effect = (struct ChromaSDK::Keyboard::WAVE_EFFECT_TYPE *)pParam;
		if (wave_effect != NULL)
		{
			additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_WAVE" << "\"" << ',';

			switch (wave_effect->Direction)
			{
			case ChromaSDK::Keyboard::WAVE_EFFECT_TYPE::DIRECTION_NONE:
				additional_effect_data << "\"effect_config\": " << "\"" << "NONE" << "\"";
				break;
			case ChromaSDK::Keyboard::WAVE_EFFECT_TYPE::DIRECTION_LEFT_TO_RIGHT:
				additional_effect_data << "\"effect_config\": " << "\"" << "LEFT_TO_RIGHT" << "\"";
				break;
			case ChromaSDK::Keyboard::WAVE_EFFECT_TYPE::DIRECTION_RIGHT_TO_LEFT:
				additional_effect_data << "\"effect_config\": " << "\"" << "RIGHT_TO_LEFT" << "\"";
				break;
			default:
				additional_effect_data << "\"effect_config\": " << "\"" << "INVALID" << "\"";
				break;
			}
		}
	}
	else if (Effect == ChromaSDK::Keyboard::CHROMA_RESERVED)
	{
		additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_RESERVED" << "\"";
	}
	else
	{
		additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_INVALID" << "\"";
	}

	std::stringstream ss;
	ss << "\"command\": " << "\"CreateKeyboardEffect\"" << ',';
	ss << "\"command_data\": {";

	ss << "\"custom_mode\": " << 0;
	ss << additional_effect_data.str();
	ss << '}';

	return_effect.command_cargo = ss.str();

	return return_effect;
}

WRAPPER_EFFECT HandleMouseEffect(ChromaSDK::Mouse::EFFECT_TYPE Effect, PRZPARAM pParam)
{
	WRAPPER_EFFECT return_effect;
	std::stringstream additional_effect_data;

	additional_effect_data << ',';

	switch (Effect)
	{
		//case ChromaSDK::Mouse::CHROMA_NONE:
		//	break;
		//case ChromaSDK::Mouse::CHROMA_BLINKING:
		//	break;
		//case ChromaSDK::Mouse::CHROMA_BREATHING:
		//	break;
		//case ChromaSDK::Mouse::CHROMA_CUSTOM:
		//	break;
		//case ChromaSDK::Mouse::CHROMA_REACTIVE:
		//	break;
		//case ChromaSDK::Mouse::CHROMA_SPECTRUMCYCLING:
		//	break;
		//case ChromaSDK::Mouse::CHROMA_STATIC:
		//	break;
	case ChromaSDK::Mouse::CHROMA_WAVE:
		break;
	case ChromaSDK::Mouse::CHROMA_CUSTOM2:
		break;
	case ChromaSDK::Mouse::CHROMA_INVALID:
		break;
	default:
		break;
	}

	if (Effect == ChromaSDK::Mouse::CHROMA_STATIC)
	{
		struct ChromaSDK::Mouse::STATIC_EFFECT_TYPE *static_effect = (struct ChromaSDK::Mouse::STATIC_EFFECT_TYPE *)pParam;
		if (static_effect != NULL)
		{
			unsigned char blue = GetBValue(static_effect->Color);
			unsigned char green = GetGValue(static_effect->Color);
			unsigned char red = GetRValue(static_effect->Color);

			if (static_effect->LEDId == ChromaSDK::Mouse::RZLED::RZLED_LOGO || static_effect->LEDId == ChromaSDK::Mouse::RZLED::RZLED_ALL)
			{
				if (current_peripheral[0] != blue ||
					current_peripheral[1] != green ||
					current_peripheral[2] != red
					)
					requiresUpdate = true;

				return_effect.peripheral[0] = blue;
				return_effect.peripheral[1] = green;
				return_effect.peripheral[2] = red;
				return_effect.peripheral[3] = (char)255;
			}

			additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_STATIC" << "\"";
		}
	}
	else if (Effect == ChromaSDK::Mouse::CHROMA_BLINKING)
	{
		struct ChromaSDK::Mouse::BLINKING_EFFECT_TYPE *blinking_effect = (struct ChromaSDK::Mouse::BLINKING_EFFECT_TYPE *)pParam;
		if (blinking_effect != NULL)
		{
			additional_effect_data << "\"red_start\": " << "\"" << GetRValue(blinking_effect->Color) << "\"" << ',';
			additional_effect_data << "\"green_start\": " << "\"" << GetGValue(blinking_effect->Color) << "\"" << ',';
			additional_effect_data << "\"blue_start\": " << "\"" << GetBValue(blinking_effect->Color) << "\"" << ',';
			additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_BLINKING" << "\"";
		}
	}
	else if (Effect == ChromaSDK::Mouse::CHROMA_NONE)
	{
		if (current_peripheral[0] != 0 ||
			current_peripheral[1] != 0 ||
			current_peripheral[2] != 0
			)
			requiresUpdate = true;

		return_effect.peripheral[0] = (char)0;
		return_effect.peripheral[1] = (char)0;
		return_effect.peripheral[2] = (char)0;
		return_effect.peripheral[3] = (char)255;

		additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_NONE" << "\"";
	}
	else if (Effect == ChromaSDK::Mouse::CHROMA_CUSTOM)
	{
		struct ChromaSDK::Mouse::CUSTOM_EFFECT_TYPE *custom_effect = (struct ChromaSDK::Mouse::CUSTOM_EFFECT_TYPE *)pParam;
		if (custom_effect != NULL)
		{
			unsigned char blue = GetBValue(custom_effect->Color[ChromaSDK::Mouse::RZLED::RZLED_LOGO]);
			unsigned char green = GetGValue(custom_effect->Color[ChromaSDK::Mouse::RZLED::RZLED_LOGO]);
			unsigned char red = GetRValue(custom_effect->Color[ChromaSDK::Mouse::RZLED::RZLED_LOGO]);

			if (current_peripheral[0] != blue ||
				current_peripheral[1] != green ||
				current_peripheral[2] != red
				)
				requiresUpdate = true;

			return_effect.peripheral[0] = blue;
			return_effect.peripheral[1] = green;
			return_effect.peripheral[2] = red;
			return_effect.peripheral[3] = (char)255;

			additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_CUSTOM" << "\"";
		}
	}
	else if (Effect == ChromaSDK::Mouse::CHROMA_BREATHING)
	{
		struct ChromaSDK::Mouse::BREATHING_EFFECT_TYPE *breathing_effect = (struct ChromaSDK::Mouse::BREATHING_EFFECT_TYPE *)pParam;
		if (breathing_effect != NULL)
		{
			additional_effect_data << "\"red_start\": " << "\"" << GetRValue(breathing_effect->Color1) << "\"" << ',';
			additional_effect_data << "\"green_start\": " << "\"" << GetGValue(breathing_effect->Color1) << "\"" << ',';
			additional_effect_data << "\"blue_start\": " << "\"" << GetBValue(breathing_effect->Color1) << "\"" << ',';
			additional_effect_data << "\"red_end\": " << "\"" << GetRValue(breathing_effect->Color2) << "\"" << ',';
			additional_effect_data << "\"green_end\": " << "\"" << GetGValue(breathing_effect->Color2) << "\"" << ',';
			additional_effect_data << "\"blue_end\": " << "\"" << GetBValue(breathing_effect->Color2) << "\"" << ',';
			additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_BREATHING" << "\"" << ',';

			switch (breathing_effect->Type)
			{
			case ChromaSDK::Mouse::BREATHING_EFFECT_TYPE::Type::ONE_COLOR:
				additional_effect_data << "\"effect_config\": " << "\"" << "TWO_COLORS" << "\"";
				break;
			case ChromaSDK::Mouse::BREATHING_EFFECT_TYPE::Type::TWO_COLORS:
				additional_effect_data << "\"effect_config\": " << "\"" << "TWO_COLORS" << "\"";
				break;
			case ChromaSDK::Mouse::BREATHING_EFFECT_TYPE::Type::RANDOM_COLORS:
				additional_effect_data << "\"effect_config\": " << "\"" << "RANDOM_COLORS" << "\"";
				break;
			default:
				additional_effect_data << "\"effect_config\": " << "\"" << "INVALID" << "\"";
				break;
			}
		}
	}
	else if (Effect == ChromaSDK::Mouse::CHROMA_REACTIVE)
	{
		struct ChromaSDK::Mouse::REACTIVE_EFFECT_TYPE *reactive_effect = (struct ChromaSDK::Mouse::REACTIVE_EFFECT_TYPE *)pParam;
		if (reactive_effect != NULL)
		{
			additional_effect_data << "\"red_start\": " << "\"" << GetRValue(reactive_effect->Color) << "\"" << ',';
			additional_effect_data << "\"green_start\": " << "\"" << GetGValue(reactive_effect->Color) << "\"" << ',';
			additional_effect_data << "\"blue_start\": " << "\"" << GetBValue(reactive_effect->Color) << "\"" << ',';
			additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_REACTIVE" << "\"" << ',';

			switch (reactive_effect->Duration)
			{
			case ChromaSDK::Keyboard::REACTIVE_EFFECT_TYPE::Duration::DURATION_SHORT:
				additional_effect_data << "\"effect_config\": " << "\"" << "SHORT" << "\"";
				break;
			case ChromaSDK::Keyboard::REACTIVE_EFFECT_TYPE::Duration::DURATION_MEDIUM:
				additional_effect_data << "\"effect_config\": " << "\"" << "MEDIUM" << "\"";
				break;
			case ChromaSDK::Keyboard::REACTIVE_EFFECT_TYPE::Duration::DURATION_LONG:
				additional_effect_data << "\"effect_config\": " << "\"" << "LONG" << "\"";
				break;
			default:
				additional_effect_data << "\"effect_config\": " << "\"" << "NONE" << "\"";
				break;
			}
		}
	}
	else if (Effect == ChromaSDK::Mouse::CHROMA_SPECTRUMCYCLING)
	{
		additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_SPECTRUMCYCLING" << "\"";
	}
	else if (Effect == ChromaSDK::Mouse::CHROMA_WAVE)
	{
		additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_WAVE" << "\"";
	}
	else if (Effect == ChromaSDK::Mouse::CHROMA_CUSTOM2)
	{
		struct ChromaSDK::Mouse::CUSTOM_EFFECT_TYPE2 *custom_effect = (struct ChromaSDK::Mouse::CUSTOM_EFFECT_TYPE2 *)pParam;
		if (custom_effect != NULL)
		{
			unsigned char blue = GetBValue(custom_effect->Color[HIBYTE(ChromaSDK::Mouse::RZLED2_LOGO)][LOBYTE(ChromaSDK::Mouse::RZLED2_LOGO)]);
			unsigned char green = GetGValue(custom_effect->Color[HIBYTE(ChromaSDK::Mouse::RZLED2_LOGO)][LOBYTE(ChromaSDK::Mouse::RZLED2_LOGO)]);
			unsigned char red = GetRValue(custom_effect->Color[HIBYTE(ChromaSDK::Mouse::RZLED2_LOGO)][LOBYTE(ChromaSDK::Mouse::RZLED2_LOGO)]);

			if (current_peripheral[0] != blue ||
				current_peripheral[1] != green ||
				current_peripheral[2] != red
				)
				requiresUpdate = true;

			return_effect.peripheral[0] = blue;
			return_effect.peripheral[1] = green;
			return_effect.peripheral[2] = red;
			return_effect.peripheral[3] = (char)255;

			additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_CUSTOM2" << "\"";
		}
	}
	else
	{
		additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_INVALID" << "\"";
	}

	std::stringstream ss;
	ss << "\"command\": " << "\"CreateMouseEffect\"" << ',';
	ss << "\"command_data\": {";

	ss << "\"custom_mode\": " << 0;
	ss << additional_effect_data.str();
	ss << '}';

	return_effect.command_cargo = ss.str();

	return return_effect;
}

WRAPPER_EFFECT HandleMousepadEffect(ChromaSDK::Mousepad::EFFECT_TYPE Effect, PRZPARAM pParam)
{
	WRAPPER_EFFECT return_effect;
	std::stringstream additional_effect_data;

	additional_effect_data << ',';

	switch (Effect)
	{
	case ChromaSDK::Mousepad::CHROMA_WAVE:
		break;
	case ChromaSDK::Mousepad::CHROMA_INVALID:
		break;
	default:
		break;
	}
	if (Effect == ChromaSDK::Mousepad::CHROMA_STATIC)
	{
		struct ChromaSDK::Mousepad::STATIC_EFFECT_TYPE *static_effect = (struct ChromaSDK::Mousepad::STATIC_EFFECT_TYPE *)pParam;
		if (static_effect != NULL)
		{
			unsigned char blue = GetBValue(static_effect->Color);
			unsigned char green = GetGValue(static_effect->Color);
			unsigned char red = GetRValue(static_effect->Color);
			for (int colorset = 0; colorset < 56; colorset += 4)
			{
				if (current_mpad[colorset] != blue ||
					current_mpad[colorset + 1] != green ||
					current_mpad[colorset + 2] != red
					)
					requiresUpdate = true;

				return_effect.mpad[colorset] = blue;
				return_effect.mpad[colorset + 1] = green;
				return_effect.mpad[colorset + 2] = red;
				return_effect.mpad[colorset + 3] = (char)255;
			}
			additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_STATIC" << "\"";
		}
	}
	else if (Effect == ChromaSDK::Mousepad::CHROMA_NONE)
	{
		for (int colorset = 0; colorset < 56; colorset += 4)
		{
			if (current_mpad[colorset] != 0 ||
				current_mpad[colorset + 1] != 0 ||
				current_mpad[colorset + 2] != 0
				)
				requiresUpdate = true;

			return_effect.mpad[colorset] = 0;
			return_effect.mpad[colorset + 1] = 0;
			return_effect.mpad[colorset + 2] = 0;
			return_effect.mpad[colorset + 3] = (char)255;
		}

		additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_NONE" << "\"";
	}
	else if (Effect == ChromaSDK::Mousepad::CHROMA_CUSTOM)
	{
		struct ChromaSDK::Mousepad::CUSTOM_EFFECT_TYPE *custom_effect = (struct ChromaSDK::Mousepad::CUSTOM_EFFECT_TYPE *)pParam;
		if (custom_effect != NULL)
		{
			int colorset = 0;
			for (int index = 0; index < 15; index++)
			{
				unsigned char blue = GetBValue(custom_effect->Color[index]);
				unsigned char green = GetGValue(custom_effect->Color[index]);
				unsigned char red = GetRValue(custom_effect->Color[index]);

				if (current_mpad[colorset] != blue ||
					current_mpad[colorset + 1] != green ||
					current_mpad[colorset + 2] != red
					)
					requiresUpdate = true;
				return_effect.mpad[colorset] = blue;
				return_effect.mpad[colorset + 1] = green;
				return_effect.mpad[colorset + 2] = red;
				return_effect.mpad[colorset + 3] = (char)255;
				colorset = colorset + 4;
			}

			additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_CUSTOM" << "\"";
		}
	}
	else if (Effect == ChromaSDK::Mousepad::CHROMA_BREATHING)
	{
		struct ChromaSDK::Mousepad::BREATHING_EFFECT_TYPE *breathing_effect = (struct ChromaSDK::Mousepad::BREATHING_EFFECT_TYPE *)pParam;
		if (breathing_effect != NULL)
		{
			additional_effect_data << "\"red_start\": " << "\"" << GetRValue(breathing_effect->Color1) << "\"" << ',';
			additional_effect_data << "\"green_start\": " << "\"" << GetGValue(breathing_effect->Color1) << "\"" << ',';
			additional_effect_data << "\"blue_start\": " << "\"" << GetBValue(breathing_effect->Color1) << "\"" << ',';
			additional_effect_data << "\"red_end\": " << "\"" << GetRValue(breathing_effect->Color2) << "\"" << ',';
			additional_effect_data << "\"green_end\": " << "\"" << GetGValue(breathing_effect->Color2) << "\"" << ',';
			additional_effect_data << "\"blue_end\": " << "\"" << GetBValue(breathing_effect->Color2) << "\"" << ',';
			additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_BREATHING" << "\"" << ',';

			switch (breathing_effect->Type)
			{
			case ChromaSDK::Mousepad::BREATHING_EFFECT_TYPE::Type::TWO_COLORS:
				additional_effect_data << "\"effect_config\": " << "\"" << "TWO_COLORS" << "\"";
				break;
			case ChromaSDK::Mousepad::BREATHING_EFFECT_TYPE::Type::RANDOM_COLORS:
				additional_effect_data << "\"effect_config\": " << "\"" << "RANDOM_COLORS" << "\"";
				break;
			default:
				additional_effect_data << "\"effect_config\": " << "\"" << "INVALID" << "\"";
				break;
			}
		}
	}

	else if (Effect == ChromaSDK::Mousepad::CHROMA_SPECTRUMCYCLING)
	{
		additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_SPECTRUMCYCLING" << "\"";
	}
	else if (Effect == ChromaSDK::Mousepad::CHROMA_WAVE)
	{
		additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_WAVE" << "\"";
	}
	else
	{
		additional_effect_data << "\"effect_type\": " << "\"" << "CHROMA_INVALID" << "\"";
	}

	std::stringstream ss;
	ss << "\"command\": " << "\"CreateMousepadEffect\"" << ',';
	ss << "\"command_data\": {";

	ss << "\"custom_mode\": " << 0;
	ss << additional_effect_data.str();
	ss << '}';

	return_effect.command_cargo = ss.str();

	return return_effect;
}
#ifdef __cplusplus
extern "C" {
#endif

	__declspec(dllexport) RZRESULT Init()
	{
		write_text_to_log_file("Call, Init()");

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

			if (hPipe == NULL || hPipe == INVALID_HANDLE_VALUE)
			{
				DWORD last_error = GetLastError();

				switch (last_error)
				{
				case ERROR_PIPE_BUSY:
					write_text_to_log_file("Pipe error, ERROR_PIPE_BUSY");
					break;
				case ERROR_PIPE_CONNECTED:
					write_text_to_log_file("Pipe error, ERROR_PIPE_CONNECTED");
					break;
				case ERROR_PIPE_LISTENING:
					write_text_to_log_file("Pipe error, ERROR_PIPE_LISTENING");
					break;
				case ERROR_PIPE_LOCAL:
					write_text_to_log_file("Pipe error, ERROR_PIPE_LOCAL");
					break;
				case ERROR_PIPE_NOT_CONNECTED:
					write_text_to_log_file("Pipe error, ERROR_PIPE_NOT_CONNECTED");
					break;
				default:
					write_text_to_log_file("Non-pipe related error");
					break;
				}

				isInitialized = false;
				return RZRESULT_INVALID;
			}
		}
		else
		{
			return RZRESULT_ALREADY_INITIALIZED;
		}

		write_text_to_log_file("Initialized Successfully");

		isInitialized = true;
		return RZRESULT_SUCCESS;
	}

	__declspec(dllexport) RZRESULT UnInit()
	{
		write_text_to_log_file("Call, UnInit()");
		if (isInitialized && (hPipe != NULL && hPipe != INVALID_HANDLE_VALUE))
			CloseHandle(hPipe);

		isInitialized = false;
		return RZRESULT_SUCCESS;
	}

	__declspec(dllexport) RZRESULT CreateEffect(RZDEVICEID DeviceId, ChromaSDK::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID *pEffectId)
	{
		write_text_to_log_file("Call, CreateEffect()");

		if (isInitialized)
		{
			WRAPPER_EFFECT createdEffect;

			if (DeviceId == ChromaSDK::BLACKWIDOW_CHROMA)
			{
				ChromaSDK::Keyboard::EFFECT_TYPE kbType;

				switch (Effect)
				{
				case ChromaSDK::CHROMA_NONE:
					kbType = ChromaSDK::Keyboard::EFFECT_TYPE::CHROMA_NONE;
					break;
				case ChromaSDK::CHROMA_WAVE:
					kbType = ChromaSDK::Keyboard::EFFECT_TYPE::CHROMA_WAVE;
					break;
				case ChromaSDK::CHROMA_SPECTRUMCYCLING:
					kbType = ChromaSDK::Keyboard::EFFECT_TYPE::CHROMA_SPECTRUMCYCLING;
					break;
				case ChromaSDK::CHROMA_BREATHING:
					kbType = ChromaSDK::Keyboard::EFFECT_TYPE::CHROMA_BREATHING;
					break;
				case ChromaSDK::CHROMA_REACTIVE:
					kbType = ChromaSDK::Keyboard::EFFECT_TYPE::CHROMA_REACTIVE;
					break;
				case ChromaSDK::CHROMA_STATIC:
					kbType = ChromaSDK::Keyboard::EFFECT_TYPE::CHROMA_STATIC;
					break;
				case ChromaSDK::CHROMA_CUSTOM:
					kbType = ChromaSDK::Keyboard::EFFECT_TYPE::CHROMA_CUSTOM;
					break;
				case ChromaSDK::CHROMA_RESERVED:
					kbType = ChromaSDK::Keyboard::EFFECT_TYPE::CHROMA_RESERVED;
					break;
				default:
					kbType = ChromaSDK::Keyboard::EFFECT_TYPE::CHROMA_INVALID;
					break;
				}

				createdEffect = HandleKeyboardEffect(kbType, pParam);
			}
			else if (DeviceId == ChromaSDK::DEATHADDER_CHROMA)
			{
				ChromaSDK::Mouse::EFFECT_TYPE mouseType;

				switch (Effect)
				{
				case ChromaSDK::CHROMA_NONE:
					mouseType = ChromaSDK::Mouse::EFFECT_TYPE::CHROMA_NONE;
					break;
				case ChromaSDK::CHROMA_WAVE:
					mouseType = ChromaSDK::Mouse::EFFECT_TYPE::CHROMA_WAVE;
					break;
				case ChromaSDK::CHROMA_SPECTRUMCYCLING:
					mouseType = ChromaSDK::Mouse::EFFECT_TYPE::CHROMA_SPECTRUMCYCLING;
					break;
				case ChromaSDK::CHROMA_BREATHING:
					mouseType = ChromaSDK::Mouse::EFFECT_TYPE::CHROMA_BREATHING;
					break;
				case ChromaSDK::CHROMA_BLINKING:
					mouseType = ChromaSDK::Mouse::EFFECT_TYPE::CHROMA_BLINKING;
					break;
				case ChromaSDK::CHROMA_REACTIVE:
					mouseType = ChromaSDK::Mouse::EFFECT_TYPE::CHROMA_REACTIVE;
					break;
				case ChromaSDK::CHROMA_STATIC:
					mouseType = ChromaSDK::Mouse::EFFECT_TYPE::CHROMA_STATIC;
					break;
				case ChromaSDK::CHROMA_CUSTOM:
					mouseType = ChromaSDK::Mouse::EFFECT_TYPE::CHROMA_CUSTOM2;
					break;
				default:
					mouseType = ChromaSDK::Mouse::EFFECT_TYPE::CHROMA_INVALID;
					break;
				}

				createdEffect = HandleMouseEffect(mouseType, pParam);
			}
			else
			{
				return RZRESULT_SUCCESS;
			}

			if (pEffectId == NULL)
			{
				WriteToPipe(createdEffect);
			}
			else
			{
				if (*pEffectId == GUID_NULL)
					CoCreateGuid(pEffectId);

				effects[*pEffectId] = createdEffect;
			}

			return RZRESULT_SUCCESS;
		}
		else
		{
			return RZRESULT_INVALID;
		}
	}

	__declspec(dllexport) RZRESULT CreateKeyboardEffect(ChromaSDK::Keyboard::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID *pEffectId)
	{
		write_text_to_log_file("Call, CreateKeyboardEffect()");

		if (isInitialized)
		{
			WRAPPER_EFFECT kbEffect = HandleKeyboardEffect(Effect, pParam);

			if (pEffectId == NULL)
			{
				WriteToPipe(kbEffect);
			}
			else
			{
				if (*pEffectId == GUID_NULL)
					CoCreateGuid(pEffectId);

				effects[*pEffectId] = kbEffect;
			}

			return RZRESULT_SUCCESS;
		}
		else
		{
			return RZRESULT_INVALID;
		}
	}

	__declspec(dllexport) RZRESULT CreateHeadsetEffect(ChromaSDK::Headset::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID *pEffectId)
	{
		if (isInitialized)
		{
			// Not Implemented

			/*
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
			*/
			return RZRESULT_SUCCESS;
		}
		else
		{
			return RZRESULT_INVALID;
		}
	}

	__declspec(dllexport) RZRESULT CreateMousepadEffect(ChromaSDK::Mousepad::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID *pEffectId)
	{
		if (isInitialized)
		{
			WRAPPER_EFFECT mouseEffect = HandleMousepadEffect(Effect, pParam);

			if (pEffectId == NULL)
			{
				WriteToPipe(mouseEffect);
			}
			else
			{
				if (*pEffectId == GUID_NULL)
					CoCreateGuid(pEffectId);

				effects[*pEffectId] = mouseEffect;
			}

			return RZRESULT_SUCCESS;
		}
		else
		{
			return RZRESULT_INVALID;
		}
	}

	__declspec(dllexport) RZRESULT CreateMouseEffect(ChromaSDK::Mouse::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID *pEffectId)
	{
		if (isInitialized)
		{
			WRAPPER_EFFECT mouseEffect = HandleMouseEffect(Effect, pParam);

			if (pEffectId == NULL)
			{
				WriteToPipe(mouseEffect);
			}
			else
			{
				if (*pEffectId == GUID_NULL)
					CoCreateGuid(pEffectId);

				effects[*pEffectId] = mouseEffect;
			}

			return RZRESULT_SUCCESS;
		}
		else
		{
			return RZRESULT_INVALID;
		}
	}

	__declspec(dllexport) RZRESULT CreateKeypadEffect(ChromaSDK::Keypad::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID *pEffectId)
	{
		if (isInitialized)
		{
			// Not Implemented

			/*
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
			*/
			return RZRESULT_SUCCESS;
		}
		else
		{
			return RZRESULT_INVALID;
		}
	}

	__declspec(dllexport) RZRESULT CreateChromaLinkEffect(ChromaSDK::ChromaLink::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID *pEffectId)
	{
		if (isInitialized)
		{
			// Not Implemented

			/*
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
			*/
			return RZRESULT_SUCCESS;
		}
		else
		{
			return RZRESULT_INVALID;
		}
	}

	__declspec(dllexport) RZRESULT SetEffect(RZEFFECTID EffectId)
	{
		if (isInitialized)
		{
			if (EffectId != GUID_NULL && effects.count(EffectId) != 0)
			{
				requiresUpdate = true;
				WriteToPipe(effects[EffectId]);
			}

			return RZRESULT_SUCCESS;
		}
		else
		{
			return RZRESULT_INVALID;
		}
	}

	__declspec(dllexport) RZRESULT DeleteEffect(RZEFFECTID EffectId)
	{
		if (isInitialized)
		{
			if (EffectId != GUID_NULL && effects.count(EffectId) != 0)
				effects.erase(EffectId);

			return RZRESULT_SUCCESS;
		}
		else
		{
			return RZRESULT_INVALID;
		}
	}

	__declspec(dllexport) RZRESULT RegisterEventNotification(HWND hWnd)
	{
		if (isInitialized)
		{
			// Not Implemented

			return RZRESULT_SUCCESS;
		}
		else
		{
			return RZRESULT_INVALID;
		}
	}

	__declspec(dllexport) RZRESULT UnregisterEventNotification(HWND hWnd)
	{
		if (isInitialized)
		{
			// Not Implemented

			return RZRESULT_SUCCESS;
		}
		else
		{
			return RZRESULT_INVALID;
		}
	}

	__declspec(dllexport) RZRESULT QueryDevice(RZDEVICEID DeviceId, ChromaSDK::DEVICE_INFO_TYPE &DeviceInfo)
	{
		if (isInitialized)
		{
			if (DeviceId == ChromaSDK::BLACKWIDOW_CHROMA)
				DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DeviceType::DEVICE_KEYBOARD;
			else if (DeviceId == ChromaSDK::DEATHADDER_CHROMA)
				DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DeviceType::DEVICE_MOUSE;
			else
			{
				DeviceInfo.Connected = 0;
				return RZRESULT_SUCCESS;
				//return RZRESULT_DEVICE_NOT_CONNECTED;
			}

			DeviceInfo.Connected = 1;

			return RZRESULT_SUCCESS;
		}
		else
		{
			return RZRESULT_INVALID;
		}
	}

#ifdef __cplusplus
}
#endif

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