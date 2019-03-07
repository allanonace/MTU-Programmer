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
* First written on 2018/07/05 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for managing the characteristic advertising
*
***************************************************/

#include <string.h>
#include <stdint.h>
#include "ble_time_configuration_service.h"
#include "ble_time_configuration_service_char_on_timeout.h"
#include "ble_time_configuration_service_char_advertising.h"
#include "bluetooth_management.h"
#include "connection_control_management.h"

uint16_t current_advertising;

/**************************************************
* Function name	: uint32_t ble_time_configuration_service_advertising_add(ble_time_configuration_service_t *p_time_configuration_service, 
*                           const ble_time_configuration_service_init_t *p_time_configuration_service_init)                  
*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to TIME Configuration Service structure
*    arg2			: pointer ble_time_configuration_service_init_t for the service initialization
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/07/03
* Description		: adds Configuration Hardware Characteristic. Properties: read, write and notify
* Notes			: restrictions, odd modes
**************************************************/  
uint32_t ble_time_configuration_service_advertising_add(ble_time_configuration_service_t *p_time_configuration_service, const ble_time_configuration_service_init_t *p_time_configuration_service_init)
{
		ble_gatts_char_md_t char_md;

    ble_gatts_attr_t    attr_char_value;
    ble_uuid_t          ble_uuid;
    ble_gatts_attr_md_t attr_md;
	  uint8_t             initial_advertising[LEN_ADVERTISING_CHAR];
  
    current_advertising = bluetooth_management_get_adv_interval();
    initial_advertising[0]  = current_advertising>>8;
    initial_advertising[1]  = current_advertising;
    
		memset(&char_md, 0, sizeof(char_md));    
    char_md.char_props.read   = 1;
	  char_md.char_props.write   = 1;
		char_md.char_props.write_wo_resp  = 0;

    char_md.char_props.notify = 1;
    char_md.p_char_user_desc  = NULL;
    char_md.p_char_pf         = NULL;
    char_md.p_user_desc_md    = NULL;

	  char_md.p_cccd_md         = NULL;
    char_md.p_sccd_md         = NULL;
	
		ble_uuid.type = p_time_configuration_service->uuid_type;
    ble_uuid.uuid = BLE_UUID_ADVERTISING_CHAR;
		
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
    attr_char_value.init_len     = LEN_ADVERTISING_CHAR;
    attr_char_value.init_offs    = 0;
    attr_char_value.max_len      = LEN_ADVERTISING_CHAR;
    attr_char_value.p_value      = initial_advertising;

		return sd_ble_gatts_characteristic_add(p_time_configuration_service->service_handle, &char_md,
                                               &attr_char_value,
                                               &p_time_configuration_service->time_configuration_advertising);																		 
																							 
}

/**************************************************
* Function name	: uint32_t advertising_update(ble_time_configuration_service_t * p_time_configuration_service, 
*                 uint8_t *data)                 
*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to TIME Configuration Service structure
*    arg2			: uint8_t new advertising
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/07/03
* Description		: updates the characteristic advertising with the new value
* Notes			: restrictions, odd modes
**************************************************/  
uint32_t advertising_update(ble_time_configuration_service_t * p_time_configuration_service, uint8_t *data)
{
			uint32_t err_code = NRF_SUCCESS;
			uint16_t len = LEN_ADVERTISING_CHAR;
			ble_gatts_value_t gatts_value;
			
	
			gatts_value.len     = len;
			gatts_value.offset  = 0;
			gatts_value.p_value = data;	


			// Update database
			err_code = sd_ble_gatts_value_set(p_time_configuration_service->conn_handle,
                                          p_time_configuration_service->time_configuration_advertising.value_handle,
                                          &gatts_value);

			
			 if (err_code != NRF_SUCCESS)     
							return err_code;
   	
				ble_gatts_hvx_params_t hvx_params;
				memset(&hvx_params, 0, sizeof(hvx_params));
				hvx_params.handle = p_time_configuration_service->time_configuration_advertising.value_handle;
				hvx_params.type   = BLE_GATT_HVX_NOTIFICATION;
				hvx_params.offset = gatts_value.offset;
				hvx_params.p_len  = &gatts_value.len;
				hvx_params.p_data = gatts_value.p_value;
				err_code = sd_ble_gatts_hvx(p_time_configuration_service->conn_handle, &hvx_params);
        
				return err_code;
}

/**************************************************
* Function name	: void ble_time_configuration_char_advertising_handler(ble_time_configuration_service_t * p_time_configuration, 
                  uint8_t * new_state)                 
*    returns		: ---
*    arg1			: pointer to TIME Configuration Service structure
*    arg2			: uint8_t* received data
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/07/03
* Description		: interruption when the characteristic advertising is written
* Notes			: restrictions, odd modes
**************************************************/ 
void ble_time_configuration_char_advertising_handler(ble_time_configuration_service_t * p_time_configuration, uint8_t * new_state)
{
    uint16_t advertising;
  
    if(connection_control_management_get_connection_valid())
    {
            
      advertising = (new_state[0]<<8) + new_state[1];    
      //Check thresholds
      if( (advertising  <=  BLUETOOTH_MANAGEMENT_MAX_ADV_INTERVAL)  &&  (advertising >=  BLUETOOTH_MANAGEMENT_MIN_ADV_INTERVAL))
      {
          current_advertising = advertising;
          flash_management_flash_write_needed();

      }
    }
    
    new_state[0]  = current_advertising>>8;
    new_state[1]  = current_advertising;
    
    advertising_update(p_time_configuration,new_state);
    bluetooth_management_new_adv_interval(current_advertising);
	
}


