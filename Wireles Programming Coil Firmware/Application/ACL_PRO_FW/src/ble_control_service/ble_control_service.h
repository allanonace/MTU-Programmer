/**
 * Copyright (c) 2012 - 2017, Nordic Semiconductor ASA
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form, except as embedded into a Nordic
 *    Semiconductor ASA integrated circuit in a product or a software update for
 *    such product, must reproduce the above copyright notice, this list of
 *    conditions and the following disclaimer in the documentation and/or other
 *    materials provided with the distribution.
 * 
 * 3. Neither the name of Nordic Semiconductor ASA nor the names of its
 *    contributors may be used to endorse or promote products derived from this
 *    software without specific prior written permission.
 * 
 * 4. This software, with or without modification, must only be used with a
 *    Nordic Semiconductor ASA integrated circuit.
 * 
 * 5. Any software provided in binary form under this license must not be reverse
 *    engineered, decompiled, modified and/or disassembled.
 * 
 * THIS SOFTWARE IS PROVIDED BY NORDIC SEMICONDUCTOR ASA "AS IS" AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY, NONINFRINGEMENT, AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL NORDIC SEMICONDUCTOR ASA OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
 * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
 * OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 */

/**@file
 *
 * @defgroup ble_control Virtual UART Service
 * @{
 * @ingroup  ble_sdk_srv
 * @brief    Virtual UART Service implementation.
 *
 * @details The Virtual UART Service is a simple GATT-based service with RX and FRAME characteristics.
 *          Data received from the peer is passed to the application, and the data received
 *          from the application of this service is sent to the peer as Handle Value
 *          Notifications. This module demonstrates how to implement a custom GATT-based
 *          service and characteristics using the SoftDevice. The service
 *          is used by the application to send and receive ASCII text strings to and from the
 *          peer.
 *
 * @note The application must propagate SoftDevice events to the Virtual UART Service module
 *       by calling the ble_control_service_on_ble_evt() function from the ble_stack_handler callback.
 */

#ifndef BLE_CONTROL_SERVICE_H__
#define BLE_CONTROL_SERVICE_H__

#include "ble.h"
#include "ble_srv_common.h"
#include <stdint.h>
#include <stdbool.h>

#ifdef __cplusplus
extern "C" {
#endif
 
#define BLE_UUID_CONTROL_SERVICE 0x2500                      /**< The UUID of the Virtual UART Service. */
#define BLE_CONTROL_MAX_DATA_LEN 16

//BA79xxxx-13D9-409B-8ABB-48893A06DC7D
//#define BLE_CONTROL_SERVICE_BASE_UUID {0xBA,0x79,0x24,0x00,0x13,0xD9,0x40,0x9B,0x8A,0xBB,0x48,0x89,0x3A,0x06,0xDC,0x7D}
#define BLE_CONTROL_SERVICE_BASE_UUID   {0x7D,0xDC,0x06,0x3A,0x89,0x48,0xBB,0x8A,0x9B,0x40,0xD9,0x13,0x00,0x24,0x79,0xBA}


#define OPCODE_LENGTH 1
#define HANDLE_LENGTH 2


/* Forward declaration of the ble_control_service_t type. */
typedef struct ble_control_s ble_control_service_t;

/**@brief Virtual UART Service event handler type. */
typedef void (*ble_control_data_handler_t) (ble_control_service_t * p_ble_control, uint8_t * p_data, uint16_t length);

/**@brief Virtual UART Service initialization structure.
 *
 * @details This structure contains the initialization information for the service. The application
 * must fill this structure and pass it to the service using the @ref ble_control_service_init
 *          function.
 */
typedef struct
{
    ble_control_data_handler_t data_handler; /**< Event handler to be called for handling received data. */
} ble_control_service_init_t;

//typedef void (*ble_control_service_write_handler_t) (ble_ble_control_service_t * p_ble_control, uint8_t* new_state);

/**@brief Virtual UART Service structure.
 *
 * @details This structure contains status information related to the service.
 */
struct ble_control_s
{
    uint8_t                     uuid_type;               /**< UUID type for Virtual UART Service Base UUID. */
    uint16_t                    service_handle;          /**< Handle of Virtual UART Service (as provided by the SoftDevice). */
    ble_gatts_char_handles_t    pass_H_handles;              /**< Handles related to the RX characteristic (as provided by the SoftDevice). */
    ble_gatts_char_handles_t    pass_L_handles;              /**< Handles related to the RX characteristic (as provided by the SoftDevice). */

    ble_gatts_char_handles_t    frame_handles;              /**< Handles related to the FRAME characteristic (as provided by the SoftDevice). */
    uint16_t                    conn_handle;             /**< Handle of the current connection (as provided by the SoftDevice). BLE_CONN_HANDLE_INVALID if not in a connection. */
    bool                        is_notification_enabled; /**< Variable to indicate if the peer has enabled notification of the FRAME characteristic.*/
    ble_control_data_handler_t  data_handler;            /**< Event handler to be called for handling received data. */
  
    
};

/**@brief Function for initializing the Virtual UART Service.
 *
 * @param[out] p_ble_control      Virtual UART Service structure. This structure must be supplied
 *                        by the application. It is initialized by this function and will
 *                        later be used to identify this particular service instance.
 * @param[in] p_ble_control_service_init  Information needed to initialize the service.
 *
 * @retval NRF_SUCCESS If the service was successfully initialized. Otherwise, an error code is returned.
 * @retval NRF_ERROR_NULL If either of the pointers p_ble_control or p_ble_control_service_init is NULL.
 */
uint32_t ble_control_service_init(ble_control_service_t * p_ble_control, const ble_control_service_init_t * p_ble_control_service_init);

/**@brief Function for handling the Virtual UART Service's BLE events.
 *
 * @details The Virtual UART Service expects the application to call this function each time an
 * event is received from the SoftDevice. This function processes the event if it
 * is relevant and calls the Virtual UART Service event handler of the
 * application if necessary.
 *
 * @param[in] p_ble_control       Virtual UART Service structure.
 * @param[in] p_ble_evt   Event received from the SoftDevice.
 */
void ble_control_service_on_ble_evt(ble_control_service_t * p_ble_control, ble_evt_t * p_ble_evt);





#ifdef __cplusplus
}
#endif

#endif // BLE_CONTROL_SERVICE_H__

/** @} */
