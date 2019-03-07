/***************************************************
* Module name: leds.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/06/13 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for controlling leds operations
*
***************************************************/

/*  Include section
* Add all #includes here
*
***************************************************/
#include "nrf_gpio.h"
#include "nrf_delay.h"
#include "app_timer.h"

#include "leds_management.h"
#include "pinout_ACL_PRO.h"
#include "button_management.h"

/*  Defines section
* Add all #defines here
*
***************************************************/
#define TIME_LEDS_STATE_ON_NOT_PAIRED   APP_TIMER_TICKS(3000)
#define TIME_LEDS_STATE_ON_PAIRED       APP_TIMER_TICKS(1500)
#define TIME_BLINKING_ON                TIME_LEDS_STATE_ON_PAIRED/2 //APP_TIMER_TICKS(200)
#define TIME_BLINKING_SLEEP             APP_TIMER_TICKS(100)
#define TIME_BLINKING_DFU_ALARM_IDE     APP_TIMER_TICKS(1000)
#define TIME_BLINKING_DFU_ALARM_TOGG    APP_TIMER_TICKS(200)

#define NUMBER_TOGGLES_ON         4
#define NUMBER_TOGGLES_SLEEP      8
#define NUMBER_TOGGLES_DFU_ALARM  8
#define NUMBER_TOGGLES_DFU_MODE   4

APP_TIMER_DEF(m_timer_leds_id);

/*  Variables section
* Add all variables here
*
***************************************************/
bool  FLAG_TIMER_LEDS_ON			=	false;
bool  FLAG_ATTEND_LED = false;      


leds_pattern_t  leds_control;
uint8_t         leds_status = LEDS_STATE_DFU_ON;
uint8_t         toggle_counter  = 0;

/*  Function Prototype Section
* Add prototypes for all functions called by this
* module, with the exception of runtime routines.
*
***************************************************/
/**************************************************
* Function name	: void timer_control_leds_start(uint32_t	time_timer_leds)

*    returns		: ----
*    args			  : uint32_t desired temporization
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Starts the timer which control leds squence 
* Notes			: restrictions, odd modes
**************************************************/
void timer_control_leds_start(uint32_t	tiempo_timer_leds)
{	
 
		if(!FLAG_TIMER_LEDS_ON)
		{
				app_timer_start(m_timer_leds_id,tiempo_timer_leds,NULL);
				FLAG_TIMER_LEDS_ON	=	true;
		}
}

/**************************************************
* Function name	: void timer_control_leds_meas_timeout_handler(void * p_context)

*    returns		: ----
*    args			  : unused pointer
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Interruption of the timer which 
*                 control leds squence 
* Notes			: restrictions, odd modes
**************************************************/
void timer_control_leds_meas_timeout_handler(void * p_context)
{
		UNUSED_PARAMETER(p_context);
		FLAG_TIMER_LEDS_ON	=	false;
		FLAG_ATTEND_LED	=	true;
}

/**************************************************
* Function name	: void timer_control_leds_stop(void)

*    returns		: ----
*    args			  : ---
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Stops the timer which control leds squence 
* Notes			: restrictions, odd modes
**************************************************/
void timer_control_leds_stop(void)
{
		if(FLAG_TIMER_LEDS_ON)
		{
				app_timer_stop(m_timer_leds_id);
				FLAG_TIMER_LEDS_ON	=	false;
		}
		FLAG_ATTEND_LED	=	false;
    toggle_counter  = 0;
}


/**************************************************
* Function name	: void leds_management_on(void)

*    returns		: ----
*    args			  : uint8_t target led
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Turns on the desired led
* Notes			: restrictions, odd modes
**************************************************/
static void leds_management_on(uint8_t led)
{
    nrf_gpio_pin_set(led);    
}

/**************************************************
* Function name	: void leds_management_off(void)

*    returns		: ----
*    args			  : uint8_t target led
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Turns off the desired led
* Notes			: restrictions, odd modes
**************************************************/
static void leds_management_off(uint8_t led)
{
    nrf_gpio_pin_clear(led);    

}

/**************************************************
* Function name	: void leds_management_toggle(void)

*    returns		: ----
*    args			  : uint8_t target led
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Toggles the desired led
* Notes			: restrictions, odd modes
**************************************************/
static void leds_management_toggle(uint8_t led)
{
    nrf_gpio_pin_toggle(led);    

}

/**************************************************
* Function name	: void leds_management_prepare_on(void)

*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/19
* Description		: Led sequence at ON
* Notes			: restrictions, odd modes
**************************************************/
static void leds_management_prepare_on(void)
{
    //Led blue blinking
    leds_control.leds_state.bl.init_blue    = true;
    leds_control.leds_state.bl.init_red     = false;
    leds_control.leds_state.bl.toggle_blue  = true;
    leds_control.leds_state.bl.toggle_red   = false;
    
    //Each 3 seconds
    leds_control.time_toggle    = TIME_BLINKING_ON;    
    leds_control.time_idle      = TIME_LEDS_STATE_ON_NOT_PAIRED;
  
    //Number of toggles
    leds_control.number_toggles = NUMBER_TOGGLES_ON;
  
    FLAG_ATTEND_LED  = true;
  
}

/**************************************************
* Function name	: void leds_management_prepare_dfu_alarm(void)

*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/19
* Description		: Led sequence for indicating DFU enter
*                 (before entering DFU)
* Notes			: restrictions, odd modes
**************************************************/
static void leds_management_prepare_dfu_alarm(void)
{
    //Red blue blinking
    leds_control.leds_state.bl.init_blue    = false;
    leds_control.leds_state.bl.init_red     = true;
    leds_control.leds_state.bl.toggle_blue  = false;
    leds_control.leds_state.bl.toggle_red   = true;
    
 
    leds_control.time_toggle    = TIME_BLINKING_DFU_ALARM_TOGG; 
    leds_control.time_idle      = TIME_BLINKING_DFU_ALARM_IDE;
  
    //Number of toggles
    leds_control.number_toggles = NUMBER_TOGGLES_DFU_ALARM;
  
    FLAG_ATTEND_LED  = true;
}

/**************************************************
* Function name	: void leds_management_prepare_dfu_mode(void)

*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/19
* Description		: Led sequence for indicating DFU state
* Notes			: restrictions, odd modes
**************************************************/
static void leds_management_prepare_dfu_mode(void)
{
    //Led blue blinking
    leds_control.leds_state.bl.init_blue    = false;
    leds_control.leds_state.bl.init_red     = true;
    leds_control.leds_state.bl.toggle_blue  = false;
    leds_control.leds_state.bl.toggle_red   = true;
    
    //Each 3 seconds
    leds_control.time_toggle    = TIME_BLINKING_ON;    
    leds_control.time_idle      = TIME_LEDS_STATE_ON_NOT_PAIRED;
  
    //Number of toggles
    leds_control.number_toggles = NUMBER_TOGGLES_ON;
  
    FLAG_ATTEND_LED  = true;
  
}

/**************************************************
* Function name	: void leds_management_all_off(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Function for turning off all leds
* Notes			: restrictions, odd modes
**************************************************/
void leds_management_all_off(void)
{
  //TODO: parar el timer
    leds_management_off(LED_BLUE);
    leds_management_off(LED_RED);
}

/**************************************************
* Function name	: void leds_management_sleep(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Leds to sleep
* Notes			: restrictions, odd modes
**************************************************/
void leds_management_sleep(void)
{
    
    timer_control_leds_stop();
    leds_management_all_off();
    leds_status  = LEDS_STATE_SLEEP;

}

/**************************************************
* Function name	: void leds_management_functions(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Funtction for controlling the toggles
* Notes			: restrictions, odd modes
**************************************************/
void leds_management_functions(void)
{
    //If timer not expired, nothing
    if(!FLAG_ATTEND_LED)
    {
        return;
    }
    FLAG_ATTEND_LED = false;

    toggle_counter++;
    /**************************************************
		* Check if is the first toggle
		**************************************************/
		if(toggle_counter	==	1)
		{
				/**************************************************
				* Check initial state of leds
				**************************************************/
				if(leds_control.leds_state.bl.init_red)
				{
						leds_management_on(LED_RED);
				}
				else
				{
						leds_management_off(LED_RED);
				}
				if(leds_control.leds_state.bl.init_blue)
				{
						leds_management_on(LED_BLUE);
				}
				else
				{
						leds_management_off(LED_BLUE);
				}
				
				
	 }
    else
    {
    
      if(leds_control.number_toggles  >=  toggle_counter)
      {
          /**************************************************
          * Continue toggle
          **************************************************/
          if(leds_control.leds_state.bl.toggle_blue)
          {
              leds_management_toggle(LED_BLUE);
          }
          if(leds_control.leds_state.bl.toggle_red)
          {
              leds_management_toggle(LED_RED);
          }
      }
  }
    
    /**************************************************
		* Check whether we have to star timer
		* If there are still toggles, start with time between
		* toggles.
		* If not, start with iddle time
		**************************************************/
    if(leds_control.number_toggles	>	toggle_counter)
		{
        
				if(leds_control.time_toggle	>	0)
				{
					timer_control_leds_start(leds_control.time_toggle);
				}
		}
		else
		{
//        leds_management_off(LED_BLUE);
//        leds_management_off(LED_RED);
				if(leds_control.time_idle	>	0)
				{
					timer_control_leds_start(leds_control.time_idle);
				}
				toggle_counter	=	0;
		}
}

/**************************************************
* Function name	: void leds_management_exe(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Funtction for managing leds state machine
* Notes			: restrictions, odd modes
**************************************************/
void leds_management_exe(void)
{ 
    switch(leds_status)
    {
      case LEDS_STATE_DFU_ON:
        if(button_management_status() ==  BUTTON_STATUS_PRESSED)
        {
            if(button_management_type_push()  ==  PUSH_TYPE_MEDIUM)
            {
                              
                //Change to ON blinking
                leds_management_prepare_on();              
                leds_status = LEDS_STATE_ON_BLINKING;
            }
            
        }
       break;
        
      case LEDS_STATE_ON_BLINKING:
        if(button_management_status() ==  BUTTON_STATUS_PRESSED)
        {
            if(button_management_type_push()  ==  PUSH_TYPE_LONG_I)
            {
                //Stop timer which controls blinking
                timer_control_leds_stop();
                leds_management_off(LED_BLUE);
              
                //Red blinking fast
                leds_management_prepare_dfu_alarm();
                leds_status = LEDS_STATE_ON_BOOT_INDICATE;
            }
        }
      break;
        
      case LEDS_STATE_ON_BOOT_INDICATE:
        if(button_management_status() ==  BUTTON_STATUS_PRESSED)
        {
            if(button_management_type_push()  ==  PUSH_TYPE_LONG_II)
            {
                //Stop timer which controls blinking
                timer_control_leds_stop();
                leds_management_off(LED_RED);
              
                //Red blinking slow
                leds_management_prepare_dfu_mode();
                leds_status = LEDS_STATE_ON_BOOT_MODE;
            }
        }
      break;
        
      case LEDS_STATE_ON_BOOT_MODE:
          //
      break;
        
    }
    leds_management_functions();
    
}

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
void leds_management_init_valid_app(void)
{
    leds_management_on(LED_BLUE);
    leds_status = LEDS_STATE_DFU_ON;
}

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
void leds_management_init_dfu_mode(void)
{
    
  
    //Red blinking slow
    leds_management_prepare_dfu_mode();
    leds_status = LEDS_STATE_ON_BOOT_MODE;
}

/**************************************************
* Function name	: void leds_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Leds initialization 
* Notes			: restrictions, odd modes
**************************************************/
void leds_managment_init(void)
{
    //Outputs
    nrf_gpio_cfg_output(LED_BLUE);
    nrf_gpio_cfg_output(LED_RED);
  
    leds_management_off(LED_BLUE);
    leds_management_off(LED_RED);
  
    app_timer_create(&m_timer_leds_id,APP_TIMER_MODE_SINGLE_SHOT,timer_control_leds_meas_timeout_handler);
  
   
}
