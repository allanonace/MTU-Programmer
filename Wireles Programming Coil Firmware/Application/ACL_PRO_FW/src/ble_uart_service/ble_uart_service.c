/***************************************************
* Module name: ble_uart_service.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/05/17 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for controlling the operations regarding to
* the Virtual Uart Service
*
***************************************************/

/*  Include section
* Add all #includes here
*
***************************************************/

#include "sdk_common.h"
#include "ble_uart_service.h"
#include "ble_uart_char_TX.h"
#include "ble_uart_char_RX.h"
#include "ble_srv_common.h"


/*  Function Prototype Section
* Add prototypes for all functions called by this
* module, with the exception of runtime routines.
*
***************************************************/

/**@brief Function for handling the @ref BLE_GAP_EVT_CONNECTED event from the S110 SoftDevice.
 *
 * @param[in] p_ble_uart     Virtual UART Service structure.
 * @param[in] p_ble_evt Pointer to the event received from BLE stack.
 */
static void on_connect(ble_uart_t * p_ble_uart, ble_evt_t * p_ble_evt)
{
    p_ble_uart->conn_handle = p_ble_evt->evt.gap_evt.conn_handle;
}


/**@brief Function for handling the @ref BLE_GAP_EVT_DISCONNECTED event from the S110 SoftDevice.
 *
 * @param[in] p_ble_uart     Virtual UART Service structure.
 * @param[in] p_ble_evt Pointer to the event received from BLE stack.
 */
static void on_disconnect(ble_uart_t * p_ble_uart, ble_evt_t * p_ble_evt)
{
    UNUSED_PARAMETER(p_ble_evt);
    p_ble_uart->conn_handle = BLE_CONN_HANDLE_INVALID;
}


/**@brief Function for handling the @ref BLE_GATTS_EVT_WRITE event from the S110 SoftDevice.
 *
 * @param[in] p_ble_uart     Virtual UART Service structure.
 * @param[in] p_ble_evt Pointer to the event received from BLE stack.
 */
static void on_write(ble_uart_t * p_ble_uart, ble_evt_t * p_ble_evt)
{
    ble_gatts_evt_write_t * p_evt_write = &p_ble_evt->evt.gatts_evt.params.write;

    if (
        (p_evt_write->handle == p_ble_uart->tx_handles.cccd_handle)
        &&
        (p_evt_write->len == 2)
       )
    {
        if (ble_srv_is_notification_enabled(p_evt_write->data))
        {
            p_ble_uart->is_notification_enabled = true;
        }
        else
        {
            p_ble_uart->is_notification_enabled = false;
        }
    }
    else if (
             (p_evt_write->handle == p_ble_uart->rx_handles.value_handle)
             &&
             (p_ble_uart->data_handler != NULL)
            )
    {
        p_ble_uart->data_handler(p_ble_uart, p_evt_write->data, p_evt_write->len);
    }
    else
    {
        // Do Nothing. This event is not relevant for this service.
    }
}




 /**************************************************
* Function name	: void ble_uart_on_ble_evt(ble_uart_t * p_ble_uart,
                  ble_evt_t * p_ble_evt)
*    returns		: ---
*    arg1			: pointer to Virtual UART service structure
*    arg2			: pointer to structure ble_evt_t (Bluetooth event)  
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/05/17
* Description		: BLE events for Virtual UART. 
* Notes			: restrictions, odd modes
**************************************************/
void ble_uart_on_ble_evt(ble_uart_t * p_ble_uart, ble_evt_t * p_ble_evt)
{
    if ((p_ble_uart == NULL) || (p_ble_evt == NULL))
    {
        return;
    }

    switch (p_ble_evt->header.evt_id)
    {
        case BLE_GAP_EVT_CONNECTED:
            on_connect(p_ble_uart, p_ble_evt);
            break;

        case BLE_GAP_EVT_DISCONNECTED:
            on_disconnect(p_ble_uart, p_ble_evt);
            break;

        case BLE_GATTS_EVT_WRITE:
            on_write(p_ble_uart, p_ble_evt);
            break;

        default:
            // No implementation needed.
            break;
    }
}

/**************************************************
* Function name	: uint32_t ble_uart_init(ble_uart_t * p_ble_uart, 
                          const ble_uart_init_t * p_ble_uart_init)
*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to Virtual UART service structure
*    arg2			: pointer p_servicio_uart_ble_init for the service initialization
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/05/17
* Description		: initializes Virtual UART Service and adds its corresponding UUID
* Notes			: restrictions, odd modes
**************************************************/
uint32_t ble_uart_init(ble_uart_t * p_ble_uart, const ble_uart_init_t * p_ble_uart_init)
{
    uint32_t      err_code;
    ble_uuid_t    ble_uuid;
    ble_uuid128_t nus_base_uuid = {BLE_UART_SERVICE_BASE_UUID};
    
    /**@snippet [Adding proprietary Service to S110 SoftDevice] */
    // Add a custom base UUID.
    err_code = sd_ble_uuid_vs_add(&nus_base_uuid, &p_ble_uart->uuid_type);
    VERIFY_SUCCESS(err_code);
    

    VERIFY_PARAM_NOT_NULL(p_ble_uart);
    VERIFY_PARAM_NOT_NULL(p_ble_uart_init);

    // Initialize the service structure.
    p_ble_uart->conn_handle             = BLE_CONN_HANDLE_INVALID;
    p_ble_uart->data_handler            = p_ble_uart_init->data_handler;
    p_ble_uart->is_notification_enabled = false;

    
    ble_uuid.type = p_ble_uart->uuid_type;
    ble_uuid.uuid = BLE_UUID_UART_SERVICE;
    
//    ble_uuid.type = p_ble_uart->uuid_type;
//    ble_uuid.uuid = BLE_UUID_UART_SERVICE;

  // Add service
   // BLE_UUID_BLE_ASSIGN(ble_uuid, BLE_UUID_UART_SERVICE);

    // Add the service.
    err_code = sd_ble_gatts_service_add(BLE_GATTS_SRVC_TYPE_PRIMARY,
                                        &ble_uuid,
                                        &p_ble_uart->service_handle);
    /**@snippet [Adding proprietary Service to S110 SoftDevice] */
    VERIFY_SUCCESS(err_code);

    // Add the TX Characteristic.
    err_code = tx_char_add(p_ble_uart, p_ble_uart_init);
    VERIFY_SUCCESS(err_code);

    // Add the RX Characteristic.
    err_code = rx_char_add(p_ble_uart, p_ble_uart_init);
    VERIFY_SUCCESS(err_code);

    return NRF_SUCCESS;
}








