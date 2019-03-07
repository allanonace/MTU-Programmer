/***************************************************
* Module name: uart_management.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/05/18 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for controlling UART operations
*
***************************************************/

/*  Include section
* Add all #includes here
*
***************************************************/
#include "app_uart.h"
#include "uart_management.h"
#include "data_management.h"


/*  Defines section
* Add all #defines here
*
***************************************************/
#define UART_TX_BUF_SIZE                256  /**< UART TX buffer size. */
#define UART_RX_BUF_SIZE                256  /**< UART RX buffer size. */


/*  Variables section
* Add all variables here
*
***************************************************/
bool uart_status  = false; //To ensure whether UART is enabled or disabled (problems in some SDKs)
bool uart_parity  = UART_INITIAL_PARITY;

uint32_t  uart_baudrate = UART_INITIAL_BAUD;


/**@brief   Function for handling app_uart events.
 *
 * @details This function will receive a single character from the app_uart module and append it to
 *          a string. The string will be be sent over BLE when the last character received was a
 *          'new line' i.e '\r\n' (hex 0x0D) or if the string has reached a length of
 *          @ref NUS_MAX_DATA_LENGTH.
 */
/**@snippet [Handling the data received over UART] */
   

void uart_event_handle(app_uart_evt_t * p_event)
{

    switch (p_event->evt_type)
    {
        case APP_UART_DATA_READY:
            data_management_uart_received();
        break;

        case APP_UART_COMMUNICATION_ERROR:
            //APP_ERROR_HANDLER(p_event->data.error_communication);
            break;

        case APP_UART_FIFO_ERROR:
            //APP_ERROR_HANDLER(p_event->data.error_code);
            break;
        case  APP_UART_TX_EMPTY:
          //
        break;
        case  APP_UART_DATA:
//            UNUSED_VARIABLE(app_uart_get(&data_array[index]));
//            index++;
        break;
        
        
        default:
            break;
    }
}

/**@brief  Function for initializing the UART module.
 */
/**@snippet [UART Initialization] */
void uart_management_init(void)
{
    uint32_t                     err_code;
    const app_uart_comm_params_t comm_params =
    {
        RX_PIN,
        TX_PIN,
        RTS_PIN,
        CTS_PIN,
        APP_UART_FLOW_CONTROL_DISABLED,
        uart_parity,
        uart_baudrate  
        
    };

    if(!uart_status)
    {
        APP_UART_FIFO_INIT( &comm_params,
                           UART_RX_BUF_SIZE,
                           UART_TX_BUF_SIZE,
                           uart_event_handle,
                           APP_IRQ_PRIORITY_LOWEST,
                           err_code);
        APP_ERROR_CHECK(err_code);
      
        uart_status = true;
    }
}

/**************************************************
* Function name	: void uart_management_stop(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/24
* Description		: stops UART module
* Notes			    : restrictions, odd modes
**************************************************/
void uart_management_stop(void)
{
    if(uart_status)
    {
        app_uart_close();
        uart_status = false;
    }
}


/**************************************************
* Function name	: void uart_management_new_baud(uint32_t new_baud)

*    returns		: ----
*    args			  : uint32_t new baudrate
* Created by		: María Viqueira
* Date created	: 2018/07/09
* Description		: changes UART baudrate to new_baud
* Notes			    : restrictions, odd modes
**************************************************/
void uart_management_new_baud(uint32_t new_baud)
{
    uart_management_stop();
    uart_baudrate = new_baud;
    uart_management_init();
    
}

/**************************************************
* Function name	: uint32_t uart_management_get_baudrate(void)

*    returns		: uint32_t: uart baudrate
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/07/09
* Description		: returns the physical UART baudrate
* Notes			    : restrictions, odd modes
**************************************************/
uint32_t uart_management_get_baudrate(void)
{
    return uart_baudrate;
}


/**************************************************
* Function name	: bool uart_management_new_parity(bool new_parity)

*    returns		: ----
*    args			  : bool new parity
* Created by		: María Viqueira
* Date created	: 2018/07/09
* Description		: changes UART parity to new_baud
* Notes			    : restrictions, odd modes
**************************************************/
void uart_management_new_parity(bool new_parity)
{
    uart_management_stop();
    uart_parity = new_parity;
    uart_management_init();
    
}

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
bool uart_management_get_parity(void)
{
    return uart_parity;
}


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
uint32_t uart_management_num2code(uint32_t  num_value)
{
    uint32_t value_coded  = 0;
    switch(num_value)
    {
      case 1200:  
        
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud1200;
      break;
      
      case 2400:  
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud2400;
      break;
      
      case 4800:  
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud4800;
      break;
      
      case 9600:  
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud9600;
      break;
      
      case 14400:  
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud14400;
      break;
      
      case 19200:  
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud19200;
      break;
      
      case 28800:  
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud28800;
      break;
      
      case 31250:  
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud31250;
      break;
      
      case 38400:  
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud38400;
      break;
      
      case 56000:  
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud56000;
      break;
      
      case 57600:  
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud57600;
      break;
      
      case 76800:  
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud76800;
      break;
      
      case 115200:  
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud115200;
      break;
      
      case 230400:  
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud230400;
      break;
      
      case 250000:  
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud250000;
      break;
      
      case 460800:  
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud460800;
      break;
      
      case 921600:  
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud921600;
      break;
      
      case 1000000:  
        value_coded = UARTE_BAUDRATE_BAUDRATE_Baud1M;
      break;
      
      default:
        //Invalid data
        value_coded = INVALID_DATA;
      break;
    }
    
    return value_coded;
}


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
uint32_t uart_management_code2num(uint32_t  code_value)
{
  uint32_t value_num;
  switch(code_value)
    {
      case UARTE_BAUDRATE_BAUDRATE_Baud1200:
        value_num = 1200;
      break;
      case UARTE_BAUDRATE_BAUDRATE_Baud2400:
        value_num = 2400;
      break;
      case UARTE_BAUDRATE_BAUDRATE_Baud4800:
        value_num = 4800;
      break;
      case UARTE_BAUDRATE_BAUDRATE_Baud9600:
        value_num = 9600;
      break;
      case UARTE_BAUDRATE_BAUDRATE_Baud14400:
        value_num = 4400;
      break;
      case UARTE_BAUDRATE_BAUDRATE_Baud19200:
        value_num = 19200;
      break;
      case UARTE_BAUDRATE_BAUDRATE_Baud28800:
        value_num = 28800;
      break;
      case UARTE_BAUDRATE_BAUDRATE_Baud31250:
        value_num = 31250;
      break;
      case UARTE_BAUDRATE_BAUDRATE_Baud38400:
        value_num = 38400;
      break;
      case UARTE_BAUDRATE_BAUDRATE_Baud56000:
        value_num = 56000;
      break;
      case UARTE_BAUDRATE_BAUDRATE_Baud57600:
        value_num = 57600;
      break;
      case UARTE_BAUDRATE_BAUDRATE_Baud76800:
        value_num = 76800;
      break;
      case UARTE_BAUDRATE_BAUDRATE_Baud115200:
        value_num = 115200;
      break;
      case UARTE_BAUDRATE_BAUDRATE_Baud230400:
        value_num = 230400;
      break;
      case UARTE_BAUDRATE_BAUDRATE_Baud250000:
        value_num = 250000;
      break;
      case UARTE_BAUDRATE_BAUDRATE_Baud460800:
        value_num = 460800;
      break;
      case UARTE_BAUDRATE_BAUDRATE_Baud921600:
        value_num = 921600;
      break;
      case UARTE_BAUDRATE_BAUDRATE_Baud1M:
        value_num = 1000000;
      break;
      
      default:
        //Invalid data
        value_num = INVALID_DATA;
      break;
      
      
      
    }
    return value_num;
}
