/***************************************************
* Module name: ble_device_configuration_service.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/05/23 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for managing Configuration Service
*
***************************************************/
/*  Include section
* Add all #includes here
*
***************************************************/
#include <stdio.h>
#include <string.h>
#include "ble_device_configuration_service.h"
#include "ble_device_configuration_service_char_version.h"
#include "ble_device_configuration_service_char_battery.h"
extern ble_device_configuration_service_t  m_ble_device_configuration_service;
/**@brief Function for handling the @ref BLE_GAP_EVT_CONNECTED event from the S110 SoftDevice.
 *
 * @param[in] p_ble_uart     Virtual UART Service structure.
 * @param[in] p_ble_evt Pointer to the event received from BLE stack.
 */
static void on_connect(ble_device_configuration_service_t * p_ble_device_configuration, ble_evt_t * p_ble_evt)
{
    p_ble_device_configuration->conn_handle = p_ble_evt->evt.gap_evt.conn_handle;
}
/**@brief Function for handling the @ref BLE_GAP_EVT_DISCONNECTED event from the S110 SoftDevice.
 *
 * @param[in] p_ble_uart     Virtual UART Service structure.
 * @param[in] p_ble_evt Pointer to the event received from BLE stack.
 */
static void on_disconnect(ble_device_configuration_service_t * p_ble_device_configuration, ble_evt_t * p_ble_evt)
{
    UNUSED_PARAMETER(p_ble_evt);
    p_ble_device_configuration->conn_handle = BLE_CONN_HANDLE_INVALID;
}
/**@brief Function for handling the @ref BLE_GATTS_EVT_WRITE event from the S110 SoftDevice.
 *
 * @param[in] p_ble_uart     Virtual UART Service structure.
 * @param[in] p_ble_evt Pointer to the event received from BLE stack.
 */
static void on_write(ble_device_configuration_service_t * p_ble_device_configuration, ble_evt_t * p_ble_evt)
{
    ble_gatts_evt_write_t * p_evt_write = &p_ble_evt->evt.gatts_evt.params.write;
    if ((p_evt_write->handle == p_ble_device_configuration->ble_device_configuration_service_battery_level_char_handles.cccd_handle)||
      (p_evt_write->handle == p_ble_device_configuration->ble_device_configuration_service_battery_status_char_handles.cccd_handle))
    {
        
        if(p_evt_write->len == 2)
        {
          if (ble_srv_is_notification_enabled(p_evt_write->data))
          {
              p_ble_device_configuration->is_notification_enabled = true;
          }
          else
          {
              p_ble_device_configuration->is_notification_enabled = false;
          }
        }
    }
    
    else
    {
        // Do Nothing. This event is not relevant for this service.
    }
}
 /**************************************************
* Function name	: void ble_device_configuration_on_ble_evt(ble_uart_t * p_ble_uart,
                  ble_evt_t * p_ble_evt)
*    returns		: ---
*    arg1			: pointer to Virtual UART service structure
*    arg2			: pointer to structure ble_evt_t (Bluetooth event)  
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/05/17
* Description		: BLE events for Virtual UART. 
* Notes			: restrictions, odd modes
**************************************************/
void ble_device_configuration_on_ble_evt(ble_device_configuration_service_t * p_ble_device_configuration, ble_evt_t * p_ble_evt)
{
    if ((p_ble_device_configuration == NULL) || (p_ble_evt == NULL))
    {
        return;
    }
    switch (p_ble_evt->header.evt_id)
    {
        case BLE_GAP_EVT_CONNECTED:
            on_connect(p_ble_device_configuration, p_ble_evt);
            break;
        case BLE_GAP_EVT_DISCONNECTED:
            on_disconnect(p_ble_device_configuration, p_ble_evt);
            break;
        case BLE_GATTS_EVT_WRITE:
            on_write(p_ble_device_configuration, p_ble_evt);
            break;
        default:
            // No implementation needed.
            break;
    }
}
 /**************************************************
* Function name	: uint32_t ble_device_configuration_service_init(ble_device_configuration_service_t *p_ble_device_configuration_service, 
                                const ble_device_configuration_service_init_t *p_ble_device_configuration_service_init)
*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to Configuration Service structure
*    arg2			: pointer ble_device_configuration_service_init_t for the service initialization
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/05/23
* Description		: initializes Configuration Service and adds its corresponding UUID
* Notes			: restrictions, odd modes
**************************************************/
uint32_t ble_device_configuration_service_init(ble_device_configuration_service_t *p_ble_device_configuration_service, const ble_device_configuration_service_init_t *p_ble_device_configuration_service_init)
{
		uint32_t   err_code;
    ble_uuid_t ble_uuid;
	
  
		ble_uuid128_t base_uuid = BLE_UUID_BLE_DEVICE_CONFIGURATION_SERVICE_SERVICE_BASE;
		err_code = sd_ble_uuid_vs_add(&base_uuid, &p_ble_device_configuration_service->uuid_type);
		
		p_ble_device_configuration_service->evt_handler	=	NULL;
		p_ble_device_configuration_service->conn_handle	=	BLE_CONN_HANDLE_INVALID;
		
	
		//Service addition
		ble_uuid.type = p_ble_device_configuration_service->uuid_type;
		ble_uuid.uuid=BLE_UUID_BLE_DEVICE_CONFIGURATION_SERVICE_SERVICE;
	
    //BLE_UUID_BLE_ASSIGN(ble_uuid, BLE_UUID_BLE_DEVICE_CONFIGURATION_SERVICE_SERVICE);
  
		err_code = sd_ble_gatts_service_add(BLE_GATTS_SRVC_TYPE_PRIMARY, &ble_uuid, &p_ble_device_configuration_service->service_handle);
    if (err_code != NRF_SUCCESS)    
    {
        return err_code;
    }
    
			
		//Characteristic:
    
    //FW
		err_code = ble_device_configuration_service_fw_char_add(p_ble_device_configuration_service, p_ble_device_configuration_service_init);
    if (err_code != NRF_SUCCESS)
    {
        return err_code;
    }
    
    //HW
    err_code = ble_device_configuration_service_hw_char_add(p_ble_device_configuration_service, p_ble_device_configuration_service_init);
    if (err_code != NRF_SUCCESS)
    {
        return err_code;
    }
    
    //Battery level
    err_code = ble_device_configuration_service_battery_level_char_add(p_ble_device_configuration_service, p_ble_device_configuration_service_init);
    if (err_code != NRF_SUCCESS)
    {
        return err_code;
    }
    
    //Battery status
    err_code = ble_device_configuration_service_battery_status_char_add(p_ble_device_configuration_service, p_ble_device_configuration_service_init);
    if (err_code != NRF_SUCCESS)
    {
        return err_code;
    }
		 
		return err_code;
	
}
