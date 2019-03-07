#ifndef FLASH_MANAGEMENT_H__
#define FLASH_MANAGEMENT_H__

#include <stdint.h>

#define FLASH_PAGE_CONFIG         0x72
#define TIME_BOOTLOADER           (20*1000)
#define TIME_CANCEL_BOOTLOADER    (5*1000)
#define TIME_WAKE_UP              (1500)

#define FLASH_MANAGEMENT_MIN_THRES_ENTER_BOOTLOADER      (10*100)
#define FLASH_MANAGEMENT_MAX_THRES_ENTER_BOOTLOADER      (30*1000)
#define FLASH_MANAGEMENT_MIN_THRES_CANCEL_BOOTLOADER     (3*1000)
#define FLASH_MANAGEMENT_MAX_THRES_CANCEL_BOOTLOADER     (7*1000)
#define FLASH_MANAGEMENT_MIN_THRES_WAKE_UP               (1500)
#define FLASH_MANAGEMENT_MAX_THRES_WAKE_UP               (5000)

/**************************************
* Functions
**************************************/ 
/**************************************************
* Function name	: void flash_management_init(void)
*    returns		: -----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/07/10
* Description		: Reads data from flash and initialize
*                 values
* Notes			    : restrictions, odd modes
**************************************************/
void flash_management_init(void);

/**************************************************
* Function name	: void flash_management_new_bootloader_timeout(uint32_t boot_time)
*    returns		: -----
*    args			  : uint32_t new bootloader time
* Created by		: María Viqueira
* Date created	: 2018/07/10
* Description		: Updates the value of the new bootloader
*                 time for store it at flash
* Notes			    : restrictions, odd modes
**************************************************/
void flash_management_new_bootloader_timeout(uint32_t boot_time);

/**************************************************
* Function name	: void flash_management_new_cancel_bootloader_timeout(uint32_t boot_time)
*    returns		: -----
*    args			  : uint32_t new cancel bootloader time
* Created by		: María Viqueira
* Date created	: 2018/07/10
* Description		: Updates the value of the new cancel 
*                 of bootloader time for store it at flash
* Notes			    : restrictions, odd modes
**************************************************/
void flash_management_new_cancel_bootloader_timeout(uint32_t boot_cancel_time);

/**************************************************
* Function name	: void flash_management_check_write(void)
*    returns		: -----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/07/10
* Description		: Check if it is necessary to store
*                 data in flash and sotres it
* Notes			    : restrictions, odd modes
**************************************************/
void flash_management_check_write(void);

/**************************************************
* Function name	: void flash_management_new_wake_up_from_sleep_timeout(uint32_t boot_time)
*    returns		: -----
*    args			  : uint32_t new cancel bootloader time
* Created by		: María Viqueira
* Date created	: 2018/07/12
* Description		: Updates the value of the new wake up  
*                 from sleep time for store it at flash
* Notes			    : restrictions, odd modes
**************************************************/
void flash_management_new_wake_up_from_sleep_timeout(uint32_t new_wake);

/**************************************************
* Function name	: void flash_management_flash_write_needed(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/25
* Description		: sets the flag which control flash write
* Notes			    : restrictions, odd modes
**************************************************/ 
void flash_management_flash_write_needed(void);

/**************************************************
* Function name	: uint32_t flash_management_get_bootloader_timeout(void)
*    returns		: uint32_t bootloader time at flash
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/07/10
* Description		: Returns the last bootloader time
* Notes			    : restrictions, odd modes
**************************************************/
uint32_t flash_management_get_bootloader_timeout(void);

/**************************************************
* Function name	: uint32_t flash_management_get_cancel_bootloader_timeout(void)
*    returns		: uint32_t cancel bootloader time at flash
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/07/10
* Description		: Returns the last cancel bootloader time
* Notes			    : restrictions, odd modes
**************************************************/
uint32_t flash_management_get_cancel_bootloader_timeout(void);

/**************************************************
* Function name	: uint32_t flash_management_get_wake_up_from_sleep_timeout(void)
*    returns		: uint32_t time for waking the device up from sleep
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/07/12
* Description		: Returns the time for waking the device up
* Notes			    : restrictions, odd modes
**************************************************/
uint32_t flash_management_get_wake_up_from_sleep_timeout(void);

#endif

