#ifndef TIME_CONFIGURATION_SERVICE_CHAR_ON_TO_PAIRING_H__
#define TIME_CONFIGURATION_SERVICE_CHAR_ON_TO_PAIRING_H__

#include <stdint.h>
#include <stdbool.h>

#include "ble_time_configuration_service.h"

#define LEN_ON_TO_PAIRING_CHAR	4

#define BLE_UUID_ON_TO_PAIRING_CHAR					0x002A


/**************************************
* Functions
**************************************/
/**************************************************
* Function name	: uint32_t ble_time_configuration_service_on_to_pairing_add(ble_time_configuration_service_t *p_time_configuration_service, 
*                           const ble_time_configuration_service_init_t *p_time_configuration_service_init)                  
*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to TIME Configuration Service structure
*    arg2			: pointer ble_time_configuration_service_init_t for the service initialization
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/07/03
* Description		: adds Configuration Hardware Characteristic. Properties: read, write and notify
* Notes			: restrictions, odd modes
**************************************************/  
uint32_t ble_time_configuration_service_on_to_pairing_add(ble_time_configuration_service_t *p_time_configuration_service, const ble_time_configuration_service_init_t *p_time_configuration_service_init);

/**************************************************
* Function name	: void ble_time_configuration_char_on_to_pairing_handler(ble_time_configuration_service_t * p_time_configuration, 
                  uint8_t * new_state)                 
*    returns		: ---
*    arg1			: pointer to TIME Configuration Service structure
*    arg2			: uint8_t* received data
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/07/03
* Description		: interruption when the characteristic on_to_pairing is written
* Notes			: restrictions, odd modes
**************************************************/
void ble_time_configuration_char_on_to_pairing_handler(ble_time_configuration_service_t * p_time_configuration, uint8_t * new_state);


#endif //TIME_CONFIGURATION_SERVICE_CHAR_ON_TO_PAIRING_H__
