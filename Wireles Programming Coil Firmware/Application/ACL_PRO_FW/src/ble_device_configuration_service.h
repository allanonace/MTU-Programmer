#ifndef BLE_DEVICE_CONFIGURATION_SERVICE_H__
#define BLE_DEVICE_CONFIGURATION_SERVICE_H__
#include <stdint.h>
#include <stdbool.h>
#include "ble.h" 
#include "ble_srv_common.h"
#include "nordic_common.h"
#include "ble_srv_common.h"
#include "app_util.h"
#define LEN_BLE_DEVICE_CONFIGURATION_SERVICE	1
#define BLE_UUID_BLE_DEVICE_CONFIGURATION_SERVICE_SERVICE         0x2100
#define BLE_UUID_BLE_DEVICE_CONFIGURATION_SERVICE_SERVICE_BASE 		{0x0D,0x21,0x01,0x99,0xFF,0x7C,0xBB,0xBD,0xAB,0x41,0x5A,0xDC,0x00,0x21,0x63,0x1D}
/**@brief Configuration Service event type. */
typedef enum
{
    BLE_DEVICE_CONFIGURATION_SERVICE_EVT_NOTIFICATION_ENABLED,                            
    BLE_DEVICE_CONFIGURATION_SERVICE_EVT_NOTIFICATION_DISABLED                            
} ble_device_configuration_service_evt_type_t;
/**@brief Service event. */
typedef struct
{
    ble_device_configuration_service_evt_type_t evt_type; /**< Type of event. */
} ble_device_configuration_service_evt_t;
// Forward declaration of the ble_device_configuration_service_t type. 
typedef struct ble_device_configuration_service_s ble_device_configuration_service_t;
typedef void (*ble_device_configuration_service_evt_handler_t) (ble_device_configuration_service_t * p_ble_device_configuration_service, ble_device_configuration_service_evt_t * p_evt);
typedef void (*ble_device_configuration_service_write_handler_t) (ble_device_configuration_service_t * p_ble_device_configuration_service, uint8_t *new_state);
/**@brief Structure of the inizialization. This contains all options and data needed for
 *        initialization of the service.*/
typedef struct
{
    ble_device_configuration_service_evt_handler_t         			evt_handler; //Event handler to be called for handling events in the Configuration service. */
	
} ble_device_configuration_service_init_t;
/**@briefThis contains various status information for the service. */
typedef struct ble_device_configuration_service_s
{
	
		uint16_t                      							          service_handle; // Handle of Configuration Service (as provided by the BLE stack). */
    ble_device_configuration_service_evt_handler_t    		evt_handler; // Event handler to be called for handling events in the Configuration Service. */			
		ble_gatts_char_handles_t      						            ble_device_configuration_service_battery_level_char_handles;
    ble_gatts_char_handles_t      						            ble_device_configuration_service_battery_status_char_handles;
    ble_gatts_char_handles_t      						            ble_device_configuration_service_hw_version_char_handles;
    ble_gatts_char_handles_t      						            ble_device_configuration_service_fw_version_char_handles;

		uint16_t                      						report_ref_handle; // Handle of the Report Reference descriptor. */
    uint8_t                     							uuid_type;
    uint8_t                       						last_ble_device_configuration_service_state; //Last state
    uint16_t                      						conn_handle; // Handle of the current connection (as provided by the BLE stack, is BLE_CONN_HANDLE_INVALID if not in a connection). */
    
		bool                          						is_notification_supported; // TRUE if notification is supported. */
    bool                     is_notification_enabled; /**< Variable to indicate if the peer has enabled notification of the TX characteristic.*/
	
} ble_device_configuration_service_t;
/**************************************
* Functions
**************************************/
uint32_t ble_device_configuration_service_init(ble_device_configuration_service_t *p_ble_device_configuration_service, const ble_device_configuration_service_init_t *p_ble_device_configuration_service_init);
void ble_device_configuration_on_ble_evt(ble_device_configuration_service_t * p_ble_device_configuration, ble_evt_t * p_ble_evt);
#endif // BLE_DEVICE_CONFIGURATION_SERVICE_H__











