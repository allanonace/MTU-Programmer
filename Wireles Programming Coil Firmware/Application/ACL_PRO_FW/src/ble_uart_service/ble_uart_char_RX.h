
#ifndef BLE_UART_CHART_RX_H__
#define BLE_UART_CHART_RX_H__

#include "ble_uart_service.h"

#define BLE_UUID_UART_CHAR_RX_CHARACTERISTIC      0x0002                      /**< The UUID of the RX Characteristic. */
#define BLE_UART_MAX_RX_CHAR_LEN                  BLE_UART_MAX_DATA_LEN        /**< Maximum length of the RX Characteristic (in bytes). */



/**************************************
* Functions
**************************************/
/**@brief Function for adding RX characteristic.
 *
 * @param[in] p_ble_uart       Virtual UART Service structure.
 * @param[in] p_ble_uart_init  Information needed to initialize the service.
 *
 * @return NRF_SUCCESS on success, otherwise an error code.
 */
uint32_t rx_char_add(ble_uart_t * p_ble_uart, const ble_uart_init_t * p_ble_uart_init);

/**@brief Function for handling the data from the Virtual UART Service.
 *
 * @details This function will process the data received from the Nordic UART BLE Service and send
 *          it to the UART module.
 *
 * @param[in] p_ble_uart    Virtual UART Service structure.
 * @param[in] p_data   Data to be send to UART module.
 * @param[in] length   Length of the data.
 */
/**@snippet [Handling the data received over BLE] */
void ble_uart_data_handler(ble_uart_t * p_ble_uart, uint8_t * p_data, uint16_t length);

uint8_t get_rx_ble_buff(uint8_t *p_data);
uint32_t ble_uart_ack_nack_send(ble_uart_t * p_ble_uart, uint8_t * p_ack_nack, uint16_t length);

  
#endif //BLE_UART_CHART_RX_H__



