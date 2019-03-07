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
* First written on 2018/05/17 by María Viqueira.
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
#include <stdbool.h>
#include <stdint.h>
#include <string.h>

#include "nordic_common.h"
#include "nrf.h"
#include "app_error.h"
#include "ble.h"
#include "ble_hci.h"
#include "ble_srv_common.h"
#include "ble_advdata.h"
#include "ble_advertising.h"
#include "ble_conn_params.h"
#include "boards.h"
#include "app_timer.h"
#include "fds.h"
#include "peer_manager.h"

#include "bsp.h"
#include "bsp_btn_ble.h"
//#include "sensorsim.h"
#include "nrf_gpio.h"
#include "ble_hci.h"
#include "ble_advdata.h"
#include "ble_advertising.h"
#include "ble_conn_state.h"

/***************************/
#include "nrf_log.h"
#include "nrf_log_ctrl.h"
#include "nrf_log_default_backends.h"

/***************************/
#include "bluetooth_management.h"
#include "uart_management.h"
#include "data_management.h"
#include "gpio_management.h"
#include "state_machine_management.h"
#include "timers_management.h"
#include "encryption_management.h"
#include "button_management.h"
#include "leds_management.h"
#include "battery_management.h"
#include "adc_batt_management.h"
#include "flash_management.h"
#include "connection_control_management.h"
#include "watchdog_management.h"
#include "adc_ntc_management.h"


//NFC pins as GPIO
const uint32_t UICR_ADDR_0x20C    __attribute__((at(0x1000120C))) __attribute__((used)) = 0xFFFFFFFE;

/*  Function Prototype Section
* Add prototypes for all functions called by this
* module, with the exception of runtime routines.
*
***************************************************/
/**@brief Function for the Power manager.
 */
static void power_manage(void)
{
    uint32_t err_code = sd_app_evt_wait();

    APP_ERROR_CHECK(err_code);
}



/**@brief Function for the Power manager.
 */
static void log_init(void)
{
    uint32_t err_code = NRF_LOG_INIT(NULL);
    APP_ERROR_CHECK(err_code);

    NRF_LOG_DEFAULT_BACKENDS_INIT();
}


/**@brief Function for application main entry.
 */
int main(void)
{ 
    bool        sleep;
 
    // Initialize.
    log_init();
    
		timers_management_init();
    connection_control_management_init();

    //Softdevice initialization
    bluetooth_management_ble_stack_init();

    flash_management_init();

    gpio_management_init();
    button_management_init(); 
    leds_management_init();


    data_management_init();
    timers_management_sec_start();
    battery_management_init();
  
    
    
    watchdog_management_init();
    battery_management_first_read();
    
    bluetooth_management_init(false);
    

    // Enter main loop.
    for (;;)
    {
        watchdog_management_feed();
         //Main control
         state_machine_trans();
         sleep  = state_machine_exe();
         leds_management_exe(); 
         battery_management_exe();
         if(sleep)
         {           
            power_manage();
            
         }
        
    }
}

