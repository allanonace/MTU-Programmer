
#ifndef ADC_BATT_MANAGEMENT_H__
#define ADC_BATT_MANAGEMENT_H__

/**************************************
* Functions
**************************************/
/**************************************************
* Function name	: void adc_batt_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/22
* Description		: ADC initialization 
* Notes			    : restrictions, odd modes
**************************************************/
void adc_batt_management_init(void);

/**************************************************
* Function name	: void adc_batt_management_read_adc(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/22
* Description		: Start reading ADC
* Notes			    : restrictions, odd modes
**************************************************/
void adc_batt_management_read_adc(void);

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
void adc_batt_management_conversion_attended(void);

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
bool adc_batt_management_get_status_conversion(void);

/**************************************************
* Function name	: uint8_t adc_batt_management_get_batt_level(void)

*    returns		: uint8_t: battery level, in %
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/22
* Description		: Return the battery level
* Notes			    : restrictions, odd modes
**************************************************/
uint8_t adc_batt_management_get_batt_level(void);


//uint16_t adc_batt_management_get_adc_value(void);
//void prueba_adc_batt(void);


#endif //ADC_BATT_MANAGEMENT_H__


