
#ifndef LEDS_MANAGEMENT_H__
#define LEDS_MANAGEMENT_H__


typedef union 
{
	struct
	{
		uint8_t		toggle_blue:				1;	//If blue led must be toggled
		uint8_t		toggle_red:					1;	//If red led must be toggled
		uint8_t		init_blue:					1;  //If blue inits ON
		uint8_t		init_red:					  1;	//If red inits ON
		
		
	}bl;
	
}	bits_leds_t;


typedef struct
{
		bits_leds_t		leds_state;  
		uint8_t				number_toggles;
		uint32_t			time_toggle;
		uint32_t			time_idle;
		
}leds_pattern_t;

typedef enum
{
    LEDS_STATE_ON_BLINKING,
    LEDS_STATE_ON_PRESSED_SHORT,
    LEDS_STATE_ON_PRESSED_MED,
    LEDS_STATE_ON_PRESSED_LONG,
    LEDS_STATE_PAIRING_BLINKING,
    LEDS_STATE_PAIRING_PRESSED_SHORT,
    LEDS_STATE_PAIRING_PRESSED_MED,
    LEDS_STATE_PAIRING_SUCCESS,
    LEDS_STATE_PAIRING_SUCCESS_WAIT_SEQUENCE,
    LEDS_STATE_SLEEP,
    LEDS_STATE_LOW_BATT_RED,
    LEDS_STATE_LOW_BATT_BLUE,
    LEDS_STATE_LOW_BATT_SHORT,
    LEDS_STATE_LOW_BATT_BLINK_MED
  
}leds_status_t;

/**************************************
* Functions
**************************************/
/**************************************************
* Function name	: void leds_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Leds initialization 
* Notes			: restrictions, odd modes
**************************************************/
void leds_management_init(void);

/**************************************************
* Function name	: void leds_management_exe(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Funtction for managing leds state machine
* Notes			: restrictions, odd modes
**************************************************/
void leds_management_exe(void);

/**************************************************
* Function name	: void leds_management_all_off(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Function for turning off all leds
* Notes			: restrictions, odd modes
**************************************************/
void leds_management_all_off(void);

/**************************************************
* Function name	: void leds_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Leds initialization 
* Notes			: restrictions, odd modes
**************************************************/
void leds_management_blink_pairing_success(void);

/**************************************************
* Function name	: void leds_management_timeout_pairing(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/22
* Description		: Leds function when pairing timeout expired 
* Notes			: restrictions, odd modes
**************************************************/
void leds_management_timeout_pairing(void);

#endif //LEDS_MANAGEMENT_H__

