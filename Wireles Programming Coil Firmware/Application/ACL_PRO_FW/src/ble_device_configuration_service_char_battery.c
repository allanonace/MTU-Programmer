/***************************************************
* Module name: ble_device_configuration_service_char_version.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/06/18 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for managing the characteristics of battery level
* and battery status
*
***************************************************/
/*  Include section
* Add all #includes here
*
***************************************************/
#include <stdio.h>
#include <string.h>
#include "sdk_macros.h"
#include "ble_device_configuration_service.h"
#include "ble_device_configuration_service_char_battery.h"
#include "battery_management.h"
/**************************************************
* Function name	: uint32_t ble_device_configuration_service_battery_level_char_add(ble_device_configuration_service_t *p_ble_device_configuration_service,
                  const ble_device_configuration_service_init_t *p_ble_device_configuration_service_init)
*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to Configuration Service structure
*    arg2			: pointer ble_device_configuration_service_init_t for the service initialization
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/05/23
* Description		: adds Battery Level Characteristic. Properties: read and notify
* Notes			: restrictions, odd modes
**************************************************/  
uint32_t ble_device_configuration_service_battery_level_char_add(ble_device_configuration_service_t *p_ble_device_configuration_service, const ble_device_configuration_service_init_t *p_ble_device_configuration_service_init)
{
		
		 /**@snippet [Adding proprietary characteristic to S110 SoftDevice] */
    ble_gatts_char_md_t char_md;
    ble_gatts_attr_md_t cccd_md;
    ble_gatts_attr_t    attr_char_value;
    ble_uuid_t          ble_uuid;
    ble_gatts_attr_md_t attr_md;
    memset(&cccd_md, 0, sizeof(cccd_md));
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&cccd_md.read_perm);
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&cccd_md.write_perm);
    cccd_md.vloc = BLE_GATTS_VLOC_STACK;
    memset(&char_md, 0, sizeof(char_md));
    char_md.char_props.notify = 1;
    char_md.char_props.read   = 1;
  
    char_md.p_char_user_desc  = NULL;
    char_md.p_char_pf         = NULL;
    char_md.p_user_desc_md    = NULL;
    char_md.p_cccd_md         = &cccd_md;
    char_md.p_sccd_md         = NULL;
//    ble_uuid.type = p_ble_uart->uuid_type;
//    ble_uuid.uuid = BLE_UUID_NUS_TX_CHARACTERISTIC;
BLE_UUID_BLE_ASSIGN(ble_uuid, BLE_UUID_CONFIGURATION_SERVICE_BATTERY_LEVEL_CHAR);
    memset(&attr_md, 0, sizeof(attr_md));
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&attr_md.read_perm);
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&attr_md.write_perm);
    attr_md.vloc    = BLE_GATTS_VLOC_STACK;
    attr_md.rd_auth = 0;
    attr_md.wr_auth = 0;
    attr_md.vlen    = 1;
    memset(&attr_char_value, 0, sizeof(attr_char_value));
    attr_char_value.p_uuid    = &ble_uuid;
    attr_char_value.p_attr_md = &attr_md;
    attr_char_value.init_len  = sizeof(uint8_t);
    attr_char_value.init_offs = 0;
    attr_char_value.max_len   = LEN_BLE_DEVICE_CONFIGURATION_BATTERY_LEVEL_CHAR;
    return sd_ble_gatts_characteristic_add(p_ble_device_configuration_service->service_handle,
                                           &char_md,
                                           &attr_char_value,
                                           &p_ble_device_configuration_service->ble_device_configuration_service_battery_level_char_handles);
    /**@snippet [Adding proprietary characteristic to S110 SoftDevice] */
}
/**************************************************
* Function name	: uint32_t ble_device_configuration_service_battery_status_char_add(ble_device_configuration_service_t *p_ble_device_configuration_service,
                  const ble_device_configuration_service_init_t *p_ble_device_configuration_service_init)
*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to Configuration Service structure
*    arg2			: pointer ble_device_configuration_service_init_t for the service initialization
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/05/23
* Description		: adds Battery Status Characteristic. Properties: read and notify
* Notes			: restrictions, odd modes
**************************************************/  
uint32_t ble_device_configuration_service_battery_status_char_add(ble_device_configuration_service_t *p_ble_device_configuration_service, const ble_device_configuration_service_init_t *p_ble_device_configuration_service_init)
{
		
		ble_gatts_char_md_t 		char_md;
    ble_gatts_attr_md_t 	  cccd_md;
    ble_gatts_attr_t   			attr_char_value;
    ble_uuid_t          		ble_uuid;
    ble_gatts_attr_md_t 		attr_md;
		uint8_t 								array_configuration[LEN_BLE_DEVICE_CONFIGURATION_BATTERY_STATUS_CHAR];
	
		memset(&cccd_md, 0, sizeof(cccd_md));
	
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&cccd_md.read_perm);
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&cccd_md.write_perm);		
    cccd_md.vloc = BLE_GATTS_VLOC_STACK;
	
		
		memset(&char_md, 0, sizeof(char_md));    
    char_md.char_props.read   = 1;
	  char_md.char_props.write   = 0;
    char_md.char_props.notify = 1;
    char_md.p_char_user_desc  = NULL;
    char_md.p_char_pf         = NULL;
    char_md.p_user_desc_md    = NULL;
    char_md.p_cccd_md         = &cccd_md;
    char_md.p_sccd_md         = NULL;
	
//		ble_uuid.type = p_ble_device_configuration_service->uuid_type;
//    ble_uuid.uuid = BLE_UUID_CONFIGURATION_SERVICE_CHAR;
    BLE_UUID_BLE_ASSIGN(ble_uuid, BLE_UUID_CONFIGURATION_SERVICE_BATTERY_STATUS_CHAR);
		
		memset(&attr_md, 0, sizeof(attr_md));
    
    
     
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&attr_md.read_perm);
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&attr_md.write_perm);
		attr_md.vloc       = BLE_GATTS_VLOC_STACK;
    attr_md.rd_auth    = 0;
    attr_md.wr_auth    = 0;
    attr_md.vlen       = 0;
		
		
		 
			
		memset(&attr_char_value, 0, sizeof(attr_char_value));
    attr_char_value.p_uuid       = &ble_uuid;
    attr_char_value.p_attr_md    = &attr_md;
    attr_char_value.init_len     = LEN_BLE_DEVICE_CONFIGURATION_BATTERY_STATUS_CHAR;
    attr_char_value.init_offs    = 0;
    attr_char_value.max_len      = LEN_BLE_DEVICE_CONFIGURATION_BATTERY_STATUS_CHAR;
    attr_char_value.p_value      = array_configuration;
		
		return sd_ble_gatts_characteristic_add(p_ble_device_configuration_service->service_handle, &char_md,
                                               &attr_char_value,
                                               &p_ble_device_configuration_service->ble_device_configuration_service_battery_status_char_handles);			
}

uint32_t ble_device_configuration_service_battery_level_update(ble_device_configuration_service_t * p_ble_device_configuration_service, uint8_t * p_string, uint16_t length)
{
     ble_gatts_hvx_params_t hvx_params;
    VERIFY_PARAM_NOT_NULL(p_ble_device_configuration_service);
//    if (  (p_ble_device_configuration_service->conn_handle == BLE_CONN_HANDLE_INVALID)  ||
//         (!p_ble_device_configuration_service->is_notification_enabled))
//    {
//        return NRF_ERROR_INVALID_STATE;
//    }
   
    memset(&hvx_params, 0, sizeof(hvx_params));
    hvx_params.handle = p_ble_device_configuration_service->ble_device_configuration_service_battery_level_char_handles.value_handle;
    hvx_params.p_data = p_string;
    hvx_params.p_len  = &length;
    hvx_params.type   = BLE_GATT_HVX_NOTIFICATION;
    return sd_ble_gatts_hvx(p_ble_device_configuration_service->conn_handle, &hvx_params);
}

uint32_t ble_device_configuration_service_battery_status_update(ble_device_configuration_service_t * p_ble_device_configuration_service, uint8_t * p_string, uint16_t length)
{
     ble_gatts_hvx_params_t hvx_params;
    VERIFY_PARAM_NOT_NULL(p_ble_device_configuration_service);
//    if (  (p_ble_device_configuration_service->conn_handle == BLE_CONN_HANDLE_INVALID)  ||
//         (!p_ble_device_configuration_service->is_notification_enabled))
//    {
//        return NRF_ERROR_INVALID_STATE;
//    }
   
    memset(&hvx_params, 0, sizeof(hvx_params));
    hvx_params.handle = p_ble_device_configuration_service->ble_device_configuration_service_battery_status_char_handles.value_handle;
    hvx_params.p_data = p_string;
    hvx_params.p_len  = &length;
    hvx_params.type   = BLE_GATT_HVX_NOTIFICATION;
    return sd_ble_gatts_hvx(p_ble_device_configuration_service->conn_handle, &hvx_params);
}

