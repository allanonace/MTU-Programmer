/***************************************************
* Module name: ble_control_char_pass.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/07/09 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for controlling the characteristic which shows
* the password
***************************************************/

/*  Include section
* Add all #includes here
*
***************************************************/
#include <string.h>
#include "ble_control_service.h"
#include "ble_control_service_char_pass.h"
#include "..\data_management.h"
#include "encryption_management.h"

/*  Defines section
* Add all #defines here
*
***************************************************/

/*  Variables section
* Add all variables here
*
***************************************************/
uint8_t   pass_buff_ble[AES_BYTES];

/*  Function Prototype Section
* Add prototypes for all functions called by this
* module, with the exception of runtime routines.
*
***************************************************/

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
uint32_t pass_H_char_add(ble_control_service_t * p_ble_control, const ble_control_service_init_t * p_ble_control_service_init)
{
    ble_gatts_char_md_t char_md;
    ble_gatts_attr_t    attr_char_value;
    ble_uuid_t          ble_uuid;
    ble_gatts_attr_md_t attr_md;
    uint8_t             initial_value[BLE_CONTROL_MAX_PASS_CHAR_LEN];
  
    
    memset(initial_value,0,sizeof(initial_value));
    memset(&char_md, 0, sizeof(char_md));

    char_md.char_props.read          = 1;
    char_md.char_props.write         = 0;
    char_md.char_props.write_wo_resp = 0;
    char_md.p_char_user_desc         = NULL;
    char_md.p_char_pf                = NULL;
    char_md.p_user_desc_md           = NULL;
    char_md.p_cccd_md                = NULL;
    char_md.p_sccd_md                = NULL;

    BLE_UUID_BLE_ASSIGN(ble_uuid, BLE_UUID_UART_CHAR_PASS_H_CHARACTERISTIC);

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
    attr_char_value.init_len  = BLE_CONTROL_MAX_PASS_CHAR_LEN;
    attr_char_value.init_offs = 0;
    attr_char_value.max_len   = BLE_CONTROL_MAX_PASS_CHAR_LEN;
    attr_char_value.p_value   = initial_value;
    

    return sd_ble_gatts_characteristic_add(p_ble_control->service_handle,
                                           &char_md,
                                           &attr_char_value,
                                           &p_ble_control->pass_H_handles);
}


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
uint32_t pass_L_char_add(ble_control_service_t * p_ble_control, const ble_control_service_init_t * p_ble_control_service_init)
{
    ble_gatts_char_md_t char_md;
    ble_gatts_attr_t    attr_char_value;
    ble_uuid_t          ble_uuid;
    ble_gatts_attr_md_t attr_md;
    uint8_t             initial_value[BLE_CONTROL_MAX_PASS_CHAR_LEN];
  
    memset(initial_value,0,sizeof(initial_value));
    memset(&char_md, 0, sizeof(char_md));

    char_md.char_props.read          = 1;
    char_md.char_props.write         = 0;
    char_md.char_props.write_wo_resp = 0;
    char_md.p_char_user_desc         = NULL;
    char_md.p_char_pf                = NULL;
    char_md.p_user_desc_md           = NULL;
    char_md.p_cccd_md                = NULL;
    char_md.p_sccd_md                = NULL;

    BLE_UUID_BLE_ASSIGN(ble_uuid, BLE_UUID_UART_CHAR_PASS_L_CHARACTERISTIC);

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
    attr_char_value.init_len  = BLE_CONTROL_MAX_PASS_CHAR_LEN;
    attr_char_value.init_offs = 0;
    attr_char_value.max_len   = BLE_CONTROL_MAX_PASS_CHAR_LEN;
    
    attr_char_value.p_value   = initial_value;
    

    return sd_ble_gatts_characteristic_add(p_ble_control->service_handle,
                                           &char_md,
                                           &attr_char_value,
                                           &p_ble_control->pass_L_handles);
}

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
uint32_t pass_char_update(ble_control_service_t * p_ble_control, bool show_pass)
{
    uint32_t            err_code;
    uint8_t             current_value_H[BLE_CONTROL_MAX_PASS_CHAR_LEN];
    uint8_t             current_value_L[BLE_CONTROL_MAX_PASS_CHAR_LEN];
    ble_gatts_value_t   gatts_value_H,gatts_value_L;
  
    if(show_pass)
    {
        encryption_control_management_get_dynamic_pass(pass_buff_ble);
      
        for(uint8_t i=0;i<BLE_CONTROL_MAX_PASS_CHAR_LEN;i++)
        {
            current_value_H[i]  = pass_buff_ble[i];
        }
        
        for(uint8_t i=0;i<BLE_CONTROL_MAX_PASS_CHAR_LEN;i++)
        {
            current_value_L[i]  = pass_buff_ble[i+16];
        }
    }
    else
    {
        memset(current_value_H,0,sizeof(current_value_H));
        memset(current_value_L,0,sizeof(current_value_L));
    }
    gatts_value_H.len     = BLE_CONTROL_MAX_PASS_CHAR_LEN;
    gatts_value_H.offset  = 0;
    gatts_value_H.p_value = current_value_H;
    
    err_code  = sd_ble_gatts_value_set(p_ble_control->conn_handle,
                                       p_ble_control->pass_H_handles.value_handle,
                                       &gatts_value_H); 
    
    gatts_value_L.len     = BLE_CONTROL_MAX_PASS_CHAR_LEN;
    gatts_value_L.offset  = 0;
    gatts_value_L.p_value = current_value_L;
    
    err_code  = sd_ble_gatts_value_set(p_ble_control->conn_handle,
                                       p_ble_control->pass_L_handles.value_handle,
                                       &gatts_value_L); 
    
    return err_code;
                                          
}
