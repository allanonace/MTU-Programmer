/***************************************************
* Module name: timers_management.c
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
* Module for managing timer operations
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
#include "nrf_log.h"
#include "nrf_log_ctrl.h"
/*****************************************/
#include "timers_management.h"
#include "connection_control_management.h"
#include "data_management.h"

/*  Defines section
* Add all #defines here
*
***************************************************/
#define APP_TIMER_OP_QUEUE_SIZE         9     /**< Size of timer operation queues. */


APP_TIMER_DEF(m_timer_pairing_id);
APP_TIMER_DEF(m_timer_sec_id);

/*  Variables section
* Add all variables here
*
***************************************************/
bool  FLAG_TIMER_PAIRING  = false;
bool  FLAG_TIMER_SEC      = false;
bool  paired_expired      = false;
bool  timeout_expired     = false;

uint32_t  milli_seconds_counter = 0;
uint32_t  timeout_no_activity = TIMER_NO_ACTIVITY;
uint32_t  pairing_timeout_value = TIMER_PAIRING;

/*  Function Prototype Section
* Add prototypes for all functions called by this
* module, with the exception of runtime routines.
*
***************************************************/
//void timers_management_control_paired_timmer_attended(void)
//{ 
//  //TODO: DO NOT CALL THIS FUNCTCION
//    //paired_expired  = false;
//}

/**************************************************
* Function name	: void timers_management_control_pairing_start(void)
*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/20
* Description		: Starts pairing timer
* Notes			    : restrictions, odd modes
**************************************************/
void timers_management_control_pairing_start(void)
{	
    static uint32_t err_code;
		if(!FLAG_TIMER_PAIRING)
		{
				err_code  = app_timer_start(m_timer_pairing_id,APP_TIMER_TICKS(pairing_timeout_value),NULL);
        APP_ERROR_CHECK(err_code);
				FLAG_TIMER_PAIRING	=	true;
		}
    paired_expired  = false;
}

/**************************************************
* Function name	: void timer_control_pairing_meas_timeout_handler(void * p_context)
*    returns		: ----
*    args			  : unused pointer
* Created by		: María Viqueira
* Date created	: 2018/06/20
* Description		: Interruption of pairing timer
* Notes			    : restrictions, odd modes
**************************************************/
void timer_control_pairing_meas_timeout_handler(void * p_context)
{
		UNUSED_PARAMETER(p_context);
		FLAG_TIMER_PAIRING	=	false;
    paired_expired  = true;
}

/**************************************************
* Function name	: void timers_management_control_pairing_stop(void)
*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/20
* Description		: Stops pairing timer
* Notes			    : restrictions, odd modes
**************************************************/
void timers_management_control_pairing_stop(void)
{
		if(FLAG_TIMER_PAIRING)
		{
				app_timer_stop(m_timer_pairing_id);
				FLAG_TIMER_PAIRING	=	false;        
		}
    paired_expired  = false;
}

/***********************************************/
/**************************************************
* Function name	: void timers_management_sec_start(void)
*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/20
* Description		: Starts inactivity timer
* Notes			    : restrictions, odd modes
**************************************************/
void timers_management_sec_start(void)
{	
    static uint32_t  err_code;
    timeout_expired = false;
    milli_seconds_counter = 0;
		if(!FLAG_TIMER_SEC)
		{
				err_code  = app_timer_start(m_timer_sec_id,TIMER_SEC,NULL);
        APP_ERROR_CHECK(err_code);
				FLAG_TIMER_SEC	=	true;
		}
    
}

/**************************************************
* Function name	: void timer_sec_meas_timeout_handler(void * p_context)
*    returns		: ----
*    args			  : unused pointer
* Created by		: María Viqueira
* Date created	: 2018/06/20
* Description		: Interruption of inactivity timer
* Notes			    : restrictions, odd modes
**************************************************/
void timer_sec_meas_timeout_handler(void * p_context)
{
		UNUSED_PARAMETER(p_context);
		milli_seconds_counter++;
    if(milli_seconds_counter>=timeout_no_activity)
    {
      timeout_expired  = true;
    }
}

/**************************************************
* Function name	: void timers_management_sec_stop(void)
*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/20
* Description		: Stops inactivity timer
* Notes			    : restrictions, odd modes
**************************************************/
void timers_management_sec_stop(void)
{
		if(FLAG_TIMER_SEC)
		{
				app_timer_stop(m_timer_sec_id);
				FLAG_TIMER_SEC	=	false;        
		}
    
}
/**@brief Function for the Timer initialization.
 *
 * @details Initializes the timer module. This creates and starts application timers.
 */
 void timers_management_init(void)
{
    uint32_t err_code;
  
    // Initialize timer module.
    err_code = app_timer_init();
    APP_ERROR_CHECK(err_code);
  
    err_code = app_timer_create(&m_timer_pairing_id,APP_TIMER_MODE_SINGLE_SHOT,timer_control_pairing_meas_timeout_handler);
    APP_ERROR_CHECK(err_code);
  
    err_code = app_timer_create(&m_timer_sec_id,APP_TIMER_MODE_REPEATED,timer_sec_meas_timeout_handler);
    APP_ERROR_CHECK(err_code);
  
    data_management_create_timer();
    
  
    connection_control_management_create_timer();
   
}

/**************************************************
* Function name	: void timers_management_reset_on_timeout(void)
*    returns		: ----
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/20
* Description		: Resets the counter of inactivity timeout
* Notes			    : restrictions, odd modes
**************************************************/
void timers_management_reset_on_timeout(void)
{
    milli_seconds_counter = 0;
}

/**************************************************
* Function name	: bool  timers_management_get_on_timeout_status(void)
*    returns		: true if ON timeout was expired, otherwise: false
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/20
* Description		: Indicates if inactivity timeout was expired or not
* Notes			    : restrictions, odd modes
**************************************************/
bool  timers_management_get_on_timeout_status(void)
{
    return timeout_expired;
}

/**************************************************
* Function name	: bool timers_management_get_pairing_timer_status(void)
*    returns		: bool: true if timeout of paring was
*                       expired, otherwise: false
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/20
* Description		: indicates whether paired timeout expired 
* Notes			    : restrictions, odd modes
**************************************************/
bool timers_management_get_pairing_timer_status(void)
{
    return paired_expired;
}

/**************************************************
* Function name	: uint32_t  timers_management_get_on_timeout_value(void)
*    returns		: uint32_t current value for no activity
*    args			  : ---
* Created by		: María Viqueira
* Date created	: 2018/07/16
* Description		: return the value of inactivity for going
*                 to SLEEP
* Notes			: restrictions, odd modes
**************************************************/
uint32_t  timers_management_get_on_timeout_value(void) 
{
    return timeout_no_activity;
}

/**************************************************
* Function name	: void timers_management_new_on_timeout(uint32_t new_time)
*    returns		: ----
*    args			  : uint32_t new inactivity timeout
* Created by		: María Viqueira
* Date created	: 2018/07/16
* Description		: updates value of the inactivity timeout
* Notes			: restrictions, odd modes
**************************************************/
void timers_management_new_on_timeout(uint32_t new_time)
{
    timeout_no_activity = new_time;
}

/**************************************************
* Function name	: uint32_t  timers_management_get_pairing_timeout_value(void)
*    returns		: uint32_t current value for no activity at PAIRING
*    args			  : ---
* Created by		: María Viqueira
* Date created	: 2018/07/16
* Description		: return the value of inactivity for going
*                 from PAIRING to ON
* Notes			: restrictions, odd modes
**************************************************/
uint32_t  timers_management_get_pairing_timeout_value(void)
{
    return pairing_timeout_value;
}

/**************************************************
* Function name	: void timers_management_new_pairing_timeout(uint32_t new_time)
*    returns		: ----
*    args			  : uint32_t new PAIRING timeout
* Created by		: María Viqueira
* Date created	: 2018/07/16
* Description		: updates value of the PAIRING timeout
* Notes			: restrictions, odd modes
**************************************************/
void timers_management_new_pairing_timeout(uint32_t new_time)
{
    pairing_timeout_value = new_time;
}

