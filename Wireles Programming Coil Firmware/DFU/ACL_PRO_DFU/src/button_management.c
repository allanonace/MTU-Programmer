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

/*  Defines section
* Add all #defines here
*
***************************************************/
#define BUTTON_PULL    									NRF_GPIO_PIN_NOPULL
#define MAIN_BUTTON_TIMER_INTERVAL      APP_TIMER_TICKS(1)

#define LENGTH_BEFORE_DFU_ENTER         5000 //length ms. Time before entering at bootloader to indicate the user
#define LENGTH_MDIUM_PULSE              1500 //length ms
#define LENGTH_DFU_ENTER_PULSE          20000 //length 0ms. Bootloader mode
#define LENGTH_DFU_ALARM_PULSE          (LENGTH_DFU_ENTER_PULSE - LENGTH_BEFORE_DFU_ENTER) //length ms. 


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
    bool      button_pending; 
}button_manager_t;

button_manager_t  button_manager;

static bool     FLAG_TIMER_BUTTON_ON    = false;
static uint16_t counter_main_button = 0;

uint32_t  length_enter_dfu    = LENGTH_DFU_ENTER_PULSE;
uint32_t  length_alarm_dfu    = LENGTH_DFU_ALARM_PULSE;
uint32_t  length_medium_press = LENGTH_MDIUM_PULSE;

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
  
    if(counter_main_button  >=  length_enter_dfu)
    {
      button_manager.push_type = PUSH_TYPE_LONG_II; //Bootloader
    }
    else if (counter_main_button  >=  length_alarm_dfu)
    {
      
      button_manager.push_type = PUSH_TYPE_LONG_I;  //Advise we are entering at bootloader mode
    }
    else if (counter_main_button>=length_medium_press)
    {
        button_manager.push_type = PUSH_TYPE_MEDIUM; //On state. Launch app
    }
}

/**************************************************
* Function name	: static void button_was_pushed(void)
*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/15
* Description		: Same actions as if button pushed
* Notes			: restrictions, odd modes
**************************************************/
static void button_was_pushed(void)
{
    if(!FLAG_TIMER_BUTTON_ON)
    {
        app_timer_start(m_main_button_timer_id,MAIN_BUTTON_TIMER_INTERVAL,NULL);
        
        FLAG_TIMER_BUTTON_ON  = true;
    }  
    button_manager.button_status  = BUTTON_STATUS_PRESSED;
    button_manager.button_pending = true;
    button_manager.push_type      = PUSH_TYPE_SHORT;  
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
    switch (button_action)
    {
        case APP_BUTTON_PUSH:
          /****************************************************
          * If we haven't attended yet a release interruption,
          * we will not attend any push
          ***************************************************/
          if(!button_manager.button_pending)
          {
            button_was_pushed();
          }
          
        break;
           

        case APP_BUTTON_RELEASE:
          /****************************************************
          * If we detect a release interrupt and the prevoious
          * release was not attended, we will ignore this release
          * Release only attended if there was a previous push
          ***************************************************/
        
//          if(button_manager.button_status  == APP_BUTTON_PUSH)
//          {
            if(FLAG_TIMER_BUTTON_ON)
            {
                app_timer_stop(m_main_button_timer_id);
                FLAG_TIMER_BUTTON_ON  = false;
            }
            button_manager.button_status  = BUTTON_STATUS_RELEASE;
            button_manager.button_pending = true;
         // }
          
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
    button_manager.button_pending = false;
    button_manager.button_status   = BUTTON_STATUS_NOT_PRESSED;
    button_manager.push_type       = PUSH_TYPE_NULL; 
    counter_main_button = 0;
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
* Function name	: uint8_t button_management_type_push(void)

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
* Function name	: void button_management_button_pushed(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/13
* Description		: Initialize timer and button structure as
*                 if an interruption happened
* Notes			: restrictions, odd modes
**************************************************/
void button_management_button_pushed(void)
{
    button_was_pushed();
}

/**************************************************
* Function name	: void button_management_new_wake_up_length_value(uint32_t new_wake)
*    returns		: ----
*    args			  : uint32_t new wake_up time value
* Created by		: María Viqueira
* Date created	: 2018/07/27
* Description		: updates value of the length pulse for wake up
* Notes			: restrictions, odd modes
**************************************************/
void button_management_new_wake_up_length_value(uint32_t new_wake)
{
    length_medium_press = new_wake;
}

/**************************************************
* Function name	: void button_management_new_alarm_dfu_length_value(uint32_t new_alarm)
*    returns		: ----
*    args			  : uint32_t new DFU alarm time value
* Created by		: María Viqueira
* Date created	: 2018/07/27
* Description		: updates value of the length pulse for DFU alarm
* Notes			: restrictions, odd modes
**************************************************/
void button_management_new_alarm_dfu_length_value(uint32_t new_alarm)
{
    length_alarm_dfu  = new_alarm;
}

/**************************************************
* Function name	: void button_management_new_enter_dfu_length_value(uint32_t new_dfu)
*    returns		: ----
*    args			  : uint32_t new dfu enter time value
* Created by		: María Viqueira
* Date created	: 2018/07/27
* Description		: updates value of the length pulse for bootloader mode
* Notes			: restrictions, odd modes
**************************************************/
void button_management_new_enter_dfu_length_value(uint32_t new_dfu)
{
    length_enter_dfu  = new_dfu;
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

