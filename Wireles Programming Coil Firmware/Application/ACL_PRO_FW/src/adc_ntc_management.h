
#ifndef ADC_ntc_MANAGEMENT_H__
#define ADC_ntc_MANAGEMENT_H__

#define   LIMIT_NTC_LEVEL       4900

/**************************************
* Functions
**************************************/
/**************************************************
* Function name	: void adc_ntc_management_init(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/05
* Description		: ADC initialization 
* Notes			    : restrictions, odd modes
**************************************************/
void adc_ntc_management_init(void);

/**************************************************
* Function name	: void adc_ntc_management_read_adc(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/05
* Description		: Start reading ADC
* Notes			    : restrictions, odd modes
**************************************************/
void adc_ntc_management_read_adc(void);

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
void adc_ntc_management_conversion_attended(void);

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
bool adc_ntc_management_get_status_conversion(void);

/**************************************************
* Function name	: uint8_t adc_ntc_management_get_ntc_level(void)

*    returns		: uint8_t: ntcery level, in %
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/09/05
* Description		: Return the NTC level
* Notes			    : restrictions, odd modes
**************************************************/
uint16_t adc_ntc_management_get_ntc_level(void);

//void prueba_adc_ntc(void);

#endif //ADC_ntc_MANAGEMENT_H__


