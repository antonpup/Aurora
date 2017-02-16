/*********************************************************

LFXDecl.h - definitions for Dell LightFX 2.0

Purpose: Provide definitions for communicating with
the LightFX 2.0 API

Copyright (c) 2007 Dell, Inc. All rights reserved.
Date: 6/21/2007

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

// Helper release and delete functions, used in cleanup routines
#define LFX_SAFE_DELETE(p)       { if(p) { delete (p);     (p)=NULL; } }
#define LFX_SAFE_RELEASE(p)      { if(p) { (p)->Release(); (p)=NULL; } }
#define LFX_SAFE_DELETE_ARRAY(p) { if(p) { delete[] (p);   (p)=NULL; } }

#define LFX_DEF_STRING_SIZE 255	// Default string size
#define LFX_MAX_STRING_SIZE 255	// Maximum string size
#define LFX_MAX_COLOR_VALUE 255	// Maximum color value

#define LFX_MINIMUM_DEVICE_SIZE 3 // Min device physical size/bounds in cm
#define LFX_SEGMENTS_PER_AXIS 3 // Each location axis (x,y,z) has 3 segments (e.g. left,center,right), resulting in 27 possible locations

// LightFX 1.x legacy support
#define LFX_INTENSITY_CONV	36	// Brightness/intensity conversion factor
#define LFX_INDEX_COLOR_COUNT 17 // 16 colors, plus 0 for "off"

// Type definitions
#define LFX_PROTOCOL_UNKNOWN	0x00
#define LFX_PROTOCOL_LFX1		0x01
#define LFX_PROTOCOL_ESA		0x28
#define LFX_PROTOCOL_OTHER		0xFF

#define LFX_CATEGORY_UNKNOWN	0x00
#define LFX_CATEGORY_SYSTEM		0x01
#define LFX_CATEGORY_PERIPH		0x02
#define LFX_CATEGORY_OTHER		0xFF

#define LFX_DEVTYPE_UNKNOWN		0x00
#define LFX_DEVTYPE_NOTEBOOK	0x01
#define LFX_DEVTYPE_DESKTOP		0x02
#define LFX_DEVTYPE_SERVER		0x03
#define LFX_DEVTYPE_DISPLAY		0x04
#define LFX_DEVTYPE_MOUSE		0x05
#define LFX_DEVTYPE_KEYBOARD	0x06
#define LFX_DEVTYPE_GAMEPAD		0x07
#define LFX_DEVTYPE_SPEAKER		0x08
#define LFX_DEVTYPE_OTHER		0xFF

// Return values
#define LFX_RESULT unsigned int
#define LFX_SUCCESS				0		// Success
#define LFX_FAILURE				1		// Generic failure
#define LFX_ERROR_NOINIT		2		// System not initialized yet
#define LFX_ERROR_NODEVS		3		// No devices available
#define LFX_ERROR_NOLIGHTS		4		// No lights available
#define LFX_ERROR_BUFFSIZE		5		// Buffer size too small

// Translation layer position/location encoding
// Note: This is a bit mask, with 27 zones encoded into 32 bits
//		Bits 0 through 8 are all part of the front-most plane (closest to the user)
//		Bits 9 through 17 are all part of the middle plane (mid-way from the user)
//		Bits 18 through 27 are all part of the back-most plane (furthest from the user)
//		Bits 28 through 32 are reserved
//		
//		Bits 0 through 8 are split into lower, middle and upper sections (relative to the floor & ceiling)
//
//		Bit 0: [ FRONT, LOWER, LEFT ]
//		Bit 1: [ FRONT, LOWER, CENTER ]
//		Bit 2: [ FRONT, LOWER, RIGHT ]
//
//		Bit 3: [ FRONT, MIDDLE, LEFT ]
//		Bit 4: [ FRONT, MIDDLE, CENTER ]
//		Bit 5: [ FRONT, MIDDLE, RIGHT ]
//
//		Bit 6: [ FRONT, UPPER, LEFT ]
//		Bit 7: [ FRONT, UPPER, CENTER ]
//		Bit 8: [ FRONT, UPPER, RIGHT ]
//
//		Bits 9 through 17 are split into lower, middle and upper sections as well
//
//		Bit  9: [ MIDDLE, LOWER, LEFT ]
//		Bit 10: [ MIDDLE, LOWER, CENTER ]
//		Bit 11: [ MIDDLE, LOWER, RIGHT ]
//
//		Bit 12: [ MIDDLE, MIDDLE, LEFT ]
//		Bit 13: [ MIDDLE, MIDDLE, CENTER ]
//		Bit 14: [ MIDDLE, MIDDLE, RIGHT ]
//
//		Bit 15: [ MIDDLE, UPPER, LEFT ]
//		Bit 16: [ MIDDLE, UPPER, CENTER ]
//		Bit 17: [ MIDDLE, UPPER, RIGHT ]
//
//		Similar with bits 18 through 26
//
//		Bit 18: [ BACK, LOWER, LEFT ]
//		Bit 19: [ BACK, LOWER, CENTER ]
//		Bit 20: [ BACK, LOWER, RIGHT ]
//
//		Bit 21: [ BACK, MIDDLE, LEFT ]
//		Bit 22: [ BACK, MIDDLE, CENTER ]
//		Bit 23: [ BACK, MIDDLE, RIGHT ]
//
//		Bit 24: [ BACK, UPPER, LEFT ]
//		Bit 25: [ BACK, UPPER, CENTER ]
//		Bit 26: [ BACK, UPPER, RIGHT ]
//
//		Bits 27 through 32 are reserved

// Near Z-plane light definitions
#define LFX_FRONT_LOWER_LEFT	0x00000001
#define LFX_FRONT_LOWER_CENTER	0x00000002
#define LFX_FRONT_LOWER_RIGHT	0x00000004

#define LFX_FRONT_MIDDLE_LEFT	0x00000008
#define LFX_FRONT_MIDDLE_CENTER	0x00000010
#define LFX_FRONT_MIDDLE_RIGHT	0x00000020

#define LFX_FRONT_UPPER_LEFT	0x00000040
#define LFX_FRONT_UPPER_CENTER	0x00000080
#define LFX_FRONT_UPPER_RIGHT	0x00000100

// Mid Z-plane light definitions
#define LFX_MIDDLE_LOWER_LEFT	0x00000200
#define LFX_MIDDLE_LOWER_CENTER 0x00000400
#define LFX_MIDDLE_LOWER_RIGHT	0x00000800

#define LFX_MIDDLE_MIDDLE_LEFT	0x00001000
#define LFX_MIDDLE_MIDDLE_CENTER 0x00002000
#define LFX_MIDDLE_MIDDLE_RIGHT 0x00004000

#define LFX_MIDDLE_UPPER_LEFT	0x00008000
#define LFX_MIDDLE_UPPER_CENTER 0x00010000
#define LFX_MIDDLE_UPPER_RIGHT	0x00020000

// Far Z-plane light definitions
#define LFX_REAR_LOWER_LEFT		0x00040000
#define LFX_REAR_LOWER_CENTER	0x00080000
#define LFX_REAR_LOWER_RIGHT	0x00100000

#define LFX_REAR_MIDDLE_LEFT	0x00200000
#define LFX_REAR_MIDDLE_CENTER	0x00400000
#define LFX_REAR_MIDDLE_RIGHT	0x00800000

#define LFX_REAR_UPPER_LEFT		0x01000000
#define LFX_REAR_UPPER_CENTER	0x02000000
#define LFX_REAR_UPPER_RIGHT	0x04000000

// Combination bit masks
#define LFX_ALL					0x07FFFFFF
#define LFX_ALL_RIGHT			0x04924924
#define LFX_ALL_LEFT			0x01249249
#define LFX_ALL_UPPER			0x070381C0
#define LFX_ALL_LOWER			0x001C0E07
#define LFX_ALL_FRONT			0x000001FF
#define LFX_ALL_REAR			0x07FC0000

// Translation layer color encoding
#define LFX_OFF		0x00000000
#define LFX_BLACK	0x00000000
#define LFX_RED		0x00FF0000
#define LFX_GREEN	0x0000FF00
#define LFX_BLUE	0x000000FF
#define LFX_WHITE	0x00FFFFFF
#define LFX_YELLOW	0x00FFFF00
#define LFX_ORANGE	0x00FF8000
#define LFX_PINK	0x00FF80FF
#define LFX_CYAN	0x0000FFFF

// Translation layer brightness encoding
#define LFX_FULL_BRIGHTNESS	0xFF000000
#define LFX_HALF_BRIGHTNESS 0x80000000
#define LFX_MIN_BRIGHTNESS	0x00000000

// Predifined kinds of actions
#define LFX_ACTION_MORPH	0x00000001
#define LFX_ACTION_PULSE	0x00000002
#define LFX_ACTION_COLOR	0x00000003

// Color, encoded into 4 unsigned chars
typedef struct _LFX_COLOR
{
	unsigned char red;
	unsigned char green;
	unsigned char blue;
	unsigned char brightness;

}LFX_COLOR, *PLFX_COLOR;

/**************************************************************************************
IMPORTANT NOTE:

The semantics of LightFX position, location mask, and bounds are defined as follows:

BOUNDS are the physical bounds, in centimeters, of any given device/enclosure,
relative to an anchor point at the lower, left, rear corner
POSITION is a physical position, in centimeters, of any given light relative to
the lower, left, rear corner of the device's bounding box.
LOCATION (or "location mask") is a 32-bit mask that denotes one or more light positions
in terms of the device's bounding box. There are 27 bits for each smaller cube
within this bounding box, divided evenly. (Imagine a Rubick's cube...)

BOUNDS or POSITION may be encoded into a LFX_POSITION structure, so it is important to
examine the context of the usage to determine what the data inside the structure refers to.

LOCATION should always be encoded into a 32-bit (or larger) value; see the bit field
declarations above.
***************************************************************************************/

/* Position, encoded into a 3-axis value.
Note that these are relative to the lower, left, rear
corner of the device's bounding box.
X increases from left to right.
Y increases from bottom to top.
Z increases from back to front. */
typedef struct _LFX_POSITION
{
	unsigned char x;
	unsigned char y;
	unsigned char z;

} LFX_POSITION, *PLFX_POSITION;