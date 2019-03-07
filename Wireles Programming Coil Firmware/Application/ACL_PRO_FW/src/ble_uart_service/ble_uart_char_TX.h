#ifndef BLE_UART_CHART_TX_H__
#define BLE_UART_CHART_TX_H__

#include "ble_uart_service.h"

#define BLE_UUID_NUS_TX_CHARACTERISTIC    0x0003                      /**< The UUID of the TX Characteristic. */
#define BLE_UART_MAX_TX_CHAR_LEN          BLE_UART_MAX_DATA_LEN        /**< Maximum length of the TX Characteristic (in bytes). */


/**************************************
* Functions
**************************************/
/**@brief Function for adding TX characteristic.
 *
 * @param[in] p_ble_uart       Virtual UART Service structure.
 * @param[in] p_ble_uart_init  Information needed to initialize the service.
 *
 * @return NRF_SUCCESS on success, otherwise an error code.
 */
uint32_t tx_char_add(ble_uart_t * p_ble_uart, const ble_uart_init_t * p_ble_uart_init);

/**@brief Function for sending a string to the peer.
 *
 * @details This function sends the input string as an TX characteristic notification to the
 *          peer.
 *
 * @param[in] p_ble_uart       Pointer to the Virtual UART Service structure.
 * @param[in] p_string    String to be sent.
 * @param[in] length      Length of the string.
 *
 * @retval NRF_SUCCESS If the string was sent successfully. Otherwise, an error code is returned.
 */
uint32_t ble_uart_string_send(ble_uart_t * p_ble_uart, uint8_t * p_string, uint16_t length);
#endif //BLE_UART_CHART_TX_H__


