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
* Module for managing the characteristic parity
*
***************************************************/

#include <string.h>
#include <stdint.h>
#include "ble_uart_configuration_service.h"
#include "ble_uart_configuration_service_char_parity.h"
#include "uart_management.h"
#include "connection_control_management.h"

uint8_t current_parity  = 0;
/**************************************************
* Function name	: uint32_t ble_uart_configuration_service_parity_add(ble_uart_configuration_service_t *p_ble_uart_configuration_service, 
*                           const ble_uart_configuration_service_init_t *p_ble_uart_configuration_service_init)                  
*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to UART Configuration Service structure
*    arg2			: pointer ble_uart_configuration_service_init_t for the service initialization
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/07/03
* Description		: adds Configuration Hardware Characteristic. Properties: read, write and notify
* Notes			: restrictions, odd modes
**************************************************/  
uint32_t ble_uart_configuration_service_parity_add(ble_uart_configuration_service_t *p_ble_uart_configuration_service, const ble_uart_configuration_service_init_t *p_ble_uart_configuration_service_init)
{
		ble_gatts_char_md_t char_md;

    ble_gatts_attr_t    attr_char_value;
    ble_uuid_t          ble_uuid;
    ble_gatts_attr_md_t attr_md;
  
    if(uart_management_get_parity())
    {
        current_parity  = 1;
    }
    else
    {
        current_parity  = 0;
    }

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
	
		ble_uuid.type = p_ble_uart_configuration_service->uuid_type;
    ble_uuid.uuid = BLE_UUID_PARITY_CHAR;
		
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
    attr_char_value.init_len     = LEN_PARITY_CHAR;
    attr_char_value.init_offs    = 0;
    attr_char_value.max_len      = LEN_PARITY_CHAR;
    attr_char_value.p_value      = &current_parity;

		return sd_ble_gatts_characteristic_add(p_ble_uart_configuration_service->service_handle, &char_md,
                                               &attr_char_value,
                                               &p_ble_uart_configuration_service->uart_configuration_parity);																		 
																							 
}

/**************************************************
* Function name	: uint32_t parity_update(ble_uart_configuration_service_t * p_ble_uart_configuration_service,
*                 uint8_t *data)                
*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to UART Configuration Service structure
*    arg2			: uint8_t new parity
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/07/03
* Description		: updates the characteristic parity with the new value
* Notes			: restrictions, odd modes
**************************************************/  
uint32_t parity_update(ble_uart_configuration_service_t * p_ble_uart_configuration_service, uint8_t *data)
{
			uint32_t err_code = NRF_SUCCESS;
			uint16_t len = LEN_PARITY_CHAR;
			ble_gatts_value_t gatts_value;
			
      gatts_value.len     = len;
			gatts_value.offset  = 0;      
			gatts_value.p_value = data;	


			// Update database
			err_code = sd_ble_gatts_value_set(p_ble_uart_configuration_service->conn_handle,
                                          p_ble_uart_configuration_service->uart_configuration_parity.value_handle,
                                          &gatts_value);

			
			 if (err_code != NRF_SUCCESS)     
							return err_code;
   	
				ble_gatts_hvx_params_t hvx_params;
				memset(&hvx_params, 0, sizeof(hvx_params));
				hvx_params.handle = p_ble_uart_configuration_service->uart_configuration_parity.value_handle;
				hvx_params.type   = BLE_GATT_HVX_NOTIFICATION;
				hvx_params.offset = gatts_value.offset;
				hvx_params.p_len  = &gatts_value.len;
				hvx_params.p_data = gatts_value.p_value;
				err_code = sd_ble_gatts_hvx(p_ble_uart_configuration_service->conn_handle, &hvx_params);
        
				return err_code;
}


/**************************************************
* Function name	: void uart_configuration_char_parity_handler(ble_uart_configuration_service_t * p_uart_configuration, 
*                 uint8_t * new_state)               
*    returns		: ---
*    arg1			: pointer to UART Configuration Service structure
*    arg2			: uint8_t* received data
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/07/03
* Description		: interruption when the characteristic parity is written
* Notes			: restrictions, odd modes
**************************************************/ 
void uart_configuration_char_parity_handler(ble_uart_configuration_service_t * p_uart_configuration, uint8_t * new_state)
{
  
    if(connection_control_management_get_connection_valid())
    {
     switch(new_state[0])
     {
        case 0:
          uart_management_new_parity(false);
          current_parity  = 0;
        break;
        
        case 1:
          uart_management_new_parity(true);
          current_parity  = 1;
        break;
        
        default:
          if(uart_management_get_parity())
          {
              current_parity  = 1;
          }
          else
          {
              current_parity  = 0;
          }
        break;
     }
     flash_management_flash_write_needed();
   }
   new_state[0]  = current_parity;
   parity_update(p_uart_configuration,new_state);
   
}


