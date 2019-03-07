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
#include <stdint.h>
#include "nrf_gpio.h"
#include "app_timer.h"
#include "leds_management.h"
#include "pinout_ACL_PRO.h"
#include "button_management.h"
#include "defines_tests.h"
#include "battery_management.h"
#include "connection_control_management.h"


/*  Defines section
* Add all #defines here
*
***************************************************/
#define TIME_LEDS_STATE_ON_NOT_PAIRED   APP_TIMER_TICKS(3000)
#define TIME_LEDS_STATE_ON_PAIRED       APP_TIMER_TICKS(1500)
#define TIME_BLINKING_ON                TIME_LEDS_STATE_ON_PAIRED/4 //APP_TIMER_TICKS(200)
#define TIME_BLINKING_SLEEP             APP_TIMER_TICKS(100)
#define TIME_RED_LOW_BATT               APP_TIMER_TICKS(1500)/2

#define NUMBER_TOGGLES_ON               2//4
#define NUMBER_TOGGLES_SLEEP            8
#define NUMBER_TOGGLES_PAIRING          2
#define NUMBER_TOGGLES_PAIRING_SUCCESS  2
#define NUMBER_TOGGLES_MED              1
#define NUMBER_TOGGLES_LOW_BLUE         1
#define NUMBER_TOGGLES_LOW_RED          1

APP_TIMER_DEF(m_timer_leds_id);

/*  Variables section
* Add all variables here
*
***************************************************/
bool  FLAG_TIMER_LEDS_ON			=	false;
bool  FLAG_ATTEND_LED = false;      


leds_pattern_t  leds_control;
uint8_t         leds_status;
uint8_t         toggle_counter  = 0;

bool  leds_on_paired  = false;

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
void timer_control_leds_start(uint32_t	time_timer_leds)
{	
    static uint32_t err_code;
 
		if(!FLAG_TIMER_LEDS_ON)
		{
				err_code  = app_timer_start(m_timer_leds_id,time_timer_leds,NULL);
        APP_ERROR_CHECK(err_code);
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
    static uint32_t err_code;
		if(FLAG_TIMER_LEDS_ON)
		{
				err_code  = app_timer_stop(m_timer_leds_id);
        APP_ERROR_CHECK(err_code);
				FLAG_TIMER_LEDS_ON	=	false;
		}
		FLAG_ATTEND_LED	=	false;
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
  
    if(connection_control_management_get_connection_valid())
    {
      leds_control.time_idle      = TIME_LEDS_STATE_ON_PAIRED;
    }
    else
    {
      leds_control.time_idle      = TIME_LEDS_STATE_ON_NOT_PAIRED;
    }
  
    //Number of toggles
    leds_control.number_toggles = NUMBER_TOGGLES_ON;
  
    FLAG_ATTEND_LED  = true;
  
}

/**************************************************
* Function name	: void leds_management_prepare_med(void)

*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/19
* Description		: Led sequence for indicating SLEEP
* Notes			: restrictions, odd modes
**************************************************/
static void leds_management_prepare_med(void)
{
    //Led blue blinking
    leds_control.leds_state.bl.init_blue    = true;
    leds_control.leds_state.bl.init_red     = false;
    leds_control.leds_state.bl.toggle_blue  = true;
    leds_control.leds_state.bl.toggle_red   = false;
  
  
    leds_control.time_toggle    = TIME_BLINKING_SLEEP;    
    leds_control.time_idle      = 0;
  
    leds_control.number_toggles = NUMBER_TOGGLES_MED;
  
 
    FLAG_ATTEND_LED  = true;
}

/**************************************************
* Function name	: void leds_management_prepare_pairing(void)

*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/19
* Description		: Led sequence for indicating PAIRING
* Notes			: restrictions, odd modes
**************************************************/
static void leds_management_prepare_pairing(void)
{
    //Leds blue and red blinking
    leds_control.leds_state.bl.init_blue    = true;
    leds_control.leds_state.bl.init_red     = false;
    leds_control.leds_state.bl.toggle_blue  = true;
    leds_control.leds_state.bl.toggle_red   = true;
  
  
    leds_control.time_toggle    = TIME_BLINKING_ON;
    leds_control.time_idle      = 0;
  
    //One toggle
    leds_control.number_toggles = 2;
    
 
  
    FLAG_ATTEND_LED  = true;

}

/**************************************************
* Function name	: void leds_management_prepare_pairing_success(void)

*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/19
* Description		: Led sequence for indicating pairing success
* Notes			: restrictions, odd modes
**************************************************/
static void leds_management_prepare_pairing_success(void)
{
    leds_control.leds_state.bl.init_blue    = true;
    leds_control.leds_state.bl.init_red     = false;
    leds_control.leds_state.bl.toggle_blue  = true;
    leds_control.leds_state.bl.toggle_red   = false;
    
    //
    leds_control.time_toggle    = APP_TIMER_TICKS(200);    
    leds_control.time_idle      =  APP_TIMER_TICKS(200); 
  
    leds_control.number_toggles = NUMBER_TOGGLES_PAIRING_SUCCESS;
  
    FLAG_ATTEND_LED  = true;
}

/**************************************************
* Function name	: void leds_management_prepare_low_blue(void)

*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/09/19
* Description		: Blue led sequence for indicating low battery
* Notes			: restrictions, odd modes
**************************************************/
static void leds_management_prepare_low_blue(void)
{
    leds_control.leds_state.bl.init_blue    = true;
    leds_control.leds_state.bl.init_red     = false;
    leds_control.leds_state.bl.toggle_blue  = true;
    leds_control.leds_state.bl.toggle_red   = false;
    
    leds_control.time_toggle    = TIME_BLINKING_ON;    
    leds_control.time_idle      = TIME_RED_LOW_BATT;
  
    leds_control.number_toggles = NUMBER_TOGGLES_LOW_BLUE;
    toggle_counter  = 0;
    leds_management_off(LED_RED);
    FLAG_ATTEND_LED  = true;
}

/**************************************************
* Function name	: void leds_management_prepare_low_red(void)

*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/09/19
* Description		: Red led sequence for indicating low battery
* Notes			: restrictions, odd modes
**************************************************/
static void leds_management_prepare_low_red(void)
{
    leds_control.leds_state.bl.init_blue    = false;
    leds_control.leds_state.bl.init_red     = true;
    leds_control.leds_state.bl.toggle_blue  = false;
    leds_control.leds_state.bl.toggle_red   = true;
    
    leds_control.time_toggle    = TIME_BLINKING_ON;    
    leds_control.time_idle      = TIME_RED_LOW_BATT;
  
    leds_control.number_toggles = NUMBER_TOGGLES_LOW_RED;
    toggle_counter  = 0;
    leds_management_off(LED_BLUE);
    FLAG_ATTEND_LED  = true;
}

/**************************************************
* Function name	: void leds_management_sleep(void)

*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/21
* Description		: Preparing leds before SLEEP
* Notes			: restrictions, odd modes
**************************************************/
static void leds_management_sleep(void)
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
        timer_control_leds_start(leds_control.time_toggle);
        return;
				
    }

      //First, check if this sequence is only toggle (no idle)
      if(leds_control.time_idle == 0)
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
            timer_control_leds_start(leds_control.time_toggle);
            return;
      }
      
      
      //Toggle and idle
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
          timer_control_leds_start(leds_control.time_toggle);
          return;
      }
    
      //IDLE
      toggle_counter  = 0;
      leds_management_all_off();
  
      timer_control_leds_start(leds_control.time_idle);
         
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
        case LEDS_STATE_ON_BLINKING:
          if(button_management_status() ==  BUTTON_STATUS_PRESSED)
          {
              if(button_management_type_push()  ==  PUSH_TYPE_SHORT)
              {
                  //Stop timer which controls blinking
                  timer_control_leds_stop();
                
                  //Change to ON
                  leds_management_on(LED_BLUE);
                
                  leds_status = LEDS_STATE_ON_PRESSED_SHORT;
              }
              
          }
          if(!leds_on_paired)
          {
              if  (connection_control_management_get_connection_valid())
              {
                  //Stop timer which controls blinking
                  timer_control_leds_stop();
                  leds_management_prepare_on();
                  leds_on_paired  = true;
                  leds_management_all_off();
              }
          }
          else
          {
              if(!connection_control_management_get_connection_valid())
              {
                  //Stop timer which controls blinking
                  timer_control_leds_stop();
                  leds_management_prepare_on();
                  leds_on_paired  = false;
                  leds_management_all_off();
              }
          }
          if(battery_management_is_low_battery())
          {
              //Stop timer which controls blinking
              timer_control_leds_stop();
              leds_management_prepare_low_blue();
              leds_status = LEDS_STATE_LOW_BATT_BLUE;
          }
          
        break;
          
        case LEDS_STATE_ON_PRESSED_SHORT:
          if(button_management_status() ==  BUTTON_STATUS_PRESSED)
          {
            if(button_management_type_push()  ==  PUSH_TYPE_MEDIUM)
            {
              //Blink blue
              leds_management_off(LED_BLUE);
              leds_management_prepare_med();              
              leds_status     = LEDS_STATE_ON_PRESSED_MED;
              
            }
          }
          else
          {
              
              //State: ON 
              leds_management_prepare_on();            
              leds_status = LEDS_STATE_ON_BLINKING;
          }
        break;
        
        case LEDS_STATE_ON_PRESSED_MED:
          if(button_management_status() ==  BUTTON_STATUS_PRESSED)
          {
            if(button_management_type_push()  ==  PUSH_TYPE_LONG)
            {
              //Blink blue and red
              toggle_counter=0;
              leds_management_all_off();
              leds_management_prepare_pairing();              
              leds_status     = LEDS_STATE_PAIRING_BLINKING;
              
            }
          }
          else
          {
                            
              //Sleep sate. 
              
              leds_management_sleep();
              #ifdef TEST_NO_SLEEP
                leds_status = LEDS_STATE_ON_BLINKING;
                leds_management_prepare_on();
              #endif
                
          }
        break;
          
        case LEDS_STATE_PAIRING_BLINKING:
            if(button_management_status() ==  BUTTON_STATUS_PRESSED)
            {
                if(button_management_type_push()  ==  PUSH_TYPE_SHORT)
                {
                  //Stop timer which controls blinking
                  timer_control_leds_stop();
                
                  //Change Blue to ON. Red, off
                  leds_management_off(LED_RED);
                  leds_management_on(LED_BLUE);
                  leds_status = LEDS_STATE_PAIRING_PRESSED_SHORT;
                }
            }
        break;
            
        case LEDS_STATE_PAIRING_PRESSED_SHORT:
          if(button_management_status() ==  BUTTON_STATUS_PRESSED)
          {
            if(button_management_type_push()  ==  PUSH_TYPE_MEDIUM)
            {
                //Blink blue
                leds_management_off(LED_BLUE);
                leds_management_prepare_med();     
                toggle_counter  = 0;  
                leds_status = LEDS_STATE_PAIRING_PRESSED_MED;
            }
          }
          else
          {
              //Pairing
               //Blink blue and red
              leds_management_off(LED_BLUE);
              leds_management_prepare_pairing();    
              toggle_counter  = 0;
              leds_status     = LEDS_STATE_PAIRING_BLINKING;
          }
        break;
        
        case LEDS_STATE_PAIRING_PRESSED_MED:
          if(button_management_status() ==  BUTTON_STATUS_PRESSED)
          {
            if(button_management_type_push()  ==  PUSH_TYPE_LONG)
            {
                //State: ON
                toggle_counter  = 0;
                //Stop timer which controls blinking
                timer_control_leds_stop();
              
                leds_management_prepare_on();
              
                leds_status = LEDS_STATE_ON_BLINKING;
              
                
            }
          }
          else
          {
              //Sleep sate. 
              leds_management_sleep();
              #ifdef TEST_NO_SLEEP
                leds_status = LEDS_STATE_ON_BLINKING;
                leds_management_prepare_on();
              #endif

              
          }
        break;
          
        case LEDS_STATE_PAIRING_SUCCESS: 
          leds_status = LEDS_STATE_PAIRING_SUCCESS_WAIT_SEQUENCE;
        break;
        
        case LEDS_STATE_PAIRING_SUCCESS_WAIT_SEQUENCE:
          
          //Wait toggless
          if(toggle_counter>=NUMBER_TOGGLES_PAIRING_SUCCESS)
          {
              //Finished
              //State: ON 
              leds_management_prepare_on();            
              leds_status = LEDS_STATE_ON_BLINKING;
              leds_on_paired  = true;
              
          }
        break;
          
        case LEDS_STATE_LOW_BATT_RED:
            if(button_management_type_push() ==  PUSH_TYPE_SHORT)
            {
                toggle_counter  = 0;
                timer_control_leds_stop();
                leds_management_all_off();
                leds_management_on(LED_BLUE);
                leds_status = LEDS_STATE_LOW_BATT_SHORT;
            }
            else if((toggle_counter == 0) &&FLAG_ATTEND_LED)
            {
              leds_management_all_off();
              leds_management_prepare_low_blue();
              leds_status = LEDS_STATE_LOW_BATT_BLUE;
            }
        break;
        
        case LEDS_STATE_LOW_BATT_BLUE:
           if(button_management_type_push()  ==  PUSH_TYPE_SHORT)
            {
                toggle_counter  = 0;
                timer_control_leds_stop();
                leds_management_all_off();
                leds_management_on(LED_BLUE);
                leds_status = LEDS_STATE_LOW_BATT_SHORT;
            }
            else if((toggle_counter == 0) &&FLAG_ATTEND_LED)
            {             
                leds_management_prepare_low_red();
                leds_status = LEDS_STATE_LOW_BATT_RED;
            }
          
        break;
         
        case LEDS_STATE_LOW_BATT_SHORT:
          if(button_management_type_push()  ==  PUSH_TYPE_MEDIUM)
          {
                toggle_counter  = 0;
                leds_management_all_off(); 
                leds_management_prepare_med();
                leds_status = LEDS_STATE_LOW_BATT_BLINK_MED;
          }
        break;

        
            
        case LEDS_STATE_LOW_BATT_BLINK_MED:
          
            if((button_management_type_push()  ==  PUSH_TYPE_LONG)  ||  (button_management_status() ==  BUTTON_STATUS_NOT_PRESSED))
            {
                toggle_counter  = 0;
                leds_management_all_off();
                leds_management_prepare_low_red();
                leds_status = LEDS_STATE_LOW_BATT_RED;
            }
        break;
          
    }
    leds_management_functions();
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
* Function name	: void leds_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Leds initialization 
* Notes			: restrictions, odd modes
**************************************************/
void leds_management_blink_pairing_success(void)
{
    //Sequence when paired success
    timer_control_leds_stop();
    
    leds_management_prepare_pairing_success();
  
    leds_status = LEDS_STATE_PAIRING_SUCCESS;
    toggle_counter  = 0;
    
}
/**************************************************
* Function name	: void leds_management_timeout_pairing(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/22
* Description		: Leds function when pairing timeout expired 
* Notes			: restrictions, odd modes
**************************************************/
void leds_management_timeout_pairing(void)
{
    //Stop timer
    timer_control_leds_stop();
  
    //Preparing ON state
    leds_management_prepare_on();
  
    leds_status = LEDS_STATE_ON_BLINKING;

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
void leds_management_init(void)
{
    //Outputs
    nrf_gpio_cfg_output(LED_BLUE);
    nrf_gpio_cfg_output(LED_RED);
  
    leds_management_off(LED_BLUE);
    leds_management_off(LED_RED);
  
    //Preparing ON state
    app_timer_create(&m_timer_leds_id,APP_TIMER_MODE_SINGLE_SHOT,timer_control_leds_meas_timeout_handler);
    leds_management_prepare_on();
    
    

 
}
