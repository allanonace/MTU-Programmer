/***************************************************
* Module name: button_management.c
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
* Module for controlling button operations
*
***************************************************/

/*  Include section
* Add all #includes here
*
***************************************************/
#include "app_button.h"
#include "pinout_ACL_PRO.h"
#include "button_management.h"
#include "app_timer.h"
#include "timers_management.h"

/*  Defines section
* Add all #defines here
*
***************************************************/
#define BUTTON_PULL    									NRF_GPIO_PIN_NOPULL
#define MAIN_BUTTON_TIMER_INTERVAL      APP_TIMER_TICKS(1)

APP_TIMER_DEF(m_main_button_timer_id);

/*  Variables section
* Add all variables here
*
***************************************************/
/**************************************************
* Local variables
**************************************************/
typedef struct{
    uint8_t   button_status; //No pulse, push, release
    uint8_t   push_type;   //Short, medium,long
    bool      button_pending; //Complete sequence (push and release) attended or not
}button_manager_t;

button_manager_t  button_manager;

static bool       FLAG_TIMER_BUTTON_ON    = false;
static uint32_t   counter_main_button = 0;


uint32_t  length_medium_press = LENGTH_MDIUM_PULSE;
uint32_t  length_long_press   = LENGTH_LONG_PULSE;

/*  Function Prototype Section
* Add prototypes for all functions called by this
* module, with the exception of runtime routines.
*
***************************************************/
/**************************************************
* Function name	: void main_button_meas_timeout_handler(void * p_context)

*    returns		: ----
*    args			  : unused pointer
* Created by		: María Viqueira
* Date created	: 2018/06/15
* Description		: Interruption of the timer which 
*                 control button length 
* Notes			: restrictions, odd modes
**************************************************/
void main_button_meas_timeout_handler(void * p_context)
{
		UNUSED_PARAMETER(p_context);
    counter_main_button++;
  
    if(counter_main_button>=length_long_press)
    {
      button_manager.push_type = PUSH_TYPE_LONG;
    }
    else if (counter_main_button>=length_medium_press)
    {
      
      button_manager.push_type = PUSH_TYPE_MEDIUM;
    }
}

/**************************************************
* Function name	: button_evt_handler(uint8_t pin_no, uint8_t button_action)

*    returns		: ----
*    args			  : button information
* Created by		: María Viqueira
* Date created	: 2018/06/15
* Description		: Interruption when button is pressed or release
* Notes			: restrictions, odd modes
**************************************************/
void button_evt_handler(uint8_t pin_no, uint8_t button_action)
{
  
    timers_management_reset_on_timeout();
    switch (button_action)
    {
        case APP_BUTTON_PUSH:
          /****************************************************
          * If we haven't attended yet a release interruption,
          * we will not attend any push
          ***************************************************/
          if(!button_manager.button_pending)
          {
              if(!FLAG_TIMER_BUTTON_ON)
              {
                  app_timer_start(m_main_button_timer_id,MAIN_BUTTON_TIMER_INTERVAL,NULL);
                  
                  FLAG_TIMER_BUTTON_ON  = true;
              }  
              button_manager.button_status  = BUTTON_STATUS_PRESSED;
              button_manager.button_pending = true;
              button_manager.push_type      = PUSH_TYPE_SHORT;  
              counter_main_button = 0;
         }
          
        break;
           

        case APP_BUTTON_RELEASE:
          /****************************************************
          * If we detect a release interrupt and the prevoious
          * release was not attended, we will ignore this release
          * Release only attended if there was a previous push
          ***************************************************/
          if(button_manager.button_status  == APP_BUTTON_PUSH)
          {
              if(FLAG_TIMER_BUTTON_ON)
              {
                  app_timer_stop(m_main_button_timer_id);
                  FLAG_TIMER_BUTTON_ON  = false;
              }
              button_manager.button_status  = BUTTON_STATUS_RELEASE;
              button_manager.button_pending = true;
              counter_main_button = 0;
          }
          
        break;
           

        default:
             // no implementation needed
        break;
    }
}


/**************************************************
* Function name	: void button_managment_attended(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/15
* Description		: Function to indicate button 
*                 interruption was attended
* Notes			: restrictions, odd modes
**************************************************/
void button_managment_attended(void)
{
    button_manager.button_pending  = false;
    button_manager.button_status   = BUTTON_STATUS_NOT_PRESSED;
    button_manager.push_type       = PUSH_TYPE_NULL; 
    
}


/**************************************************
* Function name	: uint8_t button_management_status(void)

*    returns		: uint8_t: status of button
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/15
* Description		: Function to indicate if button is 
*                 pressed, relase or without action
* Notes			: restrictions, odd modes
**************************************************/
uint8_t button_management_status(void)
{
    return button_manager.button_status;
}

/**************************************************
* Function name	: uint8_t button_management_status(void)

*    returns		: uint8_t: type of pulse
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/15
* Description		: Function to indicate the type of
*                 pulse: short, medium, long or none
* Notes			: restrictions, odd modes
**************************************************/
uint8_t button_management_type_push(void)
{
    return button_manager.push_type;
}


/**************************************************
* Function name	: uint32_t  button_management_get_on_to_pairing_value(void)
*    returns		: uint32_t current long press value
*    args			  : ---
* Created by		: María Viqueira
* Date created	: 2018/07/16
* Description		: return the length of the pulse for changing from ON 
*                 to PAIRING
* Notes			: restrictions, odd modes
**************************************************/
uint32_t  button_management_get_on_to_pairing_value(void)
{
    return length_long_press;
}

/**************************************************
* Function name	: void button_management_new_on_to_pairing_value(uint32_t new_time)
*    returns		: ----
*    args			  : uint32_t new ON to PAIRING time value
* Created by		: María Viqueira
* Date created	: 2018/07/16
* Description		: updates value of the length pulse for ON to PAIRING
* Notes			: restrictions, odd modes
**************************************************/
void button_management_new_on_to_pairing_value(uint32_t new_time)
{
    length_long_press = new_time;
}


/**************************************************
* Function name	: uint32_t  button_management_get_power_off_value(void)
*    returns		: uint32_t current medium press value
*    args			  : ---
* Created by		: María Viqueira
* Date created	: 2018/07/16
* Description		: return the length of the pulse for going to SLEEP
* Notes			: restrictions, odd modes
**************************************************/
uint32_t  button_management_get_power_off_value(void)
{
    return length_medium_press;
}

/**************************************************
* Function name	: void button_management_new_power_off_value(uint32_t new_time)
*    returns		: ----
*    args			  : uint32_t new power off time value
* Created by		: María Viqueira
* Date created	: 2018/07/16
* Description		: updates value of the length pulse for power off
* Notes			: restrictions, odd modes
**************************************************/
void button_management_new_power_off_value(uint32_t new_time)
{
    length_medium_press = new_time;
}



/**************************************************
* Function name	: void button_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/13
* Description		: Button initialization 
* Notes			: restrictions, odd modes
**************************************************/
void button_management_init(void)
{
    uint32_t err_code;
    static app_button_cfg_t buttons[] =
    {
        {BUTTON_ACL,true,NRF_GPIO_PIN_PULLDOWN,button_evt_handler},
    };
    
   err_code = app_button_init(buttons, sizeof(buttons) / sizeof(buttons[0]),APP_TIMER_TICKS(20));
   APP_ERROR_CHECK(err_code);
   app_button_enable();
    
   err_code = app_timer_create(&m_main_button_timer_id,APP_TIMER_MODE_REPEATED,main_button_meas_timeout_handler);
   APP_ERROR_CHECK(err_code);
    
   button_managment_attended(); 
   
	
}

