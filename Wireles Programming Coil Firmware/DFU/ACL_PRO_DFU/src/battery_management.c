/***************************************************
* Module name: battery_management.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/06/18 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for managing Battery operations
*
***************************************************/
/*  Include section
* Add all #includes here
*
***************************************************/
#include "app_timer.h"
#include "nrf_gpio.h"
#include "nrf_delay.h"

#include "battery_management.h"
#include "adc_batt_management.h"
#include "pinout_ACL_PRO.h"
#include "adc_ntc_management.h"


/*  Defines section
* Add all #defines here
*
***************************************************/
#define   MAXIMUM_BATT_LEVEL      80  //% for stoping charger
#define   MINIMUM_BATT_LEVEL      10  //% for indicating low level


#define  BATTERY_TIMER_INTERVAL APP_TIMER_TICKS(60*1000)
APP_TIMER_DEF(m_battery_timer_id);

bool ntc_high  = false;

/*  Variables section
* Add all variables here
*
***************************************************/

//States for charger state machine
typedef enum
{
    CHARGER_STATE_IDLE,
    CHARGER_STATE_WAIT_ADC_BATT,
    CHARGER_STATE_WAIT_ADC_NTC
    
}charger_state_machine_t;

uint8_t battery_level       = 0x64; //Real battery level
uint8_t previous_batt_level = 101;  //Previous battery level. Forcing intialization different from battery_level
uint8_t battery_status      = 0x01; //Three states: charging, charge complete, discconected

uint8_t charger_state       = CHARGER_STATE_IDLE;

bool battery_needs_reading  = false;  //Indicates if we need to read battery
bool battery_low_level      = false;  //Indicates that battery level is under minimum %
bool charger_is_charging    = false;  //Indicates that battery is being charged



/*  Function Prototype Section
* Add prototypes for all functions called by this
* module, with the exception of runtime routines.
*
***************************************************/
/**************************************************
* Function name	: static void battery_meas_timeout_handler(void * p_context)

*    returns		: ----
*    args			  : unused pointer
* Created by		: María Viqueira
* Date created	: 2018/06/22
* Description		: Interruption of the timer to indicate that
*                 battery must be readed
* Notes			: restrictions, odd modes
**************************************************/
static void battery_meas_timeout_handler(void * p_context)
{
    battery_needs_reading = true;
//    nrf_gpio_pin_set(BATT_EN);
//    adc_batt_management_read_adc();
}

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
bool battery_management_is_usb_connected(void)
{
    if(nrf_gpio_pin_read(USB_DETECT)  ==  0)
    {
        return false;
    }
    else
    {
        return true;
    }
}

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
bool battery_management_is_low_battery(void)
{
    return battery_low_level;
}

/**************************************************
* Function name	: bool battery_management_is_low_battery(void)
*    returns		: bool: true if temperature is high
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/05
* Description		: Indicates whether NTC temperature is high or not
* Notes			    : restrictions, odd modes
**************************************************/
bool battery_management_is_ntc_high(void)
{
    return ntc_high;
}

/**************************************************
* Function name	: bool battery_management_disable_charger(void)
*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/05
* Description		: Function for disabling charger
* Notes			    : restrictions, odd modes
**************************************************/
void battery_management_disable_charger(void)
{
    nrf_gpio_pin_set(CHARGER_EN);
}

/**************************************************
* Function name	: bool battery_management_enable_charger(void)
*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/05
* Description		: Function for enable charger
* Notes			    : restrictions, odd modes
**************************************************/
void battery_management_enable_charger(void)
{
    nrf_gpio_pin_clear(CHARGER_EN);
  
}

/**************************************************
* Function name	: uint8_t battery_management_get_level(void)
*    returns		: uint8_t battery level
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/22
* Description		: Indicates the battery level, in %
* Notes			    : restrictions, odd modes
**************************************************/
uint8_t battery_management_get_level(void)
{
    return  battery_level;
}

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
uint8_t battery_management_get_status(void)
{
    return  battery_status;
}


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
bool battery_management_get_battery_needs_reading(void)
{
    return battery_needs_reading;
}

/**************************************************
* Function name	: uint8_t battery_management_status(void)
*    returns		: uint8_t: charger status
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/29
* Description		: Indicates the charger status: 
*                 disconnected, charging, charged
*                 to USB  
* Notes			    : restrictions, odd modes
**************************************************/
uint8_t battery_management_status(void)
{
    return battery_status;
}

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
void battery_management_attended(void)
{
    battery_needs_reading = false;
}

/**************************************************
* Function name	: void ntc_decode(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/05
* Description		: Function for decoding NTC values
* Notes			    : restrictions, odd modes
**************************************************/
void ntc_decode(void)
{
    uint16_t ntc_value;
    ntc_value = adc_ntc_management_get_ntc_level();
    if(ntc_value  <=  LIMIT_NTC_LEVEL)
    {
        battery_management_disable_charger();
        ntc_high = true;
    }
    else
    {
        ntc_high  = false;
    }
    adc_ntc_management_conversion_attended();
    
}

/**************************************************
* Function name	: void battery_decode(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/22
* Description		: Function for decoding battery values
* Notes			    : restrictions, odd modes
**************************************************/
void battery_decode(void)
{
     battery_level = adc_batt_management_get_batt_level();
  
    //We need to decode data
    if( battery_level >=  MAXIMUM_BATT_LEVEL)
    {
        //Disable charger
        battery_management_disable_charger();
        
        battery_status  = CHARGER_STATUS_DISCONNECTED;
    }
    else
    {
        battery_management_enable_charger();
    }
    
    if( battery_level <=  MINIMUM_BATT_LEVEL)
    {
        battery_low_level = true;
    }

    
    //Attended
    adc_batt_management_conversion_attended();
    
  
}
/**************************************************
* Function name	: void battery_management_first_read(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/07/02
* Description		: First read before advertising
* Notes			    : restrictions, odd modes
**************************************************/
void battery_management_first_read(void)
{
    uint8_t delay_counter = 100;
  
    
     //Battery enable
    nrf_gpio_pin_set(BATT_EN);
  
    //Start ADC reading
    adc_batt_management_read_adc();
  
    //Active delay
    while(!adc_batt_management_get_status_conversion()  &&  (delay_counter  > 0))
    {
        nrf_delay_ms(delay_counter);
        delay_counter--;
    }
    
    delay_counter = 100;
    adc_ntc_management_read_adc();
     while(!adc_ntc_management_get_status_conversion()  &&  (delay_counter  > 0))
     {
        nrf_delay_ms(delay_counter);
        delay_counter--;
    }  
    
    battery_decode();
    
    
}


/**************************************************
* Function name	: void battery_management_exe(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/29
* Description		: Funtction for managing charger state machine
* Notes			: restrictions, odd modes
**************************************************/
void battery_management_exe(void)
{
    
    switch(charger_state)
    {
       
        
        case CHARGER_STATE_IDLE:
          if(battery_needs_reading)
          {
              
              //Battery enable
              nrf_gpio_pin_set(BATT_EN);
            
              //Start ADC reading
              adc_batt_management_read_adc();
            
              battery_needs_reading = false;
              charger_state = CHARGER_STATE_WAIT_ADC_BATT;
          }
        break;
        
       
        
        case CHARGER_STATE_WAIT_ADC_BATT:
          if(adc_batt_management_get_status_conversion())
          {
              battery_decode();
              if(nrf_gpio_pin_read(USB_DETECT)  ==  0)
              {
                  //USB is not connnected, we don't need to read NTC
                  charger_state = CHARGER_STATE_IDLE;
                  nrf_gpio_pin_clear(BATT_EN);
              }
              else
              {
                  //USB is connected
                  //Start ADC reading
                  adc_ntc_management_read_adc();
                  charger_state = CHARGER_STATE_WAIT_ADC_NTC;
              }
          }
        break;
          
          
        case CHARGER_STATE_WAIT_ADC_NTC:
           if(adc_ntc_management_get_status_conversion())
           {
              ntc_decode();
           }
        break;
        
    }
}

/**************************************************
* Function name	: void battery_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/18
* Description		: Battery initialization 
* Notes			: restrictions, odd modes
**************************************************/
void battery_management_init(void)
{
    uint32_t  err_code;
    
    //Enables:
    nrf_gpio_cfg_output(CHARGER_EN);
    nrf_gpio_cfg_output(BATT_EN);
  
    //Stat
    nrf_gpio_cfg_input(CHARGER_STAT,NRF_GPIO_PIN_PULLUP);
    
    err_code = app_timer_create(&m_battery_timer_id,APP_TIMER_MODE_REPEATED,battery_meas_timeout_handler);
    APP_ERROR_CHECK(err_code);
    err_code = app_timer_start(m_battery_timer_id,BATTERY_TIMER_INTERVAL,NULL);
    APP_ERROR_CHECK(err_code);

    //Enable charger
    battery_management_enable_charger();
  
    
}

