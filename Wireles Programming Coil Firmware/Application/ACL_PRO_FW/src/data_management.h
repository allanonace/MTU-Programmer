#ifndef DATA_MANAGEMENT_H__
#define DATA_MANAGEMENT_H__


#include <stdint.h>

	

/**************************************
* Functions
**************************************/ 
/**************************************************
* Function name	: void data_management_ble_received(void)
*    returns		: ----
*    args			  : uint8_t *p_data: pointer to the received data
*               : uint16_t length: number of bytes received
* Created by		: María Viqueira
* Date created	: 2018/05/18
* Description		: BLE interruption to indicate data was received
* Notes			: restrictions, odd modes
**************************************************/
void      data_management_ble_received(void);

/**************************************************
* Function name	: void data_management_ble_decode(void)
*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/18
* Description		: function for decoding data received
*               : from BLE
* Notes			: restrictions, odd modes
**************************************************/
void      data_management_ble_decode(void);

/**************************************************
* Function name	: uint16_t  data_management_ble_get_num_data(void)
*    returns		: uint16_t number of BLE data
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/18
* Description		: Returns the number of no decoded data from BLE
* Notes			    : restrictions, odd modes
**************************************************/
uint16_t  data_management_ble_get_num_data(void);

/*********************************************************************/
/**************************************************
* Function name	: void data_management_uart_received(void)
*    returns		: ---
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/05/18
* Description		: Physical UART intteruption
* Notes			    : restrictions, odd modes
**************************************************/
void      data_management_uart_received(void);

/**************************************************
* Function name	: uint32_t data_management_uart_decode(void)
*    returns		: uint32_t with error type: 0, success. 
*                 Otherwise, error      
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/18
* Description		: function for decoding data received
*               : from UART
* Notes			    : restrictions, odd modes
**************************************************/
uint32_t  data_management_uart_decode(void);

/**************************************************
* Function name	: uint16_t  data_management_uart_get_num_data(void)
*    returns		: uint16_t number of data from physical UART
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/18
* Description		: Returns the number of no decoded data from 
*                pyhiscal UART
* Notes			    : restrictions, odd modes
**************************************************/
uint16_t  data_management_uart_get_num_data(void);


/*********************************************************************/
/**************************************************
* Function name	: void data_management_init(void)
*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/18
* Description		: Data management inicialization
* Notes			: restrictions, odd modes
**************************************************/
void data_management_init(void);

/**************************************************
* Function name	: void data_management_timer_start(void)
*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/28
* Description		: Timer for sending data to BLE, on
* Notes			: restrictions, odd modes
**************************************************/
void data_management_timer_start(void);

/**************************************************
* Function name	: void data_management_timer_stop(void)
*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/28
* Description		: Timer for sending data to BLE, off
* Notes			: restrictions, odd modes
**************************************************/
void data_management_timer_stop(void);

/**************************************************
* Function name	: void data_management_init(void)
*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/28
* Description		: Creates timer for BLE sending
* Notes			: restrictions, odd modes
**************************************************/
void data_management_create_timer(void);

//void prueba_data_management(void);
//void prueba_uart(void);

#endif //DATA_MANAGEMENT_H__



