/***************************************************
* Module name: gpio_management.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/05/23 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for controlling gpio interrupts and outputs
***************************************************/

/*  Include section
* Add all #includes here
*
***************************************************/
#include "nrf_gpio.h"
#include "nrf_delay.h"
#include "gpio_management.h"
#include "pinout_ACL_PRO.h"
#include "nrf_drv_gpiote.h"
#include "nrf_soc.h"
#include "uart_management.h"
#include "leds_management.h"
#include "flash_management.h"
#include "connection_control_management.h"
#include "nrf_power.h"
#include "bluetooth_management.h"
#include "battery_management.h"
#include "adc_ntc_management.h"
#include "watchdog_management.h"

/*  Variables section
* Add all variables here
*
***************************************************/


/*  Function Prototype Section
* Add prototypes for all functions called by this
* module, with the exception of runtime routines.
*
***************************************************/

/**************************************************
* Function name	: static void deep_sleep_irq_prepare(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/23
* Description		: Configures GPIOS for waking from SLEEP 
* Notes			: restrictions, odd modes
**************************************************/
static void deep_sleep_irq_prepare(void)
{

  nrf_gpio_cfg_sense_input(BUTTON_ACL,
                            NRF_GPIO_PIN_NOPULL,
														NRF_GPIO_PIN_SENSE_HIGH);
  
   if(!battery_management_is_usb_connected())
  {
     nrf_gpio_cfg_sense_input(USB_DETECT,
                               NRF_GPIO_PIN_PULLDOWN,
                               NRF_GPIO_PIN_SENSE_HIGH);
  }
  else
  {
      nrf_gpio_cfg_sense_input(USB_DETECT,
                               NRF_GPIO_PIN_PULLDOWN,
                               NRF_GPIO_PIN_SENSE_LOW);
  }
}

/**************************************************
* Function name	: void gpio_go_to_deep_sleep(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/23
* Description		: function for managing deep sleep 
* Notes			: restrictions, odd modes
**************************************************/
void gpio_go_to_deep_sleep(void)
{
    static uint32_t err_code  = 0;
    leds_management_all_off();
    uart_management_stop();
    
    nrf_gpio_pin_clear(PIN_ACL_UART_EN);
    nrf_gpio_pin_clear(BATT_EN);
  
    if(battery_management_is_usb_connected())
    {
        battery_management_disable_charger();
    }
    else
    {      
      battery_management_enable_charger();
    }
  
    if(adc_ntc_management_get_ntc_level()  <=  LIMIT_NTC_LEVEL)
    {
        //battery_management_disable_charger();
        //TODO
        nrf_gpio_pin_set(CHARGER_EN);
    }
   
    nrf_gpio_pin_clear(PIN_ACL_3V3_ADP);
  
    flash_management_check_write();
    //Update GPREGRET
  
    bluetooth_management_softdevice_stop();
    NRF_POWER->GPREGRET = connection_control_management_get_number_paired_times();
    bluetooth_management_ble_stack_init();
    deep_sleep_irq_prepare();
    err_code  = sd_power_system_off();
    APP_ERROR_CHECK(err_code);
}



/**************************************************
* Function name	: void gpio_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/23
* Description		: GPIO initialization 
* Notes			: restrictions, odd modes
**************************************************/
void gpio_management_init(void)
{
    //DCDC enable
   nrf_gpio_cfg_output(PIN_ACL_UART_EN);
   nrf_gpio_pin_clear(PIN_ACL_UART_EN);
   nrf_gpio_cfg_output(PIN_ACL_3V3_ADP);
   nrf_gpio_pin_clear(PIN_ACL_3V3_ADP);
  
   nrf_gpio_cfg_input(USB_DETECT,NRF_GPIO_PIN_PULLDOWN);
  
  
    
  
}
