/***************************************************
* Module name: sate_machine_management.c
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
* Module for controlling the general state machine
***************************************************/


/*  Include section
* Add all #includes here
*
***************************************************/
#include <stdint.h>
#include "state_machine_management.h"
#include "bluetooth_management.h"
#include "data_management.h"
#include "gpio_management.h"
#include "uart_management.h"
#include "timers_management.h"
#include "button_management.h"
#include "battery_management.h"
#include "leds_management.h"
#include "battery_management.h"
#include "adc_batt_management.h"
#include "defines_tests.h"
#include "connection_control_management.h"
#include "encryption_management.h"
#include  "nrf_gpio.h"

/*  Variables section
* Add all variables here
*
***************************************************/
uint8_t general_status  = STATE_INIT_WAIT_CONNECTION;

/**************************************************
* Function name	: void state_machine_trans(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/28
* Description		: Functions when device is disconnected
* Notes			    : restrictions, odd modes
**************************************************/
static void device_disconnected(void)
{
    bluetooth_management_stop_connection();
    uart_management_stop();
    nrf_gpio_pin_clear(PIN_ACL_UART_EN);
    nrf_gpio_pin_clear(PIN_ACL_3V3_ADP);
}
/**************************************************
* Function name	: void state_machine_trans(void)

*    returns		: ----
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/23
* Description		: Controls the transactions of the
*                 state machine
* Notes			    : restrictions, odd modes
**************************************************/
void state_machine_trans(void)
{
    switch(general_status)
    {
        case STATE_INIT_WAIT_CONNECTION:
          if(battery_management_is_usb_connected())
          {
              general_status  = STATE_RESET;
          }
          else if(battery_management_get_level()  ==  0)
          {
              general_status  = STATE_SLEEP_PREPARE;
          }
          else if(button_management_status() ==  BUTTON_STATUS_RELEASE)
          {
              switch(button_management_type_push())
              {
                  case  PUSH_TYPE_SHORT:
                    //Not changing
                    
                  break;
                  
                  case  PUSH_TYPE_MEDIUM:
                      general_status  = STATE_SLEEP_PREPARE;
                  break;
                  
                  case  PUSH_TYPE_LONG:
                    //Pairing, only if no low battery
                    if(!battery_management_is_low_battery())
                    {
                      general_status  = STATE_ON_TO_PAIRING;
                    }
                  break;
              }
              //Complete button pulse was attended
              button_managment_attended();
          }
          else if (bluetooth_management_get_status_connected())
          {
              general_status  = STATE_INIT_CONNECTED;
          }
          else if(timers_management_get_on_timeout_status())
          {
              general_status  = STATE_SLEEP_PREPARE;
          }
         
        break;
          
        case STATE_INIT_CONNECTED:
          general_status  = STATE_INIT_WAIT_PRESENTATION;    
        break;
        
        case STATE_INIT_WAIT_PRESENTATION:
          if(battery_management_get_level()  ==  0)
          {
              general_status  = STATE_BLE_PERIPHERAL_DISCONNECTED;
          }
          else if(!bluetooth_management_get_status_connected())
          {
              general_status  = STATE_INIT_CONTROL_FINISHED;
              
          }
          else if(button_management_status() ==  BUTTON_STATUS_RELEASE)
          {
              switch(button_management_type_push())
              {
                  case  PUSH_TYPE_SHORT:
                    //Not changing
                    
                  break;
                  
                  case  PUSH_TYPE_MEDIUM:                   
                      general_status  = STATE_SLEEP_PREPARE;
                  break;
                  
                  case  PUSH_TYPE_LONG:
                    //Pairing
                    if(!battery_management_is_low_battery())
                    {
                      general_status  = STATE_ON_TO_PAIRING_DISCONNECT;
                    }
                  break;
              }
              //Complete button pulse was attended
              button_managment_attended();
          }
          else if(connection_control_management_get_connection_valid())
          {
            general_status  = STATE_INIT_PRESENTATION_OK;
          }
          else if(connection_control_management_get_timer_expired())
          {
              general_status  = STATE_INIT_CONTROL_FINISHED;
              connection_control_management_timer_attended();
          }
        break;
        
        case STATE_INIT_PRESENTATION_OK:
          general_status  = STATE_ON;
        break;  
        
        case STATE_INIT_CONTROL_FINISHED:
          general_status  = STATE_INIT_WAIT_CONNECTION;
        break;
        
        case STATE_ON_TO_PAIRING_DISCONNECT:
          general_status  = STATE_ON_TO_PAIRING_WAT_DISCONNECT;  
          
        break;
        
        case STATE_ON_TO_PAIRING_WAT_DISCONNECT:
          if(!bluetooth_management_get_status_connected())
          {
              general_status  = STATE_ON_TO_PAIRING;
          }
        break;
        
        case STATE_ON:
          if((battery_management_get_level()  ==  0) ) 
          {
              if(bluetooth_management_get_status_connected())
              {
                general_status  = STATE_BLE_PERIPHERAL_DISCONNECTED;
              }
              else
              {
                  general_status  = STATE_SLEEP_PREPARE;
              }
          }
          else if(battery_management_is_usb_connected())
          {
              if(bluetooth_management_get_status_connected())
              {
                general_status  = STATE_RESET_DISCONNECT;
              }
              else
              {
                  general_status  = STATE_SLEEP_PREPARE;
              }
          }
          else if(button_management_status() ==  BUTTON_STATUS_RELEASE)
          {
              switch(button_management_type_push())
              {
                  case  PUSH_TYPE_SHORT:
                    //Not changing
                    
                  break;
                  
                  case  PUSH_TYPE_MEDIUM:
                    //Sleep
                    #ifdef TEST_NO_SLEEP
                      general_status  = STATE_ON_TO_PAIRING;
                    #else
                      general_status  = STATE_SLEEP_PREPARE;
                    #endif
                  break;
                  
                  case  PUSH_TYPE_LONG:
                    //Pairing
                    general_status  = STATE_ON_TO_PAIRING_DISCONNECT;
                  break;
              }
              //Complete button pulse was attended
              button_managment_attended();
          }
          else if(!bluetooth_management_get_status_connected())
          {
              general_status  = STATE_INIT_CONTROL_FINISHED;
          }
          
        break;
          
        case STATE_ON_TO_PAIRING:
            general_status  = STATE_PAIRING;
        break;

        case STATE_PAIRING:
            if(button_management_status() ==  BUTTON_STATUS_RELEASE)
            {
              switch(button_management_type_push())
              {
                  
                  case  PUSH_TYPE_SHORT:
                      //Not changing
                     //Attended at state_machine_exe()
                  
                  break;
                  
                  case  PUSH_TYPE_MEDIUM:
                    //Sleep
                    #ifdef TEST_NO_SLEEP
                       general_status  = STATE_PAIRING_TO_ON;
                    #else
                      general_status  = STATE_SLEEP_PREPARE;
                    #endif
                  break;
                  
                  case  PUSH_TYPE_LONG:
                    //ON
                    general_status  = STATE_PAIRING_TO_ON;
                  break;
              }
              //Complete button pulse was attended
              button_managment_attended();
            }
            //else if(bluetooth_management_get_paired_success())
            else if(connection_control_management_get_connection_valid())
            {
                general_status  = STATE_PAIRED_SUCCESS;
            }
            else if(timers_management_get_pairing_timer_status())
            {
                
                  general_status  = STATE_PAIRING_TO_ON;
                  //timers_management_control_paired_timmer_attended();
            }
        break;
        
        case STATE_PAIRED_SUCCESS:     
         // general_status  = STATE_PAIRING_TO_ON;
        general_status  = STATE_INIT_PRESENTATION_OK;
        break;
        
        case STATE_PAIRING_TO_ON:    
          //general_status  = STATE_ON;
          general_status  = STATE_INIT_CONTROL_FINISHED;

        break;
        
        case STATE_BLE_MASTER_DISCONNECTED:
          general_status  = STATE_ON;
        break;
        
          
        case   STATE_BLE_PERIPHERAL_DISCONNECTED:
          general_status  = STATE_BLE_WAIT_DISCONNECTION;
        break;
        
        case   STATE_BLE_WAIT_DISCONNECTION:
          //Check disconnection
          if(!bluetooth_management_get_status_connected())
          {
              general_status  = STATE_BLE_DISCONNECTION_SUCCESS;
          }
        break;
          
        case   STATE_BLE_DISCONNECTION_SUCCESS:
           
          general_status  = STATE_SLEEP_PREPARE;
        break;
        
        case STATE_RESET_DISCONNECT:
          general_status  = STATE_RESET_WAIT_DISCONNECTION;
        break;
        
        case STATE_RESET_WAIT_DISCONNECTION:
          if(!bluetooth_management_get_status_connected())
          {
              general_status  = STATE_RESET;
          }
        break;
        
        case STATE_RESET:
            //
        break;
        
        case   STATE_SLEEP_PREPARE:
          //
        break;
    }
}

/**************************************************
* Function name	: void state_machine_trans(void)
*    returns		: bool to prevent main application
*                 from going to sleep in some cases
*    args			  : -----
* Created by		: María Viqueira
* Date created	: 2018/05/23
* Description		: Controls the actions of the
*                 state machine
* Notes			    : restrictions, odd modes
**************************************************/
bool state_machine_exe(void)
{
    bool sleep  = true;
  
    switch(general_status)
    {
        case STATE_INIT_WAIT_CONNECTION:
          //
        break;
        
        case STATE_INIT_CONNECTED:
//TODO: new presentation frame?
        timers_management_sec_stop();     
        connection_control_management_timer_start();
        sleep = false;
        break;
        
        case STATE_INIT_WAIT_PRESENTATION:
          //
        break;
        
        case STATE_INIT_PRESENTATION_OK:
          nrf_gpio_pin_set(PIN_ACL_UART_EN);
          nrf_gpio_pin_set(PIN_ACL_3V3_ADP);
          uart_management_init();
          //Timer for sending BLE data
          data_management_timer_start();
          connection_control_management_timer_stop();
          data_management_init();
          
        
          sleep = false;
        break;
        
        case STATE_INIT_CONTROL_FINISHED:
          //If connected, disconnect
          if(bluetooth_management_get_status_connected())
          {
              bluetooth_management_stop_connection();
          }
          //Stop timers, if working
          
          //Connection control timer
          connection_control_management_timer_stop();
          
          //Pairing timer
          timers_management_control_pairing_stop();
          
          //Uart to BLE timer
          data_management_timer_stop();            
            
          //Start timer general timeout
          timers_management_sec_start();
        
          //Hide password
          bluetooth_management_update_pass(false);
          
          sleep = false;
        break;
          
        case STATE_ON_TO_PAIRING_DISCONNECT:
          device_disconnected();
          sleep  = false;
        break;
        
        case STATE_ON_TO_PAIRING_WAT_DISCONNECT:
          //
        break;  
        
        case STATE_ON:
          
                 
          //1. Check BLE data
          if(data_management_ble_get_num_data()>0)  
          {
              data_management_ble_decode();
          }
          
          //2. Check UART data
           if(data_management_uart_get_num_data() >0)
           {
              data_management_uart_decode();
             
           }
           
           
        break;
           
        case STATE_ON_TO_PAIRING:
          
            
            //New password
            encryption_control_management_create_new_dynamic_pass(true);        
                   
            bluetooth_management_update_pass(true);
                    
            //Starting timeout for pairing
            timers_management_control_pairing_start();
        
            //Stop timer of connection control, if ON
            connection_control_management_timer_stop();
        
            //Stop timer activiy, if ON
            timers_management_sec_stop();
        
            sleep = false;
        
        break;
        
        case STATE_PAIRING:
         //
        break;
        
        case STATE_PAIRED_SUCCESS:
          
          //Blink
          leds_management_blink_pairing_success();
        
          //Stop timeout for pairing
          timers_management_control_pairing_stop();
        
          //Stop timer connection control
          connection_control_management_timer_stop();
        
          //Update number of pairings
          connection_control_management_update_number_paired_times();
          
        
          sleep = false;
        break;
        
        case STATE_PAIRING_TO_ON:
           
            
            //New advertising
            bluetooth_management_advertising_on();
            encryption_control_management_create_new_dynamic_pass(false);
            if( timers_management_get_pairing_timer_status())
            {
                //Leds indication
                leds_management_timeout_pairing();
            }
            //Stop pairing timer, in case is on
            timers_management_control_pairing_stop();
        
            sleep = false;
        
        break;
                         
        case STATE_BLE_MASTER_DISCONNECTED:
            device_disconnected();
            sleep =  false;
        break;
        
        case   STATE_BLE_PERIPHERAL_DISCONNECTED:
          bluetooth_management_stop_connection();
          sleep = false;
        break;
        
        case   STATE_BLE_WAIT_DISCONNECTION:
          //Nothing. Just wait
        break;
        
        
        case   STATE_BLE_DISCONNECTION_SUCCESS:
          device_disconnected();
          sleep =  false;
        break;
        
        case STATE_RESET_DISCONNECT:
          bluetooth_management_stop_connection();
          sleep = false;
        break;
        
        case STATE_RESET_WAIT_DISCONNECTION:
          //
        break;
        
        case STATE_RESET:
           NVIC_SystemReset();
        break;
        
        case   STATE_SLEEP_PREPARE:
          #ifdef TEST_NO_SLEEP
            
          #else
            gpio_go_to_deep_sleep();
          #endif
          //Program finished
        break;
        
        
    }
  
    return sleep;
}


