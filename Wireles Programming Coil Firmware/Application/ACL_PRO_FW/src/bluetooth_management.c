/***************************************************
* Module name: bluetooth_management.c
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
* Module for managing Bluetooth Low Energy communications
*
***************************************************/
/*  Include section
* Add all #includes here
*
***************************************************/
#include <stdbool.h>
#include <stdint.h>
#include <string.h>
#include "nordic_common.h"
#include "nrf.h"
#include "app_error.h"
#include "ble.h"
#include "ble_hci.h"
#include "ble_srv_common.h"
#include "ble_advdata.h"
#include "ble_advertising.h"
#include "ble_conn_params.h"
#include "boards.h"
//#include "softdevice_handler.h"
#include "app_timer.h"

#include "nrf_pwr_mgmt.h"
#include "bsp.h"
#include "bsp_btn_ble.h"
#include "nrf_gpio.h"
#include "ble_hci.h"
#include "ble_advdata.h"
#include "ble_advertising.h"
#include "ble_conn_state.h"
#include "nrf_sdh.h"
#include "nrf_sdh_soc.h"
#include "nrf_sdh_ble.h"
#include "nrf_ble_gatt.h"
#include "ble_dfu.h"
#include "ble_gap.h"
#include "nrf_delay.h"

//#include "ble_dfu.h"
//#define NRF_LOG_MODULE_NAME "APP"
//#include "nrf_log.h"
//#include "nrf_log_ctrl.h"
/*********************************/
#include "bluetooth_management.h"
#include "ble_uart_service.h"
#include "ble_uart_char_RX.h"
#include "ble_device_configuration_service.h"
#include "timers_management.h"
#include "defines_tests.h"
#include "leds_management.h"
#include "data_management.h"
#include "battery_management.h"
#include "ble_uart_configuration_service.h"
#include "ble_uart_configuration_service_char_baudrate.h"
#include "ble_uart_configuration_service_char_parity.h"

#include "ble_time_configuration_service.h"
#include "ble_time_configuration_service_char_on_timeout.h"
#include "ble_time_configuration_service_char_advertising.h"
#include "ble_time_configuration_service_char_pairing_timeout.h"
#include "ble_time_configuration_service_char_wake_up_from_sleep.h"
#include "ble_time_configuration_service_char_on_to_pairing.h"
#include "ble_time_configuration_service_char_power_off.h"
#include "ble_time_configuration_service_char_bootloader.h"
#include "ble_time_configuration_service_char_cancel_bootloader.h"

#include "ble_control_service.h"
#include "ble_control_service_char_frame.h"
#include "ble_control_service_char_pass.h"  

 #include "connection_control_management.h"
 #include "adc_batt_management.h"

/*  Defines section
* Add all #defines here
*
***************************************************/
#define DEVICE_NAME                     "Aclara"                            //!< Name of device. Will be included in the advertising data.



/*  Variables section
* Add all variables here
*
***************************************************/
/**************************************************
* Local variables
**************************************************/
static  uint16_t                             m_conn_handle = BLE_CONN_HANDLE_INVALID;    /**< Handle of the current connection. */
static  bool                                 status_connected = false; //Controls the connection and disconnection
static  bool                                 paired_successful  = false;   
static uint16_t                              adv_interval_ms  =  APP_ADV_INTERVAL;
static bool                                  FLAG_SOFT  = false;

/**************************************************
* Global variables
**************************************************/
extern ble_uart_t                             m_ble_uart;  /**< Structure to identify the Virtual UART Service. */
extern ble_device_configuration_service_t     m_ble_device_configuration_service;
extern ble_uart_configuration_service_t				m_ble_uart_configuration_service;
extern ble_time_configuration_service_t				m_ble_time_configuration_service;
extern ble_control_service_t                  m_ble_control_service;

/**************************************************
* DEVICE_ID
**************************************************/
#define MY_ARRAY_ADDRESS                0x24FB0   //Address where device ID is stored at flash
#define MY_ARRAY_SIZE                   4         //Size ID
const uint8_t remoteID[MY_ARRAY_SIZE] __attribute__((at(MY_ARRAY_ADDRESS))) __attribute__((used)) = {0x5a,0x5a,0,0x0d};
//const uint8_t remoteID[MY_ARRAY_SIZE] __attribute__((at(MY_ARRAY_ADDRESS))) __attribute__((used)) = {0x4d,0x41,0x26,0xAB};
//const uint8_t remoteID[MY_ARRAY_SIZE] __attribute__((at(MY_ARRAY_ADDRESS))) __attribute__((used)) = {0x52,0x55,0x26,0xAc};
//const uint8_t remoteID[MY_ARRAY_SIZE] __attribute__((at(MY_ARRAY_ADDRESS))) __attribute__((used)) = {0x50,0x50,0,1};
static  uint32_t  device_id;

                                               /**< Structure used to identify the battery service. */
NRF_BLE_GATT_DEF(m_gatt);                                           /**< GATT module instance. */
BLE_ADVERTISING_DEF(m_advertising);                                 /**< Advertising module instance. */

//static ble_uuid_t m_adv_uuids[] = {{BLE_UUID_HUMAN_INTERFACE_DEVICE_SERVICE, BLE_UUID_TYPE_BLE}};
static ble_uuid_t m_adv_uuids[] = {{BLE_UUID_DEVICE_INFORMATION_SERVICE, BLE_UUID_TYPE_BLE}}; /**< Universally unique service identifiers. */

static void advertising_init(void);

/**@brief Callback function for asserts in the SoftDevice.
 *
 * @details This function will be called in case of an assert in the SoftDevice.
 *
 * @warning This handler is an example only and does not fit a final product. You need to analyze
 *          how your product is supposed to react in case of Assert.
 * @warning On assert from the SoftDevice, the system can only recover on reset.
 *
 * @param[in]   line_num   Line number of the failing ASSERT call.
 * @param[in]   file_name  File name of the failing ASSERT call.
 */
void assert_nrf_callback(uint16_t line_num, const uint8_t * p_file_name)
{
    app_error_handler(DEAD_BEEF, line_num, p_file_name);
}
/**@brief Fetch the list of peer manager peer IDs.
 *
 * @param[inout] p_peers   The buffer where to store the list of peer IDs.
 * @param[inout] p_size    In: The size of the @p p_peers buffer.
 *                         Out: The number of peers copied in the buffer.
 */

static void advertising_start(bool erase_bonds)
{
    uint32_t err_code;
    err_code = ble_advertising_start(&m_advertising, BLE_ADV_MODE_FAST);
    APP_ERROR_CHECK(err_code);
}



/**@brief Function for the GAP initialization.
 *
 * @details This function sets up all the necessary GAP (Generic Access Profile) parameters of the
 *          device including the device name, appearance, and the preferred connection parameters.
 */
static void gap_params_init(void)
{
    uint32_t                err_code;
    ble_gap_conn_params_t   gap_conn_params;
    ble_gap_conn_sec_mode_t sec_mode;

    BLE_GAP_CONN_SEC_MODE_SET_OPEN(&sec_mode);

    err_code = sd_ble_gap_device_name_set(&sec_mode,
                                          (const uint8_t *) DEVICE_NAME,
                                          strlen(DEVICE_NAME));
    APP_ERROR_CHECK(err_code);

    memset(&gap_conn_params, 0, sizeof(gap_conn_params));

    gap_conn_params.min_conn_interval = MIN_CONN_INTERVAL;
    gap_conn_params.max_conn_interval = MAX_CONN_INTERVAL;
    gap_conn_params.slave_latency     = SLAVE_LATENCY;
    gap_conn_params.conn_sup_timeout  = CONN_SUP_TIMEOUT;

    err_code = sd_ble_gap_ppcp_set(&gap_conn_params);
    APP_ERROR_CHECK(err_code);
}
/**@brief Function for handling events from the GATT library. */
void gatt_evt_handler(nrf_ble_gatt_t * p_gatt, nrf_ble_gatt_evt_t const * p_evt)
{
    if ((m_conn_handle == p_evt->conn_handle) && (p_evt->evt_id == NRF_BLE_GATT_EVT_ATT_MTU_UPDATED))
    {
        //NRF_LOG_INFO("Data len is set to 0x%X(%d)", m_ble_nus_max_data_len, m_ble_nus_max_data_len);
    }
//    NRF_LOG_DEBUG("ATT MTU exchange completed. central 0x%x peripheral 0x%x",
//                  p_gatt->att_mtu_desired_central,
//                  p_gatt->att_mtu_desired_periph);
}

/**@brief Function for initializing the GATT module.
 */
static void gatt_init(void)
{
    ret_code_t err_code;

    err_code = nrf_ble_gatt_init(&m_gatt, gatt_evt_handler);
    APP_ERROR_CHECK(err_code);

//    err_code = nrf_ble_gatt_att_mtu_periph_set(&m_gatt, 64);
//    APP_ERROR_CHECK(err_code);
}
/**@brief Function for initializing services that will be used by the application.
 */
static void services_init(void)
{
    uint32_t err_code;
    
    
    ble_uart_init_t 								          uart_ble_init;
    ble_device_configuration_service_init_t   device_configuration_init;
    ble_uart_configuration_service_init_t			uart_configuration_service_init;
    ble_time_configuration_service_init_t			time_configuration_service_init;
    ble_control_service_init_t                control_service_init;    

    //Virtual UART
    memset(&uart_ble_init, 0, sizeof(uart_ble_init));
    uart_ble_init.data_handler = ble_uart_data_handler;
    err_code = ble_uart_init(&m_ble_uart, &uart_ble_init);
    APP_ERROR_CHECK(err_code);
    
    //Device information
    memset(&device_configuration_init, 0, sizeof(device_configuration_init));
    err_code = ble_device_configuration_service_init(&m_ble_device_configuration_service, &device_configuration_init);
    APP_ERROR_CHECK(err_code);
  
    //UART Configuration  
    memset(&uart_configuration_service_init, 0, sizeof(uart_configuration_service_init));
    uart_configuration_service_init.write_baudrate_handler  = uart_configuration_char_baudrate_handler;
    uart_configuration_service_init.write_parity_handler    = uart_configuration_char_parity_handler;
    err_code = ble_ble_uart_configuration_service_init(&m_ble_uart_configuration_service, &uart_configuration_service_init);
    APP_ERROR_CHECK(err_code);
    
    //Time Configuration
    memset(&time_configuration_service_init, 0, sizeof(time_configuration_service_init));
    
    time_configuration_service_init.write_on_timeout_handler          = ble_time_configuration_char_on_timeout_handler;
    time_configuration_service_init.write_advertising_handler         = ble_time_configuration_char_advertising_handler;
    time_configuration_service_init.write_pairing_timeout_handler     = ble_time_configuration_char_pairing_timeout_handler;
    time_configuration_service_init.write_wake_up_from_sleep_handler  = ble_time_configuration_char_wake_up_from_sleep_handler;
    time_configuration_service_init.write_on_to_pairing_handler       = ble_time_configuration_char_on_to_pairing_handler;
    time_configuration_service_init.write_power_off_handler           = ble_time_configuration_char_power_off_handler; 
    time_configuration_service_init.write_bootloader_handler          = ble_time_configuration_char_bootloader_handler;
    time_configuration_service_init.write_cancel_bootloader_handler   = ble_time_configuration_char_cancel_bootloader_handler;

    
    err_code = ble_time_configuration_service_init(&m_ble_time_configuration_service, &time_configuration_service_init);
    APP_ERROR_CHECK(err_code);
    
    //Control
    memset(&control_service_init, 0, sizeof(control_service_init));
    control_service_init.data_handler = ble_control_data_handler;
    err_code  = ble_control_service_init(&m_ble_control_service,&control_service_init);
    APP_ERROR_CHECK(err_code);
    
}
/**@brief Function for handling a Connection Parameters error.
 *
 * @param[in]   nrf_error   Error code containing information about what went wrong.
 */
static void conn_params_error_handler(uint32_t nrf_error)
{
    APP_ERROR_HANDLER(nrf_error);
}
/**@brief Function for handling an event from the Connection Parameters Module.
 *
 * @details This function will be called for all events in the Connection Parameters Module
 *          which are passed to the application.
 *
 * @note All this function does is to disconnect. This could have been done by simply setting
 *       the disconnect_on_fail config parameter, but instead we use the event handler
 *       mechanism to demonstrate its use.
 *
 * @param[in] p_evt  Event received from the Connection Parameters Module.
 */
static void on_conn_params_evt(ble_conn_params_evt_t * p_evt)
{
    uint32_t err_code;

    if (p_evt->evt_type == BLE_CONN_PARAMS_EVT_FAILED)
    {
        err_code = sd_ble_gap_disconnect(m_conn_handle, BLE_HCI_CONN_INTERVAL_UNACCEPTABLE);
        APP_ERROR_CHECK(err_code);
    }
}

/**@brief Function for initializing the Connection Parameters module.
 */
static void conn_params_init(void)
{
    uint32_t               err_code;
    ble_conn_params_init_t cp_init;

    memset(&cp_init, 0, sizeof(cp_init));

    cp_init.p_conn_params                  = NULL;
    cp_init.first_conn_params_update_delay = FIRST_CONN_PARAMS_UPDATE_DELAY;
    cp_init.next_conn_params_update_delay  = NEXT_CONN_PARAMS_UPDATE_DELAY;
    cp_init.max_conn_params_update_count   = MAX_CONN_PARAMS_UPDATE_COUNT;
    cp_init.start_on_notify_cccd_handle    = BLE_GATT_HANDLE_INVALID;
    cp_init.disconnect_on_fail             = false;
    cp_init.evt_handler                    = on_conn_params_evt;
    cp_init.error_handler                  = conn_params_error_handler;

    err_code = ble_conn_params_init(&cp_init);
    APP_ERROR_CHECK(err_code);
}

/**@brief Function for handling advertising events.
 *
 * @details This function will be called for advertising events which are passed to the application.
 *
 * @param[in] ble_adv_evt  Advertising event.
 */
static void on_adv_evt(ble_adv_evt_t ble_adv_evt)
{
    
    switch (ble_adv_evt)
    {
        case BLE_ADV_EVT_FAST:
            
            break;
        case BLE_ADV_EVT_IDLE:
            break;
        default:
            break;
    }
}
/**@brief Function for handling BLE events.
 *
 * @param[in]   p_ble_evt   Bluetooth stack event.
 * @param[in]   p_context   Unused.
 */
//static void ble_evt_handler(ble_evt_t const * p_ble_evt, void * p_context)
static void ble_evt_handler(ble_evt_t  * p_ble_evt, void * p_context)
{
    ret_code_t err_code;
  
    ble_uart_on_ble_evt(&m_ble_uart, p_ble_evt);
    ble_device_configuration_on_ble_evt(&m_ble_device_configuration_service,p_ble_evt);
    ble_uart_configuration_service_on_ble_evt(&m_ble_uart_configuration_service,p_ble_evt);
    ble_time_configuration_service_on_ble_evt(&m_ble_time_configuration_service,p_ble_evt);
    ble_control_service_on_ble_evt(&m_ble_control_service,p_ble_evt);
  
    switch (p_ble_evt->header.evt_id)
    {
        case BLE_GAP_EVT_CONNECTED:
            //NRF_LOG_INFO("Connected");
//            err_code = bsp_indication_set(BSP_INDICATE_CONNECTED);
//            APP_ERROR_CHECK(err_code);
            m_conn_handle = p_ble_evt->evt.gap_evt.conn_handle;
                   
            status_connected  = true;
            break;

        case BLE_GAP_EVT_DISCONNECTED:
            //NRF_LOG_INFO("Disconnected");
            // LED indication will be changed when advertising starts.
            status_connected  = false;
        //Reset connection control
          connection_control_management_reset_connection();
            m_conn_handle = BLE_CONN_HANDLE_INVALID;
            sd_ble_gap_adv_stop();
            advertising_init();
            ble_advertising_start(&m_advertising, BLE_ADV_MODE_FAST);
            break;

#ifndef S140
        case BLE_GAP_EVT_PHY_UPDATE_REQUEST:
        {
           // NRF_LOG_DEBUG("PHY update request.");
            ble_gap_phys_t const phys =
            {
                .rx_phys = BLE_GAP_PHY_AUTO,
                .tx_phys = BLE_GAP_PHY_AUTO,
            };
            err_code = sd_ble_gap_phy_update(p_ble_evt->evt.gap_evt.conn_handle, &phys);
            APP_ERROR_CHECK(err_code);
        } break;
#endif

        case BLE_GAP_EVT_SEC_PARAMS_REQUEST:
            // Pairing not supported
            err_code = sd_ble_gap_sec_params_reply(m_conn_handle, BLE_GAP_SEC_STATUS_PAIRING_NOT_SUPP, NULL, NULL);
            APP_ERROR_CHECK(err_code);
            break;
#if !defined (S112)
         case BLE_GAP_EVT_DATA_LENGTH_UPDATE_REQUEST:
        {
            ble_gap_data_length_params_t dl_params;

            // Clearing the struct will effectivly set members to @ref BLE_GAP_DATA_LENGTH_AUTO
            memset(&dl_params, 0, sizeof(ble_gap_data_length_params_t));
            err_code = sd_ble_gap_data_length_update(p_ble_evt->evt.gap_evt.conn_handle, &dl_params, NULL);
            APP_ERROR_CHECK(err_code);
        } break;
#endif //!defined (S112)
        case BLE_GATTS_EVT_SYS_ATTR_MISSING:
            // No system attributes have been stored.
            err_code = sd_ble_gatts_sys_attr_set(m_conn_handle, NULL, 0, 0);
            APP_ERROR_CHECK(err_code);
            break;

        case BLE_GATTC_EVT_TIMEOUT:
            // Disconnect on GATT Client timeout event.
            err_code = sd_ble_gap_disconnect(p_ble_evt->evt.gattc_evt.conn_handle,
                                             BLE_HCI_REMOTE_USER_TERMINATED_CONNECTION);
            APP_ERROR_CHECK(err_code);
            break;

        case BLE_GATTS_EVT_TIMEOUT:
            // Disconnect on GATT Server timeout event.
            err_code = sd_ble_gap_disconnect(p_ble_evt->evt.gatts_evt.conn_handle,
                                             BLE_HCI_REMOTE_USER_TERMINATED_CONNECTION);
            APP_ERROR_CHECK(err_code);
            break;

        case BLE_EVT_USER_MEM_REQUEST:
            err_code = sd_ble_user_mem_reply(p_ble_evt->evt.gattc_evt.conn_handle, NULL);
            APP_ERROR_CHECK(err_code);
            break;

        case BLE_GATTS_EVT_RW_AUTHORIZE_REQUEST:
        {
            ble_gatts_evt_rw_authorize_request_t  req;
            ble_gatts_rw_authorize_reply_params_t auth_reply;

            req = p_ble_evt->evt.gatts_evt.params.authorize_request;

            if (req.type != BLE_GATTS_AUTHORIZE_TYPE_INVALID)
            {
                if ((req.request.write.op == BLE_GATTS_OP_PREP_WRITE_REQ)     ||
                    (req.request.write.op == BLE_GATTS_OP_EXEC_WRITE_REQ_NOW) ||
                    (req.request.write.op == BLE_GATTS_OP_EXEC_WRITE_REQ_CANCEL))
                {
                    if (req.type == BLE_GATTS_AUTHORIZE_TYPE_WRITE)
                    {
                        auth_reply.type = BLE_GATTS_AUTHORIZE_TYPE_WRITE;
                    }
                    else
                    {
                        auth_reply.type = BLE_GATTS_AUTHORIZE_TYPE_READ;
                    }
                    auth_reply.params.write.gatt_status = APP_FEATURE_NOT_SUPPORTED;
                    err_code = sd_ble_gatts_rw_authorize_reply(p_ble_evt->evt.gatts_evt.conn_handle,
                                                               &auth_reply);
                    APP_ERROR_CHECK(err_code);
                }
            }
        } break; // BLE_GATTS_EVT_RW_AUTHORIZE_REQUEST

        default:
            // No implementation needed.
            break;
    }
}
/**@brief Function for initializing the BLE stack.
 *
 * @details Initializes the SoftDevice and the BLE event interrupt.
 */
void bluetooth_management_ble_stack_init(void)
{
    if(!FLAG_SOFT)
    {
      ret_code_t err_code;
      err_code = nrf_sdh_enable_request();
      APP_ERROR_CHECK(err_code);
      // Configure the BLE stack using the default settings.
      // Fetch the start address of the application RAM.
      uint32_t ram_start = 0;
      err_code = nrf_sdh_ble_default_cfg_set(APP_BLE_CONN_CFG_TAG, &ram_start);
      APP_ERROR_CHECK(err_code);
      // Enable BLE stack.
      err_code = nrf_sdh_ble_enable(&ram_start);
      APP_ERROR_CHECK(err_code);
      // Register a handler for BLE events.
      NRF_SDH_BLE_OBSERVER(m_ble_observer, APP_BLE_OBSERVER_PRIO, (nrf_sdh_ble_evt_handler_t)ble_evt_handler, NULL);
      FLAG_SOFT = true;
    }
}


/**@brief Function for initializing the Advertising functionality.
 */

static void advertising_init(void)
{
     uint32_t               err_code;
    ble_advertising_init_t init;
   ble_advdata_manuf_data_t 	adv_manuf_data;

    uint8_t                   size_id;  
    uint8_t                   pos_adv_data  = 0;  
    uint8_array_t            	adv_manuf_data_array;
		uint8_t                  	adv_manuf_data_data[8];
  
    size_id = sizeof(device_id);
  
  
    //Device ID
    for(pos_adv_data = 0;pos_adv_data<size_id;pos_adv_data++)
    {
      adv_manuf_data_data[pos_adv_data]  = device_id>>(8*(size_id-pos_adv_data-1));
    }
  
     //Battery level
    adv_manuf_data_data[pos_adv_data] = battery_management_get_level();
//uint16_t adc_val  = adc_batt_management_get_adc_value();  
//adv_manuf_data_data[pos_adv_data-1] = adc_val>>8;
//adv_manuf_data_data[pos_adv_data] = adc_val;
    memset(&init, 0, sizeof(init));

    init.advdata.name_type          = BLE_ADVDATA_FULL_NAME;
    init.advdata.include_appearance = false;
    init.advdata.flags              = BLE_GAP_ADV_FLAGS_LE_ONLY_GENERAL_DISC_MODE;
    
    adv_manuf_data_array.p_data 							 = adv_manuf_data_data;
		adv_manuf_data_array.size 								 = sizeof(adv_manuf_data_data);
		adv_manuf_data.data 											 = adv_manuf_data_array;
		init.advdata.p_manuf_specific_data 				 = &adv_manuf_data;

    init.srdata.uuids_complete.uuid_cnt = sizeof(m_adv_uuids) / sizeof(m_adv_uuids[0]);
    init.srdata.uuids_complete.p_uuids  = m_adv_uuids;

    init.config.ble_adv_fast_enabled  = true;
    init.config.ble_adv_directed_slow_enabled = true;
    init.config.ble_adv_fast_interval = adv_interval_ms;
    init.config.ble_adv_fast_timeout  = APP_ADV_TIMEOUT_IN_SECONDS;

    init.evt_handler = on_adv_evt;

    err_code = ble_advertising_init(&m_advertising, &init);
    APP_ERROR_CHECK(err_code);

    ble_advertising_conn_cfg_tag_set(&m_advertising, APP_BLE_CONN_CFG_TAG);
}

/**************************************************
* Function name	: void bluetooth_management_init_id(void)
*    returns		: ---
*    args			  : --- 
* Created by		: María Viqueira
* Date created	: 2018/09/03
* Description		: Reads unique identifiers
* Notes			    : restrictions, odd modes
**************************************************/
void bluetooth_management_init_id(void)
{
		uint32_t data_buff[1]	=	{0};
		uint32_t *p_page_base_address ;
		
		//Addres for ID
		p_page_base_address = (uint32_t*)MY_ARRAY_ADDRESS;
    memcpy(data_buff,p_page_base_address,4);
		
		device_id =	(data_buff[0]<<24)	+	 ((data_buff[0]<<8)&0x00FF0000)+	((data_buff[0]>>8)&0x0000FF00) +	(data_buff[0]>>24);
}
/**************************************************
* Function name	: void bluetooth_management_init(void)
*    returns		: ----
*    args			  : bool erase_bonds: true for erasing 
*                 bondings
* Created by		: María Viqueira
* Date created	: 2018/05/17
* Description		: Bluetooth initialization 
* Notes			    : restrictions, odd modes
**************************************************/
void bluetooth_management_init(bool erase_bonds)
{
		
    bluetooth_management_init_id();
    gap_params_init();
    gatt_init();
    advertising_init();
    services_init();
    conn_params_init();
    
    ble_advertising_start(&m_advertising, BLE_ADV_MODE_FAST);
}
/**************************************************
* Function name	: bool bluetooth_management_get_status_connected(void)
*    returns		: bool: true if we are connected, otherwise: false
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/24
* Description		: indicates whether device is connected or not 
* Notes			    : restrictions, odd modes
**************************************************/
bool bluetooth_management_get_status_connected(void)
{
    return status_connected;
}
/**************************************************
* Function name	: bool bluetooth_management_get_paired_success(void)
*    returns		: bool: true if we are connected, otherwise: false
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/19
* Description		: indicates whether device is paired or not 
* Notes			    : restrictions, odd modes
**************************************************/
bool bluetooth_management_get_paired_success(void)
{
    return paired_successful;
}
/**************************************************
* Function name	: void bluetooth_management_stop_connection(void)
*    returns		: ---
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/05/24
* Description		: disconnects from BLE 
* Notes			    : restrictions, odd modes
**************************************************/
void bluetooth_management_stop_connection(void)
{
    sd_ble_gap_disconnect(m_conn_handle,BLE_HCI_REMOTE_USER_TERMINATED_CONNECTION);
}
/**************************************************
* Function name	: void bluetooth_management_advertising_pairing(void)
*    returns		: ---
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: initializes the advertising for pairing mode
* Notes			    : restrictions, odd modes
**************************************************/
void bluetooth_management_advertising_pairing(void)
{
    paired_successful = false; 
  
    if(bluetooth_management_get_status_connected())
    {
        bluetooth_management_stop_connection();
        nrf_delay_ms(100);
    }
    //Stop advertising
    sd_ble_gap_adv_stop();
            
    //Start advertising, erase bonds
    advertising_start(true);   
}
/**************************************************
* Function name	: void bluetooth_management_advertising_on(void)
*    returns		: ---
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: initializes the advertising for ON mode
* Notes			    : restrictions, odd modes
**************************************************/
void bluetooth_management_advertising_on(void)
{
    //
    if(!bluetooth_management_get_status_connected())
    {
        //Stop advertising
        sd_ble_gap_adv_stop();
            
        //Start advertising, don't erase bonds
        advertising_start(false);   
     }
    
}

/**************************************************
* Function name	: uint16_t bluetooth_management_get_adv_interval(void)
*    returns		: uint16_t current advertising interval, in ms
*    args			  : ---
* Created by		: María Viqueira
* Date created	: 2018/07/03
* Description		: Returns advertising interval
* Notes			: restrictions, odd modes
**************************************************/
uint16_t bluetooth_management_get_adv_interval(void)
{
    uint16_t  adv_ms;
    //We have to multiply for 0.625
    adv_ms  = adv_interval_ms*625/1000; 
    return adv_ms;
}

/**************************************************
* Function name	: void bluetooth_management_new_adv_interval(uint16_t adv_int)
*    returns		: ---
*    args			  : uint16_t new advertising interval in ms
* Created by		: María Viqueira
* Date created	: 2018/07/03
* Description		: Updates advertising interval
* Notes			: restrictions, odd modes
**************************************************/
void bluetooth_management_new_adv_interval(uint16_t adv_int)
{
    adv_interval_ms = MSEC_TO_UNITS(adv_int,UNIT_0_625_MS);
}

/**************************************************
* Function name	: void bluetooth_management_softdevice_stop(void)
*    returns		: ---
*    args			  : ---
* Created by		: María Viqueira
* Date created	: 2018/05/07
* Description		: Disables softdevice
* Notes			: restrictions, odd modes
**************************************************/
void bluetooth_management_softdevice_stop(void)
  {
		if(FLAG_SOFT	==	true)
		{
			
			ble_conn_params_stop();
			nrf_sdh_disable_request();
			FLAG_SOFT	=	false;
		}
		

}
  
/**************************************************
* Function name	: void bluetooth_management_update_pass(boot pass_update)
*    returns		: ---
*    args			  : bool: false: shows 0x00. True: shows real password
* Created by		: María Viqueira
* Date created	: 2018/07/18
* Description		: updates the characteristic with the new password
* Notes			    : restrictions, odd modes
**************************************************/
void bluetooth_management_update_pass(bool show_pass)
{
    static uint32_t err_code;
    err_code  = pass_char_update(&m_ble_control_service,show_pass);
    APP_ERROR_CHECK(err_code);
}

/**************************************************
* Function name	: void bluetooth_management_change_advertising(void)
*    returns		: ---
*    args			  : ---
* Created by		: María Viqueira
* Date created	: 2018/09/17
* Description		: Restarts advertising
* Notes			    : restrictions, odd modes
**************************************************/
void bluetooth_management_change_advertising(void)
{
    //Stop advertising
    sd_ble_gap_adv_stop();
  
    //Start
    advertising_init();
    ble_advertising_start(&m_advertising, BLE_ADV_MODE_FAST);
    
}
