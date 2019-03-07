/***************************************************
* Module name: dfu_launch_management.c
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
* Module for controlling DFU operations when we are
* at bootoloader mode
***************************************************/

/*  Include section
* Add all #includes here
*
***************************************************/
#include "app_scheduler.h"
#include "nrf_ble_dfu.h"
#include "nrf_dfu_settings.h"
#include "nrf_dfu.h"
#include "nrf_log.h"
#include "nrf_log_ctrl.h"
#include "nrf_dfu_transport.h"
#include "nrf_drv_clock.h"
#include "nrf_dfu_req_handler.h"
#include "nrf_dfu_utils.h"


#include "dfu_launch_management.h"
#include "leds_management.h"
#include "gpio_management.h"
#include "battery_management.h"
#include "watchdog_management.h"

/**************************************************
* Function name	: void wait_for_event_main(void)
*    returns		: -----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/25
* Description		: Main loop when we are at BOOTLOADER state
* Notes			: restrictions, odd modes
**************************************************/
void wait_for_event_main(void)
{
    while (true)
    {
        battery_management_exe();
        leds_management_exe();
        app_sched_execute();
        if (!NRF_LOG_PROCESS())
        {
        #ifdef BLE_STACK_SUPPORT_REQD
            (void)sd_app_evt_wait();
            watchdog_management_feed();
        #else
            __WFE();
        #endif
        }
        if(advertising_timeout_expired())
        {
            if(nrf_dfu_app_is_valid())
            {
                //Launch app
                NVIC_SystemReset();
            }
            else
            {
              gpio_go_to_deep_sleep(false);
            }
        }
    }
}

/**************************************************
* Function name	: uint8_t dfu_launch_management_launch_bootloader(void)

*    returns		: -----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/25
* Description		: Function to launch bootloader
* Notes			: restrictions, odd modes
**************************************************/
void dfu_launch_management_launch_bootloader(void)
{
        uint32_t ret_val;

        scheduler_init();
        
        
        if (ret_val != NRF_SUCCESS)
        {
            NRF_LOG_ERROR("Could not initialize inactivity timer");
        }
        // Initializing transports
        ret_val = nrf_dfu_transports_init();
        if (ret_val != NRF_SUCCESS)
        {
            NRF_LOG_ERROR("Could not initalize DFU transport: 0x%08x", ret_val);
            
        }
        (void)nrf_dfu_req_handler_init();
        // This function will never return
        NRF_LOG_DEBUG("Waiting for events");
        
        
        
        wait_for_event_main();

        NRF_LOG_DEBUG("After waiting for events");
}
