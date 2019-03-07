#ifndef BATTERY_MANAGEMENT_H__
#define BATTERY_MANAGEMENT_H__

//Status for charger: discconected, charging, charged
typedef enum
{
  CHARGER_STATUS_DISCONNECTED,
  CHARGER_STATUS_CHARGING,
  CHARGER_STATUS_CHARGED  
    
}charger_status_t;

/**************************************
* Functions
**************************************/

/**************************************************
* Function name	: void battery_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Battery initialization 
* Notes			: restrictions, odd modes
**************************************************/
void battery_management_init(void);

/**************************************************
* Function name	: void battery_management_attended(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/22
* Description		: Function to indicate battery manager
*                 that the battery read was attended
* Notes			    : restrictions, odd modes
**************************************************/
void battery_management_attended(void);

/**************************************************
* Function name	: void battery_management_check_batt_level(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/22
* Description		: Function to check if new battery value
*                 must be sent to BLE
* Notes			    : restrictions, odd modes
**************************************************/
void battery_management_check_batt_level(void);


/**************************************************
* Function name	: void battery_management_exe(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/29
* Description		: Funtction for managing charger state machine
* Notes			: restrictions, odd modes
**************************************************/
void battery_management_exe(void);

/**************************************************
* Function name	: void battery_management_first_read(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/07/02
* Description		: First read before advertising
* Notes			    : restrictions, odd modes
**************************************************/
void battery_management_first_read(void);


/**************************************************
* Function name	: bool battery_management_disable_charger(void)
*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/05
* Description		: Function for disabling charger
* Notes			    : restrictions, odd modes
**************************************************/
void battery_management_disable_charger(void);

/**************************************************
* Function name	: bool battery_management_enable_charger(void)
*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/05
* Description		: Function for enable charger
* Notes			    : restrictions, odd modes
**************************************************/
void battery_management_enable_charger(void);

/**************************************************
* Function name	: bool battery_management_get_battery_needs_reading(void)
*    returns		: bool: true if battery timer was expired,
*                       otherwise: false
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/22
* Description		: Indicates whether battery must be read
* Notes			    : restrictions, odd modes
**************************************************/
bool battery_management_get_battery_needs_reading(void);

/**************************************************
* Function name	: bool battery_management_is_low_battery(void)
*    returns		: bool: true if temperature is high
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/05
* Description		: Indicates whether NTC temperature is high or not
* Notes			    : restrictions, odd modes
**************************************************/
bool battery_management_is_ntc_high(void);


/**************************************************
* Function name	: bool battery_management_is_low_battery(void)
*    returns		: bool: true if battery is under 20%,
*                       otherwise: false
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/18
* Description		: Indicates whether battery must is low
* Notes			    : restrictions, odd modes
**************************************************/
bool battery_management_is_low_battery(void);

/**************************************************
* Function name	: bool battery_management_is_usb_connected(void)
*    returns		: bool: true if USB is connected,
*                       otherwise: false
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/14
* Description		: Indicates whether USB is connected or not
* Notes			    : restrictions, odd modes
**************************************************/
bool battery_management_is_usb_connected(void);
/**************************************************
* Function name	: uint8_t battery_management_get_level(void)
*    returns		: uint8_t battery level
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/22
* Description		: Indicates the battery level, in %
* Notes			    : restrictions, odd modes
**************************************************/
uint8_t battery_management_get_level(void);

/**************************************************
* Function name	: uint8_t battery_management_get_status(void)
*    returns		: uint8_t battery status: 
*                   0: disconnected
*                   1: charging
*                   2: charged
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/10
* Description		: Indicates the status of the battery
* Notes			    : restrictions, odd modes
**************************************************/
uint8_t battery_management_get_status(void);

#endif //BATTERY_MANAGEMENT_H__


