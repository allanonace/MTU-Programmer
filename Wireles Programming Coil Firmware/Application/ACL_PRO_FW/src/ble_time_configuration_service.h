#ifndef TIME_CONFIGURATION_SERVICE_H__
#define TIME_CONFIGURATION_SERVICE_H__

#include <stdint.h>
#include <stdbool.h>
#include "ble.h"
#include "ble_srv_common.h"
#include "nordic_common.h"
#include "ble_srv_common.h"
#include "app_util.h"
#include "flash_management.h"


#define BLE_UUID_TIME_CONFIGURATION_SERVICE				0x2400
//0E92xxxx-73A2-4BEE-9B54-9D5C6043A8D0
//#define BLE_UUID_TIME_CONFIGURATION_SERVICE_BASE 	{0x24,0x00,0x73,0xA2,0x4B,0xEE,0x9B,0x54,0x9D,0x5C,0x60,0x43,0xA8,0xD0}
#define BLE_UUID_TIME_CONFIGURATION_SERVICE_BASE 		{0xD0,0xA8,0x43,0x60,0x5C,0x9D,0x54,0x9B,0xEE,0x4B,0xA2,0x73,0x00,0x24}



/**@brief Uart Configuration Service event type. */
typedef enum
{
    TIME_CONFIGURATION_SERVICE_EVT_NOTIFICATION_ENABLED,                             /**< Estado Uart Configuration notification enabled event. */
    TIME_CONFIGURATION_SERVICE_EVT_NOTIFICATION_DISABLED                             /**< Estado Uart Configuration notification disabled event. */
} time_configuration_service_evt_type_t;

/**@brief Uart Configuration Service event. */
typedef struct
{
    time_configuration_service_evt_type_t evt_type; /**< Type of event. */
} time_configuration_service_evt_t;


// Forward declaration of the ble_time_configuration_service_t type. 
typedef struct time_configuration_service_s ble_time_configuration_service_t;

/**@brief Uart Configuration event handler type. */
typedef void (*time_configuration_service_evt_handler_t) (ble_time_configuration_service_t * p_time_configuration, uint16_t new_state);

typedef void (*time_configuration_service_write_handler_t) (ble_time_configuration_service_t * p_time_configuration, uint8_t* new_state);


/**@brief This contains all options and data needed for
 *        initialization of the service.*/
typedef struct
{
    time_configuration_service_evt_handler_t         		evt_handler; //Event handler to be called for handling events in the Uart Configuration service. */
    time_configuration_service_write_handler_t					write_advertising_handler;
    time_configuration_service_write_handler_t					write_on_timeout_handler;
    time_configuration_service_write_handler_t				  write_pairing_timeout_handler;
    time_configuration_service_write_handler_t				  write_wake_up_from_sleep_handler;
    time_configuration_service_write_handler_t				  write_on_to_pairing_handler;
    time_configuration_service_write_handler_t				  write_power_off_handler;
    time_configuration_service_write_handler_t				  write_bootloader_handler;
    time_configuration_service_write_handler_t				  write_cancel_bootloader_handler;

	
	
	
} ble_time_configuration_service_init_t;



/**@brief This contains various status information for the service. */
typedef struct time_configuration_service_s
{
	
		uint16_t                      						service_handle; 

    time_configuration_service_evt_handler_t    	evt_handler; // Event handler to be called for handling events in the Uart Configuration Service. */
		
		time_configuration_service_write_handler_t					write_advertising_handler;
    time_configuration_service_write_handler_t					write_on_timeout_handler;
    time_configuration_service_write_handler_t				  write_pairing_timeout_handler;
    time_configuration_service_write_handler_t				  write_wake_up_from_sleep_handler;
    time_configuration_service_write_handler_t				  write_on_to_pairing_handler;
    time_configuration_service_write_handler_t				  write_power_off_handler;
    time_configuration_service_write_handler_t				  write_bootloader_handler;
    time_configuration_service_write_handler_t				  write_cancel_bootloader_handler;
    
		ble_gatts_char_handles_t      						time_configuration_advertising; 
		ble_gatts_char_handles_t      						time_configuration_on_timeout; 
    ble_gatts_char_handles_t      						time_configuration_pairing_timeout; 
		ble_gatts_char_handles_t      						time_configuration_wake_up_from_sleep; 
		ble_gatts_char_handles_t      						time_configuration_on_to_pairing; 

		ble_gatts_char_handles_t      						time_configuration_power_off; 
    ble_gatts_char_handles_t      						time_configuration_bootloader; 
    ble_gatts_char_handles_t      						time_configuration_cancel_bootloader; 

		
  
		
		 
		uint16_t                      						report_ref_handle; // Handle of the Report Reference descriptor. */
    uint8_t                     							uuid_type;

    uint16_t                      						conn_handle; // Handle of the current connection (as provided by the BLE stack, is BLE_CONN_HANDLE_INVALID if not in a connection). */
    bool                          						is_notification_supported; // TRUE if notification of Uart Configuration is supported. */
	
} ble_time_configuration_service_t;

/**************************************
* Functions
**************************************/

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
uint32_t ble_time_configuration_service_init(ble_time_configuration_service_t * p_time_configuration_service, const ble_time_configuration_service_init_t * p_time_configuration_service_init);

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
void ble_time_configuration_service_on_ble_evt(ble_time_configuration_service_t *p_time_configuration_service,ble_evt_t *p_ble_evt);

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
void ble_time_configuration_service_handler(ble_time_configuration_service_t * p_time_configuration, uint16_t new_state);

#endif // TIME_CONFIGURATION_SERVICE_H__
