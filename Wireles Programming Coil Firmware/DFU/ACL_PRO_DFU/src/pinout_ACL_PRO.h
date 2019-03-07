#ifndef PINOUT_ACL_PRO_H__
#define PINOUT_ACL_PRO_H__

#include "nrf_saadc.h"
/**************************************************
* GPIO
**************************************************/
//#define BUTTON_ACL           14
//#define LED_BLUE             17 
//#define LED_RED              18

#define BUTTON_ACL           29
#define LED_BLUE             5 
#define LED_RED              7

/**************************************************
* Charger
**************************************************/
#define CHARGER_ADC         NRF_SAADC_INPUT_AIN2   
#define CHARGER_EN          14
#define CHARGER_STAT        8
#define CHARGER_NTC         NRF_SAADC_INPUT_AIN0

#define BATT_EN             3
#define USB_DETECT          13
/**************************************************
* UART
**************************************************/
//#define PIN_ACL_UART_EN      15

//#define PIN_ACL_PRO_UART_RX   8
//#ifdef  TEST_KIT_NORDIC
//  #define PIN_ACL_PRO_UART_TX   7
//#else
//  #define PIN_ACL_PRO_UART_TX   6
//#endif
//#define PIN_ACL_PRO_UART_CTS  10
//#define PIN_ACL_PRO_UART_RTS  11

#define PIN_ACL_3V3_ADP       6
#define PIN_ACL_UART_EN       18
#define PIN_ACL_PRO_UART_RX   16
#define PIN_ACL_PRO_UART_TX   15
#define PIN_ACL_PRO_UART_CTS  20
#define PIN_ACL_PRO_UART_RTS  17

#endif  //PINOUT_ACL_PRO_H__


