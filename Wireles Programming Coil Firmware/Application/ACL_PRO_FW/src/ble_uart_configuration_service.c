/***************************************************
* Module name: ble_uart_configuration_service.c
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
* the Configuration of the physical UART
*
***************************************************/

/*  Include section
* Add all #includes here
*
***************************************************/

#include <string.h>
#include <stdint.h>

#include "ble_uart_configuration_service.h"
#include "ble_uart_configuration_service_char_baudrate.h"
#include "ble_uart_configuration_service_char_parity.h"



extern ble_uart_configuration_service_t						 	 m_ble_uart_configuration_service;


/**@brief Function for handling the Connect event.
 *
 * @param[in]   p_ble_uart_configuration_service        Uart configuration Service structure.
 * @param[in]   p_ble_uart_configuration_service_init   Information needed to initialize the service.
 */
static void on_connect(ble_uart_configuration_service_t * p_ble_uart_configuration_service, ble_evt_t * p_ble_evt)
{
    p_ble_uart_configuration_service->conn_handle = p_ble_evt->evt.gap_evt.conn_handle;
}

/**@brief Function for handling the Disconnect event.
 *
 * @param[in]   p_ble_uart_configuration_service        Uart configuration Service structure.
 * @param[in]   p_ble_uart_configuration_service_init   Information needed to initialize the service.
 */
static void on_disconnect(ble_uart_configuration_service_t * p_ble_uart_configuration_service, ble_evt_t * p_ble_evt)
{
    UNUSED_PARAMETER(p_ble_evt);
    p_ble_uart_configuration_service->conn_handle = BLE_CONN_HANDLE_INVALID;
}

///**@brief Function for handling the Write event.
// *
// * @param[in]  p_ble_uart_configuration_service        Uart configuration Service structure.
// * @param[in]  p_ble_uart_configuration_service_init   Information needed to initialize the service.
// */
static void on_write(ble_uart_configuration_service_t * p_ble_uart_configuration_service, ble_evt_t * p_ble_evt)
{
      ble_gatts_evt_write_t * p_evt_write = &p_ble_evt->evt.gatts_evt.params.write;
	
			/********************************************************************************
      * Check whether is the handle and length of baudrate configuration
			********************************************************************************/
		 if ((p_evt_write->handle == p_ble_uart_configuration_service->uart_configuration_baudrate.value_handle) &&
            (p_evt_write->len == LEN_BAUDRATE_CHAR))
		 {

				p_ble_uart_configuration_service->write_baudrate_handler(p_ble_uart_configuration_service, p_evt_write->data); 
		 }
     
		 /********************************************************************************
      * Check whether is the handle and length of parity configuration
			********************************************************************************/
      if ((p_evt_write->handle == p_ble_uart_configuration_service->uart_configuration_parity.value_handle) &&
            (p_evt_write->len == LEN_PARITY_CHAR))
		 {

				p_ble_uart_configuration_service->write_parity_handler(p_ble_uart_configuration_service, p_evt_write->data); 
		 }
		 
}

/**************************************************
* Function name	: void ble_uart_configuration_service_on_ble_evt(ble_uart_configuration_service_t *p_uart_configuration_service,
                  *ble_evt_t *p_ble_evt)
*    returns		: ---
*    arg1			: pointer to UART Configuration service structure
*    arg2			: pointer to structure ble_evt_t (Bluetooth event)  
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/07/03
* Description		: BLE events for UART Configuration Service. 
* Notes			: restrictions, odd modes
**************************************************/
void ble_uart_configuration_service_on_ble_evt(ble_uart_configuration_service_t *p_ble_uart_configuration_service,ble_evt_t *p_ble_evt)
{
		
		switch (p_ble_evt->header.evt_id)
    {
        case BLE_GAP_EVT_CONNECTED:
						
            on_connect(p_ble_uart_configuration_service, p_ble_evt);
            break;

        case BLE_GAP_EVT_DISCONNECTED:
						
            on_disconnect(p_ble_uart_configuration_service, p_ble_evt);

            break;

        case BLE_GATTS_EVT_WRITE:
						
            on_write(p_ble_uart_configuration_service, p_ble_evt);
            break;
				

        default:
				
            // No implementation needed.
            break;
    }
}

/**************************************************
* Function name	: uint32_t ble_uart_configuration_service_init(ble_uart_configuration_service_t *p_uart_configuration_service, 
*                 const ble_uart_configuration_service_init_t *p_uart_configuration_service_init)

*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to UART Configuration service structure
*    arg2			: pointer p_uart_configuration_service for the service initialization
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/03/17
* Description		: initializes UART Configuration Service and adds its corresponding UUID
* Notes			: restrictions, odd modes
**************************************************/
uint32_t ble_ble_uart_configuration_service_init(ble_uart_configuration_service_t *p_ble_uart_configuration_service, const ble_uart_configuration_service_init_t *p_ble_uart_configuration_service_init)
{
		uint32_t   err_code;
    ble_uuid_t ble_uuid;
	
		ble_uuid128_t base_uuid = BLE_UUID_UART_CONFIGURATION_SERVICE_BASE;
		err_code = sd_ble_uuid_vs_add(&base_uuid, &p_ble_uart_configuration_service->uuid_type);

		//Service structure intialization
		p_ble_uart_configuration_service->evt_handler	=	p_ble_uart_configuration_service_init->evt_handler;
		p_ble_uart_configuration_service->conn_handle	=	BLE_CONN_HANDLE_INVALID;

	
		//Write handles
		p_ble_uart_configuration_service->write_parity_handler	  = p_ble_uart_configuration_service_init->write_parity_handler;
		p_ble_uart_configuration_service->write_baudrate_handler	= p_ble_uart_configuration_service_init->write_baudrate_handler;
	
	
			
	
		//Adding service
		ble_uuid.type=p_ble_uart_configuration_service->uuid_type;
		ble_uuid.uuid=BLE_UUID_UART_CONFIGURATION_SERVICE;
	
	
		err_code = sd_ble_gatts_service_add(BLE_GATTS_SRVC_TYPE_PRIMARY, &ble_uuid, &p_ble_uart_configuration_service->service_handle);
    if (err_code != NRF_SUCCESS)    
        return err_code;
    
		//Adding characteristics
		
		//Baudrate
		err_code = ble_uart_configuration_service_baudrate_add(p_ble_uart_configuration_service, p_ble_uart_configuration_service_init);
    if (err_code != NRF_SUCCESS)
    {
        return err_code;
    }
    
    //Parity
		err_code = ble_uart_configuration_service_parity_add(p_ble_uart_configuration_service, p_ble_uart_configuration_service_init);
    if (err_code != NRF_SUCCESS)
    {
        return err_code;
    }
		
    
		
		
		 
		return err_code;
	
}

/**************************************************
* Function name	: void ble_uart_configuration_service_handler(ble_uart_configuration_service_t * p_uart_configuration, 
*                 uint16_t new_state)

*    returns		: ---
*    arg1			: pointer to UART Configuration service structure
*    arg2			: new data
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/03/17
* Description		: UART Configuration handler. 
* Notes			: Not used
**************************************************/
void ble_uart_configuration_service_handler(ble_uart_configuration_service_t * p_uart_configuration, uint16_t new_state)
{
}
