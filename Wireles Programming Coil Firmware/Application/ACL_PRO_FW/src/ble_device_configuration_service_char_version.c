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
* Module for managing the characteristics of fw and hw version
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
#include "versions_fw_hw.h"

/**************************************************
* Function name	: uint32_t ble_device_configuration_service_hw_char_add(ble_device_configuration_service_t *p_ble_device_configuration_service,
                  const ble_device_configuration_service_init_t *p_ble_device_configuration_service_init)
*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to Configuration Service structure
*    arg2			: pointer ble_device_configuration_service_init_t for the service initialization
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/05/23
* Description		: adds Configuration Hardware Characteristic. Properties: read
* Notes			: restrictions, odd modes
**************************************************/  
uint32_t ble_device_configuration_service_hw_char_add(ble_device_configuration_service_t *p_ble_device_configuration_service, const ble_device_configuration_service_init_t *p_ble_device_configuration_service_init)
{
		
		ble_gatts_char_md_t 		char_md;
    ble_gatts_attr_md_t 	  cccd_md;
    ble_gatts_attr_t   			attr_char_value;
    ble_uuid_t          		ble_uuid;
    ble_gatts_attr_md_t 		attr_md;
		uint8_t 								array_configuration[LEN_BLE_DEVICE_CONFIGURATION_HW_CHAR];
	
		memset(&cccd_md, 0, sizeof(cccd_md));
	
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&cccd_md.read_perm);
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&cccd_md.write_perm);		
    cccd_md.vloc = BLE_GATTS_VLOC_STACK;
	
		
		memset(&char_md, 0, sizeof(char_md));    
    char_md.char_props.read   = 1;
	  char_md.char_props.write   = 0;
    char_md.char_props.notify = 0;
    char_md.p_char_user_desc  = NULL;
    char_md.p_char_pf         = NULL;
    char_md.p_user_desc_md    = NULL;
    char_md.p_cccd_md         = &cccd_md;
    char_md.p_sccd_md         = NULL;
	
//		ble_uuid.type = p_ble_device_configuration_service->uuid_type;
//    ble_uuid.uuid = BLE_UUID_CONFIGURATION_SERVICE_CHAR;
    BLE_UUID_BLE_ASSIGN(ble_uuid, BLE_UUID_CONFIGURATION_SERVICE_HW_CHAR);
		
		memset(&attr_md, 0, sizeof(attr_md));
    
    array_configuration[0]  = HW_VERSION>>8;  
    array_configuration[1]  = HW_VERSION;  
    
    array_configuration[2]  = HW_SUBVERSION>>8;  
    array_configuration[3]  = HW_SUBVERSION;  
    
     
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&attr_md.read_perm);
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&attr_md.write_perm);
		attr_md.vloc       = BLE_GATTS_VLOC_STACK;
    attr_md.rd_auth    = 0;
    attr_md.wr_auth    = 0;
    attr_md.vlen       = 0;
		
		
		
			
		memset(&attr_char_value, 0, sizeof(attr_char_value));
    attr_char_value.p_uuid       = &ble_uuid;
    attr_char_value.p_attr_md    = &attr_md;
    attr_char_value.init_len     = LEN_BLE_DEVICE_CONFIGURATION_HW_CHAR;
    attr_char_value.init_offs    = 0;
    attr_char_value.max_len      = LEN_BLE_DEVICE_CONFIGURATION_HW_CHAR;
    attr_char_value.p_value      = array_configuration;
		
		return sd_ble_gatts_characteristic_add(p_ble_device_configuration_service->service_handle, &char_md,
                                               &attr_char_value,
                                               &p_ble_device_configuration_service->ble_device_configuration_service_hw_version_char_handles);			
}
/**************************************************
* Function name	: uint32_t ble_device_configuration_service_char_add(ble_device_configuration_service_t *p_ble_device_configuration_service,
                  const ble_device_configuration_service_init_t *p_ble_device_configuration_service_init)
*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to Configuration Service structure
*    arg2			: pointer ble_device_configuration_service_init_t for the service initialization
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/05/23
* Description		: adds Configuration Firmware Characteristic. Properties: read
* Notes			: restrictions, odd modes
**************************************************/  
uint32_t ble_device_configuration_service_fw_char_add(ble_device_configuration_service_t *p_ble_device_configuration_service, const ble_device_configuration_service_init_t *p_ble_device_configuration_service_init)
{
		
		ble_gatts_char_md_t 		char_md;
    ble_gatts_attr_md_t 	  cccd_md;
    ble_gatts_attr_t   			attr_char_value;
    ble_uuid_t          		ble_uuid;
    ble_gatts_attr_md_t 		attr_md;
		uint8_t 								array_configuration[LEN_BLE_DEVICE_CONFIGURATION_FW_CHAR];
	
		memset(&cccd_md, 0, sizeof(cccd_md));
	
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&cccd_md.read_perm);
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&cccd_md.write_perm);		
    cccd_md.vloc = BLE_GATTS_VLOC_STACK;
	
		
		memset(&char_md, 0, sizeof(char_md));    
    char_md.char_props.read   = 1;
	  char_md.char_props.write   = 0;
    char_md.char_props.notify = 0;
    char_md.p_char_user_desc  = NULL;
    char_md.p_char_pf         = NULL;
    char_md.p_user_desc_md    = NULL;
    char_md.p_cccd_md         = &cccd_md;
    char_md.p_sccd_md         = NULL;
	
//		ble_uuid.type = p_ble_device_configuration_service->uuid_type;
//    ble_uuid.uuid = BLE_UUID_CONFIGURATION_SERVICE_CHAR;
    BLE_UUID_BLE_ASSIGN(ble_uuid, BLE_UUID_CONFIGURATION_SERVICE_FW_CHAR);
		
		memset(&attr_md, 0, sizeof(attr_md));
    
    array_configuration[0]  = FW_VERSION>>8;  
    array_configuration[1]  = FW_VERSION;  
    
    array_configuration[2]  = FW_SUBVERSION>>8;  
    array_configuration[3]  = FW_SUBVERSION;  
    
    
     
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&attr_md.read_perm);
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&attr_md.write_perm);
		attr_md.vloc       = BLE_GATTS_VLOC_STACK;
    attr_md.rd_auth    = 0;
    attr_md.wr_auth    = 0;
    attr_md.vlen       = 0;
		
		
		
			
		memset(&attr_char_value, 0, sizeof(attr_char_value));
    attr_char_value.p_uuid       = &ble_uuid;
    attr_char_value.p_attr_md    = &attr_md;
    attr_char_value.init_len     = LEN_BLE_DEVICE_CONFIGURATION_FW_CHAR;
    attr_char_value.init_offs    = 0;
    attr_char_value.max_len      = LEN_BLE_DEVICE_CONFIGURATION_FW_CHAR;
    attr_char_value.p_value      = array_configuration;
		
		return sd_ble_gatts_characteristic_add(p_ble_device_configuration_service->service_handle, &char_md,
                                               &attr_char_value,
                                               &p_ble_device_configuration_service->ble_device_configuration_service_fw_version_char_handles);			
}


