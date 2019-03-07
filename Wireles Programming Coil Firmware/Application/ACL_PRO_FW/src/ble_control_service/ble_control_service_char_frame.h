#ifndef BLE_CONTROL_CHAR_FRAME_H__
#define BLE_CONTROL_CHAR_FRAME_H__

#include "ble_control_service.h"

#define BLE_UUID_NUS_FRAME_CHARACTERISTIC    0x0041                     /**< The UUID of the FRAME Characteristic. */
#define BLE_CONTROL_MAX_FRAME_CHAR_LEN       16        /**< Maximum length of the FRAME Characteristic (in bytes). */


/**************************************
* Functions
**************************************/
/**@brief Function for adding FRAME characteristic.
 *
 * @param[in] p_ble_control       Virtual UART Service structure.
 * @param[in] p_ble_control_service_init  Information needed to initialize the service.
 *
 * @return NRF_SUCCESS on success, otherwise an error code.
 */
uint32_t frame_char_add(ble_control_service_t * p_ble_control, const ble_control_service_init_t * p_ble_control_service_init);

/**@brief Function for sending a string to the peer.
 *
 * @details This function sends the input string as an FRAME characteristic notification to the
 *          peer.
 *
 * @param[in] p_ble_control       Pointer to the Virtual UART Service structure.
 * @param[in] p_string    String to be sent.
 * @param[in] length      Length of the string.
 *
 * @retval NRF_SUCCESS If the string was sent successfully. Otherwise, an error code is returned.
 */
uint32_t ble_control_string_send(ble_control_service_t * p_ble_control, uint8_t * p_string, uint16_t length);

void ble_control_data_handler(ble_control_service_t * p_ble_control, uint8_t * p_data, uint16_t length);

#endif //BLE_CONTROL_CHAR_FRAME_H__


