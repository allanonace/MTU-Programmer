
#ifndef BLE_CONTROL_CHART_RX_H__
#define BLE_CONTROL_CHART_RX_H__

#include "ble_control_service.h"

#define BLE_UUID_UART_CHAR_PASS_H_CHARACTERISTIC      0x0040                      /**< The UUID of the RX Characteristic. */
#define BLE_UUID_UART_CHAR_PASS_L_CHARACTERISTIC      0x0042                      /**< The UUID of the RX Characteristic. */

#define BLE_CONTROL_MAX_PASS_CHAR_LEN             16        /**< Maximum length of the RX Characteristic (in bytes). */



/**************************************
* Functions
**************************************/
/**************************************************
* Function name	:uint32_t pass_H_char_add(ble_control_service_t * p_ble_control, 
*                 const ble_control_service_init_t * p_ble_control_service_init)             
*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to Connection Control Service structure
*    arg2			: pointer ble_control_service_init_t for the service initialization
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/07/09
* Description		: adds PassH Characteristic. Properties: read
* Notes			: restrictions, odd modes
**************************************************/  
uint32_t pass_H_char_add(ble_control_service_t * p_ble_control, const ble_control_service_init_t * p_ble_control_service_init);


/**************************************************
* Function name	:uint32_t pass_L_char_add(ble_control_service_t * p_ble_control, 
*                 const ble_control_service_init_t * p_ble_control_service_init)             
*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to Connection Control Service structure
*    arg2			: pointer ble_control_service_init_t for the service initialization
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/07/09
* Description		: adds PassL Characteristic. Properties: read
* Notes			: restrictions, odd modes
**************************************************/ 
uint32_t pass_L_char_add(ble_control_service_t * p_ble_control, const ble_control_service_init_t * p_ble_control_service_init);

/**************************************************
* Function name	:uint32_t pass_char_update(ble_control_service_t * p_ble_control,
*                 bool show_pass)               
*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to Connection COntrol Service structure
*    arg2			: bool: true if we want to show password, false if all 0x00
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/07/09
* Description		: updates the characteristic with the dynamic password
* Notes			: restrictions, odd modes
**************************************************/ 
uint32_t pass_char_update(ble_control_service_t * p_ble_control, bool show_pass);



  
#endif //BLE_CONTROL_CHART_RX_H__



