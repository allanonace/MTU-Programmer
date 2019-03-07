
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
    LEDS_STATE_DFU_ON,
    LEDS_STATE_ON_BLINKING,
    LEDS_STATE_ON_PRESSED_SHORT,
    LEDS_STATE_ON_BOOT_INDICATE,
    LEDS_STATE_ON_BOOT_MODE,
    LEDS_STATE_SLEEP
  
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
void leds_managment_init(void);

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
* Function name	: void leds_management_init_valid_app(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/25
* Description		: Leds sequence when there is valid app:
*                 blue ON
* Notes			    : restrictions, odd modes
**************************************************/
void leds_management_init_valid_app(void);

/**************************************************
* Function name	: void leds_management_init_dfu_mode(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/25
* Description		: Leds sequence when there is no valid 
*                 app: red blinking
* Notes			: restrictions, odd modes
**************************************************/
void leds_management_init_dfu_mode(void);

/**************************************************
* Function name	: void leds_management_sleep(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Leds to sleep
* Notes			: restrictions, odd modes
**************************************************/
void leds_management_sleep(void);


#endif //LEDS_MANAGEMENT_H__

