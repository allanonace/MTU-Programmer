/***************************************************
* Module name: time_configuration_service.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/07/03 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for controlling the operations regarding to
* the Configuration of the physical TIME
*
***************************************************/

/*  Include section
* Add all #includes here
*
***************************************************/

#include <string.h>
#include <stdint.h>

#include "ble_time_configuration_service.h"
#include "ble_time_configuration_service_char_on_timeout.h"
#include "ble_time_configuration_service_char_advertising.h"
#include "ble_time_configuration_service_char_pairing_timeout.h"
#include "ble_time_configuration_service_char_wake_up_from_sleep.h"
#include "ble_time_configuration_service_char_on_to_pairing.h"
#include "ble_time_configuration_service_char_power_off.h"  
#include "ble_time_configuration_service_char_bootloader.h"
#include "ble_time_configuration_service_char_cancel_bootloader.h"



extern ble_time_configuration_service_t       m_ble_time_configuration_service;


/**@brief Function for handling the Connect event.
 *
 * @param[in]   p_time_configuration_service        Time configuration Service structure.
 * @param[in]   p_time_configuration_service_init   Information needed to initialize the service.
 */
static void on_connect(ble_time_configuration_service_t * p_time_configuration_service, ble_evt_t * p_ble_evt)
{
    p_time_configuration_service->conn_handle = p_ble_evt->evt.gap_evt.conn_handle;
}

/**@brief Function for handling the Disconnect event.
 *
 * @param[in]   p_time_configuration_service        Time configuration Service structure.
 * @param[in]   p_time_configuration_service_init   Information needed to initialize the service.
 */
static void on_disconnect(ble_time_configuration_service_t * p_time_configuration_service, ble_evt_t * p_ble_evt)
{
    UNUSED_PARAMETER(p_ble_evt);
    p_time_configuration_service->conn_handle = BLE_CONN_HANDLE_INVALID;
}

///**@brief Function for handling the Write event.
// *
// * @param[in]  p_time_configuration_service        Time configuration Service structure.
// * @param[in]  p_time_configuration_service_init   Information needed to initialize the service.
// */
static void on_write(ble_time_configuration_service_t * p_time_configuration_service, ble_evt_t * p_ble_evt)
{
      ble_gatts_evt_write_t * p_evt_write = &p_ble_evt->evt.gatts_evt.params.write;
  
			/********************************************************************************
      * Check whether is the handle and length of on_timeout configuration
			********************************************************************************/
		 if ((p_evt_write->handle == p_time_configuration_service->time_configuration_on_timeout.value_handle) &&
            (p_evt_write->len == LEN_ON_TIMEOUT_CHAR))
		 {

				p_time_configuration_service->write_on_timeout_handler(p_time_configuration_service, p_evt_write->data); 
		 }
     
		 /********************************************************************************
      * Check whether is the handle and length of advertising configuration
			********************************************************************************/
      if ((p_evt_write->handle == p_time_configuration_service->time_configuration_advertising.value_handle) &&
            (p_evt_write->len == LEN_ADVERTISING_CHAR))
		 {

				p_time_configuration_service->write_advertising_handler(p_time_configuration_service, p_evt_write->data); 
		 }
     
          
		  /********************************************************************************
      * Check whether is the handle and length of pairing_timeout configuration
			********************************************************************************/
      if ((p_evt_write->handle == p_time_configuration_service->time_configuration_pairing_timeout.value_handle) &&
            (p_evt_write->len == LEN_PAIRING_TIMEOUT_CHAR))
		 {

				p_time_configuration_service->write_pairing_timeout_handler(p_time_configuration_service, p_evt_write->data); 
		 }
     
     /********************************************************************************
      * Check whether is the handle and length of wake_up_from_sleep configuration
			********************************************************************************/
      if ((p_evt_write->handle == p_time_configuration_service->time_configuration_wake_up_from_sleep.value_handle) &&
            (p_evt_write->len == LEN_WAKE_UP_FROM_SLEEP_CHAR))
		 {

				p_time_configuration_service->write_wake_up_from_sleep_handler(p_time_configuration_service, p_evt_write->data); 
		 }
     
      /********************************************************************************
      * Check whether is the handle and length of on_to_pairing configuration
			********************************************************************************/
      if ((p_evt_write->handle == p_time_configuration_service->time_configuration_on_to_pairing.value_handle) &&
            (p_evt_write->len == LEN_ON_TO_PAIRING_CHAR))
		 {

				p_time_configuration_service->write_on_to_pairing_handler(p_time_configuration_service, p_evt_write->data); 
		 }
     
      /********************************************************************************
      * Check whether is the handle and length of power_off configuration
			********************************************************************************/
      if ((p_evt_write->handle == p_time_configuration_service->time_configuration_power_off.value_handle) &&
            (p_evt_write->len == LEN_POWER_OFF_CHAR))
		 {

				p_time_configuration_service->write_power_off_handler(p_time_configuration_service, p_evt_write->data); 
		 }
     
     
      /********************************************************************************
      * Check whether is the handle and length of bootloader configuration
			********************************************************************************/
      if ((p_evt_write->handle == p_time_configuration_service->time_configuration_bootloader.value_handle) &&
            (p_evt_write->len == LEN_BOOTLOADER_CHAR))
		 {

				p_time_configuration_service->write_bootloader_handler(p_time_configuration_service, p_evt_write->data); 
		 }
     
      /********************************************************************************
      * Check whether is the handle and length of cancel_bootloader configuration
			********************************************************************************/
      if ((p_evt_write->handle == p_time_configuration_service->time_configuration_cancel_bootloader.value_handle) &&
            (p_evt_write->len == LEN_CANCEL_BOOTLOADER_CHAR))
		 {

				p_time_configuration_service->write_cancel_bootloader_handler(p_time_configuration_service, p_evt_write->data); 
		 }
}

/**************************************************
* Function name	: void ble_time_configuration_service_on_ble_evt(ble_time_configuration_service_t *p_time_configuration_service,
                  *ble_evt_t *p_ble_evt)
*    returns		: ---
*    arg1			: pointer to Time Configuration service structure
*    arg2			: pointer to structure ble_evt_t (Bluetooth event)  
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/07/03
* Description		: BLE events for Time Configuration Service. 
* Notes			: restrictions, odd modes
**************************************************/
void ble_time_configuration_service_on_ble_evt(ble_time_configuration_service_t *p_time_configuration_service,ble_evt_t *p_ble_evt)
{
		
		switch (p_ble_evt->header.evt_id)
    {
        case BLE_GAP_EVT_CONNECTED:
						
            on_connect(p_time_configuration_service, p_ble_evt);
            break;

        case BLE_GAP_EVT_DISCONNECTED:
						
            on_disconnect(p_time_configuration_service, p_ble_evt);

            break;

        case BLE_GATTS_EVT_WRITE:
						
            on_write(p_time_configuration_service, p_ble_evt);
            break;
				

        default:
				
            // No implementation needed.
            break;
    }
}



/**************************************************
* Function name	: uint32_t ble_time_configuration_service_init(ble_time_configuration_service_t *p_time_configuration_service, 
*                 const ble_time_configuration_service_init_t *p_time_configuration_service_init)

*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to Time Configuration service structure
*    arg2			: pointer p_time_configuration_service for the service initialization
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/03/17
* Description		: initializes Time Configuration Service and adds its corresponding UUID
* Notes			: restrictions, odd modes
**************************************************/
uint32_t ble_time_configuration_service_init(ble_time_configuration_service_t *p_time_configuration_service, const ble_time_configuration_service_init_t *p_time_configuration_service_init)
{
		uint32_t   err_code;
    ble_uuid_t ble_uuid;
	
		ble_uuid128_t base_uuid = BLE_UUID_TIME_CONFIGURATION_SERVICE_BASE;
		err_code = sd_ble_uuid_vs_add(&base_uuid, &p_time_configuration_service->uuid_type);

		//Service structure intialization
		p_time_configuration_service->evt_handler	=	p_time_configuration_service_init->evt_handler;
		p_time_configuration_service->conn_handle	=	BLE_CONN_HANDLE_INVALID;
    
	
		//Write handles
		p_time_configuration_service->write_advertising_handler	        = p_time_configuration_service_init->write_advertising_handler;
		p_time_configuration_service->write_on_timeout_handler	        = p_time_configuration_service_init->write_on_timeout_handler;
    p_time_configuration_service->write_pairing_timeout_handler	    = p_time_configuration_service_init->write_pairing_timeout_handler;
    p_time_configuration_service->write_wake_up_from_sleep_handler	= p_time_configuration_service_init->write_wake_up_from_sleep_handler;
    p_time_configuration_service->write_on_to_pairing_handler	      = p_time_configuration_service_init->write_on_to_pairing_handler;
    p_time_configuration_service->write_power_off_handler	          = p_time_configuration_service_init->write_power_off_handler;
    p_time_configuration_service->write_bootloader_handler	        = p_time_configuration_service_init->write_bootloader_handler;
    p_time_configuration_service->write_cancel_bootloader_handler	  = p_time_configuration_service_init->write_cancel_bootloader_handler;


			
	
		//Adding service
		ble_uuid.type=p_time_configuration_service->uuid_type;
		ble_uuid.uuid=BLE_UUID_TIME_CONFIGURATION_SERVICE;
	
	
		err_code = sd_ble_gatts_service_add(BLE_GATTS_SRVC_TYPE_PRIMARY, &ble_uuid, &p_time_configuration_service->service_handle);
    if (err_code != NRF_SUCCESS)    
        return err_code;
    
		//Adding characteristics
		
		//on_timeout characteristic
		err_code = ble_time_configuration_service_on_timeout_add(p_time_configuration_service, p_time_configuration_service_init);
    if (err_code != NRF_SUCCESS)
    {
        return err_code;
    }
    
    //advertising characteristic
		err_code = ble_time_configuration_service_advertising_add(p_time_configuration_service, p_time_configuration_service_init);
    if (err_code != NRF_SUCCESS)
    {
        return err_code;
    }
		
    //pairing_timeout characteristic
    err_code = ble_time_configuration_service_pairing_timeout_add(p_time_configuration_service, p_time_configuration_service_init);
    if (err_code != NRF_SUCCESS)
    {
        return err_code;
    }
		
		err_code = ble_time_configuration_service_wake_up_from_sleep_add(p_time_configuration_service, p_time_configuration_service_init);
    if (err_code != NRF_SUCCESS)
    {
        return err_code;
    }
		 
    //on_to_pairing characteristic
    err_code = ble_time_configuration_service_on_to_pairing_add(p_time_configuration_service, p_time_configuration_service_init);
    if (err_code != NRF_SUCCESS)
    {
        return err_code;
    }
    
    
    //power_off characteristic
    err_code = ble_time_configuration_service_power_off_add(p_time_configuration_service, p_time_configuration_service_init);
    if (err_code != NRF_SUCCESS)
    {
        return err_code;
    }
 

    //bootloader characteristic
    err_code = ble_time_configuration_service_bootloader_add(p_time_configuration_service, p_time_configuration_service_init);
    if (err_code != NRF_SUCCESS)
    {
        return err_code;
    }  
    


    //cancel_bootloader characteristic
    err_code = ble_time_configuration_service_cancel_bootloader_add(p_time_configuration_service, p_time_configuration_service_init);
    if (err_code != NRF_SUCCESS)
    {
        return err_code;
    }
    
		return err_code;
	
}

/**************************************************
* Function name	: void ble_time_configuration_service_handler(ble_time_configuration_service_t * p_time_configuration, 
*                 uint16_t new_state)

*    returns		: ---
*    arg1			: pointer to Time Configuration service structure
*    arg2			: new data
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/03/17
* Description		: Time Configuration handler. 
* Notes			: Not used
**************************************************/
void ble_time_configuration_service_handler(ble_time_configuration_service_t * p_time_configuration, uint16_t new_state)
{
}
