/***************************************************
* Module name: main.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/05/31 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* SDK: 14
* Softdevice: s132_nrf52_5.1.0_softdevice
*
* 
*

***************************************************/


/*  Include section
* Add all #includes here
*
***************************************************/
#include <stdint.h>
#include "boards.h"
#include "nrf_mbr.h"
#include "nrf_bootloader.h"
#include "nrf_bootloader_app_start.h"
#include "nrf_dfu.h"
#include "nrf_log.h"
#include "nrf_log_ctrl.h"
#include "nrf_log_default_backends.h"
#include "app_error.h"
#include "app_error_weak.h"
#include "nrf_bootloader_info.h"
#include "nrf_delay.h"

#include "nrf_ble_dfu.h"
#include "nrf_dfu_settings.h"
#include "nrf_dfu.h"
#include "nrf_dfu_utils.h"
#include "nrf_power.h"
#include "nrf_dfu_req_handler.h"
#include "nrf_dfu_transport.h"
#include "nrf_drv_clock.h"

#include "dfu_states_management.h"
#include "timer_management.h"
#include "leds_management.h"
#include "button_management.h"
#include "gpio_management.h"
#include "pinout_ACL_PRO.h"
#include "pruebas.h"
#include "app_scheduler.h"
#include "dfu_launch_management.h"

#include "battery_management.h"
#include "adc_batt_management.h"
#include "flash_management.h"
#include "watchdog_management.h"
#include "adc_ntc_management.h"

#define	MAIN_BUTTON_PUSHED							 1	
#define CHARGER_NOT_CONNECTED            1

#define BOOTLOADER_BUTTON   (BSP_BUTTON_3)          /**< Button for entering DFU mode. */
const uint32_t UICR_ADDR_0x20C    __attribute__((at(0x1000120C))) __attribute__((used)) = 0xFFFFFFFE;

typedef enum
{
    RESET_POWER,
    RESET_BUTTON,
    RESET_USB
}reset_reason_t;

void app_error_fault_handler(uint32_t id, uint32_t pc, uint32_t info)
{
    NRF_LOG_ERROR("Received a fault! id: 0x%08x, pc: 0x%08x, info: 0x%08x", id, pc, info);
    NVIC_SystemReset();
}
void app_error_handler_bare(uint32_t error_code)
{
    (void)error_code;
    NRF_LOG_ERROR("Received an error: 0x%08x!", error_code);
    NVIC_SystemReset();
}
int main(void)
{
    uint32_t ret_val;
    uint8_t  reset_reason = RESET_POWER; 
    bool     sleep;

    (void) NRF_LOG_INIT(NULL);
    NRF_LOG_DEFAULT_BACKENDS_INIT();
  
    //Initialization  
  
    // Initialize the clock and timers.
    timer_management_init();
  
    
   
    //Button
    button_management_init();
  
    
  
    
    //Leds
    leds_managment_init();

    //GPIO
    gpio_management_init();

    //Battery
    battery_management_init();
    
    //Watchdog
    watchdog_management_init();
    
    battery_management_first_read();

    //First, check if USB is connected
    if(nrf_gpio_pin_read(USB_DETECT) !=  0)
    {
        reset_reason  = RESET_USB;
        nrf_gpio_pin_set(LED_RED);
    }
    //Next, check whether Button is pressed or not
    else if(nrf_gpio_pin_read(BUTTON_ACL)  ==  MAIN_BUTTON_PUSHED)
    {
        reset_reason  = RESET_BUTTON;
        //Button was presed. We act as if an interrupt happened
        button_management_button_pushed();
        
    }    
    else
    {
        gpio_go_to_deep_sleep(true);
    }
    
    //If reset is because button pressed, we need to check battery level. If is low, we go to sleep
    if(reset_reason ==  RESET_BUTTON)
    {
        
        if(battery_management_is_low_battery())
        {
            for (uint8_t i=0;i<10;i++)
            {
                nrf_gpio_pin_toggle(LED_RED);
                nrf_delay_ms(100);
            }
            nrf_gpio_pin_clear(LED_RED);
            gpio_go_to_deep_sleep(true);
        }
    }
        
    //Flash
    flash_management_init();
  /*****************************************************************/
    uint32_t enter_bootloader_mode = 0;
    NRF_LOG_DEBUG("In real nrf_dfu_init");
    nrf_dfu_settings_init(false);
        
    // Continue ongoing DFU operations
    // Note that this part does not rely on SoftDevice interaction
    ret_val = nrf_dfu_continue(&enter_bootloader_mode);
    if (ret_val != NRF_SUCCESS)
    {
        NRF_LOG_DEBUG("Could not continue DFU operation: 0x%08x", ret_val);
        enter_bootloader_mode = 1;
    }
 
    if(reset_reason ==  RESET_BUTTON)
    {
        if(nrf_dfu_app_is_valid())
        {
         
          leds_management_init_valid_app();
        }
        else
        {
          leds_management_init_dfu_mode();
        }
    }
    
    //If button was presehed and we don't have a valid application, we launch bootloader
    if (!nrf_dfu_app_is_valid() &&  (reset_reason ==  RESET_BUTTON)  )
    {
        dfu_launch_management_launch_bootloader();  
    }
    
    // Erase additional data like peer data or advertisement name
    ret_val = nrf_dfu_settings_additional_erase();
    if (ret_val != NRF_SUCCESS)
    {
        return ret_val;
    }
    
   /*****************************************************************/
   
    /*****************************************************
    * We are here for two possible reaseons:
    *   1. Button was pushed and we have a valid app
    *   2. USB is connected
    *****************************************************/
    
    while(1)
    {
        sleep = true;
        if(reset_reason ==  RESET_USB)
        {
            //If we are not connected, sleep
            if(nrf_gpio_pin_read(USB_DETECT)  ==  0)
            {
                gpio_go_to_deep_sleep(true);
            }
            
            //If Temperature is high, sleep
            if(battery_management_is_ntc_high())
            {
                gpio_go_to_deep_sleep(true);
            }
            //If we are charged, sleep
            if(nrf_gpio_pin_read(CHARGER_STAT)  !=0)
            {
              //TODO: not sleeping
               //gpio_go_to_deep_sleep(true);
            }
            
        }
        watchdog_management_feed();
      
        if(reset_reason ==  RESET_BUTTON)
        {
          dfu_state_machine_trans();
          sleep = dfu_state_machine_exe();
        }
      
        battery_management_exe();
        if(reset_reason ==  RESET_BUTTON)
        {
          leds_management_exe();
        }
        
        if(sleep)
        {
          sd_app_evt_wait();
          
        }
    }
  
 
}
