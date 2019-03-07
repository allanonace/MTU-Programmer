/***************************************************
* Module name: ble_control_char_FRAME.c
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
#include "ble_control_service.h"
#include "ble_control_service_char_frame.h"
#include "sdk_macros.h"
#include "connection_control_management.h"

#define ANSW_CORRECT     0x11
#define ANSW_NO_CORRECT  0xCC

extern ble_control_service_t   m_ble_control_service;

uint8_t   frame_buff_ble[BLE_CONTROL_MAX_FRAME_CHAR_LEN];

/*  Function Prototype Section
* Add prototypes for all functions called by this
* module, with the exception of runtime routines.
*
***************************************************/
/**@brief Function for adding FRAME characteristic.
 *
 * @param[in] p_ble_control       Virtual UART Service structure.
 * @param[in] p_ble_control_service_init  Information needed to initialize the service.
 *
 * @return NRF_SUCCESS on success, otherwise an error code.
 */
uint32_t frame_char_add(ble_control_service_t * p_ble_control, const ble_control_service_init_t * p_ble_control_service_init)
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

    char_md.char_props.notify = 1;
	 char_md.char_props.write = 1;
    //char_md.char_props.indicate = 1;
    char_md.p_char_user_desc  = NULL;
    char_md.p_char_pf         = NULL;
    char_md.p_user_desc_md    = NULL;
    char_md.p_cccd_md         = &cccd_md;
    char_md.p_sccd_md         = NULL;

//    ble_uuid.type = p_ble_control->uuid_type;
//    ble_uuid.uuid = BLE_UUID_NUS_FRAME_CHARACTERISTIC;
BLE_UUID_BLE_ASSIGN(ble_uuid, BLE_UUID_NUS_FRAME_CHARACTERISTIC);

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
    attr_char_value.max_len   = BLE_CONTROL_MAX_FRAME_CHAR_LEN;

    return sd_ble_gatts_characteristic_add(p_ble_control->service_handle,
                                           &char_md,
                                           &attr_char_value,
                                           &p_ble_control->frame_handles);
    /**@snippet [Adding proprietary characteristic to S110 SoftDevice] */
}


/**************************************************
* Function name	: uint32_t ble_control_string_send(ble_control_service_t * p_ble_control, uint8_t * p_string, uint16_t length)

*    returns		: NRF_SUCCESS on success, otherwise an error code.
*    arg1			: pointer to Virtual UART service structure
*    arg2			: pointer to a data array with the data to be sent
*    arg3     : length of the data to be sent
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/05/17
* Description		: function for sending the data through FRAME characteristic
* Notes			: restrictions, odd modes
**************************************************/
uint32_t ble_control_string_send(ble_control_service_t * p_ble_control, uint8_t * p_string, uint16_t length)
{
    ble_gatts_hvx_params_t hvx_params;

    VERIFY_PARAM_NOT_NULL(p_ble_control);

    //if ((p_ble_control->conn_handle == BLE_CONN_HANDLE_INVALID) || (!p_ble_control->is_notification_enabled))
    if (p_ble_control->conn_handle == BLE_CONN_HANDLE_INVALID)
    {
        return NRF_ERROR_INVALID_STATE;
    }

    if (length > BLE_CONTROL_MAX_DATA_LEN)
    {
        return NRF_ERROR_INVALID_PARAM;
    }

    memset(&hvx_params, 0, sizeof(hvx_params));

    hvx_params.handle = p_ble_control->frame_handles.value_handle;
    hvx_params.p_data = p_string;
    hvx_params.p_len  = &length;
    hvx_params.type   = BLE_GATT_HVX_NOTIFICATION;

    return sd_ble_gatts_hvx(p_ble_control->conn_handle, &hvx_params);
}

/**************************************************
* Function name	: void ble_control_data_handler(ble_control_service_t * p_ble_control, uint8_t * p_data, uint16_t length)

*    returns		: -----
*    arg1			: pointer to Virtual UART service structure
*    arg2			: pointer to a data array with the received data
*    arg3     : length of the received data
* Created by		: Nordic (modified by María Viqueira)
* Date created		: 2018/05/17
* Description		: interruption when data are received at RX characteristic
* Notes			: restrictions, odd modes
**************************************************/
void ble_control_data_handler(ble_control_service_t * p_ble_control, uint8_t * p_data, uint16_t length)
{
    uint8_t answ_frame[BLE_CONTROL_MAX_FRAME_CHAR_LEN];
    uint8_t  value_frame;
  
    connection_control_management_decode_frame_hello(p_data);
    if(connection_control_management_get_connection_valid())
    {
        value_frame = ANSW_CORRECT;
    }
    else
    {
        value_frame = ANSW_NO_CORRECT;
    }
    memset(answ_frame,value_frame,sizeof(answ_frame));
    ble_control_string_send(p_ble_control,answ_frame,BLE_CONTROL_MAX_FRAME_CHAR_LEN);
    //Data storage
//    for(uint8_t i=0;i<length;i++)
//    {
//        frame_buff_ble[i] = p_data[i];
//    }
}
