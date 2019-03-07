#include <stdint.h>
#include <stdbool.h>

#include "general_management.h"
#include "button_management.h"
#include "leds_management.h"
#include "interruption_flags.h"

//GLOBAL VARIABLES
extern button_interruptions_t	button_interruption;


static uint8_t	general_state	=	GENERAL_STATE_IDE;

static void general_control_trans(void)
{
		switch(general_state)
		{
				case	GENERAL_STATE_IDE:
					if(button_management_is_button_pushed())
					{
							general_state	=	GENERAL_STATE_BUTTON_PUSHED;
					}
					
				break;
					
				case GENERAL_STATE_BUTTON_PUSHED:
					general_state	=	GENERAL_STATE_WAIT_ACTION;
				break;
				
				case GENERAL_STATE_WAIT_ACTION:
					if(button_interruption.flag_button_released)
					{
							general_state	=	GENERAL_STATE_BUTTON_RELEASED;
							button_interruption.flag_button_released	=	false;
					}
				break;
				
				case	GENERAL_STATE_BUTTON_RELEASED:
					//
				break;
				
				case	GENERAL_STATE_LAUNCH_APP:
					//
				break;
				
				case	GENERAL_STATE_LAUNCH_DFU:
					//
				break;
		}
}

static bool general_control_exe(void)
{
		bool	sleep	=	true;
	
		switch(general_state)
		{
				case	GENERAL_STATE_IDE:
					//
				break;
				
				case GENERAL_STATE_BUTTON_PUSHED:
					led_button_pushed();
					sleep	=	false;
				break;
				
				case GENERAL_STATE_WAIT_ACTION:
					//
				break;
				
				case	GENERAL_STATE_BUTTON_RELEASED:
					led_button_release();
					sleep	=	false;
				break;
				
				case	GENERAL_STATE_LAUNCH_APP:
					//
				break;
				
				case	GENERAL_STATE_LAUNCH_DFU:
					//
				break;
		}
	
		return sleep;
}


bool general_control(void)
{
		
		bool	sleep;
	
		general_control_trans();
		sleep	=	general_control_exe();
	
		return sleep;
}