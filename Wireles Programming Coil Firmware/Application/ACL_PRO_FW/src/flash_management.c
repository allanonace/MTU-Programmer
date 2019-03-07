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
#include "nrf_delay.h"
#include "ble_flash.h"

#include "flash_management.h"
#include "bluetooth_management.h"
#include "timers_management.h"
#include "button_management.h"
#include "uart_management.h"



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




static uint32_t bootloader_timeout_flash            = TIME_BOOTLOADER;
static uint32_t cancel_bootloader_timeout_flash     = TIME_CANCEL_BOOTLOADER;
static uint32_t wake_up_from_sleep_timeout_flash    = TIME_WAKE_UP;

bool  flash_write_needed = false;
uint32_t  *p_page_base_address;

                                       
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
void flash_management_check_write(void)
{
    uint32_t data_to_store[SIZE_FLASH_DATA];
  
    
    
    data_to_store[POS_FLASH_UART_BAUDRATE]          = uart_management_get_baudrate(); 
    data_to_store[POS_FLASH_UART_PARITY]            = uart_management_get_parity();
    data_to_store[POS_FLASH_ADVERTISING_INTERVAL]   = bluetooth_management_get_adv_interval();
    data_to_store[POS_FLASH_ON_TIMEOUT]             = timers_management_get_on_timeout_value();
    data_to_store[POS_FLASH_PAIRING_TIMEOUT]        = timers_management_get_pairing_timeout_value();
    data_to_store[POS_FLASH_WAKE_UP_FROM_SLEEP]     = wake_up_from_sleep_timeout_flash;
    data_to_store[POS_FLASH_ON_TO_PAIRING]  = button_management_get_on_to_pairing_value();
    data_to_store[POS_FLASH_POWER_OFF]              = button_management_get_power_off_value();
    data_to_store[POS_FLASH_BOOTLADER]              = bootloader_timeout_flash;
    data_to_store[POS_FLASH_BOOTLADER]              = bootloader_timeout_flash;
    data_to_store[POS_FLASH_CANCEL_BOOTLADER]       = cancel_bootloader_timeout_flash;
    
  
    p_page_base_address = (uint32_t *) (FLASH_PAGE_CONFIG * NRF_FICR->CODEPAGESIZE);
  
    if(flash_write_needed)
    {
        //Stop Softdevice
       bluetooth_management_softdevice_stop();
       ble_flash_page_erase(FLASH_PAGE_CONFIG); 
       ble_flash_block_write(p_page_base_address,data_to_store,SIZE_FLASH_DATA);
       bluetooth_management_ble_stack_init();
       flash_write_needed = false; 
    }
}

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
  
      uint32_t  aux_on_to_pairing  =  NO_DATA;
      uint32_t  aux_power_off      =  NO_DATA;
      uint32_t  aux_cancel_dfu     =  NO_DATA;
      uint32_t  aux_enter_dfu      =  NO_DATA;
      uint32_t  aux_wake_up        =  NO_DATA; 
  
      uint32_t  data_stored[SIZE_FLASH_DATA];
      p_page_base_address = (uint32_t *) (FLASH_PAGE_CONFIG * NRF_FICR->CODEPAGESIZE);
      
      memset(data_stored,0,sizeof(data_stored));
  
      //Read flash data
      memcpy(data_stored,p_page_base_address,SIZE_FLASH_DATA*sizeof(uint32_t));
      
      /**************************************************
      * UART Baudrate
      **************************************************/
      data_read = data_stored[POS_FLASH_UART_BAUDRATE];
      data_read = uart_management_num2code(data_read);
      
      if( data_read !=  NO_DATA)
      {
          uart_management_new_baud(data_read);
      }
      
      /**************************************************
      * UART Parity
      **************************************************/      
      data_read = data_stored[POS_FLASH_UART_PARITY];
      
      if(data_read !=  NO_DATA)
      {
          if(data_read ==  0)
          {
              uart_management_new_parity(false);
          }
          else
          {
              uart_management_new_parity(true);
          }
      }
      
      /**************************************************
      * Advertising Interval
      **************************************************/      
      data_read = data_stored[POS_FLASH_ADVERTISING_INTERVAL];
      min_thr   = BLUETOOTH_MANAGEMENT_MIN_ADV_INTERVAL;
      max_thr   = BLUETOOTH_MANAGEMENT_MAX_ADV_INTERVAL;
      
      if( (data_read <=  max_thr)  &&  (data_read >=  min_thr))
      {
          bluetooth_management_new_adv_interval(data_read);
      }
      
      /**************************************************
      * ON Timeout
      **************************************************/      
      data_read = data_stored[POS_FLASH_ON_TIMEOUT];
      min_thr   = TIMERS_MANGEMENT_MIN_THRES_ON_TIMEOUT;
      max_thr   = TIMERS_MANGEMENT_MAX_THRES_ON_TIMEOUT;
      if( (data_read <=  max_thr)  &&  (data_read >=  min_thr))
      {
          timers_management_new_on_timeout(data_read);
      }
      
      /**************************************************
      * PAIRING Timeout
      **************************************************/      
      data_read = data_stored[POS_FLASH_PAIRING_TIMEOUT];
      min_thr   = TIMERS_MANGEMENT_MIN_THRES_PAIRING_TIMEOUT;
      max_thr   = TIMERS_MANGEMENT_MAX_THRES_PAIRING_TIMEOUT;
      if( (data_read <=  max_thr)  &&  (data_read >=  min_thr))
      {
          timers_management_new_pairing_timeout(data_read);
      }
      
      
      
      //Concordance: ON to PAIRING, at least 1 second higher than Power Off
      /**************************************************
      * ON to PAIRING
      **************************************************/      
      data_read = data_stored[POS_FLASH_ON_TO_PAIRING];
      min_thr   = BUTTON_MANAGEMENT_MIN_THRES_ON_TO_PAIRING;
      max_thr   = BUTTON_MANAGEMENT_MAX_THRES_ON_TO_PAIRING;
      if( (data_read <=  max_thr)  &&  (data_read >=  min_thr))
      {
          
          aux_on_to_pairing = data_read;
      }
      
      /**************************************************
      * Power Off
      **************************************************/      
      data_read = data_stored[POS_FLASH_POWER_OFF];
      min_thr   = BUTTON_MANAGEMENT_MIN_THRES_POWER_OFF;
      max_thr   = BUTTON_MANAGEMENT_MAX_THRES_POWER_OFF;
      if( (data_read <=  max_thr)  &&  (data_read >=  min_thr))
      {
          
          aux_power_off = data_read;
      }
      
      //Check if we can update:
      if( (aux_on_to_pairing  >=  (aux_power_off  + BUTTON_MANAGEMENT_MIN_DISTANCE_BETWEEN_PULSE))  )
      {
          //Last check: both must be between thresholds 
          if( (aux_on_to_pairing  !=  NO_DATA)  &&  ((aux_power_off  !=  NO_DATA)))
          {
              //Update
              button_management_new_power_off_value(aux_power_off);
              button_management_new_on_to_pairing_value(aux_on_to_pairing);
              
          }
      }
      
       //Bootloader higher than Cancel bootloader +  wake up from sleep
      /**************************************************
      * Wake up from SLEEP
      **************************************************/      
      data_read = data_stored[POS_FLASH_WAKE_UP_FROM_SLEEP];
      min_thr   = FLASH_MANAGEMENT_MIN_THRES_WAKE_UP;
      max_thr   = FLASH_MANAGEMENT_MAX_THRES_WAKE_UP;
      if( (data_read <=  max_thr)  &&  (data_read >=  min_thr))
      {
          aux_wake_up = data_read;
      }
      
      /**************************************************
      * Enter Bootloader
      **************************************************/      
      data_read = data_stored[POS_FLASH_BOOTLADER];
      min_thr   = FLASH_MANAGEMENT_MIN_THRES_ENTER_BOOTLOADER;
      max_thr   = FLASH_MANAGEMENT_MAX_THRES_ENTER_BOOTLOADER;
      if( (data_read <=  max_thr)  &&  (data_read >=  min_thr))
      {
          
          aux_enter_dfu = data_read;
      }
      
      /**************************************************
      * Cancel Bootloader
      **************************************************/      
      data_read = data_stored[POS_FLASH_CANCEL_BOOTLADER];
      min_thr   = FLASH_MANAGEMENT_MIN_THRES_CANCEL_BOOTLOADER;
      max_thr   = FLASH_MANAGEMENT_MAX_THRES_CANCEL_BOOTLOADER;
      if( (data_read <=  max_thr)  &&  (data_read >=  min_thr))
      {
          
          aux_cancel_dfu  = data_read;
      }
      
      //Check if we can update:
      if( aux_enter_dfu  >= (aux_cancel_dfu + aux_wake_up))
      {
          //All of them with valid data
          if( (aux_wake_up  !=  NO_DATA)  &&  (aux_cancel_dfu  !=  NO_DATA)  &&  (aux_enter_dfu  !=  NO_DATA)  )
          {
              //Update
              flash_management_new_wake_up_from_sleep_timeout(aux_wake_up);
              flash_management_new_bootloader_timeout(aux_enter_dfu);
              flash_management_new_cancel_bootloader_timeout(aux_cancel_dfu);

          }
      }
      
      flash_write_needed = false;
}


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
void flash_management_new_bootloader_timeout(uint32_t boot_time)
{
     
      bootloader_timeout_flash  = boot_time;
      flash_write_needed  = true;
  
      
 }

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
void flash_management_new_cancel_bootloader_timeout(uint32_t boot_cancel_time)
{
     
      cancel_bootloader_timeout_flash  = boot_cancel_time;
      flash_write_needed  = true;
     
}

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
void flash_management_new_wake_up_from_sleep_timeout(uint32_t new_wake)
{
     
      wake_up_from_sleep_timeout_flash  = new_wake;
      flash_write_needed  = true;
     
}

/**************************************************
* Function name	: uint32_t flash_management_get_bootloader_timeout(void)
*    returns		: uint32_t bootloader time at flash
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/07/10
* Description		: Returns the last bootloader time
* Notes			    : restrictions, odd modes
**************************************************/
uint32_t flash_management_get_bootloader_timeout(void)
{
    return bootloader_timeout_flash;
}


/**************************************************
* Function name	: uint32_t flash_management_get_cancel_bootloader_timeout(void)
*    returns		: uint32_t cancel bootloader time at flash
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/07/10
* Description		: Returns the last cancel bootloader time
* Notes			    : restrictions, odd modes
**************************************************/
uint32_t flash_management_get_cancel_bootloader_timeout(void)
{
    return cancel_bootloader_timeout_flash;
}

/**************************************************
* Function name	: uint32_t flash_management_get_wake_up_from_sleep_timeout(void)
*    returns		: uint32_t time for waking the device up from sleep
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/07/12
* Description		: Returns the time for waking the device up
* Notes			    : restrictions, odd modes
**************************************************/
uint32_t flash_management_get_wake_up_from_sleep_timeout(void)
{
    return wake_up_from_sleep_timeout_flash;
}

/**************************************************
* Function name	: void flash_management_flash_write_needed(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/25
* Description		: sets the flag which control flash write
* Notes			    : restrictions, odd modes
**************************************************/ 
void flash_management_flash_write_needed(void)
{
    flash_write_needed  = true;
}
