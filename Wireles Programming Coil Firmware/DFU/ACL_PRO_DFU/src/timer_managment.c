/***************************************************
* Module name: timer_management.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/06/14 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for controlling timer operations
*
***************************************************/

/*  Include section
* Add all #includes here
*
***************************************************/
#include "nrf_drv_clock.h"
#include "app_timer.h"
#include "nrf_dfu.h"
#include "nrf_dfu.h"
#include "nrf_ble_dfu.h"


/*  Defines section
* Add all #defines here
*
***************************************************/
/**@brief Function for the Timer initialization.
 *
 * @details Initializes the timer module. This creates and starts application timers.
 */
void timer_management_init(void)
{
  clock_init();
  timers_init();
      
}
