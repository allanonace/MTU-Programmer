/***************************************************
* Module name: data_management.c
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
* Module for controlling the data exchange between
* Virtual UART and physical UART
***************************************************/

/**************************************************
* 02 01 05 25 80 00 FF 5C
*  Hi, I'm Aclara: 48692c2049276d2041636c617261
* Encrypt:07a6271209736449361a31468bc89c98
* All data: 02010539680f8d1aa93e1660a4f569730fad9100
* One data: 020105c656e115d5baf089d3f76c54ef188ca700
* Pass: 96a3be3cf272e017046d1b2674a52bd396a3be3cf272e017046d1b2674a52bd3
**************************************************/


/*  Include section
* Add all #includes here
*
***************************************************/
#include <string.h>
#include "app_uart.h"
#include "nrf_delay.h"
#include "app_timer.h"
#include "uart_management.h"
#include "data_management.h"
#include "ble_uart_service.h"
#include "ble_uart_char_TX.h"
#include "ble_uart_char_RX.h"
#include "encryption_management.h"
/*  Defines section
* Add all #defines here
*
***************************************************/
#define   LEN_ARRAY_MANAGEMENT  1200 
#define   LEN_ARRAY_AES_LONG    16  
#define   LEN_ARRAY_AES_SHORT   4
#define   DATA_TIMER_INTERVAL APP_TIMER_TICKS(10)
#define   SIZE_DATA_BLOCK       16
APP_TIMER_DEF(m_data_timer_id);

/**************************************************
*  BLE protocol
**************************************************/
//Positions of BLE frame
typedef enum
{
  POS_BYTE_BLE_STX,
  POS_BYTE_BLE_NUM_FRAME,
  POS_BYTE_BLE_BLOCK_SIZE,
  POS_BYTE_BLE_DATA
  
}positions_ble_t;

//BLE errors
typedef enum
{
    BLE_ACK_NO_ERROR,
    BLE_ACK_ERROR_BLE,
  
}ble_ack_types_t;

typedef enum
{
    BLE_ERROR_TYPE_NON_CONSECUTIVE_SEQUENCE,
    BLE_ERROR_TYPE_NON_CONSECUTIVE_BLOCK_FRAME,
    BLE_ERROR_TYPE_INCORRECT_SIZE,
    BLE_ERROR_TYPE_TIMEOUT
  
}ble_error_types_t;

/*  Variables section
* Add all variables here
*
***************************************************/
/**************************************************
* Global variables
**************************************************/
extern ble_uart_t                                m_ble_uart;  /**< Structure to identify the Virtual UART Service. */
/**************************************************
* Local variables
**************************************************/
//Arrays
uint8_t   data_array_from_uart[LEN_ARRAY_MANAGEMENT];
uint8_t   data_array_from_ble [LEN_ARRAY_MANAGEMENT];
uint8_t   data_to_ble[BLE_UART_MAX_RX_CHAR_LEN];

//Pointers
uint16_t  index_array_from_ble_output   = 0;  
uint16_t  index_array_from_ble_input          = 0; 

uint16_t  index_array_from_uart_input     = 0;
uint16_t  index_array_from_uart_output    = 0;
uint8_t   counter_frame_from_ble          = 0;

bool  frame_dfu           = false;
bool  FLAG_TIMER_DATA     = false;
bool  data_timer_expired  = false;
 
/*  Function Prototype Section
* Add prototypes for all functions called by this
* module, with the exception of runtime routines.
*
***************************************************/ 

/**************************************************
* Function name	: static uint32_t send_data_to_ble(uint8_t *p_data, uint16_t length)
*    returns		: NRF_SUCCESS if no error, otherwise: error code
*    args			  : uint8_t *p_data: pointer to data to be sent
*               : uint16_t length: number of bytes to send
* Created by		: María Viqueira
* Date created	: 2018/05/18
* Description		: Sends data to Virtual UART
* Notes			: restrictions, odd modes
**************************************************/
static uint32_t send_data_to_ble(uint8_t *p_data, uint16_t length)
{
    static uint32_t err_code;
  
    err_code = ble_uart_string_send(&m_ble_uart, p_data, length);
    return err_code;
}

/**************************************************
* Function name	: static uint32_t send_data_to_uart(uint8_t *p_data, uint16_t length)
*    returns		: ---
*    args			  : uint8_t *p_data: pointer to data to be sent
*               : uint16_t length: number of bytes to send
* Created by		: María Viqueira
* Date created	: 2018/05/18
* Description		: Sends data to Physical UART
* Notes			: restrictions, odd modes
**************************************************/
static void send_data_to_uart(uint8_t *p_data, uint16_t length)
{
    for (uint32_t i = 0; i < length; i++)
    {
        while (app_uart_put(p_data[i]) != NRF_SUCCESS);
    }

}
/***************************
*           BLE
***************************/
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
void data_management_ble_received(void)
{
    uint8_t actual_data[BLE_UART_MAX_RX_CHAR_LEN];  
    uint8_t num_bytes_received  = 0;
  
    num_bytes_received  = get_rx_ble_buff(actual_data);
    
    for(uint8_t i=0;i<num_bytes_received;i++)
    {
        data_array_from_ble[index_array_from_ble_input] = actual_data[i];
        index_array_from_ble_input++;
    }
   
}
//static void data_management_reset_ble_to_uart_data(void)
//{
//    
//}

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
void data_management_ble_decode(void)
{
    uint8_t ble_data_frame[SIZE_DATA_BLOCK]; 
    uint8_t frame_decoded[SIZE_DATA_BLOCK];
    uint8_t ble_data_frame_ack[BLE_UART_MAX_RX_CHAR_LEN];
    uint8_t current_counter_frame;
  
    memset(ble_data_frame_ack,0,sizeof(ble_data_frame_ack));
    memset(ble_data_frame,0,sizeof(ble_data_frame));
  
    
    //1. Frame decode acording to procol
    for(uint8_t i=0;i<SIZE_DATA_BLOCK;i++)
    {
        ble_data_frame[i] = data_array_from_ble[i+POS_BYTE_BLE_DATA];
    }
    
    current_counter_frame = data_array_from_ble[POS_BYTE_BLE_NUM_FRAME];
    ble_data_frame_ack[POS_BYTE_BLE_NUM_FRAME]  = current_counter_frame;
    
    if(current_counter_frame  !=  (counter_frame_from_ble + 1))
    {
        //Error
        ble_data_frame_ack [POS_BYTE_BLE_DATA]  = BLE_ACK_ERROR_BLE;
        ble_data_frame_ack[POS_BYTE_BLE_DATA+1] = BLE_ERROR_TYPE_NON_CONSECUTIVE_SEQUENCE;
    }
    else
    {
        //No error
        ble_data_frame_ack[POS_BYTE_BLE_DATA] = BLE_ACK_NO_ERROR;
    }
    
    //Counter increment
    counter_frame_from_ble = current_counter_frame;
    if(counter_frame_from_ble  ==  0xFF)
    {
        counter_frame_from_ble = 0;
    }
    
    ble_uart_ack_nack_send(&m_ble_uart,ble_data_frame_ack,BLE_UART_MAX_RX_CHAR_LEN);
    
    //If there was an error, we don't continue
    if(ble_data_frame_ack[POS_BYTE_BLE_DATA]  ==  BLE_ACK_ERROR_BLE)
    {
//        //Clean buffer and reset
//        index_array_from_ble_output = 0;
//        index_array_from_ble_input  = 0;
//        data_management_reset_ble_to_uart_data();
        index_array_from_ble_input  = 0;
        return;
    }
    
    //2. Data decrypt      
    encryption_management_decrypt_aes((unsigned char*)ble_data_frame,(unsigned char*)frame_decoded,SIZE_DATA_BLOCK);
    
    send_data_to_uart(frame_decoded,data_array_from_ble[POS_BYTE_BLE_BLOCK_SIZE]);  
    index_array_from_ble_input  = 0;
 
  
    
}

/**************************************************
* Function name	: uint16_t  data_management_ble_get_num_data(void)
*    returns		: uint16_t number of BLE data
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/18
* Description		: Returns the number of no decoded data from BLE
* Notes			    : restrictions, odd modes
**************************************************/
uint16_t  data_management_ble_get_num_data(void)
{
    return index_array_from_ble_input;
}  

/***************************
*           UART
***************************/
/**************************************************
* Function name	: void data_management_uart_received(void)
*    returns		: ---
*    args			  : ----
* Created by		: María Viqueira
* Date created	: 2018/05/18
* Description		: Physical UART intteruption
* Notes			    : restrictions, odd modes
**************************************************/
void data_management_uart_received(void)
{
    UNUSED_VARIABLE(app_uart_get(&data_array_from_uart[index_array_from_uart_input]));
  
   
    index_array_from_uart_input++;
  
    if(index_array_from_uart_input>=LEN_ARRAY_MANAGEMENT)
    {
        //Overflow
        index_array_from_uart_input = 0;
    }
    
    
  
}

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
uint32_t data_management_uart_decode(void)
{
    uint16_t  number_real_data  = 0;
    uint16_t  index_input_aux = 0;
    uint16_t   i,j;
    uint32_t  err_code  = NRF_SUCCESS;    
    uint8_t   number_data_actual_ble_frame  = 0; //Data that will be sent in the BLE frame
    
    bool      overflow_input    = false;
  
  
    //We need a fixed input pointer because index_array_from_uart_input will be incremented during this function
    index_input_aux = index_array_from_uart_input;
  
    //First, we need to calculate how many bytes we have  
    if(index_array_from_uart_output>index_input_aux)
    {
        //Input arrived at the end and now is at the beginning
        number_real_data  = LEN_ARRAY_MANAGEMENT  - index_array_from_uart_output  + index_input_aux;
        overflow_input    = true;
    }
    else
    {
        number_real_data  = index_input_aux - index_array_from_uart_output;
        
    }
    
    
    /*************************************************************************************************
    * Now, we have to check whether the overflow affects to our sending:
    * For example: buffer length: 1200
    * index_array_from_uart_input: 5
    * index_array_from_uart_output: 1170
    * There are actually 35 bytes which have not been sent yet, but we are going to send only 20 bytes
    * in this cycle, so 1170+20 = 1190. There is not overflow yet
    *************************************************************************************************/
    if(overflow_input)
    {
        if( (number_real_data + index_array_from_uart_output)<LEN_ARRAY_MANAGEMENT)
        {
            //Overflow doesn't happen in this cycle
            overflow_input  = false;
        }
    }
     
    
    //Check timer to now if we have to send data
    if(data_timer_expired)
    {
        
        data_timer_expired  = false;
        memset(data_to_ble,0,sizeof(data_to_ble));
      
        //1. We need to check whether there are more data than the allowed by BLE protocol
        if(number_real_data>SIZE_DATA_BLOCK)
        {
            number_data_actual_ble_frame  = SIZE_DATA_BLOCK;
        }
        else
        {
            number_data_actual_ble_frame  = (uint8_t)number_real_data;
        }
         
        //2. Copy the data pointed by index_array_from_uart_output
        data_to_ble[POS_BYTE_BLE_BLOCK_SIZE]  = number_data_actual_ble_frame;
        if(!overflow_input)
        {    
            //Bytes are copied directly from input array to output            
            for(i=0;i<number_data_actual_ble_frame;i++)
            {
                data_to_ble[i+POS_BYTE_BLE_DATA]  = data_array_from_uart[index_array_from_uart_output+i];              
            }
                   
            
        }
        else
        {
            //We have data at the end and at the begining of the array. 
            //First, we need to copy the last bytes of the array and then, the first ones                    
            for(i=0;i<(LEN_ARRAY_MANAGEMENT - index_array_from_uart_output);i++)
            {
                data_to_ble[i+POS_BYTE_BLE_DATA]  = data_array_from_uart[index_array_from_uart_output+i];
            }
            
            //i++;
                        
            for(j=0;j<=(number_data_actual_ble_frame - i - 1);j++)
            {
                data_to_ble[i+j+POS_BYTE_BLE_DATA]  = data_array_from_uart[j];
            }
            
        }
        
        //Encryption
        encryption_management_encrypt_aes(data_to_ble+POS_BYTE_BLE_DATA,data_to_ble+POS_BYTE_BLE_DATA,SIZE_DATA_BLOCK);        
 
        err_code  = send_data_to_ble(data_to_ble,BLE_UART_MAX_DATA_LEN);
        
        if(err_code ==  NRF_SUCCESS)
        {
            //Calculate new position of ouput
            if(overflow_input)
            {
                index_array_from_uart_output  = j;
            }
            else
            {
                index_array_from_uart_output  = index_array_from_uart_output  + number_data_actual_ble_frame;
              
                //We need to check whether next position is higher than LEN_ARRAY_MANAGEMENT           
                if( index_array_from_uart_output  >=  LEN_ARRAY_MANAGEMENT  )
                {
                      index_array_from_uart_output  = index_array_from_uart_output  - LEN_ARRAY_MANAGEMENT;
                }
                
            }
         }
        
    }
    return err_code;
}

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
uint16_t  data_management_uart_get_num_data(void)
{
    uint16_t number_real_data;
  
    if(index_array_from_uart_output>index_array_from_uart_input)
    {
        //Input arrived at the end and now is at the beginning
        number_real_data  = index_array_from_uart_input - (LEN_ARRAY_MANAGEMENT - index_array_from_uart_output); 
    }
    else
    {
        number_real_data  = index_array_from_uart_input - index_array_from_uart_output;
        
    }
    return number_real_data;
}  

/***************************
*           COMMON
***************************/
static void data_meas_timeout_handler(void * p_context)
{
    data_timer_expired  = true;
}
/**************************************************
* Function name	: void data_management_timer_start(void)
*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/28
* Description		: Timer for sending data to BLE, on
* Notes			: restrictions, odd modes
**************************************************/
void data_management_timer_start(void)
{
    if(!FLAG_TIMER_DATA)
    {
      app_timer_start(m_data_timer_id,DATA_TIMER_INTERVAL,NULL);
      FLAG_TIMER_DATA = true;
    }
   data_timer_expired = false;
}
/**************************************************
* Function name	: void data_management_timer_stop(void)
*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/28
* Description		: Timer for sending data to BLE, off
* Notes			: restrictions, odd modes
**************************************************/
void data_management_timer_stop(void)
{
    if(FLAG_TIMER_DATA)
    {
        app_timer_stop(m_data_timer_id);
        FLAG_TIMER_DATA = false;
    }
    data_timer_expired = false;
}
/**************************************************
* Function name	: void data_management_init(void)
*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/18
* Description		: Data management inicialization
* Notes			: restrictions, odd modes
**************************************************/
void data_management_init(void)
{
  
    memset(data_array_from_ble,0,sizeof(data_array_from_ble));
    memset(data_array_from_uart,0,sizeof(data_array_from_uart));
    index_array_from_ble_input    = 0;
    index_array_from_uart_input   = 0;
    index_array_from_uart_output  = 0;
    counter_frame_from_ble        = 0;
  
   
}

/**************************************************
* Function name	: void data_management_init(void)
*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/06/28
* Description		: Creates timer for BLE sending
* Notes			: restrictions, odd modes
**************************************************/
void data_management_create_timer(void)
{
        static uint32_t err_code;

     //Timer for sending data to BLE
    err_code  = app_timer_create(&m_data_timer_id,APP_TIMER_MODE_REPEATED,data_meas_timeout_handler);
    APP_ERROR_CHECK(err_code);
}
//void prueba_data_management(void)
//{
//    uint8_t hola_mundo_hex[20]={0x48,0x6f,0x6c,0x61,0x20,0x6d,0x75,0x6e,0x64,0x6f,0x20,0x31,0x32,0x33,0x34,0x35,0x36,0x37,0x38,0x39};
//    uint8_t cadena_decodificada[20];  
//    unsigned char cadena_16[LEN_ARRAY_AES_LONG]="",cadena_4[LEN_ARRAY_AES_SHORT];    
//    strncpy((char*)data_to_ble,(char*)hola_mundo_hex,20);
//    
//    encryption_management_encrypt_aes((unsigned char*)data_to_ble,cadena_16,LEN_ARRAY_AES_LONG);        
//    encryption_management_encrypt_aes((unsigned char*)data_to_ble+LEN_ARRAY_AES_LONG,cadena_4,LEN_ARRAY_AES_SHORT);
//    
//        
//    strncpy((char*)data_to_ble,(char*)cadena_16,LEN_ARRAY_AES_LONG);  
//    strncpy((char*)data_to_ble+LEN_ARRAY_AES_LONG,(char*)cadena_4,LEN_ARRAY_AES_SHORT);  
//      
//    //Decryption
//    encryption_management_decrypt_aes(data_to_ble,cadena_decodificada,LEN_ARRAY_AES_LONG); 
//    encryption_management_decrypt_aes(data_to_ble+LEN_ARRAY_AES_LONG,cadena_decodificada+LEN_ARRAY_AES_LONG,LEN_ARRAY_AES_SHORT);   
//        
//      
//}
//void prueba_uart(void)
//{
//  nrf_delay_ms(1000);
//  //static uint8_t cadena_prueba[10]={0x25,0x80,0x00,0x01,0x5a};
//  static uint8_t cadena_prueba[10]={0x25,0x80,0x00,0xFF,0x5C};
//  //uint8_t cadena_prueba[10]={0,0,0,0,0};
//  //static uint8_t cadena_prueba[1];
//    //send_data_to_uart(cadena_prueba,5);
//  static uint8_t contador=0;
//   //send_data_to_uart(cadena_prueba,5);
//  while(1)
//  {
////    contador=0x54;
////    contador++;
////    cadena_prueba[0]  = contador;
////    send_data_to_uart(cadena_prueba,1);
//      send_data_to_uart(cadena_prueba,5);
//    nrf_delay_ms(3000);
//  }
//}
