/***************************************************
* Module name: adc_batt_management.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/06/22 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for managing ADC of the battery
*
***************************************************/
/*  Include section
* Add all #includes here
*
***************************************************/
#include "nrf_drv_saadc.h"
#include "nrf_gpio.h"

#include "adc_batt_management.h"
#include "pinout_ACL_PRO.h"



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
#define adc_batt_result_IN_MILLI_VOLTS(ADC_VALUE)        ((((ADC_VALUE) * ADC_REF_VOLTAGE_IN_MILLIVOLTS) / ADC_RES_10BIT) * ADC_PRE_SCALING_COMPENSATION)
        
#define SAMPLES_IN_BUFFER 5
#define ADC_REF_VOLTAGE_IN_MILLIVOLTS   600                                     /**< Reference voltage (in milli volts) used by ADC while doing conversion. */
#define ADC_PRE_SCALING_COMPENSATION    6                                       /**< The ADC is configured to use VDD with 1/3 prescaling as input. And hence the result of conversion is to be multiplied by 3 to get the actual value of the battery voltage.*/
#define DIODE_FWD_VOLT_DROP_MILLIVOLTS  70   //Empyric                                  /**< Typical forward voltage drop of the diode . */
#define ADC_RES_10BIT                   1024                                    /**< Maximum digital value for 10-bit ADC conversion. */

#define MAXIMUM_MILVOLT                 4200 //Empyric
#define MINIMUM_MILVOLT                 0xAF0//0x9c0

//Voltage divider
#define R1  11
#define R2  33

#define VOLTAGE_DIVIDER_FACTOR          ( (R1+R2) / R1)

/*  Variables section
* Add all variables here
*
***************************************************/
static nrf_saadc_value_t    adc_batt_buf[2];
nrf_saadc_value_t           adc_batt_result;
uint16_t                    batt_lvl_in_milli_volts;
uint8_t                     percentage_batt_lvl;

bool adc_batt_converted = false;


/*  Function Prototype Section
* Add prototypes for all functions called by this
* module, with the exception of runtime routines.
*
***************************************************/

 /**@brief Function for handling the ADC interrupt.
 *
 * @details  This function will fetch the conversion result from the ADC, convert the value into
 *           percentage and send it to peer.
 */
static void saadc_event_handler(nrf_drv_saadc_evt_t const * p_event)
{
    if (p_event->type == NRF_DRV_SAADC_EVT_DONE)
    {
       
        uint32_t          err_code;

        adc_batt_result = p_event->data.done.p_buffer[0];

        err_code = nrf_drv_saadc_buffer_convert(p_event->data.done.p_buffer, 1);
        APP_ERROR_CHECK(err_code);

       
        batt_lvl_in_milli_volts = adc_batt_result_IN_MILLI_VOLTS(adc_batt_result);
        
        //Before voltage divider (real):
        batt_lvl_in_milli_volts = batt_lvl_in_milli_volts*VOLTAGE_DIVIDER_FACTOR+DIODE_FWD_VOLT_DROP_MILLIVOLTS;
        
      
        if (batt_lvl_in_milli_volts >= MAXIMUM_MILVOLT)
        {
            percentage_batt_lvl = 100;
        }
        else if (batt_lvl_in_milli_volts  <= MINIMUM_MILVOLT)
        {
            percentage_batt_lvl = 0;
        }
        else
        {
          //Normalize
          percentage_batt_lvl = 100*(batt_lvl_in_milli_volts  - MINIMUM_MILVOLT)/(MAXIMUM_MILVOLT - MINIMUM_MILVOLT);
        }
        adc_batt_converted = true;

    }
    nrf_drv_saadc_uninit();
}

/**@brief Function for configuring ADC to do battery level conversion.
 */
static void adc_batt_configure(void)
{
    ret_code_t err_code = nrf_drv_saadc_init(NULL, saadc_event_handler);
    APP_ERROR_CHECK(err_code);

    nrf_saadc_channel_config_t config =
    NRF_DRV_SAADC_DEFAULT_CHANNEL_CONFIG_SE(CHARGER_ADC);
    err_code = nrf_drv_saadc_channel_init(0, &config);
    APP_ERROR_CHECK(err_code);

  
    err_code = nrf_drv_saadc_buffer_convert(&adc_batt_buf[0], 1);
    APP_ERROR_CHECK(err_code);

    err_code = nrf_drv_saadc_buffer_convert(&adc_batt_buf[1], 1);
    APP_ERROR_CHECK(err_code);
}

/**************************************************
* Function name	: bool adc_batt_management_get_status_conversion(void)
*    returns		: bool: true if ADC conversion was finished,
*                       otherwise: false
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/22
* Description		: Indicates whether ADC conversion was done
* Notes			    : restrictions, odd modes
**************************************************/
bool adc_batt_management_get_status_conversion(void)
{
    return adc_batt_converted;
}

/**************************************************
* Function name	: void adc_batt_management_conversion_attended(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/22
* Description		: Function to indicate ADC manager
*                 that the ADC read was attended
* Notes			    : restrictions, odd modes
**************************************************/
void adc_batt_management_conversion_attended(void)
{
    adc_batt_converted = false;
}

/**************************************************
* Function name	: uint8_t adc_batt_management_get_batt_level(void)

*    returns		: uint8_t: battery level, in %
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/22
* Description		: Return the battery level
* Notes			    : restrictions, odd modes
**************************************************/
uint8_t adc_batt_management_get_batt_level(void)
{
    return percentage_batt_lvl;
}


/**************************************************
* Function name	: void adc_batt_management_read_adc(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/22
* Description		: Start reading ADC
* Notes			    : restrictions, odd modes
**************************************************/
void adc_batt_management_read_adc(void)
{
      ret_code_t err_code;
      adc_batt_management_init();
      err_code = nrf_drv_saadc_sample();
      APP_ERROR_CHECK(err_code);

}


/**************************************************
* Function name	: void adc_batt_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/22
* Description		: ADC initialization 
* Notes			    : restrictions, odd modes
**************************************************/
void adc_batt_management_init(void)
{
    adc_batt_configure();

}


//uint16_t adc_batt_management_get_adc_value(void)
//{
//    return  batt_lvl_in_milli_volts;
//}
//void prueba_adc_batt(void)
//{
//  adc_batt_management_read_adc();
//  while(1);
//  
//}
