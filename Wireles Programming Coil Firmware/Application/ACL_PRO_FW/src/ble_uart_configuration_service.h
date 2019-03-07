#ifndef UART_CONFIGURATION_SERVICE_H__
#define UART_CONFIGURATION_SERVICE_H__

#include <stdint.h>
#include <stdbool.h>
#include "ble.h"
#include "ble_srv_common.h"
#include "nordic_common.h"
#include "ble_srv_common.h"
#include "app_util.h"
#include "flash_management.h"


#define BLE_UUID_UART_CONFIGURATION_SERVICE				0x2200
//6A130x2200-C8F5-4B41-AD6A-45704BAC66CB
//#define BLE_UUID_UART_CONFIGURATION_SERVICE_BASE 	{0x6A,0x13,0x22,0x00,0xC8,0xF5,0x4B,0x41,0xAD,0x6A,0x45,0x70,0x4B,0xAC,0x66,0xCB}
#define BLE_UUID_UART_CONFIGURATION_SERVICE_BASE 		{0xCB,0x66,0xAC,0x4B,0x70,0x45,0x6A,0xAD,0x41,0x4B,0xF5,0xC8,0x00,0x22,0x13,0x6A}



/**@brief Uart Configuration Service event type. */
typedef enum
{
    UART_CONFIGURATION_SERVICE_EVT_NOTIFICATION_ENABLED,                             /**< Estado Uart Configuration notification enabled event. */
    UART_CONFIGURATION_SERVICE_EVT_NOTIFICATION_DISABLED                             /**< Estado Uart Configuration notification disabled event. */
} ble_uart_configuration_service_evt_type_t;

/**@brief Uart Configuration Service event. */
typedef struct
{
    ble_uart_configuration_service_evt_type_t evt_type; /**< Type of event. */
} ble_uart_configuration_service_evt_t;


// Forward declaration of the ble_uart_configuration_service_t type. 
typedef struct ble_uart_configuration_service_s ble_uart_configuration_service_t;

/**@brief Uart Configuration event handler type. */
typedef void (*ble_uart_configuration_service_evt_handler_t) (ble_uart_configuration_service_t * p_uart_configuration, uint16_t new_state);

typedef void (*ble_uart_configuration_service_write_handler_t) (ble_uart_configuration_service_t * p_uart_configuration, uint8_t* new_state);


/**@brief This contains all options and data needed for
 *        initialization of the service.*/
typedef struct
{
    ble_uart_configuration_service_evt_handler_t         		evt_handler; //Event handler to be called for handling events in the Uart Configuration service. */
    ble_uart_configuration_service_write_handler_t						write_parity_handler;
    ble_uart_configuration_service_write_handler_t						write_baudrate_handler;

	
	
	
} ble_uart_configuration_service_init_t;



/**@brief This contains various status information for the service. */
typedef struct ble_uart_configuration_service_s
{
	
		uint16_t                      						service_handle; // 

    ble_uart_configuration_service_evt_handler_t    	evt_handler; // Event handler to be called for handling events in the Uart Configuration Service. */
		
		ble_uart_configuration_service_write_handler_t						write_parity_handler;
    ble_uart_configuration_service_write_handler_t					write_baudrate_handler;

	
	
	
    
		ble_gatts_char_handles_t      						uart_configuration_parity; // Handles related to the parity characteristic. */
		ble_gatts_char_handles_t      						uart_configuration_baudrate; // Handles related to the baudrate characteristic. */

		
		
		 
		uint16_t                      						report_ref_handle; // Handle of the Report Reference descriptor. */
    uint8_t                     							uuid_type;

    uint16_t                      						conn_handle; // Handle of the current connection (as provided by the BLE stack, is BLE_CONN_HANDLE_INVALID if not in a connection). */
    bool                          						is_notification_supported; // TRUE if notification of Uart Configuration is supported. */
	
} ble_uart_configuration_service_t;


/**************************************
* Functions
**************************************/
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
uint32_t ble_ble_uart_configuration_service_init(ble_uart_configuration_service_t * p_ble_uart_configuration_service, const ble_uart_configuration_service_init_t * p_ble_uart_configuration_service_init);

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
void ble_uart_configuration_service_on_ble_evt(ble_uart_configuration_service_t *p_ble_uart_configuration_service,ble_evt_t *p_ble_evt);

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
void ble_uart_configuration_service_handler(ble_uart_configuration_service_t * p_uart_configuration, uint16_t new_state);

#endif // UART_CONFIGURATION_SERVICE_H__
