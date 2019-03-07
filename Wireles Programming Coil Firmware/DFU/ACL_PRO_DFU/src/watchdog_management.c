/***************************************************
* Module name: watchdog_management.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/08/01 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for controlling watchdog operations
*
***************************************************/

/*  Include section
* Add all #includes here
*
***************************************************/
#include <stdint.h>
#include <string.h>
#include "nordic_common.h"
#include "nrf.h"
#include "boards.h"
#include "watchdog_management.h"

/*  Variables section
* Add all variables here
*
***************************************************/
static bool FLAG_WDT_INIT =	false;  

/*  Function Prototype Section
* Add prototypes for all functions called by this
* module, with the exception of runtime routines.
*
***************************************************/

/**************************************************
* Function name	: void watchdog_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/08/01
* Description		: Function for starting watchdog
* Notes			: restrictions, odd modes
**************************************************/
void watchdog_management_init(void)
{
	
	if(!FLAG_WDT_INIT)
	{
			NRF_WDT->CONFIG = (WDT_CONFIG_HALT_Pause << WDT_CONFIG_HALT_Pos) | ( WDT_CONFIG_SLEEP_Pause << WDT_CONFIG_SLEEP_Pos);
			NRF_WDT->CRV = WATCHDOG_SECONDS*32768; 
			NRF_WDT->RREN |= WDT_RREN_RR0_Msk;  //Enable reload register 0
			NRF_WDT->TASKS_START = 1;
			FLAG_WDT_INIT	=	true;
	}
}

/**************************************************
* Function name	: void watchdog_management_stop(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/08/01
* Description		: Function for stoppping watchdog
* Notes			: restrictions, odd modes
**************************************************/
void watchdog_management_stop(void)
{
	NRF_WDT->TASKS_START  = 0;
  FLAG_WDT_INIT         = false;
}

/**************************************************
* Function name	: void watchdog_management_feed(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/08/01
* Description		: Function for feeding watchdog
* Notes			: restrictions, odd modes
**************************************************/
void watchdog_management_feed(void)
{
	if(FLAG_WDT_INIT)
	{
		NRF_WDT->RR[0] = WDT_RR_RR_Reload;
	}
}

