/***************************************************
* Module name: dfu_states.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/06/25 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for controlling state machine
*
***************************************************/

/*  Include section
* Add all #includes here
*
***************************************************/
#include <stdint.h>
#include <stdbool.h>
#include "nrf_dfu_utils.h"
#include "nrf_drv_clock.h"
#include "nrf_bootloader_app_start.h"
#include "nrf_log.h"
#include "nrf_log_ctrl.h"
#include "nrf_log_default_backends.h"
#include "nrf_bootloader_info.h"
#include "nrf_delay.h"

#include "button_management.h"
#include "dfu_states_management.h"
#include "leds_management.h"
#include "dfu_launch_management.h"
#include "gpio_management.h"

/*  Defines section
* Add all #defines here
*
***************************************************/

/*  Variables section
* Add all variables here
*
***************************************************/
uint8_t dfu_state = STATE_WAIT_RELEASE;

/*  Function Prototype Section
* Add prototypes for all functions called by this
* module, with the exception of runtime routines.
*
***************************************************/

/**************************************************
* Function name	: void dfu_state_machine_trans(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/25
* Description		: Controls the transactions of the
*                 state machine
* Notes			    : restrictions, odd modes
**************************************************/
void dfu_state_machine_trans(void)
{
    switch(dfu_state)
    {
        case STATE_WAIT_RELEASE:
          
          //First, check whether app is valid or not
          if(nrf_dfu_app_is_valid())
          {
              //Wait until release
              if(button_management_status() ==  BUTTON_STATUS_RELEASE)
              {
                  switch(button_management_type_push())
                  {
                      case PUSH_TYPE_SHORT:  //p<1.5
                        //Sleep again
                        dfu_state = STATE_SLEEP_PREPARE;
                      break;
                      
                      case PUSH_TYPE_MEDIUM: //1.5<=p<3
                        //Launch app
                        dfu_state = SATE_LAUNCH_APP;
                      break;
                      case PUSH_TYPE_LONG_I:   //15<=p<20
                        //Sleep again
                        dfu_state = STATE_SLEEP_PREPARE;
                      break;
                      
                      case PUSH_TYPE_LONG_II: //p>=20
                        //Boot mode
                        dfu_state = STATE_BOOT_MODE_INIT;
                      break;
                  }
                 // button_managment_attended(); 
              }
          }
          else
          {
             //Boot mode
             dfu_state = STATE_BOOT_MODE_INIT;
          }
          
          
        break;
       
       case STATE_BOOT_MODE_INIT:     
         dfu_state  = STATE_BOOT_MODE;
       break;
       
       case STATE_BOOT_MODE:
         //
       break;
          
       case STATE_SLEEP_PREPARE:
         //
       break;
    }
}

bool dfu_state_machine_exe(void)
{
    bool  sleep  =  true;
    switch(dfu_state)
    {
        case STATE_WAIT_RELEASE:
          //
        break;
        
        case STATE_BOOT_MODE_INIT:     
         //Initialize bootloader
          dfu_launch_management_launch_bootloader();
        
        break;
          
        case STATE_SLEEP_PREPARE:
         gpio_go_to_deep_sleep(true);
        break;
        
        case SATE_LAUNCH_APP:
          // Uninitialize the clock driver to prevent interfering with app.
          nrf_delay_ms(1000);
          nrf_drv_clock_uninit();
          NRF_LOG_DEBUG("Jumping to: 0x%08x", MAIN_APPLICATION_START_ADDR);
          nrf_bootloader_app_start(MAIN_APPLICATION_START_ADDR);
        
          //Not reached
        break;
    }
  
    
    return  sleep;
  
}



