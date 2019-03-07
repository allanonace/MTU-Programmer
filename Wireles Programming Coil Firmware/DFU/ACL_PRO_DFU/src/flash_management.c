/***************************************************
* Module name: flash_management.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/07/10 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for managing the flash functions
*
***************************************************/
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "nordic_common.h"
#include "nrf.h"
#include "app_error.h"
#include "ble.h"
#include "ble_hci.h"
#include "ble_srv_common.h"
#include "boards.h"
#include "app_timer.h"
#include "nrf_delay.h"
#include "ble_flash.h"

#include "flash_management.h"
#include "button_management.h"



#define NO_DATA 0xFFFFFFFF
typedef enum
{
  POS_FLASH_UART_BAUDRATE,
  POS_FLASH_UART_PARITY,
  POS_FLASH_ADVERTISING_INTERVAL,
  POS_FLASH_ON_TIMEOUT,
  POS_FLASH_PAIRING_TIMEOUT,
  POS_FLASH_WAKE_UP_FROM_SLEEP,
  POS_FLASH_ON_TO_PAIRING,
  POS_FLASH_POWER_OFF,
  POS_FLASH_BOOTLADER,
  POS_FLASH_CANCEL_BOOTLADER,
  SIZE_FLASH_DATA
}positions_flash_t;



bool  flash_write_needed = false;
uint32_t  *p_page_base_address;

                                       

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
void flash_management_init(void)
{
      static uint32_t   *p_page_base_address ;
      static uint32_t   data_read;
      static uint32_t   min_thr, max_thr;
      
      uint32_t  aux_cancel_dfu     =  NO_DATA;
      uint32_t  aux_enter_dfu      =  NO_DATA;
      uint32_t  aux_wake_up        =  NO_DATA; 
  
      uint32_t  data_stored[SIZE_FLASH_DATA];
      p_page_base_address = (uint32_t *) (FLASH_PAGE_CONFIG * NRF_FICR->CODEPAGESIZE);
      
      memset(data_stored,0,sizeof(data_stored));
  
      //Read flash data
      memcpy(data_stored,p_page_base_address,SIZE_FLASH_DATA*sizeof(uint32_t));
      
      data_read = data_stored[POS_FLASH_WAKE_UP_FROM_SLEEP];
      min_thr   = FLASH_MANAGEMENT_MIN_THRES_WAKE_UP;
      max_thr   = FLASH_MANAGEMENT_MAX_THRES_WAKE_UP;
      if( (data_read <=  max_thr)  &&  (data_read >=  min_thr))
      {
          
          aux_wake_up = data_read;
      }

      data_read = data_stored[POS_FLASH_BOOTLADER];
      min_thr   = FLASH_MANAGEMENT_MIN_THRES_ENTER_BOOTLOADER;
      max_thr   = FLASH_MANAGEMENT_MAX_THRES_ENTER_BOOTLOADER;
      if( (data_read <=  max_thr)  &&  (data_read >=  min_thr))
      {
          //bootloader_timeout_flash = data_read; 
          aux_enter_dfu = data_read;
          
      }
            
      data_read = data_stored[POS_FLASH_CANCEL_BOOTLADER];
      min_thr   = FLASH_MANAGEMENT_MIN_THRES_CANCEL_BOOTLOADER;
      max_thr   = FLASH_MANAGEMENT_MAX_THRES_CANCEL_BOOTLOADER;
      if( (data_read <=  max_thr)  &&  (data_read >=  min_thr))
      {
          //cancel_bootloader_timeout_flash = data_read;
          aux_cancel_dfu  = data_read;
          
      }
      /***************************************************************************
      * Although application should verify it, we are going to add security and 
      * only update parameters if bootloader timeout meets all of the following
      * contidion:
      *   Bootloader time must be higher than wake up + cancel bootloader
      ****************************************************************************/      
      if (  aux_enter_dfu >=  (aux_cancel_dfu + aux_wake_up))
      {
          //We also verify thresholds
          if( (aux_wake_up  !=  NO_DATA)  &&  (aux_enter_dfu  !=  NO_DATA)  &&  (aux_cancel_dfu  !=  NO_DATA))
          {
            button_management_new_wake_up_length_value(aux_wake_up);
            button_management_new_enter_dfu_length_value(aux_enter_dfu);
            button_management_new_alarm_dfu_length_value(aux_enter_dfu  - aux_cancel_dfu);
          }
        
      }
         
}

