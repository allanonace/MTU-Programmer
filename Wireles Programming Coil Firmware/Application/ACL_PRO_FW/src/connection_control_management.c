/***************************************************
* Module name: connection_control_management.c
*
* Copyright Aclara Technologies, LLC 
* All Rights Reserved.
*
* The information contained herein is confidential 
* property of Aclara Technologies. The user, copying, transfer
* or disclosure of such information is prohibited except
* by express written agreement with Aclara Technologies.
*
* First written on 2018/07/16 by María Viqueira.
*
* RevX on xxxxxx by xxxxxx
*	- Added xxxxx fucntion
*	- Improve xxxxxxxx
*
*
* Module Description:
* Module for managing operations related with the connection control
*
***************************************************/

/*  Include section
* Add all #includes here
*
***************************************************/
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include "app_timer.h"
#include "nrf_power.h"

#include "connection_control_management.h"
#include "ble_control_service_char_frame.h"
#include "encryption_management.h"
#include "defines_tests.h"


/*  Defines section
* Add all #defines here
*
***************************************************/
#define FRAME_HELLO              "Hi, I'm Aclara"
#define CONNECTION_TIMEOUT_MS    (60*1000)

APP_TIMER_DEF(m_connection_timer_id);

/*  Variables section
* Add all variables here
*
***************************************************/
char  frame_to_decode[BLE_CONTROL_MAX_FRAME_CHAR_LEN];
bool  connection_valid          = false;
bool  FLAG_CONNECTION_TIMER     = false;
bool  connection_timer_expired  = false;  
bool  dynamic_key_created       = false;

uint8_t number_paired_times = 0;
  
/*  Function Prototype Section
* Add prototypes for all functions called by this
* module, with the exception of runtime routines.
*
***************************************************/  

/**************************************************
* Function name	: static bool connection_control_management_check_hello(char *command)
*    returns		: ---
*    arg 1			: char* pointer to the frame to compare
* Created by		: María Viqueira
* Date created	: 2018/07/23
* Description		: Compares the frame with the Hello frame
* Notes			    : restrictions, odd modes
**************************************************/ 
static bool connection_control_management_check_hello(char *command)
{
    char *pointer;
    pointer = strstr(command,FRAME_HELLO);
  
    if(pointer  ==  0)
    {
        return false;
    }
    
    return true;
  
} 
/**************************************************
* Function name	: static void timer_connection_timeout_handler(void * p_context)
*    returns		: ----
*    args			  : unused pointer
* Created by		: María Viqueira
* Date created	: 2018/07/23
* Description		: Interruption of the timer which controls
*                 the wait of Hello frame
* Notes			: restrictions, odd modes
**************************************************/
static void timer_connection_timeout_handler(void * p_context)
{
    UNUSED_PARAMETER(p_context);
    connection_timer_expired  = true;
}

/**************************************************
* Function name	: void connection_control_management_timer_attended(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/23
* Description		: resets the indicator of connection control expired 
* Notes			    : restrictions, odd modes
**************************************************/ 
void connection_control_management_timer_attended(void)
{
    connection_timer_expired  = false;
}

/**************************************************
* Function name	: bool connection_control_management_get_timer_expired(void)
*    returns		: bool with the status of timer of control. True if timer is expired
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/23
* Description		: returns the status of the timer of the connection control
* Notes			    : restrictions, odd modes
**************************************************/ 
bool connection_control_management_get_timer_expired(void)
{
    return connection_timer_expired;
}

/**************************************************
* Function name	: void connection_control_management_timer_stop(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/23
* Description		: stops the timer of the connection control  
* Notes			    : restrictions, odd modes
**************************************************/ 
void connection_control_management_timer_stop(void)
{
    static uint32_t err_code;
    if(FLAG_CONNECTION_TIMER)
    {
      err_code  = app_timer_stop(m_connection_timer_id);
      APP_ERROR_CHECK(err_code);
      FLAG_CONNECTION_TIMER = false;  
    }
    connection_timer_expired  = false;  
}


/**************************************************
* Function name	: void connection_control_management_dynamic_pass_was_created(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/17
* Description		: indicates dynamic pass was created 
* Notes			    : restrictions, odd modes
**************************************************/  
void connection_control_management_dynamic_pass_was_created(void)
{
    dynamic_key_created = true;
}

/**************************************************
* Function name	: void connection_control_management_timer_start(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/23
* Description		: starts the timer of the connection control  
* Notes			    : restrictions, odd modes
**************************************************/ 
void connection_control_management_timer_start(void)
{
    uint32_t err_code;
    if(!FLAG_CONNECTION_TIMER)
    {
      err_code = app_timer_start(m_connection_timer_id,APP_TIMER_TICKS(CONNECTION_TIMEOUT_MS),NULL); 
      APP_ERROR_CHECK(err_code);
      FLAG_CONNECTION_TIMER = true;
    }
    connection_timer_expired  = false;  
}

/**************************************************
* Function name	: void connection_control_management_create_timer(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/23
* Description		: creates the timer of the connection control  
* Notes			    : restrictions, odd modes
**************************************************/  
void connection_control_management_create_timer(void)
{
    uint32_t err_code;
    err_code = app_timer_create(&m_connection_timer_id, APP_TIMER_MODE_SINGLE_SHOT, timer_connection_timeout_handler);
    APP_ERROR_CHECK(err_code);
}
/**************************************************
* Function name	: void connection_control_management_decode_frame_hello(uint8_t *data_rec)
*    returns		: ---
*    arg 1			: pointer to the frame to check
* Created by		: María Viqueira
* Date created	: 2018/07/17
* Description		: checks whether the received frame is correct or not  
* Notes			    : restrictions, odd modes
**************************************************/  
void connection_control_management_decode_frame_hello(uint8_t *data_rec)
{
    uint8_t frame_decrypted[BLE_CONTROL_MAX_FRAME_CHAR_LEN];
  
    //1. Check if we generated any password
    if(!dynamic_key_created)
    {
        //First time, never paired
        connection_valid  = false;
        return;
    }
    
    //2. Decrypt
    encryption_management_decrypt_aes(data_rec,frame_decrypted,AES_BYTES/2);
     for(uint8_t i=0;i<BLE_CONTROL_MAX_FRAME_CHAR_LEN;i++)
     {
       frame_to_decode[i]  = frame_decrypted[i];
     }
//    memset(frame_to_decode,0,sizeof(frame_to_decode));
//    sprintf(frame_to_decode,"%s",frame_decrypted);
  
    //3. Check frame
    connection_valid = connection_control_management_check_hello(frame_to_decode);   

    
}

/**************************************************
* Function name	: bool connection_control_management_get_connection_valid(void)
*    returns		: bool: true if received frame is correct. Otherwise, false
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/17
* Description		: indicates the status of the received frame  
* Notes			    : restrictions, odd modes
**************************************************/  
bool connection_control_management_get_connection_valid(void)
{
    return connection_valid;
}

/**************************************************
* Function name	: void connection_control_management_reset_connection(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/17
* Description		: Sets flag to falses
* Notes			    : restrictions, odd modes
**************************************************/  
void connection_control_management_reset_connection(void)
{
    connection_valid  = false;
        
}

/**************************************************
* Function name	: void connection_control_management_update_number_paired_times(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/26
* Description		: increments number of paired times
* Notes			    : restrictions, odd modes
**************************************************/  
void connection_control_management_update_number_paired_times(void)
{
    if(number_paired_times ==  0xFF)
    {
        number_paired_times  = 0;
    }
    number_paired_times++;
    
    
    
}

/**************************************************
* Function name	: uint8_t connection_control_management_get_number_paired_times(void)
*    returns		: uint8_t number of pairings
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/25
* Description		: returns the number of paired times of the device
* Notes			    : restrictions, odd modes
**************************************************/ 
uint8_t connection_control_management_get_number_paired_times(void)
{
    return number_paired_times;
}

/**************************************************
* Function name	: void connection_control_management_init(void)
*    returns		: ---
*    arg 1			: ---
* Created by		: María Viqueira
* Date created	: 2018/07/25
* Description		: reads the GPREGRET for knowing the number or times
*                 the device was paired
* Notes			    : restrictions, odd modes
**************************************************/ 
void connection_control_management_init(void)
{
    number_paired_times = NRF_POWER->GPREGRET;
    if(number_paired_times  !=  0)
    {
        
        encryption_control_management_create_new_dynamic_pass(false);
        
    }
   
    
}

//void prueba_connection(void)
//{
//    static bool equal;
//  
//    equal = connection_control_management_check_hello("Hi, I'm Ojmar");
//}
