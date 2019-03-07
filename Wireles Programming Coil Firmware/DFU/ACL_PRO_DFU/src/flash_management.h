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


#endif

