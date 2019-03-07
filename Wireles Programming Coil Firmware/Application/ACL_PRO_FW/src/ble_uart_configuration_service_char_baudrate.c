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
* First written on 2018/07/03 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for managing the characteristic baudrate
*
***************************************************/

#include <string.h>
#include <stdint.h>
#include "ble_uart_configuration_service.h"
#include "ble_uart_configuration_service_char_baudrate.h"
#include "uart_management.h"
#include "app_uart.h"
#include "connection_control_management.h"

uint32_t  uart_baudrate_value;
uint32_t  uart_baudrate_coded;

/**************************************************
* Function name	: uint32_t ble_uart_configuration_service_baudrate_add(ble_uart_configuration_service_t *p_ble_uart_configuration_service, 
*                           const ble_uart_configuration_service_init_t *p_ble_uart_configuration_service_init)                  
*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to UART Configuration Service structure
*    arg2			: pointer ble_uart_configuration_service_init_t for the service initialization
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/07/03
* Description		: adds Configuration Hardware Characteristic. Properties: read, write and notify
* Notes			: restrictions, odd modes
**************************************************/  
uint32_t ble_uart_configuration_service_baudrate_add(ble_uart_configuration_service_t *p_ble_uart_configuration_service, const ble_uart_configuration_service_init_t *p_ble_uart_configuration_service_init)
{
		ble_gatts_char_md_t char_md;

    ble_gatts_attr_t    attr_char_value;
    ble_uuid_t          ble_uuid;
    ble_gatts_attr_md_t attr_md;
	  uint8_t             initial_baudrate[LEN_BAUDRATE_CHAR];
    
    uart_baudrate_coded = uart_management_get_baudrate();
    uart_baudrate_value = uart_management_code2num(uart_baudrate_coded);
    
    uint8_t   num_shift = 0;
    for(uint8_t i=0;i<LEN_BAUDRATE_CHAR;i++)
    {
        num_shift = 8*(LEN_BAUDRATE_CHAR-i-1);
        initial_baudrate[i]  =  uart_baudrate_value>>num_shift;
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
    ble_uuid.uuid = BLE_UUID_BAUDRATE_CHAR;
		
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
    attr_char_value.init_len     = LEN_BAUDRATE_CHAR;
    attr_char_value.init_offs    = 0;
    attr_char_value.max_len      = LEN_BAUDRATE_CHAR;
    attr_char_value.p_value      = initial_baudrate;

		return sd_ble_gatts_characteristic_add(p_ble_uart_configuration_service->service_handle, &char_md,
                                               &attr_char_value,
                                               &p_ble_uart_configuration_service->uart_configuration_baudrate);																		 
																							 
}

/**************************************************
* Function name	: uint32_t baudrate_update(ble_uart_configuration_service_t * p_ble_uart_configuration_service, 
*                 uint8_t *data)                 
*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to UART Configuration Service structure
*    arg2			: uint8_t new baudrate
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/07/03
* Description		: updates the characteristic baudrate with the new value
* Notes			: restrictions, odd modes
**************************************************/  
uint32_t baudrate_update(ble_uart_configuration_service_t * p_ble_uart_configuration_service, uint8_t *data)
{
			uint32_t err_code = NRF_SUCCESS;
			uint16_t len = LEN_BAUDRATE_CHAR;
			ble_gatts_value_t gatts_value;
			
			gatts_value.len     = len;
			gatts_value.offset  = 0;
			gatts_value.p_value = data;	


			// Update database
			err_code = sd_ble_gatts_value_set(p_ble_uart_configuration_service->conn_handle,
                                          p_ble_uart_configuration_service->uart_configuration_baudrate.value_handle,
                                          &gatts_value);

			
			 if (err_code != NRF_SUCCESS)     
							return err_code;
   	
				ble_gatts_hvx_params_t hvx_params;
				memset(&hvx_params, 0, sizeof(hvx_params));
				hvx_params.handle = p_ble_uart_configuration_service->uart_configuration_baudrate.value_handle;
				hvx_params.type   = BLE_GATT_HVX_NOTIFICATION;
				hvx_params.offset = gatts_value.offset;
				hvx_params.p_len  = &gatts_value.len;
				hvx_params.p_data = gatts_value.p_value;
				err_code = sd_ble_gatts_hvx(p_ble_uart_configuration_service->conn_handle, &hvx_params);
        
				return err_code;
}


/**************************************************
* Function name	: void uart_configuration_char_baudrate_handler(ble_uart_configuration_service_t * p_uart_configuration, 
*                 uint8_t * new_state)                
*    returns		: ---
*    arg1			: pointer to UART Configuration Service structure
*    arg2			: uint8_t* received data
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/07/03
* Description		: interruption when the characteristic baudrate is written
* Notes			: restrictions, odd modes
**************************************************/ 
void uart_configuration_char_baudrate_handler(ble_uart_configuration_service_t * p_uart_configuration, uint8_t * new_state)
{
    
    uint8_t   num_shift = 0;
    uint32_t  received_value_num  = 0;
    uint32_t  received_value_decode = 0;
    
    if(connection_control_management_get_connection_valid())
    {
    
      //Data storage    
      for(uint8_t i=0;i<LEN_BAUDRATE_CHAR;i++)
      {
          num_shift = 8*(LEN_BAUDRATE_CHAR-i-1);
          received_value_num = received_value_num + (new_state[i]<<num_shift);
      }
      
      //We need to verify if configuration is valid
      
      //1. Change from hexadecunal to Nordic codification
      received_value_decode = uart_management_num2code(received_value_num);
      
      //2. If is 0xFFFFFFFF, data is not valid
      if(received_value_decode  !=  INVALID_DATA)
      {
          //We change configuration
        
          //Restart UART
          uart_baudrate_coded = received_value_decode;
          uart_management_new_baud(uart_baudrate_coded);
        
          //Update value
          uart_baudrate_value = received_value_num;
          flash_management_flash_write_needed();

        
      }
      
    }
    //We have to change "new_state" to uart_baudrate_value
    num_shift = 0;
    for(uint8_t i=0;i<LEN_BAUDRATE_CHAR;i++)
    {
        num_shift = 8*(LEN_BAUDRATE_CHAR-i-1);
        new_state[i]  =  uart_baudrate_value>>num_shift;
    }
    
   
    
    baudrate_update(p_uart_configuration,new_state);
}


