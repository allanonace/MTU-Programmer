/***************************************************
* Module name: ble_uart_char_RX.c
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
* Module for controlling the characteristic which sends
* Virtual UART data
***************************************************/

/*  Include section
* Add all #includes here
*
***************************************************/
#include <string.h>
#include "ble_uart_service.h"
#include "ble_uart_char_RX.h"
#include "..\data_management.h"
#include "sdk_macros.h"

/*  Defines section
* Add all #defines here
*
***************************************************/

/*  Variables section
* Add all variables here
*
***************************************************/
uint8_t   rx_buff_ble[BLE_UART_MAX_RX_CHAR_LEN];
uint8_t   len_data_ble  = 0;

/*  Function Prototype Section
* Add prototypes for all functions called by this
* module, with the exception of runtime routines.
*
***************************************************/

/**@brief Function for adding RX characteristic.
 *
 * @param[in] p_ble_uart       Virtual UART Service structure.
 * @param[in] p_ble_uart_init  Information needed to initialize the service.
 *
 * @return NRF_SUCCESS on success, otherwise an error code.
 */
uint32_t rx_char_add(ble_uart_t * p_ble_uart, const ble_uart_init_t * p_ble_uart_init)
{
    ble_gatts_char_md_t char_md;
    ble_gatts_attr_t    attr_char_value;
    ble_uuid_t          ble_uuid;
    ble_gatts_attr_md_t attr_md;

    memset(&char_md, 0, sizeof(char_md));

    char_md.char_props.write         = 1;
    char_md.char_props.notify        = 1;
    char_md.char_props.write_wo_resp = 0;
    char_md.p_char_user_desc         = NULL;
    char_md.p_char_pf                = NULL;
    char_md.p_user_desc_md           = NULL;
    char_md.p_cccd_md                = NULL;
    char_md.p_sccd_md                = NULL;

    BLE_UUID_BLE_ASSIGN(ble_uuid, BLE_UUID_UART_CHAR_RX_CHARACTERISTIC);

    memset(&attr_md, 0, sizeof(attr_md));

    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&attr_md.read_perm);
    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&attr_md.write_perm);



    attr_md.vloc    = BLE_GATTS_VLOC_STACK;
    attr_md.rd_auth = 0;
    attr_md.wr_auth = 0;
    attr_md.vlen    = 0;//1;

    memset(&attr_char_value, 0, sizeof(attr_char_value));

    attr_char_value.p_uuid    = &ble_uuid;
    attr_char_value.p_attr_md = &attr_md;
    attr_char_value.init_len  = 1;
    attr_char_value.init_offs = 0;
    attr_char_value.max_len   = BLE_UART_MAX_RX_CHAR_LEN;
    
    //Buffer initialization
    memset(rx_buff_ble,0,sizeof(rx_buff_ble));

    return sd_ble_gatts_characteristic_add(p_ble_uart->service_handle,
                                           &char_md,
                                           &attr_char_value,
                                           &p_ble_uart->rx_handles);
}

/**************************************************
* Function name	: void ble_uart_data_handler(ble_uart_t * p_ble_uart, uint8_t * p_data, uint16_t length)

*    returns		: -----
*    arg1			: pointer to Virtual UART service structure
*    arg2			: pointer to a data array with the received data
*    arg3     : length of the received data
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/05/17
* Description		: interruption when data are received at RX characteristic
* Notes			: restrictions, odd modes
**************************************************/
void ble_uart_data_handler(ble_uart_t * p_ble_uart, uint8_t * p_data, uint16_t length)
{
    //Data storage
    for(uint8_t i=0;i<length;i++)
    {
        rx_buff_ble[i] = p_data[i];
    }
    len_data_ble  = length;
    data_management_ble_received();
}

/**************************************************
* Function name	: uint32_t ble_uart_ack_nack_send(ble_uart_t * p_ble_uart, uint8_t * p_ack_nack, uint16_t length)

*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to Virtual UART service structure
*    arg2			: pointer to a data array with the data to be sent
*    arg3     : length of the data to be sent
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/08/02
* Description		: function for sending the ACK/NACK through RX characteristic
* Notes			: restrictions, odd modes
**************************************************/
uint32_t ble_uart_ack_nack_send(ble_uart_t * p_ble_uart, uint8_t * p_ack_nack, uint16_t length)
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

    hvx_params.handle = p_ble_uart->rx_handles.value_handle;
    hvx_params.p_data = p_ack_nack;
    hvx_params.p_len  = &length;
    hvx_params.type   = BLE_GATT_HVX_NOTIFICATION;

    return sd_ble_gatts_hvx(p_ble_uart->conn_handle, &hvx_params);
}

/**************************************************
* Function name	: uint8_t get_rx_ble_buff(uint8_t *p_data)

*    returns		: length of received data
*    arg			  : pointer to a data array with the received data
* Created by		: María Viqueira
* Date created	: 2018/05/23
* Description		: copies the received data
* Notes			: restrictions, odd modes
**************************************************/
uint8_t get_rx_ble_buff(uint8_t *p_data)
{
    for(uint8_t i=0;i<len_data_ble;i++)
    {
        p_data[i] = rx_buff_ble[i];
    }
    return len_data_ble;
}
