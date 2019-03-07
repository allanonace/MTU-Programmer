/***************************************************
* Module name: ble_uart_char_TX.c
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
* Module for controlling the characteristic which receives
* Virtual UART data
***************************************************/

/*  Include section
* Add all #includes here
*
***************************************************/

#include <string.h>
#include "ble_uart_service.h"
#include "ble_uart_char_TX.h"
#include "sdk_macros.h"

/*  Function Prototype Section
* Add prototypes for all functions called by this
* module, with the exception of runtime routines.
*
***************************************************/
/**@brief Function for adding TX characteristic.
 *
 * @param[in] p_ble_uart       Virtual UART Service structure.
 * @param[in] p_ble_uart_init  Information needed to initialize the service.
 *
 * @return NRF_SUCCESS on success, otherwise an error code.
 */
uint32_t tx_char_add(ble_uart_t * p_ble_uart, const ble_uart_init_t * p_ble_uart_init)
{
    /**@snippet [Adding proprietary characteristic to S110 SoftDevice] */
    ble_gatts_char_md_t char_md;
    ble_gatts_attr_md_t cccd_md;
    ble_gatts_attr_t    attr_char_value;
    ble_uuid_t          ble_uuid;
    ble_gatts_attr_md_t attr_md;

    memset(&cccd_md, 0, sizeof(cccd_md));

    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&cccd_md.read_perm);
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&cccd_md.write_perm);

    cccd_md.vloc = BLE_GATTS_VLOC_STACK;

    memset(&char_md, 0, sizeof(char_md));

    //char_md.char_props.notify = 1;
    char_md.char_props.indicate = 1;
    char_md.p_char_user_desc  = NULL;
    char_md.p_char_pf         = NULL;
    char_md.p_user_desc_md    = NULL;
    char_md.p_cccd_md         = &cccd_md;
    char_md.p_sccd_md         = NULL;

//    ble_uuid.type = p_ble_uart->uuid_type;
//    ble_uuid.uuid = BLE_UUID_NUS_TX_CHARACTERISTIC;
BLE_UUID_BLE_ASSIGN(ble_uuid, BLE_UUID_NUS_TX_CHARACTERISTIC);

    memset(&attr_md, 0, sizeof(attr_md));

    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&attr_md.read_perm);
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&attr_md.write_perm);

    attr_md.vloc    = BLE_GATTS_VLOC_STACK;
    attr_md.rd_auth = 0;
    attr_md.wr_auth = 0;
    attr_md.vlen    = 1;

    memset(&attr_char_value, 0, sizeof(attr_char_value));

    attr_char_value.p_uuid    = &ble_uuid;
    attr_char_value.p_attr_md = &attr_md;
    attr_char_value.init_len  = sizeof(uint8_t);
    attr_char_value.init_offs = 0;
    attr_char_value.max_len   = BLE_UART_MAX_TX_CHAR_LEN;

    return sd_ble_gatts_characteristic_add(p_ble_uart->service_handle,
                                           &char_md,
                                           &attr_char_value,
                                           &p_ble_uart->tx_handles);
    /**@snippet [Adding proprietary characteristic to S110 SoftDevice] */
}


/**************************************************
* Function name	: uint32_t ble_uart_string_send(ble_uart_t * p_ble_uart, uint8_t * p_string, uint16_t length)

*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to Virtual UART service structure
*    arg2			: pointer to a data array with the data to be sent
*    arg3     : length of the data to be sent
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/05/17
* Description		: function for sending the data through TX characteristic
* Notes			: restrictions, odd modes
**************************************************/
uint32_t ble_uart_string_send(ble_uart_t * p_ble_uart, uint8_t * p_string, uint16_t length)
{
    ble_gatts_hvx_params_t hvx_params;

    VERIFY_PARAM_NOT_NULL(p_ble_uart);

    //if ((p_ble_uart->conn_handle == BLE_CONN_HANDLE_INVALID) || (!p_ble_uart->is_notification_enabled))
    if (p_ble_uart->conn_handle == BLE_CONN_HANDLE_INVALID)
    {
        return NRF_ERROR_INVALID_STATE;
    }

    if (length > BLE_UART_MAX_DATA_LEN)
    {
        return NRF_ERROR_INVALID_PARAM;
    }

    memset(&hvx_params, 0, sizeof(hvx_params));

    hvx_params.handle = p_ble_uart->tx_handles.value_handle;
    hvx_params.p_data = p_string;
    hvx_params.p_len  = &length;
    hvx_params.type   = BLE_GATT_HVX_INDICATION;//BLE_GATT_HVX_NOTIFICATION;

    return sd_ble_gatts_hvx(p_ble_uart->conn_handle, &hvx_params);
}

