#ifndef UART_CONFIGURATION_SERVICE_CHAR_BAUDRATE_H__
#define UART_CONFIGURATION_SERVICE_CHAR_BAUDRATE_H__

#include <stdint.h>
#include <stdbool.h>

#include "ble_uart_configuration_service.h"

#define LEN_BAUDRATE_CHAR	4

#define BLE_UUID_BAUDRATE_CHAR					0x0012


/**************************************
* Functions
**************************************/
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
uint32_t ble_uart_configuration_service_baudrate_add(ble_uart_configuration_service_t *p_ble_uart_configuration_service, const ble_uart_configuration_service_init_t *p_ble_uart_configuration_service_init);

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
void uart_configuration_char_baudrate_handler(ble_uart_configuration_service_t * p_uart_configuration, uint8_t * new_state);


#endif //UART_CONFIGURATION_SERVICE_CHAR_BAUDRATE_H__
