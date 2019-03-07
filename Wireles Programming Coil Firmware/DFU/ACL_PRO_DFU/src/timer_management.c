/***************************************************
* Module name: timer_management.c
*
* Copyright 2016 Bizintek Innova S.L. 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Bizintek Innova. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Bizintek Innova.
*
* First written on 2018/05/31 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for managing timer operations
*
***************************************************/


/*  Include section
* Add all #includes here
*
***************************************************/
#include "app_timer.h"
#include "timer_management.h"

/*  Defines section
* Add all #defines here
*
***************************************************/
#define APP_TIMER_OP_QUEUE_SIZE         20  // Size of timer operation queues. */
#define TIMESTAMP_INTERVAL							APP_TIMER_TICKS(1000, APP_TIMER_PRESCALER)

/*  Variables Section
* 
***************************************************/
APP_TIMER_DEF(m_timestamp_timer_id);  // Timestamp timer. */

static uint32_t	seconds_counter	=	0;


/*  Function Prototype Section
*
***************************************************/

	
static void timer_timestamp_timeout_handler(void * p_context)
{
    UNUSED_PARAMETER(p_context);
		//Incrementation of the seconds counter
		seconds_counter++;

}

/**@brief Function for the Timer initialization.
 *
 * @details Initializes the timer module. This creates and starts application timers.
 */
void timer_management_init(void)
{

    // Initialize timer module.
    APP_TIMER_INIT(APP_TIMER_PRESCALER, APP_TIMER_OP_QUEUE_SIZE, false);

    // Create timers.

   
	 uint32_t err_code;
	 err_code = app_timer_create(&m_timestamp_timer_id,
																APP_TIMER_MODE_REPEATED, 
																timer_timestamp_timeout_handler);
	 APP_ERROR_CHECK(err_code); 
	

}

void timer_management_start(void)
{
		uint32_t err_code;
	
		err_code = app_timer_start(m_timestamp_timer_id, TIMESTAMP_INTERVAL, NULL);
    APP_ERROR_CHECK(err_code);
	
}

uint32_t timer_manager_get_number_seconds(void)
{
		return seconds_counter;
}
