#ifndef INTERRUPTION_FLAGS_H
#define INTERRUPTION_FLAGS_H

#include <stdint.h>

typedef struct{
	uint8_t	flag_button_pushed:		1;
	uint8_t	flag_button_released:	1;
	
}button_interruptions_t;
	

#endif
