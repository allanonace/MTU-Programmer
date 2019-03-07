#ifndef UART_MANAGEMENT_H__
#define UART_MANAGEMENT_H__

#include  "pinout_ACL_PRO.h"


#define RX_PIN    PIN_ACL_PRO_UART_RX   
#define TX_PIN    PIN_ACL_PRO_UART_TX   
#define CTS_PIN   PIN_ACL_PRO_UART_CTS  
#define RTS_PIN   PIN_ACL_PRO_UART_RTS  

#define INVALID_DATA          0xFFFFFFFF
#define UART_INITIAL_BAUD     UART_BAUDRATE_BAUDRATE_Baud1200
#define UART_INITIAL_PARITY   false      

/**************************************
* Functions
**************************************/
/**@brief  Function for initializing the UART module.
 */
/**@snippet [UART Initialization] */
void uart_management_init(void);

/**************************************************
* Function name	: void uart_management_stop(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/24
* Description		: stops UART module
* Notes			    : restrictions, odd modes
**************************************************/
void uart_management_stop(void);

/**************************************************
* Function name	: void uart_management_new_baud(uint32_t new_baud)

*    returns		: ----
*    args			  : uint32_t new baudrate
* Created by		: María Viqueira
* Date created	: 2018/07/09
* Description		: changes UART baudrate to new_baud
* Notes			    : restrictions, odd modes
**************************************************/
void uart_management_new_baud(uint32_t new_baud);

/**************************************************
* Function name	: bool uart_management_new_parity(bool new_parity)

*    returns		: ----
*    args			  : bool new parity
* Created by		: María Viqueira
* Date created	: 2018/07/09
* Description		: changes UART parity to new_baud
* Notes			    : restrictions, odd modes
**************************************************/
void uart_management_new_parity(bool new_parity);

/**************************************************
* Function name	: bool uart_management_get_parity(void)

*    returns		: bool: uart parity. 
*                       True: even parity
*                       False: no parity
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/07/09
* Description		: returns the physical UART parity
* Notes			    : restrictions, odd modes
**************************************************/
bool uart_management_get_parity(void);

/**************************************************
* Function name	: uint32_t uart_management_code2num(uint32_t  code_value)
*    returns		: uint32_t: hexadecimal uart baudrate coded from Nordic format
*                           If data is not valid, returns 0xFFFFFFFF
*    args			  : uint32_t: uart baudrate coded as Nordic format
* Created by		: María Viqueira
* Date created	: 2018/07/09
* Description		: converts the value with Nordic format to real number
* Notes			    : restrictions, odd modes
**************************************************/
uint32_t uart_management_code2num(uint32_t  code_value);

/**************************************************
* Function name	: uint32_t uart_management_num2code(uint32_t  num_value)
*    returns		: uint32_t: uart baudrate coded as Nordic format
*                           If data is not valid, returns 0xFFFFFFFF
*    args			  : uint32_t: hexadecimal value
* Created by		: María Viqueira
* Date created	: 2018/07/09
* Description		: converts the hexadecimal value received to
*                 a format understood by Nordic.
* Notes			    : restrictions, odd modes
**************************************************/
uint32_t uart_management_num2code(uint32_t  num_value);

/**************************************************
* Function name	: uint32_t uart_management_get_baudrate(void)

*    returns		: uint32_t: uart baudrate
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/07/09
* Description		: returns the physical UART baudrate
* Notes			    : restrictions, odd modes
**************************************************/
uint32_t uart_management_get_baudrate(void);

#endif //UART_MANAGEMENT_H__
