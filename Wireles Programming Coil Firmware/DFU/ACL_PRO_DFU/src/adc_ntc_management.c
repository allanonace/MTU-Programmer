/***************************************************
* Module name: adc_ntc_management.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/09/05 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for managing ADC of the NTC
*
***************************************************/
/*  Include section
* Add all #includes here
*
***************************************************/
#include "nrf_drv_saadc.h"
#include "nrf_gpio.h"

#include "adc_ntc_management.h"
#include "pinout_ACL_PRO.h"

#include "nrf_delay.h"


/*  Defines section
* Add all #defines here
*
***************************************************/

/**@brief Macro to convert the result of ADC conversion in millivolts.
 *
 * @param[in]  ADC_VALUE   ADC result.
 *
 * @retval     Result converted to millivolts.
 */
#define ADC_NTC_RESULT_IN_MILLI_VOLTS(ADC_VALUE)        ((((ADC_VALUE) * ADC_REF_VOLTAGE_IN_MILLIVOLTS) / ADC_RES_10BIT) * ADC_PRE_SCALING_COMPENSATION)
        
#define SAMPLES_IN_BUFFER 5
#define ADC_REF_VOLTAGE_IN_MILLIVOLTS   600                                     /**< Reference voltage (in milli volts) used by ADC while doing conversion. */
#define ADC_PRE_SCALING_COMPENSATION    6                                       /**< The ADC is configured to use VDD with 1/3 prescaling as input. And hence the result of conversion is to be multiplied by 3 to get the actual value of the ntcery voltage.*/
#define DIODE_FWD_VOLT_DROP_MILLIVOLTS  270                                     /**< Typical forward voltage drop of the diode . */
#define ADC_RES_10BIT                   1024                                    /**< Maximum digital value for 10-bit ADC conversion. */

#define MAXIMUM_MILVOLT                 4200
#define MINIMUM_MILVOLT                 3000

//Voltage divider
#define R1        10000
#define VIN_DIV   3300


/*  Variables section
* Add all variables here
*
***************************************************/
static nrf_saadc_value_t  adc_ntc_buf[2];
nrf_saadc_value_t         ADC_NTC_RESULT;
uint16_t                  ntc_lvl_in_milli_volts;
uint16_t                  ntc_resistance;

bool adc_ntc_converted = false;

 /**@brief Function for handling the ADC interrupt.
 *
 * @details  This function will fetch the conversion result from the ADC, convert the value into
 *           percentage and send it to peer.
 */
static void saadc_ntc_event_handler(nrf_drv_saadc_evt_t const * p_event)
{
    if (p_event->type == NRF_DRV_SAADC_EVT_DONE)
    {
       
        uint32_t          err_code;

        ADC_NTC_RESULT = p_event->data.done.p_buffer[0];

        err_code = nrf_drv_saadc_buffer_convert(p_event->data.done.p_buffer, 1);
        APP_ERROR_CHECK(err_code);

        
        ntc_lvl_in_milli_volts = ADC_NTC_RESULT_IN_MILLI_VOLTS(ADC_NTC_RESULT);
      
        ntc_resistance  = R1*ntc_lvl_in_milli_volts/(VIN_DIV-ntc_lvl_in_milli_volts);
        
        adc_ntc_converted = true;

    }
      nrf_drv_saadc_uninit();

}

/**@brief Function for configuring ADC to do NTC level conversion.
 */
static void adc_ntc_configure(void)
{
    ret_code_t err_code = nrf_drv_saadc_init(NULL, saadc_ntc_event_handler);
    APP_ERROR_CHECK(err_code);

    nrf_saadc_channel_config_t config =
    NRF_DRV_SAADC_DEFAULT_CHANNEL_CONFIG_SE(CHARGER_NTC);
    err_code = nrf_drv_saadc_channel_init(1, &config);
    APP_ERROR_CHECK(err_code);

    err_code = nrf_drv_saadc_buffer_convert(&adc_ntc_buf[0], 1);
    APP_ERROR_CHECK(err_code);

    err_code = nrf_drv_saadc_buffer_convert(&adc_ntc_buf[1], 1);
    APP_ERROR_CHECK(err_code);
}

/**************************************************
* Function name	: bool adc_ntc_management_get_status_conversion(void)
*    returns		: bool: true if ADC conversion was finished,
*                       otherwise: false
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/05
* Description		: Indicates whether ADC conversion was done
* Notes			    : restrictions, odd modes
**************************************************/
bool adc_ntc_management_get_status_conversion(void)
{
    return adc_ntc_converted;
}

/**************************************************
* Function name	: void adc_ntc_management_conversion_attended(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/05
* Description		: Function to indicate ADC manager
*                 that the ADC read was attended
* Notes			    : restrictions, odd modes
**************************************************/
void adc_ntc_management_conversion_attended(void)
{
    adc_ntc_converted = false;
}

/**************************************************
* Function name	: uint8_t adc_ntc_management_get_ntc_level(void)

*    returns		: uint8_t: ntcery level, in %
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/05
* Description		: Return the NTC level
* Notes			    : restrictions, odd modes
**************************************************/
uint16_t adc_ntc_management_get_ntc_level(void)
{
    return ntc_resistance;
}


/**************************************************
* Function name	: void adc_ntc_management_read_adc(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/05
* Description		: Start reading ADC
* Notes			    : restrictions, odd modes
**************************************************/
void adc_ntc_management_read_adc(void)
{
      ret_code_t err_code;
      nrf_gpio_pin_set(BATT_EN);
      adc_ntc_management_init();
      err_code = nrf_drv_saadc_sample();
      APP_ERROR_CHECK(err_code);

}


/**************************************************
* Function name	: void adc_ntc_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/05
* Description		: ADC initialization 
* Notes			    : restrictions, odd modes
**************************************************/
void adc_ntc_management_init(void)
{
    adc_ntc_configure();
}



//void prueba_adc_ntc(void)
//{
//  adc_ntc_management_read_adc();
//  while(1);
//  
//}
