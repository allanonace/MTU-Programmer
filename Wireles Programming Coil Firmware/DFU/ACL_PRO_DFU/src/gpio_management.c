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
#include "nrf_ble_dfu.h"
#include "leds_management.h"
#include "battery_management.h"
#include "adc_ntc_management.h"


/*  Variables section
* Add all variables here
*
***************************************************/

/**************************************************
* Local variables
**************************************************/
//bool irq_status = false;


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
*    args			  : bool: indicates whether softdevice
*                       must be initialized or not
* Created by		: María Viqueira
* Date created	: 2018/05/23
* Description		: function for managing deep sleep 
* Notes			: restrictions, odd modes
**************************************************/
void gpio_go_to_deep_sleep(bool init_softdevice)
{
  
    static uint32_t err_code  = 0;
    if(init_softdevice)
    {
      ble_stack_init(true);
    }
    leds_management_sleep();
    nrf_gpio_pin_clear(BATT_EN);
    
    if(battery_management_is_usb_connected())
    {
        battery_management_disable_charger();
    }
    else
    {      
      battery_management_enable_charger();
    }
     battery_management_enable_charger();
    if(adc_ntc_management_get_ntc_level()  <=  LIMIT_NTC_LEVEL)
    {
        //battery_management_disable_charger();
        //TODO
        nrf_gpio_pin_set(CHARGER_EN);
    }
    nrf_gpio_pin_clear(PIN_ACL_UART_EN);
    nrf_gpio_pin_clear(CHARGER_EN);
    deep_sleep_irq_prepare();
    err_code  = sd_power_system_off();
}



//static void in_pin_handler(nrf_drv_gpiote_pin_t pin, nrf_gpiote_polarity_t action)
//{
//   // irq_status  = true;
//}



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
    nrf_gpio_cfg_input(USB_DETECT,NRF_GPIO_PIN_PULLDOWN);

  
}
