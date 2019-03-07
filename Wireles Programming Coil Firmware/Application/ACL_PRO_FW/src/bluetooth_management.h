#ifndef BLUETOOTH_MANAGEMENT_H__
#define BLUETOOTH_MANAGEMENT_H__

#define APP_ADV_INTERVAL                    MSEC_TO_UNITS(1000,UNIT_0_625_MS);          /**< The advertising interval (in units of 0.625 ms.*/
#define APP_ADV_TIMEOUT_IN_SECONDS          0                                         /**< The advertising timeout (in units of seconds). */
#define MANUFACTURER_NAME                   "NordicSemiconductor"                      /**< Manufacturer. Will be passed to Device Information Service. */
#define APP_BLE_OBSERVER_PRIO               3                                          /**< Application's BLE observer priority. You shouldn't need to modify this value. */
#define APP_BLE_CONN_CFG_TAG                1                                          /**< A tag identifying the SoftDevice BLE configuration. */
/*lint -emacro(524, MIN_CONN_INTERVAL) // Loss of precision */
#define MIN_CONN_INTERVAL                   MSEC_TO_UNITS(7.5, UNIT_1_25_MS)           /**< Minimum connection interval (7.5 ms) */
#define MAX_CONN_INTERVAL                   MSEC_TO_UNITS(30, UNIT_1_25_MS)            /**< Maximum connection interval (30 ms). */
#define SLAVE_LATENCY                       6                                          /**< Slave latency. */
#define CONN_SUP_TIMEOUT                    MSEC_TO_UNITS(430, UNIT_10_MS)             /**< Connection supervisory timeout (430 ms). */
#define FIRST_CONN_PARAMS_UPDATE_DELAY      APP_TIMER_TICKS(5000)                      /**< Time from initiating event (connect or start of notification) to first time sd_ble_gap_conn_param_update is called (5 seconds). */
#define NEXT_CONN_PARAMS_UPDATE_DELAY       APP_TIMER_TICKS(30000)                     /**< Time between each call to sd_ble_gap_conn_param_update after the first call (30 seconds). */
#define MAX_CONN_PARAMS_UPDATE_COUNT        3                                          /**< Number of attempts before giving up the connection parameter negotiation. */
#define SEC_PARAM_BOND                      1                                          /**< Perform bonding. */
#define SEC_PARAM_MITM                      0                                          /**< Man In The Middle protection not required. */
#define SEC_PARAM_LESC                      0                                          /**< LE Secure Connections not enabled. */
#define SEC_PARAM_KEYPRESS                  0                                          /**< Keypress notifications not enabled. */
#define SEC_PARAM_IO_CAPABILITIES           BLE_GAP_IO_CAPS_NONE                       /**< No I/O capabilities. */
#define SEC_PARAM_OOB                       0                                          /**< Out Of Band data not available. */
#define APP_FEATURE_NOT_SUPPORTED           BLE_GATT_STATUS_ATTERR_APP_BEGIN + 2       /**< Reply when unsupported features are requested. */
#define DEAD_BEEF                           0xDEADBEEF                                 /**< Value used as error code on stack dump, can be used to identify stack location on stack unwind. */


#define BLUETOOTH_MANAGEMENT_MIN_ADV_INTERVAL      (50)
#define BLUETOOTH_MANAGEMENT_MAX_ADV_INTERVAL      (1*1000)

/**************************************
* Functions
**************************************/

/**************************************************
* Function name	: void bluetooth_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/17
* Description		: Bluetooth initialization and advertising 
* Notes			: restrictions, odd modes
**************************************************/
void bluetooth_management_init(bool erase_bonds);

/**@brief Function for initializing the BLE stack.
 *
 * @details Initializes the SoftDevice and the BLE event interrupt.
 */
void bluetooth_management_ble_stack_init(void);

/**************************************************
* Function name	: bool bluetooth_management_get_status_connected(void)
*    returns		: bool: true if we are connected, otherwise: false
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/24
* Description		: indicates whether device is connected or not 
* Notes			    : restrictions, odd modes
**************************************************/
bool bluetooth_management_get_status_connected(void);

/**************************************************
* Function name	: void bluetooth_management_stop_connection(void)
*    returns		: ---
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/05/24
* Description		: disconnects from BLE 
* Notes			    : restrictions, odd modes
**************************************************/
void bluetooth_management_stop_connection(void);

/**************************************************
* Function name	: void bluetooth_management_advertising_pairing(void)
*    returns		: ---
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: initializes the advertising for pairing mode
* Notes			    : restrictions, odd modes
**************************************************/
void bluetooth_management_advertising_pairing(void);

/**************************************************
* Function name	: void bluetooth_management_advertising_on(void)
*    returns		: ---
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: initializes the advertising for ON mode
* Notes			    : restrictions, odd modes
**************************************************/
void bluetooth_management_advertising_on(void);

/**************************************************
* Function name	: void bluetooth_management_new_adv_interval(uint16_t adv_int)
*    returns		: ---
*    args			  : uint16_t new advertising interval in ms
* Created by		: María Viqueira
* Date created	: 2018/07/03
* Description		: Updates advertising interval
* Notes			: restrictions, odd modes
**************************************************/
void bluetooth_management_new_adv_interval(uint16_t adv_int);

/**************************************************
* Function name	: void bluetooth_management_softdevice_stop(void)
*    returns		: ---
*    args			  : ---
* Created by		: María Viqueira
* Date created	: 2018/05/07
* Description		: Disables softdevice
* Notes			: restrictions, odd modes
**************************************************/
void bluetooth_management_softdevice_stop(void);

/**************************************************
* Function name	: void bluetooth_management_update_pass(boot pass_update)
*    returns		: ---
*    args			  : bool: false: shows 0x00. True: shows real password
* Created by		: María Viqueira
* Date created	: 2018/07/18
* Description		: updates the characteristic with the new password
* Notes			    : restrictions, odd modes
**************************************************/
void bluetooth_management_update_pass(bool show_pass);

/**************************************************
* Function name	: void bluetooth_management_change_advertising(void)
*    returns		: ---
*    args			  : ---
* Created by		: María Viqueira
* Date created	: 2018/09/17
* Description		: Restarts advertising
* Notes			    : restrictions, odd modes
**************************************************/
void bluetooth_management_change_advertising(void);

/**************************************************
* Function name	: bool bluetooth_management_get_paired_success(void)
*    returns		: bool: true if we are connected, otherwise: false
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/19
* Description		: indicates whether device is paired or not 
* Notes			    : restrictions, odd modes
**************************************************/
bool bluetooth_management_get_paired_success(void);

/**************************************************
* Function name	: uint16_t bluetooth_management_get_adv_interval(void)
*    returns		: uint16_t current advertising interval, in ms
*    args			  : ---
* Created by		: María Viqueira
* Date created	: 2018/07/03
* Description		: Returns advertising interval
* Notes			: restrictions, odd modes
**************************************************/
uint16_t bluetooth_management_get_adv_interval(void);


#endif


